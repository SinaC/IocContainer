using System;

namespace SinaC.IocContainer
{
    internal abstract class RegisterEntryBase
    {
        public Type InterfaceType { get; set; }
        public string Name { get; set; }
    }
}
