using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskRunner;
using TaskRunner.SubTasks;

namespace TaskRunnerTesting
{
    [TestClass]
    public class TaskTest
    {
        [TestMethod]
        [Description("Test of the EnumerateSubTasks() method")]
        public void EnumerateSubTasksTest()
        {
            List<SubTask> subtasks = Task.EnumerateSubTasks();
            Assert.IsNotNull(subtasks);
            Assert.IsTrue(subtasks.Count > 0);
        }

        [TestMethod]
        [Description("Test of the Constructor(s)")]
        public void ConstructorTest()
        {
            try
            {
                Task t = new Task();
                Assert.IsTrue(t.Items != null && t.Items.Count == 0);
                Assert.IsTrue(t.LogEntries != null && t.LogEntries.Count == 0);
                Assert.IsTrue(t.Enabled == true);
                Assert.IsTrue(string.IsNullOrEmpty(t.TaskID) == false);
            }
            catch (Exception e)
            {
                Assert.Fail();
            }

        }

        [TestMethod]
        [Description("Test of the Serialize() and Deserialize() methods")]
        public void SerializeDeserializeTest()
        {
            try
            {
                Task t = new Task();
                DummyTask dummy = new DummyTask();
                dummy.Name = "sub-task1";
                t.Items.Add(dummy);
                t.TaskName = "task1";
                t.Serialize("serializeDeserializeTest.xml");
                Task t2 = Task.Deserialize("serializeDeserializeTest.xml");
                Assert.IsTrue(t.TaskName == t2.TaskName);
               // Assert.IsTrue(t.TaskID == t2.TaskID); Automatically generated
                Assert.IsTrue(t.Items.Count == t2.Items.Count);
                Assert.IsTrue(t.Items[0].Name == t2.Items[0].Name);
                Task t3 = Task.Deserialize("not_existing_configuration.xml");
                Assert.IsNull(t3);
                using (StreamWriter sw = new StreamWriter("corrupt_config_file.xml"))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf - 8\"?><element>no content</element>");
                    sw.Flush();
                    sw.Close();
                }
                Task t4 = Task.Deserialize("corrupt_config_file.xml");
                Assert.IsNull(t3);
            }
            catch (Exception e)
            {
                Assert.Fail();
            }

        }

        [TestMethod]
        [Description("Test of the CreateDemoFile() method")]
        public void CreateDemoFileTest()
        {
            string fileName = "checkDemoFileTest.xml";
            Task.TaskType type = Task.TaskType.DummyTask;
            string text = Task.CreateDemoFile(type);
            Assert.IsTrue(string.IsNullOrEmpty(text) == false && CheckDemoFile(text, type, false) == true);

            type = Task.TaskType.ControlService;
            Task.CreateDemoFile(fileName, type);
            Assert.IsTrue(string.IsNullOrEmpty(text) == false && CheckDemoFile(fileName, type, true) == true);

            type = Task.TaskType.DelayTask;
            text = Task.CreateDemoFile(type);
            Assert.IsTrue(string.IsNullOrEmpty(text) == false && CheckDemoFile(text, type, false) == true);

            type = Task.TaskType.DeleteRegKey;
            Task.CreateDemoFile(fileName, type);
            Assert.IsTrue(string.IsNullOrEmpty(text) == false && CheckDemoFile(fileName, type, true) == true);

            type = Task.TaskType.DeleteFile;
            text = Task.CreateDemoFile(type);
            Assert.IsTrue(string.IsNullOrEmpty(text) == false && CheckDemoFile(text, type, false) == true);

            type = Task.TaskType.KillProcess;
            Task.CreateDemoFile(fileName, type);
            Assert.IsTrue(string.IsNullOrEmpty(text) == false && CheckDemoFile(fileName, type, true) == true);

            type = Task.TaskType.MetaTask;
            text = Task.CreateDemoFile(type);
            Assert.IsTrue(string.IsNullOrEmpty(text) == false && CheckDemoFile(text, type, false) == true);

            type = Task.TaskType.MixedTask;
            Task.CreateDemoFile(fileName, type);
            Assert.IsTrue(string.IsNullOrEmpty(text) == false && CheckDemoFile(fileName, type, true) == true);

            type = Task.TaskType.StartProgram;
            text = Task.CreateDemoFile(type);
            Assert.IsTrue(string.IsNullOrEmpty(text) == false && CheckDemoFile(text, type, false) == true);

            type = Task.TaskType.WriteLog;
            Task.CreateDemoFile(fileName, type);
            Assert.IsTrue(string.IsNullOrEmpty(text) == false && CheckDemoFile(fileName, type, true) == true); 

        }

        [TestMethod]
        [Description("Test of the CreateDemoFile() method for errors")]
        public void CreateDemoFileTest2()
        {
            try
            {
                Task.TaskType type = Task.TaskType.DummyTask;
                Task.CreateDemoFile(null, type); // does not create a file but shall not raise an exception
                string text = Task.CreateDemoFile((Task.TaskType)99); // Unknown but shall not raise an exception
                using (FileStream fs = new FileStream("checkDemoFileTest2.xml", FileMode.Create))
                {
                    Task.CreateDemoFile("checkDemoFileTest2.xml", Task.TaskType.DummyTask); // Locked stream but shall not raise an exception
                }
            }
            catch (Exception e)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        [Description("Test of the Log() method")]
        public void LogTest()
        {
            Parameter.RegisterSystemParameters(); // Mandatory
            string fileName = "logtest.log";
            string header = "Date\tStatus\tStatus-Code\tTask\tSub-Task\r\n---------------------------------------------------------------------------------------------\r\n";
            string comparison = header;
            Task t = new Task();
            t.TaskName = "task1";
            SubTask s = new DummyTask();
            s.Name = "subtask1";
            t.Items.Add(s);
            Assert.IsTrue(CheckLogFile(fileName, true, t, Task.Status.none, comparison));
            Tuple<int, DateTime, string, string, string> line;
            //List<DateTime>dates = new List<DateTime>();
            //List<int> lines = new List<int>();
            //dates.Add(DateTime.Now);
            //lines.Add(1);
            List<Tuple<int, DateTime, string, string, string>> lines = new List<Tuple<int, DateTime, string, string, string>>();
            lines.Add(new Tuple<int, DateTime, string, string, string>(2,DateTime.Now, "success","task1","subtask1"));
            t.Run(false, false, false, null);
            t.Log(fileName, Task.Status.success);
            Thread.Sleep(2000);
            t.TaskName = "task2";
            lines.Add(new Tuple<int, DateTime, string, string, string>(3, DateTime.Now, "failed", "task2", "subtask1"));
            t.Run(false, false, true, null);
            t.Log(fileName, Task.Status.failed);
            Assert.IsTrue(CheckLogEntries(fileName, lines, 2000));
        }

        [TestMethod]
        [Description("Test of the PrintExecutionCode() method")]
        public void PrintExecutionCodeTest()
        {
            LogEntry e = new LogEntry();
            byte[] bytes = new byte[] { 8, 128, 33, 255, 3, 22, 188 };
            LogEntry entry = new LogEntry() { ExecutionCode = bytes };
            Task t = new Task();
            string code = t.PrintExecutionCode(entry);
            Assert.AreEqual(code, "088021FF0316BC");
        }

        [TestMethod]
        [Description("Test of the Run() method")]
        public void RunTest()
        {
            
            Task t = SetupTask(true);
            DateTime systemStartTime = DateTime.Now;
            ((DummyTask)t.Items[0]).SetRunStatus(Task.Status.success);
            ((DummyTask)t.Items[1]).SetRunStatus(Task.Status.success);
            DateTime executionTime = DateTime.Now;
            Task.Status status = t.Run(false, false, false, null);
            DateTime endTime = DateTime.Now;
            Assert.IsTrue(status == Task.Status.success);
            Assert.IsTrue(MatchDates(Parameter.GetSystemParameter(Parameter.SysParam.SYSTEM_TIME_START).DateTimeValue, systemStartTime, 2000));
            Assert.IsTrue(MatchDates(Parameter.GetSystemParameter(Parameter.SysParam.TASK_LAST_TIME_START).DateTimeValue, executionTime, 2000));
            Assert.IsTrue(MatchDates(Parameter.GetSystemParameter(Parameter.SysParam.SUBTASK_LAST_TIME_END).DateTimeValue, executionTime, 2000));
            Assert.IsTrue(Parameter.GetSystemParameter(Parameter.SysParam.SUBTASK_ALL_NUMBER_SUCCESS).NumericValue.Equals(2));
            Assert.IsTrue(Parameter.GetSystemParameter(Parameter.SysParam.SUBTASK_ALL_NUMBER_TOTAL).NumericValue.Equals(2));
            Assert.IsTrue(Parameter.GetSystemParameter(Parameter.SysParam.SUBTASK_ALL_NUMBER_FAIL).NumericValue.Equals(0));
            Assert.IsTrue(Parameter.GetSystemParameter(Parameter.SysParam.SUBTASK_ALL_SUCCESS_PARTIAL).BooleanValue == true); // Full success means partial success automatically
            Assert.IsTrue(Parameter.GetSystemParameter(Parameter.SysParam.SUBTASK_ALL_SUCCESS).BooleanValue == true);
            Assert.IsTrue(Parameter.GetSystemParameter(Parameter.SysParam.SUBTASK_LAST_SUCCESS).BooleanValue == true);
            Assert.IsTrue(Parameter.GetSystemParameter(Parameter.SysParam.SUBTASK_LAST_SUCCESS_PARTIAL).BooleanValue == true); // Full success means partial success automatically
            Assert.IsTrue(Parameter.GetSystemParameter(Parameter.SysParam.TASK_LAST_SUCCESS).BooleanValue == true);
            Assert.IsTrue(Parameter.GetSystemParameter(Parameter.SysParam.TASK_LAST_SUCCESS_PARTIAL).BooleanValue == true); // Full success means partial success automatically
            Assert.IsTrue(Parameter.GetSystemParameter(Parameter.SysParam.TASK_ALL_NUMBER_SUCCESS).NumericValue.Equals(1));
            Assert.IsTrue(Parameter.GetSystemParameter(Parameter.SysParam.TASK_ALL_NUMBER_FAIL).NumericValue.Equals(0));
            Assert.IsTrue(Parameter.GetSystemParameter(Parameter.SysParam.TASK_ALL_NUMBER_TOTAL).NumericValue.Equals(1));
        }

        private Task SetupTask(bool registerParameters)
        {
            if (registerParameters == true)
            {
                Parameter.SystemParameters.Clear();
                Parameter.RegisterSystemParameters();
            }

            Task t = new Task();
            t.TaskName = "task1";
            SubTask s1 = new DummyTask();
            s1.Name = "subtask1";
            SubTask s2 = new DummyTask();
            s2.Name = "subtask2";
            t.Items.Add(s1);
            t.Items.Add(s2);
            return t;
        }

        private bool CheckLogEntries(string fileName, List<Tuple<int, DateTime, string, string, string>> entries, int dateTolerance)
        {
            try
            {
                string raw = "";
                using (StreamReader sr = new StreamReader(fileName))
                {
                    raw = sr.ReadToEnd();
                }
                char[] delimiters = new[] { '\n', '\r' };
                string[] lines = raw.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                string[] fields;
                int j;
                DateTime parsedDate = new DateTime(1,1,1,1,1,1);
                //TimeSpan span;
                int len = entries.Count;
                for (int i = 0; i < lines.Length; i++)
                {
                    fields = lines[i].Split('\t');
                    for (j = 0; j < len; j++)
                    {
                        if (entries[j].Item1 == i)
                        {
                            if (fields.Length != 5)
                            {
                                return false;
                            }
                            DateTime.TryParseExact(fields[0], Task.DATEFORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate);
                            if (MatchDates(entries[j].Item2, parsedDate, dateTolerance) == false)
                            {
                                return false;
                            }
                            if (fields[1].ToLower() != entries[j].Item3.ToLower()) { return false; }
                            if (fields[3].ToLower() != entries[j].Item4.ToLower()) { return false; }
                            if (fields[4].ToLower() != entries[j].Item5.ToLower()) { return false; }
                        }
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                return false;
            }

        }

        private bool MatchDates(DateTime d1, DateTime d2, int tolerance)
        {
            TimeSpan span = d1 - d2;
            if (Math.Abs(span.Milliseconds) <= tolerance)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool CheckDemoFile(string text, Task.TaskType expectedType, bool asFile)
        {
            string fileName;
            try
            {
                if (asFile == false)
                {
                    fileName = "checkDemoFileTest.xml";
                    using (FileStream fs = new FileStream(fileName, FileMode.Create))
                    {
                        using (StreamWriter sw = new StreamWriter(fs))
                        {
                            sw.Write(text);
                            sw.Flush();
                        }
                    }
                }
                else
                {
                    fileName = text;
                }

                Task t = Task.Deserialize(fileName);
                if (t == null)
                {
                    return false;
                }

                if (t.Type == expectedType)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

        }

        private bool CheckLogFile(string fileName, bool deleteFile, Task t, Task.Status status, string expectedValue)
        {
            try
            {
                if (File.Exists(fileName) && deleteFile == true)
                {
                    File.Delete(fileName);
                }
                t.Log(fileName, status);
                if (File.Exists(fileName) == false)
                {
                    return false;
                }
                string text = "";
                using (StreamReader sr = new StreamReader(fileName))
                {
                    text = sr.ReadToEnd();
                }

                if (text == expectedValue)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

    }
}
