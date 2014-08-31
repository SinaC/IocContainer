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
        public void TestReset()
        {
            IocContainer.Default.Reset();
            IocContainer.Default.Register<ITestInterface1, TestClass1ImplementingInterface1>();
            IocContainer.Default.Register<ITestInterface2, TestClass1ImplementingInterface2>();

            IocContainer.Default.Reset();
            try
            {
                IocContainer.Default.Resolve<ITestInterface1>();

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentException)
            {
            }

            try
            {
                IocContainer.Default.Resolve<ITestInterface2>();

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentException)
            {
            }
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
        public void TestRegister2DifferentRegistrationsOnSameInterface()
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
        public void TestRegister2IdenticalRegistrationsOnSameInterface()
        {
            IocContainer.Default.Reset();
            IocContainer.Default.Register<ITestInterface1, TestClass1ImplementingInterface1>();
            IocContainer.Default.Register<ITestInterface1, TestClass1ImplementingInterface1>();

            ITestInterface1 instance = IocContainer.Default.Resolve<ITestInterface1>();

            Assert.IsNotNull(instance);
            Assert.IsInstanceOfType(instance, typeof(TestClass1ImplementingInterface1));
        }

        [TestMethod]
        public void TestRegisterOnNonInterface()
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
        public void TestRegisterOnAbstractClass()
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
        public void TestRegisterInterfaceAsImplementation()
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
        public void TestRegisterNotAssignable()
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

        [TestMethod]
        public void TestResolveSingleInstanceIfSingleton()
        {
            IocContainer.Default.Reset();
            IocContainer.Default.Register<ITestInterface1, TestClass1ImplementingInterface1>();

            ITestInterface1 instance1 = IocContainer.Default.Resolve<ITestInterface1>();
            ITestInterface1 instance2 = IocContainer.Default.Resolve<ITestInterface1>();

            Assert.AreEqual(instance1, instance2);
        }

        [TestMethod]
        public void TestResolveMultipleInstanceIfNewInstance()
        {
            IocContainer.Default.Reset();
            IocContainer.Default.Register<ITestInterface1, TestClass1ImplementingInterface1>();

            ITestInterface1 instance1 = IocContainer.Default.Resolve<ITestInterface1>(ResolveMethods.NewInstance);
            ITestInterface1 instance2 = IocContainer.Default.Resolve<ITestInterface1>(ResolveMethods.NewInstance);

            Assert.AreNotEqual(instance1, instance2);
        }

        [TestMethod]
        public void TestResolveReturnsRegisteredInstance()
        {
            IocContainer.Default.Reset();
            IocContainer.Default.Register<ITestInterface1, TestClass1ImplementingInterface1>();
            ITestInterface1 instance0 = new TestClass1ImplementingInterface1();
            IocContainer.Default.Register(instance0);

            ITestInterface1 instance1 = IocContainer.Default.Resolve<ITestInterface1>();
            ITestInterface1 instance2 = IocContainer.Default.Resolve<ITestInterface1>();

            Assert.AreEqual(instance0, instance1);
            Assert.AreEqual(instance1, instance2);
        }

        [TestMethod]
        public void TestUnregisterExceptionOnNonRegisteredInterface()
        {
            IocContainer.Default.Reset();
            IocContainer.Default.Register<ITestInterface1, TestClass1ImplementingInterface1>();

            try
            {
                IocContainer.Default.Unregister<ITestInterface2>();

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentException)
            {
            }
        }

        [TestMethod]
        public void TestUnregisterNoExceptionOnRegisteredInterface()
        {
            IocContainer.Default.Reset();
            IocContainer.Default.Register<ITestInterface1, TestClass1ImplementingInterface1>();

            IocContainer.Default.Unregister<ITestInterface1>();
        }

        [TestMethod]
        public void TestUnregisterNoRegistrationFound()
        {
            IocContainer.Default.Reset();
            IocContainer.Default.Register<ITestInterface1, TestClass1ImplementingInterface1>();

            IocContainer.Default.Unregister<ITestInterface1>();
            try
            {
                IocContainer.Default.Resolve<ITestInterface1>();

                Assert.Fail("No exception thrown");
            }
            catch (ArgumentException)
            {
            }
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
