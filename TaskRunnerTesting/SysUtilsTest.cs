using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskRunner;

namespace TaskRunnerTesting
{
    [TestClass]
    public class SysUtilsTest
    {
        [TestMethod]
        [Description("Test of the constructor of SysUtils")]
        public void SysUtilsConstructorTest()
        {
            SysUtils s = new SysUtils();
            Assert.IsTrue(s.Utilities != null);
            Assert.IsTrue(s.Utilities.Count > 0);
        }

        [TestMethod]
        [Description("Test of the constructor of SysUtil")]
        public void SysUtilConstructorTest()
        {
            SysUtils.SysUtil u = new SysUtils.SysUtil("name1", "description1", "command1", "args1");
            Assert.IsTrue(u.Name == "name1");
            Assert.IsTrue(u.Description == "description1");
            Assert.IsTrue(u.Command == "command1");
            Assert.IsTrue(u.Arguments == "args1");
        }

        [TestMethod]
        [Description("Test of the SysUtil run() function (starting of obscure dialer.exe in System32)")]
        public void SysUtilRunTest()
        {
            SysUtils.SysUtil u = new SysUtils.SysUtil("dialer", "", "dialer");
            u.Run();
            Process[] procs = Process.GetProcessesByName("dialer");
            Assert.IsTrue(procs.Length > 0);
            try
            {
                foreach (Process proc in procs)
                {
                    proc.Kill();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        [TestMethod]
        [Description("Test of the SysUtil run() function (starting of dxdiag.exe in System32 with arguments)")]
        public void SysUtilRunTest2()
        {
            string file = AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar.ToString() + "dxdiagCallTest.txt";
            SysUtils.SysUtil u = new SysUtils.SysUtil("dxdiag", "", "dxdiag", "/t " + file);
            u.Run();
            Thread.Sleep(5000); // Wait 5 seconds until testing
            Assert.IsTrue(File.Exists(file));
            try
            {
                File.Delete(file);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }



    }
}
