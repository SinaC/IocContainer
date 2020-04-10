using System;

namespace EasyIoc.Attributes
{
    [AttributeUsage(AttributeTargets.Constructor)]
    public class ResolvingConstructorAttribute : Attribute
    {
    }
}
