using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace TaskRunner.SubTasks
{
    /// <summary>
    /// Task Runner - (c) 2017 - Raphael Stoeckli
    /// This program and its code is released under the MIT license
    /// -----------------------------------------------------------
    /// Derived Sub-Task Class for deleting registry entries
    /// </summary>
    public class DeleteRegKeyTask : SubTask
    {

        /// <summary>
        /// Implemented code of the task type (02)
        /// </summary>
        [XmlIgnore]
        public override byte TaskTypeCode
        {
            get { return 0x02; }
        }

        /// <summary>
        /// Type of the Task / Sub-task
        /// </summary>
        [XmlIgnore]
        //public override Task.TaskType Type => Task.TaskType.DeleteRegKey;
         public override Task.TaskType Type
        {
            get { return Task.TaskType.DeleteRegKey; }
        }

        /// <summary>
        /// Name of the demo file
        /// </summary>
        [XmlIgnore]
       // public override string DemoFileName => "DEMO_DeleteregKey.xml";
        public override string DemoFileName
        {
            get { return "DEMO_DeleteRegKey.xml"; }
        }

        /// <summary>
        /// Name of the markdown file
        /// </summary>
        [XmlIgnore]
        public override string MarkdownFileName
        {
            get { return "DeleteRegKey.md"; }
        }

        /// <summary>
        /// The hive of the registry key (e.g. HKLM or HKCU)
        /// </summary>
        [XmlAttribute("hive")]
        public string Hive { get; set; }

        /// <summary>
        /// If true, the arguments are the parameter names (of global parameters) and not the actual values
        /// </summary>
        [XmlAttribute("argumentIsParamName")]
        public bool ArgumentIsParamName { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public DeleteRegKeyTask()
            : base()
        {

        }

        /// <summary>
        /// Implemented Run method of the SubTask class
        /// </summary>
        /// <returns>True if the task was executed successfully, otherwise false</returns>
        public override bool Run()
        {
            //string hive = "";
            //string regValue = "";
            bool status = true;
            RegistryKey key = null;
            try
            {
                if (string.IsNullOrEmpty(this.MainValue))
                {
                    this.Message = "No key was defined";
                    this.StatusCode = 0x05;
                    return false;
                }
                if (string.IsNullOrEmpty(this.Hive))
                {
                    this.Message = "The Hive of the key " + this.MainValue + " was not defined";
                    this.StatusCode = 0x02;
                    return false;
                }
                if (this.Arguments.Count < 1)
                {
                    this.Message = "No value in the key " + this.MainValue + " to delete was defined";
                    this.StatusCode = 0x03;
                    return false;
                }
                this.Hive = this.Hive.ToUpper();
                //regValue = this.Arguments[1];
                if (this.Hive == "HKLM")
                {
                    key = Registry.LocalMachine.OpenSubKey(this.MainValue, true);
                    if (key == null) { status = false; }
                }
                else if (this.Hive == "HKCU")
                {
                    key = Registry.CurrentUser.OpenSubKey(this.MainValue, true);
                    if (key == null) { status = false; }
                }
                else if (this.Hive == "HKCR")
                {
                    key = Registry.ClassesRoot.OpenSubKey(this.MainValue, true);
                    if (key == null) { status = false; }
                }
                else if (this.Hive == "HKCC")
                {
                    key = Registry.CurrentConfig.OpenSubKey(this.MainValue, true);
                    if (key == null) { status = false; }
                }
                else if (this.Hive == "HKU")
                {
                    key = Registry.Users.OpenSubKey(this.MainValue, true);
                    if (key == null) { status = false; }
                }
                else if (this.Hive == "HKPD")
                {
                    key = Registry.PerformanceData.OpenSubKey(this.MainValue, true);
                    if (key == null) { status = false; }
                }
                else
                {
                    this.Message = "The hive " + this.Hive + " is undefined";
                    this.StatusCode = 0x04;
                    return false;
                }

                if (status == false)
                {
                    this.Message = "The key " + this.MainValue + " is not present in " + this.Hive + ". Nothing to do";
                    this.StatusCode = 0x02;
                    return true;
                }
                else
                {
                    int counter = 0;
                    object o;
                    Parameter p;
                    string value;
                    foreach(string item in this.Arguments)
                    {

                        if (this.ArgumentIsParamName == true)
                        {
                            p = Parameter.GetParameter(item, this.ParentTask.DisplayOutput);
                            if (p.Valid == false)
                            {
                                this.Message = "The parameter with the name '" + item + "' is not defined";
                                this.StatusCode = 0x06;
                                return false;
                            }
                            else
                            {
                                value = p.Value;
                            }
                        }
                        else
                        {
                            value = item;
                        }
                        o = key.GetValue(value);
                        if (o != null)
                        {
                            key.DeleteValue(value, false);
                            counter++;
                        }
                    }
                    if (counter == 0)
                    {
                        this.Message = "No value to delete was found in the key " + this.MainValue + ". Nothing to do";
                        this.StatusCode = 0x03;
                        return true;
                    }
                    else
                    {
                        this.Message = counter + " values were deleted in the key " + this.MainValue + " in " + this.Hive;
                        this.StatusCode = 0x01;
                        return true;
                    }
                }


            }
            catch (Exception e)
            {
                this.Message = "The value(s) in the key " + this.MainValue + " in " + this.Hive + " could not be deleted" + e.Message;
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
            DeleteRegKeyTask t = new DeleteRegKeyTask();
            t.Name = "Delete-Registry-Task_" + number.ToString();
            t.Description = "This is sub-task " + number.ToString();
            t.Hive = "HKLM";
            t.MainValue = @"Software\Microsoft\Windows\CurrentVersion\Run\base_key" + number.ToString();
            if (number == 3)
            {
                t.Arguments.Add("PARAM_NAME_2");
                t.Arguments.Add("PARAM_NAME_3");
                t.Arguments.Add("PARAM_NAME_4");
                t.ArgumentIsParamName = true;
                t.Description = t.Description = t.Description + ". The arguments are the names of global parameters and not the actual values to delete in the registry";
            }
            else
            {
                t.Arguments.Add("value_to_delete_1");
                t.Arguments.Add("value_to_delete_2");
                t.Arguments.Add("value_to_delete_3");
            }
            return t;
        }

        /// <summary>
        /// Returns the documentation of the status codes for the specific Sub-Task
        /// </summary>
        /// <returns>Documentation object</returns>
        public override Documentation GetDocumentationStatusCodes()
        {
            Documentation codes = new Documentation("Delete Registry-Key (Value) Task", "Status Codes");
            codes.AddTuple(this.PrintStatusCode(true, 0x01), "The value was deleted successfully");
            codes.AddTuple(this.PrintStatusCode(true, 0x02), "The key is not existing. Nothing to do");
            codes.AddTuple(this.PrintStatusCode(true, 0x03), "The value was not found in the key. Nothing to do");
            codes.AddTuple(this.PrintStatusCode(false, 0x01), "The value could not be deleted due to an unknown reason");
            codes.AddTuple(this.PrintStatusCode(false, 0x02), "The hive was not defined");
            codes.AddTuple(this.PrintStatusCode(false, 0x03), "No value to delete was defined");
            codes.AddTuple(this.PrintStatusCode(false, 0x04), "The hive is not defined / unknown");
            codes.AddTuple(this.PrintStatusCode(false, 0x05), "No key to check was defined");
            codes.AddTuple(this.PrintStatusCode(false, 0x06), "The parameter is not defined");
            return codes;
        }

        /// <summary>
        /// Returns the documentation of the XML tags for the specific Sub-Task
        /// </summary>
        /// <returns>Documentation object</returns>
        public override Documentation GetTagDocumentationParameters()
        {
            Documentation tags = new Documentation("Delete Registry-Key (Value) Task", "Tags", "The following specific tags are defined (see also the demo files or the example configuration)");
            this.AppendCommonTags(ref tags, "<deleteRegKeyItem>");
            tags.AddTuple("deleteRegKeyItem", "Main tag of a Sub-Task within the <items> tag");
            tags.AddTuple("mainValue", "Defines the path to the registry key without the hive");
            tags.AddTuple("argument", "Each <argument> tag within the <arguments> tag contains one value to delete within the key. If the argumentIsParamName attribute is set to true, each argument is a global parameter name instead of the actual value. In this case, the value will be resolved at runtime");
            return tags;
        }

        /// <summary>
        /// Returns the documentation of the XML attributes for the specific Sub-Task
        /// </summary>
        /// <returns>Documentation object</returns>
        public override Documentation GetAttributesDocumentationParameters()
        {
            Documentation attributes = new Documentation("Delete Registry-Key (Value) Task", "Attributes", "The following attributes are defined");
            this.AppendCommonAttributes(ref attributes, "<deleteRegKeyItem>", "DeleteRegKey");
            attributes.AddTuple("hive", "Indicates which registry hive is accessed. Valid values are 'HKLM', 'HKCU', 'HKCR', 'HKCC', 'HKU' and 'HKPD'. The attribute is part of the <deleteRegKeyItem> tag.");
            attributes.AddTuple("argumentIsParamName", "Indicates whether the arguments are the parameter names (of global parameters) and not the actual values. Valid values of the parameter are 'true' and 'false'. The attribute is part of the <deleteRegKeyItem> tag and is optional.");
            return attributes;
        }

        /// <summary>
        /// Returns the description as documentation for the specific Sub-Task
        /// </summary>
        /// <returns>Documentation object</returns>
        public override Documentation GetDocumentationDescription()
        {
            return new Documentation("Delete Registry-Key (Value) Task", "Description", "The task deletes a value of a registry key in the Windows registry. Several hives like HKLM or HKCU can be defined. Note that write permission to the registry must be granted to execute such a task.");
        }
    }
}
