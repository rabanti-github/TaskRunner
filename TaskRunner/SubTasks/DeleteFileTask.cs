using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace TaskRunner.SubTasks
{
    /// <summary>
    /// Task Runner - (c) 2017 - Raphael Stoeckli
    /// This program and its code is released under the MIT license
    /// -----------------------------------------------------------
    /// Derived Sub-Task Class for deleting files
    /// </summary>
    public class DeleteFileTask : SubTask
    {

        /// <summary>
        /// Implemented code of the task type (01)
        /// </summary>
        [XmlIgnore]
        public override byte TaskTypeCode
        {
            get { return 0x01; }
        }

        /// <summary>
        /// Type of the Task / Sub-task
        /// </summary>
        [XmlIgnore]
        //public override Task.TaskType Type => Task.TaskType.DeleteFile;
        public override Task.TaskType Type
        {
	        get { return Task.TaskType.DeleteFile; }
        }

        /// <summary>
        /// Name of the demo file
        /// </summary>
        [XmlIgnore]
       // public override string DemoFileName => "DEMO_DeleteFile.xml";
        public override string DemoFileName
        {
            get { return "DEMO_DeleteFile.xml"; }
        }

        /// <summary>
        /// Name of the markdown file
        /// </summary>
        [XmlIgnore]
        public override string MarkdownFileName
        {
            get { return "DeleteFile.md"; }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public DeleteFileTask()
            : base()
        {

        }

        /// <summary>
        /// Implemented Run method of the SubTask class
        /// </summary>
        /// <returns>True if the task was executed successfully, otherwise false</returns>
        public override bool Run()
        {
            try
            {
                if (string.IsNullOrEmpty(this.MainValue))
                {
                    this.Message = "No file to delete was defined";
                    this.StatusCode = 0x02;
                    return false;
                }
                if (File.Exists(this.MainValue) == false)
                {
                    this.Message = this.MainValue + " does not exist. Nothing to do";
                    this.StatusCode = 0x02;
                }
                else
                {
                    System.IO.File.Delete(this.MainValue);
                    this.Message = this.MainValue + " was deleted";
                    this.StatusCode = 0x01;
                }
                return true;
            }
            catch (Exception e)
            {
                this.Message = this.MainValue + " could not be deleted:\n" + e.Message;
                this.StatusCode = 0x01;
                return false;
            }
        }

        /// <summary>
        /// Implemented GetDemoFile method of the SubTask class
        /// </summary>
        /// <param name="number">Optional number to indicate several Sub-Tasks</param>
        /// <returns>Instance of the implemented class</returns>
        public override SubTask GetDemoFile(int number)
        {
            DeleteFileTask t = new DeleteFileTask();
            t.Name = "Delete-File-Task_" + number.ToString();
            t.Description = "This is sub-task " + number.ToString();
            t.MainValue = @"C:\temp\filesToDelete\file" + number.ToString() + ".txt";
            return t;
        }

        /// <summary>
        /// Returns the documentation of the status codes for the specific Sub-Task
        /// </summary>
        /// <returns>Documentation object</returns>
        public override Documentation GetDocumentationStatusCodes()
        {
            Documentation codes = new Documentation("Delete File Task", "Status Codes");
            codes.AddTuple(this.PrintStatusCode(true, 0x01), "The file was deleted successfully");
            codes.AddTuple(this.PrintStatusCode(true, 0x02), "The file does not exist. Nothing to do");
            codes.AddTuple(this.PrintStatusCode(false, 0x01), "The file could not be deleted due to an unknown reason");
            codes.AddTuple(this.PrintStatusCode(false, 0x02), "No file to delete was defined");
            return codes;
        }

        /// <summary>
        /// Returns the documentation of the XML tags for the specific Sub-Task
        /// </summary>
        /// <returns>Documentation object</returns>
        public override Documentation GetTagDocumentationParameters()
        {
            Documentation tags = new Documentation("Delete File Task", "Tags", "The following specific tags are defined (see also the demo files or the example configuration)");
            this.AppendCommonTags(ref tags, "<deleteFileItem>");
            tags.AddTuple("deleteFileItem", "Main tag of a Sub-Task within the <items> tag");
            tags.AddTuple("mainValue", "Defines the full path to the file to delete");
            return tags;
        }

        /// <summary>
        /// Returns the documentation of the XML attributes for the specific Sub-Task
        /// </summary>
        /// <returns>Documentation object</returns>
        public override Documentation GetAttributesDocumentationParameters()
        {
            Documentation attributes = new Documentation("Delete File Task", "Attributes", "The following attributes are defined");
            this.AppendCommonAttributes(ref attributes, "<deleteFileItem>", "DeleteFile");
            return attributes;
        }

        /// <summary>
        /// Returns the description as documentation for the specific Sub-Task
        /// </summary>
        /// <returns>Documentation object</returns>
        public override Documentation GetDocumentationDescription()
        {
            return new Documentation("Delete File Task", "Description", "The tasks deletes one or several files. There are no additional options. At the moment, no wildcards are allowed.");
        }
    }
}
