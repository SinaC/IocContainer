using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

// TODO: 
//  detect double-recursivity on ctor
//  group multiple dictionary into one dictionary

// Unless a specific instance has been registered, every Resolve will create a new instance
namespace EasyIoc
{
    public class IocContainer : IIocContainer
    {
        private readonly Dictionary<Type, Type> _interfaceToImplementationMap = new Dictionary<Type, Type>();
        private readonly Dictionary<Type, Func<object>> _factories = new Dictionary<Type, Func<object>>();
        private readonly Dictionary<Type, object> _instances = new Dictionary<Type, object>();
        private readonly Dictionary<Type, ResolvableConstructor> _resolvableConstructors = new Dictionary<Type,ResolvableConstructor>();

        private readonly object _lockObject = new object();

        private static readonly Lazy<IocContainer> LazyDefault = new Lazy<IocContainer>(() => new IocContainer());
        public static IocContainer Default
        {
            get
            {
                return LazyDefault.Value;
            }
        }

        #region IIocContainer

        public bool IsRegistered<TInterface>()
            where TInterface : class
        {
            Type interfaceType = typeof (TInterface);
            bool found;
            lock (_lockObject)
            {
                found = _interfaceToImplementationMap.ContainsKey(interfaceType);
            }
            return found;
        }

        public void RegisterType<TInterface, TImplementation>() 
            where TInterface : class 
            where TImplementation : class
        {
            Type interfaceType = typeof(TInterface);

            if (!interfaceType.IsInterface)
                throw new ArgumentException("Cannot Register: Only an interface can be registered");

            Type implementationType = typeof(TImplementation);

            if (implementationType.IsInterface || implementationType.IsAbstract)
                throw new ArgumentException("Cannot Register: No interface or abstract class is  valid as implementation");

            if (!interfaceType.IsAssignableFrom(implementationType))
                throw new ArgumentException(String.Format("{0} is not assignable from {1}", interfaceType.FullName, implementationType.FullName));

            lock (_lockObject)
            {
                if (_interfaceToImplementationMap.ContainsKey(interfaceType))
                {
                    if (_interfaceToImplementationMap[interfaceType] != implementationType)
                        throw new InvalidOperationException(String.Format("There is already an implementation registered for interface {0}", interfaceType.FullName));
                }
                else
                    _interfaceToImplementationMap.Add(interfaceType, implementationType);

                if (!_resolvableConstructors.ContainsKey(interfaceType))
                {
                    ResolvableConstructor resolvableConstructor = BuildCreatableConstructor(interfaceType);
                    _resolvableConstructors.Add(interfaceType, resolvableConstructor);
                }
            }
        }

        public void RegisterFactory<TInterface>(Func<TInterface> createFunc) 
            where TInterface : class
        {
            if (createFunc == null)
                throw new ArgumentNullException("createFunc");

            Type interfaceType = typeof(TInterface);

            if (!interfaceType.IsInterface)
                throw new ArgumentException("Cannot RegisterFactory: Only an interface can be registered");

            lock (_lockObject)
            {
                if (_factories.ContainsKey(interfaceType))
                {
                    if (_factories[interfaceType] != createFunc)
                        throw new InvalidOperationException(String.Format("There is already a factory registered for interface {0}", interfaceType.FullName));
                }
                else
                    _factories[interfaceType] = createFunc;
            }
        }

        public void RegisterInstance<TInterface>(TInterface instance) 
            where TInterface : class
        {
            if (instance == null)
                throw new ArgumentNullException("instance");

            Type interfaceType = typeof(TInterface);

            if (!interfaceType.IsInterface)
                throw new ArgumentException("Cannot RegisterInstance: Only an interface can be registered");

            lock (_lockObject)
            {
                if (_instances.ContainsKey(interfaceType))
                {
                    if (_instances[interfaceType] != instance)
                        throw new InvalidOperationException(String.Format("There is already an instance registered for interface {0}", interfaceType.FullName));
                }
                else
                    _instances.Add(interfaceType, instance);
            }
        }

        public void Unregister<TInterface>() 
            where TInterface : class
        {
            Type interfaceType = typeof(TInterface);

            lock (_lockObject)
            {
                if (!_interfaceToImplementationMap.ContainsKey(interfaceType))
                    throw new ArgumentException(String.Format("Cannot Unregister: No registration found for interface {0}", interfaceType.FullName));

                _interfaceToImplementationMap.Remove(interfaceType);
                _factories.Remove(interfaceType);
                _instances.Remove(interfaceType);
                _resolvableConstructors.Remove(interfaceType);
            }
        }

        //public TInterface Resolve<TInterface>() 
        //    where TInterface : class
        //{
        //    Type interfaceType = typeof(TInterface);

        //    if (!interfaceType.IsInterface)
        //        throw new ArgumentException("Cannot Resolve: Only an interface can be resolved");

        //    object resolved;
        //    lock (_lockObject)
        //    {
        //        if (!_interfaceToImplementationMap.ContainsKey(interfaceType))
        //            throw new ArgumentException(String.Format("Cannot Resolve: No registration found for interface {0}", interfaceType.FullName));

        //        if (_instances.ContainsKey(interfaceType)) // Get instance if any registered
        //            resolved = _instances[interfaceType];
        //        else if (_factories.ContainsKey(interfaceType)) // Create new instance using exising factory if any registered or created on a previous Resolve
        //        {
        //            Func<object> creator = _factories[interfaceType];

        //            resolved = creator.DynamicInvoke(null);
        //        }
        //        else // Create factory, register factory and create new instance
        //        {
        //            if (!_interfaceToImplementationMap.ContainsKey(interfaceType))
        //                throw new ArgumentException(String.Format("Cannot Resolve: No registration found for interface {0}", interfaceType.FullName));
        //            Type implementationType = _interfaceToImplementationMap[interfaceType];
                    
        //            Func<object> creator = CreateFactory(implementationType);
        //            _factories.Add(interfaceType, creator);

        //            resolved = creator.DynamicInvoke(null);
        //        }
        //    }

        //    return (TInterface)resolved;
        //}

        public TInterface Resolve<TInterface>()
            where TInterface : class
        {
            Type interfaceType = typeof(TInterface);

            if (!interfaceType.IsInterface)
                throw new ArgumentException("Cannot Resolve: Only an interface can be resolved");

            object resolved;
            lock (_lockObject)
            {
                if (!_interfaceToImplementationMap.ContainsKey(interfaceType))
                    throw new ArgumentException(String.Format("Cannot Resolve: No registration found for interface {0}", interfaceType.FullName));

                if (_instances.ContainsKey(interfaceType))
                    resolved = _instances[interfaceType];
                else if (_resolvableConstructors.ContainsKey(interfaceType))
                {
                    ResolvableConstructor resolvableConstructor = _resolvableConstructors[interfaceType];
                    if (resolvableConstructor == ResolvableConstructor.TypeNotRegistered)
                        throw new ArgumentException(String.Format("Cannot Resolve: No registration found for type {0}", interfaceType.FullName));
                    if (resolvableConstructor == ResolvableConstructor.NoPublicConstructorOrNoConstructor)
                        throw new ArgumentException(String.Format("Cannot Resolve: No constructor or not public constructor for type {0}", interfaceType.FullName));
                    if (resolvableConstructor == ResolvableConstructor.NoResolvableConstructor)
                        throw new ArgumentException(String.Format("Cannot Resolve: No resolvable constructor for type {0}", interfaceType.FullName));
                    resolved = CreateInstance(resolvableConstructor);
                }
                else
                    throw new ArgumentException("Cannot Resolve: no resolvable constructor for type {0}", interfaceType.FullName);
            }
            return (TInterface)resolved;
        }

        public void Reset()
        {
            lock (_lockObject)
            {
                _interfaceToImplementationMap.Clear();
                _factories.Clear();
                _instances.Clear();
                _resolvableConstructors.Clear();
            } 
        }

        #endregion
        
        // Following methods are responsible for locking collections, it's the caller responsability
        //private static Func<object> CreateFactory(Type implementationType)
        //{
        //    ConstructorInfo[] constructorInfos = implementationType.GetConstructors();

        //    if (constructorInfos.Length == 0
        //        || (constructorInfos.Length == 1 && !constructorInfos[0].IsPublic))
        //        throw new ArgumentException(String.Format("Cannot Create Instance: No public constructor found in {0}.", implementationType.Name));

        //    // TODO: handle ctor with parameters

        //    // TODO: get first parameterless ctor
        //    ConstructorInfo constructor = constructorInfos.FirstOrDefault(x => x.GetParameters().Length == 0);

        //    if (constructor == null)
        //        throw new ArgumentException(String.Format("Cannot Create Instance: No parameterless constructor found in {0}.", implementationType.Name));

        //    return (() => constructor.Invoke(null));
        //}

        //// Test method only accessible from IocContainer instance
        //public TInterface Test<TInterface>()
        //    where TInterface:class
        //{
        //    Type interfaceType = typeof (TInterface);

        //    if (!_interfaceToImplementationMap.ContainsKey(interfaceType))
        //        return null; // TODO: throw exception

        //    ResolvableConstructor ResolvableConstructor = BuildCreatableConstructor(interfaceType);
        //    if (ResolvableConstructor == ResolvableConstructor.TypeNotRegistered)
        //        throw new ArgumentException(String.Format("Cannot Resolve: No registration found for type {0}", interfaceType.FullName));
        //    if (ResolvableConstructor == ResolvableConstructor.NoPublicConstructorOrNoConstructor)
        //        throw new ArgumentException(String.Format("Cannot Resolve: No constructor or not public constructor for type {0}", interfaceType.FullName));
        //    if (ResolvableConstructor == ResolvableConstructor.NoResolvableConstructor)
        //        throw new ArgumentException(String.Format("Cannot Resolve: No resolvable constructor for type {0}", interfaceType.FullName));
        //    object instance = CreateInstance(ResolvableConstructor);
        //    return (TInterface)instance;
        //}

        private class ResolvableConstructor
        {
            public static readonly ResolvableConstructor TypeNotRegistered = new ResolvableConstructor();
            public static readonly ResolvableConstructor NoPublicConstructorOrNoConstructor = new ResolvableConstructor();
            public static readonly ResolvableConstructor NoResolvableConstructor = new ResolvableConstructor();

            public Type InterfaceType { get; set; }
            public Type ImplementationType { get; set; }
            public ConstructorInfo ConstructorInfo { get; set; }
            public List<ResolvableConstructor> Parameters { get; set; }

            public bool IsValid
            {
                get { return this != TypeNotRegistered && this != NoPublicConstructorOrNoConstructor && this != NoResolvableConstructor; }
            }
        }

        private ResolvableConstructor BuildCreatableConstructor(Type interfaceType)
        {
            if (!_interfaceToImplementationMap.ContainsKey(interfaceType))
                return ResolvableConstructor.TypeNotRegistered;

            Type implementationType = _interfaceToImplementationMap[interfaceType];

            ConstructorInfo[] constructorInfos = implementationType.GetConstructors();

            if (constructorInfos.Length == 0
                || (constructorInfos.Length == 1 && !constructorInfos[0].IsPublic))
                return ResolvableConstructor.NoPublicConstructorOrNoConstructor;

            // Get parameters for each ctor
            var constructorAndParameters = constructorInfos.Select(x => new
            {
                Constructor = x,
                Parameters = x.GetParameters()
            }).ToList();

            // Get first parameterless if any
            var parameterless = constructorAndParameters.FirstOrDefault(x => x.Parameters.Length == 0);
            if (parameterless != null)
                return new ResolvableConstructor
                    {
                        InterfaceType = interfaceType,
                        ImplementationType = implementationType,
                        ConstructorInfo = parameterless.Constructor,
                        // No parameters
                    };

            // Check if a ctor has every parameters registered in container or creatable
            foreach (var c in constructorAndParameters)
            {
                List<ResolvableConstructor> parametersCreatableConstructor = new List<ResolvableConstructor>(c.Parameters.Length);

                bool ok = true;
                foreach (ParameterInfo parameterInfo in c.Parameters)
                {
                    ResolvableConstructor parameterResolvableConstructor = BuildCreatableConstructor(parameterInfo.ParameterType);
                    if (!parameterResolvableConstructor.IsValid)
                    {
                        ok = false;
                        break;
                    }
                    parametersCreatableConstructor.Add(parameterResolvableConstructor);
                }

                if (ok)
                    return new ResolvableConstructor
                        {
                            InterfaceType = interfaceType,
                            ImplementationType = implementationType,
                            ConstructorInfo = c.Constructor,
                            Parameters = parametersCreatableConstructor
                        };
            }

            //
            return ResolvableConstructor.NoResolvableConstructor;
        }

        private object CreateInstance(ResolvableConstructor resolvableConstructor)
        {
            if (resolvableConstructor.ConstructorInfo == null)
                return null;

            // If no parameter, create instance
            if (resolvableConstructor.Parameters == null)
            {
                if (_instances.ContainsKey(resolvableConstructor.InterfaceType))
                    return _instances[resolvableConstructor.InterfaceType];
                return resolvableConstructor.ConstructorInfo.Invoke(null);
            }

            // If parameters, recursively create parameters instance
            object[] parameters = new object[resolvableConstructor.Parameters.Count];
            for (int i = 0; i < resolvableConstructor.Parameters.Count; i++)
            {
                if (_instances.ContainsKey(resolvableConstructor.Parameters[i].InterfaceType))
                    parameters[i] = _instances[resolvableConstructor.Parameters[i].InterfaceType];
                else
                {
                    object parameterInstance;
                    if (_factories.ContainsKey(resolvableConstructor.Parameters[i].InterfaceType))
                    {
                        Func<object> factory = _factories[resolvableConstructor.Parameters[i].InterfaceType];
                        parameterInstance = factory.DynamicInvoke(null);
                    }
                    else
                        parameterInstance = CreateInstance(resolvableConstructor.Parameters[i]);
                    parameters[i] = parameterInstance;
                }
            }
            return resolvableConstructor.ConstructorInfo.Invoke(parameters);
        }

        //private object CreateInstance(ResolvableConstructor ResolvableConstructor, Dictionary<Type, object> instances)
        //{
        //    // If no parameter, create instance
        //    if (ResolvableConstructor.Parameters == null)
        //        return ResolvableConstructor.ConstructorInfo.Invoke(null);
        //
        //    // If parameters, recursively create parameters instance (only one instance for each type)
        //    object[] parameters = new object[ResolvableConstructor.Parameters.Count];
        //    for (int i = 0; i < ResolvableConstructor.Parameters.Count; i++)
        //    {
        //        if (instances.ContainsKey(ResolvableConstructor.Parameters[i].InterfaceType))
        //            parameters[i] = instances[ResolvableConstructor.Parameters[i].InterfaceType];
        //        else
        //        {
        //            object parameterInstance;
        //            if (_factories.ContainsKey(ResolvableConstructor.Parameters[i].InterfaceType))
        //            {
        //                Func<object> factory = _factories[ResolvableConstructor.Parameters[i].InterfaceType];
        //                parameterInstance = factory.DynamicInvoke(null);
        //            }
        //            else
        //                parameterInstance = CreateInstance(ResolvableConstructor.Parameters[i], instances);
        //            instances.Add(ResolvableConstructor.Parameters[i].InterfaceType, parameterInstance);
        //            parameters[i] = parameterInstance;
        //        }
        //    }
        //    return ResolvableConstructor.ConstructorInfo.Invoke(parameters);
        //}
    }
}
