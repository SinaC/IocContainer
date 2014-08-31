using System;
using EasyIoc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EasyIocTest
{
    [TestClass]
    public class IocContainerFirstVersionTest
    {
        [TestMethod]
        public void TestDefaultNonNull()
        {
            IIocContainerFirstVersion container = IocContainerFirstVersion.Default;

            Assert.IsNotNull(container);
        }

        [TestMethod]
        public void TestDefaultUniqueness()
        {
            IIocContainerFirstVersion container1 = IocContainerFirstVersion.Default;
            IIocContainerFirstVersion container2 = IocContainerFirstVersion.Default;

            Assert.AreSame(container1, container2);
        }

        [TestMethod]
        public void TestReset()
        {
            IocContainerFirstVersion.Default.Reset();
            IocContainerFirstVersion.Default.Register<ITestInterface1, TestClass1ImplementingInterface1>();
            IocContainerFirstVersion.Default.Register<ITestInterface2, TestClass1ImplementingInterface2>();

            IocContainerFirstVersion.Default.Reset();
            try
            {
                IocContainerFirstVersion.Default.Resolve<ITestInterface1>();

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentException)
            {
            }

            try
            {
                IocContainerFirstVersion.Default.Resolve<ITestInterface2>();

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentException)
            {
            }
        }

        [TestMethod]
        public void TestSuccessfulRegistrationNoCreator()
        {
            IocContainerFirstVersion.Default.Reset();
            IocContainerFirstVersion.Default.Register<ITestInterface1, TestClass1ImplementingInterface1>();

            ITestInterface1 instance = IocContainerFirstVersion.Default.Resolve<ITestInterface1>();
            
            Assert.IsNotNull(instance);
            Assert.IsInstanceOfType(instance, typeof(TestClass1ImplementingInterface1));
        }

        [TestMethod]
        public void TestSuccessfulRegistrationCreator()
        {
            IocContainerFirstVersion.Default.Reset();
            IocContainerFirstVersion.Default.Register<ITestInterface1>(() => new TestClass1ImplementingInterface1());

            ITestInterface1 instance = IocContainerFirstVersion.Default.Resolve<ITestInterface1>();

            Assert.IsNotNull(instance);
            Assert.IsInstanceOfType(instance, typeof(TestClass1ImplementingInterface1));
        }

        [TestMethod]
        public void TestRegister2DifferentRegistrationsOnSameInterface()
        {
            IocContainerFirstVersion.Default.Reset();
            IocContainerFirstVersion.Default.Register<ITestInterface1, TestClass1ImplementingInterface1>();

            try
            {
                IocContainerFirstVersion.Default.Register<ITestInterface1, TestClass2ImplementingInterface1>();

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
            IocContainerFirstVersion.Default.Reset();
            IocContainerFirstVersion.Default.Register<ITestInterface1, TestClass1ImplementingInterface1>();
            IocContainerFirstVersion.Default.Register<ITestInterface1, TestClass1ImplementingInterface1>();

            ITestInterface1 instance = IocContainerFirstVersion.Default.Resolve<ITestInterface1>();

            Assert.IsNotNull(instance);
            Assert.IsInstanceOfType(instance, typeof(TestClass1ImplementingInterface1));
        }

        [TestMethod]
        public void TestRegisterOnNonInterface()
        {
            IocContainerFirstVersion.Default.Reset();
            try
            {
                IocContainerFirstVersion.Default.Register<TestClass1ImplementingInterface1, TestClass1ImplementingInterface1>();

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
            IocContainerFirstVersion.Default.Reset();
            try
            {
                IocContainerFirstVersion.Default.Register<ITestInterface1, TestAbstractClassImplentatingInterface1>();

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
            IocContainerFirstVersion.Default.Reset();
            try
            {
                IocContainerFirstVersion.Default.Register<ITestInterface1, ITestInterface1>();

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
            IocContainerFirstVersion.Default.Reset();
            try
            {
                IocContainerFirstVersion.Default.Register<ITestInterface1, TestClass1ImplementingInterface2>();

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
            IocContainerFirstVersion.Default.Reset();
            IocContainerFirstVersion.Default.Register<ITestInterface1, TestClass1ImplementingInterface1>();

            bool isRegistered = IocContainerFirstVersion.Default.IsRegistered<ITestInterface2>();

            Assert.IsFalse(isRegistered);
        }

        [TestMethod]
        public void TestIsRegisteredOnRegisteredInterface()
        {
            IocContainerFirstVersion.Default.Reset();
            IocContainerFirstVersion.Default.Register<ITestInterface1, TestClass1ImplementingInterface1>();

            bool isRegistered = IocContainerFirstVersion.Default.IsRegistered<ITestInterface1>();

            Assert.IsTrue(isRegistered);
        }

        [TestMethod]
        public void TestResolveSingleInstanceIfSingleton()
        {
            IocContainerFirstVersion.Default.Reset();
            IocContainerFirstVersion.Default.Register<ITestInterface1, TestClass1ImplementingInterface1>();

            ITestInterface1 instance1 = IocContainerFirstVersion.Default.Resolve<ITestInterface1>();
            ITestInterface1 instance2 = IocContainerFirstVersion.Default.Resolve<ITestInterface1>();

            Assert.AreEqual(instance1, instance2);
        }

        [TestMethod]
        public void TestResolveMultipleInstanceIfNewInstance()
        {
            IocContainerFirstVersion.Default.Reset();
            IocContainerFirstVersion.Default.Register<ITestInterface1, TestClass1ImplementingInterface1>();

            ITestInterface1 instance1 = IocContainerFirstVersion.Default.Resolve<ITestInterface1>(ResolveMethods.NewInstance);
            ITestInterface1 instance2 = IocContainerFirstVersion.Default.Resolve<ITestInterface1>(ResolveMethods.NewInstance);

            Assert.AreNotEqual(instance1, instance2);
        }

        [TestMethod]
        public void TestResolveReturnsRegisteredInstance()
        {
            IocContainerFirstVersion.Default.Reset();
            IocContainerFirstVersion.Default.Register<ITestInterface1, TestClass1ImplementingInterface1>();
            ITestInterface1 instance0 = new TestClass1ImplementingInterface1();
            IocContainerFirstVersion.Default.Register(instance0);

            ITestInterface1 instance1 = IocContainerFirstVersion.Default.Resolve<ITestInterface1>();
            ITestInterface1 instance2 = IocContainerFirstVersion.Default.Resolve<ITestInterface1>();

            Assert.AreEqual(instance0, instance1);
            Assert.AreEqual(instance1, instance2);
        }

        [TestMethod]
        public void TestUnregisterExceptionOnNonRegisteredInterface()
        {
            IocContainerFirstVersion.Default.Reset();
            IocContainerFirstVersion.Default.Register<ITestInterface1, TestClass1ImplementingInterface1>();

            try
            {
                IocContainerFirstVersion.Default.Unregister<ITestInterface2>();

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentException)
            {
            }
        }

        [TestMethod]
        public void TestUnregisterNoExceptionOnRegisteredInterface()
        {
            IocContainerFirstVersion.Default.Reset();
            IocContainerFirstVersion.Default.Register<ITestInterface1, TestClass1ImplementingInterface1>();

            IocContainerFirstVersion.Default.Unregister<ITestInterface1>();
        }

        [TestMethod]
        public void TestUnregisterNoRegistrationFound()
        {
            IocContainerFirstVersion.Default.Reset();
            IocContainerFirstVersion.Default.Register<ITestInterface1, TestClass1ImplementingInterface1>();

            IocContainerFirstVersion.Default.Unregister<ITestInterface1>();
            try
            {
                IocContainerFirstVersion.Default.Resolve<ITestInterface1>();

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
