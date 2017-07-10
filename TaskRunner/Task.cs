using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using Microsoft.Win32;
using TaskRunner.SubTasks;
using System.Text;

namespace TaskRunner
{
    /// <summary>
    /// Task Runner - (c) 2017 - Raphael Stoeckli
    /// This program and its code is released under the MIT license
    /// -----------------------------------------------------------
    /// Class for orchestration and execution of the Sub-Tasks
    /// </summary>
    [XmlRoot("task")]
    public class Task
    {
        /// <summary>
        /// Globally used format string for dates / times
        /// </summary>
        public const string DATEFORMAT = "yyyy-MM-dd H:mm:ss";

        /// <summary>
        /// Type of the Task and its Sub-Tasks
        /// </summary>
        public enum TaskType
        {
            /// <summary>
            /// Task deletes files
            /// </summary>
            DeleteFile,
            /// <summary>
            /// Task delete registry entries
            /// </summary>
            DeleteRegKey,
            /// <summary>
            /// Task writes log entries
            /// </summary>
            WriteLog,
            /// <summary>
            /// Task start programs
            /// </summary>
            StartProgram,
        }
        
        /// <summary>
        /// Type of this Task
        /// </summary>
        [XmlAttribute("type")]
        public TaskType Type { get; set; }

        /// <summary>
        /// List of Sub-Tasks to execute in this Task
        /// </summary>
        /// <remarks>Each class of a Sub-Task needs a distinct XML annotation (XmlArrayItem) for proper serialization / deserialization</remarks>
        [XmlArray("items")]
        [XmlArrayItem(Type = typeof(DeleteFileTask), ElementName = "deleteFileItem")]
        [XmlArrayItem(Type = typeof(DeleteRegKeyTask), ElementName = "deleteRegKeyItem")]
        [XmlArrayItem(Type = typeof(WriteLogTask), ElementName = "writeLogItem")]
        [XmlArrayItem(Type = typeof(StartProgramTask), ElementName = "startProgramItem")]
        public List<SubTask> Items { get; set; }
        /// <summary>
        /// Optional Task name
        /// </summary>
        [XmlAttribute("name")]
        public string TaskName { get; set; }
        /// <summary>
        /// Optional Task description
        /// </summary>
        [XmlElement("description")]
        public string Description { get; set; }
        /// <summary>
        /// If proper deserialized, this value is set to true. It indicates that the configuration is valid (valid XML)
        /// </summary>
        [XmlIgnore]
        public bool Valid { get; set; }
        /// <summary>
        /// The number of executed Sub-Tasks
        /// </summary>
        [XmlIgnore]
        public int ExecutedTasks { get; set; }
        /// <summary>
        /// The number of occurred errors during execution
        /// </summary>
        [XmlIgnore]
        public int OccurredErrors { get; set; }
        /// <summary>
        /// List of log entries. Each executed Sub-Task has one entry
        /// </summary>
        [XmlIgnore]
        public List<LogEntry> LogEntries { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Task()
        {
            this.Items = new List<SubTask>();
            this.LogEntries = new List<LogEntry>();
        }

        /// <summary>
        /// Method to run all SUb-Tasks of the current configuration
        /// </summary>
        /// <param name="stopOnError">If true, the method stops if an error occurs during execution of the Sub-Tasks</param>
        /// <param name="displayOutput">In true, information about the executed Sub-Tasks is passed to the command shell</param>
        /// <returns>True if no errors occurred, otherwise false</returns>
        public bool Run(bool stopOnError, bool displayOutput)
        {
            LogEntry entry;
            this.LogEntries.Clear();
            this.OccurredErrors = 0;
            this.ExecutedTasks = 0;
            string date = DateTime.Now.ToString(DATEFORMAT);
            if (displayOutput == true)
            {
                System.Console.WriteLine("STARTING SUB-TASKS AT\t" + date + "\n");
            }
            bool status;
            foreach(SubTask subTask in this.Items)
            {
                entry = new LogEntry();
                entry.TaskName = this.TaskName;
                entry.SubTaskName = subTask.Name;
                entry.ExecutionDate = DateTime.Now;
                this.ExecutedTasks++;
                if (displayOutput == true)
                {
                    date = DateTime.Now.ToString(DATEFORMAT);
                    System.Console.WriteLine("**********************\n");
                    System.Console.WriteLine("Date:\t\t" + date);
                    System.Console.WriteLine("Sub-Task:\t" + subTask.Name);
                    if (string.IsNullOrEmpty(subTask.Prolog) == false)
                    {
                        System.Console.WriteLine("Prolog:\t\t" + subTask.Prolog);
                    }
                }
                status = subTask.Run();
                if (displayOutput == true)
                {
                    System.Console.WriteLine("Status:\t\t" + status.ToString());
                    System.Console.WriteLine("Message:\t" + subTask.Message + "\n");
                }
                if (status == false)
                {
                    entry.Status = false;
                    this.OccurredErrors++;
                    if (displayOutput == true)
                    {
                        System.Console.WriteLine("==> TASK FINISHED WITH ERRORS!\n");
                    }
                    if (stopOnError == true) { break; }
                }
                else
                {
                    entry.Status = true;
                }
                entry.ExecutionCode = subTask.ExecutionCode;
                this.LogEntries.Add(entry);
            }
            if (displayOutput == true)
            {
                date = DateTime.Now.ToString(DATEFORMAT);
                System.Console.WriteLine("\n\n******************************************\nTasks finished at " + date);
                System.Console.WriteLine(this.ExecutedTasks.ToString() + " Tasks executed");
                System.Console.WriteLine(this.OccurredErrors.ToString() + " Errors occurred\n");
            }
            if (this.OccurredErrors == 0) { return true; }
            else { return false; }
        }

        /// <summary>
        /// Method to deserialize a configuration
        /// </summary>
        /// <param name="filename">File name of the configuration</param>
        /// <returns>The deserialized configuration. In case of an error, an empty object with the parameter Valid=false is returned</returns>
        public static Task Deserialize(string filename)
        {
            try
            {
                FileStream fs = new FileStream(filename, FileMode.Open);
                StreamReader sr = new StreamReader(fs);
                XmlSerializer ser = new XmlSerializer(typeof(Task));
                object o = ser.Deserialize(sr);
                Task t = (Task)o;
                t.Valid = true;
                return t;
            }
            catch(Exception e)
            {
                System.Console.WriteLine("Error while deserializing file: " + filename + "\n" + e.Message);
                return new Task();
            }
        }

        /// <summary>
        /// Method to serialize the current configuration
        /// </summary>
        /// <param name="filename">File name of the configuration</param>
        public void Serialize(string filename)
        {
            try
            {
                FileStream fs = new FileStream(filename, FileMode.Create);
                StreamWriter sw = new StreamWriter(fs);
                XmlSerializer ser = new XmlSerializer(typeof(Task));
                ser.Serialize(sw, this);
            }
            catch (Exception e)
            {
                System.Console.WriteLine("Error while serializing file: " + filename + "\n" + e.Message);
            }
        }

        /// <summary>
        /// Static method to generate an example configuration
        /// </summary>
        /// <param name="file">File name of the demo configuration</param>
        /// <param name="type">Task type of the demo configuration</param>
        public static void CreateDemoFile(string file, TaskType type)
        {
            Task t = new Task();
            t.TaskName = "Demo-Task";
            t.Description = "This is a demo Task with several sub-tasks of the type " + type.ToString();
            t.Type = type;
            SubTask tb = null;
            SubTask t1, t2, t3;
            if (type == TaskType.DeleteFile)
            {
                tb = new DeleteFileTask();
            }
            else if (type == TaskType.DeleteRegKey)
            {
                tb = new DeleteRegKeyTask();
            }
            else if (type == TaskType.StartProgram)
            {
                tb = new StartProgramTask();
            }
            else if (type == TaskType.WriteLog)
            {
                tb = new WriteLogTask();
            }
            t1 = tb.GetDemoFile(1);
            t2 = tb.GetDemoFile(2);
            t3 = tb.GetDemoFile(3);
            t.Items.Add(t1);
            t.Items.Add(t2);
            t.Items.Add(t3);
            t.Serialize(file);
        }


        /// <summary>
        /// Method to write all log entries of the executed Tasks
        /// </summary>
        /// <param name="logFile">File name of the logfile</param>
        public void Log(string logFile)
        {
            string headerValue = "Date\tStatus\tStatus-Code\tTask\tSub-Task\r\n---------------------------------------------------------------------------------------------";
            StringBuilder sb = new StringBuilder();
            foreach (LogEntry entry in this.LogEntries)
            {
                sb.Append(entry.getLogString());
                sb.Append("\r\n");
            }
            char[] trim = new char[]{'\r', '\n'};
            Utils.Log(logFile, headerValue, sb.ToString().TrimEnd(trim));
        }


    }
}
