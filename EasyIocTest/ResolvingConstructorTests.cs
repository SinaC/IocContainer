using System.Linq;
using System.Threading.Tasks;
using EasyIoc;
using EasyIoc.Attributes;
using EasyIoc.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EasyIocTest
{
    [TestClass]
    public class ResolvingConstructorTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            IocContainer.Default.Reset();
            CountCall.Reset();
        }

        [TestMethod]
        public void ResolvingConstructorAttributeCalled_Test_1()
        {
            IocContainer.Default.InitializeFromConfig();

            IResolvingConstructor1 instance = IocContainer.Default.Resolve<IResolvingConstructor1>();

            Assert.IsNotNull(instance);
            Assert.IsInstanceOfType(instance, typeof(ResolvingConstructor1));
            Assert.AreEqual(1, CountCall.Count(ResolvingConstructor1.CtorCall));
        }

        [TestMethod]
        public void ResolvingConstructorAttributeCalled_Test_2()
        {
            IocContainer.Default.InitializeFromConfig();

            IResolvingConstructor2 instance = IocContainer.Default.Resolve<IResolvingConstructor2>();

            Assert.IsNotNull(instance);
            Assert.IsInstanceOfType(instance, typeof(ResolvingConstructor2));
            Assert.AreEqual(1, CountCall.Count(ResolvingConstructor2.ResolvingConstructorCtorCall));
            Assert.AreEqual(0, CountCall.Count(ResolvingConstructor2.ParameterLessCtorCall));
            Assert.AreEqual(1, CountCall.Count(ResolvingConstructor1.CtorCall));
        }

        [TestMethod]
        public void ResolvingConstructorAttributeCalled_Test_3()
        {
            IocContainer.Default.InitializeFromConfig();

            IResolvingConstructor3 instance = IocContainer.Default.Resolve<IResolvingConstructor3>();

            Assert.IsNotNull(instance);
            Assert.IsInstanceOfType(instance, typeof(ResolvingConstructor3));
            Assert.AreEqual(1, CountCall.Count(ResolvingConstructor3.ResolvingConstructor2CtorCall));
            Assert.AreEqual(0, CountCall.Count(ResolvingConstructor3.ResolvingConstructor1CtorCall));
            Assert.AreEqual(1, CountCall.Count(ResolvingConstructor2.ResolvingConstructorCtorCall));
            Assert.AreEqual(0, CountCall.Count(ResolvingConstructor2.ParameterLessCtorCall));
            Assert.AreEqual(1, CountCall.Count(ResolvingConstructor1.CtorCall)); // singleton
        }

        [TestMethod]
        [ExpectedException(typeof(ResolveException))]
        public void ResolvingConstructorAttribute_Test_4()
        {
            IocContainer.Default.InitializeFromConfig();

            IResolvingConstructor4_3 instance = IocContainer.Default.Resolve<IResolvingConstructor4_3>(); // will not be able to find a resolvable constructor because IResolvingConstructor2 is not registered anonymously
        }

        [TestMethod]
        public void ResolvingConstructorAttribute_Test_5()
        {
            IocContainer.Default.InitializeFromConfig();

            IResolvingConstructor5_3 instance = IocContainer.Default.Resolve<IResolvingConstructor5_3>();

            Assert.IsNotNull(instance);
            Assert.IsInstanceOfType(instance, typeof(ResolvingConstructor5_3));
            Assert.AreEqual(1, CountCall.Count(ResolvingConstructor5_3.CtorCall));
            Assert.AreEqual(1, CountCall.Count(ResolvingConstructor5_2.CtorCall));
            Assert.AreEqual(1, CountCall.Count(ResolvingConstructor5_1.CtorCall));
        }

        [TestMethod]
        public void ResolvingConstructorAttribute_Test_5_2()
        {
            IocContainer.Default.InitializeFromConfig();

            IResolvingConstructor5_3 instance1 = IocContainer.Default.Resolve<IResolvingConstructor5_3>();
            IResolvingConstructor5_3 instance2 = IocContainer.Default.Resolve<IResolvingConstructor5_3>();

            Assert.IsNotNull(instance1);
            Assert.IsInstanceOfType(instance1, typeof(ResolvingConstructor5_3));
            Assert.IsNotNull(instance2);
            Assert.IsInstanceOfType(instance2, typeof(ResolvingConstructor5_3));
            Assert.AreNotSame(instance2, instance1);
            Assert.AreEqual(2, CountCall.Count(ResolvingConstructor5_3.CtorCall));
            Assert.AreEqual(1, CountCall.Count(ResolvingConstructor5_2.CtorCall));
            Assert.AreEqual(1, CountCall.Count(ResolvingConstructor5_1.CtorCall));
        }

        [TestMethod]
        public void ResolvingConstructorAttribute_Test_5_3()
        {
            IocContainer.Default.InitializeFromConfig();
            const int taskCount = 10000;

            //List<Task> tasks = Enumerable.Range(0, taskCount).Select(x => new Task(() => IocContainer.Default.Resolve<IResolvingConstructor5_3>())).ToList();
            //foreach(Task task in tasks)
            //    task.Start();
            //Task.WaitAll(tasks.ToArray());
            Parallel.ForEach(Enumerable.Range(0, taskCount), x => IocContainer.Default.Resolve<IResolvingConstructor5_3>());

            Assert.AreEqual(taskCount, CountCall.Count(ResolvingConstructor5_3.CtorCall));
            Assert.AreEqual(1, CountCall.Count(ResolvingConstructor5_2.CtorCall));
            Assert.AreEqual(1, CountCall.Count(ResolvingConstructor5_1.CtorCall));
        }
    }

    //
    public interface IResolvingConstructor1
    {
    }

    [RegisterType(true, typeof(IResolvingConstructor1))]
    public class ResolvingConstructor1 : IResolvingConstructor1
    {
        public const string CtorCall = "ResolvingConstructor1::ctor";

        [ResolvingConstructor]
        public ResolvingConstructor1()
        {
            CountCall.Increment(CtorCall);
        }
    }

    //
    public interface IResolvingConstructor2
    {
    }

    [RegisterType(typeof(IResolvingConstructor2))]
    public class ResolvingConstructor2 : IResolvingConstructor2
    {
        public const string ParameterLessCtorCall = "ResolvingConstructor2::ParameterLessCtorCall";
        public const string ResolvingConstructorCtorCall = "ResolvingConstructor2::ResolvingConstructorCtorCall";

        public ResolvingConstructor2()
        {
            CountCall.Increment(ParameterLessCtorCall);
        }

        [ResolvingConstructor]
        public ResolvingConstructor2(IResolvingConstructor1 _)
        {
            CountCall.Increment(ResolvingConstructorCtorCall);
        }
    }

    //
    public interface IResolvingConstructor3
    {
    }

    [RegisterType(typeof(IResolvingConstructor3))]
    public class ResolvingConstructor3 : IResolvingConstructor3
    {
        public const string ParameterLessCtorCall = "ResolvingConstructor3::ParameterLessCtorCall";
        public const string ResolvingConstructor1CtorCall = "ResolvingConstructor3::ResolvingConstructor1CtorCall";
        public const string ResolvingConstructor2CtorCall = "ResolvingConstructor3::ResolvingConstructor2CtorCall";

        public ResolvingConstructor3()
        {
            CountCall.Increment(ParameterLessCtorCall);
        }

        [ResolvingConstructor]
        public ResolvingConstructor3(int index, IResolvingConstructor1 _) // <-- cannot be resolved
        {
            CountCall.Increment(ResolvingConstructor1CtorCall);
        }

        public ResolvingConstructor3(IResolvingConstructor1 i1) // <-- not ResolvingConstructor attribute: lower priority
        {
        }

        [ResolvingConstructor]
        public ResolvingConstructor3(IResolvingConstructor1 i1, IResolvingConstructor2 i2) // <-- this will be the selected ctor
        {
            CountCall.Increment(ResolvingConstructor2CtorCall);
        }
    }

    //
    public interface IResolvingConstructor4_1
    {
    }

    public interface IResolvingConstructor4_2
    {
    }

    public interface IResolvingConstructor4_3
    {
    }

    [RegisterType(typeof(IResolvingConstructor4_1))]
    public class ResolvingConstructor4_1 : IResolvingConstructor4_1
    {
    }

    [RegisterType("ResolvingConstructor4_2", typeof(IResolvingConstructor4_2))]
    public class ResolvingConstructor4_2 : IResolvingConstructor4_2
    {
        [ResolvingConstructor]
        public ResolvingConstructor4_2(IResolvingConstructor4_1 _)
        {
        }
    }

    [RegisterType(typeof(IResolvingConstructor4_3))]
    public class ResolvingConstructor4_3 : IResolvingConstructor4_3
    {
        [ResolvingConstructor]
        public ResolvingConstructor4_3(ResolvingConstructor4_2 _)
        {
        }
    }

    //
    public interface IResolvingConstructor5_1
    {
    }

    public interface IResolvingConstructor5_2
    {
    }

    public interface IResolvingConstructor5_3
    {
    }

    [RegisterType(typeof(IResolvingConstructor5_1))]
    public class ResolvingConstructor5_1 : IResolvingConstructor5_1
    {
        public const string CtorCall = "ResolvingConstructor5_1::ctor";

        public ResolvingConstructor5_1()
        {
            CountCall.Increment(CtorCall);
        }
    }

    [RegisterType(true, typeof(IResolvingConstructor5_2))]
    public class ResolvingConstructor5_2 : IResolvingConstructor5_2
    {
        public const string CtorCall = "ResolvingConstructor5_2::ctor";

        [ResolvingConstructor]
        public ResolvingConstructor5_2(IResolvingConstructor5_1 _)
        {
            CountCall.Increment(CtorCall);
        }
    }

    [RegisterType(typeof(IResolvingConstructor5_3))]
    public class ResolvingConstructor5_3 : IResolvingConstructor5_3
    {
        public const string CtorCall = "ResolvingConstructor5_3::ctor";

        [ResolvingConstructor]
        public ResolvingConstructor5_3(IResolvingConstructor5_2 _)
        {
            CountCall.Increment(CtorCall);
        }
    }
}
