namespace EasyIoc.Exceptions
{
    internal class NoConstructorResolveException : ResolveException
    {
        public NoConstructorResolveException(string msg) : base(msg)
        {
        }
    }
}
