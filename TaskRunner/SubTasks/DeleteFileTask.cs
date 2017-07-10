using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TaskRunner.SubTasks
{
    /// <summary>
    /// Task Runner - (c) 2017 - Raphael Stoeckli
    /// This program and its code is released under the MIT license
    /// -----------------------------------------------------------
    /// Derived Sub-Task Class for deleting files
    /// </summary>
    public class DeleteFileTask : SubTask
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public DeleteFileTask()
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
                if (File.Exists(this.MainValue) == false)
                {
                    this.Message = this.MainValue + " does not exist. Nothing to do";
                    this.ExecutionCode = 1;
                }
                else
                {
                    System.IO.File.Delete(this.MainValue);
                    this.Message = this.MainValue + " was deleted";
                    this.ExecutionCode = 2;
                }


                return true;
            }
            catch (Exception e)
            {
                this.Message = this.MainValue + " could not be deleted:\n" + e.Message;
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
            DeleteFileTask t = new DeleteFileTask();
            t.Name = "Delete-File-Task_" + number.ToString();
            t.Description = "This is sub-task " + number.ToString();
            t.MainValue = @"C:\temp\filesToDelete\file" + number.ToString() + ".txt";
            return t;
        }
    }
}
