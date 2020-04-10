using System.Configuration;

namespace SinaC.IocContainer.Configuration
{
    public class AssemblyConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("assemblies")]
        public AssemblyConfigCollection Assemblies => base["assemblies"] as AssemblyConfigCollection;
    }

    [ConfigurationCollection(typeof(AssemblyConfigCollection), AddItemName = "assembly", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class AssemblyConfigCollection : ConfigurationElementCollection
    {
        public AssemblyConfigCollectionElement this[int index]
        {
            get => (AssemblyConfigCollectionElement) BaseGet(index);
            set
            {
                if (BaseGet(index) != null)
                    BaseRemoveAt(index);
                BaseAdd(index, value);
            }
        }

        protected override ConfigurationElement CreateNewElement() => new AssemblyConfigCollectionElement();

        protected override object GetElementKey(ConfigurationElement element) => ((AssemblyConfigCollectionElement) element).QualifiedName;

        // unnecessary methods
        //public void Assembly(AssemblyConfigCollectionElement assembly)
        //{
        //    BaseAdd(assembly);
        //}

        //public void Clear()
        //{
        //    BaseClear();
        //}

        //public void Remove(AssemblyConfigCollectionElement assembly)
        //{
        //    BaseRemove(assembly.QualifiedName);
        //}

        //public void RemoveAt(int index)
        //{
        //    BaseRemoveAt(index);
        //}

        //public void Remove(string name)
        //{
        //    BaseRemove(name);
        //}
    }

    public class AssemblyConfigCollectionElement : ConfigurationElement
    {
        [ConfigurationProperty("qualifiedName", IsRequired = true, IsKey = true)]
        public string QualifiedName => (string) base["qualifiedName"];
    }
}
