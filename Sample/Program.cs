using EasyIoc;

namespace Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            //IocContainer.Default.RegisterType<ITestInterface1, TestClass6ImplementatingInterface1>();
            //IocContainer.Default.RegisterType<ITestInterface2, TestClass1ImplementingInterface2>();
            ////IocContainer.Default.RegisterInstance<ITestInterface3>(new TestClass1ImplementingInterface3());
            ////IocContainer.Default.RegisterType<ITestInterface3,TestClass1ImplementingInterface3>();
            //IocContainer.Default.RegisterInstance<ITestInterface3>(new TestClass1ImplementingInterface3());

            //ITestInterface1 info = IocContainer.Default.Resolve<ITestInterface1>();

            //IocContainer.Default.RegisterType<ITestInterfaceMultiple1, TestClassImplemententingMultiple1AndMultiple2>();
            //IocContainer.Default.RegisterType<ITestInterfaceMultiple2, TestClassImplemententingMultiple1AndMultiple2>();
            //ITestInterfaceMultiple1 i1 = IocContainer.Default.Resolve<ITestInterfaceMultiple1>();
            //ITestInterfaceMultiple2 i2 = IocContainer.Default.Resolve<ITestInterfaceMultiple2>();
            IocContainer.Default.RegisterInstance<ITestInterfaceMultiple1>(new TestClassImplemententingMultiple1AndMultiple2());
            IocContainer.Default.RegisterInstance<ITestInterfaceMultiple2>(new TestClassImplemententingMultiple1AndMultiple2());

            ITestInterfaceMultiple1 i1 = IocContainer.Default.Resolve<ITestInterfaceMultiple1>();
            ITestInterfaceMultiple2 i2 = IocContainer.Default.Resolve<ITestInterfaceMultiple2>();
        }
    }

    public interface ITestInterfaceMultiple1
    {
    }

    public interface ITestInterfaceMultiple2
    {
    }

    public class TestClassImplemententingMultiple1AndMultiple2 : ITestInterfaceMultiple1, ITestInterfaceMultiple2
    {
        private int _id;
        private static int _count = 0;

        public TestClassImplemententingMultiple1AndMultiple2()
        {
            _id = _count;
            _count++;
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
