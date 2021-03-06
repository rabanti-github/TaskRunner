﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace TaskRunner.SubTasks
{
    /// <summary>
    /// Task Runner - (c) 2017 - Raphael Stoeckli
    /// This program and its code is released under the MIT license
    /// -----------------------------------------------------------
    /// Derived Sub-Task Class for writing log entries
    /// </summary>
    public class WriteLogTask : SubTask
    {

        /// <summary>
        /// Implemented code of the task type (04)
        /// </summary>
        [XmlIgnore]
        public override byte TaskTypeCode
        {
            get { return 0x04 ; }
        }

        /// <summary>
        /// Type of the Task / Sub-task
        /// </summary>
        [XmlIgnore]
        //public override Task.TaskType Type => Task.TaskType.WriteLog;
        public override Task.TaskType Type
        {
	        get { return Task.TaskType.WriteLog; }
        }

        /// <summary>
        /// Name of the demo file
        /// </summary>
        [XmlIgnore]
        //public override string DemoFileName => "DEMO_WriteLog.xml";
        public override string DemoFileName
        {
            get { return "DEMO_WriteLog.xml"; }
        }

        /// <summary>
        /// Name of the markdown file
        /// </summary>
        [XmlIgnore]
        public override string MarkdownFileName
        {
            get { return "WriteLog.md"; }
        }

        /// <summary>
        /// If true, the application will try to create the folder structure of the logfile if not existing
        /// </summary>
        [XmlAttribute("createFolders")]
        public bool CreateFolders { get; set; }

        /// <summary>
        /// If true, the arguments are the parameter names (of global parameters) and not the actual values
        /// </summary>
        [XmlAttribute("argumentIsParamName")]
        public bool ArgumentIsParamName { get; set; }

        /// <summary>
        /// The header of the logfile (optional)
        /// </summary>
        [XmlElement("header")]
        public string Header { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public WriteLogTask()
            : base()
        {

        }

        /// <summary>
        /// Implemented Run method of the SubTask class
        /// </summary>
        /// <returns>True if the task was executed successfully, otherwise false</returns>
        public override bool Run()
        {
            if (string.IsNullOrEmpty(this.MainValue))
            {
                this.Message = "No logfile was defined";
                this.StatusCode = 0x04;
                return false;
            }
            if (Arguments.Count < 1)
            {
                this.Message = "No text to write was defined. Pass at least one argument (e.g. a space character)";
                this.StatusCode = 0x05;
                return false;
            }
                if (this.CreateFolders == true)
                {
                    try
                    {
                        FileInfo f = new FileInfo(this.MainValue);
                        if (Directory.Exists(f.DirectoryName) == false)
                        {
                            DirectoryInfo di = Directory.CreateDirectory(f.DirectoryName);
                        }
                    }
                    catch(Exception e)
                    {
                        this.Message = "The directory of the logfile could not be created\n" + e.Message;
                        this.StatusCode = 0x03;
                        return false;
                    }
                }
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(DateTime.Now.ToString(Task.DATEFORMAT));
                sb.Append('\t');
                Parameter p;
                foreach(string item in this.Arguments)
                {
                    if (this.ArgumentIsParamName == true)
                    {
                        p = Parameter.GetParameter(item, this.ParentTask.DisplayOutput);
                        if (p.Valid == false)
                        {
                            this.Message = "The parameter with the name '" + item + "' is not defined";
                            this.StatusCode = 0x05;
                            return false;
                        }
                        else
                        {
                            sb.Append(p.Value);
                        }
                    }
                    else
                    {
                        sb.Append(item);
                    }
                    sb.Append('\t');
                }
                string text = sb.ToString().TrimEnd('\t');
                if (string.IsNullOrEmpty(this.Header)) { this.Header = "Date\tValue\r\n*********************************************"; }
                bool check = Utils.Log(this.MainValue, this.Header, text);
                if (check == true)
                {
                    this.Message = "Logfile entry was written";
                    this.StatusCode = 0x01;
                    return true;
                }
                else
                {
                    this.Message = "Logfile could not be created or opened";
                    this.StatusCode = 0x02;
                    return false;
                }
                
            }
            catch (Exception e)
            {
                this.Message = "The logfile entry could not be written\n" + e.Message;
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
            WriteLogTask t = new WriteLogTask();
            t.Name = "Write-Log-Task_" + number.ToString();
            t.CreateFolders = true;
            t.Header = "Date\tHeaderValue1\tHeaderValue2\r\n*********************************************";
            t.Description = "This is sub-task " + number.ToString();
            t.MainValue = @"C:\temp\logs\logfile.log";
            if (number == 3)
            {
                t.ArgumentIsParamName = true;
                t.Arguments.Add("PARAM_NAME_2");
                t.Arguments.Add("PARAM_NAME_3");
                t.Arguments.Add("PARAM_NAME_4");
                t.Description = t.Description = t.Description + ". The arguments are the names of global parameters and not the actual values to write";
            }
            else
            {
                t.Arguments.Add("Text token to write 1");
                t.Arguments.Add("Text token to write 2");
                t.Arguments.Add("Text token to write 3");
            }
            return t;
        }

        /// <summary>
        /// Returns the documentation of the status codes for the specific Sub-Task
        /// </summary>
        /// <returns>Documentation object</returns>
        public override Documentation GetDocumentationStatusCodes()
        {
            Documentation codes = new Documentation("Write Log Task", "Status Codes");
            codes.AddTuple(this.PrintStatusCode(true, 0x01), "Logfile entry was written");
            codes.AddTuple(this.PrintStatusCode(false, 0x01), "The logfile entry could not be written due to an unknown reason");
            codes.AddTuple(this.PrintStatusCode(false, 0x02), "Logfile could not be created or opened");
            codes.AddTuple(this.PrintStatusCode(false, 0x03), "The directory of the logfile could not be created");
            codes.AddTuple(this.PrintStatusCode(false, 0x04), "No logfile was defined");
            codes.AddTuple(this.PrintStatusCode(false, 0x05), "The parameter is not defined");
            return codes;
        }

        /// <summary>
        /// Returns the documentation of the XML tags for the specific Sub-Task
        /// </summary>
        /// <returns>Documentation object</returns>
        public override Documentation GetTagDocumentationParameters()
        {
            Documentation tags = new Documentation("Write Log Task", "Tags", "The following specific tags are defined (see also the demo files or the example configuration)");
            this.AppendCommonTags(ref tags, "<writeLogItem>");
            tags.AddTuple("writeLogItem", "Main tag of a Sub-Task within the <items> tag.");
            tags.AddTuple("mainValue", "Defines filename and path of the logfile.");
            tags.AddTuple("header", "Tag to describe a header of a logfile. The particular fields are divided by tabs (\\t).");
            tags.AddTuple("argument", "Each <argument> tag within the <arguments> tag contains one filed of the log entry. No tabs are required. If the argumentIsParamName attribute is set to true, each argument is a global parameter name instead of the actual value. In this case, the value will be resolved at runtime");

            return tags;
        }

        /// <summary>
        /// Returns the documentation of the XML attributes for the specific Sub-Task
        /// </summary>
        /// <returns>Documentation object</returns>
        public override Documentation GetAttributesDocumentationParameters()
        {
            Documentation attributes = new Documentation("Write Log Task", "Attributes", "The following attributes are defined");
            this.AppendCommonAttributes(ref attributes, "<writeLogItem>", "WriteLog");
            attributes.AddTuple("createFolders", "Indicates whether a missing folder structure will be created when writing a log entry to a new file. Valid values are 'true' and 'false'. The attribute is part of the <writeLogItem> tag and is optional.");
            attributes.AddTuple("argumentIsParamName", "Indicates whether the arguments are the parameter names (of global parameters) and not the actual values. Valid values of the parameter are 'true' and 'false'. The attribute is part of the <writeLogItem> tag and is optional.");
            return attributes;
        }

        /// <summary>
        /// Returns the description as documentation for the specific Sub-Task
        /// </summary>
        /// <returns>Documentation object</returns>
        public override Documentation GetDocumentationDescription()
        {
            return new Documentation("Write Log Task", "Description", "Writes a defined text with the time stamp of the execution time into the defined log file. The logfile header is optional and can be passed as argument (see demo files).");
        }
    }
}
