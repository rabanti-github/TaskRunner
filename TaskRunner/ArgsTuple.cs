using System;
using System.Collections.Generic;
using System.Text;

namespace TaskRunner
{
    /// <summary>
    /// Task Runner - (c)2016 - Raphael Stoeckli
    /// This program and its code is released under the MIT license
    /// -----------------------------------------------------------
    /// Class represents a tuple to execute the program. It contains all possible flags and values
    /// </summary>
    public class ArgsTuple
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
            /// The argument is not defined
            /// </summary>
            undefined
        }
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
        /// Path to the program logfile
        /// </summary>
        public string LogFilePath { get; set; }
        /// <summary>
        /// Path to the configuration file
        /// </summary>
        public String ConfigFilePath { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public ArgsTuple()
        {
            this.HaltOnError = false;
            this.Output = false;
            this.Run = false;
            this.Log = false;
            this.Demo = false;
            this.LogFilePath = string.Empty;
            this.ConfigFilePath = string.Empty;
        }
    }
}
