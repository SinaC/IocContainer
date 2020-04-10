using EasyIoc;
using EasyIoc.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EasyIocTest
{
    [TestClass]
    public class ProtectedConstructorTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            IocContainer.Default.Reset();
            CountCall.Reset();
        }

        [TestMethod]
        [ExpectedException(typeof(ResolveException))]
        public void ProtectedParameterlessConstructor_NoInheritance_1_Resolve_Test()
        {
            IocContainer.Default.RegisterType<IProtectedConstructor1, ProtectedConstructor1>();

            IocContainer.Default.Resolve<IProtectedConstructor1>();
        }

        [TestMethod]
        public void ProtectedParameterlessConstructor_SingleInheritance_1_Resolve_Test()
        {
            IocContainer.Default.RegisterType<IProtectedConstructor1, ProtectedConstructor1_2>();

            IocContainer.Default.Resolve<IProtectedConstructor1>(); // implicit public ctor
        }

        [TestMethod]
        public void ProtectedParameterlessConstructor_SingleInheritance_2_Resolve_Test()
        {
            IocContainer.Default.RegisterType<IProtectedConstructor1, ProtectedConstructor1_3>();
            IocContainer.Default.RegisterType<IProtectedConstructor2, ProtectedConstructor2_1>();

            IocContainer.Default.Resolve<IProtectedConstructor1>(); // will use ctor(IProtectedConstructor2)
        }

        [TestMethod]
        [ExpectedException(typeof(ResolveException))]
        public void ProtectedParameterlessConstructor_NoInheritance_2_Resolve_Test()
        {
            IocContainer.Default.RegisterType<IProtectedConstructor1, ProtectedConstructor_1_4>();
            IocContainer.Default.RegisterType<IProtectedConstructor2, ProtectedConstructor2_1>();

            IocContainer.Default.Resolve<IProtectedConstructor1>(); // ctor(IProtectedConstructor2) is protected and ctor(int) cannot be resolved
        }
    }

    //
    public interface IProtectedConstructor1
    {
    }

    public class ProtectedConstructor1 : IProtectedConstructor1
    {
        protected ProtectedConstructor1()
        {
        }
    }

    //
    public class ProtectedConstructor1_2 : ProtectedConstructor1
    {
    }

    //
    public interface IProtectedConstructor2
    {
    }

    public class ProtectedConstructor2_1 : IProtectedConstructor2
    {
    }

    public class ProtectedConstructor1_3 : ProtectedConstructor1
    {
        public ProtectedConstructor1_3(IProtectedConstructor2 _)
        {
        }
    }

    //
    public class ProtectedConstructor_1_4 : IProtectedConstructor1
    {
        protected ProtectedConstructor_1_4(IProtectedConstructor2 _)
        {
        }

        public ProtectedConstructor_1_4(int a)
        {
        }
    }
}
