namespace EasyIocTest
{
    public interface ITestInterface1
    {
    }

    public class TestClass1ImplementingInterface1 : ITestInterface1
    {
    }

    public class TestClass2ImplementingInterface1 : ITestInterface1
    {
    }

    public abstract class TestAbstractClassImplentatingInterface1 : ITestInterface1
    {
    }

    public class TestClass3ImplementingInterface1 : ITestInterface1
    {
        public TestClass3ImplementingInterface1(int x)
        {
        }
    }

    public class TestClass4ImplementingInterface1 : ITestInterface1
    {
        protected TestClass4ImplementingInterface1()
        {
        }
    }

    public class TestClass5ImplementingInterface1 : TestClass4ImplementingInterface1
    {
    }

    public class TestClass6ImplementingInterface1 : ITestInterface1
    {
        public TestClass6ImplementingInterface1(ITestInterface2 interface2)
        {
        }
    }

    public interface ITestInterface2
    {
    }

    public class TestClass1ImplementingInterface2 : ITestInterface2
    {
    }

    public class TestClass2ImplementingInterface2 : ITestInterface2
    {
    }

    public class TestClass3ImplementingInterface2 : ITestInterface2
    {
        public TestClass3ImplementingInterface2(ITestInterface1 interface1)
        {
        }
    }

    public class TestClass4ImplementingInterface2 : ITestInterface2
    {
        public TestClass4ImplementingInterface2(ITestInterface2 interface2)
        {
        }
    }

    public class TestClass5ImplementingInterface2 : ITestInterface2
    {
        public TestClass5ImplementingInterface2(ITestInterface3 interface3)
        {
        }
    }

    public class TestClassImplementingNoInterface // Doesn't implement interface
    {
    }

    public interface ITestInterface3
    {
    }

    public class TestClass1ImplementingInterface3 : ITestInterface3
    {
        public TestClass1ImplementingInterface3(ITestInterface3 interface3)
        {
        }

        public TestClass1ImplementingInterface3(ITestInterface2 interface2)
        {
        }

        public TestClass1ImplementingInterface3(ITestInterface1 interface1)
        {
        }
    }
}
