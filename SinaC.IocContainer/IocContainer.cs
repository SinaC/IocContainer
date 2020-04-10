using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using SinaC.IocContainer.Attributes;
using SinaC.IocContainer.Configuration;
using SinaC.IocContainer.Exceptions;

namespace SinaC.IocContainer
{
    public class IocContainer : IIocContainer
    {
        private const string SectionName = "application.dependency";

        private CatalogContainer Catalog { get; } = new CatalogContainer();

        // Singleton-lazy pattern
        private static readonly Lazy<IocContainer> LazyDefault = new Lazy<IocContainer>(() => new IocContainer());
        public static IIocContainer Default => LazyDefault.Value;

        #region IIocContainer

        public void InitializeFromConfig()
        {
            // Search config section
            if (ConfigurationManager.GetSection(SectionName) is AssemblyConfigSection assemblyConfigSection)
            {
                InternalInitializeFromConfig(assemblyConfigSection);
            }
        }

        public void InitializeFromConfig(string filename)
        {
            ExeConfigurationFileMap configMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = filename
            };
            System.Configuration.Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);

            if (config.GetSection(SectionName) is AssemblyConfigSection assemblyConfigSection)
            {
                InternalInitializeFromConfig(assemblyConfigSection);
            }
        }

        public bool IsRegistered<TInterface>()
            where TInterface : class
        {
            return InternalIsRegistered(typeof(TInterface), null);
        }

        public bool IsRegistered<TInterface>(string name)
            where TInterface : class
        {
            return InternalIsRegistered(typeof(TInterface), name);
        }

        public void RegisterType<TInterface, TImplementation>()
            where TInterface : class
            where TImplementation : class, TInterface
        {
            throw new NotImplementedException();
        }

        public void RegisterType<TInterface, TImplementation>(string name)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            throw new NotImplementedException();
        }

        public void RegisterInstance<TInterface, TImplementation>(TImplementation instance)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            throw new NotImplementedException();
        }

        public void RegisterInstance<TInterface, TImplementation>(TImplementation instance, string instanceName)
            where TInterface : class
            where TImplementation : class, TInterface
        {
        }

        public TInterface Resolve<TInterface>()
            where TInterface : class
        {
            throw new NotImplementedException();
        }

        public TInterface Resolve<TInterface>(string name)
            where TInterface : class
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TInterface> ResolveAll<TInterface>()
            where TInterface : class
        {
            throw new NotImplementedException();
        }

        public bool TryResolve<TInterface>(out TInterface instance)
            where TInterface : class
        {
            throw new NotImplementedException();
        }

        public bool TryResolve<TInterface>(string name, out TInterface instance)
            where TInterface : class
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        #endregion

        private void InternalInitializeFromConfig(AssemblyConfigSection assemblyConfigSection)
        {
            Type registerTypeAttributeType = typeof(RegisterTypeAttribute);
            // Get domain assemblies
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            // Loop assembly found in config section and search a match in domain assemblies
            foreach (var element in assemblyConfigSection.Assemblies.OfType<AssemblyConfigCollectionElement>())
            {
                // Search a matching assembly
                Debug.Print("Assembly qualifiedName {0} found", element.QualifiedName);
                Assembly matchingAssembly = assemblies.FirstOrDefault(x => x.GetName().Name == element.QualifiedName);
                // Found a matching assembly, check defined type with RegisterType attribute
                if (matchingAssembly != null)
                {
                    // For each type in matching assembly, check if RegisterType attribute is declared
                    foreach (var definedType in matchingAssembly.DefinedTypes)
                    {
                        if (definedType.GetCustomAttribute(registerTypeAttributeType) is RegisterTypeAttribute attribute)
                        {
                            // Register type if attribute is found
                            Type interfaceType = attribute.InterfaceType;
                            Type implementationType = definedType.AsType();
                            string name = attribute.Name;
                            bool isSingleton = attribute.IsSingleton;
                            Debug.Print("Registering type {0} with implementation {1}, name {2}, isSingleton {3}", interfaceType.FullName, implementationType.FullName, name, isSingleton);
                            InternalRegisterType(interfaceType, implementationType, name, isSingleton);
                        }
                    }
                }
            }
        }

        private bool InternalIsRegistered(Type interfaceType, string name)
        {
            lock (Catalog)
                return Catalog.IsTypeRegistered(interfaceType, name);
        }

        private void InternalRegisterType(Type interfaceType, Type implementationType, string name, bool isSingleton)
        {
            Debug.Print("Registering type {0} with implementation {1}, name {2}, isSingleton {3}", interfaceType.FullName, implementationType.FullName, name, isSingleton);
            if (!interfaceType.IsInterface)
                throw new RegisterTypeException($"{interfaceType.FullName} is not an interface");

            if (implementationType.IsInterface || implementationType.IsAbstract)
                throw new RegisterTypeException($"{implementationType.FullName} cannot be an interface nor an abstract class");

            if (!interfaceType.IsAssignableFrom(implementationType))
                throw new RegisterTypeException($"{interfaceType.FullName} is not assignable from {implementationType.FullName}");

            lock (Catalog)
            {
                if (Catalog.IsTypeUniquelyRegistered(interfaceType, implementationType, name))
                {
                    Debug.Print("Type {0} with implementation {1} and name {2} already registered", interfaceType.FullName, implementationType.FullName, name);

                    if (string.IsNullOrWhiteSpace(name))
                        throw new RegisterTypeException($"An implementation has already been registered for interface {interfaceType.FullName}");
                    else
                        throw new RegisterTypeException($"An implementation has already been registered with name {name} for interface {interfaceType.FullName}");
                }

                RegisterTypeEntry registerTypeEntry = new RegisterTypeEntry
                {
                    InterfaceType = interfaceType,
                    ImplementationType = implementationType,
                    Name = name,
                    IsSingleton = isSingleton
                };

                Catalog.RegisterType(registerTypeEntry);
                // ResolveTree will be built on first call to Resolve
            }
        }
    }
}
