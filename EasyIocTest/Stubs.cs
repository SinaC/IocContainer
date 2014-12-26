using System.Collections.Generic;

namespace EasyIocTest
{
    public static class CountCall
    {
        private static readonly object Lock;
        private static Dictionary<string, int> _countCall;

        static CountCall()
        {
            Lock = new object();
            Reset();
        }

        public static int Count(string callId)
        {
            lock(Lock)
            {
                int value;
                if (_countCall.TryGetValue(callId, out value))
                    return value;
                return -1;
            }
        }

        public static void Increment(string callId)
        {
            lock(Lock)
            {
                if (_countCall.ContainsKey(callId))
                    _countCall[callId]++;
                else
                    _countCall.Add(callId, 1);
            }
        }

        public static void Reset()
        {
            lock (Lock)
                _countCall = new Dictionary<string, int>();
        }
    }

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
        public int X { get; private set; }

        public TestClass3ImplementingInterface1(int x)
        {
            CountCall.Increment("TestClass3ImplementingInterface1");
            X = x;
        }
    }

    public class TestClass4ImplementingInterface1 : ITestInterface1
    {
        protected TestClass4ImplementingInterface1()
        {
            CountCall.Increment("TestClass4ImplementingInterface1");
        }
    }

    public class TestClass5ImplementingInterface1 : TestClass4ImplementingInterface1
    {
    }

    public class TestClass6ImplementingInterface1 : ITestInterface1
    {
        public ITestInterface2 Interface2 { get; private set; }

        public TestClass6ImplementingInterface1(ITestInterface2 interface2)
        {
            CountCall.Increment("TestClass6ImplementingInterface1");
            Interface2 = interface2;
        }
    }

    public class TestClass7ImplementingInterface1 : ITestInterface1
    {
        public TestClass7ImplementingInterface1()
        {
            CountCall.Increment("TestClass7ImplementingInterface1");
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
        public ITestInterface1 Interface1 { get; private set; }

        public TestClass3ImplementingInterface2(ITestInterface1 interface1)
        {
            CountCall.Increment("TestClass3ImplementingInterface2");
            Interface1 = interface1;
        }
    }

    public class TestClass4ImplementingInterface2 : ITestInterface2
    {
        public ITestInterface2 Interface2 { get; private set; }

        public TestClass4ImplementingInterface2(ITestInterface2 interface2)
        {
            CountCall.Increment("TestClass4ImplementingInterface2");
            Interface2 = interface2;
        }
    }

    public class TestClass5ImplementingInterface2 : ITestInterface2
    {
        public ITestInterface3 Interface3 { get; private set; }

        public TestClass5ImplementingInterface2(ITestInterface3 interface3)
        {
            CountCall.Increment("TestClass5ImplementingInterface2");
            Interface3 = interface3;
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
        public ITestInterface1 Interface1 { get; private set; }
        public ITestInterface2 Interface2 { get; private set; }
        public ITestInterface3 Interface3 { get; private set; }

        public TestClass1ImplementingInterface3(ITestInterface3 interface3)
        {
            CountCall.Increment("TestClass1ImplementingInterface3");
            Interface3 = interface3;
        }

        public TestClass1ImplementingInterface3(ITestInterface2 interface2)
        {
            CountCall.Increment("TestClass1ImplementingInterface3");
            Interface2 = interface2;
        }

        public TestClass1ImplementingInterface3(ITestInterface1 interface1)
        {
            CountCall.Increment("TestClass1ImplementingInterface3");
            Interface1 = interface1;
        }
    }

    public class TestClass2ImplementingInterface3 : ITestInterface3
    {
        public ITestInterface1 Interface1 { get; private set; }
        public ITestInterface2 Interface2 { get; private set; }

        public TestClass2ImplementingInterface3(ITestInterface1 interface1, ITestInterface2 interface2)
        {
            CountCall.Increment("TestClass2ImplementingInterface3");
            Interface1 = interface1;
            Interface2 = interface2;
        }
    }
}
