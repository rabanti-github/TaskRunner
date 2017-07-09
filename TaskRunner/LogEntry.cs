using System;
using System.Collections.Generic;
using System.Text;

namespace TaskRunner
{
    /// <summary>
    /// Task Runner - (c) 2017 - Raphael Stoeckli
    /// This program and its code is released under the MIT license
    /// -----------------------------------------------------------
    /// Class represents an entry for a logfile
    /// </summary>
    public class LogEntry
    {
        public string TaskName { get; set; }
        public bool Status { get; set; }
        public DateTime ExecutionDate { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public LogEntry()
        { }

        /// <summary>
        /// Returns a formated log entry as string (line). The first value is the current date
        /// </summary>
        /// <returns>Formated log entry as string</returns>
        public string getLogString()
        {
            return this.ExecutionDate.ToString(Task.DATEFORMAT) + "\t" + this.Status + "\t" + this.TaskName;
        }

    }
}
