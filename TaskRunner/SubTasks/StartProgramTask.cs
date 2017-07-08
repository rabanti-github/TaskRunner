using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TaskRunner.SubTasks
{
    /// <summary>
    /// Task Runner - (c)2016 - Raphael Stoeckli
    /// This program and its code is released under the MIT license
    /// -----------------------------------------------------------
    /// Derived Sub-Task Class for staring programs synchronous or asynchronous
    /// </summary>
    public class StartProgramTask : SubTask
    {


        private bool asynchronous;
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
        /// Default constructor
        /// </summary>
        public StartProgramTask()
            : base()
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
        /// <returns>True if the task was executed successfully, otherwise false</returns>
        public override bool Run()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                bool executed = true;
                foreach (string arg in Arguments)
                {
                    sb.Append(arg);
                    sb.Append(" ");
                }
                string argString = sb.ToString().TrimEnd(' ');
                if (this.Asynchronous == false)
                {
                    System.Diagnostics.Process.Start(this.MainValue, argString);
                    this.Message = "The process " + this.MainValue + " was executed";
                    return true;
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
                        this.Message = "The process " + this.MainValue + " could not be executed";
                        return false;
                    }
                    else
                    {
                        this.Message = "The process " + this.MainValue + " was executed";
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                this.Message = "The process " + this.MainValue + " was executed\n" + e.Message;
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
            StartProgramTask t = new StartProgramTask();
            t.Name = "Start-Program-Task_" + number.ToString();
            t.Description = "This is sub-task " + number.ToString();
            t.MainValue = @"C:\temp\apps\app" + number.ToString() + ".exe";
            t.Arguments.Add("ARG1");
            t.Arguments.Add("ARG2");
            t.Asynchronous = true;
            return t;
        }
    }
}
