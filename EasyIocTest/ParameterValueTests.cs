using EasyIoc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EasyIocTest
{
    [TestClass]
    public class ParameterValueTests
    {
        [TestMethod]
        public void TestCtor()
        {
            IParameterValue parameterValue = new ParameterValue("joel", this);

            Assert.AreEqual(parameterValue.Name, "joel");
            Assert.AreSame(parameterValue.Value, this);
        }
    }
}
