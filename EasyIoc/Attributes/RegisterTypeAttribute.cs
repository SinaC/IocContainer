using System;

namespace EasyIoc.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RegisterTypeAttribute : Attribute
    {
        public Type InterfaceType { get; set; }
        public string Name { get; set; }
        public bool IsSingleton { get; set; }

        public RegisterTypeAttribute(Type interfaceType) : this(null, false, interfaceType)
        {
        }

        public RegisterTypeAttribute(string name, Type interfaceType) : this(name, false, interfaceType)
        {
        }

        public RegisterTypeAttribute(bool isSingleton, Type interfaceType) : this(null, isSingleton, interfaceType)
        {
        }

        public RegisterTypeAttribute(string name, bool isSingleton, Type interfaceType)
        {
            InterfaceType = interfaceType;
            IsSingleton = isSingleton;
            Name = name;
        }
    }
}
