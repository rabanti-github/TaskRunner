using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskRunner;

namespace TaskRunnerTesting
{
    [TestClass]
    public class OutputTest
    {
        private static string NL = Output.NL;
        private static string NL2 = Output.NL + Output.NL;

        [TestMethod]
        [Description("Test of the constructor")]
        public void ConstructorTest()
        {
            try
            {
                Output o = new Output();
                Assert.IsTrue(o.Width >= 0 && o.Description == "" && o.SubTitle == null && o.Title == null && o.Tuples.Count == 0);
                o = new Output(23);
                Assert.IsTrue(o.Width == 23 && o.Description == "" && o.SubTitle == null && o.Title == null && o.Tuples.Count == 0);
                o = new Output(26, "title1");
                Assert.IsTrue(o.Width == 26 && o.Description == "" && o.SubTitle == null && o.Title == "title1" && o.Tuples.Count == 0);
                o = new Output(22, "Title2", "Subtitle1") ;
                Assert.IsTrue(o.Width == 22 && o.Description == "" && o.SubTitle == "Subtitle1" && o.Title == "Title2" && o.Tuples.Count == 0);
                o = new Output(2, "Title3", "Subtitle2", "description1");
                Assert.IsTrue(o.Width == 2 && o.Description == "description1" && o.SubTitle == "Subtitle2" && o.Title == "Title3" && o.Tuples.Count == 0);

            }
            catch (Exception e)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        [Description("Test of the AddLine() method")]
        public void AddLineTest()
        {
            Output o = new Output(700);
            o.AddLine("line1");
            Assert.IsTrue(o.Tuples.Count == 1 && o.Tuples[0].Description == "line1");
            o.AddLine("line2");
            o.AddLine("line3");
            Assert.IsTrue(o.Tuples.Count == 3 && o.Tuples[2].Description == "line3");
            o.AddLine(null);
            Assert.IsTrue(o.Tuples.Count == 4 && o.Tuples[3].Description == "");
        }

        [TestMethod]
        [Description("Test of the AddTuple() method")]
        public void AddTupleTest()
        {
            Output o = new Output(700);
            o.AddTuple("v1", "d1");
            Assert.IsTrue(o.Tuples.Count == 1 && o.Tuples[0].Description == "d1" && o.Tuples[0].Value == "v1" && o.Tuples[0].OverrideTagFormatting == false);
            o.AddTuple("v2", "d2");
            o.AddTuple("v3", "d3", true);
            Assert.IsTrue(o.Tuples.Count == 3 && o.Tuples[2].Description == "d3" && o.Tuples[2].Value == "v3" && o.Tuples[2].OverrideTagFormatting == true);
            o.AddTuple(null, null);
            Assert.IsTrue(o.Tuples.Count == 4 && o.Tuples[3].Description == "" && o.Tuples[3].Value == "" && o.Tuples[3].OverrideTagFormatting == false);
        }

        [TestMethod]
        [Description("Test of the Print() method")]
        public void PrintTest()
        {
            Output o = new Output(700);
            o.AddLine("line1");
            o.AddLine("line2");
            string output = o.Print();
            Assert.IsTrue(output == "line1" + NL + "line2");

            o = new Output(700, "Title1");
            o.AddLine("line3   xyz");
            o.AddLine("line4 ");// Note the whitespace at the end
            output = o.Print();
            Assert.IsTrue(output == "Title1" + NL + "══════" + NL2 + "line3   xyz" + NL + "line4 "); // Note the whitespace at the end

            o = new Output(700, "Title2", "subtitle1");
            o.AddLine("line5");
            o.AddLine("line6");
            output = o.Print();
            Assert.IsTrue(output == "Title2 - subtitle1" + NL + "══════════════════" + NL2 + "line5" + NL + "line6");

            o = new Output(80, "Title3", "subtitle2", "description1");
            o.AddLine("line7");
            o.AddLine("line8");
            output = o.Print();
            string line = new string('─',80);
            Assert.IsTrue(output == "Title3 - subtitle2" + NL + "══════════════════" + NL2 + "description1" + NL + line + NL + "line7" + NL + "line8");

            o = new Output(700);
            o.AddTuple("value1", "description1");
            o.AddTuple("value2", "description2");
            output = o.Print();
            Assert.IsTrue(output == "value1: description1" + NL + "value2: description2");

            o = new Output(80, "Title3", "subtitle2", "description1");
            o.AddLine("line7");
            o.AddLine("line8");
            output = o.Print(true);
            line = new string('─', 80);
            Assert.IsTrue(output == "Title3 - subtitle2" + NL + "══════════════════" + NL2 + "description1"); // Tuples omitted if header section

        }

        [TestMethod]
        [Description("Test of the PrintAll() method")]
        public void PrintAllTest()
        {
            Output o = new Output(80, "Title1", "subtitle1", "description1");
            o.AddTuple("value1", "description2");
            o.AddTuple("value2", "description3");
            o.Flush();
            o.Title = "Title2";
            o.Description = "description4";
            o.SubTitle = "subtitle22";
            o.AddLine("line1");
            o.AddTuple("value3", "description5");
            string output = o.PrintAll();
            string line = new string('─', 80);
            string comparison = "Title1 - subtitle1" + NL + "══════════════════" + NL2 + "description1" + NL + line + NL + "value1: description2" + NL + "value2: description3" + NL + "Title2 - subtitle22" + NL + "═══════════════════" + NL2 + "description4" + NL + line + NL + "line1" + NL + "value3: description5";
            Assert.IsTrue(output == comparison);
            o.Flush("\nx\n");
            o.AddTuple("v1", "d1",true);
            output = o.PrintAll();
            comparison = "Title1 - subtitle1" + NL + "══════════════════" + NL2 + "description1" + NL + line + NL + "value1: description2" + NL + "value2: description3" + NL + "Title2 - subtitle22" + NL + "═══════════════════" + NL2 + "description4" + NL + line + NL + "line1" + NL + "value3: description5\nx\nv1: d1";
            Assert.IsTrue(output == comparison);
        }

        [TestMethod]
        [Description("Test of the Flush() method")]
        public void FlushTest()
        {
            Output o = new Output(700);
            o.AddLine("line1");
            o.AddLine("line2");
            string output = o.Print();
            Assert.IsTrue(output == "line1" + NL + "line2");
            o.Flush();
            output = o.Print();
            Assert.IsTrue(output == "");
            o.Title = "title1";
            o.AddTuple("value1", "description1");
            output = o.PrintAll();
            Assert.IsTrue(output == "line1" + NL + "line2" + NL + "title1" + NL + "══════" + NL2+  "value1: description1");

        }

        [TestMethod]
        [Description("Test of the ClearAll() method")]
        public void ClearAllTest()
        {
            Output o = new Output(80, "Title1", "subtitle1", "description1");
            o.AddTuple("value1", "description2");
            o.AddTuple("value2", "description3");
            o.Flush();
            o.Title = "Title2";
            o.Description = "description4";
            o.SubTitle = "subtitle22";
            o.AddLine("line1");
            o.AddTuple("value3", "description5");
            string output = o.PrintAll();
            string line = new string('─', 80);
            string comparison = "Title1 - subtitle1" + NL + "══════════════════" + NL2 + "description1" + NL + line + NL + "value1: description2" + NL + "value2: description3" + NL + "Title2 - subtitle22" + NL + "═══════════════════" + NL2 + "description4" + NL + line + NL + "line1" + NL + "value3: description5";
            Assert.IsTrue(output == comparison);
            o.ClearAll();
            output = o.PrintAll();
            Assert.IsTrue(output == "");
            o.AddLine("line1");
            o.AddLine("line2");
            output = o.Print();
            Assert.IsTrue(output == "line1" + NL + "line2");
        }
        [TestMethod]
        [Description("Test of the GetSectionTitle() method")]
        public void GetSectionTitleTest()
        {
            string output = Output.GetSectionTitle("xyz", false);
            string comparison = "┌─────┐" + NL + "│ xyz │" + NL + "└─────┘";
            Assert.IsTrue(output == comparison);
            output = Output.GetSectionTitle("ABC", true);
            comparison = "╔═════╗" + NL + "║ ABC ║" + NL + "╚═════╝";
            Assert.IsTrue(output == comparison);


        }

        [TestMethod]
        [Description("Test of the GetDefaultOutput() method")]
        public void GetDefaultOutputTest()
        {
            string output = Output.GetDefaultOutput(80, false, "text1");
            string comparison = "text1" + NL + "────────────────────────────────────────────────────────────────────────────────" + NL;
            Assert.IsTrue(output == comparison);
            output = Output.GetDefaultOutput(60, true, "text2");
            comparison = "text2";
            Assert.IsTrue(output == comparison);
            output = Output.GetDefaultOutput(60, true, "text3", "title1");
            comparison = "title1" + NL + "══════" + NL2 + "text3";
            Assert.IsTrue(output == comparison);
            output = Output.GetDefaultOutput(30, false, "text4", "title2");
            comparison = "title2" + NL + "══════" + NL2 + "text4" + NL + "──────────────────────────────" + NL;
            Assert.IsTrue(output == comparison);
            output = Output.GetDefaultOutput(60, true, "text5", "title3", "subtitle1");
            comparison = "title3 - subtitle1" + NL + "══════════════════" + NL2 + "text5";
            Assert.IsTrue(output == comparison);
            output = Output.GetDefaultOutput(30, false, "text6", "title4", "subtitle2");
            comparison = "title4 - subtitle2" + NL + "══════════════════" + NL2 + "text6" + NL + "──────────────────────────────" + NL;
            Assert.IsTrue(output == comparison);
        }

        [TestMethod]
        [Description("Test of the GetOutput() method")]
        public void GetOutputTest()
        {
            List<Output.T> tuples = new List<Output.T>();
            string output = Output.GetOutput(50, true, false, false, false, "text1", "title1", "subtitle1", null, tuples);
            string comparison = "title1 - subtitle1" + NL + "══════════════════" + NL2 + "text1" + NL + "──────────────────────────────────────────────────" + NL;
            Assert.IsTrue(output == comparison);
            tuples.Add(new Output.T("v1", "D1"));
            tuples.Add(new Output.T("V02", "t2"));
            output = Output.GetOutput(50, true, false, false, false, "text1\ntext1b", "title1", "subtitle1", "suffix1", tuples);
            comparison = "title1 - subtitle1" + NL + "══════════════════" + NL2 + "text1" + NL + "text1b" + NL + "──────────────────────────────────────────────────" + NL + "<v1>:  D1" + NL + "<V02>: t2" + NL + "──────────────────────────────────────────────────" + NL + "suffix1";
            Assert.IsTrue(output == comparison);
            tuples.Add(new Output.T("<xyz:v3>", "t3", true));
            output = Output.GetOutput(50, true, false, false, false, "text1", "title1", "subtitle1", null, tuples);
            comparison = "title1 - subtitle1" + NL + "══════════════════" + NL2 + "text1" + NL + "──────────────────────────────────────────────────" + NL + "<v1>:     D1" + NL + "<V02>:    t2" + NL + "<xyz:v3>: t3";
            Assert.IsTrue(output == comparison);
            tuples.Add(new Output.T("v4", "123456789 123456789", false));
            output = Output.GetOutput(50, true, true, false, false, "text1", "title1", "subtitle1", null, tuples); // new tuple (and all tuples) skipped
            comparison = "title1 - subtitle1" + NL + "══════════════════" + NL2 + "text1";
            Assert.IsTrue(output == comparison);
            output = Output.GetOutput(21, true, false, false, false, "text1", "title1", "subtitle1", null, tuples);
            comparison = "title1 - subtitle1" + NL + "══════════════════" + NL2 + "text1" + NL + "─────────────────────" + NL + "<v1>:     D1" + NL + "<V02>:    t2" + NL + "<xyz:v3>: t3" + NL + "<v4>:     123456789 " + NL + "          123456789";
            Assert.IsTrue(output == comparison);
            tuples.Add(new Output.T("v5", "abcdefghi 12345\n678", false));
            output = Output.GetOutput(21, true, false, false, false, "text1", "title1", "subtitle1", null, tuples);
            comparison = "title1 - subtitle1" + NL + "══════════════════" + NL2 + "text1" + NL + "─────────────────────" + NL + "<v1>:     D1" + NL + "<V02>:    t2" + NL + "<xyz:v3>: t3" + NL + "<v4>:     123456789 " + NL + "          123456789" + NL + "<v5>:     abcdefghi " + NL + "          12345" + NL + "          678";
            Assert.IsTrue(output == comparison);
            tuples.Clear();
            tuples.Add(new Output.T("v1", "d1"));
            output = Output.GetOutput(50, false, false, false, false, "text1", null, null, null, tuples);
            comparison = NL + "text1" + NL + "──────────────────────────────────────────────────" + NL + "v1: d1";
            Assert.IsTrue(output == comparison);
        }

        [TestMethod]
        [Description("Test of the GetOutput() method for markdown")]
        public void GetOutputTest2()
        {
            List<Output.T> tuples = new List<Output.T>();
            string output = Output.GetOutput(50, true, false, true, false, "text1", "title1", "subtitle1", null, null);
            string comparison = "# title1" + NL2 + "## subtitle1" + NL2 + "text1" + NL2 + "***" + NL2;
            Assert.IsTrue(output == comparison);
            tuples.Add(new Output.T("v1", "D1"));
            tuples.Add(new Output.T("V02", "t2"));
            output = Output.GetOutput(50, true, false, true, false, "text1", null, "subtitle1", "suffix1", tuples);
            comparison = NL + "## subtitle1" + NL2 + "text1" + NL2 + "***" + NL2 + NL + "Value | Description" +  NL + "--- | ---" + NL + "&lt;v1&gt; | D1" + NL + "&lt;V02&gt; | t2" + NL2 + "suffix1";
            Assert.IsTrue(output == comparison);
            tuples.Add(new Output.T("V03", "t3", true));
            output = Output.GetOutput(50, true, false, true, false, "text1", null, null, "suffix1", tuples);
            comparison = "text1" + NL2 + NL + "Value | Description" + NL + "--- | ---" + NL + "&lt;v1&gt; | D1" + NL + "&lt;V02&gt; | t2" + NL + "V03 | t3" + NL2 + "suffix1";
            Assert.IsTrue(output == comparison);
            output = Output.GetOutput(50, false, false, true, false, "text1", null, null, "suffix1", tuples);
            comparison = "text1" + NL2 + NL + "Value | Description" + NL + "--- | ---" + NL + "v1 | D1" + NL + "V02 | t2" + NL + "V03 | t3" + NL2 + "suffix1";
            Assert.IsTrue(output == comparison);
        }
    }
}
