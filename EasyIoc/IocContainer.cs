using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

// TODO: recursive CreateFactory, also called on each parameter

namespace EasyIoc
{
    public class IocContainer : IIocContainer
    {
        private readonly Dictionary<Type, Type> _interfaceToImplementationMap = new Dictionary<Type, Type>();
        private readonly Dictionary<Type, Func<object>> _factories = new Dictionary<Type, Func<object>>();
        private readonly Dictionary<Type, object> _instances = new Dictionary<Type, object>();
        private readonly Dictionary<Type, ConstructorInfo[]> _constructorInfos = new Dictionary<Type, ConstructorInfo[]>(); // TODO: use this in CreateFactory

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

                _constructorInfos.Add(interfaceType, implementationType.GetConstructors());
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
            }
        }

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

                if (_instances.ContainsKey(interfaceType)) // Get instance if any registered
                    resolved = _instances[interfaceType];
                else if (_factories.ContainsKey(interfaceType)) // Create new instance using exising factory if any registered or created on a previous Resolve
                {
                    Func<object> creator = _factories[interfaceType];

                    resolved = creator.DynamicInvoke(null);
                }
                else // Create factory, register factory and create new instance
                {
                    if (!_interfaceToImplementationMap.ContainsKey(interfaceType))
                        throw new ArgumentException(String.Format("Cannot Resolve: No registration found for interface {0}", interfaceType.FullName));
                    Type implementationType = _interfaceToImplementationMap[interfaceType];
                    
                    Func<object> creator = CreateFactory(implementationType);
                    _factories.Add(interfaceType, creator);

                    resolved = creator.DynamicInvoke(null);
                }
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
            } 
        }

        #endregion

        // Test method only accessible from IocContainer instance
        public ConstructorInfo Test<TInterface>()
        {
            Type interfaceType = typeof (TInterface);

            if (!_interfaceToImplementationMap.ContainsKey(interfaceType))
                return null;

            Type implementationType = _interfaceToImplementationMap[interfaceType];

            return GetCreatableConstructor(implementationType);
        }

        // Following methods are responsible for locking collections, it's the caller responsability
        private static Func<object> CreateFactory(Type implementationType)
        {
            ConstructorInfo[] constructorInfos = implementationType.GetConstructors();

            if (constructorInfos.Length == 0
                || (constructorInfos.Length == 1 && !constructorInfos[0].IsPublic))
                throw new ArgumentException(String.Format("Cannot Create Instance: No public constructor found in {0}.", implementationType.Name));

            // TODO: handle ctor with parameters

            // TODO: get first parameterless ctor
            ConstructorInfo constructor = constructorInfos.FirstOrDefault(x => x.GetParameters().Length == 0);
            
            if (constructor == null)
                throw new ArgumentException(String.Format("Cannot Create Instance: No parameterless constructor found in {0}.", implementationType.Name));

            return (() => constructor.Invoke(null));
        }

        // Under construction
        private ConstructorInfo GetCreatableConstructor(Type type)
        {
            ConstructorInfo[] constructorInfos = type.GetConstructors();

            if (constructorInfos.Length == 0
                || (constructorInfos.Length == 1 && !constructorInfos[0].IsPublic))
                return null;

            // Get parameters for each ctor
            var constructorAndParameters = constructorInfos.Select(x => new
            {
                Constructor = x,
                Parameters = x.GetParameters()
            }).ToList();

            // Get first parameterless if any
            var parameterless = constructorAndParameters.FirstOrDefault(x => x.Parameters.Length == 0);
            if (parameterless != null)
                return parameterless.Constructor;

            // Check if a ctor has every parameters registered in container or creatable
            //foreach (var c in constructorAndParameters)
            //{
            //    bool ok = true;
            //    foreach (ParameterInfo parameterInfo in c.Parameters)
            //    {
            //        if (_interfaceToImplementationMap.ContainsKey(parameterInfo.ParameterType))
            //        {
            //            Type implementationType = _interfaceToImplementationMap[parameterInfo.ParameterType];
            //            ConstructorInfo creatableConstructor = GetCreatableConstructor(implementationType);
            //            if (creatableConstructor == null)
            //            {
            //                ok = false;
            //                break;
            //            }
            //        }
            //    }

            //    if (ok)
            //        return c.Constructor;
            //}
            var callableConstructor = constructorAndParameters
                .FirstOrDefault(c => c.Parameters
                    .All(p => 
                        _interfaceToImplementationMap.ContainsKey(p.ParameterType) 
                        && GetCreatableConstructor(_interfaceToImplementationMap[p.ParameterType]) != null));
            if (callableConstructor != null)
                return callableConstructor.Constructor;

            return null;
        }
    }
}
