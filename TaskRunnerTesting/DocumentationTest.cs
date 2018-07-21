using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskRunner;

namespace TaskRunnerTesting
{
    [TestClass]
    public class DocumentationTest
    {

        private static string NL = Output.NL;
        private static string NL2 = Output.NL + Output.NL;

        [TestMethod]
        [Description("Test of the getters and setters")]
        public void GetterSetterTest()
        {
            Documentation d = new Documentation();
            d.Description = "d1";
            d.SubTitle = "st1";
            d.Suffix = "sf1";
            d.Title = "t1";
            Assert.IsTrue(d.Description == "d1" && d.SubTitle == "st1" && d.Suffix == "sf1" && d.Title == "t1");
        }

        [TestMethod]
        [Description("Test of the constructor(s)")]
        public void ConstructorTest()
        {
            Documentation d = new Documentation();
            string output = d.GetDocumentation(80);
            string comparison = NL;
            Assert.IsTrue(output == comparison);
            d = new Documentation("Title1", "Subtitle1");
            output = d.GetDocumentation(80);
            comparison = "Title1 - Subtitle1" + NL + "══════════════════" +NL2;
            Assert.IsTrue(output == comparison);
            d = new Documentation("Title1", "Subtitle1", "Description1");
            output = d.GetDocumentation(80);
            comparison = "Title1 - Subtitle1" + NL + "══════════════════" + NL2 + "Description1";
            Assert.IsTrue(output == comparison);
        }

        [TestMethod]
        [Description("Combined test of the ClearTuples() and AddTuple() methods")] // Underlaying methods are thoroughly tested in OutputTest class
        public void TuplesTest()
        {
            Documentation d = new Documentation("t1", "st1");
            d.AddTuple("v1", "d1");
            d.AddTuple("v2", "d2", true);
            string output = d.GetDocumentation(20, true);
            string comparison = "t1 - st1" + NL + "════════" + NL2 + "<v1>: d1" + NL + "v2:   d2"; // Mind the padding because of the missing brackets
            Assert.IsTrue(output == comparison);
            d.ClearTuples();
            output = d.GetDocumentation(20, true);
            comparison = "t1 - st1" + NL + "════════" + NL2;
            Assert.IsTrue(output == comparison);

        }

        [TestMethod]
        [Description(
            "Test of the GetDocumentation() methods")] // Underlaying methods are thoroughly tested in OutputTest class
        public void GetDocumentationTest()
        {
            Documentation d = new Documentation("t1", "st1");
            d.AddTuple("v1b", "d1");
            d.AddTuple("v2", "d2", true);
            d.Description = "description1";
            string output = d.GetDocumentation(20);
            string comparison = "t1 - st1" + NL + "════════" + NL2+ "description1";
            Assert.IsTrue(output == comparison);
            output = d.GetDocumentation(20, true);
            comparison = "t1 - st1" + NL + "════════" + NL2 + "description1" + NL + "────────────────────" + NL + "<v1b>: d1" + NL + "v2:    d2"; // Mind the padding because of the missing brackets
            Assert.IsTrue(output == comparison);
            output = d.GetDocumentation(20, false);
            comparison = "t1 - st1" + NL + "════════" + NL2 + "description1" + NL + "────────────────────" + NL + "v1b: d1" + NL + "v2:  d2";
            Assert.IsTrue(output == comparison);
            output = d.GetDocumentation(20, false, false);
            Assert.IsTrue(output == comparison);
            output = d.GetDocumentation(20, false, true);
            comparison = "# t1" + NL2 + "## st1" + NL2 + "description1" + NL2 + "***" + NL2 + NL + "Value | Description" + NL + "--- | ---" + NL + "v1b | d1" + NL + "v2 | d2" + NL;
            Assert.IsTrue(output == comparison);
            output = d.GetDocumentation(20, false, true, false, false);
            comparison = "t1 - st1" + NL + "════════" + NL2 + "description1";
            Assert.IsTrue(output == comparison);
            output = d.GetDocumentation(20, false, false, false, false);
            comparison = "t1 - st1" + NL + "════════" + NL2 + "description1" + NL + "────────────────────" + NL + "v1b: d1" + NL + "v2:  d2";
            Assert.IsTrue(output == comparison);
            output = d.GetDocumentation(20, false, false, false, true);
            comparison = "description1" + NL + "────────────────────" + NL + "v1b: d1" + NL + "v2:  d2";
            Assert.IsTrue(output == comparison);
            output = d.GetDocumentation(20, false, true, false, true);
            comparison = "description1";
            Assert.IsTrue(output == comparison);

        }


    }
}
