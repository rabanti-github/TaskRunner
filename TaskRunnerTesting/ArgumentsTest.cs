using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskRunner;

namespace TaskRunnerTesting
{
    [TestClass]
    public class ArgumentsTest
    {
        [TestMethod]
        [Description("Test of the constructor")]
        public void ConstructorTest()
        {
            try
            {
                Arguments args = new Arguments();
                Assert.IsTrue(args.DelayExecution == false && args.Run == false && args.Help == false && args.NumberOfIterations == 1); // Samples for default values
            }
            catch (Exception e)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        [Description("Test of the SetIterations() method (error test included)")]
        public void SetIterationsTest()
        {
            Arguments args = new Arguments();
            int i = args.NumberOfIterations;
            bool check = args.SetIterations("13");
            Assert.IsTrue(i == 1 && check == true && args.NumberOfIterations == 13);

            args = new Arguments();
            i = args.NumberOfIterations;
            check = args.SetIterations("one");
            Assert.IsTrue(check == false && i == args.NumberOfIterations);

            args = new Arguments();
            i = args.NumberOfIterations;
            check = args.SetIterations("-18");
            Assert.IsTrue(check == false && i == args.NumberOfIterations);

            args = new Arguments();
            i = args.NumberOfIterations;
            check = args.SetIterations("17.9");
            Assert.IsTrue(check == false && i == args.NumberOfIterations);

        }

        [TestMethod]
        [Description("Test of the SetDelay() method (error test included)")]
        public void SetDelayTest()
        {
            Arguments args = new Arguments();
            int i = args.DelayAmount;
            bool check = args.SetDelay("1500");
            Assert.IsTrue(i == 0 && check == true && args.DelayAmount == 1500);

            args = new Arguments();
            i = args.DelayAmount;
            check = args.SetDelay("1x7");
            Assert.IsTrue(check == false && i == args.DelayAmount);

            args = new Arguments();
            i = args.DelayAmount;
            check = args.SetDelay("-5");
            Assert.IsTrue(check == false && i == args.DelayAmount);

            args = new Arguments();
            i = args.DelayAmount;
            check = args.SetDelay("1.0000000000000000000001");
            Assert.IsTrue(check == false && i == args.DelayAmount);
        }

        [TestMethod]
        [Description("Test of the CheckArgs() method; simple fags without previous parameters")]
        public void CheckArgsTest()
        {
            Arguments arg = new Arguments();
            Arguments.ArgType type = Arguments.CheckArgs(ref arg, "-r", Arguments.ArgType.flag);
            Assert.IsTrue(type == Arguments.ArgType.configFile && arg.Run == true);
            arg = new Arguments();
            type = Arguments.CheckArgs(ref arg, "--RUN", Arguments.ArgType.flag);
            Assert.IsTrue(type == Arguments.ArgType.configFile && arg.Run == true);

            arg = new Arguments();
            type = Arguments.CheckArgs(ref arg, "-W", Arguments.ArgType.flag);
            Assert.IsTrue(type == Arguments.ArgType.delay && arg.DelayExecution == true);
            arg = new Arguments();
            type = Arguments.CheckArgs(ref arg, "--Wait", Arguments.ArgType.flag);
            Assert.IsTrue(type == Arguments.ArgType.delay && arg.DelayExecution == true);

            arg = new Arguments();
            type = Arguments.CheckArgs(ref arg, "-n", Arguments.ArgType.flag);
            Assert.IsTrue(type == Arguments.ArgType.flag && arg.NoInitialDelay == true);
            arg = new Arguments();
            type = Arguments.CheckArgs(ref arg, "--NoDelay", Arguments.ArgType.flag);
            Assert.IsTrue(type == Arguments.ArgType.flag && arg.NoInitialDelay == true);

            arg = new Arguments();
            type = Arguments.CheckArgs(ref arg, "-i", Arguments.ArgType.flag);
            Assert.IsTrue(type == Arguments.ArgType.iterations && arg.Iterative == true);
            arg = new Arguments();
            type = Arguments.CheckArgs(ref arg, "--ItErAtE", Arguments.ArgType.flag);
            Assert.IsTrue(type == Arguments.ArgType.iterations && arg.Iterative == true);

            arg = new Arguments();
            type = Arguments.CheckArgs(ref arg, "-H", Arguments.ArgType.flag);
            Assert.IsTrue(type == Arguments.ArgType.flag && arg.Help == true);
            arg = new Arguments();
            type = Arguments.CheckArgs(ref arg, "--HELP", Arguments.ArgType.flag);
            Assert.IsTrue(type == Arguments.ArgType.flag && arg.Help == true);

            arg = new Arguments();
            type = Arguments.CheckArgs(ref arg, "-o", Arguments.ArgType.flag);
            Assert.IsTrue(type == Arguments.ArgType.flag && arg.Output == true);
            arg = new Arguments();
            type = Arguments.CheckArgs(ref arg, "--output", Arguments.ArgType.flag);
            Assert.IsTrue(type == Arguments.ArgType.flag && arg.Output == true);

            arg = new Arguments();
            type = Arguments.CheckArgs(ref arg, "-s", Arguments.ArgType.flag);
            Assert.IsTrue(type == Arguments.ArgType.flag && arg.HaltOnError == true);
            arg = new Arguments();
            type = Arguments.CheckArgs(ref arg, "--stop", Arguments.ArgType.flag);
            Assert.IsTrue(type == Arguments.ArgType.flag && arg.HaltOnError == true);

            arg = new Arguments();
            type = Arguments.CheckArgs(ref arg, "-l", Arguments.ArgType.flag);
            Assert.IsTrue(type == Arguments.ArgType.logFile && arg.Log == true);
            arg = new Arguments();
            type = Arguments.CheckArgs(ref arg, "--lOg", Arguments.ArgType.flag);
            Assert.IsTrue(type == Arguments.ArgType.logFile && arg.Log == true);

            arg = new Arguments();
            type = Arguments.CheckArgs(ref arg, "-e", Arguments.ArgType.flag);
            Assert.IsTrue(type == Arguments.ArgType.flag && arg.Demo == true);
            arg = new Arguments();
            type = Arguments.CheckArgs(ref arg, "--Example", Arguments.ArgType.flag);
            Assert.IsTrue(type == Arguments.ArgType.flag && arg.Demo == true);

            arg = new Arguments();
            type = Arguments.CheckArgs(ref arg, "-d", Arguments.ArgType.flag);
            Assert.IsTrue(type == Arguments.ArgType.flag && arg.Docs == true);
            arg = new Arguments();
            type = Arguments.CheckArgs(ref arg, "--docs", Arguments.ArgType.flag);
            Assert.IsTrue(type == Arguments.ArgType.flag && arg.Docs == true);

            arg = new Arguments();
            type = Arguments.CheckArgs(ref arg, "-m", Arguments.ArgType.flag);
            Assert.IsTrue(type == Arguments.ArgType.flag && arg.Markdown == true);
            arg = new Arguments();
            type = Arguments.CheckArgs(ref arg, "--markdown", Arguments.ArgType.flag);
            Assert.IsTrue(type == Arguments.ArgType.flag && arg.Markdown == true);

            arg = new Arguments();
            type = Arguments.CheckArgs(ref arg, "-u", Arguments.ArgType.flag);
            Assert.IsTrue(type == Arguments.ArgType.flag && arg.Utilities == true);
            arg = new Arguments();
            type = Arguments.CheckArgs(ref arg, "--utils", Arguments.ArgType.flag);
            Assert.IsTrue(type == Arguments.ArgType.flag && arg.Utilities == true);

            arg = new Arguments();
            Parameter.UserParameters.Clear();
            type = Arguments.CheckArgs(ref arg, "-p:s:NAME1:'Text1'", Arguments.ArgType.flag);
            Parameter p = Parameter.GetUserParameter("NAME1", false);
            Assert.IsTrue(type == Arguments.ArgType.flag && p.Valid == true && p.Name == "NAME1" && p.Value == "Text1");
            arg = new Arguments();
            Parameter.UserParameters.Clear();
            type = Arguments.CheckArgs(ref arg, "--PARAM:NAME2:'TEXT2'", Arguments.ArgType.flag);
            p = Parameter.GetUserParameter("NAME2", false);
            Assert.IsTrue(type == Arguments.ArgType.flag && p.Valid == true && p.Name == "NAME2" && p.Value == "TEXT2");
            arg = new Arguments();
            Parameter.UserParameters.Clear();
            type = Arguments.CheckArgs(ref arg, "-p:NAME3:", Arguments.ArgType.flag);
            p = Parameter.GetUserParameter("NAME3", false);
            Assert.IsTrue(type == Arguments.ArgType.flag && p.Valid == true && p.Name == "NAME3" && p.Value == "");
        }

        [TestMethod]
        [Description("Test of the CheckArgs() method; advanced fags / parameters")]
        public void CheckArgsTest2()
        {
            Arguments arg = new Arguments();
            Arguments.ArgType type = Arguments.CheckArgs(ref arg, "Temp.xml", Arguments.ArgType.configFile);
            Assert.IsTrue(type == Arguments.ArgType.flag && arg.ConfigFilePath == "Temp.xml");

            arg = new Arguments();
            type = Arguments.CheckArgs(ref arg, "temp.LOG", Arguments.ArgType.logFile);
            Assert.IsTrue(type == Arguments.ArgType.flag && arg.LogFilePath == "temp.LOG");

            arg = new Arguments();
            type = Arguments.CheckArgs(ref arg, "55", Arguments.ArgType.iterations);
            Assert.IsTrue(type == Arguments.ArgType.flag && arg.NumberOfIterations.Equals(55));

            arg = new Arguments();
            type = Arguments.CheckArgs(ref arg, "0", Arguments.ArgType.iterations);
            Assert.IsTrue(type == Arguments.ArgType.flag && arg.NumberOfIterations.Equals(0));

            arg = new Arguments();
            type = Arguments.CheckArgs(ref arg, "0", Arguments.ArgType.delay);
            Assert.IsTrue(type == Arguments.ArgType.flag && arg.DelayAmount.Equals(0));

            arg = new Arguments();
            type = Arguments.CheckArgs(ref arg, "57800", Arguments.ArgType.delay);
            Assert.IsTrue(type == Arguments.ArgType.flag && arg.DelayAmount.Equals(57800));
        }

        [TestMethod]
        [Description("Test of the CheckArgs() method for errors")]
        public void CheckArgsTest3()
        {
            Arguments arg = new Arguments();
            Arguments.ArgType type = Arguments.CheckArgs(ref arg, "-x", Arguments.ArgType.flag);
            Assert.IsTrue(type == Arguments.ArgType.undefined);
            arg = new Arguments();
            type = Arguments.CheckArgs(ref arg, "-log", Arguments.ArgType.flag);
            Assert.IsTrue(type == Arguments.ArgType.undefined);
            arg = new Arguments();
            type = Arguments.CheckArgs(ref arg, "-", Arguments.ArgType.flag);
            Assert.IsTrue(type == Arguments.ArgType.undefined);
            arg = new Arguments();
            type = Arguments.CheckArgs(ref arg, "p", Arguments.ArgType.flag);
            Assert.IsTrue(type == Arguments.ArgType.undefined);
            arg = new Arguments();
            type = Arguments.CheckArgs(ref arg, "", Arguments.ArgType.flag);
            Assert.IsTrue(type == Arguments.ArgType.undefined);
            arg = new Arguments();
            type = Arguments.CheckArgs(ref arg, "-p:s:n", Arguments.ArgType.flag);
            Assert.IsTrue(type == Arguments.ArgType.undefined);
            arg = new Arguments();
            type = Arguments.CheckArgs(ref arg, "-p:b:n:x", Arguments.ArgType.flag);
            Assert.IsTrue(type == Arguments.ArgType.undefined);
            arg = new Arguments();
            type = Arguments.CheckArgs(ref arg, "", Arguments.ArgType.logFile);
            Assert.IsTrue(type == Arguments.ArgType.undefined);
            arg = new Arguments();
            type = Arguments.CheckArgs(ref arg, "", Arguments.ArgType.configFile);
            Assert.IsTrue(type == Arguments.ArgType.undefined);
            arg = new Arguments();
            type = Arguments.CheckArgs(ref arg, "", Arguments.ArgType.delay);
            Assert.IsTrue(type == Arguments.ArgType.undefined);
            arg = new Arguments();
            type = Arguments.CheckArgs(ref arg, "11.5", Arguments.ArgType.delay);
            Assert.IsTrue(type == Arguments.ArgType.undefined);
            arg = new Arguments();
            type = Arguments.CheckArgs(ref arg, "-1000", Arguments.ArgType.delay);
            Assert.IsTrue(type == Arguments.ArgType.undefined);
            arg = new Arguments();
            type = Arguments.CheckArgs(ref arg, "", Arguments.ArgType.iterations);
            Assert.IsTrue(type == Arguments.ArgType.undefined);
            arg = new Arguments();
            type = Arguments.CheckArgs(ref arg, "0.00000000000000000000001", Arguments.ArgType.iterations);
            Assert.IsTrue(type == Arguments.ArgType.undefined);
            arg = new Arguments();
            type = Arguments.CheckArgs(ref arg, "-50", Arguments.ArgType.iterations);
            Assert.IsTrue(type == Arguments.ArgType.undefined);
            arg = new Arguments();
            type = Arguments.CheckArgs(ref arg, "test", Arguments.ArgType.undefined); // Should never happen
            Assert.IsTrue(type == Arguments.ArgType.undefined);
        }

    }
}
