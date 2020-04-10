using System;
using EasyIoc;
using EasyIoc.Attributes;
using EasyIoc.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EasyIocTest
{
    [TestClass]
    public class InitializeFromConfigTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            IocContainer.Default.Reset();
            CountCall.Reset();
        }

        //
        [TestMethod]
        public void NonSingleton_IsRegistered_Test()
        {
            IocContainer.Default.InitializeFromConfig();

            Assert.IsTrue(IocContainer.Default.IsRegistered<IInitializeFromConfig1>());
        }

        [TestMethod]
        public void NonSingleton_Resolve_NotNull_Test()
        {
            IocContainer.Default.InitializeFromConfig();

            IInitializeFromConfig1 instance = IocContainer.Default.Resolve<IInitializeFromConfig1>();

            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void NonSingleton_Resolve_CorrectImplementationType_Test()
        {
            IocContainer.Default.InitializeFromConfig();

            IInitializeFromConfig1 instance = IocContainer.Default.Resolve<IInitializeFromConfig1>();

            Assert.IsInstanceOfType(instance, typeof(InitializeFromConfig1_1));
        }

        [TestMethod]
        public void NonSingleton_Resolve_MultipleResolveDifferenceInstances_Test()
        {
            IocContainer.Default.InitializeFromConfig();

            IInitializeFromConfig1 instance1 = IocContainer.Default.Resolve<IInitializeFromConfig1>();
            IInitializeFromConfig1 instance2 = IocContainer.Default.Resolve<IInitializeFromConfig1>();
            IInitializeFromConfig1 instance3 = IocContainer.Default.Resolve<IInitializeFromConfig1>();

            Assert.AreNotSame(instance1, instance2);
            Assert.AreNotSame(instance1, instance3);
            Assert.AreNotSame(instance2, instance3);
        }

        //
        [TestMethod]
        public void NonSingleton_NamedImplementation_IsRegistered_CorrectNameSpecified_Test()
        {
            IocContainer.Default.InitializeFromConfig();

            Assert.IsTrue(IocContainer.Default.IsRegistered<IInitializeFromConfig2>("NamedImplementationOfIInitializeFromConfig2"));
        }

        [TestMethod]
        public void NonSingleton_NamedImplementation__Resolve_NotNull_CorrectNameSpecified_Test()
        {
            IocContainer.Default.InitializeFromConfig();

            IInitializeFromConfig2 instance = IocContainer.Default.Resolve<IInitializeFromConfig2>("NamedImplementationOfIInitializeFromConfig2");

            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void NonSingleton_NamedImplementation__Resolve_CorrectImplementationType_CorrectNameSpecified_Test()
        {
            IocContainer.Default.InitializeFromConfig();

            IInitializeFromConfig2 instance = IocContainer.Default.Resolve<IInitializeFromConfig2>("NamedImplementationOfIInitializeFromConfig2");

            Assert.IsInstanceOfType(instance, typeof(InitializeFromConfig2_1));
        }

        [TestMethod]
        public void NonSingleton_NamedImplementation_Resolve_MultipleResolveDifferenceInstances_Test()
        {
            IocContainer.Default.InitializeFromConfig();

            IInitializeFromConfig2 instance1 = IocContainer.Default.Resolve<IInitializeFromConfig2>("NamedImplementationOfIInitializeFromConfig2");
            IInitializeFromConfig2 instance2 = IocContainer.Default.Resolve<IInitializeFromConfig2>("NamedImplementationOfIInitializeFromConfig2");
            IInitializeFromConfig2 instance3 = IocContainer.Default.Resolve<IInitializeFromConfig2>("NamedImplementationOfIInitializeFromConfig2");

            Assert.AreNotSame(instance1, instance2);
            Assert.AreNotSame(instance1, instance3);
            Assert.AreNotSame(instance2, instance3);
        }

        [TestMethod]
        public void NonSingleton_NamedImplementation_IsRegistered_NameNotSpecified_Test()
        {
            IocContainer.Default.InitializeFromConfig();

            Assert.IsFalse(IocContainer.Default.IsRegistered<IInitializeFromConfig2>());
        }

        [TestMethod]
        [ExpectedException(typeof(ResolveException))]
        public void NonSingleton_NamedImplementation_Resolve_Exception_NameNotSpecified_Test()
        {
            IocContainer.Default.InitializeFromConfig();

            IocContainer.Default.Resolve<IInitializeFromConfig2>();
        }

        [TestMethod]
        public void NonSingleton_NamedImplementation_IsRegistered_IncorrectNameSpecified_Test()
        {
            IocContainer.Default.InitializeFromConfig();

            Assert.IsFalse(IocContainer.Default.IsRegistered<IInitializeFromConfig2>("Test"));
        }

        [TestMethod]
        [ExpectedException(typeof(ResolveException))]
        public void NonSingleton_NamedImplementation_Resolve_Exception_IncorrectNameSpecified_Test()
        {
            IocContainer.Default.InitializeFromConfig();

            IocContainer.Default.Resolve<IInitializeFromConfig2>("Test");
        }

        //
        [TestMethod]
        public void Singleton_IsRegistered_Test()
        {
            IocContainer.Default.InitializeFromConfig();

            Assert.IsTrue(IocContainer.Default.IsRegistered<IInitializeFromConfig3>());
        }

        [TestMethod]
        public void Singleton_Resolve_NotNull_Test()
        {
            IocContainer.Default.InitializeFromConfig();

            IInitializeFromConfig3 instance = IocContainer.Default.Resolve<IInitializeFromConfig3>();

            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void Singleton_Resolve_CorrectImplementationType_Test()
        {
            IocContainer.Default.InitializeFromConfig();

            IInitializeFromConfig3 instance = IocContainer.Default.Resolve<IInitializeFromConfig3>();

            Assert.IsInstanceOfType(instance, typeof(InitializeFromConfig3_1));
        }

        [TestMethod]
        public void Singleton_Resolve_MultipleResolveSingleInstance_Test()
        {
            IocContainer.Default.InitializeFromConfig();

            IInitializeFromConfig3 instance1 = IocContainer.Default.Resolve<IInitializeFromConfig3>();
            IInitializeFromConfig3 instance2 = IocContainer.Default.Resolve<IInitializeFromConfig3>();
            IInitializeFromConfig3 instance3 = IocContainer.Default.Resolve<IInitializeFromConfig3>();

            Assert.AreSame(instance1, instance2);
            Assert.AreSame(instance1, instance3);
            Assert.AreSame(instance2, instance3);
            Assert.AreEqual(1, CountCall.Count("InitializeFromConfig3_1::ctor"));
        }

        //
        [TestMethod]
        public void Singleton_NamedImplementation_IsRegistered_CorrectNameSpecified_Test()
        {
            IocContainer.Default.InitializeFromConfig();

            Assert.IsTrue(IocContainer.Default.IsRegistered<IInitializeFromConfig4>("NamedImplementationOfIInitializeFromConfig4"));
        }

        [TestMethod]
        public void Singleton_NamedImplementation_Resolve_NotNull_CorrectNameSpecified_Test()
        {
            IocContainer.Default.InitializeFromConfig();

            IInitializeFromConfig4 instance = IocContainer.Default.Resolve<IInitializeFromConfig4>("NamedImplementationOfIInitializeFromConfig4");

            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void Singleton_NamedImplementation_Resolve_CorrectImplementationType_CorrectNameSpecified_Test()
        {
            IocContainer.Default.InitializeFromConfig();

            IInitializeFromConfig4 instance = IocContainer.Default.Resolve<IInitializeFromConfig4>("NamedImplementationOfIInitializeFromConfig4");

            Assert.IsInstanceOfType(instance, typeof(InitializeFromConfig4_1));
        }

        [TestMethod]
        public void Singleton_NamedImplementation_Resolve_MultipleResolveDifferenceInstances_Test()
        {
            IocContainer.Default.InitializeFromConfig();

            IInitializeFromConfig4 instance1 = IocContainer.Default.Resolve<IInitializeFromConfig4>("NamedImplementationOfIInitializeFromConfig4");
            IInitializeFromConfig4 instance2 = IocContainer.Default.Resolve<IInitializeFromConfig4>("NamedImplementationOfIInitializeFromConfig4");
            IInitializeFromConfig4 instance3 = IocContainer.Default.Resolve<IInitializeFromConfig4>("NamedImplementationOfIInitializeFromConfig4");

            Assert.AreSame(instance1, instance2);
            Assert.AreSame(instance1, instance3);
            Assert.AreSame(instance2, instance3);
            Assert.AreEqual(1, CountCall.Count("InitializeFromConfig4_1::ctor"));
        }

        [TestMethod]
        public void Singleton_NamedImplementation_IsRegistered_NameNotSpecified_Test()
        {
            IocContainer.Default.InitializeFromConfig();

            Assert.IsFalse(IocContainer.Default.IsRegistered<IInitializeFromConfig4>());
        }

        [TestMethod]
        [ExpectedException(typeof(ResolveException))]
        public void Singleton_NamedImplementation_Resolve_Exception_NameNotSpecified_Test()
        {
            IocContainer.Default.InitializeFromConfig();

            IocContainer.Default.Resolve<IInitializeFromConfig4>();
        }

        [TestMethod]
        public void Singleton_NamedImplementation_IsRegistered_IncorrectNameSpecified_Test()
        {
            IocContainer.Default.InitializeFromConfig();

            Assert.IsFalse(IocContainer.Default.IsRegistered<IInitializeFromConfig4>("Test"));
        }

        [TestMethod]
        [ExpectedException(typeof(ResolveException))]
        public void Singleton_NamedImplementation_Resolve_Exception_IncorrectNameSpecified_Test()
        {
            IocContainer.Default.InitializeFromConfig();

            IocContainer.Default.Resolve<IInitializeFromConfig4>("Test");
        }

        [TestMethod]
        public void MultipleNamedAndOneAnonymouusRegisterType_Resolve_Test()
        {
            IocContainer.Default.InitializeFromConfig();

            IInitializeFromConfig5 anonymous = IocContainer.Default.Resolve<IInitializeFromConfig5>();
            IInitializeFromConfig5 named1 = IocContainer.Default.Resolve<IInitializeFromConfig5>("InitializeFromConfig5_1");
            IInitializeFromConfig5 named2 = IocContainer.Default.Resolve<IInitializeFromConfig5>("InitializeFromConfig5_2");

            Assert.IsNotNull(anonymous);
            Assert.IsInstanceOfType(anonymous, typeof(InitializeFromConfig5_3));
            Assert.IsNotNull(named1);
            Assert.IsInstanceOfType(named1, typeof(InitializeFromConfig5_1));
            Assert.IsNotNull(named2);
            Assert.IsInstanceOfType(named2, typeof(InitializeFromConfig5_2));
        }
    }

    //
    public interface IInitializeFromConfig1
    {
    }

    [RegisterType(typeof(IInitializeFromConfig1))] // anonymous + multi-instance
    public class InitializeFromConfig1_1 : IInitializeFromConfig1
    {
    }

    //
    public interface IInitializeFromConfig2
    {
    }

    [RegisterType("NamedImplementationOfIInitializeFromConfig2", typeof(IInitializeFromConfig2))] // named + multi-instance
    public class InitializeFromConfig2_1 : IInitializeFromConfig2
    {
    }

    //
    public interface IInitializeFromConfig3
    {
    }

    [RegisterType(true, typeof(IInitializeFromConfig3))] // anonymous + singleton
    public class InitializeFromConfig3_1 : IInitializeFromConfig3
    {
        public InitializeFromConfig3_1()
        {
            CountCall.Increment("InitializeFromConfig3_1::ctor");
        }
    }

    //
    public interface IInitializeFromConfig4
    {
    }

    [RegisterType("NamedImplementationOfIInitializeFromConfig4", true, typeof(IInitializeFromConfig4))] // named + singleton
    public class InitializeFromConfig4_1 : IInitializeFromConfig4
    {
        public InitializeFromConfig4_1()
        {
            CountCall.Increment("InitializeFromConfig4_1::ctor");
        }
    }

    //
    public interface IInitializeFromConfig5
    {
    }

    [RegisterType("InitializeFromConfig5_1", typeof(IInitializeFromConfig5))]
    public class InitializeFromConfig5_1 : IInitializeFromConfig5
    {
    }

    [RegisterType("InitializeFromConfig5_2", typeof(IInitializeFromConfig5))]
    public class InitializeFromConfig5_2 : IInitializeFromConfig5
    {
    }

    [RegisterType(typeof(IInitializeFromConfig5))]
    public class InitializeFromConfig5_3 : IInitializeFromConfig5
    {
    }
}
