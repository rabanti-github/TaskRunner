using System;
using System.Collections.Generic;
using System.Text;

namespace TaskRunner
{
    /// <summary>
    /// Task Runner - (c) 2017 - Raphael Stoeckli
    /// This program and its code is released under the MIT license
    /// -----------------------------------------------------------
    /// Class represents a tuple to execute the program. It contains all possible flags and values
    /// </summary>
    public class Arguments
    {
        /// <summary>
        /// Enum to define the type of the passed argument
        /// </summary>
        public enum ArgType
        {
            /// <summary>
            /// The argument is a flag like -r or --demo
            /// </summary>
            flag,
            /// <summary>
            /// The argument is the path to the configuration file (XML file)
            /// </summary>
            configFile,
            /// <summary>
            /// The argument it the path to the program logfile
            /// </summary>
            logFile,
            /// <summary>
            /// The argument is the number milliseconds as task execution delay
            /// </summary>
            delay,
            /// <summary>
            /// The argument is the number task execution iterations
            /// </summary>
            iterations,
            /// <summary>
            /// The argument is not defined
            /// </summary>
            undefined
        }

        /// <summary>
        /// If true, the usage will be displayed
        /// </summary>
        public bool Help { get; set; }
        /// <summary>
        /// In true, the output of the executed tasks is passed to the command shell. The program runs in silent mode otherwise
        /// </summary>
        public bool Output { get; set; }
        /// <summary>
        /// If true, the program halts on a error while execution of the tasks
        /// </summary>
        public bool HaltOnError { get; set; }
        /// <summary>
        /// If true, a run flag was passed to the program. This starts the execution process
        /// </summary>
        public bool Run { get; set; }
        /// <summary>
        /// In true, the result of the executed tasks are logged to a program logfile
        /// </summary>
        public bool Log { get; set; }
        /// <summary>
        /// If true, a demo flag was passed to the program. This will generate the example configurations in the program folder
        /// </summary>
        public bool Demo { get; set; }
        /// <summary>
        /// If true, the documentation of tasks will be called
        /// </summary>
        public bool Docs { get; set; }
        /// <summary>
        /// If true, the documentation of tasks will be saved as markdown files
        /// </summary>
        public bool Markdown { get; set; }
        /// <summary>
        /// Path to the program logfile
        /// </summary>
        public string LogFilePath { get; set; }
        /// <summary>
        /// Path to the configuration file
        /// </summary>
        public String ConfigFilePath { get; set; }
        /// <summary>
        /// If true, the utilities menu will be called
        /// </summary>
        public bool Utilities { get; set; }
        /// <summary>
        /// If true, each execution (in case of iterative) or just the initial execution will be delayed
        /// </summary>
        public bool DelayExecution{ get; set; }  
        /// <summary>
        /// If true, the first execution of the task will start immediately even if a delay is defined
        /// </summary>
        public bool NoInitialDelay { get; set; }
        /// <summary>
        /// If true, the task is executed iterative and not just once
        /// </summary>
        public bool Iterative { get; set; }
        /// <summary>
        /// Gets or sets the number of iterations of the task execution
        /// </summary>
        public int NumberOfIterations { get; set; }
        /// <summary>
        /// Gets or sets the delay amount in milliseconds
        /// </summary>
        public int DelayAmount { get; set; }


        public bool SetIterations(string input)
        {
            int iterations;
            bool valid = SetValue(input, out iterations);
            NumberOfIterations = iterations;
            return valid;
        }

        public bool SetDelay(string input)
        {
            int delay;
            bool valid = SetValue(input, out delay);
            DelayAmount = delay;
            return valid;
        }

        private bool SetValue(string input, out int value)
        {
            bool valid = Int32.TryParse(input, out value);
            if (valid == true && value >= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Arguments()
        {
            this.Help = false;
            this.HaltOnError = false;
            this.Output = false;
            this.Run = false;
            this.Log = false;
            this.Demo = false;
            this.Docs = false;
            this.Markdown = false;
            this.Utilities = false;
            this.DelayExecution = false;
            this.Iterative = false;
            this.NoInitialDelay = false;
            this.LogFilePath = String.Empty;
            this.ConfigFilePath = String.Empty;
            this.NumberOfIterations = 1; // Will execute only once (fall back)
            this.DelayAmount = 0;
        }

        /// <summary>
        /// Method to check the passed arguments
        /// </summary>
        /// <param name="tuple">Argument tuple as reference</param>
        /// <param name="argValue">Passed argument value</param>
        /// <param name="argType">The expected type of the argument</param>
        /// <returns>The expected type of the next argument. In case of -r|--run and -l|--log, this is configFile or logFile</returns>
        public static Arguments.ArgType CheckArgs(ref Arguments tuple, string argValue, Arguments.ArgType argType)
        {
            string arg = argValue;
            Arguments.ArgType nextArgIs = Arguments.ArgType.flag;
            if (argType == Arguments.ArgType.flag)
            {
                arg = arg.ToLower();
                if (arg == "--run" || arg == "-r")
                {
                    tuple.Run = true;
                    nextArgIs = Arguments.ArgType.configFile;
                }
                else if (arg == "--wait" || arg == "-w")
                {
                    tuple.DelayExecution = true;
                    nextArgIs = Arguments.ArgType.delay;
                }
                else if (arg == "--nodelay" || arg == "-n")
                {
                    tuple.NoInitialDelay = true;
                    nextArgIs = Arguments.ArgType.flag;
                }
                else if (arg == "--iterate" || arg == "-i")
                {
                    tuple.Iterative = true;
                    nextArgIs = Arguments.ArgType.iterations;
                }
                else if (arg == "--help" || arg == "-h")
                {
                    tuple.Help = true;
                    nextArgIs = Arguments.ArgType.flag;
                }
                else if (arg == "--output" || arg == "-o")
                {
                    tuple.Output = true;
                    nextArgIs = Arguments.ArgType.flag;
                }
                else if (arg == "--stop" || arg == "-s")
                {
                    tuple.HaltOnError = true;
                    nextArgIs = Arguments.ArgType.flag;
                }
                else if (arg == "--log" || arg == "-l")
                {
                    tuple.Log = true;
                    nextArgIs = Arguments.ArgType.logFile;
                }
                else if (arg == "--example" || arg == "-e")
                {
                    tuple.Demo = true;
                    nextArgIs = Arguments.ArgType.flag;
                }
                else if (arg == "--docs" || arg == "-d")
                {
                    tuple.Docs = true;
                    nextArgIs = Arguments.ArgType.flag;
                }
                else if (arg == "--markdown" || arg == "-m")
                {
                    tuple.Markdown = true;
                    nextArgIs = Arguments.ArgType.flag;
                }
                else if (arg == "--utils" || arg == "-u")
                {
                    tuple.Utilities = true;
                    nextArgIs = Arguments.ArgType.flag;
                }
                else
                {
                    if (arg.StartsWith("--param") || arg.StartsWith("-p"))
                    {
                        Parameter p = Parameter.Parse(argValue);
                        Parameter.AddUserParameter(p, true);
                        nextArgIs = Arguments.ArgType.flag;
                    }
                    else
                    {
                        nextArgIs = Arguments.ArgType.undefined;
                    }  
                }
            }
            else if (argType == Arguments.ArgType.configFile)
            {
                tuple.ConfigFilePath = arg;
                nextArgIs = Arguments.ArgType.flag;
            }
            else if (argType == Arguments.ArgType.logFile)
            {
                tuple.LogFilePath = arg;
                nextArgIs = Arguments.ArgType.flag;
            }
            else if (argType == Arguments.ArgType.iterations)
            {
                if (tuple.SetIterations(arg) == false)
                {
                    nextArgIs = Arguments.ArgType.undefined;
                }
                else
                {
                    nextArgIs = Arguments.ArgType.flag;
                }
            }
            else if (argType == Arguments.ArgType.delay)
            {
                if (tuple.SetDelay(arg) == false)
                {
                    nextArgIs = Arguments.ArgType.undefined;
                }
                else
                {
                    nextArgIs = Arguments.ArgType.flag;
                }
            }
            else
            {
                nextArgIs = Arguments.ArgType.undefined;
            }
            return nextArgIs;
        }
    }
}
