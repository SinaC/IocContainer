using System;

namespace SinaC.IocContainer.Attributes
{
    [AttributeUsage(AttributeTargets.Constructor)]
    public class ResolveConstructorAttribute : Attribute
    {
    }
}
