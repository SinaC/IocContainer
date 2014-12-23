using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

// Unless a specific instance has been registered via RegisterInstance, every Resolve will create a new instance
// Resolve with parameters doesn't use resolvable constructor cache

namespace EasyIoc
{
    public class IocContainer : IIocContainer
    {
        private readonly Dictionary<Type, Type> _interfaceToImplementationMap = new Dictionary<Type, Type>();
        private readonly Dictionary<Type, Func<object>> _factories = new Dictionary<Type, Func<object>>();
        private readonly Dictionary<Type, object> _instances = new Dictionary<Type, object>();
        private readonly Dictionary<Type, ResolvableConstructor> _resolvableConstructors = new Dictionary<Type, ResolvableConstructor>();

        private readonly object _lockObject = new object();

        private static readonly Lazy<IocContainer> LazyDefault = new Lazy<IocContainer>(() => new IocContainer());
        public static IIocContainer Default
        {
            get { return LazyDefault.Value; }
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
            Type interfaceType = typeof (TInterface);

            if (!interfaceType.IsInterface)
                throw new ArgumentException("Cannot Register: Only an interface can be registered");

            Type implementationType = typeof (TImplementation);

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

                // Resolvable will be built on first Resolve
                //  because BuildResolvableConstructor will check interfaceToImplementationMap to detect if a ctor is resolvable
                //  and we cannot force the developer to use RegisterType in the right order
            }
        }

        public void RegisterFactory<TInterface>(Func<TInterface> createFunc)
            where TInterface : class
        {
            if (createFunc == null)
                throw new ArgumentNullException("createFunc");

            Type interfaceType = typeof (TInterface);

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

            Type interfaceType = typeof (TInterface);

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
            Type interfaceType = typeof (TInterface);

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

        public TInterface Resolve<TInterface>()
            where TInterface : class
        {
            Type interfaceType = typeof (TInterface);

            if (!interfaceType.IsInterface)
                throw new ArgumentException("Cannot Resolve: Only an interface can be resolved");

            object resolved;
            lock (_lockObject)
            {
                if (!_interfaceToImplementationMap.ContainsKey(interfaceType))
                    throw new ArgumentException(String.Format("Cannot Resolve: No registration found for interface {0}", interfaceType.FullName));

                // Search in registered instances
                if (_instances.ContainsKey(interfaceType))
                    resolved = _instances[interfaceType];
                else
                {
                    // Search in resolvable constructor cache
                    ResolvableConstructor resolvableConstructor;
                    if (_resolvableConstructors.ContainsKey(interfaceType))
                        resolvableConstructor = _resolvableConstructors[interfaceType];
                    else
                    {
                        // Create resolvable constructor
                        resolvableConstructor = BuildResolvableConstructor(interfaceType);
                        _resolvableConstructors.Add(interfaceType, resolvableConstructor);
                    }
                    // Check errors
                    if (resolvableConstructor == ResolvableConstructor.TypeNotRegistered)
                        throw new ArgumentException(String.Format("Cannot Resolve: No registration found for type {0}", interfaceType.FullName));
                    if (resolvableConstructor == ResolvableConstructor.NoPublicConstructorOrNoConstructor)
                        throw new ArgumentException(String.Format("Cannot Resolve: No constructor or not public constructor for type {0}", interfaceType.FullName));
                    if (resolvableConstructor == ResolvableConstructor.NoResolvableConstructor)
                        throw new ArgumentException(String.Format("Cannot Resolve: No resolvable constructor for type {0}", interfaceType.FullName));
                    if (resolvableConstructor == ResolvableConstructor.CyclicDependencyConstructor)
                        throw new ArgumentException(String.Format("Cannot Resolve: Cyclic dependency detected for type {0}", interfaceType.FullName));
                    // Create instance
                    try
                    {
                        resolved = resolvableConstructor.Resolve(_instances, _factories);
                    }
                    catch (InvalidOperationException ex)
                    {
                        throw new InvalidOperationException("Cannot Resolve: See innerException", ex);
                    }
                }
            }
            return (TInterface) resolved;
        }

        public TInterface Resolve<TInterface>(IEnumerable<ParameterValue> parameters)
            where TInterface : class
        {
            Type interfaceType = typeof (TInterface);

            if (!interfaceType.IsInterface)
                throw new ArgumentException("Cannot Resolve: Only an interface can be resolved");

            object resolved;
            lock (_lockObject)
            {
                if (!_interfaceToImplementationMap.ContainsKey(interfaceType))
                    throw new ArgumentException(String.Format("Cannot Resolve: No registration found for interface {0}", interfaceType.FullName));

                // Search in registered instances
                if (_instances.ContainsKey(interfaceType))
                    resolved = _instances[interfaceType];
                else
                {
                    // Create resolvable constructor
                    List<ParameterValue> userDefinedParameters = parameters.ToList();
                    ResolvableConstructor resolvableConstructor = BuildResolvableConstructor(interfaceType, userDefinedParameters);
                    if (resolvableConstructor == ResolvableConstructor.TypeNotRegistered)
                        throw new ArgumentException(String.Format("Cannot Resolve: No registration found for type {0}", interfaceType.FullName));
                    if (resolvableConstructor == ResolvableConstructor.NoPublicConstructorOrNoConstructor)
                        throw new ArgumentException(String.Format("Cannot Resolve: No constructor or not public constructor for type {0}", interfaceType.FullName));
                    if (resolvableConstructor == ResolvableConstructor.NoResolvableConstructor)
                        throw new ArgumentException(String.Format("Cannot Resolve: No resolvable constructor for type {0}", interfaceType.FullName));
                    if (resolvableConstructor == ResolvableConstructor.CyclicDependencyConstructor)
                        throw new ArgumentException(String.Format("Cannot Resolve: Cyclic dependency detected for type {0}", interfaceType.FullName));
                    // Create instance
                    try
                    {
                        resolved = resolvableConstructor.Resolve(_instances, _factories);
                    }
                    catch (InvalidOperationException ex)
                    {
                        throw new InvalidOperationException("Cannot Resolve: See innerException", ex);
                    }
                }
            }
            return (TInterface) resolved;
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

        // Following methods are NOT responsible for locking collections
        private interface IResolvableParameter
        {
            object Resolve(IDictionary<Type, object> instances, IDictionary<Type, Func<object>> factories);
        }

        private class UserDefinedParameter : IResolvableParameter
        {
            public ParameterValue ParameterValue { get; set; }

            public object Resolve(IDictionary<Type, object> instances, IDictionary<Type, Func<object>> factories)
            {
                return ParameterValue.Value;
            }
        }

        private class ResolvableConstructor : IResolvableParameter
        {
            public static readonly ResolvableConstructor TypeNotRegistered = new ResolvableConstructor();
            public static readonly ResolvableConstructor NoPublicConstructorOrNoConstructor = new ResolvableConstructor();
            public static readonly ResolvableConstructor NoResolvableConstructor = new ResolvableConstructor();
            public static readonly ResolvableConstructor CyclicDependencyConstructor = new ResolvableConstructor();

            public Type InterfaceType { private get; set; }
            //public Type ImplementationType { private get; set; }
            public ConstructorInfo ConstructorInfo { private get; set; }
            public List<IResolvableParameter> Parameters { private get; set; }

            public bool IsValid
            {
                get { return this != TypeNotRegistered && this != NoPublicConstructorOrNoConstructor && this != NoResolvableConstructor && this != CyclicDependencyConstructor; }
            }

            public object Resolve(IDictionary<Type, object> instances, IDictionary<Type, Func<object>> factories)
            {
                if (ConstructorInfo == null)
                    throw new InvalidOperationException("No constructor info found");

                // If parameterless, create instance
                if (Parameters == null)
                {
                    // Search in registered instances
                    if (instances.ContainsKey(InterfaceType))
                        return instances[InterfaceType];
                    return ConstructorInfo.Invoke(null);
                }

                // If parameters, recursively create parameters instance
                object[] parameters = new object[Parameters.Count];
                for (int i = 0; i < Parameters.Count; i++)
                {
                    IResolvableParameter unspecializedParameter = Parameters[i];

                    if (unspecializedParameter is UserDefinedParameter)
                        parameters[i] = (unspecializedParameter as UserDefinedParameter).ParameterValue.Value;
                    else if (unspecializedParameter is ResolvableConstructor)
                    {
                        ResolvableConstructor parameter = unspecializedParameter as ResolvableConstructor;

                        // Search in registered instances
                        if (instances.ContainsKey(parameter.InterfaceType))
                            parameters[i] = instances[parameter.InterfaceType];
                        else
                        {
                            object parameterInstance;
                            // Use factory is any registered for parameter type
                            if (factories.ContainsKey(parameter.InterfaceType))
                            {
                                Func<object> factory = factories[parameter.InterfaceType];
                                parameterInstance = factory.DynamicInvoke(null);
                            }
                            else
                                // Recursively resolve parameter
                                parameterInstance = unspecializedParameter.Resolve(instances, factories);
                            parameters[i] = parameterInstance;
                        }
                    }

                }
                // and create instance using parameters
                return ConstructorInfo.Invoke(parameters);
            }
        }

        private ResolvableConstructor BuildResolvableConstructor(Type interfaceType, List<ParameterValue> userDefinedParameters = null)
        {
            List<Type> discoveredTypes = new List<Type>
                {
                    interfaceType
                };
            return InnerBuildResolvableConstructor(interfaceType, discoveredTypes, userDefinedParameters);
        }

        private ResolvableConstructor InnerBuildResolvableConstructor(Type interfaceType, ICollection<Type> discoveredTypes, List<ParameterValue> userDefinedParameters = null)
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
                        //ImplementationType = implementationType, NOT USED
                        ConstructorInfo = parameterless.Constructor,
                        // No parameters
                    };

            // Check if every ctor's parameter is registered in container or resolvable, returns first resolvable
            foreach (var c in constructorAndParameters)
            {
                List<IResolvableParameter> parametersResolvableConstructor = new List<IResolvableParameter>(c.Parameters.Length);

                bool ok = true;
                foreach (ParameterInfo parameterInfo in c.Parameters)
                {
                    if (userDefinedParameters != null && userDefinedParameters.Any(x => x.Name == parameterInfo.Name))
                    {
                        ParameterValue parameterValue = userDefinedParameters.First(x => x.Name == parameterInfo.Name);

                        IResolvableParameter parameter = new UserDefinedParameter
                            {
                                ParameterValue = parameterValue
                            };
                        parametersResolvableConstructor.Add(parameter);
                    }
                    else
                    {
                        if (discoveredTypes.Any(x => x == parameterInfo.ParameterType)) // check cyclic dependency
                        {
                            ok = false;
                            break;
                        }

                        discoveredTypes.Add(parameterInfo.ParameterType);
                        ResolvableConstructor parameterResolvableConstructor = InnerBuildResolvableConstructor(parameterInfo.ParameterType, discoveredTypes);
                        discoveredTypes.Remove(interfaceType);

                        if (!parameterResolvableConstructor.IsValid) // once an invalid ctor parameter has been found, try next ctor
                        {
                            ok = false;
                            break;
                        }
                        parametersResolvableConstructor.Add(parameterResolvableConstructor);
                    }

                }

                if (ok)
                    return new ResolvableConstructor
                        {
                            InterfaceType = interfaceType,
                            //ImplementationType = implementationType, NOT USED
                            ConstructorInfo = c.Constructor,
                            Parameters = parametersResolvableConstructor
                        };
            }

            //
            return ResolvableConstructor.NoResolvableConstructor;
        }

    }
}
