using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
//using static TaskRunner.Task;

namespace TaskRunner.SubTasks
{
    /// <summary>
    /// Task Runner - (c) 2018 - Raphael Stoeckli
    /// This program and its code is released under the MIT license
    /// -----------------------------------------------------------
    /// Abstract class as template for Sub-Tasks to execute
    /// </summary>
    [XmlRoot("item")]
    public abstract class SubTask
    {
        /// <summary>
        /// Type of the documentation
        /// </summary>
        public enum DocumentationType
        {
            /// <summary>
            /// General description as documentation
            /// </summary>
            Description,
            /// <summary>
            /// Status codes as documentation
            /// </summary>
            StatusCodes,
            /// <summary>
            /// Tags as documentation
            /// </summary>
            Tags,
            /// <summary>
            /// Attributes as documentation
            /// </summary>
            Attributes,
        }

        private List<StatusCodeEntry> statusCodes;

        /// <summary>
        /// Indicates whether the Sub-Task is executed (enabled) or not
        /// </summary>
        [XmlAttribute("enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// Indicates whether a global parameter is used instead of the main value. In this case the main value is the parameter name
        /// </summary>
        [XmlAttribute("useParam")]
        public bool UseParameter { get; set; }

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
        /// Optional condition check for the current SubTask
        /// </summary>
        [XmlElement("condition")]
        public Condition SubTaskCondition { get; set; }
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
        /// Returned status Code (1 byte)
        /// </summary>
        [XmlIgnore]
        public byte StatusCode { get; set; }

        /// <summary>
        /// Abstract hex code of the task type (2 bytes)
        /// </summary>
        [XmlIgnore]
        public abstract byte TaskTypeCode { get; }

        /// <summary>
        /// Type of the Task / Sub-task
        /// </summary>
        [XmlIgnore]
        public abstract Task.TaskType Type { get; }

        /// <summary>
        /// Name of the demo file
        /// </summary>
        [XmlIgnore]
        public abstract string DemoFileName { get; }

        /// <summary>
        /// Name of the markdown file
        /// </summary>
        [XmlIgnore]
        public abstract string MarkdownFileName { get; }

        /// <summary>
        /// Base task of this Sub-Task
        /// </summary>
        [XmlIgnore]
        public Task ParentTask { get; set; }

        /// <summary>
        /// Internal ID of the Sub-Task. The ID is calculated by the Task ID and a sequence number
        /// </summary>
        [XmlIgnore]
        public string SubTaskID { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public SubTask()
        {
            this.statusCodes = new List<StatusCodeEntry>();
            this.Arguments = new List<string>();
            this.Message = string.Empty;
            this.Prolog = string.Empty;
            this.StatusCode = 0x0;
            this.Enabled = true;
            this.UseParameter = false;
            this.SubTaskID = Utils.GetRandomString(8); // Default
        }
        /// <summary>
        /// Abstract method to run the Sub-Task
        /// </summary>
        /// <returns>Sub-task status</returns>
        public abstract Task.Status Run();
        /// <summary>
        /// Abstract method to generate a demo configuration of the implemented class
        /// </summary>
        /// <param name="number">Optional number to indicate several Sub-Tasks</param>
        /// <returns>Sub-Task object of the implemented class</returns>
        public abstract SubTask GetDemoFile(int number);

        /// <summary>
        /// Gets the Documentation as Text
        /// </summary>
        /// <param name="type">Type of the documentation</param>
        /// <param name="maxLength">Number of characters in of one line in the console</param>
        /// <returns>Formated documentation</returns>
        public string GetDocumentation(DocumentationType type, int maxLength)
        {
            if (type == DocumentationType.Description) { return GetDocumentationDescription().GetDocumentation(maxLength);}
            else if (type == DocumentationType.Tags) {return GetTagDocumentationParameters().GetDocumentation(maxLength,true); }
            else if (type == DocumentationType.Attributes) { return GetAttributesDocumentationParameters().GetDocumentation(maxLength, false); }
            else if (type == DocumentationType.StatusCodes)
            {
                Documentation codes = GetDocumentationStatusCodes();
                Documentation prolog = this.GetStatusCodeProlog(codes.Title, codes.SubTitle);
                Documentation modes = this.GetStatusModes(codes.Title, codes.SubTitle);
                codes.Title = string.Empty;
                codes.SubTitle = string.Empty;
                prolog.Title = string.Empty;
                modes.Title = string.Empty;
                modes.SubTitle = string.Empty;
                string part1 = prolog.GetDocumentation(maxLength, false);
                string part2 = codes.GetDocumentation(maxLength, false);
                string part3 = modes.GetDocumentation(maxLength, false);
                return part1 + Documentation.NL + part2 + Documentation.NL + part3;
            }
            else { return ""; }
        }

        /// <summary>
        /// Saves the whole documentation as markdown text
        /// </summary>
        /// <param name="fileName">Filename of the markdown file</param>
        public void SaveMarkdown(string fileName)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(GetDocumentationDescription().GetDocumentation(0,false,true, true, false));
            sb.Append(Documentation.NL);
            sb.Append(GetTagDocumentationParameters().GetDocumentation(0, true, false, true, true));
            sb.Append(Documentation.NL);
            sb.Append(GetAttributesDocumentationParameters().GetDocumentation(0,false,false,true, true));
            sb.Append(Documentation.NL);
            Documentation codes = GetDocumentationStatusCodes();
            Documentation prolog = this.GetStatusCodeProlog(codes.Title, codes.SubTitle);
            Documentation modes = this.GetStatusModes(codes.Title, codes.SubTitle);
            codes.Title = string.Empty;
            codes.SubTitle = string.Empty;
            prolog.Title = string.Empty;
            modes.Title = string.Empty;
            modes.SubTitle = string.Empty;
            string part1 = prolog.GetDocumentation(0, false, true);
            string part2 = codes.GetDocumentation(0, false, true);
            string part3 = modes.GetDocumentation(0, false, true);
            sb.Append(part1);
            sb.Append(Documentation.NL);
            sb.Append(part2);
            sb.Append(Documentation.NL);
            sb.Append(part3);
            sb.Append(Documentation.NL);
            sb.Append(Documentation.NL);
            sb.Append("## Example configuration");
            sb.Append(Documentation.NL);
            sb.Append(Documentation.NL);
            sb.Append("```xml");
            sb.Append(Documentation.NL);
            string demo = Task.CreateDemoFile(this.Type);
            sb.Append(demo);
            sb.Append(Documentation.NL);
            sb.Append("```");
            try
            {
                FileStream fs = new FileStream(fileName, FileMode.Create);
                StreamWriter sw = new StreamWriter(fs);
                sw.Write(sb.ToString());
                sw.Flush();
                fs.Flush();
                fs.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

        /// <summary>
        /// Returns the main value. In case of parameter usage, the value will be taken from the global parameter, otherwise from the value of the config file
        /// </summary>
        /// <param name="displayOutput">If true, error messages will be displayed</param>
        /// <returns>Resolved main value as string</returns>
        public string GetMainValue(bool displayOutput)
        {
            if (this.UseParameter == true)
            {
                Parameter p = Parameter.GetUserParameter(this.MainValue, displayOutput);
                if (p.Valid == false)
                {
                    return "";
                }
                else
                {
                    return p.Value;
                }
            }
            else
            {
                return this.MainValue;
            }
        }

        /// <summary>
        /// Returns the static prolog of status codes as documentation 
        /// </summary>
        /// <param name="title">Title to display</param>
        /// <param name="subtitle">Subtitle to display</param>
        /// <returns>Formatted document</returns>
        public Documentation GetStatusCodeProlog(string title ,string subtitle)
        {
            Documentation doc = new Documentation(title, subtitle, "The Status Code (byte 4 of the Execution Code) is part of the whole Execution Code. The Execution Code consists of:");
            doc.AddTuple("[byte 1]","Execution Mode");
            doc.AddTuple("[byte 2]", "Task Type");
            doc.AddTuple("[byte 3]", "Status of the Execution");
            doc.AddTuple("[byte 4]", "Status Code");
            doc.Suffix = "Following Status Codes are defined for the Status (byte 3 of the Execution Code) '01' [Success], '02' [Error] and '03' [Skipped] for this Task Type '" + Utils.ConvertBytesToString(this.TaskTypeCode) + "'.";
            return doc;
        }

        /// <summary>
        /// Returns the static documentation of the execution modes
        /// </summary>
        /// <param name="title">Title to display</param>
        /// <param name="subtitle">Subtitle to display</param>
        /// <returns>Formatted document</returns>
        public Documentation GetStatusModes(string title, string subtitle)
        {
            Documentation doc = new Documentation(title, subtitle, "The Execution Mode (byte 1 of the Execution Code, above indicated as xx) is the type of the task execution. The following modes are defined:");
            doc.AddTuple("01", "No console output, no logging and no halt on errors");
            doc.AddTuple("02", "No console output, no logging and halt on errors");
            doc.AddTuple("03", "No console output, logging and no halt on errors");
            doc.AddTuple("04", "No console output, logging and halt on errors");
            doc.AddTuple("05", "Console Output, no logging and no halt on errors");
            doc.AddTuple("06", "Console output, no logging and halt on errors");
            doc.AddTuple("07", "Console output, logging and no halt on errors");
            doc.AddTuple("08", "Console output, logging and halt on errors");
            return doc;
        }

        /// <summary>
        /// Appends common attributes to the attribute documentation
        /// </summary>
        /// <param name="documentation">Specific attribute documentation object</param>
        /// <param name="baseTag">Base tag of the Sub-Task</param>
        /// <param name="type">Type of the task as string</param>
        public void AppendCommonAttributes(ref Documentation documentation, string baseTag, string type)
        {
            documentation.AddTuple("name [1]", "The first name attribute is a informal identifier for the Task. It is part of the <task> tag (root tag) and mandatory.");
            documentation.AddTuple("type [1]", "The first type attribute is a the identifier for the Task type. It is part of the <task> tag (root tag) and mandatory. For this Task-Type the valid value is '" + type+"'.");
            documentation.AddTuple("name [2]", "The second name attribute is a informal identifier for the Sub-Task. It is part of the " + baseTag + " tag and mandatory. ");
            documentation.AddTuple("useParam", "The useParam attribute indicates whether a global parameter is used instead of the main value of the config file. In this case, the <mainValue> tag contains the parameter name and not the actual value. It is part of the " + baseTag + " tag and mandatory. Valid values are 'true' and 'false' (default).");
            documentation.AddTuple("expression", "The expression attribute accepts a logical expression which will be evaluated before running the assigned SubTask. If the evaluation of this string return true, the operation in the action attribute will be executed, otherwise the action of the default attribute. The expression accepts system- and user-parameters. The attribute is part of the optional <condition> tag.");
            documentation.AddTuple("action", "The action attribute executes an operation if the expression in the expression attribute returned true. Valid values are: 'run' (Default: runs the SubTask), 'restart_last_subtast' (restarts the last SubTask), 'skip' (skips the current SubTask), 'exit' (terminates TaskRunner), 'restart_task' (restarts the whole MetaTask). Restart actions are not valid in case of pre-conditions if no preceding Sub-Task or Task was executed. The attribute is part of the optional <condition> tag.");
            documentation.AddTuple("default", "The default attribute executes an operation if the expression in the expression attribute returned false. Valid values are: 'run' (runs the SubTask), 'restart_last_subtast' (restarts the last SubTask if applicable), 'skip' (Default: skips the current SubTask), 'exit' (terminates TaskRunner), 'restart_task' (restarts the whole MetaTask). Restart actions are not valid in case of pre-conditions if no preceding Sub-Task or Task was executed. The attribute is part of the optional <condition> tag.");
            documentation.AddTuple("type [2]", "The second type patameter defines whether a condition is checked before a Task or Sub-Task is executed or afterwards. Valid values are 'pre' and 'post'. The attribute is part of the optional <condition> tag.");
        }

        /// <summary>
        /// Appends the common tags to the tag documentation
        /// </summary>
        /// <param name="documentation">Specific tag documentation object</param>
        /// <param name="baseTag">Base tag of the Sub-Task</param>
        public void AppendCommonTags(ref Documentation documentation, string baseTag)
        {
            documentation.AddTuple("task", "The <task> tag is the root tag of the Task.");
            documentation.AddTuple("items", "The <items> tag is the container tag for all Sub-Tasks.");
            documentation.AddTuple("<description> [1]", "The outer <description> tag is an informal tag for the description of the task", true);
            documentation.AddTuple("arguments", "The <arguments> tag is the container tag for all arguments within the " +  baseTag + " tag.");
            documentation.AddTuple("<description> [2]", "The inner <description> tag is an informal tag for the description of the Sub-Task ", true);
            documentation.AddTuple("condition", "Optional condition tag as control for the execution of the current Task or SubTask. Actions are a regular execution, skipping complete restart of a task or the termination of the program. The <condition> tag can either occur within <task> or the " + baseTag + " tag.");
        }

        /// <summary>
        /// Appends the common status codes, applicable for all Sub-Task types
        /// </summary>
        /// <param name="documentation">Specific tag documentation object</param>
        public void AppendCommonStatusCodes(ref Documentation documentation)
        {
            this.statusCodes.Clear();
            documentation.Tuples.Clear();
            this.RegisterStatusCode("N/A", Task.Status.skipped, "Not applicable (task skipped)", ref documentation);
            this.RegisterStatusCode("ERROR", Task.Status.failed, "The task could not be executed due to an unknown reason", ref documentation);
            this.RegisterStatusCode("CONDITION_INVALID_ARGS", Task.Status.failed, "The condition has invalid arguments", ref documentation);
            this.RegisterStatusCode("CONDITION_INVALID_ACTIONS", Task.Status.failed, "The condition has an invalid action", ref documentation);
            this.RegisterStatusCode("CONDITION_INVALID_TYPE", Task.Status.failed, "The condition has an invalid type", ref documentation); 
        }

        /// <summary>
        /// Registers a status code
        /// </summary>
        /// <param name="id">ID as string</param>
        /// <param name="status">Task status</param>
        /// <param name="description">Description text</param>
        /// <param name="documentation">Documentation object</param>
        public void RegisterStatusCode(string id, Task.Status status, string description, ref Documentation documentation)
        {
            int lastNumber = -1;
            foreach(StatusCodeEntry entry in this.statusCodes)
            {
                if (entry.GetNumber() > lastNumber && entry.Status == status)
                {
                    lastNumber = entry.GetNumber();
                }
            }
            lastNumber++;
            if ((status == Task.Status.success || status == Task.Status.failed) && lastNumber == 0) { lastNumber = 1; } // Fix for regular status
            StatusCodeEntry e = new StatusCodeEntry(id, (byte)lastNumber, status);
            e.Description = description;
            this.statusCodes.Add(e);
            documentation.AddTuple(this.PrintStatusCode(id), description);
        }

        /// <summary>
        /// Gets the status code as byte
        /// </summary>
        /// <param name="id">ID as string</param>
        /// <returns>Byte of the status code</returns>
        public byte GetStatusCode(string id)
        {
            foreach (StatusCodeEntry entry in this.statusCodes)
            {
                if (entry.ID == id)
                {
                    return entry.Code;
                }
            }
            Console.WriteLine("Error: The status code '" + id + "' was not found" );
            return 0;
        }

        /// <summary>
        /// Sets the status of the Sub-Task
        /// </summary>
        /// <param name="id">ID of the status as string</param>
        /// <param name="message">Additional message of the status</param>
        /// <returns>Resolved status</returns>
        public Task.Status SetStatus(string id, string message)
        {
            foreach (StatusCodeEntry entry in this.statusCodes)
            {
                if (entry.ID == id)
                {
                    this.StatusCode = entry.Code;
                    this.Message = message;
                    return entry.Status;
                }
            }
            Console.WriteLine("Error: The status code '" + id + "' was not found");
            return Task.Status.none;
        }

        /// <summary>
        /// Gets the status code entry
        /// </summary>
        /// <param name="id">Status code as string</param>
        /// <returns>Resolved SatusCodeEntry</returns>
        public StatusCodeEntry GetStatusCodeEntry(string id)
        {
            foreach (StatusCodeEntry entry in this.statusCodes)
            {
                if (entry.ID == id)
                {
                    return entry;
                }
            }
            Console.WriteLine("Error: The status code '" + id + "' was not found");
            return new StatusCodeEntry();
        }

        /// <summary>
        /// Prints a status code to a specific task
        /// </summary>
        /// <param name="id">ID of the status code</param>
        /// <returns>Formatted string</returns>
        public string PrintStatusCode(string id)//Task.Status status, byte code)
        {
            StatusCodeEntry entry = this.GetStatusCodeEntry(id);
            string type = Utils.ConvertBytesToString(this.TaskTypeCode);
            string codeString = Utils.ConvertBytesToString(entry.Code);
            string statusString = "02"; // False
            if (entry.Status ==  Task.Status.success) { statusString = "01"; } // True
            else if (entry.Status == Task.Status.failed) { statusString = "02"; } // False
            else { statusString = "03"; } // Skipped & termination
            return "xx|" + type + "|" + statusString + "|" + codeString;
        }

        /// <summary>
        /// Gets the description of the Sub-Task as documentation collection
        /// </summary>
        /// <returns>Documentation collection</returns>
        public abstract Documentation GetDocumentationDescription();

        /// <summary>
        /// Gets the status codes of the Sub-Task as documentation collection
        /// </summary>
        /// <returns>Documentation collection</returns>
        public abstract Documentation GetDocumentationStatusCodes();

        /// <summary>
        /// Gets the tag parameters of the Sub-Task as documentation collection
        /// </summary>
        /// <returns>Documentation collection</returns>
        public abstract Documentation GetTagDocumentationParameters();

        /// <summary>
        /// Gets the attributes parameters (of all tags) of the Sub-Task as documentation collection
        /// </summary>
        /// <returns>Documentation collection</returns>
        public abstract Documentation GetAttributesDocumentationParameters();

    }
}
