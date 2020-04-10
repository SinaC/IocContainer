using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using EasyIoc.Attributes;
using EasyIoc.Configuration;
using EasyIoc.Exceptions;

namespace EasyIoc
{
    public sealed class IocContainer : IIocContainer
    {
        private const string SectionName = "application.dependency";

        private class RegisterTypeEntity
        {
            public Type ImplementationType { get; set; }
            public bool IsSingleton { get; set; }
        }

        private readonly TypeStorageCollection<RegisterTypeEntity> _registeredTypes = new TypeStorageCollection<RegisterTypeEntity>();
        private readonly TypeStorageCollection<object> _instances = new TypeStorageCollection<object>();

        private readonly TypeStorageCollection<ResolveNodeBase> _resolveTrees = new TypeStorageCollection<ResolveNodeBase>();

        private readonly object _lockObject = new object();

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
            return InternalIsRegistered<TInterface>(null);
        }

        public bool IsRegistered<TInterface>(string name)
            where TInterface : class
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return InternalIsRegistered<TInterface>(name);
        }

        public void RegisterType<TInterface, TImplementation>()
            where TInterface : class
            where TImplementation : class, TInterface
        {
            InternalRegisterType<TInterface, TImplementation>(null, false);
        }

        public void RegisterType<TInterface, TImplementation>(string name)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            InternalRegisterType<TInterface, TImplementation>(name, false);
        }

        public void RegisterInstance<TInterface>(TInterface instance)
            where TInterface : class
        {
            InternalRegisterInstance(instance, null);
        }

        public void RegisterInstance<TInterface>(TInterface instance, string instanceName)
            where TInterface : class
        {
            if (instanceName == null)
                throw new ArgumentNullException(nameof(instanceName));

            InternalRegisterInstance(instance, instanceName);
        }

        public void Unregister<TInterface>()
            where TInterface : class
        {
            InternalUnregister<TInterface>();
        }

        public void UnregisterType<TInterface>()
            where TInterface : class
        {
            InternalUnregisterType<TInterface>(null);
        }

        public void UnregisterType<TInterface>(string name)
            where TInterface : class
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            InternalUnregisterType<TInterface>(name);
        }

        public void UnregisterInstance<TInterface>()
            where TInterface : class
        {
            InternalUnregisterInstance<TInterface>(null);
        }

        public void UnregisterInstance<TInterface>(string instanceName)
            where TInterface : class
        {
            if (instanceName == null)
                throw new ArgumentNullException(nameof(instanceName));

            InternalUnregisterInstance<TInterface>(instanceName);
        }

        public TInterface Resolve<TInterface>()
            where TInterface : class
        {
            return InternalResolve<TInterface>(null);
        }

        public TInterface Resolve<TInterface>(string name)
            where TInterface : class
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return InternalResolve<TInterface>(name);
        }

        public bool TryResolve<TInterface>(out TInterface instance)
            where TInterface : class
        {
            return InternalTryResolve(null, out instance);
        }

        public bool TryResolve<TInterface>(string name, out TInterface instance)
            where TInterface : class
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return InternalTryResolve(name, out instance);
        }

        public void Reset()
        {
            lock (_lockObject)
            {
                _registeredTypes.UnsafeClear();
                _instances.UnsafeClear();
                _resolveTrees.UnsafeClear();
            }
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
                            Debug.Print("Registering type {0}", definedType.AsType().FullName);
                            InternalRegisterType(attribute.InterfaceType, definedType.AsType(), attribute.Name, attribute.IsSingleton);
                        }
                    }
                }
            }
        }

        private bool InternalIsRegistered<TInterface>(string name)
            where TInterface : class
        {
            Type interfaceType = typeof(TInterface);

            bool found;
            lock (_lockObject)
            {
                found = _registeredTypes.UnsafeContainsKey(interfaceType, name) || _instances.UnsafeContainsKey(interfaceType, name);
            }

            return found;
        }

        private void InternalRegisterType<TInterface, TImplementation>(string name, bool isSingleton)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            Type interfaceType = typeof(TInterface);
            Type implementationType = typeof(TImplementation);

            InternalRegisterType(interfaceType, implementationType, name, isSingleton);
        }

        private void InternalRegisterType(Type interfaceType, Type implementationType, string name, bool isSingleton)
        {
            if (!interfaceType.IsInterface)
                throw new RegisterTypeException($"Cannot RegisterType: {interfaceType.FullName} is not an interface");

            if (implementationType.IsInterface || implementationType.IsAbstract)
                throw new RegisterTypeException($"Cannot RegisterType: {implementationType.FullName} cannot be an interface nor an abstract class");

            if (!interfaceType.IsAssignableFrom(implementationType))
                throw new RegisterTypeException($"Cannot RegisterType: {interfaceType.FullName} is not assignable from {implementationType.FullName}");

            lock (_lockObject)
            {
                if (_registeredTypes.UnsafeContainsKey(interfaceType, name))
                {
                    var exceptionMsg = name == null
                        ? $"Cannot RegisterType: An implementation has already been registered for interface {interfaceType.FullName}"
                        : $"Cannot RegisterType: An implementation has already been registered with that name for interface {interfaceType.FullName}";
                    throw new RegisterTypeException(exceptionMsg);
                }

                RegisterTypeEntity registerTypeEntity = new RegisterTypeEntity
                {
                    ImplementationType = implementationType,
                    IsSingleton = isSingleton
                };

                _registeredTypes.UnsafeAdd(interfaceType, name, registerTypeEntity);
                // ResolveTree will be built on first call to Resolve
            }
        }

        private void InternalUnregister<TInterface>()
            where TInterface : class
        {
            Type interfaceType = typeof(TInterface);

            lock (_lockObject)
            {
                // remove anonymous and named
                _registeredTypes.UnsafeRemove(interfaceType);
                _instances.UnsafeRemove(interfaceType);
                _resolveTrees.UnsafeRemove(interfaceType); // Force ResolveTree rebuild
            }
        }

        private void InternalRegisterInstance<TInterface>(TInterface instance, string instanceName)
            where TInterface : class
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            Type interfaceType = typeof(TInterface);

            if (!interfaceType.IsInterface)
                throw new RegisterInstanceException("Cannot RegisterInstance: Only an interface can be registered");

            lock (_lockObject)
            {
                if (_instances.UnsafeContainsKey(interfaceType, instanceName))
                    throw new RegisterInstanceException($"Cannot RegisterInstance: An instance has already been registered for interface {interfaceType.FullName}");
                _instances.UnsafeAdd(interfaceType, instanceName, instance);
                // ResolveTree will be built on first call to Resolve
            }
        }

        private void InternalUnregisterType<TInterface>(string name)
            where TInterface : class
        {
            Type interfaceType = typeof(TInterface);
            lock (_lockObject)
            {
                // remove names only
                _registeredTypes.UnsafeRemove(interfaceType, name);
                _instances.UnsafeRemove(interfaceType, name); // in case registered type was flagged as singleton
                _resolveTrees.UnsafeRemove(interfaceType, name); // Force ResolveTree rebuild
            }
        }

        private void InternalUnregisterInstance<TInterface>(string instanceName)
            where TInterface : class
        {
            Type interfaceType = typeof(TInterface);
            lock (_lockObject)
            {
                // remove names only
                _instances.UnsafeRemove(interfaceType, instanceName);
                _resolveTrees.UnsafeRemove(interfaceType, instanceName); // Force ResolveTree rebuild
            }
        }

        private TInterface InternalResolve<TInterface>(string name)
            where TInterface : class
        {
            Type interfaceType = typeof(TInterface);

            if (!interfaceType.IsInterface)
                throw new ResolveException("Cannot Resolve: Only an interface can be resolved");

            // Get resolve tree
            ResolveNodeBase resolveTree;
            lock (_lockObject)
            {
                // Search in resolve tree cache
                if (!_resolveTrees.UnsafeTryGet(interfaceType, name, out resolveTree))
                {
                    // Create resolve tree if not found
                    Debug.Print("Building ResolveTree for type {0}", interfaceType.FullName);
                    resolveTree = BuildResolveTree(interfaceType, name);
                    if (!(resolveTree is ErrorNode))
                        _resolveTrees.UnsafeAdd(interfaceType, name, resolveTree); // save resolve tree only if not in error
                }
            }

            // Check errors
            if (resolveTree is ErrorNode)
            {
                if (resolveTree == ErrorNode.TypeNotRegistered)
                    throw new ResolveException($"Cannot Resolve: No registration found for type {interfaceType.FullName}");
                if (resolveTree == ErrorNode.NoPublicConstructorOrNoConstructor)
                    throw new ResolveException($"Cannot Resolve: No constructor or not public constructor for type {interfaceType.FullName}");
                if (resolveTree == ErrorNode.NoResolvableConstructor)
                    throw new ResolveException($"Cannot Resolve: No resolvable constructor for type {interfaceType.FullName}");
                if (resolveTree == ErrorNode.CyclicDependencyConstructor)
                    throw new ResolveException($"Cannot Resolve: Cyclic dependency detected for type {interfaceType.FullName}");
            }

            // Create instance
            object resolved;
            try
            {
                // TODO: once every BuildableSingletonNode has been resolved, this lock could removed
                lock (_lockObject) // this is needed because BuildableSingletonNode can modify _instances
                {
                    Debug.Print("Resolving ResolveTree for type {0}", interfaceType.FullName);
                    resolved = resolveTree.Resolve(_instances);
                }
            }
            catch (ErrorNodeResolveException ex)
            {
                throw new ResolveException("Cannot Resolve: See innerException", ex);
            }
            catch (NoConstructorResolveException ex)
            {
                throw new ResolveException("Cannot Resolve: no valid constructor found", ex);
            }

            return (TInterface) resolved;
        }

        private bool InternalTryResolve<TInterface>(string name, out TInterface instance)
            where TInterface : class
        {
            Type interfaceType = typeof(TInterface);

            if (!interfaceType.IsInterface)
            {
                instance = null;
                return false;
            }

            // Get resolve tree
            ResolveNodeBase resolveTree;
            lock (_lockObject)
            {
                // Search in resolve tree cache
                if (!_resolveTrees.UnsafeTryGet(interfaceType, name, out resolveTree))
                {
                    // Create resolve tree if not found
                    resolveTree = BuildResolveTree(interfaceType, name);
                    if (!(resolveTree is ErrorNode))
                        _resolveTrees.UnsafeAdd(interfaceType, name, resolveTree); // save resolve tree only if not in error
                }
            }

            // Check errors
            if (resolveTree is ErrorNode)
            {
                instance = null;
                return false;
            }

            // Create instance
            try
            {
                // TODO: once every BuildableSingletonNode has been resolved, this lock could removed
                lock (_lockObject) // this is needed because BuildableSingletonNode can modify _instances
                {
                    bool resolved = resolveTree.TryResolve(_instances, out var rawInstance);
                    instance = rawInstance as TInterface;
                    return resolved;
                }
            }
            catch (Exception)
            {
                instance = null;
                return false;
            }
        }

        // Following code is NOT responsible for locking collections

        #region Resolve Tree

        private sealed class ConstructorAndParameterAndAttribute
        {
            public ConstructorInfo ConstructorInfo { get; set; }
            public ParameterInfo[] ParameterInfos { get; set; }
            public ResolvingConstructorAttribute ResolvingConstructorAttribute { get; set; }
        }

        private abstract class ResolveNodeBase
        {
            public Type InterfaceType { protected get; set; }

            public abstract object Resolve(TypeStorageCollection<object> instances);
            public abstract bool TryResolve(TypeStorageCollection<object> instances, out object obj);
        }

        private class ErrorNode : ResolveNodeBase
        {
            public static readonly ResolveNodeBase TypeNotRegistered = new ErrorNode();
            public static readonly ResolveNodeBase NoPublicConstructorOrNoConstructor = new ErrorNode();
            public static readonly ResolveNodeBase NoResolvableConstructor = new ErrorNode();
            public static readonly ResolveNodeBase CyclicDependencyConstructor = new ErrorNode();

            public override object Resolve(TypeStorageCollection<object> instances)
            {
                throw new ErrorNodeResolveException("Cannot resolve an ErrorNode");
            }

            public override bool TryResolve(TypeStorageCollection<object> instances, out object obj)
            {
                obj = null;
                return false;
            }
        }

        private sealed class InstanceNode : ResolveNodeBase
        {
            public object Instance { private get; set; }

            public override object Resolve(TypeStorageCollection<object> instances)
            {
                return Instance;
            }

            public override bool TryResolve(TypeStorageCollection<object> instances, out object obj)
            {
                obj = Instance;
                return true;
            }
        }

        private class BuildableNode : ResolveNodeBase
        {
            public ConstructorInfo ConstructorInfo { private get; set; }
            public List<ResolveNodeBase> Parameters { private get; set; }

            public override object Resolve(TypeStorageCollection<object> instances)
            {
                if (ConstructorInfo == null)
                    throw new NoConstructorResolveException($"No constructor info for type {InterfaceType.FullName}");

                // If parameter-less, create instance
                if (Parameters == null)
                {
                    return ConstructorInfo.Invoke(null);
                }

                // If parameters, recursively create parameters instance
                object[] parameters = new object[Parameters.Count];
                for (int i = 0; i < Parameters.Count; i++)
                {
                    ResolveNodeBase unspecializedParameter = Parameters[i];

                    object parameterValue = unspecializedParameter.Resolve(instances);
                    parameters[i] = parameterValue;
                }

                // and create instance using parameters
                return ConstructorInfo.Invoke(parameters);
            }

            public override bool TryResolve(TypeStorageCollection<object> instances, out object obj)
            {
                if (ConstructorInfo == null)
                {
                    obj = null;
                    return false;
                }

                // If parameter-less, create instance
                if (Parameters == null)
                {
                    obj = ConstructorInfo.Invoke(null);
                    return true;
                }

                // If parameters, recursively create parameters instance
                object[] parameters = new object[Parameters.Count];
                for (int i = 0; i < Parameters.Count; i++)
                {
                    ResolveNodeBase unspecializedParameter = Parameters[i];

                    if (!unspecializedParameter.TryResolve(instances, out var parameterValue))
                    {
                        obj = null;
                        return false;
                    }

                    parameters[i] = parameterValue;
                }

                // and create instance using parameters
                obj = ConstructorInfo.Invoke(parameters);
                return true;
            }
        }

        private sealed class BuildableSingletonNode : BuildableNode
        {
            public string Name { private get; set; }

            public override object Resolve(TypeStorageCollection<object> instances)
            {
                // Check in instances dictionary
                if (!instances.UnsafeTryGet(InterfaceType, Name, out var obj))
                {
                    // Create instance if not yet resolved
                    obj = base.Resolve(instances);
                    // Add to instances dictionary
                    instances.UnsafeAdd(InterfaceType, Name, obj);
                }

                // Return instance
                return obj;
            }

            public override bool TryResolve(TypeStorageCollection<object> instances, out object obj)
            {
                // Check in instances dictionary
                if (!instances.UnsafeTryGet(InterfaceType, Name, out obj))
                {
                    // Create instance if not yet resolved
                    bool isResolved = base.TryResolve(instances, out obj);
                    // Add to instances dictionary
                    if (isResolved)
                        instances.UnsafeAdd(InterfaceType, Name, obj);
                    else
                        return false;
                }

                // Return instance
                return true;
            }
        }

        private ResolveNodeBase BuildResolveTree(Type interfaceType, string name)
        {
            List<Type> discoveredTypes = new List<Type>
            {
                interfaceType
            };
            return InnerBuildResolveTree(interfaceType, name, discoveredTypes);
        }

        private ResolveNodeBase InnerBuildResolveTree(Type interfaceType, string name, ICollection<Type> discoveredTypes)
        {
            // Instance? if found, return instance
            if (_instances.UnsafeTryGet(interfaceType, name, out var instance))
            {
                Debug.Print("Instance found for type {0}", interfaceType.FullName);
                return new InstanceNode
                {
                    InterfaceType = interfaceType,
                    Instance = instance
                };
            }

            // Implementation? if found, we try to find a resolvable ctor
            if (!_registeredTypes.UnsafeTryGet(interfaceType, name, out var registerTypeEntity))
            {
                Debug.Print("No registration found type {0}", interfaceType.FullName);
                return ErrorNode.TypeNotRegistered;
            }
            Debug.Print("Registered {0} (singleton: {1}) found for type {2}", registerTypeEntity.ImplementationType.FullName, registerTypeEntity.IsSingleton, interfaceType.FullName);

            // We have found an implementation type matching interface type, let the fun begin
            ConstructorInfo[] constructorInfos = registerTypeEntity.ImplementationType.GetConstructors();

            // Valid constructor?
            if (constructorInfos.Length == 0
                || (constructorInfos.Length == 1 && !constructorInfos[0].IsPublic))
            {
                Debug.Print("No public constructor found for type {0}", interfaceType.FullName);
                return ErrorNode.NoPublicConstructorOrNoConstructor;
            }

            // Get parameters and attribute if any for each constructor
            Type resolvingConstructorAttributeType = typeof(ResolvingConstructorAttribute);
            var constructorAndParameters = constructorInfos.Select(x => new ConstructorAndParameterAndAttribute
            {
                ConstructorInfo = x,
                ParameterInfos = x.GetParameters(),
                ResolvingConstructorAttribute = x.GetCustomAttribute(resolvingConstructorAttributeType) as ResolvingConstructorAttribute
            }).ToList();

            // First, try constructors with ResolvingConstructor attribute
            Debug.Print("Searching constructor with ResolveConstructor attribute for type {0}", interfaceType.FullName);
            ResolveNodeBase resolvingConstructorFound = TryToResolveConstructor(constructorAndParameters.Where(x => x.ResolvingConstructorAttribute != null), registerTypeEntity, interfaceType, name, discoveredTypes);
            if (!(resolvingConstructorFound is ErrorNode))
            {
                Debug.Print("Constructor with ResolveConstructor attribute found for type {0}", interfaceType.FullName);
                return resolvingConstructorFound;
            }

            // Then, search for a parameter-less constructor
            Debug.Print("Searching for parameter-less constructor for type {0}", interfaceType.FullName);
            var parameterless = constructorAndParameters.FirstOrDefault(x => x.ParameterInfos.Length == 0);
            if (parameterless != null)
            {
                if (registerTypeEntity.IsSingleton)
                {
                    Debug.Print("Singleton parameter-less constructor found for type {0}", interfaceType.FullName);
                    return new BuildableSingletonNode
                    {
                        InterfaceType = interfaceType,
                        ConstructorInfo = parameterless.ConstructorInfo,
                        Name = name
                    };
                }
                Debug.Print("Parameter-less constructor found for type {0}", interfaceType.FullName);
                return new BuildableNode
                {
                    InterfaceType = interfaceType,
                    ConstructorInfo = parameterless.ConstructorInfo
                };
            }
            Debug.Print("No parameter-less constructor found for type {0}", interfaceType.FullName);

            // Then check if there is a constructor without ResolvingConstructor attribute can be resolved
            ResolveNodeBase constructorFound = TryToResolveConstructor(constructorAndParameters.Where(x => x.ResolvingConstructorAttribute == null), registerTypeEntity, interfaceType, name, discoveredTypes);
            Debug.Print(constructorFound is ErrorNode
                ? "No constructor found for type {0}"
                : "Constructor found for type {0}", interfaceType.FullName);
            return constructorFound;
        }

        // Check if there is constructor with if every parameter registered in container or resolvable
        private ResolveNodeBase TryToResolveConstructor(IEnumerable<ConstructorAndParameterAndAttribute> list, RegisterTypeEntity registerTypeEntity, Type interfaceType, string name, ICollection<Type> discoveredTypes)
        {
            foreach (var constructorAndParameter in list)
            {
                Debug.Print("Trying to resolve constructor {0} for type {1}", constructorAndParameter.ConstructorInfo, interfaceType.FullName);
                List<ResolveNodeBase> parametersResolvable = new List<ResolveNodeBase>(constructorAndParameter.ParameterInfos.Length);

                // Try to resolved every parameters
                bool ok = true;
                foreach (ParameterInfo parameterInfo in constructorAndParameter.ParameterInfos)
                {
                    Debug.Print("Trying to resolve parameter {0} for constructor {1} for type {2}", parameterInfo, constructorAndParameter.ConstructorInfo, interfaceType.FullName);
                    if (discoveredTypes.Any(x => x == parameterInfo.ParameterType)) // check cyclic dependency
                    {
                        Debug.Print("Cyclic depending found while resolving parameter {0} for constructor {1} for type {2}", parameterInfo, constructorAndParameter.ConstructorInfo, interfaceType.FullName);
                        ok = false;
                        break;
                    }

                    discoveredTypes.Add(parameterInfo.ParameterType); // add parameter type to discovered type
                    ResolveNodeBase parameter = InnerBuildResolveTree(parameterInfo.ParameterType, name, discoveredTypes); // try to resolve constructor parameter
                    discoveredTypes.Remove(parameterInfo.ParameterType); // remove parameter type from discovered type

                    if (parameter is ErrorNode) // once an invalid ctor parameter has been found, try next ctor
                    {
                        Debug.Print("Error found while resolving parameter {0} for constructor {1} for type {2}", parameterInfo, constructorAndParameter.ConstructorInfo, interfaceType.FullName);
                        ok = false;
                        break;
                    }

                    Debug.Print("Parameter {0} resolved for constructor {1} for type {2}", parameterInfo, constructorAndParameter.ConstructorInfo, interfaceType.FullName);
                    parametersResolvable.Add(parameter);
                }

                if (ok)
                {
                    if (registerTypeEntity.IsSingleton)
                    {
                        Debug.Print("Singleton constructor {0} found for type {1}", constructorAndParameter.ConstructorInfo, interfaceType.FullName);
                        return new BuildableSingletonNode
                        {
                            InterfaceType = interfaceType,
                            ConstructorInfo = constructorAndParameter.ConstructorInfo,
                            Parameters = parametersResolvable,
                            Name = name
                        };
                    }

                    Debug.Print("Constructor {0} found for type {1}", constructorAndParameter.ConstructorInfo, interfaceType.FullName);
                    return new BuildableNode
                    {
                        InterfaceType = interfaceType,
                        ConstructorInfo = constructorAndParameter.ConstructorInfo,
                        Parameters = parametersResolvable
                    };
                }
            }

            Debug.Print("No resolvable constructor found for type {0}", interfaceType.FullName);
            return ErrorNode.NoResolvableConstructor;
        }

        #endregion
    }
}
