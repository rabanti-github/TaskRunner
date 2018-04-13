using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.ServiceProcess;

namespace TaskRunner.SubTasks
{
    /// <summary>
    /// Task Runner - (c) 2018 - Raphael Stoeckli
    /// This program and its code is released under the MIT license
    /// -----------------------------------------------------------
    /// Derived Sub-Task Class to control Windows services
    /// </summary>
    public class ControlServiceTask : SubTask
    {

        private int timeout;

        /// <summary>
        /// Implemented code of the task type (05)
        /// </summary>
        [XmlIgnore]
        public override byte TaskTypeCode
        {
            get { return 0x05; }
        }

        /// <summary>
        /// Action of the task (start, stop, restart, pause, resume)
        /// </summary>
        [XmlAttribute("action")]
        public string Action { get; set; }

        /// <summary>
        /// If true, the program will wait until the target status or timeout is reached 
        /// </summary>
        [XmlAttribute("waitForStatus")]
        public bool WaitForStatus { get; set; }

        /// <summary>
        /// Timeout in seconds
        /// </summary>
        [XmlAttribute("timeout")]
        public string Timeout { get; set; }

        /// <summary>
        /// Type of the Task / Sub-task
        /// </summary>
        [XmlIgnore]
        public override Task.TaskType Type
        {
	        get { return Task.TaskType.ControlService; }
        }

        /// <summary>
        /// Name of the demo file
        /// </summary>
        [XmlIgnore]
        public override string DemoFileName
        {
            get { return "DEMO_ControlService.xml"; }
        }

        /// <summary>
        /// Name of the markdown file
        /// </summary>
        [XmlIgnore]
        public override string MarkdownFileName
        {
            get { return "ControlService.md"; }
        }

        /// <summary>
        /// If true, the arguments are the parameter names (of global parameters) and not the actual values
        /// </summary>
        [XmlAttribute("argumentIsParamName")]
        public bool ArgumentIsParamName { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public ControlServiceTask()
            : base()
        {
           // this.Action = ActionType.stop;
        }

        /// <summary>
        /// Implemented Run method of the SubTask class
        /// </summary>
        /// <returns>Sub-task status</returns>
        public override Task.Status Run()
        {
            if (string.IsNullOrEmpty(this.MainValue))
            {
                return this.SetStatus("NO_SVC", "No service to control was defined");
            }
            if (string.IsNullOrEmpty(this.Action))
            {
                return this.SetStatus("NO_ACTION", "No action to control a service was defined");
            }
            string action = this.Action.ToLower();
            if (action != "stop" && action != "start" && action != "restart" && action != "pause" && action != "resume")
            {
                return this.SetStatus("NO_CONTROL", "An undefined control action was defined");
            }
            if (string.IsNullOrEmpty(this.Timeout))
            {
                return this.SetStatus("NO_TIMEOUT", "No timeout was defined");
            }
            if (int.TryParse(this.Timeout, out this.timeout) == false)
            {
                return this.SetStatus("INVALID_TIMEOUT", "The timeout value is invalid");
            }
            if (this.timeout < 0)
            {
                return this.SetStatus("INVALID_TIMEOUT", "The timeout value is invalid");
            }

            ServiceController[] svcs;
            ServiceController svc = null;
            string err = "";
            if (this.Arguments.Count < 1)
            {
                svcs = ServiceController.GetServices();
            }
            else
            {
                svcs = ServiceController.GetServices(this.Arguments[0]);
                err = "at machine '" + this.Arguments[0] + "' ";
            }
            foreach(ServiceController s in svcs)
            {
                if (s.ServiceName == this.MainValue)
                {
                    if (this.Arguments.Count < 1)
                    {
                        svc = new ServiceController(s.ServiceName, s.MachineName);
                    }
                    else
                    {
                        if (this.ArgumentIsParamName == true)
                        {
                            Parameter p = Parameter.GetUserParameter(this.Arguments[0], this.ParentTask.DisplayOutput);
                            if (p.Valid == false)
                            {
                                return this.SetStatus("INVALID_PARAM", "The parameter with the name '" + this.Arguments[0] + "' is not defined");
                            }
                            else
                            {
                                svc = new ServiceController(s.ServiceName, p.Value);
                            }
                        }
                        else
                        {
                            svc = new ServiceController(s.ServiceName, this.Arguments[0]);
                        }  
                    }
                    break;
                }
            }
            if (svc == null)
            {
                return this.SetStatus("INVALID_SVC", "The service with the name " + this.MainValue + err + "was not found");
            }

            try
            {
                TimeSpan ts = new TimeSpan(this.timeout * 10000000);
                bool timeOutStatus = false;
                string status = "ERROR";
                switch (action)
                {
                    case "stop":
                        svc.Stop();
                        if (WaitForStatus == true) { svc.WaitForStatus(ServiceControllerStatus.Stopped, ts); }
                        svc.Refresh();
                        if (svc.Status != ServiceControllerStatus.Stopped) { timeOutStatus = true; }
                        status = "SUCCESS_STOPPED";
                        break;
                    case "start":
                        svc.Start();
                        if (WaitForStatus == true) { svc.WaitForStatus(ServiceControllerStatus.Running, ts); }
                        svc.Refresh();
                        if (svc.Status != ServiceControllerStatus.Running) { timeOutStatus = true; }
                        status = "SUCCESS_STARTED";
                        break;
                    case "restart":
                        
                        svc.Stop();
                        svc.WaitForStatus(ServiceControllerStatus.Stopped, ts);
                        svc.Start();
                        if (WaitForStatus == true) { svc.WaitForStatus(ServiceControllerStatus.Running, ts); }
                        svc.Refresh();
                        if (svc.Status != ServiceControllerStatus.Running) { timeOutStatus = true; }
                        status = "SUCCESS_RESTARTED";
                        break;
                    case "pause":
                        svc.Pause();
                        if (WaitForStatus == true) { svc.WaitForStatus(ServiceControllerStatus.Paused, ts); }
                        svc.Refresh();
                        if (svc.Status != ServiceControllerStatus.Paused) { timeOutStatus = true; }
                        status = "SUCCESS_PAUSED";
                        break;
                    case "resume":
                        svc.Continue();
                        if (WaitForStatus == true) { svc.WaitForStatus(ServiceControllerStatus.Running, ts); }
                        svc.Refresh();
                        if (svc.Status != ServiceControllerStatus.Running) { timeOutStatus = true; }
                        status = "SUCCESS_RESUMED";
                        break;
                    default:
                        break;
                }
                if (timeOutStatus == true)
                {
                    return this.SetStatus("TIMEOUT_REACHED", "The service '" + this.MainValue + " could not reach the end status of the action " + action + ". Timeout was reached");
                }
                return this.SetStatus(status, "The action '" + action + "' was performed successfully on service '" + this.MainValue + "'");
            }
            catch(Exception e)
            {
                return this.SetStatus("ERROR", "The service '" + this.MainValue + " could not reach status '" + action + "'\n" + e.Message);            
            }
        }

        /// <summary>
        /// Implemented GetDemoFile method of the SubTask class
        /// </summary>
        /// <param name="number">Optional number to indicate several Sub-Tasks</param>
        /// <returns>Instance of the implemented class</returns>
        public override SubTask GetDemoFile(int number)
        {
            ControlServiceTask t = new ControlServiceTask();
            t.Name = "Control-Service-Task_" + number.ToString();
            t.Description = "This is sub-task " + number.ToString();
            t.Timeout = "10";
            t.Action = "stop";
            t.WaitForStatus = true;
            t.MainValue = "WindowsService_" + number.ToString();
            if (number == 2)
            {
                t.Arguments.Add("remote_machine_name");
                t.WaitForStatus = false;
            }
            else if (number == 3)
            {
                t.Arguments.Add("PARAM_NAME_2");
                t.WaitForStatus = false;
                t.ArgumentIsParamName = true;
                t.Description = t.Description = t.Description + ". The argument is the name of a global parameter and not the actual value of the remote machine name";
            }
            return t;
        }

        /// <summary>
        /// Returns the documentation of the status codes for the specific Sub-Task
        /// </summary>
        /// <returns>Documentation object</returns>
        public override Documentation GetDocumentationStatusCodes()
        {
            Documentation codes = new Documentation("Control Service Task", "Status Codes");
            this.AppendCommonStatusCodes(ref codes);
            this.RegisterStatusCode("NO_SVC", Task.Status.failed, "No service to control was defined", ref codes);
            this.RegisterStatusCode("NO_ACTION", Task.Status.failed, "No action to control a service was defined", ref codes);
            this.RegisterStatusCode("NO_CONTROL", Task.Status.failed, "An undefined control action was defined", ref codes);
            this.RegisterStatusCode("NO_TIMEOUT", Task.Status.failed, "No timeout value was defined", ref codes);
            this.RegisterStatusCode("INVALID_TIMEOUT", Task.Status.failed, "The timeout value is invalid", ref codes);
            this.RegisterStatusCode("INVALID_PARAM", Task.Status.failed, "The parameter is not defined", ref codes);
            this.RegisterStatusCode("INVALID_SVC", Task.Status.failed, "The service was not found", ref codes);

            this.RegisterStatusCode("SUCCESS_STOPPED", Task.Status.success, "The service was stopped successfully", ref codes);
            this.RegisterStatusCode("SUCCESS_STARTED", Task.Status.success, "The service was started successfully", ref codes);
            this.RegisterStatusCode("SUCCESS_RESTARTED", Task.Status.success, "The service was restarted successfully", ref codes);
            this.RegisterStatusCode("SUCCESS_PAUSED", Task.Status.success, "The service was paused successfully", ref codes);
            this.RegisterStatusCode("SUCCESS_RESUMED", Task.Status.success, "The service was resumed successfully", ref codes);
            return codes;
        }

        /// <summary>
        /// Returns the documentation of the XML tags for the specific Sub-Task
        /// </summary>
        /// <returns>Documentation object</returns>
        public override Documentation GetTagDocumentationParameters()
        {
            Documentation tags = new Documentation("Control Service Task", "Tags", "The following specific tags are defined (see also the demo files or the example configuration)");
            this.AppendCommonTags(ref tags, "<controlServiceItem>");
            tags.AddTuple("controlServiceItem", "Main tag of a Sub-Task within the <items> tag");
            tags.AddTuple("mainValue", "Defines the name of the service");
            tags.AddTuple("argument", "The optional <argument> tag within the <arguments> tag contains the name of the remote machine if applicable.  If the argumentIsParamName attribute is set to true, each argument is a global parameter name instead of the actual value. In this case, the value will be resolved at runtime");
            return tags;
        }

        /// <summary>
        /// Returns the documentation of the XML attributes for the specific Sub-Task
        /// </summary>
        /// <returns>Documentation object</returns>
        public override Documentation GetAttributesDocumentationParameters()
        {
            Documentation attributes = new Documentation("Control Service Task", "Attributes", "The following attributes are defined");
            this.AppendCommonAttributes(ref attributes, "<controlServiceItem>", "ControlService");
            attributes.AddTuple("action", "Indicates the action of the task. Valid values are 'start', 'stop', 'restart', 'pause' and 'resume'. The attribute is part of the <controlServiceItem> tag.");
            attributes.AddTuple("waitForStatus", "Indicates whether TaskRunner waits until the final status of the action is reached. Valid values are 'true' and 'false'. The attribute is part of the <controlServiceItem> tag.");
            attributes.AddTuple("timeout", "Indicates the number of seconds to wait until abort of the attempt. Valid values are 0 and positive integers. The attribute is part of the <controlServiceItem> tag.");
            attributes.AddTuple("argumentIsParamName", "Indicates whether the arguments are the parameter names (of global parameters) and not the actual values. Valid values of the parameter are 'true' and 'false'. The attribute is part of the <controlServiceItem> tag and is optional.");
            return attributes;
        }

        /// <summary>
        /// Returns the description as documentation for the specific Sub-Task
        /// </summary>
        /// <returns>Documentation object</returns>
        public override Documentation GetDocumentationDescription()
        {
            return new Documentation("Control Service Task", "Description", "The tasks controls a Windows Service. Possible options are start, stop, restart, pause and resume. An additional remote machine name can be defined. Note that administrative permissions may be necessary to execute such a task.");
        }

    }
}
