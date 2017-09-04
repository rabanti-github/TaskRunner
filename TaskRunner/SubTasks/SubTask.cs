using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
//using static TaskRunner.Task;

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

        private bool killSubTask = false;

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
        /// Default constructor
        /// </summary>
        public SubTask()
        {
            this.Arguments = new List<string>();
            this.Message = string.Empty;
            this.Prolog = string.Empty;
            this.StatusCode = 0x0;
            this.Enabled = true;
            this.UseParameter = false;
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

        /// <summary>
        /// Gets the Documentation as Text
        /// </summary>
        /// <param name="type">Type of the documentation</param>
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
                Parameter p = Parameter.GetParameter(this.MainValue, displayOutput);
                if (p.Valid == false)
                {
                    this.killSubTask = true;
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
            doc.Suffix = "Following Status Codes are defined for the Status (byte 3 of the Execution Code) '01' [Success] and '02' [Error] for this Task Type '" + Utils.ConvertBytesToString(this.TaskTypeCode) + "'.";
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
            documentation.AddTuple("type", "The type attribute is a the identifier for the Task type. It is part of the <task> tag (root tag) and mandatory. For this Task-Type the valid value is '" + type+"'.");
            documentation.AddTuple("name [2]", "The second name attribute is a informal identifier for the Sub-Task. It is part of the " + baseTag + " tag and mandatory. ");
            documentation.AddTuple("useParam", "The useParam attribute indicates whether a global parameter is used instead of the main value of the config file. In this case, the <mainValue> tag contains the parameter name and not the actual value. It is part of the " + baseTag + " tag and mandatory. Valid values are 'true' and 'false' (default).");
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
        }

            /// <summary>
            /// Prints a status code to a specific task
            /// </summary>
            /// <param name="status">Status (success or failure) of the task</param>
            /// <param name="code">Status code to print</param>
            /// <returns>Formatted string</returns>
            public string PrintStatusCode(bool status, byte code)
        {
            string type = Utils.ConvertBytesToString(this.TaskTypeCode);
            string codeString = Utils.ConvertBytesToString(code);
            string statusString = "02"; // False
            if (status == true) { statusString = "01"; } // True
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
