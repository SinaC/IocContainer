using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyIoc;
using EasyIoc.Exceptions;
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
        [ExpectedException(typeof(ResolveException))]
        public void TestReset()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();
            IocContainer.Default.RegisterType<ITestInterface2, TestClass1ImplementingInterface2>();
            IocContainer.Default.Reset();

            IocContainer.Default.Resolve<ITestInterface1>();
            IocContainer.Default.Resolve<ITestInterface2>();
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
        public void TestSuccessfulRegistrationNoFactoryNeitherInstance_TryResolve()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();

            bool isResolved = IocContainer.Default.TryResolve(out ITestInterface1 instance);

            Assert.IsTrue(isResolved);
            Assert.IsNotNull(instance);
            Assert.IsInstanceOfType(instance, typeof(TestClass1ImplementingInterface1));
        }

        [TestMethod]
        public void TestRegisterRegisterTypeAndRegisterInstance()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();

            IocContainer.Default.RegisterInstance<ITestInterface1>(new TestClass2ImplementingInterface1());
            ITestInterface1 instance = IocContainer.Default.Resolve<ITestInterface1>();

            Assert.IsInstanceOfType(instance, typeof(TestClass2ImplementingInterface1));
        }

        [TestMethod]
        public void TestRegisterRegisterTypeAndRegisterInstance_TryResolve()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();

            IocContainer.Default.RegisterInstance<ITestInterface1>(new TestClass2ImplementingInterface1());
            bool isResolved = IocContainer.Default.TryResolve(out ITestInterface1 instance);

            Assert.IsTrue(isResolved);
            Assert.IsInstanceOfType(instance, typeof(TestClass2ImplementingInterface1));
        }

        [TestMethod]
        public void TestRegisterRegisterInstanceAndRegisterType()
        {
            IocContainer.Default.RegisterInstance<ITestInterface1>(new TestClass2ImplementingInterface1());

            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();
            ITestInterface1 instance = IocContainer.Default.Resolve<ITestInterface1>();

            Assert.IsInstanceOfType(instance, typeof(TestClass2ImplementingInterface1));
        }

        [TestMethod]
        public void TestRegisterRegisterInstanceAndRegisterType_TryResolve()
        {
            IocContainer.Default.RegisterInstance<ITestInterface1>(new TestClass2ImplementingInterface1());

            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();
            bool isResolved = IocContainer.Default.TryResolve(out ITestInterface1 instance);

            Assert.IsTrue(isResolved);
            Assert.IsInstanceOfType(instance, typeof(TestClass2ImplementingInterface1));
        }

        [TestMethod]
        [ExpectedException(typeof(RegisterTypeException))]
        public void TestRegister2DifferentRegistrationsOnSameInterface()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();

            IocContainer.Default.RegisterType<ITestInterface1, TestClass2ImplementingInterface1>();
        }

        [TestMethod]
        [ExpectedException(typeof(RegisterTypeException))]
        public void TestRegister2IdenticalRegistrationsOnSameInterface()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();

            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();
        }

        [TestMethod]
        [ExpectedException(typeof(RegisterTypeException))]
        public void TestRegisterOnNonInterface()
        {
            IocContainer.Default.RegisterType<TestClass1ImplementingInterface1, TestClass1ImplementingInterface1>();
        }

        [TestMethod]
        [ExpectedException(typeof(RegisterTypeException))]
        public void TestRegisterOnAbstractClass()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestAbstractClassImplentatingInterface1>();
        }

        [TestMethod]
        [ExpectedException(typeof(RegisterTypeException))]
        public void TestRegisterInterfaceAsImplementation()
        {
            IocContainer.Default.RegisterType<ITestInterface1, ITestInterface1>();
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
        public void TestIsRegisteredOnRegisteredInstance()
        {
            IocContainer.Default.RegisterInstance<ITestInterface1>(new TestClass1ImplementingInterface1());

            bool isRegistered = IocContainer.Default.IsRegistered<ITestInterface1>();

            Assert.IsTrue(isRegistered);
        }

        [TestMethod]
        [ExpectedException(typeof(ResolveException))]
        public void TestUnregister()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();
            IocContainer.Default.Unregister<ITestInterface1>();

            IocContainer.Default.Resolve<ITestInterface1>();
        }

        [TestMethod]
        public void TestUnregister_TryResolve()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();

            IocContainer.Default.Unregister<ITestInterface1>();
            bool isResolved = IocContainer.Default.TryResolve(out ITestInterface1 _);

            Assert.IsFalse(isResolved);
        }

        [TestMethod]
        public void TestUnregisterNoExceptionOnUnknownInterface()
        {
            IocContainer.Default.Unregister<ITestInterface1>();
        }

        [TestMethod]
        [ExpectedException(typeof(ResolveException))]
        public void TestUnregisterType()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();
            IocContainer.Default.UnregisterType<ITestInterface1>();

            IocContainer.Default.Resolve<ITestInterface1>();
        }

        [TestMethod]
        public void TestUnregisterType_TryResolve()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();

            IocContainer.Default.UnregisterType<ITestInterface1>();
            bool isResolved = IocContainer.Default.TryResolve(out ITestInterface1 _);

            Assert.IsFalse(isResolved);
        }

        [TestMethod]
        [ExpectedException(typeof(ResolveException))]
        public void TestUnregisterInstance()
        {
            IocContainer.Default.RegisterInstance<ITestInterface1>(new TestClass1ImplementingInterface1());
            IocContainer.Default.UnregisterInstance<ITestInterface1>();

            IocContainer.Default.Resolve<ITestInterface1>();
        }

        [TestMethod]
        public void TestUnregisterInstance_TryResolve()
        {
            IocContainer.Default.RegisterInstance<ITestInterface1>(new TestClass1ImplementingInterface1());

            IocContainer.Default.UnregisterInstance<ITestInterface1>();
            bool isResolved = IocContainer.Default.TryResolve(out ITestInterface1 _);

            Assert.IsFalse(isResolved);
        }

        [TestMethod]
        public void TestResolveSingleInstanceOnRegisterInstance()
        {
            IocContainer.Default.RegisterInstance<ITestInterface1>(new TestClass7ImplementingInterface1());

            ITestInterface1 instance1 = IocContainer.Default.Resolve<ITestInterface1>();
            ITestInterface1 instance2 = IocContainer.Default.Resolve<ITestInterface1>();

            Assert.AreEqual(instance1, instance2);
            Assert.AreEqual(1, CountCall.Count("TestClass7ImplementingInterface1"));
        }

        [TestMethod]
        public void TestResolveSingleInstanceOnRegisterInstance_TryResolve()
        {
            IocContainer.Default.RegisterInstance<ITestInterface1>(new TestClass7ImplementingInterface1());

            bool isResolved1 = IocContainer.Default.TryResolve(out ITestInterface1 instance1);
            bool isResolved2 = IocContainer.Default.TryResolve(out ITestInterface1 instance2);

            Assert.IsTrue(isResolved1);
            Assert.IsTrue(isResolved2);
            Assert.AreEqual(instance1, instance2);
            Assert.AreEqual(1, CountCall.Count("TestClass7ImplementingInterface1"));
        }

        [TestMethod]
        public void TestResolveMultipleInstanceOnRegisterType()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass7ImplementingInterface1>();

            ITestInterface1 instance1 = IocContainer.Default.Resolve<ITestInterface1>();
            ITestInterface1 instance2 = IocContainer.Default.Resolve<ITestInterface1>();

            Assert.AreNotEqual(instance1, instance2);
            Assert.AreEqual(2, CountCall.Count("TestClass7ImplementingInterface1"));
        }

        [TestMethod]
        public void TestResolveMultipleInstanceOnRegisterType_TryResolve()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass7ImplementingInterface1>();

            bool isResolved1 = IocContainer.Default.TryResolve(out ITestInterface1 instance1);
            bool isResolved2 = IocContainer.Default.TryResolve(out ITestInterface1 instance2);

            Assert.IsTrue(isResolved1);
            Assert.IsTrue(isResolved2);
            Assert.AreNotEqual(instance1, instance2);
            Assert.AreEqual(2, CountCall.Count("TestClass7ImplementingInterface1"));
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
        public void TestResolveReturnsRegisteredInstance_TryResolve()
        {
            ITestInterface1 instance0 = new TestClass1ImplementingInterface1();
            IocContainer.Default.RegisterInstance(instance0);

            bool isResolved1 = IocContainer.Default.TryResolve(out ITestInterface1 instance1);
            bool isResolved2 = IocContainer.Default.TryResolve(out ITestInterface1 instance2);

            Assert.IsTrue(isResolved1);
            Assert.IsTrue(isResolved2);
            Assert.AreEqual(instance0, instance1);
            Assert.AreEqual(instance1, instance2);
        }

        [TestMethod]
        [ExpectedException(typeof(ResolveException))]
        public void TestResolveWithoutPublicConstructor()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass4ImplementingInterface1>();

            ITestInterface1 instance = IocContainer.Default.Resolve<ITestInterface1>();
        }

        [TestMethod]
        public void TestResolveWithoutPublicConstructor_TryResolve()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass4ImplementingInterface1>();

            bool isResolved = IocContainer.Default.TryResolve(out ITestInterface1 _);

            Assert.IsFalse(isResolved);
        }

        [TestMethod]
        [ExpectedException(typeof(ResolveException))]
        public void TestResolveWithComplexConstructor()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass3ImplementingInterface1>();

            ITestInterface1 instance = IocContainer.Default.Resolve<ITestInterface1>();
        }

        [TestMethod]
        public void TestResolveWithComplexConstructor_TryResolve()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass3ImplementingInterface1>();

            bool isResolved = IocContainer.Default.TryResolve(out ITestInterface1 instance);

            Assert.IsFalse(isResolved);
        }

        [TestMethod]
        public void TestResolveWithRegisterInstanceAndComplexConstructor()
        {
            IocContainer.Default.RegisterInstance<ITestInterface1>(new TestClass3ImplementingInterface1(5));

            ITestInterface1 instance = IocContainer.Default.Resolve<ITestInterface1>();

            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void TestResolveWithRegisterInstanceAndComplexConstructor_TryResolve()
        {
            IocContainer.Default.RegisterInstance<ITestInterface1>(new TestClass3ImplementingInterface1(5));

            bool isResolved = IocContainer.Default.TryResolve(out ITestInterface1 instance);

            Assert.IsTrue(isResolved);
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
        public void TestResolveWithRegisterInstanceAndNoPublicConstructor_TryResolve()
        {
            IocContainer.Default.RegisterInstance<ITestInterface1>(new TestClass5ImplementingInterface1());

            bool isResolved = IocContainer.Default.TryResolve(out ITestInterface1 instance);

            Assert.IsTrue(isResolved);
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
        public void TestResolveManyConstructors_TryResolve()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass5ImplementingInterface1>();
            IocContainer.Default.RegisterType<ITestInterface2, TestClass5ImplementingInterface2>();
            IocContainer.Default.RegisterType<ITestInterface3, TestClass1ImplementingInterface3>();

            bool isResolved = IocContainer.Default.TryResolve(out ITestInterface3 instance); // Will resolve using ctor TestClass1ImplementingInterface3(ITestInterface1 interface1)

            Assert.IsTrue(isResolved);
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        [ExpectedException(typeof(ResolveException))]
        public void TestResolveSimpleCyclicDependency()
        {
            IocContainer.Default.RegisterType<ITestInterface2, TestClass4ImplementingInterface2>();

            ITestInterface2 instance = IocContainer.Default.Resolve<ITestInterface2>();
        }

        [TestMethod]
        public void TestResolveSimpleCyclicDependency_TryResolve()
        {
            IocContainer.Default.RegisterType<ITestInterface2, TestClass4ImplementingInterface2>();

            bool isResolved = IocContainer.Default.TryResolve(out ITestInterface2 _);

            Assert.IsFalse(isResolved);
        }

        [TestMethod]
        [ExpectedException(typeof(ResolveException))]
        public void TestResolveComplexCyclicDependency()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass6ImplementingInterface1>();
            IocContainer.Default.RegisterType<ITestInterface2, TestClass3ImplementingInterface2>();

            IocContainer.Default.Resolve<ITestInterface1>();
        }

        [TestMethod]
        public void TestResolveComplexCyclicDependency_TryResolve()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass6ImplementingInterface1>();
            IocContainer.Default.RegisterType<ITestInterface2, TestClass3ImplementingInterface2>();

            bool isResolved = IocContainer.Default.TryResolve(out ITestInterface1 _);

            Assert.IsFalse(isResolved);
        }

        [TestMethod]
        [ExpectedException(typeof(ResolveException))]
        public void TestResolveReallyComplexCyclicDependency()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass5ImplementingInterface1>();
            IocContainer.Default.RegisterType<ITestInterface2, TestClass5ImplementingInterface2>();
            IocContainer.Default.RegisterType<ITestInterface3, TestClass2ImplementingInterface3>();

            IocContainer.Default.Resolve<ITestInterface3>();
        }

        [TestMethod]
        public void TestResolveReallyComplexCyclicDependency_TryResolve()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass5ImplementingInterface1>();
            IocContainer.Default.RegisterType<ITestInterface2, TestClass5ImplementingInterface2>();
            IocContainer.Default.RegisterType<ITestInterface3, TestClass2ImplementingInterface3>();

            bool isResolved = IocContainer.Default.TryResolve(out ITestInterface3 _);

            Assert.IsFalse(isResolved);
        }

        [TestMethod]
        public void TestResolveReallyComplexCyclicDependency_AdditionalNonCyclicCtor()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass5ImplementingInterface1>();
            IocContainer.Default.RegisterType<ITestInterface2, TestClass5ImplementingInterface2>();
            IocContainer.Default.RegisterType<ITestInterface3, TestClass3ImplementingInterface3>();

            ITestInterface3 instance = IocContainer.Default.Resolve<ITestInterface3>();

            Assert.IsInstanceOfType(instance, typeof(TestClass3ImplementingInterface3));
            Assert.IsNotNull((instance as TestClass3ImplementingInterface3).Interface1);
            Assert.IsNull((instance as TestClass3ImplementingInterface3).Interface2);
        }

        [TestMethod]
        public void TestResolveReallyComplexCyclicDependency_AdditionalNonCyclicCtor_TryResolve()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass5ImplementingInterface1>();
            IocContainer.Default.RegisterType<ITestInterface2, TestClass5ImplementingInterface2>();
            IocContainer.Default.RegisterType<ITestInterface3, TestClass3ImplementingInterface3>();

            bool isResolved = IocContainer.Default.TryResolve(out ITestInterface3 instance); // will use 2nd ctor

            Assert.IsTrue(isResolved);
            Assert.IsInstanceOfType(instance, typeof(TestClass3ImplementingInterface3));
            Assert.IsNotNull((instance as TestClass3ImplementingInterface3).Interface1);
            Assert.IsNull((instance as TestClass3ImplementingInterface3).Interface2);
        }

        [TestMethod]
        [ExpectedException(typeof(ResolveException))]
        public void TestResolveResolveTreeNotSavedOnError()
        {
            IocContainer.Default.RegisterType<ITestInterface2, TestClass3ImplementingInterface2>();

            IocContainer.Default.Resolve<ITestInterface2>(); // resolve will fail because ITestInterface1 parameter of TestClass3ImplementingInterface2 ctor cannot be resolved
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
        public void TestRegisterDeeperInheriting_TryResolve()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass5ImplementingInterface1>();

            bool isResolved = IocContainer.Default.TryResolve(out ITestInterface1 instance);

            Assert.IsTrue(isResolved);
            Assert.IsNotNull(instance);
            Assert.IsInstanceOfType(instance, typeof(TestClass5ImplementingInterface1));
        }

        [TestMethod]
        public void TestResolveScenario1()
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
            Assert.AreEqual(1, CountCall.Count("TestClass3ImplementingInterface1"));
        }

        [TestMethod]
        public void TestResolveScenario1_TryResolve()
        {
            IocContainer.Default.RegisterType<ITestInterface3, TestClass1ImplementingInterface3>();
            IocContainer.Default.RegisterType<ITestInterface2, TestClass3ImplementingInterface2>();
            IocContainer.Default.RegisterInstance<ITestInterface1>(new TestClass3ImplementingInterface1(7));

            bool isResolved = IocContainer.Default.TryResolve(out ITestInterface3 instance);

            Assert.IsTrue(isResolved);
            Assert.IsNotNull(instance);
            Assert.IsInstanceOfType(instance, typeof(TestClass1ImplementingInterface3));
            Assert.IsNull((instance as TestClass1ImplementingInterface3).Interface1);
            Assert.IsNotNull((instance as TestClass1ImplementingInterface3).Interface2);
            Assert.IsNull((instance as TestClass1ImplementingInterface3).Interface3);
            Assert.IsInstanceOfType((instance as TestClass1ImplementingInterface3).Interface2, typeof(TestClass3ImplementingInterface2));
            Assert.IsNotNull(((instance as TestClass1ImplementingInterface3).Interface2 as TestClass3ImplementingInterface2).Interface1);
            Assert.IsInstanceOfType(((instance as TestClass1ImplementingInterface3).Interface2 as TestClass3ImplementingInterface2).Interface1, typeof(TestClass3ImplementingInterface1));
            Assert.AreEqual((((instance as TestClass1ImplementingInterface3).Interface2 as TestClass3ImplementingInterface2).Interface1 as TestClass3ImplementingInterface1).X, 7);
            Assert.AreEqual(1, CountCall.Count("TestClass3ImplementingInterface1"));
        }

        [TestMethod]
        public void TestResolveScenario2()
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
            Assert.AreEqual(2, CountCall.Count("TestClass7ImplementingInterface1")); // TestClass2ImplementingInterface3 + TestClass3ImplementingInterface2
            Assert.AreEqual(1, CountCall.Count("TestClass3ImplementingInterface2")); // TestClass2ImplementingInterface3
            Assert.AreEqual(1, CountCall.Count("TestClass2ImplementingInterface3")); //
        }

        [TestMethod]
        public void TestResolveScenario2_TryResolve()
        {
            IocContainer.Default.RegisterType<ITestInterface3, TestClass2ImplementingInterface3>();
            IocContainer.Default.RegisterType<ITestInterface2, TestClass3ImplementingInterface2>();
            IocContainer.Default.RegisterType<ITestInterface1, TestClass7ImplementingInterface1>();

            bool isResolved = IocContainer.Default.TryResolve(out ITestInterface3 instance);

            Assert.IsTrue(isResolved);
            Assert.IsNotNull(instance);
            Assert.IsInstanceOfType(instance, typeof(TestClass2ImplementingInterface3));
            Assert.IsNotNull((instance as TestClass2ImplementingInterface3).Interface1);
            Assert.IsNotNull((instance as TestClass2ImplementingInterface3).Interface2);
            Assert.IsInstanceOfType((instance as TestClass2ImplementingInterface3).Interface1, typeof(TestClass7ImplementingInterface1));
            Assert.IsInstanceOfType((instance as TestClass2ImplementingInterface3).Interface2, typeof(TestClass3ImplementingInterface2));
            Assert.IsNotNull(((instance as TestClass2ImplementingInterface3).Interface2 as TestClass3ImplementingInterface2).Interface1);
            Assert.IsInstanceOfType(((instance as TestClass2ImplementingInterface3).Interface2 as TestClass3ImplementingInterface2).Interface1, typeof(TestClass7ImplementingInterface1));
            Assert.AreEqual(2, CountCall.Count("TestClass7ImplementingInterface1")); // TestClass2ImplementingInterface3 + TestClass3ImplementingInterface2
            Assert.AreEqual(1, CountCall.Count("TestClass3ImplementingInterface2")); // TestClass2ImplementingInterface3
            Assert.AreEqual(1, CountCall.Count("TestClass2ImplementingInterface3")); //
        }

        [TestMethod]
        public void TestConcurrentAccessResolve()
        {
            const int taskCount = 10000;

            IocContainer.Default.RegisterType<ITestInterface3, TestClass2ImplementingInterface3>();
            IocContainer.Default.RegisterType<ITestInterface2, TestClass3ImplementingInterface2>();
            IocContainer.Default.RegisterType<ITestInterface1, TestClass7ImplementingInterface1>();

            List<Task> tasks = Enumerable.Range(0, taskCount).Select(x => new Task(() =>
            {
                ITestInterface3 instance = IocContainer.Default.Resolve<ITestInterface3>();

                Assert.IsNotNull(instance);
                Assert.IsInstanceOfType(instance, typeof(TestClass2ImplementingInterface3));
                Assert.IsNotNull((instance as TestClass2ImplementingInterface3).Interface1);
                Assert.IsNotNull((instance as TestClass2ImplementingInterface3).Interface2);
                Assert.IsInstanceOfType((instance as TestClass2ImplementingInterface3).Interface1, typeof(TestClass7ImplementingInterface1));
                Assert.IsInstanceOfType((instance as TestClass2ImplementingInterface3).Interface2, typeof(TestClass3ImplementingInterface2));
                Assert.IsNotNull(((instance as TestClass2ImplementingInterface3).Interface2 as TestClass3ImplementingInterface2).Interface1);
                Assert.IsInstanceOfType(((instance as TestClass2ImplementingInterface3).Interface2 as TestClass3ImplementingInterface2).Interface1, typeof(TestClass7ImplementingInterface1));
            })).ToList();

            try
            {
                foreach (Task task in tasks)
                    task.Start();
                Task.WaitAll(tasks.ToArray());

                Assert.AreEqual(2 * taskCount, CountCall.Count("TestClass7ImplementingInterface1")); // TestClass2ImplementingInterface3 + TestClass3ImplementingInterface2
                Assert.AreEqual(1 * taskCount, CountCall.Count("TestClass3ImplementingInterface2")); // TestClass2ImplementingInterface3
                Assert.AreEqual(1 * taskCount, CountCall.Count("TestClass2ImplementingInterface3")); //
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.ToString()); // failed
            }
        }

        [TestMethod]
        public void TestConcurrentAccessResolve_TryResolve()
        {
            const int taskCount = 10000;

            IocContainer.Default.RegisterType<ITestInterface3, TestClass2ImplementingInterface3>();
            IocContainer.Default.RegisterType<ITestInterface2, TestClass3ImplementingInterface2>();
            IocContainer.Default.RegisterType<ITestInterface1, TestClass7ImplementingInterface1>();

            List<Task> tasks = Enumerable.Range(0, taskCount).Select(x => new Task(() =>
            {
                bool isResolved = IocContainer.Default.TryResolve(out ITestInterface3 instance);

                Assert.IsTrue(isResolved);
                Assert.IsNotNull(instance);
                Assert.IsInstanceOfType(instance, typeof(TestClass2ImplementingInterface3));
                Assert.IsNotNull((instance as TestClass2ImplementingInterface3).Interface1);
                Assert.IsNotNull((instance as TestClass2ImplementingInterface3).Interface2);
                Assert.IsInstanceOfType((instance as TestClass2ImplementingInterface3).Interface1, typeof(TestClass7ImplementingInterface1));
                Assert.IsInstanceOfType((instance as TestClass2ImplementingInterface3).Interface2, typeof(TestClass3ImplementingInterface2));
                Assert.IsNotNull(((instance as TestClass2ImplementingInterface3).Interface2 as TestClass3ImplementingInterface2).Interface1);
                Assert.IsInstanceOfType(((instance as TestClass2ImplementingInterface3).Interface2 as TestClass3ImplementingInterface2).Interface1, typeof(TestClass7ImplementingInterface1));
            })).ToList();

            try
            {
                foreach (Task task in tasks)
                    task.Start();
                Task.WaitAll(tasks.ToArray());

                Assert.AreEqual(2 * taskCount, CountCall.Count("TestClass7ImplementingInterface1")); // TestClass2ImplementingInterface3 + TestClass3ImplementingInterface2
                Assert.AreEqual(1 * taskCount, CountCall.Count("TestClass3ImplementingInterface2")); // TestClass2ImplementingInterface3
                Assert.AreEqual(1 * taskCount, CountCall.Count("TestClass2ImplementingInterface3")); //
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.ToString()); // failed
            }
        }

        [TestMethod]
        public void TestNamedResolveOnNamedRegister()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>("class1");

            ITestInterface1 instance = IocContainer.Default.Resolve<ITestInterface1>("class1");

            Assert.IsNotNull(instance);
            Assert.IsInstanceOfType(instance, typeof(TestClass1ImplementingInterface1));
        }

        [TestMethod]
        public void TestNamedResolveOnNamedRegister_TryResolve()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>("class1");

            bool isResolved = IocContainer.Default.TryResolve("class1", out ITestInterface1 instance);

            Assert.IsTrue(isResolved);
            Assert.IsNotNull(instance);
            Assert.IsInstanceOfType(instance, typeof(TestClass1ImplementingInterface1));
        }


        [TestMethod]
        public void TestRegisterOnNamedAndAnonymousOnSameImplementation()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>("class1");
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();

            ITestInterface1 anonymousInstance = IocContainer.Default.Resolve<ITestInterface1>();
            ITestInterface1 namedInstance = IocContainer.Default.Resolve<ITestInterface1>("class1");

            Assert.IsNotNull(anonymousInstance);
            Assert.IsInstanceOfType(anonymousInstance, typeof(TestClass1ImplementingInterface1));
            Assert.IsNotNull(namedInstance);
            Assert.IsInstanceOfType(namedInstance, typeof(TestClass1ImplementingInterface1));
        }

        [TestMethod]
        public void TestAnonymousResolveOnNamedAndAnonymousRegister()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>("class1");
            IocContainer.Default.RegisterType<ITestInterface1, TestClass2ImplementingInterface1>();

            ITestInterface1 instance = IocContainer.Default.Resolve<ITestInterface1>();

            Assert.IsNotNull(instance);
            Assert.IsInstanceOfType(instance, typeof(TestClass2ImplementingInterface1)); // anonymous instance will be resolved one
        }

        [TestMethod]
        public void TestAnonymousResolveOnNamedAndAnonymousRegister_TryResolve()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>("class1");
            IocContainer.Default.RegisterType<ITestInterface1, TestClass2ImplementingInterface1>();

            bool isResolved = IocContainer.Default.TryResolve(out ITestInterface1 instance);

            Assert.IsTrue(isResolved);
            Assert.IsInstanceOfType(instance, typeof(TestClass2ImplementingInterface1)); // anonymous instance will be resolved one
        }

        [TestMethod]
        public void TestNamedResolveOnNamedAndAnonymousRegister()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>("class1");
            IocContainer.Default.RegisterType<ITestInterface1, TestClass2ImplementingInterface1>();

            ITestInterface1 instance = IocContainer.Default.Resolve<ITestInterface1>("class1");

            Assert.IsInstanceOfType(instance, typeof(TestClass1ImplementingInterface1)); // named instance will be resolved one
        }

        [TestMethod]
        public void TestNamedResolveOnNamedAndAnonymousRegister_TryResolve()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>("class1");
            IocContainer.Default.RegisterType<ITestInterface1, TestClass2ImplementingInterface1>();

            bool isResolved = IocContainer.Default.TryResolve("class1", out ITestInterface1 instance);

            Assert.IsTrue(isResolved);
            Assert.IsInstanceOfType(instance, typeof(TestClass1ImplementingInterface1)); // named instance will be resolved one
        }

        [TestMethod]
        [ExpectedException(typeof(ResolveException))]
        public void TestNamedResolveOnDifferentNamedRegister()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>("class1");

            IocContainer.Default.Resolve<ITestInterface1>("class2");
        }

        [TestMethod]
        public void TestNamedResolveOnDifferentNamedRegister_TryResolve()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>("class1");

            bool isResolved = IocContainer.Default.TryResolve("class2", out ITestInterface1 _);

            Assert.IsFalse(isResolved);
        }

        [TestMethod]
        [ExpectedException(typeof(ResolveException))]
        public void TestNamedResolveOnAnonymousRegister()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();

            IocContainer.Default.Resolve<ITestInterface1>("class1");
        }

        [TestMethod]
        public void TestNamedResolveOnAnonymousRegister_TryResolve()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();

            bool isResolved = IocContainer.Default.TryResolve("class1", out ITestInterface1 _);

            Assert.IsFalse(isResolved);
        }

        [TestMethod]
        [ExpectedException(typeof(ResolveException))]
        public void TestAnonymousResolveOnNamedRegister()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>("class1");

            IocContainer.Default.Resolve<ITestInterface1>();
        }

        [TestMethod]
        public void TestAnonymousResolveOnNamedRegister_TryResolve()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>("class1");

            bool isResolved = IocContainer.Default.TryResolve(out ITestInterface1 _);

            Assert.IsFalse(isResolved);
        }

        [TestMethod]
        public void TestNamedResolveOnMultipleNamedRegister()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>("class1");
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>("class2");
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>("class3");

            ITestInterface1 instance1 = IocContainer.Default.Resolve<ITestInterface1>("class3");
            ITestInterface1 instance2 = IocContainer.Default.Resolve<ITestInterface1>("class2");
            ITestInterface1 instance3 = IocContainer.Default.Resolve<ITestInterface1>("class1");

            Assert.IsNotNull(instance1);
            Assert.IsInstanceOfType(instance1, typeof(TestClass1ImplementingInterface1));
            Assert.IsNotNull(instance2);
            Assert.IsInstanceOfType(instance2, typeof(TestClass1ImplementingInterface1));
            Assert.IsNotNull(instance3);
            Assert.IsInstanceOfType(instance3, typeof(TestClass1ImplementingInterface1));
        }

        [TestMethod]
        public void TestNamedResolveOnMultipleNamedRegister_TryResolve()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>("class1");
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>("class2");
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>("class3");

            bool isResolved1 = IocContainer.Default.TryResolve<ITestInterface1>("class3", out ITestInterface1 instance1);
            bool isResolved2 = IocContainer.Default.TryResolve<ITestInterface1>("class2", out ITestInterface1 instance2);
            bool isResolved3 = IocContainer.Default.TryResolve<ITestInterface1>("class1", out ITestInterface1 instance3);

            Assert.IsTrue(isResolved1);
            Assert.IsNotNull(instance1);
            Assert.IsInstanceOfType(instance1, typeof(TestClass1ImplementingInterface1));
            Assert.IsTrue(isResolved2);
            Assert.IsNotNull(instance2);
            Assert.IsInstanceOfType(instance2, typeof(TestClass1ImplementingInterface1));
            Assert.IsTrue(isResolved3);
            Assert.IsNotNull(instance3);
            Assert.IsInstanceOfType(instance3, typeof(TestClass1ImplementingInterface1));
        }

        [TestMethod]
        [ExpectedException(typeof(RegisterTypeException))]
        public void TestMultipleNamedRegisterOnSameNameAndSameType()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>("class1");

            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>("class1");
        }

        [TestMethod]
        public void TestMultipleNamedRegisterOnSameNameAndDifferentType()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>("class1");

            IocContainer.Default.RegisterType<ITestInterface2, TestClass1ImplementingInterface2>("class1");

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestResolveOnMultipleNamedRegisterOnSameNameAndDifferentType()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>("class1");
            IocContainer.Default.RegisterType<ITestInterface2, TestClass1ImplementingInterface2>("class1");

            ITestInterface1 instance1 = IocContainer.Default.Resolve<ITestInterface1>("class1");
            ITestInterface2 instance2 = IocContainer.Default.Resolve<ITestInterface2>("class1");

            Assert.IsInstanceOfType(instance1, typeof(TestClass1ImplementingInterface1));
            Assert.IsInstanceOfType(instance2, typeof(TestClass1ImplementingInterface2));
        }

        [TestMethod]
        public void TestResolveOnMultipleNamedRegisterOnSameNameAndDifferentType_TryResolve()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>("class1");
            IocContainer.Default.RegisterType<ITestInterface2, TestClass1ImplementingInterface2>("class1");

            bool isResolved1 = IocContainer.Default.TryResolve("class1", out ITestInterface1 instance1);
            bool isResolved2 = IocContainer.Default.TryResolve("class1", out ITestInterface2 instance2);

            Assert.IsTrue(isResolved1);
            Assert.IsInstanceOfType(instance1, typeof(TestClass1ImplementingInterface1));
            Assert.IsTrue(isResolved2);
            Assert.IsInstanceOfType(instance2, typeof(TestClass1ImplementingInterface2));
        }

        [TestMethod]
        public void TestAnonymousUnregisterOnNamedRegister()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>("class1");

            IocContainer.Default.Unregister<ITestInterface1>();

            Assert.IsFalse(IocContainer.Default.IsRegistered<ITestInterface1>());
        }

        [TestMethod]
        public void TestAnonymousUnregisterTypeOnNamedRegister()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>("class1");

            IocContainer.Default.UnregisterType<ITestInterface1>();

            Assert.IsFalse(IocContainer.Default.IsRegistered<ITestInterface1>());
        }

        [TestMethod]
        public void TestNamedUnregisterTypeOnAnonymousRegister()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>();

            IocContainer.Default.UnregisterType<ITestInterface1>("class1");

            Assert.IsTrue(IocContainer.Default.IsRegistered<ITestInterface1>());
        }

        [TestMethod]
        public void TestNamedUnregisterTypeOnNamedRegister()
        {
            IocContainer.Default.RegisterType<ITestInterface1, TestClass1ImplementingInterface1>("class1");

            IocContainer.Default.UnregisterType<ITestInterface1>("class1");

            Assert.IsFalse(IocContainer.Default.IsRegistered<ITestInterface1>());
        }

        // TODO: more named and named vs anonymous tests
    }
}