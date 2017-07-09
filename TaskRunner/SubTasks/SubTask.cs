using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace TaskRunner.SubTasks
{
    /// <summary>
    /// Task Runner - (c) 2017 - Raphael Stoeckli
    /// This program and its code is released under the MIT license
    /// -----------------------------------------------------------
    /// Abstract class as template for Sub-Tasks to execute
    /// </summary>
    [XmlRoot("item")]
    public abstract class SubTask
    {
        /// <summary>
        /// Name of the Sub-Task. Will be displayed in -o mode
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }
        /// <summary>
        /// The main value / command to execute. The meaning of this value varies depending on the actual implementation of the derived class
        /// </summary>
        [XmlElement("mainValue")]
        public string MainValue { get; set; }
        /// <summary>
        /// Optional description of the Sub-Task
        /// </summary>
        [XmlElement("description")]
        public string Description { get; set; }
        /// <summary>
        /// Optional arguments as strings to execute the Sub-Task. The meaning of these values varies depending on the actual implementation of the derived class
        /// </summary>
        [XmlArray("arguments")]
        [XmlArrayItem("argument")]
        public List<string> Arguments { get; set; }
        /// <summary>
        /// Message after execution of the Sub-Task. This Message is not serialized
        /// </summary>
        [XmlIgnore]
        public String Message { get; set; }
        /// <summary>
        /// Optional Message before execution of the Sub-Task. This Message is not serialized
        /// </summary>
        [XmlIgnore]
        public string Prolog { get; set; }
        /// <summary>
        /// Default constructor
        /// </summary>
        public SubTask()
        {
            this.Arguments = new List<string>();
            this.Message = string.Empty;
            this.Prolog = string.Empty;
        }
        /// <summary>
        /// Abstract method to run the Sub-Task
        /// </summary>
        /// <returns>True if the task was executed successfully, otherwise false</returns>
        public abstract bool Run();
        /// <summary>
        /// Abstract method to generate a demo configuration of the implemented class
        /// </summary>
        /// <param name="number">Optional number to indicate several Sub-Tasks</param>
        /// <returns>Sub-Task object of the implemented class</returns>
        public abstract SubTask GetDemoFile(int number);


    }
}
