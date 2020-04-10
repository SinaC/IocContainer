using System;

namespace SinaC.IocContainer.Exceptions
{
    public class RegisterTypeException : Exception
    {
        public RegisterTypeException(string msg) : base(msg)
        {
        }
    }
}
