using System.Collections.Generic;

namespace SinaC.IocContainer
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

        void RegisterInstance<TInterface, TImplementation>(TImplementation instance)
            where TInterface : class
            where TImplementation : class, TInterface;

        void RegisterInstance<TInterface, TImplementation>(TImplementation instance, string instanceName)
            where TInterface : class
            where TImplementation : class, TInterface;

        TInterface Resolve<TInterface>()
            where TInterface : class;

        TInterface Resolve<TInterface>(string name)
            where TInterface : class;

        IEnumerable<TInterface> ResolveAll<TInterface>()
            where TInterface : class;

        bool TryResolve<TInterface>(out TInterface instance)
            where TInterface : class;

        bool TryResolve<TInterface>(string name, out TInterface instance)
            where TInterface : class;

        void Reset();
    }
}
