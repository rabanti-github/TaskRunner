using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace TaskRunner.SubTasks
{
    /// <summary>
    /// Task Runner - (c) 2018 - Raphael Stoeckli
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
         public override Task.TaskType Type
        {
            get { return Task.TaskType.DeleteRegKey; }
        }

        /// <summary>
        /// Name of the demo file
        /// </summary>
        [XmlIgnore]
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
        public DeleteRegKeyTask() : base()
        {

        }

        /// <summary>
        /// Implemented Run method of the SubTask class
        /// </summary>
        /// <returns>Sub-task status</returns>
        public override Task.Status Run()
        {
            bool status = true;
            RegistryKey key = null;
            try
            {
                if (string.IsNullOrEmpty(this.MainValue))
                {
                    return this.SetStatus("NO_KEY", "No key was defined");
                }
                if (string.IsNullOrEmpty(this.Hive))
                {
                    return this.SetStatus("NO_HIVE", "The Hive of the key " + this.MainValue + " was not defined");
                }
                if (this.Arguments.Count < 1)
                {
                    return this.SetStatus("NO_VALUE", "No value in the key " + this.MainValue + " to delete was defined");
                }
                this.Hive = this.Hive.ToUpper();
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
                    return this.SetStatus("INVALID_HIVE", "The hive " + this.Hive + " is undefined");
                }

                if (status == false)
                {
                    return this.SetStatus("SUCCESS_NO_ACTION", "The key " + this.MainValue + " is not present in " + this.Hive + ". Nothing to do");
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
                            p = Parameter.GetUserParameter(item, this.ParentTask.DisplayOutput);
                            if (p.Valid == false)
                            {
                                return this.SetStatus("NO_PARAMETER", "The parameter with the name '" + item + "' is not defined");
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
                        return this.SetStatus("SUCCESS_NO_ACTION2", "No value to delete was found in the key " + this.MainValue + ". Nothing to do");
                    }
                    else
                    {
                        return this.SetStatus("SUCCESS_DELETED", counter + " values were deleted in the key " + this.MainValue + " in " + this.Hive);
                    }
                }
            }
            catch (Exception e)
            {
                return this.SetStatus("ERROR", "The value(s) in the key " + this.MainValue + " in " + this.Hive + " could not be deleted" + e.Message);
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
            this.AppendCommonStatusCodes(ref codes);

            this.RegisterStatusCode("NO_HIVE", Task.Status.failed, "The hive was not defined", ref codes);
            this.RegisterStatusCode("NO_VALUE", Task.Status.failed, "No value to delete was defined", ref codes);
            this.RegisterStatusCode("INVALID_HIVE", Task.Status.failed, "The hive is not defined / unknown", ref codes);
            this.RegisterStatusCode("NO_KEY", Task.Status.failed, "No key to check was defined", ref codes);
            this.RegisterStatusCode("NO_PARAMETER", Task.Status.failed, "The parameter is not defined", ref codes);

            this.RegisterStatusCode("SUCCESS_DELETED", Task.Status.success, "The value was deleted successfully", ref codes);
            this.RegisterStatusCode("SUCCESS_NO_ACTION", Task.Status.success, "The key is not existing. Nothing to do", ref codes);
            this.RegisterStatusCode("SUCCESS_NO_ACTION2", Task.Status.success, "The value was not found in the key. Nothing to do", ref codes);
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
