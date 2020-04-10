using System;

namespace EasyIoc.Exceptions
{
    public class ResolveException : Exception
    {
        public ResolveException(string msg) : base(msg)
        {
        }

        public ResolveException(string msg, Exception innerException) : base(msg, innerException)
        {
        }
    }
}
