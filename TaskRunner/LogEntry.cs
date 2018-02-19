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

        /// <summary>
        /// The name of the main Task
        /// </summary>
        public string TaskName { get; set; }
        /// <summary>
        /// The name of the Sub-Task
        /// </summary>
        public string SubTaskName { get; set; }

        /// <summary>
        /// The execution date
        /// </summary>
        public DateTime ExecutionDate { get; set; }
        /// <summary>
        /// The execution code of the Sub-Task (4 bytes)
        /// </summary>
        public byte[] ExecutionCode { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public LogEntry()
        {
            this.ExecutionCode = new byte[4];
        }

        /// <summary>
        /// Returns a formated log entry as string (line). The first value is the current date
        /// </summary>
        /// <param name="status">Status of the log entry</param>
        /// <returns>Formated log entry as string</returns>
        public string getLogString(Task.Status status)
        {
            return this.ExecutionDate.ToString(Task.DATEFORMAT) + "\t" + status + "\t" + PrintExecutionCode() + "\t" + this.TaskName + "\t" + this.SubTaskName;
        }

        /// <summary>
        /// Prints the execution Code as Hex string
        /// </summary>
        /// <returns>4 byte Hex string</returns>
        public string PrintExecutionCode()
        {
            return Utils.ConvertBytesToString(this.ExecutionCode);
        }

        /// <summary>
        /// Inserts a byte into the execution code
        /// </summary>
        /// <param name="value">byte to replace</param>
        /// <param name="index">Index (0 to 3)</param>
        public void InsertCodeByte(byte value, int index)
        {
            this.ExecutionCode[index] = value;
        }

    }
}
