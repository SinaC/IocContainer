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

    public class TestClass2ImplementingInterface2 : ITestInterface2
    {
    }

    public class TestClassImplementatingNoInterface // Doesn't implement interface
    {
    }
}
