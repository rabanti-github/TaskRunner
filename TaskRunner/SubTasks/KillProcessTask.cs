using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TaskRunner.SubTasks
{
    /// <summary>
    /// Task Runner - (c) 2017 - Raphael Stoeckli
    /// This program and its code is released under the MIT license
    /// -----------------------------------------------------------
    /// Derived Sub-Task Class for killing a windows process
    /// </summary>
    public class KillProcessTask : SubTask
    {

        /// <summary>
        /// Implemented code of the task type (06)
        /// </summary>
        [XmlIgnore]
        public override byte TaskTypeCode
        {
            get { return 0x06; }
        }

        /// <summary>
        /// Type of the Task / Sub-task
        /// </summary>
        [XmlIgnore]
        public override Task.TaskType Type
        {
            get { return Task.TaskType.KillProcess; }
        }

        /// <summary>
        /// Name of the demo file
        /// </summary>
        [XmlIgnore]
        public override string DemoFileName
        {
            get { return "DEMO_KillProcess.xml"; }
        }

        /// <summary>
        /// Name of the markdown file
        /// </summary>
        [XmlIgnore]
        public override string MarkdownFileName
        {
            get { return "KillProcess.md"; }
        }

        /// <summary>
        /// If true, the arguments are the parameter names (of global parameters) and not the actual values
        /// </summary>
        [XmlAttribute("argumentIsParamName")]
        public bool ArgumentIsParamName { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public KillProcessTask() : base()
        { }


        /// <summary>
        /// Implemented Run method of the SubTask class
        /// </summary>
        /// <returns>True if the task was executed successfully, otherwise false</returns>
        public override bool Run()
        {
            if (string.IsNullOrEmpty(this.MainValue))
            {
                this.Message = "No process to terminate was defined";
                this.StatusCode = 0x02;
                return false;
            }
            Process[] procs;
            try
            {
                if (this.Arguments.Count < 1)
                {
                    procs = Process.GetProcessesByName(this.MainValue);
                }
                else
                {

                    if (this.ArgumentIsParamName == true)
                    {
                        Parameter p = Parameter.GetParameter(this.Arguments[0], this.ParentTask.DisplayOutput);
                        if (p.Valid == false)
                        {
                            this.Message = "The parameter with the name '" + this.Arguments[0] + "' is not defined";
                            this.StatusCode = 0x03;
                            return false;
                        }
                        else
                        {
                            procs = Process.GetProcessesByName(this.MainValue, p.Value);
                        }
                    }
                    else
                    {
                        procs = Process.GetProcessesByName(this.MainValue, this.Arguments[0]);
                    }  
                }
                if (procs.Length == 0)
                {
                    this.Message = "The process "+ this.MainValue + " was not found. Nothing to do";
                    this.StatusCode = 0x02;
                }
                else
                {
                    for(int i = 0; i < procs.Length; i++)
                    {
                        procs[i].Kill();
                    }
                    if (procs.Length > 1)
                    {
                        this.Message = "The process " + this.MainValue + " was terminated (" + procs.Length.ToString() + " instance)";
                    }
                    else
                    {
                        this.Message = "The process " + this.MainValue + " was terminated";
                    }
                    this.StatusCode = 0x01;
                }
                return true;
            }
            catch(Exception e)
            {
                this.Message = this.MainValue + " could not be terminated:\n" + e.Message;
                this.StatusCode = 0x01;
                return false;
            }
        }

        /// <summary>
        /// Implemented GetDemoFile method of the SubTask class
        /// </summary>
        /// <param name="number">Optional number to indicate several Sub-Tasks</param>
        /// <returns>Instance of the implemented class</returns>
        public override SubTask GetDemoFile(int number)
        {
            KillProcessTask t = new KillProcessTask();
            t.Name = "Kill-Process-Task_" + number.ToString();
            t.Description = "This is sub-task" + number.ToString();
            t.MainValue = "notepad";
            if (number == 2)
            {
                t.Arguments.Add("remote_machine_name");
            }
            else if (number == 3)
            {
                t.Arguments.Add("PARAM_NAME_2");
                t.ArgumentIsParamName = true;
                t.Description = t.Description = t.Description + ". The argument is the name of a global parameter and not the actual value of the remote machine name";
            }
            return t;
        }

        /// <summary>
        /// Returns the description as documentation for the specific Sub-Task
        /// </summary>
        /// <returns>Documentation object</returns>
        public override Documentation GetDocumentationDescription()
        {
            return new Documentation("Kill Process Task", "Description", "The tasks terminates one or more Windows processes by the process(es) name(s). An additional remote machine name can be defined. Note that administrative permissions may be necessary to execute such a task.");
        }

        /// <summary>
        /// Returns the documentation of the status codes for the specific Sub-Task
        /// </summary>
        /// <returns>Documentation object</returns>
        public override Documentation GetDocumentationStatusCodes()
        {
            Documentation codes = new Documentation("Kill Process Task", "Status Codes");
            codes.AddTuple(this.PrintStatusCode(true, 0x01), "The process was terminated successfully");
            codes.AddTuple(this.PrintStatusCode(true, 0x02), "The process does not exist. Nothing to do");
            codes.AddTuple(this.PrintStatusCode(false, 0x01), "The process could not be terminated due to an unknown reason");
            codes.AddTuple(this.PrintStatusCode(false, 0x02), "No process to terminate was defined");
            codes.AddTuple(this.PrintStatusCode(false, 0x03), "The parameter is not defined");
            return codes;
        }

        /// <summary>
        /// Returns the documentation of the XML tags for the specific Sub-Task
        /// </summary>
        /// <returns>Documentation object</returns>
        public override Documentation GetTagDocumentationParameters()
        {
            Documentation tags = new Documentation("Kill Process Task", "Tags", "The following specific tags are defined (see also the demo files or the example configuration)");
            this.AppendCommonTags(ref tags, "<killProcessItem>");
            tags.AddTuple("killProcessItem", "Main tag of a Sub-Task within the <items> tag");
            tags.AddTuple("mainValue", "Defines the name of the process");
            tags.AddTuple("argument", "The optional <argument> tag within the <arguments> tag contains the name of the remote machine if applicable. If the argumentIsParamName attribute is set to true, each argument is a global parameter name instead of the actual value. In this case, the value will be resolved at runtime");
            return tags;
        }

        /// <summary>
        /// Returns the documentation of the XML attributes for the specific Sub-Task
        /// </summary>
        /// <returns>Documentation object</returns>
        public override Documentation GetAttributesDocumentationParameters()
        {
            Documentation attributes = new Documentation("Kill Process Task", "Attributes", "The following attributes are defined");
            this.AppendCommonAttributes(ref attributes, "<killProcessItem>", "KillProcess");
            attributes.AddTuple("argumentIsParamName", "Indicates whether the arguments are the parameter names (of global parameters) and not the actual values. Valid values of the parameter are 'true' and 'false'. The attribute is part of the <killProcessItem> tag and is optional.");
            return attributes;
        }
    }
}
