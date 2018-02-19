using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Threading;

namespace TaskRunner.SubTasks
{
    /// <summary>
    /// Task Runner - (c) 2017 - Raphael Stoeckli
    /// This program and its code is released under the MIT license
    /// -----------------------------------------------------------
    /// Derived Sub-Task Class to pause the program for a particular number of milliseconds
    /// </summary>
    public class DelayTask: SubTask
    {

        /// <summary>
        /// Implemented code of the task type (07)
        /// </summary>
        [XmlIgnore]
        public override byte TaskTypeCode
        {
            get { return 0x08; }
        }

        /// <summary>
        /// Type of the Task / Sub-task
        /// </summary>
        [XmlIgnore]
        public override Task.TaskType Type
        {
            get { return Task.TaskType.DelayTask; }
        }

        /// <summary>
        /// Name of the demo file
        /// </summary>
        [XmlIgnore]
        public override string DemoFileName
        {
            get { return "DEMO_DelayTask.xml"; }
        }

        /// <summary>
        /// Name of the markdown file
        /// </summary>
        [XmlIgnore]
        public override string MarkdownFileName
        {
            get { return "DelayTask.md"; }
        }

        /// <summary>
        /// Implemented Run method of the SubTask class
        /// </summary>
        /// <returns>Sub-task status</returns>
        public override Task.Status Run()
        {
            if (string.IsNullOrEmpty(this.MainValue))
            {
                return this.SetStatus("NO_TIME", "No time as delay was defined");
            }
            int millis;
            if (int.TryParse(this.MainValue, out millis) == false)
            {
                return this.SetStatus("INVALID_TIME", "The delay time is invalid");
            }
            if (millis < 0)
            {
                return this.SetStatus("INVALID_TIME", "The delay time is invalid");
            }
            try
            {
                Thread.Sleep(millis);
                return this.SetStatus("SUCCESS", "Program was delayed for " + this.MainValue + " milliseconds");
            }
            catch (Exception e)
            {
                return this.SetStatus("ERROR", "The delay could not be established:\n" + e.Message);
            }
        }

        /// <summary>
        /// Implemented GetDemoFile method of the SubTask class
        /// </summary>
        /// <param name="number">Optional number to indicate several Sub-Tasks</param>
        /// <returns>Instance of the implemented class</returns>
        public override SubTask GetDemoFile(int number)
        {
            DelayTask t = new DelayTask();
            t.Name = "Delay-Task_" + number.ToString();
            if (number != 3)
            {
                t.Description = "This is sub-task " + number.ToString() + " (Delay for " + (number * 1000).ToString() + " milliseconds)";
                t.MainValue = (number * 1000).ToString();
            }
            else
            {
                t.Description = "This is sub-task " + number.ToString()  + ". The main value is a global parameter and not the actual value of number of milliseconds to wait";
                t.MainValue = "PARAM_NAME_2";
            }
            return t;
        }

        /// <summary>
        /// Returns the description as documentation for the specific Sub-Task
        /// </summary>
        /// <returns>Documentation object</returns>
        public override Documentation GetDocumentationDescription()
        {
            return new Documentation("Delay Task", "Description", "The tasks stops TaskRunner for the defined number of milliseconds. This task type is used as intermediate task in a MetaTask or in mixed tasks if a delay is necessary between two task operations. There are no additional options.");
        }

        /// <summary>
        /// Returns the documentation of the status codes for the specific Sub-Task
        /// </summary>
        /// <returns>Documentation object</returns>
        public override Documentation GetDocumentationStatusCodes()
        {
            Documentation codes = new Documentation("Delay Task", "Status Codes");
            this.AppendCommonStatusCodes(ref codes);
            this.RegisterStatusCode("NO_TIME", Task.Status.failed, "No time as delay was defined", ref codes);
            this.RegisterStatusCode("INVALID_TIME", Task.Status.failed, "The delay time is invalid", ref codes);
            this.RegisterStatusCode("SUCCESS", Task.Status.success, "The delay task was performed successfully", ref codes);
            return codes;
        }

        /// <summary>
        /// Returns the documentation of the XML tags for the specific Sub-Task
        /// </summary>
        /// <returns>Documentation object</returns>
        public override Documentation GetTagDocumentationParameters()
        {
            Documentation tags = new Documentation("Delay Task", "Tags", "The following specific tags are defined (see also the demo files or the example configuration)");
            this.AppendCommonTags(ref tags, "<delayItem>");
            tags.AddTuple("delayItem", "Main tag of a Sub-Task within the <items> tag");
            tags.AddTuple("mainValue", "Defines the number of milliseconds to pause TaskRunner");
            return tags;
        }

        /// <summary>
        /// Returns the documentation of the XML attributes for the specific Sub-Task
        /// </summary>
        /// <returns>Documentation object</returns>
        public override Documentation GetAttributesDocumentationParameters()
        {
            Documentation attributes = new Documentation("Delay Task", "Attributes", "The following attributes are defined");
            this.AppendCommonAttributes(ref attributes, "<delayItem>", "Delay");
            return attributes;
        }

    }
}
