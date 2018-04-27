using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskRunner;

namespace TaskRunnerTesting
{
    [TestClass]
    public class UtilsTest
    {
        [TestMethod]
        [Description("Test of ConvertBytesToStringTest()")]
        public void ConvertBytesToStringTest()
        {
            byte[] bytes = new byte[] {1, 128, 33, 255, 2, 22, 188};
            string output = Utils.ConvertBytesToString(bytes);
            Assert.AreEqual(output, "018021FF0216BC");

        }

        [TestMethod]
        [Description("Test of ConvertBytesToStringTest() for one byte")]
        public void ConvertByteToStringTest()
        {
            string output = Utils.ConvertBytesToString(188);
            Assert.AreEqual(output, "BC");

        }

        [TestMethod]
        [Description("Test of Log()")]
        public void LogTest()
        {
            string header = "h1\th2\th3";
            string value = "v1\tv2\tv3";
            string logFile = "logtest.log";
            try
            {
                if (File.Exists(logFile))
                {
                    File.Delete(logFile);
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            bool result = Utils.Log(logFile, header, value);
            Assert.IsTrue(result);
            StreamReader sr = new StreamReader(logFile);
            string entry = sr.ReadToEnd();
            sr.Close();
            string shouldBe = header + "\r\n" + value + "\r\n";
            Assert.AreEqual(entry, shouldBe);

        }

        [TestMethod]
        [Description("Test of Log() if log already exits")]
        public void LogTest2()
        {
            string header = "h1\th2\th3";
            string value = "v1\tv2\tv3";
            string value2 = "v4\tv5\tv6";
            string logFile = "logtest.log";
            try
            {
                if (File.Exists(logFile))
                {
                    File.Delete(logFile);
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            bool result1 = Utils.Log(logFile, header, value);
            bool result2 = Utils.Log(logFile, header, value2);
            Assert.IsTrue(result1);
            Assert.IsTrue(result2);
            StreamReader sr = new StreamReader(logFile);
            string entry = sr.ReadToEnd();
            sr.Close();
            string shouldBe = header + "\r\n" + value + "\r\n" + value2 + "\r\n";
            Assert.AreEqual(entry, shouldBe);

        }

        [TestMethod]
        [Description("Test of Log(); Test of return == false if log is in use (no exception)")]
        public void LogTest3()
        {
            string header = "h1\th2\th3";
            string value = "v1\tv2\tv3";
            string value2 = "v4\tv5\tv6";
            string logFile = "logtest.log";
            try
            {
                if (File.Exists(logFile))
                {
                    File.Delete(logFile);
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            bool result1 = Utils.Log(logFile, header, value);
            StreamReader sr = new StreamReader(logFile);
            bool result2 = Utils.Log(logFile, header, value2);
            Assert.IsTrue(result1);
            Assert.IsFalse(result2);
            sr.Close();
        }

        [TestMethod]
        [Description("Test of GetRandomString()")]
        public void GetRandomStringTest()
        {
            string rnd = Utils.GetRandomString(7);
            string rnd2 = Utils.GetRandomString(7);
            string rnd3 = Utils.GetRandomString(0); // Should not throw an exception
            Assert.IsTrue(rnd.Length == 7);
            Assert.IsTrue(rnd.Equals(rnd2) == false);
            Assert.IsTrue(rnd3.Length == 0);
        }


    }
}
