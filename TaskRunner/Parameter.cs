using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace TaskRunner
{
    /// <summary>
    /// Task Runner - (c) 2018 - Raphael Stoeckli
    /// This program and its code is released under the MIT license
    /// -----------------------------------------------------------
    /// Class for parameter handling
    /// </summary>
    public class Parameter
    {
        private static Dictionary<string, Parameter> userParameters;
        private static Dictionary<string, Parameter> systemParameters;
        private static Dictionary<string, int> taskIterations;
        private static Dictionary<string, int> subTaskIterations;
        private static string[] dateFormats = null;

        /// <summary>
        /// Static dictionary for user parameters 
        /// </summary>
        public static Dictionary<string, Parameter> UserParameters
        {
            get
            {
                if (Parameter.userParameters == null)
                {
                    Parameter.userParameters = new Dictionary<string, Parameter>();
                }
                return Parameter.userParameters; 
            }
            set { Parameter.userParameters = value; }
        }

        /// <summary>
        /// Static dictionary for system parameters 
        /// </summary>
        public static Dictionary<string, Parameter> SystemParameters
        {
            get
            {
                if (Parameter.systemParameters == null)
                {
                    Parameter.systemParameters = new Dictionary<string, Parameter>();
                }
                return Parameter.systemParameters;
            }
            set { Parameter.systemParameters = value; }
        }

        /// <summary>
        /// Static dictionary to check the iterations of Tasks
        /// </summary>
        public static Dictionary<string, int> TaskIterations
        {
            get
            {
                if (Parameter.taskIterations == null)
                {
                    Parameter.taskIterations = new Dictionary<string, int>();
                }
                return Parameter.taskIterations;
            }
        }

        /// <summary>
        /// Static dictionary to check the iterations of Sub-Tasks
        /// </summary>
        public static Dictionary<string, int> SubTaskIterations
        {
            get
            {
                if (Parameter.subTaskIterations == null)
                {
                    Parameter.subTaskIterations = new Dictionary<string, int>();
                }
                return Parameter.subTaskIterations;
            }
        }

        /// <summary>
        /// Enum for data types
        /// </summary>
        public enum Types
        {
            /// <summary>
            /// Data type is a string
            /// </summary>
            String,
            /// <summary>
            /// Data type is a number (double)
            /// </summary>
            Number,
            /// <summary>
            /// Data type is a boolean
            /// </summary>
            Boolean,
            /// <summary>
            /// Data type is a date / time
            /// </summary>
            DateTime,
        }

        /// <summary>
        /// Enum for the parameter type
        /// </summary>
        public enum ParamType
        {
            /// <summary>
            /// Parameter is a read-only system parameter
            /// </summary>
            SYSTEM,
            /// <summary>
            /// Parameter is a overridable system / environment parameter
            /// </summary>
            ENV,
            /// <summary>
            /// Parameter is a read-only task-related parameter
            /// </summary>
            TASK,
            /// <summary>
            /// Parameter is a read-only sub-task-related parameter
            /// </summary>
            SUBTASK,
            /// <summary>
            /// Parameter is a freely assignable user parameter
            /// </summary>
            USER,
            /// <summary>
            /// Default parameter / Not defined
            /// </summary>
            NONE,
        }

        /// <summary>
        /// Enum for the available system parameters
        /// </summary>
        public enum SysParam
        {
            /// <summary>
            /// Boolean parameter indicates whether the last task was successful
            /// </summary>
            TASK_LAST_SUCCESS,
            /// <summary>
            /// Boolean parameter indicates whether the last task was partially successful
            /// </summary>
            TASK_LAST_SUCCESS_PARTIAL,
            /// <summary>
            /// DateTime parameter of starting time of the last task
            /// </summary>
            TASK_LAST_TIME_START,
            /// <summary>
            /// DateTime parameter of end time of the last task
            /// </summary>
            TASK_LAST_TIME_END,
            /// <summary>
            /// Boolean parameter indicates whether the logging of the last task is suppressed
            /// </summary>
            TASK_LAST_LOGGING_SUPPERSS,
            /// <summary>
            /// Boolean parameter indicates whether all tasks were successful
            /// </summary>
            TASK_ALL_SUCCESS,
            /// <summary>
            /// Boolean parameter indicates whether all tasks were partially successful
            /// </summary>
            TASK_ALL_SUCCESS_PARTIAL,
            /// <summary>
            /// Numeric parameter regarding the total number of tasks
            /// </summary>
            TASK_ALL_NUMBER_TOTAL,
            /// <summary>
            /// Numeric parameter regarding the total number of successful
            /// </summary>
            TASK_ALL_NUMBER_SUCCESS,
            /// <summary>
            /// Numeric parameter regarding the total number of failed tasks
            /// </summary>
            TASK_ALL_NUMBER_FAIL,
            /// <summary>
            /// Boolean parameter indicates whether the last sub-task was successful
            /// </summary>
            SUBTASK_LAST_SUCCESS,
            /// <summary>
            /// Boolean parameter indicates whether the last sub-task was partially successful
            /// </summary>
            SUBTASK_LAST_SUCCESS_PARTIAL,
            /// <summary>
            /// Boolean parameter indicates whether all sub-tasks were successful
            /// </summary>
            SUBTASK_ALL_SUCCESS,
            /// <summary>
            /// DateTime parameter of starting time of the last sub-task
            /// </summary>
            SUBTASK_LAST_TIME_START,
            /// <summary>
            /// DateTime parameter of end time of the last sub-task
            /// </summary>
            SUBTASK_LAST_TIME_END,
            /// <summary>
            /// Boolean parameter indicates whether all sub-task were partially successful
            /// </summary>
            SUBTASK_ALL_SUCCESS_PARTIAL,
            /// <summary>
            /// Numeric parameter regarding the total number of sub-tasks
            /// </summary>
            SUBTASK_ALL_NUMBER_TOTAL,
            /// <summary>
            /// Numeric parameter regarding the total number of successfully sub-tasks
            /// </summary>
            SUBTASK_ALL_NUMBER_SUCCESS,
            /// <summary>
            /// Numeric parameter regarding the total number of failed sub-tasks
            /// </summary>
            SUBTASK_ALL_NUMBER_FAIL,
            /// <summary>
            /// DateTime parameter of start time of the program
            /// </summary>
            SYSTEM_TIME_START,
            /// <summary>
            /// DateTime parameter of end time of the program
            /// </summary>
            SYSTEM_TIME_END,
            /// <summary>
            /// String parameter regarding the result of the last condition evaluation (true branch)
            /// </summary>
            SYSTEM_CONDITION_TRUE,
            /// <summary>
            /// String parameter regarding the result of the last condition evaluation (false branch)
            /// </summary>
            SYSTEM_CONDITION_FALSE,
            /// <summary>
            /// Overridable maximum number of iterations of tasks to avoid infinite loops
            /// </summary>
            ENV_MAX_TASK_ITERATIONS,
            /// <summary>
            /// Overridable maximum number of iterations of sub-tasks to avoid infinite loops
            /// </summary>
            ENV_MAX_SUBTASK_ITERATIONS,
        }

        /// <summary>
        /// Name of the parameter
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// String value of the parameter
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Data type of the parameter
        /// </summary>
        public Types ParameterType { get; set; }

        /// <summary>
        /// Boolean value of the parameter
        /// </summary>
        public bool BooleanValue { get; set; }

        /// <summary>
        /// Numeric value of the parameter
        /// </summary>
        public double NumericValue { get; set; }

        /// <summary>
        /// DateTime value of the parameter
        /// </summary>
        public DateTime DateTimeValue { get; set; }

        /// <summary>
        /// If true, the parameter is valid
        /// </summary>
        public bool Valid { get; set; }

        /// <summary>
        /// Type of the parameter
        /// </summary>
        public ParamType Flag { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Parameter()
        {
            this.Flag = ParamType.USER;
        }

        /// <summary>
        /// Parameter with arguments
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="value">String value of the parameter</param>
        public Parameter(string name, string value): this()
        {
            this.Name = name;
            this.Value = value;
            this.ParameterType = Types.String;
            this.Valid = true;
        }

        /// <summary>
        /// Parameter with arguments
        /// </summary>
        /// <param name="rawValue">Raw value</param>
        public Parameter(string rawValue) : this()
        {
            Parameter p = Parameter.Parse(rawValue);
            this.BooleanValue = p.BooleanValue;
            this.Name = p.Name;
            this.NumericValue = p.NumericValue;
            this.ParameterType = p.ParameterType;
            this.DateTimeValue = p.DateTimeValue;
            this.Valid = p.Valid;
            this.Value = p.Value;
        }

        /// <summary>
        /// Method to register or increment a task in the list to check iterations. If the task does not exist, it will be added with the counter 0. In it exists, the counter will be increased by 1.
        /// </summary>
        /// <param name="task">Task to register</param>
        public static void RegisterTaskIterations(Task task)
        {
            if (Parameter.TaskIterations.ContainsKey(task.TaskID))
            {
                Parameter.TaskIterations[task.TaskID]++;
                return;
            }
            else
            {
                Parameter.TaskIterations.Add(task.TaskID, 0);
                foreach(SubTasks.SubTask s in task.Items)
                {
                    Parameter.SubTaskIterations.Add(s.SubTaskID, 0);
                }
            }
        }

        /// <summary>
        /// Method to check the number of iterations of a Task
        /// </summary>
        /// <param name="taskID">Task ID to check</param>
        /// <param name="displayOutput">If true, information about errors is passed to the command shell</param>
        /// <returns>If true, the task can be executed, otherwise the program will be terminated</returns>
        public static bool CheckTaskIteration(string taskID, bool displayOutput)
        {
            if (Parameter.taskIterations.ContainsKey(taskID) == false)
            {
                if (displayOutput == true)
                { Console.WriteLine("Error: the Task-ID '" + taskID + "' was not found"); }
                return false;
            }
            Parameter.taskIterations[taskID]++;
            if (Parameter.taskIterations[taskID] >= Parameter.GetSystemParameter(SysParam.ENV_MAX_TASK_ITERATIONS).NumericValue)
            {
                if (displayOutput == true)
                { Console.WriteLine("The maximum number of Task iteration for the Task-ID '" + taskID + "' was reached."); }
                return false;
            }
            else
            { return true; }
        }

        /// <summary>
        /// Method to check the number of iterations of a Sub-Task
        /// </summary>
        /// <param name="subTaskID">Sub-Task ID to check</param>
        /// <param name="displayOutput">If true, information about errors is passed to the command shell</param>
        /// <returns>If true, the task can be executed, otherwise the program will be terminated</returns>
        public static bool CheckSubTaskIteration(string subTaskID, bool displayOutput)
        {
            if (Parameter.subTaskIterations.ContainsKey(subTaskID) == false)
            {
                if (displayOutput == true)
                { Console.WriteLine("Error: the Sub-Task-ID '" + subTaskID + "' was not found"); }
                return false;
            }
            Parameter.subTaskIterations[subTaskID]++;
            if (Parameter.subTaskIterations[subTaskID] >= Parameter.GetSystemParameter(SysParam.ENV_MAX_SUBTASK_ITERATIONS).NumericValue)
            {
                if (displayOutput == true)
                { Console.WriteLine("The maximum number of Sub-Task iteration for the Task-ID '" + subTaskID + "' was reached. The task will be terminated"); }
                return false;
            }
            else
            { return true; }
        }

        /// <summary>
        /// Method to return the value of a parameter
        /// </summary>
        /// <param name="name">Parameter to check</param>
        /// <param name="displayOutput">If true, information about errors is passed to the command shell</param>
        /// <returns>Parameter of the name</returns>
        public static Parameter GetParameter(string name, bool displayOutput)
        {
            Parameter p = Parameter.GetParameter(name, false, true);
            if (p.Valid == true) { return p; }
            else
            {
                return Parameter.GetParameter(name, displayOutput, false);
            }
        }

        /// <summary>
        /// Method to return a user parameter
        /// </summary>
        /// <param name="name">Parameter to check</param>
        /// <param name="displayOutput">If true, information about errors is passed to the command shell</param>
        /// <returns>Parameter of the name</returns>
        public static Parameter GetUserParameter(string name, bool displayOutput)
        {
            return Parameter.GetParameter(name, displayOutput, true);
        }

        /// <summary>
        /// Method to return a system parameter
        /// </summary>
        /// <param name="parameter">Parameter to check</param>
        /// <returns>Parameter of the name</returns>
        public static Parameter GetSystemParameter(string parameter)
        {
            return Parameter.GetParameter(parameter, false, false);
        }

        /// <summary>
        /// Method to return a system parameter
        /// </summary>
        /// <param name="parameter">Parameter to check</param>
        /// <returns>Parameter of the name</returns>
        public static Parameter GetSystemParameter(Parameter.SysParam parameter)
        {
            return Parameter.GetParameter(parameter.ToString(), false, false);
        }

        /// <summary>
        /// Internal method to return a parameter
        /// </summary>
        /// <param name="name">Parameter to check</param>
        /// <param name="displayOutput">If true, information about errors is passed to the command shell</param>
        /// <param name="userParameter">If true, a user parameter will be retrieved, otherwise a system parameter</param>
        /// <returns>Parameter of the name</returns>
        private static Parameter GetParameter(string name, bool displayOutput, bool userParameter)
        {
            Parameter p = null;
            if (userParameter == true)
            {
                if (Parameter.UserParameters.ContainsKey(name))
                {
                    p = Parameter.UserParameters[name];
                }
                else
                {
                    if (displayOutput == true && userParameter == true)
                    {
                        Console.WriteLine("the parameter '" + name + "' was not found");
                    }
                    p = new Parameter(name, "");
                    p.Valid = false;
                }
            }
            else
            {
                if (Parameter.SystemParameters.ContainsKey(name))
                {
                    p = Parameter.SystemParameters[name];
                }
                else
                {
                    p = new Parameter(name, "");
                    p.Valid = false;
                }
            }
            return p;
        }

        /// <summary>
        /// Method to add a user parameter
        /// </summary>
        /// <param name="parameter">Parameter name</param>
        /// <param name="displayOutput">If true, information about errors is passed to the command shell</param>
        /// <returns>In true the parameter was added successfully</returns>
        public static bool AddUserParameter(Parameter parameter, bool displayOutput)
        {
            ParamType paramType;
            Types dataType;
            if (CheckGlobalParameterName(parameter.Name, out paramType, out dataType) == false)
            {
                if (displayOutput == true)
                {
                    Console.WriteLine("the passed parameter '" + parameter.Name + "' is assigned to a reserved system parameter and cannot be overwritten");
                }
                return false;
            }
            else
            {
                if (paramType == ParamType.ENV)
                {
                    if (dataType != parameter.ParameterType)
                    {
                        if (displayOutput == true)
                        {
                            Console.WriteLine("the passed environment parameter '" + parameter.Name + "' cannot be overwritten because of a wrong data type");
                        }
                        return false;
                    }
                    return Parameter.AddParameter(parameter, displayOutput, true, paramType);
                }
                else // User
                {
                    return Parameter.AddParameter(parameter, displayOutput, true);
                }
            }
        }

        /// <summary>
        /// Method to add a system parameter
        /// </summary>
        /// <param name="parameter">Parameter name</param>
        /// <param name="flag">Parameter type</param>
        /// <returns>In true the parameter was added successfully</returns>
        private static bool AddSystemParameter(Parameter parameter, ParamType flag)
        {
            parameter.Flag = flag;
            return Parameter.AddParameter(parameter, false, false);
        }

        /// <summary>
        /// Internal method to add a parameter
        /// </summary>
        /// <param name="parameter">Parameter name</param>
        /// <param name="displayOutput">If true, information about errors is passed to the command shell</param>
        /// <param name="userParameter">If true, the parameter is a user or environment parameter, otherwise a system parameter</param>
        /// <param name="paramType">Optional parameter type to check whether parameter is a user, system or overridable environment parameter</param>
        /// <returns>In true the parameter was added successfully</returns>
        private static bool AddParameter(Parameter parameter, bool displayOutput, bool userParameter, ParamType paramType = ParamType.NONE)
        {
            string msg = "";
            bool error = false;
            if (Parameter.ValidateParameterName(parameter.Name) == false)
            {
                msg = "the passed parameter name '" + parameter.Name + "' is invalid";
                error = true;
            }
            if (parameter.Valid == false)
            { 
                msg = "the passed parameter value '" + parameter.Value + "' is invalid";
                error = true;
            }
            if (error == true)
            {
                if (displayOutput == true) { Console.WriteLine(msg); }
                return false;
            }
            if (userParameter == true && paramType != ParamType.ENV) // ENV is assigned to system parameters although passed as user parameter
            {
                if (Parameter.UserParameters.ContainsKey(parameter.Name))
                {
                    Parameter.UserParameters[parameter.Name] = parameter;
                    if (displayOutput == true)
                    {
                        Console.WriteLine("the passed parameter '" + parameter.Name + "' was overwritten by value '" + parameter.Value + "'");
                    }
                }
                else
                {
                    Parameter.UserParameters.Add(parameter.Name, parameter);
                }
            }
            else
            {
                if (Parameter.SystemParameters.ContainsKey(parameter.Name))
                {
                    parameter.Flag = paramType; // set to appropriate flag
                    Parameter.SystemParameters[parameter.Name] = parameter;
                    if (displayOutput == true && paramType == ParamType.ENV)
                    {
                        Console.WriteLine("the environment parameter '" + parameter.Name + "' was overwritten by value '" + parameter.Value + "'");
                    }
                }
                else
                {
                    Parameter.SystemParameters.Add(parameter.Name, parameter);
                }
            }
            return true;
        }

        /// <summary>
        /// Method to check a parameter before inserting
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <returns>If true, the parameter is valid</returns>
        private static bool ValidateParameterName(string name)
        {
           Regex rx = new Regex(@"^[a-zA-Z_0-9]*$");
           if (rx.IsMatch(name)) { return true; }
           else { return false; }
        }

        /// <summary>
        /// Method to add 1 to a numeric system parameter
        /// </summary>
        /// <param name="key">Parameter enumeration</param>
        public static void UpdateSystemParameterNumber(SysParam key)
        {
            Parameter p = Parameter.GetSystemParameter(key);
            Parameter.UpdateSystemParameterInternal(key, p.NumericValue + 1);
        }

        /// <summary>
        /// Method to update a numeric system parameter
        /// </summary>
        /// <param name="key">Parameter number</param>
        /// <param name="number">Value to update</param>
        public static void UpdateSystemParameter(SysParam key, double number)
        {
            Parameter.UpdateSystemParameterInternal(key, number);
        }

        /// <summary>
        /// Method to update a DateTime system parameter
        /// </summary>
        /// <param name="key">Parameter number</param>
        /// <param name="date">Value to update</param>
        public static void UpdateSystemParameter(SysParam key, DateTime date)
        {
            Parameter.UpdateSystemParameterInternal(key, date);
        }

        /// <summary>
        /// Method to update a boolean system parameter
        /// </summary>
        /// <param name="key">Parameter number</param>
        /// <param name="boolvalue">Value to update</param>
        public static void UpdateSystemParameter(SysParam key, bool boolvalue)
        {
            Parameter.UpdateSystemParameterInternal(key, boolvalue);
        }

        /// <summary>
        /// Method to update a string system parameter
        /// </summary>
        /// <param name="key">Parameter number</param>
        /// <param name="value">Value to update</param>
        public static void UpdateSystemParameter(SysParam key, string value)
        {
            Parameter.UpdateSystemParameterInternal(key, value);
        }

        /// <summary>
        /// Internal method to update a system parameter
        /// </summary>
        /// <param name="parameter">Parameter number</param>
        /// <param name="o">Value to update</param>
        private static void UpdateSystemParameterInternal(SysParam parameter, object o)
        {
            string key = parameter.ToString(); 
            if (Parameter.SystemParameters.ContainsKey(key) == false)
            {
                Console.WriteLine("Error while updating system parameter '" + key + "' (not found)");
                return;
            }
            Parameter p = GetSystemParameter(parameter);
            if (o.GetType() == typeof(DateTime)) { p.DateTimeValue = (DateTime)o; p.Value = p.DateTimeValue.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture); }
            else if (o.GetType() == typeof(double)) { p.NumericValue = (Double)o; p.Value = p.NumericValue.ToString(); }
            else if (o.GetType() == typeof(bool)) { p.BooleanValue = (bool)o; p.Value = p.BooleanValue.ToString(); }
            else { p.Value = o.ToString(); }
            Parameter.SystemParameters[key] = p;
        }

        /// <summary>
        /// Method to register all system parameters
        /// </summary>
        public static void RegisterSystemParameters()
        {
            Parameter.AddSystemParameter(new Parameter("-p:b:" + SysParam.TASK_LAST_SUCCESS.ToString() + ":false"), ParamType.TASK);
            Parameter.AddSystemParameter(new Parameter("-p:b:" + SysParam.TASK_LAST_SUCCESS_PARTIAL.ToString() + ":false"), ParamType.TASK);
            Parameter.AddSystemParameter(new Parameter("-p:d:" + SysParam.TASK_LAST_TIME_START.ToString() + ":" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)), ParamType.TASK);
            Parameter.AddSystemParameter(new Parameter("-p:d:" + SysParam.TASK_LAST_TIME_END.ToString() + ":" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)), ParamType.TASK);
            Parameter.AddSystemParameter(new Parameter("-p:b:" + SysParam.TASK_ALL_SUCCESS.ToString() + ":true"), ParamType.TASK); // Default true
            Parameter.AddSystemParameter(new Parameter("-p:b:" + SysParam.TASK_LAST_LOGGING_SUPPERSS.ToString() + ":false"), ParamType.TASK);
            Parameter.AddSystemParameter(new Parameter("-p:b:" + SysParam.TASK_ALL_SUCCESS_PARTIAL.ToString() + ":false"), ParamType.TASK);
          //  Parameter.AddSystemParameter(new Parameter("-p:d:" + SysParam.TASK_ALL_TIME_START.ToString() + ":" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + "'"), "TASK");
          //  Parameter.AddSystemParameter(new Parameter("-p:d:" + SysParam.TASK_ALL_TIME_END.ToString() + ":" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + "'"), "TASK");
            Parameter.AddSystemParameter(new Parameter("-p:n:" + SysParam.TASK_ALL_NUMBER_TOTAL.ToString() + ":0"), ParamType.TASK);
            Parameter.AddSystemParameter(new Parameter("-p:n:" + SysParam.TASK_ALL_NUMBER_SUCCESS.ToString() + ":0"), ParamType.TASK);
            Parameter.AddSystemParameter(new Parameter("-p:n:" + SysParam.TASK_ALL_NUMBER_FAIL.ToString() + ":0"), ParamType.TASK);
            Parameter.AddSystemParameter(new Parameter("-p:b:" + SysParam.SUBTASK_LAST_SUCCESS.ToString() + ":false"), ParamType.SUBTASK);
            Parameter.AddSystemParameter(new Parameter("-p:b:" + SysParam.SUBTASK_LAST_SUCCESS_PARTIAL.ToString() + ":false"), ParamType.SUBTASK);
            Parameter.AddSystemParameter(new Parameter("-p:b:" + SysParam.SUBTASK_ALL_SUCCESS.ToString() + ":true"), ParamType.SUBTASK); // default
            Parameter.AddSystemParameter(new Parameter("-p:d:" + SysParam.SUBTASK_LAST_TIME_START.ToString() + ":" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)), ParamType.SUBTASK);
            Parameter.AddSystemParameter(new Parameter("-p:d:" + SysParam.SUBTASK_LAST_TIME_END.ToString() + ":" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)), ParamType.SUBTASK);
            Parameter.AddSystemParameter(new Parameter("-p:b:" + SysParam.SUBTASK_ALL_SUCCESS_PARTIAL.ToString() + ":false"), ParamType.SUBTASK);
            Parameter.AddSystemParameter(new Parameter("-p:n:" + SysParam.SUBTASK_ALL_NUMBER_TOTAL.ToString() + ":0"), ParamType.SUBTASK);
            Parameter.AddSystemParameter(new Parameter("-p:n:" + SysParam.SUBTASK_ALL_NUMBER_SUCCESS.ToString() + ":0"), ParamType.SUBTASK);
            Parameter.AddSystemParameter(new Parameter("-p:n:" + SysParam.SUBTASK_ALL_NUMBER_FAIL.ToString() + ":0"), ParamType.SUBTASK);
            Parameter.AddSystemParameter(new Parameter("-p:d:" + SysParam.SYSTEM_TIME_START.ToString() + ":" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)), ParamType.SYSTEM);
            Parameter.AddSystemParameter(new Parameter("-p:d:" + SysParam.SYSTEM_TIME_END.ToString() + ":" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)), ParamType.SYSTEM);
            Parameter.AddSystemParameter(new Parameter("-p:s:" + SysParam.SYSTEM_CONDITION_TRUE.ToString() + ":none"), ParamType.SYSTEM);
            Parameter.AddSystemParameter(new Parameter("-p:s:" + SysParam.SYSTEM_CONDITION_FALSE.ToString() + ":none"), ParamType.SYSTEM);
            Parameter.AddSystemParameter(new Parameter("-p:n:" + SysParam.ENV_MAX_TASK_ITERATIONS.ToString() + ":10"), ParamType.ENV); // Environment
            Parameter.AddSystemParameter(new Parameter("-p:n:" + SysParam.ENV_MAX_SUBTASK_ITERATIONS.ToString() + ":10"), ParamType.ENV); // Environment
        }

        /// <summary>
        /// Method to reset the parameters regarding Sub-Tasks
        /// </summary>
        public static void ResetSubTaskParameters()
        {
            Parameter.UpdateSystemParameter(Parameter.SysParam.SUBTASK_ALL_SUCCESS, false);
            Parameter.UpdateSystemParameter(Parameter.SysParam.SYSTEM_CONDITION_TRUE, "none");
            Parameter.UpdateSystemParameter(Parameter.SysParam.SYSTEM_CONDITION_FALSE, "none");
        }

        /// <summary>
        /// Method to reset the parameters regarding Tasks
        /// </summary>
        public static void ResetTaskParameters()
        {
            Parameter.UpdateSystemParameter(SysParam.TASK_LAST_LOGGING_SUPPERSS, false);
            Parameter.UpdateSystemParameter(Parameter.SysParam.SYSTEM_CONDITION_TRUE, "none");
            Parameter.UpdateSystemParameter(Parameter.SysParam.SYSTEM_CONDITION_FALSE, "none");
        }

        /// <summary>
        /// Method to check whether a global parameter name is valid
        /// </summary>
        /// <param name="name">Name to check</param>
        /// <param name="paramType">Type of the parameter as out parameter</param>
        /// <param name="dataType">Data type of the parameter</param>
        /// <returns>If true, the checked parameter is valid</returns>
        private static bool CheckGlobalParameterName(string name, out ParamType paramType, out Types dataType)
        {
            bool envFound = false;
            Types envType = Types.String;
            foreach (KeyValuePair<string, Parameter> item in Parameter.SystemParameters)
            {
                if (item.Key == name && item.Value.Flag != ParamType.USER && item.Value.Flag != ParamType.ENV) 
                {
                    paramType = item.Value.Flag;
                    dataType = item.Value.ParameterType;
                    return false; 
                }
                else if (item.Key == name && item.Value.Flag == ParamType.ENV)
                {
                    envFound = true;
                    envType = item.Value.ParameterType;
                    break;
                }
            }

            if (envFound == false)
            {
                paramType = ParamType.NONE; // Default (can be USER)
                dataType = Types.String;
            }
            else
            {
                paramType = ParamType.ENV;
                dataType = envType;
            }

            return true;
        }

        /// <summary>
        /// Method to parse a parameter from a string
        /// </summary>
        /// <param name="rawValue">Raw value</param>
        /// <returns>Resolved parameter</returns>
        public static Parameter Parse(string rawValue)
        {
            Parameter p = new Parameter();
            if (string.IsNullOrEmpty(rawValue) == true) { p.Valid = false; return p; }
            if (rawValue.StartsWith("-p:") == false && rawValue.StartsWith("--param:") == false) { p.Valid = false; return p; }
            string param;
            if (rawValue.StartsWith("-p:"))
            {
                param = rawValue.Substring(3);
            }
            else
            {
                param = rawValue.Substring(8);
            }
            
            string param2;
            if (param.ToLower().StartsWith("s:") == true)
            { 
                p.ParameterType = Types.String;
                param2 = param.Substring(2);
            }
            else if (param.ToLower().StartsWith("n:") == true)
            {
                p.ParameterType = Types.Number;
                param2 = param.Substring(2);
            }
            else if (param.ToLower().StartsWith("b:") == true)
            {
                p.ParameterType = Types.Boolean;
                param2 = param.Substring(2);
            }
            else if (param.ToLower().StartsWith("d:") == true)
            {
                p.ParameterType = Types.DateTime;
                param2 = param.Substring(2);
            }
            else
            { 
                p.ParameterType = Types.String;
                param2 = param;
            }
            string[] tuple = param2.Split(':');
            if (tuple.Length < 2)
            {
                p.Valid = false;
                return p;
            }
            p.Name = tuple[0];
            param = param2.Substring(p.Name.Length + 1);
            if ((param.StartsWith("\"") && param.EndsWith("\"")) || (param.StartsWith("'") && param.EndsWith("'")))
            {
                p.Value = param.Substring(1, param.Length - 2);
            }
            else
            {
                p.Value = param;
            }
            if (p.ParameterType == Types.Boolean)
            {
                bool bv;
                if (bool.TryParse(p.Value, out bv) == false)
                {
                    p.Valid = false;
                    return p;
                }
                else
                {
                    p.BooleanValue = bv;
                }
            }
            if (p.ParameterType == Types.Number)
            {
                System.Globalization.NumberStyles style = NumberStyles.Any;
                CultureInfo ci = CultureInfo.InvariantCulture;
                p.Value = p.Value.ToLower().Replace(" ", ""); // Remove all white spaces within the number
    
                double dv;
                if (p.Value == "min")
                {
                    dv = Double.MinValue;
                    p.NumericValue = dv;
                }
                else if (p.Value == "max")
                {
                    dv = Double.MaxValue;
                    p.NumericValue = dv;
                }
                else
                {
                    if (double.TryParse(p.Value, style, ci, out dv) == false)
                    {
                        p.Valid = false;
                        return p;
                    }
                    else
                    {
                        p.NumericValue = dv;
                    }
                }
            }
            if (p.ParameterType == Types.DateTime)
            {
                if (Parameter.dateFormats == null)
                {
                    GetDateParsingFormats();
                }
                DateTime dv;
                CultureInfo ci = CultureInfo.InvariantCulture;
                DateTimeStyles style = DateTimeStyles.AdjustToUniversal;
              //  string[] formats = ci.DateTimeFormat.GetAllDateTimePatterns();
                if (DateTime.TryParseExact(p.Value, Parameter.dateFormats, ci, style, out dv) == false)
                {
                    p.Valid = false;
                    return p;
                }
                else
                {
                    p.DateTimeValue = dv;
                }
            }
            p.Valid = true;
            return p;
        }

        private static void GetDateParsingFormats()
        {
            List<string> formats = new List<string>();
            string[] localFormats;
            CultureInfo[] ci = CultureInfo.GetCultures(CultureTypes.AllCultures);
            int len = ci.Length;
            int j, len2;
            for (int i = 0; i < len; i++)
            {
                localFormats = ci[i].DateTimeFormat.GetAllDateTimePatterns();
                len2 = localFormats.Length;
                for (j = 0; j < len2; j++)
                {
                    if (formats.Contains(localFormats[j]) == false)
                    {
                        formats.Add(localFormats[j]);
                    }
                }
            }
            dateFormats = formats.ToArray();
        }

    }
}
