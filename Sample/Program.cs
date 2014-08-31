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
            container.RegisterType<ITestInterface3, TestClass1ImplementingInterface3>();
            container.RegisterFactory<ITestInterface3>(() => new TestClass1ImplementingInterface3());

            ITestInterface1 info = container.Test<ITestInterface1>();
        }
    }

    public interface ITestInterface1
    {
    }

    public class TestClass6ImplementatingInterface1 : ITestInterface1
    {
        private static int _count = 0;

        public TestClass6ImplementatingInterface1(ITestInterface2 interface2, ITestInterface3 interface3)
        {
            _count++;
        }
    }

    public interface ITestInterface2
    {
    }

    public class TestClass1ImplementingInterface2 : ITestInterface2
    {
        private static int _count = 0;

        public TestClass1ImplementingInterface2(ITestInterface3 interface3)
        {
            _count++;
        }
    }

    public interface ITestInterface3
    {
    }

    public class TestClass1ImplementingInterface3 : ITestInterface3
    {
        private static int _count = 0;

        //public TestClass1ImplementingInterface3(ITestInterface1 interface1)
        public TestClass1ImplementingInterface3()
        {
            _count++;
        }
    }
}
