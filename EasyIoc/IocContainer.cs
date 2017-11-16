using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EasyIoc
{
    public sealed class IocContainer : IIocContainer
    {
        private readonly Dictionary<Type, AnonymousNamedEntity<Type>> _implementations = new Dictionary<Type, AnonymousNamedEntity<Type>>();
        private readonly Dictionary<Type, AnonymousNamedEntity<object>> _instances = new Dictionary<Type, AnonymousNamedEntity<object>>();

        private readonly Dictionary<Type, AnonymousNamedEntity<ResolveNodeBase>> _resolveTrees = new Dictionary<Type, AnonymousNamedEntity<ResolveNodeBase>>();

        private readonly object _lockObject = new object();

        private static readonly Lazy<IocContainer> LazyDefault = new Lazy<IocContainer>(() => new IocContainer());

        public static IIocContainer Default => LazyDefault.Value;

        #region IIocContainer

        public bool IsRegistered<TInterface>()
            where TInterface : class
        {
            Type interfaceType = typeof (TInterface);
            bool found;
            lock (_lockObject)
            {
                found = ContainsKey(_implementations, interfaceType) || ContainsKey(_instances, interfaceType);
            }
            return found;
        }

        public void RegisterType<TInterface, TImplementation>()
            where TInterface : class
            where TImplementation : class, TInterface
        {
            Type interfaceType = typeof (TInterface);

            if (!interfaceType.IsInterface)
                throw new ArgumentException("Cannot RegisterType: Only an interface can be registered");

            Type implementationType = typeof (TImplementation);

            if (implementationType.IsInterface || implementationType.IsAbstract)
                throw new ArgumentException("Cannot RegisterType: No interface or abstract class is valid as implementation");

            if (!interfaceType.IsAssignableFrom(implementationType))
                throw new ArgumentException($"Cannot RegisterType: {interfaceType.FullName} is not assignable from {implementationType.FullName}");

            lock (_lockObject)
            {
                if (ContainsKey(_implementations, interfaceType))
                    throw new ArgumentException($"Cannot RegisterType: An implementation has already been registered for interface {interfaceType.FullName}");

                UnsafeAdd(_implementations, interfaceType, implementationType);
                // ResolveTree will be built on first call to Resolve
            }
        }

        public void RegisterType<TInterface, TImplementation>(string name)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            Type interfaceType = typeof(TInterface);

            if (!interfaceType.IsInterface)
                throw new ArgumentException("Cannot RegisterType: Only an interface can be registered");

            Type implementationType = typeof(TImplementation);

            if (implementationType.IsInterface || implementationType.IsAbstract)
                throw new ArgumentException("Cannot RegisterType: No interface or abstract class is valid as implementation");

            if (!interfaceType.IsAssignableFrom(implementationType))
                throw new ArgumentException($"Cannot RegisterType: {interfaceType.FullName} is not assignable from {implementationType.FullName}");

            lock (_lockObject)
            {
                if (ContainsKey(_implementations, interfaceType, name))
                    throw new ArgumentException($"Cannot RegisterType: An implementation has already been registered with that name for interface {interfaceType.FullName}");

                UnsafeAdd(_implementations, interfaceType, name, implementationType);
                // ResolveTree will be built on first call to Resolve
            }
        }

        public void RegisterInstance<TInterface>(TInterface instance)
            where TInterface : class
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            Type interfaceType = typeof (TInterface);

            if (!interfaceType.IsInterface)
                throw new ArgumentException("Cannot RegisterInstance: Only an interface can be registered");

            lock (_lockObject)
            {
                if (ContainsKey(_instances, interfaceType))
                    throw new ArgumentException($"Cannot RegisterInstance: An instance has already been registered for interface {interfaceType.FullName}");
                UnsafeAdd(_instances, interfaceType, instance);
                // ResolveTree will be built on first call to Resolve
            }
        }

        public void RegisterInstance<TInterface>(TInterface instance, string instanceName)
            where TInterface : class
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            Type interfaceType = typeof(TInterface);

            if (!interfaceType.IsInterface)
                throw new ArgumentException("Cannot RegisterInstance: Only an interface can be registered");

            lock (_lockObject)
            {
                if (ContainsKey(_instances, interfaceType, instanceName))
                    throw new ArgumentException($"Cannot RegisterInstance: An instance has already been registered for interface {interfaceType.FullName}");
                UnsafeAdd(_instances, interfaceType, instanceName, instance);
                // ResolveTree will be built on first call to Resolve
            }
        }

        public void Unregister<TInterface>()
            where TInterface : class
        {
            Type interfaceType = typeof (TInterface);

            lock (_lockObject)
            {
                // remove anonymous and named
                _implementations.Remove(interfaceType);
                _instances.Remove(interfaceType);
                _resolveTrees.Remove(interfaceType); // Force ResolveTree rebuild
            }
        }

        public void UnregisterType<TInterface>()
            where TInterface : class
        {
            Type interfaceType = typeof (TInterface);
            lock (_lockObject)
            {
                // remove anonymous only
                Remove(_implementations, interfaceType);
                Remove(_resolveTrees, interfaceType); // Force ResolveTree rebuild
            }
        }

        public void UnregisterType<TInterface>(string name)
            where TInterface : class
        {
            Type interfaceType = typeof(TInterface);
            lock (_lockObject)
            {
                // remove names only
                Remove(_implementations, interfaceType, name);
                Remove(_resolveTrees, interfaceType, name); // Force ResolveTree rebuild
            }
        }

        public void UnregisterInstance<TInterface>()
            where TInterface : class
        {
            Type interfaceType = typeof (TInterface);
            lock (_lockObject)
            {
                // remove anonymous only
                Remove(_instances, interfaceType);
                Remove(_resolveTrees, interfaceType); // Force ResolveTree rebuild
            }
        }

        public void UnregisterInstance<TInterface>(string instanceName)
            where TInterface : class
        {
            Type interfaceType = typeof(TInterface);
            lock (_lockObject)
            {
                // remove names only
                Remove(_instances, interfaceType, instanceName);
                Remove(_resolveTrees, interfaceType, instanceName); // Force ResolveTree rebuild
            }
        }

        public TInterface Resolve<TInterface>() where TInterface : class
        {
            Type interfaceType = typeof(TInterface);

            if (!interfaceType.IsInterface)
                throw new ArgumentException("Cannot Resolve: Only an interface can be resolved");

            object resolved;
            ResolveNodeBase resolveTree;
            lock (_lockObject)
            {
                // Search in resolve tree cache
                if (!TryGet(_resolveTrees, interfaceType, out resolveTree))
                {
                    // Create resolve tree if not found
                    resolveTree = BuildResolveTree(interfaceType, null);
                    if (!(resolveTree is ErrorNode))
                        UnsafeAdd(_resolveTrees, interfaceType, resolveTree); // save resolve tree only if not in error
                }
            }
            // Check errors
            if (resolveTree is ErrorNode)
            {
                if (resolveTree == ErrorNode.TypeNotRegistered)
                    throw new ArgumentException($"Cannot Resolve: No registration found for type {interfaceType.FullName}");
                if (resolveTree == ErrorNode.NoPublicConstructorOrNoConstructor)
                    throw new ArgumentException($"Cannot Resolve: No constructor or not public constructor for type {interfaceType.FullName}");
                if (resolveTree == ErrorNode.NoResolvableConstructor)
                    throw new ArgumentException($"Cannot Resolve: No resolvable constructor for type {interfaceType.FullName}");
                if (resolveTree == ErrorNode.CyclicDependencyConstructor)
                    throw new ArgumentException($"Cannot Resolve: Cyclic dependency detected for type {interfaceType.FullName}");
            }
            // Create instance
            try
            {
                resolved = resolveTree.Resolve();
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Cannot Resolve: See innerException", ex);
            }
            return (TInterface)resolved;
        }

        public TInterface Resolve<TInterface>(string name) where TInterface : class
        {
            Type interfaceType = typeof(TInterface);

            if (!interfaceType.IsInterface)
                throw new ArgumentException("Cannot Resolve: Only an interface can be resolved");

            object resolved;
            ResolveNodeBase resolveTree;
            lock (_lockObject)
            {
                // Search in resolve tree cache
                if (!TryGet(_resolveTrees, interfaceType, out resolveTree))
                {
                    // Create resolve tree if not found
                    resolveTree = BuildResolveTree(interfaceType, name);
                    if (!(resolveTree is ErrorNode))
                        UnsafeAdd(_resolveTrees, interfaceType, resolveTree); // save resolve tree only if not in error
                }
            }
            // Check errors
            if (resolveTree is ErrorNode)
            {
                if (resolveTree == ErrorNode.TypeNotRegistered)
                    throw new ArgumentException($"Cannot Resolve: No registration found for type {interfaceType.FullName}");
                if (resolveTree == ErrorNode.NoPublicConstructorOrNoConstructor)
                    throw new ArgumentException($"Cannot Resolve: No constructor or not public constructor for type {interfaceType.FullName}");
                if (resolveTree == ErrorNode.NoResolvableConstructor)
                    throw new ArgumentException($"Cannot Resolve: No resolvable constructor for type {interfaceType.FullName}");
                if (resolveTree == ErrorNode.CyclicDependencyConstructor)
                    throw new ArgumentException($"Cannot Resolve: Cyclic dependency detected for type {interfaceType.FullName}");
            }
            // Create instance
            try
            {
                resolved = resolveTree.Resolve();
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Cannot Resolve: See innerException", ex);
            }
            return (TInterface)resolved;
        }

        public void Reset()
        {
            lock (_lockObject)
            {
                _implementations.Clear();
                _instances.Clear();
                _resolveTrees.Clear();
            }
        }

        #endregion

        private class AnonymousNamedEntity<T>
            where T:class
        {
            public T Anonymous { get; set; }
            public Dictionary<string, T> Named { get; set; }

            public AnonymousNamedEntity()
            {
                Named = new Dictionary<string, T>();
            }
        }

        // Following code is NOT responsible for locking collections

        #region Resolve Tree

        private abstract class ResolveNodeBase
        {
            public Type InterfaceType { protected get; set; }

            public abstract object Resolve();
        }

        private sealed class ErrorNode : ResolveNodeBase
        {
            public static readonly ResolveNodeBase TypeNotRegistered = new ErrorNode();
            public static readonly ResolveNodeBase NoPublicConstructorOrNoConstructor = new ErrorNode();
            public static readonly ResolveNodeBase NoResolvableConstructor = new ErrorNode();
            public static readonly ResolveNodeBase CyclicDependencyConstructor = new ErrorNode();

            public override object Resolve()
            {
                throw new InvalidOperationException("Cannot resolve an ErrorNode");
            }
        }

        private sealed class InstanceNode : ResolveNodeBase
        {
            public object Instance { private get; set; }

            public override object Resolve()
            {
                return Instance;
            }
        }

        private sealed class BuildableNode : ResolveNodeBase
        {
            public ConstructorInfo ConstructorInfo { private get; set; }
            public List<ResolveNodeBase> Parameters { private get; set; }

            public override object Resolve()
            {
                if (ConstructorInfo == null)
                    throw new InvalidOperationException($"No constructor info for type {InterfaceType.FullName}");

                // If parameterless, create instance
                if (Parameters == null)
                    return ConstructorInfo.Invoke(null);

                // If parameters, recursively create parameters instance
                object[] parameters = new object[Parameters.Count];
                for (int i = 0; i < Parameters.Count; i++)
                {
                    ResolveNodeBase unspecializedParameter = Parameters[i];

                    object parameterValue = unspecializedParameter.Resolve();
                    parameters[i] = parameterValue;
                }
                // and create instance using parameters
                return ConstructorInfo.Invoke(parameters);
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
            // Instance ? if found, return instance
            object instance;
            if (TryGet(_instances, interfaceType, name, out instance))
                return new InstanceNode
                {
                    InterfaceType = interfaceType,
                    Instance = instance
                };

            // Implementation ? if found, we try to find a resolvable ctor
            Type implementationType;
            if (!TryGet(_implementations, interfaceType, name, out implementationType))
                return ErrorNode.TypeNotRegistered;

            // We have found an implementation type matching interface type, let's the fun begin
            ConstructorInfo[] constructorInfos = implementationType.GetConstructors();

            // Valid constructor ?
            if (constructorInfos.Length == 0
                || (constructorInfos.Length == 1 && !constructorInfos[0].IsPublic))
                return ErrorNode.NoPublicConstructorOrNoConstructor;

            // Get parameters for each ctor
            var constructorAndParameters = constructorInfos.Select(x => new
            {
                Constructor = x,
                Parameters = x.GetParameters()
            }).ToList();

            // Get first parameterless if any // TODO: we should get the first ctor matching user defined parameters
            var parameterless = constructorAndParameters.FirstOrDefault(x => x.Parameters.Length == 0);
            if (parameterless != null)
                return new BuildableNode
                {
                    InterfaceType = interfaceType,
                    ConstructorInfo = parameterless.Constructor
                };

            // Check if every ctor's parameter is registered in container or resolvable, returns first resolvable
            foreach (var c in constructorAndParameters)
            {
                List<ResolveNodeBase> parametersResolvable = new List<ResolveNodeBase>(c.Parameters.Length);

                // Try to resolved every parameters
                bool ok = true;
                foreach (ParameterInfo parameterInfo in c.Parameters)
                {
                    if (discoveredTypes.Any(x => x == parameterInfo.ParameterType)) // check cyclic dependency
                    {
                        ok = false;
                        break;
                    }

                    discoveredTypes.Add(parameterInfo.ParameterType); // add parameter type to discovered type
                    ResolveNodeBase parameter = InnerBuildResolveTree(parameterInfo.ParameterType, name, discoveredTypes);
                    discoveredTypes.Remove(parameterInfo.ParameterType); // remove parameter type from discovered type

                    if (parameter is ErrorNode) // once an invalid ctor parameter has been found, try next ctor
                    {
                        ok = false;
                        break;
                    }
                    parametersResolvable.Add(parameter);
                }

                if (ok)
                    return new BuildableNode
                    {
                        InterfaceType = interfaceType,
                        ConstructorInfo = c.Constructor,
                        Parameters = parametersResolvable
                    };
            }
            return ErrorNode.NoResolvableConstructor;
        }

        private bool ContainsKey<T>(IReadOnlyDictionary<Type, AnonymousNamedEntity<T>> dictionary, Type interfaceType, string name = null)
            where T : class
        {
            AnonymousNamedEntity<T> entity;
            if (!dictionary.TryGetValue(interfaceType, out entity))
                return false;
            if (name == null)
                return entity.Anonymous != default(T);
            return entity.Named.ContainsKey(name);
        }

        private bool TryGet<T>(IReadOnlyDictionary<Type, AnonymousNamedEntity<T>> dictionary, Type interfaceType, out T value)
            where T : class
        {
            return TryGet(dictionary, interfaceType, null, out value);
        }

        private bool TryGet<T>(IReadOnlyDictionary<Type, AnonymousNamedEntity<T>> dictionary, Type interfaceType, string name, out T value)
            where T:class
        {
            AnonymousNamedEntity<T> entity;
            if (!dictionary.TryGetValue(interfaceType, out entity))
            {
                value = default(T);
                return false;
            }
            if (name == null)
            {
                value = entity.Anonymous;
                return value != default(T);
            }
            if (entity.Named.TryGetValue(name, out value))
                return true;
            return false;
        }

        private void UnsafeAdd<T>(Dictionary<Type, AnonymousNamedEntity<T>> dictionary, Type interfaceType, T value)
            where T : class
        {
            UnsafeAdd(dictionary, interfaceType, null, value);
        }

        private void UnsafeAdd<T>(Dictionary<Type, AnonymousNamedEntity<T>> dictionary, Type interfaceType, string name, T value)
            where T : class
        {
            AnonymousNamedEntity<T> entity;
            if (!dictionary.TryGetValue(interfaceType, out entity))
            {
                entity = new AnonymousNamedEntity<T>();
                dictionary.Add(interfaceType, entity);
            }
            if (name == null)
                entity.Anonymous = value;
            else
                entity.Named[name] = value;
        }

        private bool Remove<T>(Dictionary<Type, AnonymousNamedEntity<T>> dictionary, Type interfaceType, string name = null)
            where T : class
        {
            AnonymousNamedEntity<T> entity;
            if (!dictionary.TryGetValue(interfaceType, out entity))
                return false;
            if (name == null)
            {
                entity.Anonymous = default(T);
                return true;
            }
            return entity.Named.Remove(name);
        }

        #endregion
    }
}
