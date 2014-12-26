using EasyIoc;

namespace Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass6ImplementatingInterface1>();
            IocContainer.Default.RegisterType<ITestInterface2, TestClass1ImplementingInterface2>();
            //IocContainer.Default.RegisterInstance<ITestInterface3>(new TestClass1ImplementingInterface3());
            IocContainer.Default.RegisterType<ITestInterface3,TestClass1ImplementingInterface3>();

            ITestInterface1 info = IocContainer.Default.Resolve<ITestInterface1>();
        }
    }

    public interface ITestInterface1
    {
    }

    public class TestClass6ImplementatingInterface1 : ITestInterface1
    {
        private static int _count = 0;

        public ITestInterface2 TestInterface2 { get; private set; }
        public ITestInterface3 TestInterface3 { get; private set; }

        public TestClass6ImplementatingInterface1(ITestInterface2 interface2, ITestInterface3 interface3)
        {
            _count++;

            TestInterface2 = interface2;
            TestInterface3 = interface3;
        }
    }

    public class TestClass7ImplementatingInterface1 : ITestInterface1
    {
        private static int _count = 0;

        public int X { get; private set; }

        public TestClass7ImplementatingInterface1(int x)
        {
            _count++;

            X = x;
        }
    }

    public interface ITestInterface2
    {
    }

    public class TestClass1ImplementingInterface2 : ITestInterface2
    {
        private static int _count = 0;

        public ITestInterface3 TestInterface3 { get; private set; }

        public TestClass1ImplementingInterface2(ITestInterface3 interface3)
        {
            _count++;

            TestInterface3 = interface3;
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
