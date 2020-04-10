using System;

namespace EasyIoc.Exceptions
{
    public class RegisterInstanceException : Exception
    {
        public RegisterInstanceException(string msg) : base(msg)
        {
        }
    }
}
