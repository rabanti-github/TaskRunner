using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TaskRunner.SubTasks
{
    /// <summary>
    /// Dummy Task to handle Pre-Sub-Task conditions
    /// </summary>
    public class DummyTask : SubTask
    {
        /// <summary>
        /// Implemented code of the task type (00 = Pre-Sub-Task condition)
        /// </summary>
        [XmlIgnore]
        public override byte TaskTypeCode
        {
            get { return 0x00; }
        }

        /// <summary>
        /// Type of the Task / Sub-task
        /// </summary>
        [XmlIgnore]
        public override Task.TaskType Type
        {
            get { return Task.TaskType.DummyTask; }
        }

        /// <summary>
        /// Name of the demo file
        /// </summary>
        [XmlIgnore]
        public override string DemoFileName
        {
            get { return "DEMO_DummyTask.xml"; }
        }

        /// <summary>
        /// Name of the markdown file
        /// </summary>
        [XmlIgnore]
        public override string MarkdownFileName
        {
            get { return "DummyTask.md"; }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public DummyTask(): base()
        {
            GetDocumentationStatusCodes();
            this.SetStatus("ERROR", "Undefined Error");
        }

        /// <summary>
        /// Returns the documentation of the XML attributes for the specific Sub-Task
        /// </summary>
        /// <returns>Documentation object</returns>
        public override Documentation GetAttributesDocumentationParameters()
        {
            Documentation attributes = new Documentation("Task (general)", "Attributes", "The following attributes are defined");
            this.AppendCommonAttributes(ref attributes, "<[SUBTASK]Item>", "Dummy");
            return attributes;
        }

        /// <summary>
        /// Implemented GetDemoFile method of the SubTask class
        /// </summary>
        /// <param name="number">Optional number to indicate several Sub-Tasks</param>
        /// <returns>Instance of the implemented class</returns>
        public override SubTask GetDemoFile(int number)
        {
            return new DummyTask();
        }

        /// <summary>
        /// Returns the description as documentation for the specific Sub-Task
        /// </summary>
        /// <returns>Documentation object</returns>
        public override Documentation GetDocumentationDescription()
        {
            return new Documentation("Task (general)", "Description", "This Sub-Task is to handle conditions that occurs before the execution of an actual Sub-Task");
        }

        /// <summary>
        /// Returns the documentation of the status codes for the specific Sub-Task
        /// </summary>
        /// <returns>Documentation object</returns>
        public override Documentation GetDocumentationStatusCodes()
        {
            Documentation codes = new Documentation("Task (genera)", "Status Codes");
            this.AppendCommonStatusCodes(ref codes);
            return codes;
        }

        /// <summary>
        /// Returns the documentation of the XML tags for the specific Sub-Task
        /// </summary>
        /// <returns>Documentation object</returns>
        public override Documentation GetTagDocumentationParameters()
        {
            Documentation tags = new Documentation("Task (general)", "Tags", "The following general tags are defined");
            this.AppendCommonTags(ref tags, "<[SUBTASK]Item>");
            return tags;
        }

        /// <summary>
        /// Implemented Run method of the SubTask class
        /// </summary>
        /// <returns>Sub-task status</returns>
        public override Task.Status Run()
        {
            return Task.Status.none; // No action
        }
    }
}
