using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using Microsoft.Win32;
using TaskRunner.SubTasks;
using System.Text;
using System.Security.Cryptography;

namespace TaskRunner
{
    /// <summary>
    /// Task Runner - (c) 2018 - Raphael Stoeckli
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
            /// Dummy Task (Pre-Sub-Task condition, will not be listed)
            /// </summary>
            DummyTask,
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
            /// <summary>
            /// Kills a Windows process
            /// </summary>
            KillProcess,
            /// <summary>
            /// Loads further config files to execute
            /// </summary>
            MetaTask,
            /// <summary>
            /// Delays the program execution. This tasks is used to play a delay between two other tasks 
            /// </summary>
            DelayTask,
            /// <summary>
            /// Task with several different Sub-Tasks
            /// </summary>
            MixedTask,
        }

        /// <summary>
        /// Status of the task or sub task
        /// </summary>
        public enum Status
        {
            /// <summary>
            /// Task or sub task was successful
            /// </summary>
            success,
            /// <summary>
            /// Task or sub task was not successful
            /// </summary>
            failed,
            /// <summary>
            /// Task or sub task was skipped
            /// </summary>
            skipped,
            /// <summary>
            /// Program will be terminated
            /// </summary>
            terminate,
            /// <summary>
            /// Error - Status unclear
            /// </summary>
            none,
        }

        /// <summary>
        /// Gets a List of all task types as instances of Sub-Tasks
        /// </summary>
        /// <returns>List of types (SubTask)</returns>
        public static List<SubTask> EnumerateSubTasks()
        {
            List<SubTask> output = new List<SubTask>();
            output.Add(new DeleteFileTask());
            output.Add(new DeleteRegKeyTask());
            output.Add(new WriteLogTask());
            output.Add(new StartProgramTask());
            output.Add(new ControlServiceTask());
            output.Add(new KillProcessTask());
            output.Add(new MetaTask());
            output.Add(new DelayTask());
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
        [XmlArrayItem(Type = typeof(KillProcessTask), ElementName = "killProcessItem")]
        [XmlArrayItem(Type = typeof(MetaTask), ElementName = "metaItem")]
        [XmlArrayItem(Type = typeof(DelayTask), ElementName = "delayItem")]
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
        /// Optional condition check for the current SubTask
        /// </summary>
        [XmlElement("condition")]
        public Condition TaskCondition { get; set; }
        /// <summary>
        /// If proper deserialized, this value is set to true. It indicates that the configuration is valid (valid XML)
        /// </summary>
        [XmlIgnore]
        public bool Valid { get; set; }
        /// <summary>
        /// The number of executed Sub-Tasks
        /// </summary>
        [XmlIgnore]
        public int ExecutedSubTasks { get; set; }
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
        /// If true, the method stops if an error occurs during execution of the Sub-Tasks
        /// </summary>
        [XmlIgnore]
        public bool StopOnError { get; set; }

        /// <summary>
        /// If true, information about the executed Sub-Tasks is passed to the command shell
        /// </summary>
        [XmlIgnore]
        public bool DisplayOutput { get; set; }

        /// <summary>
        /// If true, the execution of the Task and its Sub-Tasks will be logged
        /// </summary>
        [XmlIgnore]
        public bool WriteLog { get; set; }

        /// <summary>
        /// Path to the system logfile
        /// </summary>
        [XmlIgnore]
        public string LogfilePath { get; set; }

        /// <summary>
        /// Internal ID of the task. The ID is calculated by the file name
        /// </summary>
        [XmlIgnore]
        public string TaskID { get; set; }

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

        private bool CheckCondition(Condition condition, ref SubTask subtask, out  Condition.ConditionAction action, out Condition.ConditionAction defaultAction)
        {
            action = Condition.ConditionAction.none; // Default = error
            defaultAction = Condition.ConditionAction.none; // Default = error
            Condition.ConditionType type;
            if (string.IsNullOrEmpty(condition.Expression))
            {
                subtask.SetStatus("CONDITION_INVALID_ARGS", "No condition to evaluate was defined");
                return false;
            }
            if (string.IsNullOrEmpty(condition.Action))
            {
                subtask.SetStatus("CONDITION_INVALID_ACTIONS", "No action to execute was defined");
                return false;
            }
            if (string.IsNullOrEmpty(condition.Default))
            {
                subtask.SetStatus("CONDITION_INVALID_ACTIONS", "No default action to execute was defined");
                return false;
            }
            if (string.IsNullOrEmpty(condition.Type))
            {
                subtask.SetStatus("CONDITION_INVALID_TYPE", "No evaluation type was defined");
                return false;
            }
            action = condition.CheckOperation(true);
            if (action == Condition.ConditionAction.none)
            {
                subtask.SetStatus("CONDITION_INVALID_ACTIONS", "An invalid action to execute was defined");
                return false;
            }
            defaultAction = condition.CheckOperation(false);
            if (defaultAction == Condition.ConditionAction.none)
            {
                subtask.SetStatus("CONDITION_INVALID_ACTIONS", "An invalid default action to execute was defined");
                return false;
            }
            type = condition.CheckType();
            if (type == Condition.ConditionType.none)
            {
                subtask.SetStatus("CONDITION_INVALID_TYPE", "an invalid evaluation type was defined");
                return false;
            }
            return true;
        }

        private bool EvaluateCondition(Condition condition, ref SubTask subtask, Condition.ConditionAction originalAction, Condition.ConditionAction defaultAction, out Condition.ConditionAction action, bool displayOutput)
        {
            bool status = condition.Evaluate(this.DisplayOutput);
            if (status == true)
            {
                action = originalAction;
                Parameter.UpdateSystemParameter(Parameter.SysParam.SYSTEM_CONDITION_TRUE, condition.Action.ToString());
                Parameter.UpdateSystemParameter(Parameter.SysParam.SYSTEM_CONDITION_FALSE, condition.Default.ToString()); 
                if (action == Condition.ConditionAction.skip)
                {
                    subtask.SetStatus("N/A", "The task was skipped");
                }
                else if (action == Condition.ConditionAction.exit || action == Condition.ConditionAction.restart_last_subtask || action == Condition.ConditionAction.restart_task)
                {
                    subtask.SetStatus("N/A", "The task was skipped - A further action follows...");
                }
                return true;
            }
            else
            {
                Parameter.UpdateSystemParameter(Parameter.SysParam.SYSTEM_CONDITION_TRUE, defaultAction.ToString());
                Parameter.UpdateSystemParameter(Parameter.SysParam.SYSTEM_CONDITION_FALSE, originalAction.ToString());
                action = defaultAction;
                return false;
            }
        }

        private void Verbose(string text)
        {
            Verbose(text, false);
        }

        private void Verbose(string text, bool skipNewline)
        {
            if (this.DisplayOutput == true)
            {
                if (skipNewline == true)
                {
                    System.Console.WriteLine(text);
                }
                else
                {
                    System.Console.WriteLine(text + "\n");
                }
            }
        }

        private Condition.ConditionAction HandleCondition(ref SubTask currentTask, bool preCondition, bool task)
        {
            bool conditionStatus = false;
            Condition.ConditionAction action = Condition.ConditionAction.none;
            Condition.ConditionAction execAction;
            Condition.ConditionAction defaultAction = Condition.ConditionAction.none;
            Condition condition;
            string type;
            if (task == false)
            { 
                type = " SUB-TASK ";
                condition = currentTask.SubTaskCondition;
            }
            else
            {
                type = " TASK ";
                condition = this.TaskCondition;
            }
            if (condition != null) // Check condition
            {
                if (currentTask == null)
                {
                    currentTask = new DummyTask();
                }
                currentTask.GetDocumentationStatusCodes(); // Necessary to load the status codes
                if (this.CheckCondition(condition, ref currentTask, out execAction, out defaultAction) == false)
                {
                    SetLogEntry(currentTask);
                    return Condition.ConditionAction.exit; // Terminate because of errors
                }
                if (condition.Type.ToLower() == "pre" && preCondition == false)
                {
                    return Condition.ConditionAction.run; // No action = run
                }
                else if (condition.Type.ToLower() == "post" && preCondition == true)
                {
                    return Condition.ConditionAction.run; // No action = run
                }
                conditionStatus = this.EvaluateCondition(condition, ref currentTask,  execAction, defaultAction, out action, this.DisplayOutput);
                if (conditionStatus == true)
                {
                    if (action == Condition.ConditionAction.exit)
                    {
                        Verbose("==> THE PROGRAM WILL BE TREMINATED DUE TO A" + type + condition.Type.ToUpper() + "-CONDITION");
                        currentTask.SetStatus("N/A", "The program will be terminated");
                        SetLogEntry(currentTask);
                        return Condition.ConditionAction.exit;
                    }
                    else if (action == Condition.ConditionAction.skip)
                    {
                        Verbose("==> THE" + type + "WILL BE SKIPPED DUE TO A" + type + condition.Type.ToUpper() + "-CONDITION");
                        currentTask.SetStatus("N/A", "The" + type.ToLower() + "will be skipped");
                        SetLogEntry(currentTask);
                        return Condition.ConditionAction.skip;
                    }
                    else if (action == Condition.ConditionAction.restart_task && (condition.Type.ToLower() == "post" || task == false)) // In task only allowed as post-condition, in sub-task always
                    {
                        Verbose("==> THE TASK WILL BE RESTARTED DUE TO " + type + condition.Type.ToUpper() + "-CONDITION");
                        currentTask.SetStatus("N/A", "The task will be restarted");
                        SetLogEntry(currentTask);
                        return Condition.ConditionAction.restart_task;
                    }
                    else if (action == Condition.ConditionAction.restart_last_subtask && task == false) // TO CHECK: Sub-task can always be restarted (also the first occurrence in a task?)
                    {
                        Verbose("==> THE SUB-TASK WILL BE RESTARTED DUE TO " + type + condition.Type.ToUpper() + "-CONDITION");
                        currentTask.SetStatus("N/A", "The task will be restarted");
                        SetLogEntry(currentTask);
                        return Condition.ConditionAction.restart_last_subtask;
                    }
                    else if (action == Condition.ConditionAction.run) { } // Default
                    else
                    {
                        Verbose("==> THE CONDITIONAL ACTION '" + action.ToString() + "' IS NOT ALLOWED AS " + condition.Type.ToUpper() + "-CONDITION OF A" + type.TrimEnd(' ') + ". THE PROGRAM WILL BE TERMINATED");
                        currentTask.SetStatus("ERROR", "Condition action not allowed");
                        SetLogEntry(currentTask);
                        return Condition.ConditionAction.exit;
                    }

                }
                else
                {
                    if (action == Condition.ConditionAction.exit)
                    {
                        Verbose("==> THE PROGRAM WILL BE TREMINATED BECAUSE A" + type + condition.Type.ToUpper() + "-CONDITION WAS NOT MET");
                        currentTask.SetStatus("N/A", "The program will be terminated");
                        SetLogEntry(currentTask);
                        return Condition.ConditionAction.exit;
                    }
                    else if (action == Condition.ConditionAction.skip)
                    {
                        Verbose("==> THE TASK WILL BE SKIPPED BECAUSE A" + type + condition.Type.ToUpper() + "-CONDITION WAS NOT MET");
                        currentTask.SetStatus("N/A", "The task will be skipped");
                        SetLogEntry(currentTask);
                        return Condition.ConditionAction.skip;
                    }
                    else if (action == Condition.ConditionAction.restart_task && (condition.Type.ToLower() == "post" || task == false)) // In task only allowed as post-condition, in sub-task always
                    {
                        Verbose("==> THE TASK WILL BE RESTARTED BECAUSE A" + type + condition.Type.ToUpper() + "-CONDITION WAS NOT MET");
                        currentTask.SetStatus("N/A", "The task will be restarted");
                        SetLogEntry(currentTask);
                        return Condition.ConditionAction.restart_task;
                    }
                    else if (action == Condition.ConditionAction.restart_last_subtask && task == false) // TO CHECK: Sub-task can always be restarted (also the first occurrence in a task?)
                    {
                        Verbose("==> THE SUB-TASK WILL BE RESTARTED BECAUSE A" + type + condition.Type.ToUpper() + "-CONDITION WAS NOT MET");
                        currentTask.SetStatus("N/A", "The sub-task will be restarted");
                        SetLogEntry(currentTask);
                        return Condition.ConditionAction.restart_last_subtask;
                    }
                    else if (action == Condition.ConditionAction.run) { } // Default
                    else
                    {
                        Verbose("==> THE CONDITIONAL ACTION '" + action.ToString() + "' IS NOT ALLOWED AS " + condition.Type.ToUpper() + "-CONDITION OF A" + type.TrimEnd(' ') + ". THE PROGRAM WILL BE TERMINATED");
                        currentTask.SetStatus("ERROR", "Condition action not allowed");
                        SetLogEntry(currentTask);                        
                        return Condition.ConditionAction.exit;
                    }
                }
            }
            return Condition.ConditionAction.run; // No action = run
        }

        private LogEntry SetLogEntry(SubTask currentTask)
        {
            LogEntry entry = new LogEntry();
            entry.TaskName = this.TaskName;
            entry.SubTaskName = currentTask.Name;
            entry.ExecutionDate = DateTime.Now;
            entry.InsertCodeByte(this.TaskMode, 0);
            entry.InsertCodeByte(currentTask.TaskTypeCode, 1);
            entry.InsertCodeByte(currentTask.StatusCode, 2);
            entry.InsertCodeByte(currentTask.StatusCode, 3);
            this.LogEntries.Add(entry);
            return entry;
        }

        /// <summary>
        /// Method to run all Sub-Tasks of the current configuration
        /// </summary>
        /// <param name="stopOnError">If true, the method stops if an error occurs during execution of the Sub-Tasks</param>
        /// <param name="displayOutput">If true, information about the executed Sub-Tasks is passed to the command shell</param>
        /// <param name="log">If true, the execution of the Task and its Sub-Tasks will be logged</param>
        /// <param name="logFilePath">Path to the system logfile</param>
        /// <returns>Task status</returns>
        public Status Run (bool stopOnError, bool displayOutput, bool log, string logFilePath)
        {
            
            SubTask currentTask = null;
            LogEntry entry;
            Status status;
            Condition.ConditionAction action;
            string date;
            bool restartTask = false;
            
            while (true)
            {

            this.StopOnError = stopOnError;
            this.DisplayOutput = displayOutput;
            this.WriteLog = log;
            this.LogfilePath = logFilePath;
            ResolveTaskMode(stopOnError, displayOutput, log);
            this.LogEntries.Clear();
            this.OccurredErrors = 0;
            this.ExecutedSubTasks = 0;
            date = DateTime.Now.ToString(DATEFORMAT);

            Parameter.RegisterTaskIterations(this);                                                 // Registers the task (if not done before) to avoid infinite loops. If already registered, it will be incremented by 1
            Parameter.UpdateSystemParameterNumber(Parameter.SysParam.TASK_ALL_NUMBER_TOTAL);        // Add 1 to the total number of executed tasks
            Parameter.UpdateSystemParameter(Parameter.SysParam.TASK_LAST_TIME_START, DateTime.Now); // Set start time of the last (this) executed task
            if (Parameter.CheckTaskIteration(this.TaskID, displayOutput) == false)                  // Check termination condition (max. iterations of task reached)
            {
                Verbose("==> THE PROGRAM WILL BE TERMINATED DUET TO THE ITERATION LIMIT (" + Parameter.GetSystemParameter(Parameter.SysParam.ENV_MAX_TASK_ITERATIONS).NumericValue + ") WAS REACHED");
                return Status.terminate;
            }
            action = HandleCondition(ref currentTask, true, true);                                  // Pre-Check
            if (action == Condition.ConditionAction.exit) { return Status.terminate; }
            else if (action == Condition.ConditionAction.skip) { return Status.skipped; }

            Verbose("Task: " + this.TaskName + " (ID: " + this.TaskID + ")\nSTARTING SUB-TASKS AT\t" + date);
            for (int i = 0; i < this.Items.Count; i++) // Loop of sub-tasks
            {
                restartTask = false;
                currentTask = this.Items[i];
                currentTask.GetDocumentationStatusCodes(); // Necessary to load the status codes
                if (Parameter.CheckSubTaskIteration(currentTask.SubTaskID, displayOutput) == false) // Check termination condition (max. iterations of task reached)
                {
                    Verbose("==> THE TASK WILL BE TERMINATED DUE TO THE ITERATION LIMIT (" + Parameter.GetSystemParameter(Parameter.SysParam.ENV_MAX_SUBTASK_ITERATIONS).NumericValue + ") WAS REACHED");
                    return Status.skipped;
                }
                if (currentTask.Enabled == false)
                {
                    Verbose("==> SUB-TASK '" + currentTask.Name + "' SKIPPED (DISABLED)");
                    continue;
                }
                date = DateTime.Now.ToString(DATEFORMAT);

                action = HandleCondition(ref currentTask, true, false);                             // Sub-Task pre-check
                if (action == Condition.ConditionAction.exit) { return Status.terminate; }
                else if (action == Condition.ConditionAction.restart_last_subtask) { i--; continue; }   // Could be a problem (to be checked)
                else if (action == Condition.ConditionAction.restart_task) { restartTask = true; break; }
                else if (action == Condition.ConditionAction.skip) { continue; }
                this.ExecutedSubTasks++;

                Verbose("**********************\nDate:\t\t" + date + "\nSub-Task:\t" + currentTask.Name + " (ID: " + currentTask.SubTaskID + ")", true);
                if (string.IsNullOrEmpty(currentTask.Prolog) == false)
                {
                    Verbose("Prolog:\t\t" + currentTask.Prolog, true);
                }

                currentTask.MainValue = currentTask.GetMainValue(displayOutput); // Resolves the main value by the config file or param name
                Parameter.UpdateSystemParameter(Parameter.SysParam.SUBTASK_LAST_TIME_START, DateTime.Now);
                Parameter.UpdateSystemParameterNumber(Parameter.SysParam.SUBTASK_ALL_NUMBER_TOTAL);
                status = currentTask.Run();     // --> RUN
                Parameter.UpdateSystemParameter(Parameter.SysParam.SUBTASK_LAST_TIME_END, DateTime.Now);
                entry = SetLogEntry(currentTask);
                Verbose("Status:\t\t" + status.ToString() + "\nExecution Code:\t" + PrintExecutionCode(entry) + "\nMessage:\t" + currentTask.Message, true);

                if (status == Status.failed)
                {
                    Parameter.UpdateSystemParameter(Parameter.SysParam.SUBTASK_LAST_SUCCESS, false);
                    Parameter.UpdateSystemParameterNumber(Parameter.SysParam.SUBTASK_ALL_NUMBER_FAIL);
                    this.OccurredErrors++;
                    Verbose("==> TASK FINISHED WITH ERRORS!");
                    if (stopOnError == true) { return Status.terminate; }
                }
                else if (status == Status.success)
                {
                    Parameter.UpdateSystemParameterNumber(Parameter.SysParam.SUBTASK_ALL_NUMBER_SUCCESS);
                    Parameter.UpdateSystemParameter(Parameter.SysParam.SUBTASK_LAST_SUCCESS, true);
                    Parameter.UpdateSystemParameter(Parameter.SysParam.SUBTASK_LAST_SUCCESS_PARTIAL, true); // Reset
                    Parameter.UpdateSystemParameter(Parameter.SysParam.SUBTASK_ALL_SUCCESS_PARTIAL, true); // No reset
                }
                else // Skipped
                {
                    Parameter.UpdateSystemParameterNumber(Parameter.SysParam.SUBTASK_ALL_NUMBER_SUCCESS);
                    Parameter.UpdateSystemParameter(Parameter.SysParam.SUBTASK_LAST_SUCCESS, true);
                    Parameter.UpdateSystemParameter(Parameter.SysParam.SUBTASK_LAST_SUCCESS_PARTIAL, true); // Reset
                    Parameter.UpdateSystemParameter(Parameter.SysParam.SUBTASK_ALL_SUCCESS_PARTIAL, true); // No reset
                    Verbose("==> TASK SKIPPED");
                }

                action = HandleCondition(ref currentTask, false, false);                            // Sub-Task post-check
                if (action == Condition.ConditionAction.exit) { return Status.terminate; }
                else if (action == Condition.ConditionAction.restart_last_subtask) { i--; continue; }
                else if (action == Condition.ConditionAction.restart_task) { restartTask = true; break; }
                else if (action == Condition.ConditionAction.skip) { continue; }                    // Not useful but not forbidden

            }
            if (restartTask == true) { continue; }                                                  // reset

            action = HandleCondition(ref currentTask, false, true);                                 // Post-Check
            if (action == Condition.ConditionAction.exit) { return Status.terminate; }
            else if (action == Condition.ConditionAction.restart_task) { continue; }                // reset
            else if (action == Condition.ConditionAction.skip) { return Status.skipped; }           // Not useful but not forbidden

                break; // Leave loop (default)
            }
            date = DateTime.Now.ToString(DATEFORMAT);
            Verbose("\n\n******************************************\nSUB-TASKS FINISHED AT\t" + date + "\n" + this.ExecutedSubTasks.ToString() + " Sub-Tasks executed\n" + this.OccurredErrors.ToString() + " Errors occurred");
            Parameter.UpdateSystemParameter(Parameter.SysParam.TASK_LAST_TIME_END, DateTime.Now);

            if (this.OccurredErrors == 0)
            {
                Parameter.UpdateSystemParameterNumber(Parameter.SysParam.TASK_ALL_NUMBER_SUCCESS);
                Parameter.UpdateSystemParameter(Parameter.SysParam.TASK_LAST_SUCCESS, true);
                Parameter.UpdateSystemParameter(Parameter.SysParam.TASK_LAST_SUCCESS_PARTIAL, true);
                Parameter.UpdateSystemParameter(Parameter.SysParam.SUBTASK_ALL_SUCCESS, true);
                status = Status.success;
            }
            else
            {
                Parameter.UpdateSystemParameterNumber(Parameter.SysParam.TASK_ALL_NUMBER_FAIL);
                Parameter.UpdateSystemParameter(Parameter.SysParam.TASK_ALL_SUCCESS, false);
                Parameter.UpdateSystemParameter(Parameter.SysParam.TASK_LAST_SUCCESS, false);
                status = Status.failed;
            }
            if (this.WriteLog == true)
            {
                if (Parameter.GetSystemParameter(Parameter.SysParam.TASK_LAST_LOGGING_SUPPERSS).BooleanValue == false)
                {
                    this.Log(this.LogfilePath, status);
                }
            }
            Parameter.ResetTaskParameters();
            Parameter.UpdateSystemParameter(Parameter.SysParam.SYSTEM_TIME_END, DateTime.Now);
            return status; 
        }

        /*
        /// <summary>
        /// Method to run all Sub-Tasks of the current configuration
        /// </summary>
        /// <param name="stopOnError">If true, the method stops if an error occurs during execution of the Sub-Tasks</param>
        /// <param name="displayOutput">If true, information about the executed Sub-Tasks is passed to the command shell</param>
        /// <param name="log">If true, the execution of the Task and its Sub-Tasks will be logged</param>
        /// <param name="logFilePath">Path to the system logfile</param>
        /// <returns>Task status</returns>
        public Status Run(bool stopOnError, bool displayOutput, bool log, string logFilePath)
        {
            Parameter.RegisterTaskIterations(this);
            Parameter.UpdateSystemParameterNumber(Parameter.SysParam.TASK_ALL_NUMBER_TOTAL);
            Parameter.UpdateSystemParameter(Parameter.SysParam.TASK_LAST_TIME_START, DateTime.Now);
            if (Parameter.CheckTaskIteration(this.TaskID, displayOutput) == false)
            {
                if (displayOutput == true)
                {
                    System.Console.WriteLine("PROGRAM WILL BE TERMINATED BECAUSE ITERATION LIMIT (" + Parameter.GetSystemParameter(Parameter.SysParam.ENV_MAX_TASK_ITERATIONS).NumericValue + ") WAS REACHED");
                }
                return Status.terminate;
            }
            this.StopOnError = stopOnError;
            this.DisplayOutput = displayOutput;
            this.WriteLog = log;
            this.LogfilePath = logFilePath;
            ResolveTaskMode(stopOnError, displayOutput, log);
            LogEntry entry;
            this.LogEntries.Clear();
            this.OccurredErrors = 0;
            this.ExecutedSubTasks = 0;
            string date = DateTime.Now.ToString(DATEFORMAT);
            if (displayOutput == true)
            {
                System.Console.WriteLine("Task: " + this.TaskName + " (ID: " + this.TaskID + ")");
                System.Console.WriteLine("STARTING SUB-TASKS AT\t" + date + "\n");
            }
            Status status;
            SubTask subTask;
            //foreach(SubTask subTask in this.Items)
            for (int i = 0; i < this.Items.Count; i++ )
            {
                subTask = this.Items[i];
                subTask.GetDocumentationStatusCodes(); // Necessary to load the status codes
                if (subTask.Enabled == false)
                {
                    if (displayOutput == true)
                    {
                        System.Console.WriteLine("==> SUB-TASK '" + subTask.Name + "' SKIPPED\n");
                    }
                    continue;
                }
                entry = new LogEntry();
                entry.TaskName = this.TaskName;
                entry.SubTaskName = subTask.Name;
                entry.ExecutionDate = DateTime.Now;
                entry.InsertCodeByte(this.TaskMode, 0);
                entry.InsertCodeByte(subTask.TaskTypeCode, 1);
                this.ExecutedSubTasks++;
                if (displayOutput == true)
                {
                    date = DateTime.Now.ToString(DATEFORMAT);
                    System.Console.WriteLine("**********************\n");
                    System.Console.WriteLine("Date:\t\t" + date);
                    System.Console.WriteLine("Sub-Task:\t" + subTask.Name + " (ID: " + subTask.SubTaskID + ")");
                    if (string.IsNullOrEmpty(subTask.Prolog) == false)
                    {
                        System.Console.WriteLine("Prolog:\t\t" + subTask.Prolog);
                    }
                }
                subTask.MainValue = subTask.GetMainValue(displayOutput); // Resolves the main value by the config file or param name
                Parameter.UpdateSystemParameter(Parameter.SysParam.SUBTASK_LAST_TIME_START, DateTime.Now);
                Parameter.UpdateSystemParameterNumber(Parameter.SysParam.SUBTASK_ALL_NUMBER_TOTAL);
                if (subTask.SubTaskCondition != null)
                {
                    bool stat = subTask.SubTaskCondition.Evaluate(this.DisplayOutput);
                    if (stat == true)
                    {
                        if (stat == true)
                        {
                            Parameter.UpdateSystemParameter(Parameter.SysParam.SYSTEM_CONDITION_TRUE, action.ToString());
                            Parameter.UpdateSystemParameter(Parameter.SysParam.SYSTEM_CONDITION_FALSE, defaultAction.ToString());
                        }
                        else
                        {
                            Parameter.UpdateSystemParameter(Parameter.SysParam.SYSTEM_CONDITION_TRUE, defaultAction.ToString());
                            Parameter.UpdateSystemParameter(Parameter.SysParam.SYSTEM_CONDITION_FALSE, action.ToString());
                        }
                        if (status == true && action == ConditionalAction.skip)
                        {
                            this.Message = "The task was skipped";
                            this.StatusCode = 0x00;
                            return Task.Status.skipped;
                        }
                        else if (status == true && (action == ConditionalAction.exit || action == ConditionalAction.restart_last_subtask || action == ConditionalAction.restart_task))
                        {
                            this.Message = "The task was skipped - A further action follows...";
                            this.StatusCode = 0x00;
                            return Task.Status.skipped;
                        }

                    }
                }
                else
                {
                    status = subTask.Run();
                }
                Parameter.UpdateSystemParameter(Parameter.SysParam.SUBTASK_LAST_TIME_END, DateTime.Now);
                entry.Status = status;
                entry.InsertCodeByte(subTask.StatusCode, 3);
                if (displayOutput == true)
                {
                    System.Console.WriteLine("Status:\t\t" + status.ToString());
                    System.Console.WriteLine("Execution Code:\t" + PrintExecutionCode(entry)); //subTask.ExecutionCode.ToString());
                    System.Console.WriteLine("Message:\t" + subTask.Message + "\n");
                }
                if (status == Status.failed)
                {
                    Parameter.UpdateSystemParameter(Parameter.SysParam.SUBTASK_LAST_SUCCESS, false);
                    Parameter.UpdateSystemParameterNumber(Parameter.SysParam.SUBTASK_ALL_NUMBER_FAIL);
                    //Parameter.UpdateSystemParameter(Parameter.SysParam., DateTime.Now);
                    this.OccurredErrors++;
                    if (displayOutput == true)
                    {
                        System.Console.WriteLine("==> TASK FINISHED WITH ERRORS!\n");
                    }
                    if (stopOnError == true) { break; }
                }
                else if (status == Status.success)
                {
                    Parameter.UpdateSystemParameterNumber(Parameter.SysParam.SUBTASK_ALL_NUMBER_SUCCESS);
                    Parameter.UpdateSystemParameter(Parameter.SysParam.SUBTASK_LAST_SUCCESS, true);
                    Parameter.UpdateSystemParameter(Parameter.SysParam.SUBTASK_LAST_SUCCESS_PARTIAL, true); // Reset
                    Parameter.UpdateSystemParameter(Parameter.SysParam.SUBTASK_ALL_SUCCESS_PARTIAL, true); // No reset
                }
                else // Skipped
                {
                    Parameter.UpdateSystemParameterNumber(Parameter.SysParam.SUBTASK_ALL_NUMBER_SUCCESS);
                    Parameter.UpdateSystemParameter(Parameter.SysParam.SUBTASK_LAST_SUCCESS, true);
                    Parameter.UpdateSystemParameter(Parameter.SysParam.SUBTASK_LAST_SUCCESS_PARTIAL, true); // Reset
                    Parameter.UpdateSystemParameter(Parameter.SysParam.SUBTASK_ALL_SUCCESS_PARTIAL, true); // No reset
                    if (displayOutput == true)
                    {
                        System.Console.WriteLine("==> TASK SKIPPED\n");
                    }
                }
                this.LogEntries.Add(entry);
                if (status == Status.skipped && Parameter.GetSystemParameter(Parameter.SysParam.SYSTEM_CONDITION_TRUE).Value == "exit")
                {
                    if (displayOutput == true)
                    {
                        System.Console.WriteLine("==> PROGRAM WILL BE TERMINATED\n");
                    }
                    return Status.terminate;
                }
                Parameter.ResetSubTaskParameters();
                if (status == Status.skipped && Parameter.GetSystemParameter(Parameter.SysParam.SYSTEM_CONDITION_TRUE).Value == "restart_last_subtask")
                {
                    if (displayOutput == true)
                    { System.Console.WriteLine("==> SUB-TASK WILL BE RESTARTED\n"); }
                    i--;
                    continue;
                }
                if (status == Status.skipped && Parameter.GetSystemParameter(Parameter.SysParam.SYSTEM_CONDITION_TRUE).Value == "restart_task")
                {
                    if (displayOutput == true)
                    { System.Console.WriteLine("==> TASK WILL BE RESTARTED\n"); }
                    i = -1;
                    continue;
                }

            }
            if (displayOutput == true)
            {
                date = DateTime.Now.ToString(DATEFORMAT);
                System.Console.WriteLine("\n\n******************************************\nSUB-TASKS FINISHED AT\t" + date);
                System.Console.WriteLine(this.ExecutedSubTasks.ToString() + " Sub-Tasks executed");
                System.Console.WriteLine(this.OccurredErrors.ToString() + " Errors occurred\n");
            }
            Parameter.UpdateSystemParameter(Parameter.SysParam.TASK_LAST_TIME_END, DateTime.Now);

            if (this.WriteLog == true)
            {
                if (Parameter.GetSystemParameter(Parameter.SysParam.TASK_LAST_LOGGING_SUPPERSS).BooleanValue == true)
                {
                    this.Log(this.LogfilePath);
                }
            }
            if (this.OccurredErrors == 0)
            {
                Parameter.UpdateSystemParameterNumber(Parameter.SysParam.TASK_ALL_NUMBER_SUCCESS);
                Parameter.UpdateSystemParameter(Parameter.SysParam.TASK_LAST_SUCCESS, true);
                Parameter.UpdateSystemParameter(Parameter.SysParam.TASK_LAST_SUCCESS_PARTIAL, true);
                Parameter.UpdateSystemParameter(Parameter.SysParam.SUBTASK_ALL_SUCCESS, true);
                status =  Status.success;
            }
            else
            {
                Parameter.UpdateSystemParameterNumber(Parameter.SysParam.TASK_ALL_NUMBER_FAIL);
                Parameter.UpdateSystemParameter(Parameter.SysParam.TASK_ALL_SUCCESS, false);
                Parameter.UpdateSystemParameter(Parameter.SysParam.TASK_LAST_SUCCESS, false);
                status = Status.failed;
            }
            Parameter.ResetTaskParameters();
            Parameter.UpdateSystemParameter(Parameter.SysParam.SYSTEM_TIME_END, DateTime.Now);
            return status;
        }
          */

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
        /// Assigns the task object to tits subtasks
        /// </summary>
        private void AssignTaskToSubtask()
        {
            for(int i = 0; i < this.Items.Count; i++)
            {
                this.Items[i].ParentTask = this;
            }
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
                MD5 md5 = MD5.Create();
                byte[] bytes = md5.ComputeHash(fs);
                fs.Seek(0, SeekOrigin.Begin);
                StreamReader sr = new StreamReader(fs);
                XmlSerializer ser = new XmlSerializer(typeof(Task));
                object o = ser.Deserialize(sr);
                Task t = (Task)o;
                t.AssignTaskToSubtask();
                t.Valid = true;
                t.TaskID = Utils.ConvertBytesToString(bytes);
                for (int i = 0; i < t.Items.Count; i++)
                {
                    t.Items[i].SubTaskID = t.TaskID + "-" + (i + 1).ToString();
                }
                return t;
            }
            catch(Exception e)
            {
                System.Console.WriteLine("Error while deserializing file: " + filename + "\n" + e.Message);
                return new Task();
            }
        }

        /// <summary>
        /// Removes unused values to omit them in the serialized XML code
        /// </summary>
        private void RemoveUnusedValues()
        {
            for (int i = 0; i < this.Items.Count; i++)
            {
                if (this.Items[i].Arguments.Count == 0)
                {
                    this.Items[i].Arguments = null;
                }
            }
        }

        /// <summary>
        /// Method to serialize the current configuration
        /// </summary>
        /// <param name="filename">File name of the configuration</param>
        public void Serialize(string filename)
        {
            RemoveUnusedValues();
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
            RemoveUnusedValues();
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
            SubTask t1, t2, t3, t4;
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
            else if (type == TaskType.KillProcess)
            {
                tb = new KillProcessTask();
            }
            else if (type == TaskType.MetaTask)
            {
                tb = new MetaTask();
            }
            else if (type == TaskType.DelayTask)
            {
                tb = new DelayTask();
            }
            t1 = tb.GetDemoFile(1);
            t2 = tb.GetDemoFile(2);
            t3 = tb.GetDemoFile(3);
            t4 = tb.GetDemoFile(4);
            t3.UseParameter = true;
            t3.MainValue = "PARAM_NAME_1";
            t3.Description = t3.Description + ". This Sub-Task uses a value of a global Parameter (passed as flag -p|--param) with the parameter name PARAM_NAME_1";
            t4.SubTaskCondition = new Condition("2 > 1", "run", "exit", "pre");
            t4.Description = t4.Description + ". This Sub-Task has a pre-condition with the expression '2 > 1' (which is true) with the action 'run' if the expression is evaluated as true, and 'exit' if the expression is evaluated as false";
            t.Items.Add(t1);
            t.Items.Add(t2);
            t.Items.Add(t3);
            t.Items.Add(t4);
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
        /// <param name="status">Status of the task</param>
        /// <param name="logFile">File name of the logfile</param>
        public void Log(string logFile, Status status)
        {
            string headerValue = "Date\tStatus\tStatus-Code\tTask\tSub-Task\r\n---------------------------------------------------------------------------------------------";
            StringBuilder sb = new StringBuilder();
            foreach (LogEntry entry in this.LogEntries)
            {
                sb.Append(entry.getLogString(status));
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
