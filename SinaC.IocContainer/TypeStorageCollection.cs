using System.Collections.Generic;

namespace SinaC.IocContainer
{
    internal class TypeStorageCollection
    {
        // RegisterTypeEntry:
        //  By InterfaceType, multiple name ImplementationType + multiple anonymous ImplementationType (used only for ResolveAll)
        //  Dictionary<Type, TypeStorageCollection>
        //  TypeStorageCollection: 1 dictionary<ImplementationType, RegisterTypeEntry> + 1 dictionary<name, RegisterTypeEntry>
        // RegisterInstanceEntry:
        //  By InterfaceType, multiple name + one anonymous
        //  Dictionary<Type, TypeStorageCollection2>
        //  TypeStorageCollection2: 1 RegisterTypeEntry + 1 dictionary<name, RegisterTypeEntry>

        private Dictionary<string, RegisterTypeEntry> NamedRegisteredTypeEntries { get; set; }
        private List<RegisterTypeEntry> AnonymousRegisteredTypeEntries { get; set; }

        private Dictionary<string,RegisterInstanceEntry> NamedRegisteredInstanceEntries { get; set; }
        private List<RegisterInstanceEntry> AnonymousRegisteredInstanceEntries { get; set; }
    }
}
