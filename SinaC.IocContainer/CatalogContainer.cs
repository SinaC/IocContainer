using System;
using System.Collections.Generic;
using System.Linq;

namespace SinaC.IocContainer
{
    internal class CatalogContainer
    {
        private Dictionary<Type, TypeStorageCollection> ByType { get; set; }

        public bool IsTypeRegistered(Type interfaceType, string name)
        {
        }

        public bool IsTypeUniquelyRegistered(Type interfaceType, Type implementationType, string name)
        {
        }

        public void RegisterType(RegisterTypeEntry registerTypeEntry)
        {
            // TODO
        }
    }
}
