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
            /// <summary>
            /// Starts, stops, restarts, pauses or resumes a Windows service
            /// </summary>
            ControlService,
        }

        /// <summary>
        /// Gets a List of all task types as instances of Sub-Tasks
        /// </summary>
        /// <returns>List of types (SubTask)</returns>
        public static List<SubTask> EnumerateTaskTypes()
        {
            List<SubTask> output = new List<SubTask>();
            output.Add(new DeleteFileTask());
            output.Add(new DeleteRegKeyTask());
            output.Add(new WriteLogTask());
            output.Add(new StartProgramTask());
            output.Add(new ControlServiceTask());
            return output;
        }

        /// <summary>
        /// Indicates whether the whole task is executed (enabled) or not
        /// </summary>
        [XmlAttribute("enabled")]
        public bool Enabled { get; set; }

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
        [XmlArrayItem(Type = typeof(ControlServiceTask), ElementName = "controlServiceItem")]
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
        /// The mode how the Task is executed by the program (1 byte)
        /// </summary>
        [XmlIgnore]
        public byte TaskMode { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Task()
        {
            this.Items = new List<SubTask>();
            this.LogEntries = new List<LogEntry>();
            this.TaskMode = 0x0;
            this.Enabled = true;
        }

        /// <summary>
        /// Method to run all SUb-Tasks of the current configuration
        /// </summary>
        /// <param name="stopOnError">If true, the method stops if an error occurs during execution of the Sub-Tasks</param>
        /// <param name="displayOutput">If true, information about the executed Sub-Tasks is passed to the command shell</param>
        /// <param name="log">If true, the execution of the Task and its Sub-Tasks will be logged</param>
        /// <returns>True if no errors occurred, otherwise false</returns>
        public bool Run(bool stopOnError, bool displayOutput, bool log)
        {
            ResolveTaskMode(stopOnError, displayOutput, log);
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
                entry.InsertCodeByte(this.TaskMode, 0);
                entry.InsertCodeByte(subTask.TaskTypeCode, 1);
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
                entry.Status = status;
                entry.InsertCodeByte(subTask.StatusCode, 3);
                if (displayOutput == true)
                {
                    System.Console.WriteLine("Status:\t\t" + status.ToString());
                    System.Console.WriteLine("Execution Code:\t" +  PrintExecutionCode(entry)); //subTask.ExecutionCode.ToString());
                    System.Console.WriteLine("Message:\t" + subTask.Message + "\n");
                }
                if (status == false)
                {
                    this.OccurredErrors++;
                    if (displayOutput == true)
                    {
                        System.Console.WriteLine("==> TASK FINISHED WITH ERRORS!\n");
                    }
                    if (stopOnError == true) { break; }
                }
                this.LogEntries.Add(entry);
            }
            if (displayOutput == true)
            {
                date = DateTime.Now.ToString(DATEFORMAT);
                System.Console.WriteLine("\n\n******************************************\nSUB-TASKS FINISHED AT\t" + date);
                System.Console.WriteLine(this.ExecutedTasks.ToString() + " Sub-Tasks executed");
                System.Console.WriteLine(this.OccurredErrors.ToString() + " Errors occurred\n");
            }
            if (this.OccurredErrors == 0) { return true; }
            else { return false; }
        }

        /// <summary>
        /// Resolves the mode how the Task is executed by the program (2 byte)
        /// </summary>
        /// <param name="stopOnError">If true, the task stops if an error occurs during execution of the Sub-Tasks</param>
        /// <param name="displayOutput">If true, information about the executed Sub-Tasks is passed to the command shell</param>
        /// <param name="log">If true, the execution of the Task and its Sub-Tasks will be logged</param>
        private void ResolveTaskMode(bool stopOnError, bool displayOutput, bool log)
        {
            if      (displayOutput == false && log == false && stopOnError == false) { this.TaskMode = 0x01; }
            else if (displayOutput == false && log == false && stopOnError == true) { this.TaskMode = 0x02; }
            else if (displayOutput == false && log == true && stopOnError == false) { this.TaskMode = 0x03; }
            else if (displayOutput == false && log == true && stopOnError == true) { this.TaskMode = 0x04; }
            else if (displayOutput == true && log == false && stopOnError == false) { this.TaskMode =  0x05; }
            else if (displayOutput == true && log == false && stopOnError == true) { this.TaskMode = 0x06; }
            else if (displayOutput == true && log == true && stopOnError == false) { this.TaskMode =  0x07; }
            else if (displayOutput == true && log == true && stopOnError == true) { this.TaskMode = 0x08; }
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
        /// Method to serialize the current configuration as memory stream 
        /// </summary>
        /// <returns>Memory stream</returns>
        private MemoryStream SerializeAsStream()
        {
            MemoryStream ms = null;
            try
            {
                ms = new MemoryStream();
                StreamWriter sw = new StreamWriter(ms);
                XmlSerializer ser = new XmlSerializer(typeof(Task));
                ser.Serialize(sw, this);
                ms.Flush();
                ms.Position = 0;
            }
            catch (Exception e)
            {
                System.Console.WriteLine("Error while serializing file into the memory stream\n" + e.Message);
            }
            return ms;
        }

        /// <summary>
        /// Creates a demo file as string
        /// </summary>
        /// <param name="type">Task type of the demo configuration</param>
        /// <returns>Sting of the demo file</returns>
        public static string CreateDemoFile(TaskType type)
        {
            return CreateDemoFile(null, type, false);
        }

        /// <summary>
        /// Static method to generate an example configuration
        /// </summary>
        /// <param name="file">File name of the demo configuration</param>
        /// <param name="type">Task type of the demo configuration</param>
        public static void CreateDemoFile(string file, TaskType type)
        {
            CreateDemoFile(file, type, true);
        }


        /// <summary>
        /// Static method to generate an example configuration as file or string
        /// </summary>
        /// <param name="file">File name of the demo configuration</param>
        /// <param name="type">Task type of the demo configuration</param>
        /// <param name="asFile">If true a file will be generated, otherwise a string returned</param>
        private static string CreateDemoFile(string file, TaskType type, bool asFile)
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
            else if (type == TaskType.ControlService)
            {
                tb = new ControlServiceTask();
            }
            t1 = tb.GetDemoFile(1);
            t2 = tb.GetDemoFile(2);
            t3 = tb.GetDemoFile(3);
            t.Items.Add(t1);
            t.Items.Add(t2);
            t.Items.Add(t3);
            if (asFile == true)
            {
                t.Serialize(file);
                return "";
            }
            else
            {
                try
                {
                    MemoryStream ms = t.SerializeAsStream();
                    StreamReader sr = new StreamReader(ms);
                    string output = sr.ReadToEnd();
                    ms.Close();
                    ms.Dispose();
                    return output;
                }
                catch (Exception e)
                {
                    System.Console.WriteLine("Error while loading demo file as string\n" + e.Message);
                    return "";
                }
            }
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

        /// <summary>
        /// Prints the execution code of the task
        /// </summary>
        /// <param name="logEntry">Log entry to print the code from</param>
        /// <returns>Execution code as string</returns>
        public string PrintExecutionCode(LogEntry logEntry)
        {
            return logEntry.PrintExecutionCode();
        }


    }
}
