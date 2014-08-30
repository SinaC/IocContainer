using System;
using EasyIoc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EasyIocTest
{
    [TestClass]
    public class IocContainerTest
    {
        [TestMethod]
        public void TestDefaultNonNull()
        {
            IIocContainer container = IocContainer.Default;

            Assert.IsNotNull(container);
        }

        [TestMethod]
        public void TestDefaultUniqueness()
        {
            IIocContainer container1 = IocContainer.Default;
            IIocContainer container2 = IocContainer.Default;

            Assert.AreSame(container1, container2);
        }

        [TestMethod]
        public void TestSuccessfulRegistrationNoCreator()
        {
            IocContainer.Default.Reset();
            IocContainer.Default.Register<ITestInterface1, TestClass1ImplementingInterface1>();

            ITestInterface1 instance = IocContainer.Default.Resolve<ITestInterface1>();
            
            Assert.IsNotNull(instance);
            Assert.IsInstanceOfType(instance, typeof(TestClass1ImplementingInterface1));
        }

        [TestMethod]
        public void TestSuccessfulRegistrationCreator()
        {
            IocContainer.Default.Reset();
            IocContainer.Default.Register<ITestInterface1>(() => new TestClass1ImplementingInterface1());

            ITestInterface1 instance = IocContainer.Default.Resolve<ITestInterface1>();

            Assert.IsNotNull(instance);
            Assert.IsInstanceOfType(instance, typeof(TestClass1ImplementingInterface1));
        }

        [TestMethod]
        public void Test2DifferentRegistrationsOnSameInterface()
        {
            IocContainer.Default.Reset();
            IocContainer.Default.Register<ITestInterface1, TestClass1ImplementingInterface1>();

            try
            {
                IocContainer.Default.Register<ITestInterface1, TestClass2ImplementingInterface1>();

                Assert.Fail("InvalidOperationException not raised");
            }
            catch (InvalidOperationException)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void Test2IdenticalRegistrationsOnSameInterface()
        {
            IocContainer.Default.Reset();
            IocContainer.Default.Register<ITestInterface1, TestClass1ImplementingInterface1>();
            IocContainer.Default.Register<ITestInterface1, TestClass1ImplementingInterface1>();

            ITestInterface1 instance = IocContainer.Default.Resolve<ITestInterface1>();

            Assert.IsNotNull(instance);
            Assert.IsInstanceOfType(instance, typeof(TestClass1ImplementingInterface1));
        }

        [TestMethod]
        public void TestRegistrationOnNonInterface()
        {
            IocContainer.Default.Reset();
            try
            {
                IocContainer.Default.Register<TestClass1ImplementingInterface1, TestClass1ImplementingInterface1>();

                Assert.Fail("ArgumentException not raised");
            }
            catch (ArgumentException)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void TestRegistrationOnAbstractClass()
        {
            IocContainer.Default.Reset();
            try
            {
                IocContainer.Default.Register<ITestInterface1, TestAbstractClassImplentatingInterface1>();

                Assert.Fail("ArgumentException not raised");
            }
            catch (ArgumentException)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void TestRegistrationInterfaceAsImplementation()
        {
            IocContainer.Default.Reset();
            try
            {
                IocContainer.Default.Register<ITestInterface1, ITestInterface1>();

                Assert.Fail("ArgumentException not raised");
            }
            catch (ArgumentException)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void TestRegistrationNotAssignable()
        {
            IocContainer.Default.Reset();
            try
            {
                IocContainer.Default.Register<ITestInterface1, TestClass1ImplementingInterface2>();

                Assert.Fail("ArgumentException not raised");
            }
            catch (ArgumentException)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void TestIsRegisteredOnNonRegisteredInterface()
        {
            IocContainer.Default.Reset();
            IocContainer.Default.Register<ITestInterface1, TestClass1ImplementingInterface1>();

            bool isRegistered = IocContainer.Default.IsRegistered<ITestInterface2>();

            Assert.IsFalse(isRegistered);
        }

        [TestMethod]
        public void TestIsRegisteredOnRegisteredInterface()
        {
            IocContainer.Default.Reset();
            IocContainer.Default.Register<ITestInterface1, TestClass1ImplementingInterface1>();

            bool isRegistered = IocContainer.Default.IsRegistered<ITestInterface1>();

            Assert.IsTrue(isRegistered);
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
