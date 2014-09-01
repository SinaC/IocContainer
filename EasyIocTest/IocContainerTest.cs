using System;
using EasyIoc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EasyIocTest
{
    [TestClass]
    public class IocContainerTest
    {
        [TestInitialize]
        public void Initialize()
        {
            IocContainer.Default.Reset();
        }

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
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();
            IocContainer.Default.RegisterType<ITestInterface2, TestClass1ImplementingInterface2>();
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
            
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();

            ITestInterface1 instance = IocContainer.Default.Resolve<ITestInterface1>();
            
            Assert.IsNotNull(instance);
            Assert.IsInstanceOfType(instance, typeof(TestClass1ImplementingInterface1));
        }

        [TestMethod]
        public void TestSuccessfulRegistrationFactory()
        {
            
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();
            IocContainer.Default.RegisterFactory<ITestInterface1>(() => new TestClass1ImplementingInterface1());

            ITestInterface1 instance = IocContainer.Default.Resolve<ITestInterface1>();

            Assert.IsNotNull(instance);
            Assert.IsInstanceOfType(instance, typeof(TestClass1ImplementingInterface1));
        }

        [TestMethod]
        public void TestRegisterFactoryWithoutRegisterType()
        {
            
            IocContainer.Default.RegisterFactory<ITestInterface1>(() => new TestClass1ImplementingInterface1());

            try
            {
                ITestInterface1 instance = IocContainer.Default.Resolve<ITestInterface1>();

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentException ex)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void TestRegisterInstanceWithoutRegisterType()
        {
            
            IocContainer.Default.RegisterInstance<ITestInterface1>(new TestClass1ImplementingInterface1());

            try
            {
                ITestInterface1 instance = IocContainer.Default.Resolve<ITestInterface1>();

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentException ex)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void TestRegister2DifferentRegistrationsOnSameInterface()
        {
            
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();

            try
            {
                IocContainer.Default.RegisterType<ITestInterface1, TestClass2ImplementingInterface1>();

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
            
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();

            ITestInterface1 instance = IocContainer.Default.Resolve<ITestInterface1>();

            Assert.IsNotNull(instance);
            Assert.IsInstanceOfType(instance, typeof(TestClass1ImplementingInterface1));
        }

        [TestMethod]
        public void TestRegisterOnNonInterface()
        {
            
            try
            {
                IocContainer.Default.RegisterType<TestClass1ImplementingInterface1, TestClass1ImplementingInterface1>();

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
            
            try
            {
                IocContainer.Default.RegisterType<ITestInterface1, TestAbstractClassImplentatingInterface1>();

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
            
            try
            {
                IocContainer.Default.RegisterType<ITestInterface1, ITestInterface1>();

                Assert.Fail("ArgumentException not raised");
            }
            catch (ArgumentException)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void TestRegisterNotAssignableFrom()
        {
            
            try
            {
                IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface2>();

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
            
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();

            bool isRegistered = IocContainer.Default.IsRegistered<ITestInterface2>();

            Assert.IsFalse(isRegistered);
        }

        [TestMethod]
        public void TestIsRegisteredOnRegisteredInterface()
        {
            
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();

            bool isRegistered = IocContainer.Default.IsRegistered<ITestInterface1>();

            Assert.IsTrue(isRegistered);
        }

        [TestMethod]
        public void TestResolveSingleInstanceOnRegisterInstance()
        {
            
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();
            IocContainer.Default.RegisterInstance<ITestInterface1>(new TestClass1ImplementingInterface1());

            ITestInterface1 instance1 = IocContainer.Default.Resolve<ITestInterface1>();
            ITestInterface1 instance2 = IocContainer.Default.Resolve<ITestInterface1>();

            Assert.AreEqual(instance1, instance2);
        }

        [TestMethod]
        public void TestResolveMultipleInstanceOnRegisterType()
        {
            
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();

            ITestInterface1 instance1 = IocContainer.Default.Resolve<ITestInterface1>();
            ITestInterface1 instance2 = IocContainer.Default.Resolve<ITestInterface1>();

            Assert.AreNotEqual(instance1, instance2);
        }

        [TestMethod]
        public void TestResolveReturnsRegisteredInstance()
        {
            
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();
            ITestInterface1 instance0 = new TestClass1ImplementingInterface1();
            IocContainer.Default.RegisterInstance(instance0);

            ITestInterface1 instance1 = IocContainer.Default.Resolve<ITestInterface1>();
            ITestInterface1 instance2 = IocContainer.Default.Resolve<ITestInterface1>();

            Assert.AreEqual(instance0, instance1);
            Assert.AreEqual(instance1, instance2);
        }

        [TestMethod]
        public void TestResolveWithoutPublicConstructor()
        {
            
            IocContainer.Default.RegisterType<ITestInterface1, TestClass4ImplementingInterface1>();

            try
            {
                ITestInterface1 instance = IocContainer.Default.Resolve<ITestInterface1>();

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentException)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void TestResolveWithComplexConstructor()
        {
            
            IocContainer.Default.RegisterType<ITestInterface1, TestClass3ImplementingInterface1>();

            try
            {
                ITestInterface1 instance = IocContainer.Default.Resolve<ITestInterface1>();

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentException)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void TestResolveWithRegisterInstanceAndComplexConstructor()
        {
            
            IocContainer.Default.RegisterType<ITestInterface1, TestClass3ImplementingInterface1>();
            IocContainer.Default.RegisterInstance<ITestInterface1>(new TestClass3ImplementingInterface1(5));

            ITestInterface1 instance = IocContainer.Default.Resolve<ITestInterface1>();

            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void TestResolveWithRegisterInstanceAndNoPublicConstructor()
        {
            
            IocContainer.Default.RegisterType<ITestInterface1, TestClass4ImplementingInterface1>();
            IocContainer.Default.RegisterInstance<ITestInterface1>(new TestClass5ImplementingInterface1());

            ITestInterface1 instance = IocContainer.Default.Resolve<ITestInterface1>();

            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void TestResolveSimpleCyclicDependency()
        {
            IocContainer.Default.RegisterType<ITestInterface2, TestClass4ImplementingInterface2>();

            try
            {
                ITestInterface2 instance = IocContainer.Default.Resolve<ITestInterface2>();

                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentException ex)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void TestResolveComplexCyclicDependency()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass6ImplementingInterface1>();
            IocContainer.Default.RegisterType<ITestInterface2, TestClass3ImplementingInterface2>();

            try
            {
                ITestInterface1 instance = IocContainer.Default.Resolve<ITestInterface1>();

                Assert.Fail("Exception not thrown");
            }
            catch(ArgumentException ex)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void TestUnregisterExceptionOnNonRegisteredInterface()
        {
            
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();

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
            
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();

            IocContainer.Default.Unregister<ITestInterface1>();
        }

        [TestMethod]
        public void TestIfUnregisterRemoveInterface()
        {
            
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();

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
}
