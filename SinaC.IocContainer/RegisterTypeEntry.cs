using System;

namespace SinaC.IocContainer
{
    internal sealed class RegisterTypeEntry : RegisterEntryBase
    {
        public Type ImplementationType { get; set; }
        public bool IsSingleton { get; set; }
    }
}
