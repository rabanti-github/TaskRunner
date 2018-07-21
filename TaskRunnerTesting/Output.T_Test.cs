using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskRunner;

namespace TaskRunnerTesting
{
    [TestClass]
    public class Output_T_Test
    {
        [TestMethod]
        [Description("Test of the constructor")]
        public void ConstructotrTest()
        {
            try
            {
                Output.T t = new Output.T();
                Output.T t2 = new Output.T("value1","description1");
                Assert.IsTrue(t2.Description == "description1" && t2.Value == "value1" && t2.OverrideTagFormatting == false);
                Output.T t3 = new Output.T("value2", "description2", true);
                Assert.IsTrue(t3.Description == "description2" && t3.Value == "value2" && t3.OverrideTagFormatting == true);

                Output.T t4 = new Output.T(null,null);
                Assert.IsTrue(t4.Description == "" && t4.Value == "");
            }
            catch (Exception e)
            {
                Assert.Fail();
            }
            
        }
    }
}
