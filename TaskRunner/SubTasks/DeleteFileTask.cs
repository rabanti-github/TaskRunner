﻿using System;
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
        public override Task.TaskType Type
        {
	        get { return Task.TaskType.DeleteFile; }
        }

        /// <summary>
        /// Name of the demo file
        /// </summary>
        [XmlIgnore]
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
        /// <returns>Sub-task status</returns>
        public override Task.Status Run()
        {
            try
            {
                if (string.IsNullOrEmpty(this.MainValue))
                {
                    return this.SetStatus("NO_FILE", "No file to delete was defined");
                }
                if (File.Exists(this.MainValue) == false)
                {
                    return this.SetStatus("SUCCESS_NO_ACTION", this.MainValue + " does not exist. Nothing to do");
                }
                else
                {
                    System.IO.File.Delete(this.MainValue);
                    return this.SetStatus("SUCCESS_DELETED", this.MainValue + " was deleted");
                }
                //return Task.Status.success;
            }
            catch (Exception e)
            {
                return this.SetStatus("ERROR", this.MainValue + " could not be deleted:\n" + e.Message);
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
            this.AppendCommonStatusCodes(ref codes);
            this.RegisterStatusCode("NO_FILE", Task.Status.failed, "No file to delete was defined", ref codes);
            this.RegisterStatusCode("SUCCESS_DELETED", Task.Status.success, "The file was deleted successfully", ref codes);
            this.RegisterStatusCode("SUCCESS_NO_ACTION", Task.Status.success, "The file does not exist. Nothing to do", ref codes);
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
