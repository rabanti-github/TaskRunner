using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskRunner;

namespace TaskRunnerTesting
{
    [TestClass]
    public class LogEntryTest
    {
        [TestMethod]
        [Description("Test of the constructor and getters/setters")]
        public void ConstructorTest()
        {
            DateTime d1 = new DateTime(2018, 4, 4, 4, 4, 4);
            LogEntry entry = new LogEntry()
            {
                ExecutionCode = new byte[] {144, 2, 117},
                ExecutionDate = d1,
                SubTaskName = "subtask1",
                TaskName = "task1"
            };
            Assert.IsTrue(entry.SubTaskName == "subtask1");
            Assert.IsTrue(entry.TaskName == "task1");
            Assert.AreEqual(d1, entry.ExecutionDate);
            Assert.IsTrue(entry.ExecutionCode[0] == 144);
            Assert.IsTrue(entry.ExecutionCode[1] == 2);
            Assert.IsTrue(entry.ExecutionCode[2] == 117);
        }

        [TestMethod]
        [Description("Test of the PrintExecutionCode() method")]
        public void PrintExecutionCodeTest()
        {
            byte[] bytes = new byte[] {8, 128, 33, 255, 3, 22, 188};
            LogEntry entry = new LogEntry() {ExecutionCode = bytes};
            string code = entry.PrintExecutionCode();
            Assert.AreEqual(code, "088021FF0316BC");
        }

        [TestMethod]
        [Description("Test of the GetLogString() method")]
        public void GetLogStringTest()
        {
            DateTime d1 = new DateTime(2018, 4, 6, 8, 10, 12);
            byte[] bytes = new byte[] {9, 128, 33, 255, 3, 22, 188};
            LogEntry entry = new LogEntry()
            {
                ExecutionCode = bytes,
                ExecutionDate = d1,
                TaskName = "task2",
                SubTaskName = "subtask2"
            };
            string line = entry.getLogString(Task.Status.skipped);
            string shouldBe = d1.ToString(Task.DATEFORMAT) + "\tskipped\t098021FF0316BC\ttask2\tsubtask2";
            Assert.AreEqual(line, shouldBe);
        }

        [TestMethod]
        [Description("Test of the InsertCodeByteTest() method")]
        public void InsertCodeByteTest()
        {
            byte[] bytes = new byte[] {8, 128, 33, 255, 3, 22, 188};
            LogEntry entry = new LogEntry() {ExecutionCode = bytes};
            entry.InsertCodeByte(34, 2);
            entry.InsertCodeByte(4, 4);
            string code = entry.PrintExecutionCode();
            Assert.AreEqual(code, "088022FF0416BC");
        }

        [TestMethod]
        [Description("Test of the InsertCodeByteTest() method in case of a wrong index (out of bound)")]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void InsertCodeByteTest2()
        {
            byte[] bytes = new byte[] { 8, 128, 33, 255, 3, 22, 188 };
            LogEntry entry = new LogEntry() { ExecutionCode = bytes };
            entry.InsertCodeByte(1, 10);
        }
    }
}
