using System;
using System.Collections.Generic;
using System.Text;

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
            try
            {
                string text = DateTime.Now.ToString(Task.DATEFORMAT);
                text = text + "\t" + Arguments[0];
                string header = Arguments[1];
                if (string.IsNullOrEmpty(header)) { header = "Date\tValue\r\n*********************************************"; }
                bool check = Utils.Log(this.MainValue, header, text);
                this.Message = "Logfile entry was written";
                return true;
            }
            catch (Exception e)
            {
                this.Message = "The logfile entry could not be written" + e.Message;
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
            t.Description = "This is sub-task " + number.ToString();
            t.MainValue = @"C:\temp\logs\logfile.log";
            t.Arguments.Add("Text to write (separate fields using tab) no. " + number.ToString());
            t.Arguments.Add("Date\tHeaderValue1\tHeaderValue2\r\n*********************************************");
            return t;
        }
    }
}
