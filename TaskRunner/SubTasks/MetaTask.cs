using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TaskRunner.SubTasks
{
    /// <summary>
    /// Task Runner - (c) 2017 - Raphael Stoeckli
    /// This program and its code is released under the MIT license
    /// -----------------------------------------------------------
    /// Derived Sub-Task Class to load the config files of other tasks for execution
    /// </summary>
    public class MetaTask : SubTask
    {
        /// <summary>
        /// Implemented code of the task type (07)
        /// </summary>
        [XmlIgnore]
        public override byte TaskTypeCode
        {
            get { return 0x07; }
        }

        /// <summary>
        /// Type of the Task / Sub-task
        /// </summary>
        [XmlIgnore]
        public override Task.TaskType Type
        {
            get { return Task.TaskType.MetaTask; }
        }

        /// <summary>
        /// Name of the demo file
        /// </summary>
        [XmlIgnore]
        public override string DemoFileName
        {
            get { return "DEMO_MetaTask.xml"; }
        }

        /// <summary>
        /// Name of the markdown file
        /// </summary>
        [XmlIgnore]
        public override string MarkdownFileName
        {
            get { return "MetaTask.md"; }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public MetaTask(): base()
        { }

        /// <summary>
        /// Implemented Run method of the SubTask class
        /// </summary>
        /// <returns>True if the task was executed successfully, otherwise false</returns>
        public override bool Run()
        {
            try
            {
                Task t = Task.Deserialize(this.MainValue);
                t.Run(this.ParentTask.StopOnError, this.ParentTask.DisplayOutput, this.ParentTask.WriteLog);
                this.Message = "Task " + this.MainValue + " was executed. There may be inner errors (see logfile or console output if applicable)";
                this.StatusCode = 0x01;
                return true;
            }
            catch(Exception e)
            {
                this.Message = this.MainValue + " could not be executed:\n" + e.Message;
                this.StatusCode = 0x01;
                return false;
            }
        }

        public override SubTask GetDemoFile(int number)
        {
            MetaTask t = new MetaTask();
            t.Name = "Meta-Task_" + number.ToString();
            t.Description = "This is sub-task " + number.ToString();
            t.MainValue = @"C:\temp\tasks\task" + number.ToString() + ".xml";
            return t;
        }

        /// <summary>
        /// Returns the description as documentation for the specific Sub-Task
        /// </summary>
        /// <returns>Documentation object</returns>
        public override Documentation GetDocumentationDescription()
        {
            return new Documentation("Meta Task", "Description", "The task loads further TaskRunner tasks (config files) and executes them. There are no additional options.");
        }

        /// <summary>
        /// Returns the documentation of the status codes for the specific Sub-Task
        /// </summary>
        /// <returns>Documentation object</returns>
        public override Documentation GetDocumentationStatusCodes()
        {
            Documentation codes = new Documentation("Meta Task", "Status Codes");
            codes.AddTuple(this.PrintStatusCode(true, 0x01), "The loaded task was executed successfully");
            codes.AddTuple(this.PrintStatusCode(false, 0x01), "The task could not be loaded and / or executed due to an unknown reason");
            return codes;
        }

        /// <summary>
        /// Returns the documentation of the XML tags for the specific Sub-Task
        /// </summary>
        /// <returns>Documentation object</returns>
        public override Documentation GetTagDocumentationParameters()
        {
            Documentation tags = new Documentation("Meta Task", "Tags", "The following specific tags are defined (see also the demo files or the example configuration)");
            this.AppendCommonTags(ref tags, "<metaTaskItem>");
            tags.AddTuple("metaTaskItem", "Main tag of a Sub-Task within the <items> tag");
            tags.AddTuple("mainValue", "Defines the full path to the config file to be loaded and executed by TaskRunner");
            return tags;
        }

        public override Documentation GetAttributesDocumentationParameters()
        {
            Documentation attributes = new Documentation("Meta Task", "Attributes", "The following attributes are defined");
            this.AppendCommonAttributes(ref attributes, "<metaTaskItem>", "MetaTask");
            return attributes;
        }
    }
}
