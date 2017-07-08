using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;

namespace TaskRunner.SubTasks
{
    /// <summary>
    /// Task Runner - (c)2016 - Raphael Stoeckli
    /// This program and its code is released under the MIT license
    /// -----------------------------------------------------------
    /// Derived Sub-Task Class for deleting registry entries
    /// </summary>
    public class DeleteRegKeyTask : SubTask
    {
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
            string hive = "";
            string regValue = "";
            bool status = true;
            RegistryKey key = null;
            try
            {
                if (this.Arguments.Count < 1)
                {
                    this.Message = "The Hive of the key " + this.MainValue + " was not defined";
                    return false;
                }
                if (this.Arguments.Count < 2)
                {
                    this.Message = "The value in the key " + this.MainValue + " was not defined";
                    return false;
                }
                hive = this.Arguments[0].ToUpper();
                regValue = this.Arguments[1];
                if (hive == "HKLM")
                {
                    key = Registry.LocalMachine.OpenSubKey(this.MainValue, true);
                    if (key == null) { status = false; }
                }
                else if (hive == "HKCU")
                {
                    key = Registry.CurrentUser.OpenSubKey(this.MainValue, true);
                    if (key == null) { status = false; }
                }
                else if (hive == "HKCR")
                {
                    key = Registry.ClassesRoot.OpenSubKey(this.MainValue, true);
                    if (key == null) { status = false; }
                }
                else if (hive == "HKCC")
                {
                    key = Registry.CurrentConfig.OpenSubKey(this.MainValue, true);
                    if (key == null) { status = false; }
                }
                else if (hive == "HKU")
                {
                    key = Registry.Users.OpenSubKey(this.MainValue, true);
                    if (key == null) { status = false; }
                }
                else if (hive == "HKPD")
                {
                    key = Registry.PerformanceData.OpenSubKey(this.MainValue, true);
                    if (key == null) { status = false; }
                }
                else
                {
                    this.Message = "The hive " + hive + " is undefined";
                    return false;
                }

                if (status == false)
                {
                    this.Message = "The key " + this.MainValue + " is not present in " + hive + ". Nothing to do.";
                    return true;
                }
                else
                {
                    key.DeleteValue(regValue, false);
                    this.Message = "The value " + regValue + " in the key " + this.MainValue + " in " + hive + " was deleted";
                    return true;
                }


            }
            catch (Exception e)
            {
                this.Message = "The value " + regValue + " in  " + this.MainValue + " in " + this.Arguments[0] + " could not be deleted" + e.Message;
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
            t.MainValue = @"Software\Microsoft\Windows\CurrentVersion\Run\base_key";
            t.Arguments.Add("HKLM");
            t.Arguments.Add("value_to_delete_" + number.ToString());
            return t;
        }
    }
}
