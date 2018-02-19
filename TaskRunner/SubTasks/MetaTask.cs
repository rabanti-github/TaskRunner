using System;
using System.Collections.Generic;
using System.IO;
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
        /// <returns>Sub-task status</returns>
        public override Task.Status Run()
        {
            try
            {
                if (File.Exists(this.MainValue) == false)
                {
                    return this.SetStatus("NO_FILE", "The config file '" + this.MainValue + "' was not found");
                }

                Task t = Task.Deserialize(this.MainValue);
                Task.Status status = t.Run(this.ParentTask.StopOnError, this.ParentTask.DisplayOutput, this.ParentTask.WriteLog, this.ParentTask.LogfilePath);
                if (status == Task.Status.terminate)
                {
                    return this.SetStatus("SUCCESS_TERMINATION", "Task " + this.MainValue + " caused the termination of the program due to a condition");
                }
                else
                {
                    return this.SetStatus("SUCCESS", "Task " + this.MainValue + " was executed. There may be inner errors (see logfile or console output if applicable)");
                }
            }
            catch(Exception e)
            {
                return this.SetStatus("ERROR", this.MainValue + " could not be executed:\n" + e.Message);
            }
        }

        /// <summary>
        /// Implemented GetDemoFile method of the SubTask class
        /// </summary>
        /// <param name="number">Optional number to indicate several Sub-Tasks</param>
        /// <returns>Instance of the implemented class</returns>
        public override SubTask GetDemoFile(int number)
        {
            MetaTask t = new MetaTask();
            t.Name = "Meta-Task_" + number.ToString();
            t.Description = "This is sub-task " + number.ToString();
            t.MainValue = @"C:\temp\tasks\task" + number.ToString() + ".xml";
            if (number == 3)
            {
                Condition c = new Condition();
                c.Action = "run";
                c.Default = "skip";
                c.Expression = "SUBTASK_LAST_SUCCESS == true";
                t.SubTaskCondition = c;
            }
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
            this.AppendCommonStatusCodes(ref codes);
            this.RegisterStatusCode("NO_FILE", Task.Status.failed, "The config file was not found", ref codes);
            this.RegisterStatusCode("SUCCESS", Task.Status.success, "The loaded config file was executed successfully", ref codes);
            this.RegisterStatusCode("SUCCESS_TERMINATION", Task.Status.success, "The loaded config file was executed but caused a program termination", ref codes);
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

        /// <summary>
        /// Returns the documentation of the XML attributes for the specific Sub-Task
        /// </summary>
        /// <returns>Documentation object</returns>
        public override Documentation GetAttributesDocumentationParameters()
        {
            Documentation attributes = new Documentation("Meta Task", "Attributes", "The following attributes are defined");
            this.AppendCommonAttributes(ref attributes, "<metaTaskItem>", "MetaTask");
            return attributes;
        }

    }
}
