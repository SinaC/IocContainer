using System;
using EasyIoc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EasyIocTest
{
    [TestClass]
    public class IocContainerTests
    {
        [TestInitialize]
        public void Initialize()
        {
            IocContainer.Default.Reset();
            CountCall.Reset();
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
        public void TestDefaultDifferentThanResolved()
        {
            IocContainer.Default.RegisterType<IIocContainer, IocContainer>();

            IIocContainer container = IocContainer.Default.Resolve<IIocContainer>();

            Assert.AreNotSame(container, IocContainer.Default);
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
        public void TestSuccessfulRegistrationNoFactoryNeitherInstance()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();

            ITestInterface1 instance = IocContainer.Default.Resolve<ITestInterface1>();

            Assert.IsNotNull(instance);
            Assert.IsInstanceOfType(instance, typeof (TestClass1ImplementingInterface1));
        }

        [TestMethod]
        public void TestSuccessfulRegistrationFactory()
        {
            IocContainer.Default.RegisterFactory<ITestInterface1>(() => new TestClass1ImplementingInterface1());

            ITestInterface1 instance = IocContainer.Default.Resolve<ITestInterface1>();

            Assert.IsNotNull(instance);
            Assert.IsInstanceOfType(instance, typeof (TestClass1ImplementingInterface1));
        }

        [TestMethod]
        public void TestRegisterRegisterTypeAndRegisterInstance()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();

            try
            {
                IocContainer.Default.RegisterInstance<ITestInterface1>(new TestClass1ImplementingInterface1());

                Assert.Fail("InvalidOperationException not raised");
            }
            catch (ArgumentException)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void TestRegisterRegisterInstanceAndRegisterType()
        {
            IocContainer.Default.RegisterInstance<ITestInterface1>(new TestClass1ImplementingInterface1());

            try
            {
                IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();

                Assert.Fail("InvalidOperationException not raised");
            }
            catch (ArgumentException)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void TestRegisterRegisterTypeAndRegisterFactory()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();

            try
            {
                IocContainer.Default.RegisterFactory<ITestInterface1>(() => new TestClass1ImplementingInterface1());

                Assert.Fail("InvalidOperationException not raised");
            }
            catch (ArgumentException)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void TestRegisterRegisterFactoryAndRegisterType()
        {
            IocContainer.Default.RegisterFactory<ITestInterface1>(() => new TestClass1ImplementingInterface1());

            try
            {
                IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();

                Assert.Fail("InvalidOperationException not raised");
            }
            catch (ArgumentException)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void TestRegisterRegisterInstanceAndRegisterFactory()
        {
            IocContainer.Default.RegisterInstance<ITestInterface1>(new TestClass1ImplementingInterface1());

            try
            {
                IocContainer.Default.RegisterFactory<ITestInterface1>(() => new TestClass1ImplementingInterface1());

                Assert.Fail("InvalidOperationException not raised");
            }
            catch (ArgumentException)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void TestRegisterRegisterFactoryAndRegisterInstance()
        {
            IocContainer.Default.RegisterFactory<ITestInterface1>(() => new TestClass1ImplementingInterface1());

            try
            {
                IocContainer.Default.RegisterInstance<ITestInterface1>(new TestClass1ImplementingInterface1());

                Assert.Fail("InvalidOperationException not raised");
            }
            catch (ArgumentException)
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
            catch (ArgumentException)
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
            Assert.IsInstanceOfType(instance, typeof (TestClass1ImplementingInterface1));
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
        public void TestIsRegisteredOnRegisteredFactory()
        {
            IocContainer.Default.RegisterFactory<ITestInterface1>(() => new TestClass1ImplementingInterface1());

            bool isRegistered = IocContainer.Default.IsRegistered<ITestInterface1>();

            Assert.IsTrue(isRegistered);
        }

        [TestMethod]
        public void TestIsRegisteredOnRegisteredInstance()
        {
            IocContainer.Default.RegisterInstance<ITestInterface1>(new TestClass1ImplementingInterface1());

            bool isRegistered = IocContainer.Default.IsRegistered<ITestInterface1>();

            Assert.IsTrue(isRegistered);
        }

        [TestMethod]
        public void TestUnregister()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();

            IocContainer.Default.Unregister<ITestInterface1>();
            try
            {
                IocContainer.Default.Resolve<ITestInterface1>();

                Assert.Fail("Exception not raised");
            }
            catch (ArgumentException)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void TestUnregisterNoExceptionOnUnknownInterface()
        {
            IocContainer.Default.Unregister<ITestInterface1>();
        }

        [TestMethod]
        public void TestUnregisterType()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();

            IocContainer.Default.UnregisterType<ITestInterface1>();
            try
            {
                IocContainer.Default.Resolve<ITestInterface1>();

                Assert.Fail("Exception not raised");
            }
            catch (ArgumentException)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void TestUnregisterInstance()
        {
            IocContainer.Default.RegisterInstance<ITestInterface1>(new TestClass1ImplementingInterface1());

            IocContainer.Default.UnregisterInstance<ITestInterface1>();
            try
            {
                IocContainer.Default.Resolve<ITestInterface1>();

                Assert.Fail("Exception not raised");
            }
            catch (ArgumentException)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void TestUnregisterFactory()
        {
            IocContainer.Default.RegisterFactory<ITestInterface1>(() => new TestClass1ImplementingInterface1());

            IocContainer.Default.UnregisterFactory<ITestInterface1>();
            try
            {
                IocContainer.Default.Resolve<ITestInterface1>();

                Assert.Fail("Exception not raised");
            }
            catch (ArgumentException)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void TestResolveSingleInstanceOnRegisterInstance()
        {
            IocContainer.Default.RegisterInstance<ITestInterface1>(new TestClass7ImplementingInterface1());

            ITestInterface1 instance1 = IocContainer.Default.Resolve<ITestInterface1>();
            ITestInterface1 instance2 = IocContainer.Default.Resolve<ITestInterface1>();

            Assert.AreEqual(instance1, instance2);
            Assert.AreEqual(CountCall.Count("TestClass7ImplementingInterface1"), 1);
        }

        [TestMethod]
        public void TestResolveMultipleInstanceOnRegisterType()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass7ImplementingInterface1>();

            ITestInterface1 instance1 = IocContainer.Default.Resolve<ITestInterface1>();
            ITestInterface1 instance2 = IocContainer.Default.Resolve<ITestInterface1>();

            Assert.AreNotEqual(instance1, instance2);
            Assert.AreEqual(CountCall.Count("TestClass7ImplementingInterface1"), 2);
        }

        [TestMethod]
        public void TestResolveReturnsRegisteredInstance()
        {
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
            IocContainer.Default.RegisterInstance<ITestInterface1>(new TestClass3ImplementingInterface1(5));

            ITestInterface1 instance = IocContainer.Default.Resolve<ITestInterface1>();

            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void TestResolveWithRegisterInstanceAndNoPublicConstructor()
        {
            IocContainer.Default.RegisterInstance<ITestInterface1>(new TestClass5ImplementingInterface1());

            ITestInterface1 instance = IocContainer.Default.Resolve<ITestInterface1>();

            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void TestResolveManyConstructors()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass5ImplementingInterface1>();
            IocContainer.Default.RegisterType<ITestInterface2, TestClass5ImplementingInterface2>();
            IocContainer.Default.RegisterType<ITestInterface3, TestClass1ImplementingInterface3>();

            ITestInterface3 instance = IocContainer.Default.Resolve<ITestInterface3>(); // Will resolve using ctor TestClass1ImplementingInterface3(ITestInterface1 interface1)

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
            catch (ArgumentException ex)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void TestResolveReallyComplexCyclicDependency()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass5ImplementingInterface1>();
            IocContainer.Default.RegisterType<ITestInterface2, TestClass5ImplementingInterface2>();
            IocContainer.Default.RegisterType<ITestInterface3, TestClass2ImplementingInterface3>();

            try
            {
                ITestInterface3 instance = IocContainer.Default.Resolve<ITestInterface3>();

                Assert.IsNotNull(instance);
                Assert.Fail("Exception not thrown");
            }
            catch (ArgumentException ex)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void TestResolveWithInvalidUserDefinedParameters()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass3ImplementingInterface1>();

            try
            {
                ITestInterface1 instance = IocContainer.Default.Resolve<ITestInterface1>(new[]
                    {
                        new ParameterValue("y", 5)
                    });
                Assert.Fail("No exception thrown");
            }
            catch (ArgumentException)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void TestResolveWithUserDefinedParameters()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass3ImplementingInterface1>();

            ITestInterface1 instance = IocContainer.Default.Resolve<ITestInterface1>(new[]
                {
                    new ParameterValue("x", 5)
                });

            Assert.IsInstanceOfType(instance, typeof (TestClass3ImplementingInterface1));
            Assert.AreEqual((instance as TestClass3ImplementingInterface1).X, 5);
        }

        [TestMethod]
        public void TestResolveResolveTreeNotSavedOnError()
        {
            IocContainer.Default.RegisterType<ITestInterface2, TestClass3ImplementingInterface2>();

            ITestInterface2 instance;
            try
            {
                instance = IocContainer.Default.Resolve<ITestInterface2>(); // resolve will fail because ITestInterface1 parameter of TestClass3ImplementingInterface2 ctor cannot be resolved
                
                Assert.Fail("Exception not raised");
            }
            catch (ArgumentException)
            {
            }
            IocContainer.Default.RegisterFactory<ITestInterface1>(() => new TestClass2ImplementingInterface1());
            instance = IocContainer.Default.Resolve<ITestInterface2>();

            Assert.IsInstanceOfType(instance, typeof(TestClass3ImplementingInterface2));
            Assert.IsInstanceOfType((instance as TestClass3ImplementingInterface2).Interface1, typeof(TestClass2ImplementingInterface1));
        }

        [TestMethod]
        public void TestResolveWithUserDefinedParameters2()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass3ImplementingInterface1>();

            ITestInterface1 instance5 = IocContainer.Default.Resolve<ITestInterface1>(new[]
                {
                    new ParameterValue("x", 5)
                });
            ITestInterface1 instance3 = IocContainer.Default.Resolve<ITestInterface1>(new[]
                {
                    new ParameterValue("x", 3)
                });

            Assert.IsInstanceOfType(instance5, typeof(TestClass3ImplementingInterface1));
            Assert.AreEqual((instance5 as TestClass3ImplementingInterface1).X, 5);
            Assert.IsInstanceOfType(instance3, typeof(TestClass3ImplementingInterface1));
            Assert.AreEqual((instance3 as TestClass3ImplementingInterface1).X, 3);
        }

        [TestMethod]
        public void TestRegisterDeeperInheriting()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass5ImplementingInterface1>();

            ITestInterface1 instance = IocContainer.Default.Resolve<ITestInterface1>();

            Assert.IsNotNull(instance);
            Assert.IsInstanceOfType(instance, typeof(TestClass5ImplementingInterface1));
        }

        [TestMethod]
        public void TestResolveScenario1()
        {
            IocContainer.Default.RegisterFactory<ITestInterface1>(() => new TestClass2ImplementingInterface1());
            IocContainer.Default.RegisterType<ITestInterface2, TestClass3ImplementingInterface2>();

            ITestInterface2 instance = IocContainer.Default.Resolve<ITestInterface2>();

            Assert.IsNotNull(instance);
            Assert.IsInstanceOfType(instance, typeof (TestClass3ImplementingInterface2));
            Assert.IsNotNull((instance as TestClass3ImplementingInterface2).Interface1);
            Assert.IsInstanceOfType((instance as TestClass3ImplementingInterface2).Interface1, typeof (TestClass2ImplementingInterface1));
        }

        [TestMethod]
        public void TestResolveScenario2()
        {
            IocContainer.Default.RegisterType<ITestInterface3, TestClass1ImplementingInterface3>();
            IocContainer.Default.RegisterType<ITestInterface2, TestClass3ImplementingInterface2>();
            IocContainer.Default.RegisterInstance<ITestInterface1>(new TestClass3ImplementingInterface1(7));

            ITestInterface3 instance = IocContainer.Default.Resolve<ITestInterface3>();

            Assert.IsNotNull(instance);
            Assert.IsInstanceOfType(instance, typeof (TestClass1ImplementingInterface3));
            Assert.IsNull((instance as TestClass1ImplementingInterface3).Interface1);
            Assert.IsNotNull((instance as TestClass1ImplementingInterface3).Interface2);
            Assert.IsNull((instance as TestClass1ImplementingInterface3).Interface3);
            Assert.IsInstanceOfType((instance as TestClass1ImplementingInterface3).Interface2, typeof (TestClass3ImplementingInterface2));
            Assert.IsNotNull(((instance as TestClass1ImplementingInterface3).Interface2 as TestClass3ImplementingInterface2).Interface1);
            Assert.IsInstanceOfType(((instance as TestClass1ImplementingInterface3).Interface2 as TestClass3ImplementingInterface2).Interface1, typeof (TestClass3ImplementingInterface1));
            Assert.AreEqual((((instance as TestClass1ImplementingInterface3).Interface2 as TestClass3ImplementingInterface2).Interface1 as TestClass3ImplementingInterface1).X, 7);
            Assert.AreEqual(CountCall.Count("TestClass3ImplementingInterface1"), 1);
        }

        [TestMethod]
        public void TestResolveScenario3()
        {
            IocContainer.Default.RegisterType<ITestInterface3, TestClass1ImplementingInterface3>();
            IocContainer.Default.RegisterType<ITestInterface2, TestClass3ImplementingInterface2>();
            IocContainer.Default.RegisterType<ITestInterface1, TestClass3ImplementingInterface1>();

            ITestInterface3 instance = IocContainer.Default.Resolve<ITestInterface3>(new[]
                {
                    new ParameterValue("x", 9)
                });

            Assert.IsNotNull(instance);
            Assert.IsInstanceOfType(instance, typeof(TestClass1ImplementingInterface3));
            Assert.IsNull((instance as TestClass1ImplementingInterface3).Interface1);
            Assert.IsNotNull((instance as TestClass1ImplementingInterface3).Interface2);
            Assert.IsNull((instance as TestClass1ImplementingInterface3).Interface3);
            Assert.IsInstanceOfType((instance as TestClass1ImplementingInterface3).Interface2, typeof(TestClass3ImplementingInterface2));
            Assert.IsNotNull(((instance as TestClass1ImplementingInterface3).Interface2 as TestClass3ImplementingInterface2).Interface1);
            Assert.IsInstanceOfType(((instance as TestClass1ImplementingInterface3).Interface2 as TestClass3ImplementingInterface2).Interface1, typeof(TestClass3ImplementingInterface1));
            Assert.AreEqual((((instance as TestClass1ImplementingInterface3).Interface2 as TestClass3ImplementingInterface2).Interface1 as TestClass3ImplementingInterface1).X, 9);
        }

        [TestMethod]
        public void TestResolveScenario4()
        {
            IocContainer.Default.RegisterType<ITestInterface3, TestClass1ImplementingInterface3>();
            IocContainer.Default.RegisterType<ITestInterface2, TestClass3ImplementingInterface2>();
            IocContainer.Default.RegisterFactory<ITestInterface1>(() => new TestClass7ImplementingInterface1());

            ITestInterface3 instance = IocContainer.Default.Resolve<ITestInterface3>();

            Assert.IsNotNull(instance);
            Assert.IsInstanceOfType(instance, typeof(TestClass1ImplementingInterface3));
            Assert.IsNull((instance as TestClass1ImplementingInterface3).Interface1);
            Assert.IsNotNull((instance as TestClass1ImplementingInterface3).Interface2);
            Assert.IsNull((instance as TestClass1ImplementingInterface3).Interface3);
            Assert.IsInstanceOfType((instance as TestClass1ImplementingInterface3).Interface2, typeof(TestClass3ImplementingInterface2));
            Assert.IsNotNull(((instance as TestClass1ImplementingInterface3).Interface2 as TestClass3ImplementingInterface2).Interface1);
            Assert.IsInstanceOfType(((instance as TestClass1ImplementingInterface3).Interface2 as TestClass3ImplementingInterface2).Interface1, typeof(TestClass7ImplementingInterface1));

            Assert.AreEqual(CountCall.Count("TestClass7ImplementingInterface1"), 1); // TestClass3ImplementingInterface2(ITestInterface1 interface1)
            Assert.AreEqual(CountCall.Count("TestClass3ImplementingInterface2"), 1); // TestClass1ImplementingInterface3(ITestInterface2 interface2)
            Assert.AreEqual(CountCall.Count("TestClass1ImplementingInterface3"), 1); //
        }

        [TestMethod]
        public void TestResolveScenario5()
        {
            IocContainer.Default.RegisterType<ITestInterface3, TestClass2ImplementingInterface3>();
            IocContainer.Default.RegisterType<ITestInterface2, TestClass3ImplementingInterface2>();
            IocContainer.Default.RegisterType<ITestInterface1, TestClass7ImplementingInterface1>();

            ITestInterface3 instance = IocContainer.Default.Resolve<ITestInterface3>();

            Assert.IsNotNull(instance);
            Assert.IsInstanceOfType(instance, typeof(TestClass2ImplementingInterface3));
            Assert.IsNotNull((instance as TestClass2ImplementingInterface3).Interface1);
            Assert.IsNotNull((instance as TestClass2ImplementingInterface3).Interface2);
            Assert.IsInstanceOfType((instance as TestClass2ImplementingInterface3).Interface1, typeof(TestClass7ImplementingInterface1));
            Assert.IsInstanceOfType((instance as TestClass2ImplementingInterface3).Interface2, typeof(TestClass3ImplementingInterface2));
            Assert.IsNotNull(((instance as TestClass2ImplementingInterface3).Interface2 as TestClass3ImplementingInterface2).Interface1);
            Assert.IsInstanceOfType(((instance as TestClass2ImplementingInterface3).Interface2 as TestClass3ImplementingInterface2).Interface1, typeof(TestClass7ImplementingInterface1));
            Assert.AreEqual(CountCall.Count("TestClass7ImplementingInterface1"), 2); // TestClass2ImplementingInterface3 + TestClass3ImplementingInterface2
            Assert.AreEqual(CountCall.Count("TestClass3ImplementingInterface2"), 1); // TestClass2ImplementingInterface3
            Assert.AreEqual(CountCall.Count("TestClass2ImplementingInterface3"), 1); //
        }

        [TestMethod]
        public void TestParameterValue()
        {
            ParameterValue value = new ParameterValue("parameter", this);

            Assert.AreEqual(value.Name, "parameter");
            Assert.AreEqual(value.Value, this);
        }
    }
}

// Tests for old implementation
    //[TestClass]
    //public class IocContainerTest
    //{
    //    [TestInitialize]
    //    public void Initialize()
    //    {
    //        IocContainer.Default.Reset();
    //    }

    //    [TestMethod]
    //    public void TestDefaultNonNull()
    //    {
    //        IIocContainer container = IocContainer.Default;

    //        Assert.IsNotNull(container);
    //    }

    //    [TestMethod]
    //    public void TestDefaultUniqueness()
    //    {
    //        IIocContainer container1 = IocContainer.Default;
    //        IIocContainer container2 = IocContainer.Default;

    //        Assert.AreSame(container1, container2);
    //    }

    //    [TestMethod]
    //    public void TestDefaultDifferentThanResolved()
    //    {
    //        IocContainer.Default.RegisterType<IIocContainer, IocContainer>();

    //        IIocContainer container = IocContainer.Default.Resolve<IIocContainer>();

    //        Assert.AreNotSame(container, IocContainer.Default);
    //    }

    //    [TestMethod]
    //    public void TestReset()
    //    {
    //        IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();
    //        IocContainer.Default.RegisterType<ITestInterface2, TestClass1ImplementingInterface2>();
    //        IocContainer.Default.Reset();
            
    //        try
    //        {
    //            IocContainer.Default.Resolve<ITestInterface1>();

    //            Assert.Fail("Exception not thrown");
    //        }
    //        catch (ArgumentException)
    //        {
    //        }

    //        try
    //        {
    //            IocContainer.Default.Resolve<ITestInterface2>();

    //            Assert.Fail("Exception not thrown");
    //        }
    //        catch (ArgumentException)
    //        {
    //        }
    //    }

    //    [TestMethod]
    //    public void TestSuccessfulRegistrationNoCreator()
    //    {
    //        IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();

    //        ITestInterface1 instance = IocContainer.Default.Resolve<ITestInterface1>();
            
    //        Assert.IsNotNull(instance);
    //        Assert.IsInstanceOfType(instance, typeof(TestClass1ImplementingInterface1));
    //    }

    //    [TestMethod]
    //    public void TestSuccessfulRegistrationFactory()
    //    {
    //        IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();
    //        IocContainer.Default.RegisterFactory<ITestInterface1>(() => new TestClass1ImplementingInterface1());

    //        ITestInterface1 instance = IocContainer.Default.Resolve<ITestInterface1>();

    //        Assert.IsNotNull(instance);
    //        Assert.IsInstanceOfType(instance, typeof(TestClass1ImplementingInterface1));
    //    }

    //    [TestMethod]
    //    public void TestRegisterFactoryWithoutRegisterType()
    //    {
    //        IocContainer.Default.RegisterFactory<ITestInterface1>(() => new TestClass1ImplementingInterface1());

    //        try
    //        {
    //            ITestInterface1 instance = IocContainer.Default.Resolve<ITestInterface1>();

    //            Assert.Fail("Exception not thrown");
    //        }
    //        catch (ArgumentException ex)
    //        {
    //            Assert.IsTrue(true);
    //        }
    //    }

    //    [TestMethod]
    //    public void TestRegisterInstanceWithoutRegisterType()
    //    {
    //       IocContainer.Default.RegisterInstance<ITestInterface1>(new TestClass1ImplementingInterface1());

    //        try
    //        {
    //            ITestInterface1 instance = IocContainer.Default.Resolve<ITestInterface1>();

    //            Assert.Fail("Exception not thrown");
    //        }
    //        catch (ArgumentException ex)
    //        {
    //            Assert.IsTrue(true);
    //        }
    //    }

    //    [TestMethod]
    //    public void TestRegister2DifferentRegistrationsOnSameInterface()
    //    {
    //       IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();

    //        try
    //        {
    //            IocContainer.Default.RegisterType<ITestInterface1, TestClass2ImplementingInterface1>();

    //            Assert.Fail("InvalidOperationException not raised");
    //        }
    //        catch (InvalidOperationException)
    //        {
    //            Assert.IsTrue(true);
    //        }
    //    }

    //    [TestMethod]
    //    public void TestRegister2IdenticalRegistrationsOnSameInterface()
    //    {
    //        IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();
    //        IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();

    //        ITestInterface1 instance = IocContainer.Default.Resolve<ITestInterface1>();

    //        Assert.IsNotNull(instance);
    //        Assert.IsInstanceOfType(instance, typeof(TestClass1ImplementingInterface1));
    //    }

    //    [TestMethod]
    //    public void TestRegisterOnNonInterface()
    //    {   
    //        try
    //        {
    //            IocContainer.Default.RegisterType<TestClass1ImplementingInterface1, TestClass1ImplementingInterface1>();

    //            Assert.Fail("ArgumentException not raised");
    //        }
    //        catch (ArgumentException)
    //        {
    //            Assert.IsTrue(true);
    //        }
    //    }

    //    [TestMethod]
    //    public void TestRegisterOnAbstractClass()
    //    {
    //        try
    //        {
    //            IocContainer.Default.RegisterType<ITestInterface1, TestAbstractClassImplentatingInterface1>();

    //            Assert.Fail("ArgumentException not raised");
    //        }
    //        catch (ArgumentException)
    //        {
    //            Assert.IsTrue(true);
    //        }
    //    }

    //    [TestMethod]
    //    public void TestRegisterInterfaceAsImplementation()
    //    { 
    //        try
    //        {
    //            IocContainer.Default.RegisterType<ITestInterface1, ITestInterface1>();

    //            Assert.Fail("ArgumentException not raised");
    //        }
    //        catch (ArgumentException)
    //        {
    //            Assert.IsTrue(true);
    //        }
    //    }

    //    [TestMethod]
    //    public void TestRegisterNotAssignableFrom()
    //    {
    //        try
    //        {
    //            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface2>();

    //            Assert.Fail("ArgumentException not raised");
    //        }
    //        catch (ArgumentException)
    //        {
    //            Assert.IsTrue(true);
    //        }
    //    }

    //    [TestMethod]
    //    public void TestIsRegisteredOnNonRegisteredInterface()
    //    {
    //        IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();

    //        bool isRegistered = IocContainer.Default.IsRegistered<ITestInterface2>();

    //        Assert.IsFalse(isRegistered);
    //    }

    //    [TestMethod]
    //    public void TestIsRegisteredOnRegisteredInterface()
    //    {
    //        IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();

    //        bool isRegistered = IocContainer.Default.IsRegistered<ITestInterface1>();

    //        Assert.IsTrue(isRegistered);
    //    }

    //    [TestMethod]
    //    public void TestResolveSingleInstanceOnRegisterInstance()
    //    {
    //        IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();
    //        IocContainer.Default.RegisterInstance<ITestInterface1>(new TestClass1ImplementingInterface1());

    //        ITestInterface1 instance1 = IocContainer.Default.Resolve<ITestInterface1>();
    //        ITestInterface1 instance2 = IocContainer.Default.Resolve<ITestInterface1>();

    //        Assert.AreEqual(instance1, instance2);
    //    }

    //    [TestMethod]
    //    public void TestResolveMultipleInstanceOnRegisterType()
    //    {
    //        IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();

    //        ITestInterface1 instance1 = IocContainer.Default.Resolve<ITestInterface1>();
    //        ITestInterface1 instance2 = IocContainer.Default.Resolve<ITestInterface1>();

    //        Assert.AreNotEqual(instance1, instance2);
    //    }

    //    [TestMethod]
    //    public void TestResolveReturnsRegisteredInstance()
    //    {
    //        IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();
    //        ITestInterface1 instance0 = new TestClass1ImplementingInterface1();
    //        IocContainer.Default.RegisterInstance(instance0);

    //        ITestInterface1 instance1 = IocContainer.Default.Resolve<ITestInterface1>();
    //        ITestInterface1 instance2 = IocContainer.Default.Resolve<ITestInterface1>();

    //        Assert.AreEqual(instance0, instance1);
    //        Assert.AreEqual(instance1, instance2);
    //    }

    //    [TestMethod]
    //    public void TestResolveWithoutPublicConstructor()
    //    {
    //        IocContainer.Default.RegisterType<ITestInterface1, TestClass4ImplementingInterface1>();

    //        try
    //        {
    //            ITestInterface1 instance = IocContainer.Default.Resolve<ITestInterface1>();

    //            Assert.Fail("Exception not thrown");
    //        }
    //        catch (ArgumentException)
    //        {
    //            Assert.IsTrue(true);
    //        }
    //    }

    //    [TestMethod]
    //    public void TestResolveWithComplexConstructor()
    //    {
    //        IocContainer.Default.RegisterType<ITestInterface1, TestClass3ImplementingInterface1>();

    //        try
    //        {
    //            ITestInterface1 instance = IocContainer.Default.Resolve<ITestInterface1>();

    //            Assert.Fail("Exception not thrown");
    //        }
    //        catch (ArgumentException)
    //        {
    //            Assert.IsTrue(true);
    //        }
    //    }

    //    [TestMethod]
    //    public void TestResolveWithRegisterInstanceAndComplexConstructor()
    //    {
    //        IocContainer.Default.RegisterType<ITestInterface1, TestClass3ImplementingInterface1>();
    //        IocContainer.Default.RegisterInstance<ITestInterface1>(new TestClass3ImplementingInterface1(5));

    //        ITestInterface1 instance = IocContainer.Default.Resolve<ITestInterface1>();

    //        Assert.IsNotNull(instance);
    //    }

    //    [TestMethod]
    //    public void TestResolveWithRegisterInstanceAndNoPublicConstructor()
    //    {
    //        IocContainer.Default.RegisterType<ITestInterface1, TestClass4ImplementingInterface1>();
    //        IocContainer.Default.RegisterInstance<ITestInterface1>(new TestClass5ImplementingInterface1());

    //        ITestInterface1 instance = IocContainer.Default.Resolve<ITestInterface1>();

    //        Assert.IsNotNull(instance);
    //    }

    //    [TestMethod]
    //    public void TestResolveSimpleCyclicDependency()
    //    {
    //        IocContainer.Default.RegisterType<ITestInterface2, TestClass4ImplementingInterface2>();

    //        try
    //        {
    //            ITestInterface2 instance = IocContainer.Default.Resolve<ITestInterface2>();

    //            Assert.Fail("Exception not thrown");
    //        }
    //        catch (ArgumentException ex)
    //        {
    //            Assert.IsTrue(true);
    //        }
    //    }

    //    [TestMethod]
    //    public void TestResolveComplexCyclicDependency()
    //    {
    //        IocContainer.Default.RegisterType<ITestInterface1, TestClass6ImplementingInterface1>();
    //        IocContainer.Default.RegisterType<ITestInterface2, TestClass3ImplementingInterface2>();

    //        try
    //        {
    //            ITestInterface1 instance = IocContainer.Default.Resolve<ITestInterface1>();

    //            Assert.Fail("Exception not thrown");
    //        }
    //        catch(ArgumentException ex)
    //        {
    //            Assert.IsTrue(true);
    //        }
    //    }

    //    [TestMethod]
    //    public void TestResolveReallyComplexCyclicDependency()
    //    {
    //        IocContainer.Default.RegisterType<ITestInterface1, TestClass5ImplementingInterface1>();
    //        IocContainer.Default.RegisterType<ITestInterface2, TestClass5ImplementingInterface2>();
    //        IocContainer.Default.RegisterType<ITestInterface3, TestClass1ImplementingInterface3>();

    //        ITestInterface3 instance = IocContainer.Default.Resolve<ITestInterface3>();

    //        Assert.IsNotNull(instance);
    //    }

    //    [TestMethod]
    //    public void TestUnregisterExceptionOnNonRegisteredInterface()
    //    {
    //        IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();

    //        try
    //        {
    //            IocContainer.Default.Unregister<ITestInterface2>();

    //            Assert.Fail("Exception not thrown");
    //        }
    //        catch (ArgumentException)
    //        {
    //        }
    //    }

    //    [TestMethod]
    //    public void TestUnregisterNoExceptionOnRegisteredInterface()
    //    {
    //        IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();

    //        IocContainer.Default.Unregister<ITestInterface1>();
    //    }

    //    [TestMethod]
    //    public void TestIfUnregisterRemoveInterface()
    //    {
    //        IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();

    //        IocContainer.Default.Unregister<ITestInterface1>();
    //        try
    //        {
    //            IocContainer.Default.Resolve<ITestInterface1>();

    //            Assert.Fail("No exception thrown");
    //        }
    //        catch (ArgumentException)
    //        {
    //        }
    //    }

    //    [TestMethod]
    //    public void TestResolveWithInvalidUserDefinedParameters()
    //    {
    //        IocContainer.Default.RegisterType<ITestInterface1, TestClass3ImplementingInterface1>();

    //        try
    //        {
    //            ITestInterface1 instance = IocContainer.Default.Resolve<ITestInterface1>(new[]
    //                {
    //                    new ParameterValue("y", 5)
    //                });
    //            Assert.Fail("No exception thrown");
    //        }
    //        catch (ArgumentException)
    //        {
    //            Assert.IsTrue(true);
    //        }
    //    }

    //    [TestMethod]
    //    public void TestResolveWithUserDefinedParameters()
    //    {
    //        IocContainer.Default.RegisterType<ITestInterface1, TestClass3ImplementingInterface1>();

    //        ITestInterface1 instance = IocContainer.Default.Resolve<ITestInterface1>(new[]
    //            {
    //                new ParameterValue("x", 5)
    //            });

    //        Assert.IsInstanceOfType(instance, typeof(TestClass3ImplementingInterface1));
    //        Assert.AreEqual((instance as TestClass3ImplementingInterface1).X, 5);
    //    }
    //}
