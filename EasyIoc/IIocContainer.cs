using System;

namespace EasyIoc
{
    public enum ResolveMethods
    {
        NewInstance,
        Singleton
    }

    public interface IIocContainer
    {
        bool IsRegistered<TInterface>()
            where TInterface : class;

        void Register<TInterface, TImplementation>()
            where TInterface : class
            where TImplementation : class;

        void Register<TInterface>(Func<TInterface> createFunc)
            where TInterface : class;

        void Register<TInterface>(TInterface instance)
            where TInterface : class;

        void Unregister<TInterface>()
            where TInterface : class;

        TInterface Resolve<TInterface>(ResolveMethods method = ResolveMethods.Singleton)
            where TInterface : class;

        void Reset();
    }
}
