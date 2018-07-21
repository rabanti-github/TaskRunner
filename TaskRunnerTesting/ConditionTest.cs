using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskRunner;

namespace TaskRunnerTesting
{
    [TestClass]
    public class ConditionTest
    {
        [TestMethod]
        [Description("Test of the constructors")]
        public void ConstructorTest()
        {
            try
            {
                Condition c = new Condition();
                Parameter.SystemParameters.Clear();
                Parameter.RegisterSystemParameters();
                Condition c2 = new Condition("22 >= TASK_ALL_NUMBER_FAIL", "run", "skip", "pre");
                Assert.IsTrue(c2.Evaluate(true) == true && c2.CheckOperation(true) == Condition.ConditionAction.run && c2.CheckType() == Condition.ConditionType.pre);
            }
            catch
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        [Description("Test of the Evaluate() method (generally tested in EvaluationTest)")]
        public void EvaluateTest()
        {
            Condition c = new Condition("true", "run", "skip", "pre");
            Assert.IsTrue(c.Evaluate(true) == true);
            c = new Condition("1 >= 0", "run", "skip", "pre");
            Assert.IsTrue(c.Evaluate(true) == true);
            c = new Condition("1 < 0", "run", "skip", "pre");
            Assert.IsTrue(c.Evaluate(true) == false);
            c = new Condition("x", "run", "skip", "pre");
            Assert.IsTrue(c.Evaluate(true) == false);
            c = new Condition(null, "run", "skip", "pre");
            Assert.IsTrue(c.Evaluate(true) == false);
            c = new Condition("", "run", "skip", "pre");
            Assert.IsTrue(c.Evaluate(true) == false);
        }

        [TestMethod]
        [Description("Test of the ConditionAction() method")]
        public void ConditionActionTest()
        {
            Condition c = new Condition("true", "run", "skip", "pre");
            Assert.IsTrue(c.CheckOperation(true) == Condition.ConditionAction.run);
            c = new Condition("true", "RUN", "skip", "pre");
            Assert.IsTrue(c.CheckOperation(true) == Condition.ConditionAction.run);
            c = new Condition("true", "Skip", "skip", "pre");
            Assert.IsTrue(c.CheckOperation(true) == Condition.ConditionAction.skip);
            c = new Condition("true", "exIT", "skip", "pre");
            Assert.IsTrue(c.CheckOperation(true) == Condition.ConditionAction.exit);
            c = new Condition("true", "Restart_Last_Subtask", "skip", "pre");
            Assert.IsTrue(c.CheckOperation(true) == Condition.ConditionAction.restart_last_subtask);
            c = new Condition("true", "RESTART_TASK", "skip", "pre");
            Assert.IsTrue(c.CheckOperation(true) == Condition.ConditionAction.restart_task);

            c = new Condition("true", "run", "exit", "pre");
            Assert.IsTrue(c.CheckOperation(false) == Condition.ConditionAction.exit);
        }

        [TestMethod]
        [Description("Test of the ConditionAction() method for errors")]
        public void ConditionActionTest2()
        {
            Condition c = new Condition("true", "running", "skip", "pre");
            Assert.IsTrue(c.CheckOperation(true) == Condition.ConditionAction.none);
            c = new Condition("true", null, "skip", "pre");
            Assert.IsTrue(c.CheckOperation(true) == Condition.ConditionAction.none);
            c = new Condition("true", "", "skip", "pre");
            Assert.IsTrue(c.CheckOperation(true) == Condition.ConditionAction.none);
            c = new Condition("true", "restart task", "skip", "pre");
            Assert.IsTrue(c.CheckOperation(true) == Condition.ConditionAction.none);
            c = new Condition("true", null, null, null);
            Assert.IsTrue(c.CheckOperation(true) == Condition.ConditionAction.none);

            c = new Condition("true", "run", "xxx", "pre");
            Assert.IsTrue(c.CheckOperation(false) == Condition.ConditionAction.none);
        }

        [TestMethod]
        [Description("Test of the CheckTypeTest() method")]
        public void CheckTypeTest()
        {
            Condition c = new Condition("true", "run", "skip", "pre");
            Assert.IsTrue(c.CheckType() == Condition.ConditionType.pre);
            c = new Condition("true", "run", "skip", "POST");
            Assert.IsTrue(c.CheckType() == Condition.ConditionType.post);
        }

        [TestMethod]
        [Description("Test of the CheckTypeTest() method for errors")]
        public void CheckTypeTest2()
        {
            Condition c = new Condition("true", "run", "skip", "middle");
            Assert.IsTrue(c.CheckType() == Condition.ConditionType.none);
            c = new Condition("true", "run", "skip", "");
            Assert.IsTrue(c.CheckType() == Condition.ConditionType.none);
            c = new Condition("true", "run", "skip", " pre ");
            Assert.IsTrue(c.CheckType() == Condition.ConditionType.none);
            c = new Condition(null, null, null, null);
            Assert.IsTrue(c.CheckType() == Condition.ConditionType.none);
        }

    }
}
