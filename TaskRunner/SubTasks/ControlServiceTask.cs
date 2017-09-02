using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.ServiceProcess;

namespace TaskRunner.SubTasks
{
    /// <summary>
    /// Task Runner - (c) 2017 - Raphael Stoeckli
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


[XmlAttribute("action")]
        //public ActionType Action { get; set; }
        public string Action { get; set; }

        [XmlAttribute("waitForStatus")]
        public bool WaitForStatus { get; set; }

        [XmlAttribute("timeout")]
        public string Timeout { get; set; }

        /// <summary>
        /// Type of the Task / Sub-task
        /// </summary>
        [XmlIgnore]
        //public override Task.TaskType Type => Task.TaskType.ControlService;
        public override Task.TaskType Type
        {
	        get { return Task.TaskType.ControlService; }
        }
        /// <summary>
        /// Name of the demo file
        /// </summary>
        [XmlIgnore]
        //public override string DemoFileName => "DEMO_ControlService.xml";
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
        /// <returns>True if the task was executed successfully, otherwise false</returns>
        public override bool Run()
        {

            if (string.IsNullOrEmpty(this.MainValue))
            {
                this.Message = "No service to control was defined";
                this.StatusCode = 0x02;
                return false;
            }
            if (string.IsNullOrEmpty(this.Action))
            {
                this.Message = "No action to control a service was defined";
                this.StatusCode = 0x03;
                return false;
            }
            string action = this.Action.ToLower();
            if (action != "stop" && action != "start" && action != "restart" && action != "pause" && action != "resume")
            {
                this.Message = "An undefined control action was defined";
                this.StatusCode = 0x04;
                return false;
            }
            if (string.IsNullOrEmpty(this.Timeout))
            {
                this.Message = "No timeout was defined";
                this.StatusCode = 0x05;
                return false;
            }
            if (int.TryParse(this.Timeout, out this.timeout) == false)
            {
                this.Message = "The timeout value is invalid";
                this.StatusCode = 0x06;
                return false;
            }
            if (this.timeout < 0)
            {
                this.Message = "The timeout value is invalid";
                this.StatusCode = 0x06;
                return false;
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
                        svc = new ServiceController(s.ServiceName, this.Arguments[0]);
                    }
                    break;
                }
            }
            if (svc == null)
            {
                this.Message = "The service with the name " + this.MainValue + err + "was not found";
                this.StatusCode = 0x03; // redefine
                return false;
            }

            try
            {
                TimeSpan ts = new TimeSpan(this.timeout * 10000000);
                bool timeOutStatus = false;
                switch (action)
                {
                    case "stop":
                        svc.Stop();
                        if (WaitForStatus == true) { svc.WaitForStatus(ServiceControllerStatus.Stopped, ts); }
                        svc.Refresh();
                        if (svc.Status != ServiceControllerStatus.Stopped) { timeOutStatus = true; }
                        this.StatusCode = 0x01;
                        break;
                    case "start":
                        svc.Start();
                        if (WaitForStatus == true) { svc.WaitForStatus(ServiceControllerStatus.Running, ts); }
                        svc.Refresh();
                        if (svc.Status != ServiceControllerStatus.Running) { timeOutStatus = true; }
                        this.StatusCode = 0x02;
                        break;
                    case "restart":
                        
                        svc.Stop();
                        svc.WaitForStatus(ServiceControllerStatus.Stopped, ts);
                        svc.Start();
                        if (WaitForStatus == true) { svc.WaitForStatus(ServiceControllerStatus.Running, ts); }
                        svc.Refresh();
                        if (svc.Status != ServiceControllerStatus.Running) { timeOutStatus = true; }
                        this.StatusCode = 0x03;
                        break;
                    case "pause":
                        svc.Pause();
                        if (WaitForStatus == true) { svc.WaitForStatus(ServiceControllerStatus.Paused, ts); }
                        svc.Refresh();
                        if (svc.Status != ServiceControllerStatus.Paused) { timeOutStatus = true; }
                        this.StatusCode = 0x04;
                        break;
                    case "resume":
                        svc.Continue();
                        if (WaitForStatus == true) { svc.WaitForStatus(ServiceControllerStatus.Running, ts); }
                        svc.Refresh();
                        if (svc.Status != ServiceControllerStatus.Running) { timeOutStatus = true; }
                        this.StatusCode = 0x05;
                        break;
                    default:
                        break;
                }
                if (timeOutStatus == true)
                {
                    this.Message = "The service '" + this.MainValue + " could not reach the end status of the action " + action + ". Maybe timeout reached";
                    this.StatusCode = 0x01;
                    return false;
                }
                this.Message = "The action '"+ action + "' was performed successfully on service '" + this.MainValue + "'";
                return true;   
  

            }
            catch(Exception e)
            {
                this.Message = "The service '" + this.MainValue  + " could not reach status '" + action + "'\n" + e.Message;
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
            ControlServiceTask t = new ControlServiceTask();
            t.Name = "Control-Service-Task_" + number.ToString();
            t.Description = "This is sub-task " + number.ToString();
            t.Timeout = "10";
            t.Action = "stop";
            t.WaitForStatus = true;
            t.MainValue = "WindowsService_" + number.ToString();
            if (number > 1)
            {
                t.Arguments.Add("remote_machine_name");
                t.WaitForStatus = false;
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
            codes.AddTuple(this.PrintStatusCode(true, 0x01), "The service was stopped successfully");
            codes.AddTuple(this.PrintStatusCode(true, 0x02), "The service was started successfully");
            codes.AddTuple(this.PrintStatusCode(true, 0x03), "The service was restarted successfully");
            codes.AddTuple(this.PrintStatusCode(true, 0x04), "The service was paused successfully");
            codes.AddTuple(this.PrintStatusCode(true, 0x05), "The service was resumed successfully");
            codes.AddTuple(this.PrintStatusCode(false, 0x01), "The action could not be performed on the service due to an unknown reason");
            codes.AddTuple(this.PrintStatusCode(false, 0x02), "No service to control was defined");
            codes.AddTuple(this.PrintStatusCode(false, 0x03), "No action to control a service was defined");
            codes.AddTuple(this.PrintStatusCode(false, 0x04), "An undefined control action was defined");
            codes.AddTuple(this.PrintStatusCode(false, 0x05), "No timeout value was defined");
            codes.AddTuple(this.PrintStatusCode(false, 0x06), "The timeout value is invalid");
            return codes;
        }

        /// <summary>
        /// Returns the documentation of the XML tags for the specific Sub-Task
        /// </summary>
        /// <returns>Documentation object</returns>
        public override Documentation GetTagDocumentationParameters()
        {
            Documentation tags = new Documentation("Control Service Task", "Tags", "The following specific tags are defined (see also demo files)");
            this.AppendCommonTags(ref tags, "<controlServiceItem>");
            tags.AddTuple("controlServiceItem", "Main tag of a Sub-Task within the <items> tag");
            tags.AddTuple("mainValue", "Defines the name of the service");
            tags.AddTuple("argument", "The optional <argument> tag within the <arguments> tag contains the name of the remote machine if applicable");
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
