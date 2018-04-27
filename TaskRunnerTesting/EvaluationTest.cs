using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskRunner;

namespace TaskRunnerTesting
{
    [TestClass]
    public class EvaluationTest
    {
        [TestMethod]
        [Description("Test of the ParseCondition() method for simple conditions")]
        public void ParseConditionTest()
        {
            Assert.IsTrue(Evaluation.ParseCondition("15 >= 14", true));
            Assert.IsTrue(Evaluation.ParseCondition("15.6667>=14", true));
            Assert.IsTrue(Evaluation.ParseCondition("-22<14", true));
            Assert.IsTrue(Evaluation.ParseCondition("-0.0000000000001 <= 0", true));
            Assert.IsTrue(Evaluation.ParseCondition("13.99989==13.99989", true));
            Assert.IsTrue(Evaluation.ParseCondition("0 = 0", true));
            Assert.IsTrue(Evaluation.ParseCondition("true = true", true));
            Assert.IsTrue(Evaluation.ParseCondition("false==false", true));
            Assert.IsTrue(Evaluation.ParseCondition("TRUE = TRUE", true));
            Assert.IsTrue(Evaluation.ParseCondition("False==False", true));
            Assert.IsTrue(Evaluation.ParseCondition("True == TRUE", true));
            Assert.IsTrue(Evaluation.ParseCondition("true != false", true));
            Assert.IsTrue(Evaluation.ParseCondition("false <> True", true));
            Assert.IsTrue(Evaluation.ParseCondition("'xxx' == 'xxx'", true));
            Assert.IsTrue(Evaluation.ParseCondition("'xxy'<>'xxx'", true));
            Assert.IsTrue(Evaluation.ParseCondition("true", true));

            Assert.IsFalse(Evaluation.ParseCondition("13.999999999 >= 14", true));
            Assert.IsFalse(Evaluation.ParseCondition("13.999999999>14", true));
            Assert.IsFalse(Evaluation.ParseCondition("True == False", true));
            Assert.IsFalse(Evaluation.ParseCondition("'x' = true", true));
            Assert.IsFalse(Evaluation.ParseCondition("true != true", true));
            Assert.IsFalse(Evaluation.ParseCondition("FALSE <> false", true));
            Assert.IsFalse(Evaluation.ParseCondition("'xxy' = 'xxx'", true));
            Assert.IsFalse(Evaluation.ParseCondition("False", true));

            Assert.IsTrue(Evaluation.ParseCondition("'xyz' != 'abc'", true));
            Assert.IsTrue(Evaluation.ParseCondition("('xyz' = 'xyz')", true));
            Assert.IsTrue(Evaluation.ParseCondition("('xyz' = 'XYZ') or not ('A' = 'B') or true", true));
            Assert.IsTrue(Evaluation.ParseCondition("('x\"y\"z' = 'x\"y\"z')", true));
        }

        [TestMethod]
        [Description("Test of the ParseCondition() method for nested and complex conditions")]
        public void ParseConditionTest2()
        {
            Assert.IsTrue(Evaluation.ParseCondition("(4 < 5) && (11 > 10)", true));
            Assert.IsTrue(Evaluation.ParseCondition("(4 < 5) or (11 > 9)", true));
            Assert.IsTrue(Evaluation.ParseCondition("((4 < 5) OR (11 > 9)) AND (0 = 0)", true));
            Assert.IsTrue(Evaluation.ParseCondition("((4 < 5) OR (11 > 9)) AND NOT (1 = 0)", true));
            Assert.IsTrue(Evaluation.ParseCondition("((4 <= 5) || (11 >= 9)) && !(1 = 0)", true));
            Assert.IsTrue(Evaluation.ParseCondition("(4+1 == 5) && (10/2 == 5)", true));
            Assert.IsTrue(Evaluation.ParseCondition("(3*3 == 18/2) && (13%7 == 6)", true));
            Assert.IsTrue(Evaluation.ParseCondition("(0 - 2 <= -1) && (-3 + 4 > 0)", true));
        }

        [TestMethod]
        [Description("Test of the ParseCondition() method for system and user parameters")]
        public void ParseConditionTest3()
        {
            Parameter.UserParameters.Clear();
            Parameter.SystemParameters.Clear();
            Parameter.RegisterSystemParameters();
            Parameter.AddUserParameter(new Parameter("-p:s:NAME1:text1"), true);
            Assert.IsTrue(Evaluation.ParseCondition("NAME1 == 'text1'", true));
            Parameter.AddUserParameter(new Parameter("-p:b:NAME2:true"), true);
            Assert.IsTrue(Evaluation.ParseCondition("NAME2 == TRUE", true));
            Parameter.AddUserParameter(new Parameter("-p:n:NAME3:22.375"), true);
            Assert.IsTrue(Evaluation.ParseCondition("NAME3 == 22.375", true));
            Assert.IsFalse(Evaluation.ParseCondition("NAME4 == false", true)); // parameter does not exist and is interpreted as text
            Parameter.UpdateSystemParameter(Parameter.SysParam.ENV_MAX_TASK_ITERATIONS, 23);
            Assert.IsTrue(Evaluation.ParseCondition("ENV_MAX_TASK_ITERATIONS == 23", true));
            Assert.IsTrue(Evaluation.ParseCondition("ENV_MAX_TASK_ITERATIONS == 23", true));
            Parameter.AddUserParameter(new Parameter("-p:d:NAME5:'01.01.1905'"), true);
            Parameter.AddUserParameter(new Parameter("-p:d:NAME6:'01.01.2115'"), true);
            Assert.IsTrue(Evaluation.ParseCondition("(NAME5 < == SYSTEM_TIME_NOW) && (NAME6 > SYSTEM_TIME_NOW)", true));
        }

        [TestMethod]
        [Description("Test of the ParseCondition() method for errors")]
        public void ParseConditionTest4()
        {
            try
            {
                Assert.IsFalse(Evaluation.ParseCondition(null, true)); // null
                Assert.IsFalse(Evaluation.ParseCondition("", true)); // empty
                Assert.IsFalse(Evaluation.ParseCondition(" ", true)); // white space
                Assert.IsFalse(Evaluation.ParseCondition("1", true)); // number without context
                Assert.IsFalse(Evaluation.ParseCondition("x", true)); // non-arithmetic or logic value
                Assert.IsFalse(Evaluation.ParseCondition("15/0 == 1", true)); // zero division shall not throw an exception
                Assert.IsFalse(Evaluation.ParseCondition("1 - 1 = ", true)); // incomplete
            }
            catch (Exception e)
            {
                Assert.Fail();
            }
        }
        
    }
}
