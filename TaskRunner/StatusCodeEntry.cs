using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskRunner
{
    /// <summary>
    /// Task Runner - (c) 2017 - Raphael Stoeckli
    /// This program and its code is released under the MIT license
    /// -----------------------------------------------------------
    /// Class representing a status code entry
    /// </summary>
    public class StatusCodeEntry
    {
        /// <summary>
        /// Status code as byte
        /// </summary>
        public byte Code { get; set; }
        /// <summary>
        /// Status ID as string
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// Description of the status
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Task status
        /// </summary>
        public Task.Status Status { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public StatusCodeEntry()
        { }

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="id">ID as string</param>
        /// <param name="code">Status code as byte</param>
        /// <param name="status">Task status</param>
        public StatusCodeEntry(string id, byte code, Task.Status status)
        {
            this.ID = id;
            this.Code = code;
            this.Status = status;
        }

        /// <summary>
        /// Gets the number of the status code (byte to integer)
        /// </summary>
        /// <returns>Integer of the status byte</returns>
        public int GetNumber()
        {
            return this.Code;
        }

    }
}
