using System.Reflection;
using EasyIoc;

namespace Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            IocContainer container = new IocContainer();
            container.RegisterType<ITestInterface1, TestClass6ImplementatingInterface1>();
            container.RegisterType<ITestInterface2, TestClass1ImplementingInterface2>();

            ConstructorInfo info = container.Test<ITestInterface1>();
        }
    }

    public interface ITestInterface1
    {
    }

    public class TestClass6ImplementatingInterface1 : ITestInterface1
    {
        public TestClass6ImplementatingInterface1(ITestInterface2 interface2)
        {
        }
    }

    public interface ITestInterface2
    {
    }

    public class TestClass1ImplementingInterface2 : ITestInterface2
    {
    }
}
