using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TaskRunner.SubTasks
{
    /// <summary>
    /// Task Runner - (c) 2018 - Raphael Stoeckli
    /// This program and its code is released under the MIT license
    /// -----------------------------------------------------------
    /// Derived Sub-Task Class for staring programs synchronous or asynchronous
    /// </summary>
    public class StartProgramTask : SubTask
    {
        private bool asynchronous;

        /// <summary>
        /// Implemented code of the task type (03)
        /// </summary>
        [XmlIgnore]
        public override byte TaskTypeCode
        {
            get { return 0x3; }
        }

        /// <summary>
        /// Type of the Task / Sub-task
        /// </summary>
        [XmlIgnore]
        public override Task.TaskType Type
        {
	        get { return Task.TaskType.StartProgram; }
        }

        /// <summary>
        /// Name of the demo file
        /// </summary>
        [XmlIgnore]
        public override string DemoFileName
        {
            get { return "DEMO_StartProgram.xml"; }
        }

        /// <summary>
        /// Name of the markdown file
        /// </summary>
        [XmlIgnore]
        public override string MarkdownFileName
        {
            get { return "StartProgram.md"; }
        }

        /// <summary>
        /// If true, the program will be executed asynchronous
        /// </summary>
        [XmlAttribute("runAsynchronous")]
        public bool Asynchronous
        {
            get { return asynchronous; }
            set 
            { 
                asynchronous = value;
                if (value == true) { this.Prolog = "Starting program asynchronous. Waiting until process ends..."; }
                else { this.Prolog = "Starting program synchronous"; }
            }
        }

        /// <summary>
        /// If true, the arguments are the parameter names (of global parameters) and not the actual values
        /// </summary>
        [XmlAttribute("argumentIsParamName")]
        public bool ArgumentIsParamName { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public StartProgramTask() : base()
        {

        }

        /// <summary>
        /// Helper method to maintain the status of a running process if the task is executed asynchronous
        /// </summary>
        /// <param name="name">Name of the program to execute</param>
        /// <param name="args">Optional arguments to execute the program (separated by spaces)</param>
        /// <returns>Task object which contains the current status of the process</returns>
        private static System.Threading.Tasks.Task RunAsyncronous(string name, string args)
        {
            System.Threading.Tasks.TaskCompletionSource<bool> source = new TaskCompletionSource<bool>();
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo = new System.Diagnostics.ProcessStartInfo(name, args);
            proc.EnableRaisingEvents = true;

            proc.Exited += (sender, arguments) =>
            {
                source.SetResult(true);
                proc.Dispose();
            };
            proc.Start();
            return source.Task;
        }

        /// <summary>
        /// Implemented Run method of the SubTask class
        /// </summary>
        /// <returns>Sub-task status</returns>
        public override Task.Status Run()
        {
            try
            {
                if (string.IsNullOrEmpty(this.MainValue))
                {
                    return this.SetStatus("NO_PROGRAM", "No program to execute was defined");
                }

                StringBuilder sb = new StringBuilder();
                bool executed = true;
                Parameter p;
                foreach (string arg in Arguments)
                {
                    if (this.ArgumentIsParamName == true)
                    {
                        p = Parameter.GetUserParameter(arg, this.ParentTask.DisplayOutput);
                        if (p.Valid == false)
                        {
                            return this.SetStatus("NO_PARAMETER", "The parameter with the name '" + arg + "' is not defined");
                        }
                        else
                        {
                            sb.Append(p.Value);
                        }
                    }
                    else
                    {
                        sb.Append(arg);
                    }
                    sb.Append(" ");
                }
                string argString = sb.ToString().TrimEnd(' ');
                if (this.Asynchronous == false)
                {
                    System.Diagnostics.Process.Start(this.MainValue, argString);
                    return this.SetStatus("SUCCESS_SYNC", "The process " + this.MainValue + " was executed");
                }
                else
                {
                    System.Threading.Tasks.Task result = StartProgramTask.RunAsyncronous(this.MainValue, argString);
                    while (true)
                    {
                        if (result.IsCanceled || result.IsFaulted || result.IsCompleted)
                        {
                            if (result.IsCompleted) { executed = true; }
                            else { executed = false; }
                            break;
                        }
                        else
                        {
                            System.Threading.Thread.Sleep(10);
                        }
                    }

                    if (executed == false)
                    {
                        return this.SetStatus("FAILED_ASYNC", "The process " + this.MainValue + " could not be executed");
                    }
                    else
                    {
                        return this.SetStatus("SUCCESS_ASYNC", "The process " + this.MainValue + " was executed");
                    }
                }
            }
            catch (Exception e)
            {
                return this.SetStatus("ERROR", "The process " + this.MainValue + " could not be executed\n" + e.Message);
            }
        }

        /// <summary>
        /// Implemented GetDemoFile method of the SubTask class
        /// </summary>
        /// <param name="number">Optional number to indicate several Sub-Tasks</param>
        /// <returns>Instance of the implemented class</returns>
        public override SubTask GetDemoFile(int number)
        {
            StartProgramTask t = new StartProgramTask();
            t.Name = "Start-Program-Task_" + number.ToString();
            t.Description = "This is sub-task " + number.ToString();
            t.MainValue = @"C:\temp\apps\app" + number.ToString() + ".exe";
            t.Asynchronous = true;
            if (number == 3)
            {
                t.Arguments.Add("PARAM_NAME_2");
                t.Arguments.Add("PARAM_NAME_3");
            }
            else
            {
                t.Arguments.Add("ARG1");
                t.Arguments.Add("ARG2");
                t.ArgumentIsParamName = true;
                t.Description = t.Description = t.Description + ". The arguments are the names of global parameters and not the actual values of the program arguments";
            }
            return t;
        }

        /// <summary>
        /// Returns the documentation of the status codes for the specific Sub-Task
        /// </summary>
        /// <returns>Documentation object</returns>
        public override Documentation GetDocumentationStatusCodes()
        {
            Documentation codes = new Documentation("Start Program Task", "Status Codes");
            this.AppendCommonStatusCodes(ref codes);

            this.RegisterStatusCode("FAILED_ASYNC", Task.Status.failed, "An error occurred during the asynchronous execution", ref codes);
            this.RegisterStatusCode("NO_PROGRAM", Task.Status.failed, "No program to execute was defined", ref codes);
            this.RegisterStatusCode("NO_PARAMETER", Task.Status.failed, "The parameter is not defined", ref codes);
            this.RegisterStatusCode("SUCCESS_SYNC", Task.Status.success, "Program was started successfully asynchronous", ref codes);
            this.RegisterStatusCode("SUCCESS_ASYNC", Task.Status.success, "Program was started successfully synchronous", ref codes);
            return codes;
        }

        /// <summary>
        /// Returns the documentation of the XML tags for the specific Sub-Task
        /// </summary>
        /// <returns>Documentation object</returns>
        public override Documentation GetTagDocumentationParameters()
        {
            Documentation tags = new Documentation("Start Program Task", "Tags", "The following specific tags are defined (see also the demo files or the example configuration)");
            this.AppendCommonTags(ref tags, "<startProgramItem>");
            tags.AddTuple("startProgramItem", "Main tag of a Sub-Task within the <items> tag.");
            tags.AddTuple("mainValue", "Defines full path to the program to start.");
            tags.AddTuple("argument", "Each <argument> tag within the <arguments> tag contains one (optional) program argument. If the argumentIsParamName attribute is set to true, each argument is a global parameter name instead of the actual value. In this case, the value will be resolved at runtime ");
            return tags;
        }

        /// <summary>
        /// Returns the documentation of the XML attributes for the specific Sub-Task
        /// </summary>
        /// <returns>Documentation object</returns>
        public override Documentation GetAttributesDocumentationParameters()
        {
            Documentation attributes = new Documentation("Start Program Task", "Attributes", "The following attributes are defined");
            this.AppendCommonAttributes(ref attributes, "<startProgramItem>", "StartProgram");
            attributes.AddTuple("runAsynchronous", "Indicates whether the Sub-Tasks are executed synchronous (all programs are started at the same time) or asynchronous (programs are started sequentially). Valid values are 'true' and 'false'. The attribute is part of the <startProgramItem> tag.");
            attributes.AddTuple("argumentIsParamName", "Indicates whether the arguments are the parameter names (of global parameters) and not the actual values. Valid values of the parameter are 'true' and 'false'. The attribute is part of the <startProgramItem> tag and is optional.");
            return attributes;
        }

        /// <summary>
        /// Returns the description as documentation for the specific Sub-Task
        /// </summary>
        /// <returns>Documentation object</returns>
        public override Documentation GetDocumentationDescription()
        {
            return new Documentation("Start Program Task", "Description", "Starts one or several programs with optional arguments. It is possible to define whether the sub tasks are executed synchronous or asynchronous. The later can cause freezing of the task runner if an executed application is not terminated (process still running).");
        }
    }
}
