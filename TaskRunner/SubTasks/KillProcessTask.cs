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
    /// Task Runner - (c) 2018 - Raphael Stoeckli
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
        /// The method how the process will be terminated. Valid values are 'name' (default) and 'pid'
        /// </summary>
        [XmlAttribute("method")]
        public string Method { get; set; }


        /// <summary>
        /// Default constructor
        /// </summary>
        public KillProcessTask() : base()
        {
            this.Method = "name";
        }

        /// <summary>
        /// Implemented Run method of the SubTask class
        /// </summary>
        /// <returns>Sub-task status</returns>
        public override Task.Status Run()
        {
            if (string.IsNullOrEmpty(this.MainValue))
            {
                return this.SetStatus("NO_PROCESS", "No process to terminated was defined");
               // this.Message = "No process to terminated was defined";
               // this.StatusCode = 0x02;
               // return Task.Status.failed;
            }
            if (this.Method.ToLower() != "name" && this.Method.ToLower() != "pid")
            {
                return this.SetStatus("INVALID_METHOD", "The method is invalid");
                //this.Message = "The method is invalid";
               // this.StatusCode = 0x04;
               // return Task.Status.failed;
            }
            this.Method = this.Method.ToLower();
            Process[] procs;
            try
            {
                if (this.Arguments.Count < 1)
                {
                    if (this.Method == "name")
                    {
                        procs = Process.GetProcessesByName(this.MainValue);
                    }
                    else // PID
                    {
                        int pid;
                        if (int.TryParse(this.MainValue, out pid) == false)
                        {
                            return this.SetStatus("INVALID_PID", "The process ID is invalid");
                            //this.Message = "The process ID is invalid";
                            //this.StatusCode = 0x03;
                           // return Task.Status.failed;
                        }
                        Process proc = Process.GetProcessById(pid);
                        procs = new Process[] { proc };
                    }
                }
                else
                {
                    if (this.ArgumentIsParamName == true)
                    {
                        Parameter p = Parameter.GetUserParameter(this.Arguments[0], this.ParentTask.DisplayOutput);
                        if (p.Valid == false)
                        {
                            return this.SetStatus("NO_PARAMETER", "The parameter with the name '" + this.Arguments[0] + "' is not defined");
                           // this.Message = "The parameter with the name '" + this.Arguments[0] + "' is not defined";
                           // this.StatusCode = 0x05;
                          //  return Task.Status.failed;
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
                    return this.SetStatus("SUCCESS_NO_PROCESS", "The process " + this.MainValue + " was not found. Nothing to do");
                   // this.Message = "The process "+ this.MainValue + " was not found. Nothing to do";
                   // this.StatusCode = 0x02;
                }
                else
                {
                    string msg;
                    for(int i = 0; i < procs.Length; i++)
                    {
                        procs[i].Kill();
                    }
                    if (procs.Length > 1)
                    {
                        //this.Message = "The process " + this.MainValue + " was terminated (" + procs.Length.ToString() + " instances)";
                        msg = "The process " + this.MainValue + " was terminated (" + procs.Length.ToString() + " instances)";
                    }
                    else
                    {
                        //this.Message = "The process " + this.MainValue + " was terminated";
                        msg = "The process " + this.MainValue + " was terminated";
                    }
                    return this.SetStatus("SUCCESS_TERMINATED", msg);
                    //this.StatusCode = 0x01;
                }
               // return Task.Status.success;
            }
            catch(Exception e)
            {
                return this.SetStatus("ERROR", "The process " + this.MainValue + " could not be terminated:\n" + e.Message);
                //this.Message = "The process " + this.MainValue + " could not be terminated:\n" + e.Message;
                //this.StatusCode = 0x01;
                //return Task.Status.failed;
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
                t.Method = "pid";
                t.MainValue = "4485";
                t.Arguments.Add("remote_machine_name");
            }
            else if (number == 3)
            {
                t.Arguments.Add("PARAM_NAME_2");
                t.ArgumentIsParamName = true;
                t.Description = t.Description = t.Description + ". The arguments 'PARAM_NAME_1' and 'PARAM_NAME_2 are the names of global parameters and not the actual value of the process, the remote machine name, respectively";
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
            this.AppendCommonStatusCodes(ref codes);

            this.RegisterStatusCode("NO_PROCESS", Task.Status.failed, "No process to terminated was defined", ref codes);
            this.RegisterStatusCode("INVALID_PID", Task.Status.failed, "The process ID is invalid", ref codes);
            this.RegisterStatusCode("INVALID_METHOD", Task.Status.failed, "The method is invalid", ref codes);
            this.RegisterStatusCode("NO_PARAMETER", Task.Status.failed, "The parameter is not defined", ref codes);
            this.RegisterStatusCode("SUCCESS_TERMINATED", Task.Status.success, "The process was terminated successfully", ref codes);
            this.RegisterStatusCode("SUCCESS_NO_PROCESS", Task.Status.success, "The process does not exist. Nothing to do", ref codes);
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
            attributes.AddTuple("method", "Method how the process is determined. Valid values of the parameter are 'name' (default: process name) and 'pid' (process ID). The attribute is part of the <killProcessItem> tag.");
            return attributes;
        }
    }
}
