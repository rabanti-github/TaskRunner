using System;
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

        [XmlAttribute("createFolders")]
        public Boolean CreateFolders { get; set; }

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
                        this.ExecutionCode = 1001;
                        return false;
                    }
                }
            try
            {
                string text = DateTime.Now.ToString(Task.DATEFORMAT);
                text = text + "\t" + Arguments[0];
                string header = Arguments[1];
                if (string.IsNullOrEmpty(header)) { header = "Date\tValue\r\n*********************************************"; }
                bool check = Utils.Log(this.MainValue, header, text);
                if (check == true)
                {
                    this.Message = "Logfile entry was written";
                    this.ExecutionCode = 1;
                    return true;
                }
                else
                {
                    this.Message = "Logfile could not be created or opened";
                    this.ExecutionCode = 1001;
                    return false;
                }
                
            }
            catch (Exception e)
            {
                this.Message = "The logfile entry could not be written\n" + e.Message;
                this.ExecutionCode = 1000;
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
            t.Description = "This is sub-task " + number.ToString();
            t.MainValue = @"C:\temp\logs\logfile.log";
            t.Arguments.Add("Text to write (separate fields using tab) no. " + number.ToString());
            t.Arguments.Add("Date\tHeaderValue1\tHeaderValue2\r\n*********************************************");
            return t;
        }
    }
}
