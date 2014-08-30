using System;

namespace EasyIoc
{
    public interface IIocContainer
    {
        bool IsRegistered<TInterface>()
            where TInterface : class;

        void Register<TInterface, TImplementation>()
            where TInterface : class
            where TImplementation : class;

        void Register<TInterface>(Func<TInterface> createFunc)
            where TInterface : class;

        void Unregister<TInterface>()
            where TInterface : class;

        TInterface Resolve<TInterface>()
            where TInterface : class;

        void Reset();
    }
}
