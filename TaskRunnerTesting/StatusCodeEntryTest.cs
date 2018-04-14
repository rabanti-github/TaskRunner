using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskRunner;

namespace TaskRunnerTesting
{
    [TestClass]
    public class StatusCodeEntryTest
    {
        [TestMethod]
        [Description("Test of the constructor and getters/setters")]
        public void ConstructorTest()
        {
            StatusCodeEntry entry = new StatusCodeEntry("id1",155, Task.Status.success);
            StatusCodeEntry entry2 = new StatusCodeEntry(){Description = "description1"}; 
            Assert.IsTrue(entry.ID == "id1");
            Assert.IsTrue(entry.Code == 155);
            Assert.IsTrue(entry.Status == Task.Status.success);
            Assert.IsTrue(entry2.Description == "description1");
        }

        [TestMethod]
        [Description("Test of the GetNumber() method")]
        public void GetNumberTest()
        {
            StatusCodeEntry entry = new StatusCodeEntry() { Code = 177 };
            Assert.IsTrue(entry.GetNumber() == 177);
        }

    }
}
