﻿namespace EasyIoc
{
    public interface IIocContainer
    {
        void InitializeFromConfig();

        void InitializeFromConfig(string filename);

        bool IsRegistered<TInterface>()
            where TInterface : class;

        bool IsRegistered<TInterface>(string name)
            where TInterface : class;

        void RegisterType<TInterface, TImplementation>()
            where TInterface : class
            where TImplementation : class, TInterface;

        void RegisterType<TInterface, TImplementation>(string name)
            where TInterface : class
            where TImplementation : class, TInterface;

        void RegisterInstance<TInterface>(TInterface instance)
            where TInterface : class;

        void RegisterInstance<TInterface>(TInterface instance, string instanceName)
            where TInterface : class;

        void Unregister<TInterface>()
            where TInterface : class;

        void UnregisterType<TInterface>()
            where TInterface : class;

        void UnregisterType<TInterface>(string name)
            where TInterface : class;

        void UnregisterInstance<TInterface>()
            where TInterface : class;

        void UnregisterInstance<TInterface>(string instanceName)
            where TInterface : class;

        TInterface Resolve<TInterface>()
            where TInterface : class;

        TInterface Resolve<TInterface>(string name)
            where TInterface : class;

        bool TryResolve<TInterface>(out TInterface instance)
            where TInterface: class;

        bool TryResolve<TInterface>(string name, out TInterface instance)
            where TInterface : class;

        void Reset();
    }
}
