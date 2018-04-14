using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskRunner;

namespace TaskRunnerTesting
{
    [TestClass]
    public class ParameterTest
    {
        [TestMethod]
        [Description("Test of the constructor and getters/setters")]
        public void ConstructorTest()
        {
            Parameter p1 = new Parameter();
            Parameter p2 = new Parameter("name1", "value1");
            Parameter p3 = new Parameter("--param:s:NAME:\"text1 text2\"");
            Assert.IsTrue(p1.Flag == Parameter.ParamType.USER && p1.Valid == false);
            Assert.IsTrue(p2.Flag == Parameter.ParamType.USER && p2.Name == "name1" && p2.Value == "value1" && p2.ParameterType == Parameter.Types.String && p2.Valid == true);
            Assert.IsTrue(p3.Flag == Parameter.ParamType.USER && p3.Name == "NAME" && p3.Value == "text1 text2" && p3.ParameterType == Parameter.Types.String && p3.Valid == true);
        }

        [TestMethod]
        [Description("Test of the Parse() method")]
        public void ParseTest()
        {
            Parameter p1 = Parameter.Parse("-p:s:NAME1:text1");
            Parameter p2 = Parameter.Parse("--param:NAME2:text2");
            Parameter p3 = Parameter.Parse("-p:s:NAME3:'text1 text2 text3'");
            Parameter p4 = Parameter.Parse("-p:b:NAME4:true");
            Parameter p5 = Parameter.Parse("--param:b:NAME5:1");
            Parameter p6 = Parameter.Parse("-p:b:NAME6:TRUE");
            Parameter p7 = Parameter.Parse("-p:n:NAME7:22");
            Parameter p8 = Parameter.Parse("--param:n:NAME8:-13.5");
            string minDouble = double.MaxValue.ToString("R");
            Parameter p9 = Parameter.Parse("-p:n:NAME9:" + minDouble);
            Parameter p10 = Parameter.Parse("--param:d:NAME10:7/28/2009");
            Parameter p11 = Parameter.Parse("-p:d:NAME11:'2009-07-28 5:23:15 AM'");

            Assert.IsTrue(p1.Valid == true && p1.ParameterType == Parameter.Types.String && p1.Name == "NAME1" && p1.Value == "text1");
            Assert.IsTrue(p2.Valid == true && p2.ParameterType == Parameter.Types.String && p2.Name == "NAME2" && p2.Value == "text2");
            Assert.IsTrue(p3.Valid == true && p3.ParameterType == Parameter.Types.String && p3.Name == "NAME3" && p3.Value == "text1 text2 text3");
            Assert.IsTrue(p4.Valid == true && p4.ParameterType == Parameter.Types.Boolean && p4.Name == "NAME4" && p4.BooleanValue == true);
            Assert.IsTrue(p5.Valid == false && p5.ParameterType == Parameter.Types.Boolean && p5.Name == "NAME5"); // Invalid
            Assert.IsTrue(p6.Valid == true && p6.ParameterType == Parameter.Types.Boolean && p6.Name == "NAME6" && p6.BooleanValue == true);
            Assert.IsTrue(p7.Valid == true && p7.ParameterType == Parameter.Types.Number && p7.Name == "NAME7" && p7.NumericValue.Equals(22d) == true);
            Assert.IsTrue(p8.Valid == true && p8.ParameterType == Parameter.Types.Number && p8.Name == "NAME8" && p8.NumericValue.Equals(-13.5) == true);
            Assert.IsTrue(p9.Valid == true && p9.ParameterType == Parameter.Types.Number && p9.Name == "NAME9" && p9.NumericValue.Equals(double.MaxValue) == true);
        }

    }
}
