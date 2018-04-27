using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskRunner;
using TaskRunner.SubTasks;

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
            Assert.IsTrue(p2.Flag == Parameter.ParamType.USER && p2.Name == "name1" && p2.Value == "value1" &&
                          p2.ParameterType == Parameter.Types.String && p2.Valid == true);
            Assert.IsTrue(p3.Flag == Parameter.ParamType.USER && p3.Name == "NAME" && p3.Value == "text1 text2" &&
                          p3.ParameterType == Parameter.Types.String && p3.Valid == true);
        }

        [TestMethod]
        [Description("Test of the Parse() method")]
        public void ParseTest()
        {
            Parameter p;
            p = Parameter.Parse("-p:s:NAME1A:text1");
            Assert.IsTrue(p.Valid == true && p.ParameterType == Parameter.Types.String && p.Name == "NAME1A" &&
                          p.Value == "text1");
            p = Parameter.Parse("--param:NAME1B:text2");
            Assert.IsTrue(p.Valid == true && p.ParameterType == Parameter.Types.String && p.Name == "NAME1B" &&
                          p.Value == "text2");
            p = Parameter.Parse("-p:s:NAME1C:'text1 text2 text3'");
            Assert.IsTrue(p.Valid == true && p.ParameterType == Parameter.Types.String && p.Name == "NAME1C" &&
                          p.Value == "text1 text2 text3");

            p = Parameter.Parse("-p:b:NAME2A:true");
            Assert.IsTrue(p.Valid == true && p.ParameterType == Parameter.Types.Boolean && p.Name == "NAME2A" &&
                          p.BooleanValue == true);
            p = Parameter.Parse("--param:b:NAME2B:1");
            Assert.IsTrue(p.Valid == false && p.ParameterType == Parameter.Types.Boolean &&
                          p.Name == "NAME2B"); // Invalid
            p = Parameter.Parse("-p:b:NAME2C:TRUE");
            Assert.IsTrue(p.Valid == true && p.ParameterType == Parameter.Types.Boolean && p.Name == "NAME2C" &&
                          p.BooleanValue == true);

            p = Parameter.Parse("-p:n:NAME3A:22");
            Assert.IsTrue(p.Valid == true && p.ParameterType == Parameter.Types.Number && p.Name == "NAME3A" &&
                          p.NumericValue.Equals(22d) == true);
            p = Parameter.Parse("--param:n:NAME3B:-13.5");
            Assert.IsTrue(p.Valid == true && p.ParameterType == Parameter.Types.Number && p.Name == "NAME3B" &&
                          p.NumericValue.Equals(-13.5) == true);
            p = Parameter.Parse("-p:n:NAME3C:min");
            Assert.IsTrue(p.Valid == true && p.ParameterType == Parameter.Types.Number && p.Name == "NAME3C" &&
                          p.NumericValue.Equals(double.MinValue) == true);
            p = Parameter.Parse("-p:n:NAME3D:MAX");
            Assert.IsTrue(p.Valid == true && p.ParameterType == Parameter.Types.Number && p.Name == "NAME3D" &&
                          p.NumericValue.Equals(double.MaxValue) == true);
            p = Parameter.Parse("-p:n:NAME3E:'1.25 e+5'");
            Assert.IsTrue(p.Valid == true && p.ParameterType == Parameter.Types.Number && p.Name == "NAME3E" &&
                          p.NumericValue.Equals(125000) == true);
            p = Parameter.Parse("-p:n:NAME3F:'3.87 E-3'");
            Assert.IsTrue(p.Valid == true && p.ParameterType == Parameter.Types.Number && p.Name == "NAME3F" &&
                          p.NumericValue.Equals(0.00387) == true);
            p = Parameter.Parse("-p:n:NAME3G:'11E4'");
            Assert.IsTrue(p.Valid == true && p.ParameterType == Parameter.Types.Number && p.Name == "NAME3G" &&
                          p.NumericValue.Equals(110000) == true);
            p = Parameter.Parse("-p:n:NAME3H:'X22'");
            Assert.IsTrue(p.Valid == false && p.ParameterType == Parameter.Types.Number &&
                          p.Name == "NAME3H"); // Invalid


            p = Parameter.Parse("--param:d:NAME4A:7/28/2009");
            Assert.IsTrue(p.Valid == true && p.ParameterType == Parameter.Types.DateTime && p.Name == "NAME4A" &&
                          p.DateTimeValue.Equals(new DateTime(2009, 7, 28)) == true);
            p = Parameter.Parse("-p:d:NAME4B:'2009-07-28 5:23:15 AM'");
            Assert.IsTrue(p.Valid == true && p.ParameterType == Parameter.Types.DateTime && p.Name == "NAME4B" &&
                          p.DateTimeValue.Equals(new DateTime(2009, 7, 28, 5, 23, 15)) == true);
            p = Parameter.Parse("-p:d:NAME4C:'13.05.2019 14:05:01'");
            Assert.IsTrue(p.Valid == true && p.ParameterType == Parameter.Types.DateTime && p.Name == "NAME4C" &&
                          p.DateTimeValue.Equals(new DateTime(2019, 5, 13, 14, 5, 1)) == true);
            p = Parameter.Parse("-p:d:NAME4D:37.05.201");
            Assert.IsTrue(p.Valid == false && p.ParameterType == Parameter.Types.DateTime &&
                          p.Name == "NAME4D"); // Invalid

            p = Parameter.Parse("-p:NAME5A Invalid");
            Assert.IsTrue(p.Valid == false && p.ParameterType == Parameter.Types.String); // Invalid
            p = Parameter.Parse("-p:s:NAME5B----");
            Assert.IsTrue(p.Valid == false && p.ParameterType == Parameter.Types.String); // Invalid
            p = Parameter.Parse("-p:n:NAME5C");
            Assert.IsTrue(p.Valid == false && p.ParameterType == Parameter.Types.Number); // Invalid
            p = Parameter.Parse("-p:b:");
            Assert.IsTrue(p.Valid == false && p.ParameterType == Parameter.Types.Boolean); // Invalid
            p = Parameter.Parse("-p:b");
            Assert.IsTrue(p.Valid == false &&
            p.ParameterType == Parameter.Types.String); // Invalid (cannot determine boolean)
            p = Parameter.Parse("-p");
            Assert.IsTrue(p.Valid == false && p.ParameterType == Parameter.Types.String); // Invalid
            p = Parameter.Parse("");
            Assert.IsTrue(p.Valid == false && p.ParameterType == Parameter.Types.String); // Invalid
        }

        [TestMethod]
        [Description("Test of the RegisterSystemParameters() method")]
        public void RegisterSystemParametersTest()
        {
            try
            {
                Parameter.UserParameters.Clear();
                Parameter.SystemParameters.Clear();
                Parameter.RegisterSystemParameters();
                Assert.IsTrue(Parameter.SystemParameters.Count > 0 && Parameter.UserParameters.Count == 0);
                string key = Parameter.SysParam.ENV_MAX_TASK_ITERATIONS.ToString();
                Assert.IsTrue(Parameter.SystemParameters.ContainsKey(key) &&
                              Parameter.SystemParameters[key].ParameterType == Parameter.Types.Number &&
                              Parameter.SystemParameters[key].NumericValue.Equals(10) &&
                              Parameter.SystemParameters[key].Valid == true);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestMethod]
        [Description("Test of the RegisterSystemParameters() method for errors")]
        public void RegisterSystemParametersTest2()
        {
            try
            {
                Parameter.SystemParameters.Clear();
                Parameter.RegisterSystemParameters();
                int number = Parameter.SystemParameters.Count;
                Parameter.RegisterSystemParameters();
                Assert.IsTrue(Parameter.SystemParameters.Count == number);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestMethod]
        [Description("Test of the AddUserParameters() method")]
        public void AddUserParametersTest()
        {
            try
            {
                Parameter.UserParameters.Clear();
                Parameter.SystemParameters.Clear();
                Parameter p = Parameter.Parse("-p:s:USER_PARAM:text1");
                bool check = Parameter.AddUserParameter(p, true);
                Assert.IsTrue(Parameter.SystemParameters.Count == 0 && Parameter.UserParameters.Count == 1 &&
                              check == true);
                Assert.IsTrue(Parameter.UserParameters.ContainsKey("USER_PARAM") &&
                              Parameter.UserParameters["USER_PARAM"].Value == "text1" &&
                              Parameter.UserParameters["USER_PARAM"].Valid == true &&
                              Parameter.UserParameters["USER_PARAM"].ParameterType == Parameter.Types.String);

            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }


        [TestMethod]
        [Description("Test errors of the AddUserParameters() method")]
        public void AddUserParametersTest2()
        {
            Parameter.UserParameters.Clear();
            Parameter.SystemParameters.Clear();
            Parameter p = Parameter.Parse("-p:s:@PARAM:text1"); // Illegal characters in param name
            bool check = Parameter.AddUserParameter(p, true);
            Assert.IsFalse(check);
            p = Parameter.Parse("-p:n:PARAM1:x15"); // Attempt of assigning invalid variable
            check = Parameter.AddUserParameter(p, true);
            Assert.IsFalse(check);
            Parameter.RegisterSystemParameters();
            p = Parameter.Parse("-p:n:"+ Parameter.SysParam.TASK_ALL_NUMBER_SUCCESS.ToString()+ ":55"); // illegal overwriting of SYS param
            check = Parameter.AddUserParameter(p, true);
            Assert.IsFalse(check);
            p = Parameter.Parse("-p:s:" + Parameter.SysParam.ENV_MAX_TASK_ITERATIONS.ToString() + ":test"); // wrong ENV data type
            check = Parameter.AddUserParameter(p, true);
            Assert.IsFalse(check);
        }

        [TestMethod]
        [Description("Test overwriting in the AddUserParameters() method")]
        public void AddUserParametersTest3()
        {
            Parameter.UserParameters.Clear();
            Parameter.SystemParameters.Clear();
            Parameter p = Parameter.Parse("-p:s:USER_PARAM:text1");
            bool check = Parameter.AddUserParameter(p, true);
            Parameter.Types type = Parameter.UserParameters["USER_PARAM"].ParameterType;
            string value = Parameter.UserParameters["USER_PARAM"].Value;
            p = Parameter.Parse("-p:n:USER_PARAM:55");
            check = Parameter.AddUserParameter(p, true);
            Parameter.Types type2 = Parameter.UserParameters["USER_PARAM"].ParameterType;
            string value2 = Parameter.UserParameters["USER_PARAM"].NumericValue.ToString();
            Assert.IsTrue(check == true && value != value2 && type != type2);
            Assert.IsTrue(value2 == "55" && type2 == Parameter.Types.Number);

            Parameter.RegisterSystemParameters(); // Overwrite environment variable
            p = Parameter.Parse("-p:n:" + Parameter.SysParam.ENV_MAX_TASK_ITERATIONS.ToString() + ":23");
            check = Parameter.AddUserParameter(p, true);
            Assert.IsTrue(check == true && Parameter.SystemParameters[Parameter.SysParam.ENV_MAX_TASK_ITERATIONS.ToString()].NumericValue.Equals(23) && Parameter.SystemParameters[Parameter.SysParam.ENV_MAX_TASK_ITERATIONS.ToString()].Flag == Parameter.ParamType.ENV);
        }

        [TestMethod]
        [Description("Test of the RegisterTaskIterations() method")]
        public void RegisterTaskIterationsTest()
        {
            Task t = new Task();
            t.TaskID = "taskId1";
            Parameter.TaskIterations.Clear();
            Parameter.RegisterTaskIterations(t);
            Assert.IsTrue(Parameter.TaskIterations.Count == 1 && Parameter.TaskIterations["taskId1"] == 0);
            t = new Task();
            t.TaskID = "taskId1"; // 2nd attempt
            Parameter.RegisterTaskIterations(t);
            Assert.IsTrue(Parameter.TaskIterations.Count == 1 && Parameter.TaskIterations["taskId1"] == 0); // should skip 2nd attempt
            Task t2 = new Task();
            t2.TaskID = "taskId2";
            Parameter.RegisterTaskIterations(t2);
            Assert.IsTrue(Parameter.TaskIterations.Count == 2 && Parameter.TaskIterations["taskId2"] == 0);
        }

        [TestMethod]
        [Description("Test of the RegisterTaskIterations() method with sub-tasks")]
        public void AddTaskIterationsTest2()
        {
            Task t = new Task();
            t.TaskID = "taskId1";
            SubTask s1 = new DummyTask() {Name = "subtask1", ParentTask = t};
            SubTask s2 = new DummyTask() { Name = "subtask2", ParentTask = t };
            t.Items.Add(s1);
            t.Items.Add(s2);
            string sid1 = s1.SubTaskID;
            string sid2 = s2.SubTaskID;
            
            Parameter.TaskIterations.Clear();
            Parameter.SubTaskIterations.Clear();

            Parameter.RegisterTaskIterations(t);
            Assert.IsTrue(Parameter.TaskIterations.Count == 1 && Parameter.TaskIterations["taskId1"] == 0 && Parameter.SubTaskIterations.Count == 2 && Parameter.SubTaskIterations[sid1] == 0 && Parameter.SubTaskIterations[sid2] == 0);
            t = new Task();
            t.TaskID = "taskId1"; // 2nd attempt
            Parameter.RegisterTaskIterations(t);
            Assert.IsTrue(Parameter.TaskIterations.Count == 1 && Parameter.TaskIterations["taskId1"] == 0 && Parameter.SubTaskIterations.Count == 2 && Parameter.SubTaskIterations[sid1] == 0 && Parameter.SubTaskIterations[sid2] == 0); // should skip 2nd attempt

        }

        [TestMethod]
        [Description("Test of the CheckTaskIteration() method")]
        public void CheckTaskIterationTest()
        {
            Parameter.TaskIterations.Clear();
            Parameter.SubTaskIterations.Clear();
            Parameter.RegisterSystemParameters();
            Task t = new Task();
            t.TaskID = "taskId1";
            Parameter.RegisterTaskIterations(t);
            Assert.IsTrue(Parameter.TaskIterations.ContainsKey("taskId1") && Parameter.TaskIterations["taskId1"] == 0);
            Parameter.CheckTaskIteration(t.TaskID, true);
            bool check = Parameter.CheckTaskIteration(t.TaskID, true);
            Assert.IsTrue(check == true && Parameter.TaskIterations["taskId1"] == 2);
        }

        [TestMethod]
        [Description("Test of the CheckTaskIteration() method for errors")]
        public void CheckTaskIterationTest2()
        {
            Parameter.TaskIterations.Clear();
            Parameter.SubTaskIterations.Clear();
            Parameter.RegisterSystemParameters();
            Task t = new Task();
            t.TaskID = "taskId1";
            Parameter.RegisterTaskIterations(t);
            bool check= Parameter.CheckTaskIteration("taskId2", true);
            Assert.IsTrue(check == false && Parameter.TaskIterations.Count == 1 && Parameter.TaskIterations["taskId1"] == 0);

            Parameter.UpdateSystemParameter(Parameter.SysParam.ENV_MAX_TASK_ITERATIONS, 3);
            check = Parameter.CheckTaskIteration("taskId1", true);
            Assert.IsTrue(check);
            check = Parameter.CheckTaskIteration("taskId1", true);
            Assert.IsTrue(check);
            check = Parameter.CheckTaskIteration("taskId1", true);
            Assert.IsFalse(check);
        }

        [TestMethod]
        [Description("Test of the CheckSubTaskIteration() method")]
        public void CheckSubTaskIterationTest()
        {
            Parameter.TaskIterations.Clear();
            Parameter.SubTaskIterations.Clear();
            Parameter.RegisterSystemParameters();
            Task t = new Task();
            t.TaskID = "taskId1";
            SubTask s1 = new DummyTask() { Name = "subtask1", ParentTask = t };
            SubTask s2 = new DummyTask() { Name = "subtask2", ParentTask = t };
            t.Items.Add(s1);
            t.Items.Add(s2);
            string sid1 = s1.SubTaskID;
            string sid2 = s2.SubTaskID;
            Parameter.RegisterTaskIterations(t);
            
            Assert.IsTrue(Parameter.SubTaskIterations.ContainsKey(sid1) && Parameter.SubTaskIterations.ContainsKey(sid2) && Parameter.SubTaskIterations[sid1] == 0 && Parameter.SubTaskIterations[sid2] == 0);
            Parameter.CheckSubTaskIteration(sid1, true);
            bool check = Parameter.CheckSubTaskIteration(sid1, true);
            bool check2 = Parameter.CheckSubTaskIteration(sid2, true);
            Assert.IsTrue(check == true && check2 == true && Parameter.SubTaskIterations[sid1] == 2 && Parameter.SubTaskIterations[sid2] == 1);
        }

        [TestMethod]
        [Description("Test of the CheckSubTaskIteration() method for errors")]
        public void CheckSubTaskIterationTest2()
        {
            Parameter.TaskIterations.Clear();
            Parameter.SubTaskIterations.Clear();
            Parameter.RegisterSystemParameters();
            Task t = new Task();
            t.TaskID = "taskId1";
            SubTask s1 = new DummyTask() { Name = "subtask1", ParentTask = t };
            SubTask s2 = new DummyTask() { Name = "subtask2", ParentTask = t };
            t.Items.Add(s1);
            t.Items.Add(s2);
            string sid1 = s1.SubTaskID;
            string sid2 = s2.SubTaskID;

            Parameter.RegisterTaskIterations(t);
            bool check = Parameter.CheckSubTaskIteration("subtaskXYZ", true);
            Assert.IsTrue(check == false && Parameter.SubTaskIterations.Count == 2 && Parameter.SubTaskIterations[sid1] == 0 && Parameter.SubTaskIterations[sid2] == 0);

            Parameter.UpdateSystemParameter(Parameter.SysParam.ENV_MAX_SUBTASK_ITERATIONS, 3);
            check = Parameter.CheckSubTaskIteration(sid2, true);
            Assert.IsTrue(check);
            check = Parameter.CheckSubTaskIteration(sid2, true);
            Assert.IsTrue(check);
            check = Parameter.CheckSubTaskIteration(sid2, true);
            Assert.IsFalse(check);
            check = Parameter.CheckSubTaskIteration(sid1, true); // Should not have an effect on other sub-tasks
            Assert.IsTrue(check);
        }

        [TestMethod]
        [Description("Test of the GetParameter() method")]
        public void GetParameterTest()
        {
            Parameter.UserParameters.Clear();
            Parameter.SystemParameters.Clear();
            Parameter.RegisterSystemParameters();
            Parameter.AddUserParameter(new Parameter("-p:s:NAME1:text1"), true);
            Parameter p = Parameter.GetParameter("NAME1", true);
            Assert.IsNotNull(p);
            Assert.IsTrue(p.Name == "NAME1" && p.Value == "text1" && p.Valid == true);
            p = Parameter.GetParameter("NAME2", true);
            Assert.IsNotNull(p);
            Assert.IsTrue(p.Name == "NAME2" && p.Value == "" && p.Valid == false);
            p = Parameter.GetParameter(Parameter.SysParam.ENV_MAX_TASK_ITERATIONS.ToString(), true);
            Assert.IsNotNull(p);
            Assert.IsTrue(p.Name == Parameter.SysParam.ENV_MAX_TASK_ITERATIONS.ToString() && p.NumericValue.Equals(10));
        }

        [TestMethod]
        [Description("Test of the GetUserParameter() method")]
        public void GetUserParameterTest()
        {
            Parameter.UserParameters.Clear();
            Parameter.SystemParameters.Clear();
            Parameter.RegisterSystemParameters();
            Parameter.AddUserParameter(new Parameter("-p:s:NAME1:text1"), true);
            Parameter p = Parameter.GetUserParameter("NAME1", true);
            Assert.IsNotNull(p);
            Assert.IsTrue(p.Name == "NAME1" && p.Value == "text1" && p.Valid == true);
            p = Parameter.GetUserParameter("NAME2", true);
            Assert.IsNotNull(p);
            Assert.IsTrue(p.Name == "NAME2" && p.Value == "" && p.Valid == false);
            p = Parameter.GetUserParameter(Parameter.SysParam.ENV_MAX_TASK_ITERATIONS.ToString(), true); // Should not return the value since not a user parameter
            Assert.IsNotNull(p);
            Assert.IsTrue(p.Name == Parameter.SysParam.ENV_MAX_TASK_ITERATIONS.ToString() && p.Value == "" && p.Valid == false);
        }

        [TestMethod]
        [Description("Test of the GetSystemParameter() method")]
        public void GetSystemParameterTest()
        {
            Parameter.UserParameters.Clear();
            Parameter.SystemParameters.Clear();
            Parameter.RegisterSystemParameters();
            Parameter.AddUserParameter(new Parameter("-p:s:NAME1:text1"), true);
            Parameter p = Parameter.GetSystemParameter(Parameter.SysParam.ENV_MAX_TASK_ITERATIONS.ToString());
            Assert.IsNotNull(p);
            Assert.IsTrue(p.Name == Parameter.SysParam.ENV_MAX_TASK_ITERATIONS.ToString() && p.NumericValue.Equals(10) && p.Valid == true);
            p = Parameter.GetSystemParameter(Parameter.SysParam.ENV_MAX_SUBTASK_ITERATIONS);
            Assert.IsNotNull(p);
            Assert.IsTrue(p.Name == Parameter.SysParam.ENV_MAX_SUBTASK_ITERATIONS.ToString() && p.NumericValue.Equals(10) && p.Valid == true);
            p = Parameter.GetSystemParameter("SYSPARAM1");
            Assert.IsNotNull(p);
            Assert.IsTrue(p.Name == "SYSPARAM1" && p.Value == "" && p.Valid == false);
            p = Parameter.GetSystemParameter("NAME1"); // Should not return the value since not a system parameter
            Assert.IsNotNull(p);
            Assert.IsTrue(p.Name == "NAME1" && p.Value == "" && p.Valid == false);
        }

        [TestMethod]
        [Description("Test of the special system parameter SYSTEM_DATE_NOW")]
        public void SystemDateNowTest()
        {
            Parameter.SystemParameters.Clear();
            Parameter.RegisterSystemParameters();
            Parameter p = Parameter.GetSystemParameter(Parameter.SysParam.SYSTEM_TIME_NOW);
            DateTime now = DateTime.Now;
            long t1 = p.DateTimeValue.Ticks;
            long t2 = now.Ticks;
            long c;
            if (t1 >= t2)
            {
                c = t1 - t2;
            }
            else
            {
                c = t2 - t1;
            }
            Assert.IsTrue(p.Valid == true && c < 1000000); // difference of 0.1 sec at most
        }

        [TestMethod]
        [Description("Test of the IncreaseSystemParameterNumber() method")]
        public void IncreaseSystemParameterNumberNumberTest()
        {
            Parameter.SystemParameters.Clear();
            Parameter.RegisterSystemParameters();
            Parameter p = Parameter.GetSystemParameter(Parameter.SysParam.ENV_MAX_TASK_ITERATIONS);
            double number1 = p.NumericValue;
            Parameter.IncreaseSystemParameterNumber(Parameter.SysParam.ENV_MAX_TASK_ITERATIONS);
            Parameter.IncreaseSystemParameterNumber(Parameter.SysParam.ENV_MAX_TASK_ITERATIONS);
            Parameter p2 = Parameter.GetSystemParameter(Parameter.SysParam.ENV_MAX_TASK_ITERATIONS);
            double number2 = p2.NumericValue;
            Assert.IsTrue(number2.Equals(number1 + 2));
        }

        [TestMethod]
        [Description("Test of the IncreaseSystemParameterNumber() method, since called from that method, also UpdateSystemParameter() for errors")]
        public void IncreaseSystemParameterNumberNumberTest2()
        {
            Parameter.SystemParameters.Clear(); // Unregister all
            try
            {
                Parameter.IncreaseSystemParameterNumber(Parameter.SysParam.ENV_MAX_SUBTASK_ITERATIONS); // Does not cause an update since not registered yet
                Parameter.RegisterSystemParameters();
                Parameter.IncreaseSystemParameterNumber(Parameter.SysParam.SUBTASK_LAST_TIME_END);
                Parameter p = Parameter.GetSystemParameter(Parameter.SysParam.ENV_MAX_SUBTASK_ITERATIONS);
                Assert.IsTrue(p.NumericValue.Equals(10)); // Should not be changed
            }
            catch (Exception e)
            {
               Assert.Fail();
            }
        }

        [TestMethod]
        [Description("Test of the UpdateSystemParameter() method")]
        public void UpdateSystemParameterTest()
        {
            Parameter.SystemParameters.Clear();
            Parameter.RegisterSystemParameters();
            Parameter p = Parameter.GetSystemParameter(Parameter.SysParam.SUBTASK_ALL_NUMBER_FAIL);
            double number1 = p.NumericValue;
            Parameter.UpdateSystemParameter(Parameter.SysParam.SUBTASK_ALL_NUMBER_FAIL, 22);
            Parameter p2 = Parameter.GetSystemParameter(Parameter.SysParam.SUBTASK_ALL_NUMBER_FAIL);
            double number2 = p2.NumericValue;
            Assert.IsTrue(number1.Equals(0) && number2.Equals(22));

            p = Parameter.GetSystemParameter(Parameter.SysParam.TASK_LAST_LOGGING_SUPPERSS);
            bool boolean1 = p.BooleanValue;
            Parameter.UpdateSystemParameter(Parameter.SysParam.TASK_LAST_LOGGING_SUPPERSS, true);
            p2 = Parameter.GetSystemParameter(Parameter.SysParam.TASK_LAST_LOGGING_SUPPERSS);
            bool boolean2 = p2.BooleanValue;
            Assert.IsTrue(boolean1 == false && boolean2 == true);

            p = Parameter.GetSystemParameter(Parameter.SysParam.SUBTASK_LAST_TIME_END);
            DateTime date1 = p.DateTimeValue;
            Parameter.UpdateSystemParameter(Parameter.SysParam.SUBTASK_LAST_TIME_END, new DateTime(2018,4,1,22,22,22));
            p2 = Parameter.GetSystemParameter(Parameter.SysParam.SUBTASK_LAST_TIME_END);
            DateTime date2 = p2.DateTimeValue;
            Assert.IsTrue(date1 != date2  && date2.Equals(new DateTime(2018, 4, 1, 22, 22, 22)));

            p = Parameter.GetSystemParameter(Parameter.SysParam.ENV_FLAG);
            string string1 = p.Value;
            Parameter.UpdateSystemParameter(Parameter.SysParam.ENV_FLAG, "value1");
            p2 = Parameter.GetSystemParameter(Parameter.SysParam.ENV_FLAG);
            string string2 = p2.Value;
            Assert.IsTrue(string1 == "null" && string2 == "value1");
        }

        [TestMethod]
        [Description("Test of the ResetSubTaskParameters() method")]
        public void ResetSubTaskParametersTest()
        {
            // TODO: The necessity / purpose of that method must be evaluated 
        }

        [TestMethod]
        [Description("Test of the ResetSubTaskParameters() method")]
        public void ResetTaskParametersTest()
        {
            // TODO: The necessity / purpose of that method must be evaluated 
        }

    }
}
