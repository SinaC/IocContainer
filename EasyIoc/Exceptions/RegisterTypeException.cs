using System;

namespace EasyIoc.Exceptions
{
    public class RegisterTypeException : Exception
    {
        public RegisterTypeException(string msg) : base(msg)
        {
        }
    }
}
