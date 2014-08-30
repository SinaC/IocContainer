using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EasyIoc
{
    // TODO: recursive resolve on ctor parameters
    // questions: which ctor should be choosed if multiple ctor?
    public class IocContainer : IIocContainer
    {
        private static readonly Type NoImplementationType = null;

        private readonly Dictionary<Type, Type> _interfaceToImplementationMap = new Dictionary<Type, Type>();
        private readonly Dictionary<Type, object> _instanceRegistry = new Dictionary<Type, object>();
        private readonly Dictionary<Type, Func<object>> _createInstanceFunc = new Dictionary<Type, Func<object>>();

        private readonly object _lockObject = new object();

        private static IocContainer _default;

        public static IIocContainer Default
        {
            get
            {
                return _default ?? (_default = new IocContainer());
            }
        }

        public bool IsRegistered<TInterface>()
            where TInterface : class
        {
            Type interfaceType = typeof (TInterface);
            return _interfaceToImplementationMap.ContainsKey(interfaceType);
        }

        public void Register<TInterface, TImplementation>()
            where TInterface : class
            where TImplementation : class
        {
            Type interfaceType = typeof (TInterface);

            if (!interfaceType.IsInterface)
                throw new ArgumentException("Only an interface can be registered");

            Type implementationType = typeof (TImplementation);

            if (implementationType.IsInterface || implementationType.IsAbstract)
                throw new ArgumentException("Cannot register an interface or an abstract class");

            if (!interfaceType.IsAssignableFrom(implementationType))
                throw new ArgumentException(String.Format("{0} is not assignable from {1}", interfaceType.FullName, implementationType.FullName));

            lock (_lockObject)
                InternalRegister(interfaceType, implementationType, () => CreateObjectFunc(implementationType));
        }

        public void Register<TInterface>(Func<TInterface> createFunc)
            where TInterface : class
        {
            if (createFunc == null)
                throw new ArgumentNullException("createFunc");

            Type interfaceType = typeof(TInterface);

            if (!interfaceType.IsInterface)
                throw new ArgumentException("Only an interface can be registered");

            lock (_lockObject)
                InternalRegister(interfaceType, NoImplementationType, () => createFunc);
        }

        public void Unregister<TInterface>()
            where TInterface : class
        {
            Type interfaceType = typeof(TInterface);

            lock (_lockObject)
            {
                if (!_interfaceToImplementationMap.ContainsKey(interfaceType))
                    throw new ArgumentException("Cannot Unregister: No registration found for interface {0}", interfaceType.FullName);

                _interfaceToImplementationMap.Remove(interfaceType);
                _instanceRegistry.Remove(interfaceType);
                _createInstanceFunc.Remove(interfaceType);
            }
        }

        public TInterface Resolve<TInterface>()
            where TInterface : class
        {
            Type interfaceType = typeof (TInterface);

            if (!interfaceType.IsInterface)
                throw new ArgumentException("Only an interface can be resolved");

            object resolved = null;
            lock (_lockObject)
            {
                if (!_createInstanceFunc.ContainsKey(interfaceType))
                    throw new ArgumentException(String.Format("Cannot Resolve: No registration found for interface {0}", interfaceType.FullName));

                // Check if instance already created
                if (_instanceRegistry.ContainsKey(interfaceType))
                    resolved = _instanceRegistry[interfaceType];

                // If no instance found, create one
                if (resolved == null)
                {
                    Func<object> creator = _createInstanceFunc[interfaceType];

                    resolved = creator.DynamicInvoke(null);

                    if (!_instanceRegistry.ContainsKey(interfaceType))
                        _instanceRegistry.Add(interfaceType, resolved);
                }
            }
            return (TInterface) resolved;
        }

        public void Reset()
        {
            lock (_lockObject)
            {
                _interfaceToImplementationMap.Clear();
                _instanceRegistry.Clear();
                _createInstanceFunc.Clear();
            }
        }

        // Following methods are not responsible for sync-locking, this must be done by caller
        private static Func<object> CreateObjectFunc(Type implementationType)
        {
            ConstructorInfo[] constructorInfos = implementationType.GetConstructors();

            if (constructorInfos.Length == 0
                || (constructorInfos.Length == 1 && !constructorInfos[0].IsPublic))
                throw new Exception(String.Format("Cannot Resolve: No public constructor found in {0}.", implementationType.Name));

            // TODO: get first parameterless ctor
            ConstructorInfo constructor = constructorInfos.First();
            ParameterInfo[] parameterInfos = constructor.GetParameters();

            // TODO: handle ctor with parameters

            if (parameterInfos.Length != 0)
                throw new Exception(String.Format("Cannot Resolve: No parameterless constructor found"));

            return () => constructor.Invoke(null);
        }

        private void InternalRegister(Type interfaceType, Type implementationType, Func<Func<object>> createCreatorFunc)
        {
            if (_interfaceToImplementationMap.ContainsKey(interfaceType))
            {
                if (_interfaceToImplementationMap[interfaceType] != implementationType)
                    throw new InvalidOperationException(String.Format("There is already a class/function registered for interface {0}", interfaceType.FullName));
            }
            else
                _interfaceToImplementationMap.Add(interfaceType, implementationType);

            if (!_createInstanceFunc.ContainsKey(interfaceType))
            {
                Func<object> creator = createCreatorFunc.Invoke();
                _createInstanceFunc.Add(interfaceType, creator);
            }
        }
    }
}
