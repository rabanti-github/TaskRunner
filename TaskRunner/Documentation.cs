using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskRunner
{
    /// <summary>
    /// Task Runner - (c) 2018 - Raphael Stoeckli
    /// This program and its code is released under the MIT license
    /// -----------------------------------------------------------
    /// Class for handling of the console documentation
    /// </summary>
    public class Documentation
    {
        private Output output;

        /// <summary>
        /// Title of the documentation object
        /// </summary>
        public string Title
        {
            get { return this.output.Title; }
            set { this.output.Title = value; }
        }

        /// <summary>
        /// Sub title of the document object
        /// </summary>
        public string SubTitle
        {
            get { return this.output.SubTitle; }
            set { this.output.SubTitle = value; }
        }


        /// <summary>
        /// Description of the documentation object
        /// </summary>
        public string Description
        {
            get { return this.output.Description; }
            set { this.output.Description = value; }
        }

        /// <summary>
        /// Suffix of the documentation object
        /// </summary>
        public string Suffix { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Documentation()
        {
            this.output = new Output();
        }

        /// <summary>
        /// Constructor with parameter
        /// </summary>
        /// <param name="title">Title of the documentation object</param>
        /// <param name="subtitle">Sub title of the documentation object</param>
        public Documentation(string title, string subtitle) : this()
        {
          //  this.Title = title;
          //  this.SubTitle = subtitle;
            this.output.Title = title;
            this.output.SubTitle = subtitle;
        }

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="title">Title of the documentation object</param>
        /// <param name="description">Description of the documentation object</param>
        /// <param name="subtitle">Sub title of the documentation object</param>
        public Documentation(string title, string subtitle, string description) : this()
        {
           // this.Title = title;
           // this.SubTitle = subtitle;
           // this.Description = description;
            this.output.Title = title;
            this.output.SubTitle = subtitle;
            this.output.Description = description;
        }

        public void ClearTuples()
        {
            this.output.Tuples.Clear();
        }

        /// <summary>
        /// Adds a tuple to the documentation object
        /// </summary>
        /// <param name="value">Value of the tuple</param>
        /// <param name="description">Description of the tuple</param>
        public void AddTuple(string value, string description)
        {
            this.AddTuple(value, description, false);
        }

        /// <summary>
        /// Adds a tuple to the documentation object
        /// </summary>
        /// <param name="value">Value of the tuple</param>
        /// <param name="description">Description of the tuple</param>
        /// <param name="overrideTagFormatting">If true, the tuple will be displayed with its raw value and not as XML tag</param>
        public void AddTuple(string value, string description, bool overrideTagFormatting)
        {
            //this.Tuples.Add(new Output.T(value, description, overrideTagFormatting));
            this.output.AddTuple(value, description, overrideTagFormatting);
        }

        /// <summary>
        /// Returns the documentation object as string. Only titles, sub-titles and description texts will be processes (tuples are skipped)
        /// </summary>
        /// <param name="maxLength">Length in numbers of characters (console width)</param>
        /// <returns></returns>
        public string GetDocumentation(int maxLength)
        {
          //return GetDocumentation(maxLength, false, true, false, false);
            return Output.GetOutput(maxLength, false, true, false, false, this.output.Description, this.output.Title, this.output.SubTitle, this.Suffix, this.output.Tuples);
        }

        /// <summary>
        /// Returns the documentation object as string. The output of tuples (as tags) can be controlled by the parameter asTagDoc
        /// </summary>
        /// <param name="maxLength">Length in numbers of characters (console width)</param>
        /// <param name="asTagDoc">If true, the documentation will be rendered as documentation for tags</param>
        /// <returns></returns>
        public string GetDocumentation(int maxLength, bool asTagDoc)
        {
            //return GetDocumentation(maxLength, asTagDoc, false, false, false);
            return Output.GetOutput(maxLength, asTagDoc, false, false, false, this.output.Description, this.output.Title, this.output.SubTitle, this.Suffix, this.output.Tuples);
        }

        /// <summary>
        /// Returns the documentation object as string
        /// </summary>
        /// <param name="maxLength">Length in numbers of characters (console width)</param>
        /// <param name="asTagDoc">If true, the documentation will be rendered as documentation for tags</param>
        /// <param name="asMarkdown">If true, the documentation will be rendered as markdown, The length parameter will be skipped</param>
        /// <returns></returns>
        public string GetDocumentation(int maxLength, bool asTagDoc, bool asMarkdown)
        {
            //return GetDocumentation(maxLength, asTagDoc, false, asMarkdown, false);
            return Output.GetOutput(maxLength, asTagDoc, false, asMarkdown, false, this.output.Description, this.output.Title, this.output.SubTitle, this.Suffix, this.output.Tuples);
        }

        /// </summary>
        /// <param name="maxLength">Length in numbers of characters (console width)</param>
        /// <param name="asTagDoc">If true, the documentation will be rendered as documentation for tags</param>
        /// <param name="asHeader">If true, the header will be rendered</param>
        /// <param name="asMarkdown">If true, the documentation will be rendered as markdown, The length parameter will be skipped</param>
        /// <param name="skipTitle">If true, no Title will be rendered</param>
        /// <returns></returns>
        public string GetDocumentation(int maxLength, bool asTagDoc, bool asHeader, bool asMarkdown, bool skipTitle)

        {
            return Output.GetOutput(maxLength, asTagDoc, asHeader, asMarkdown, skipTitle, this.output.Description, this.output.Title, this.output.SubTitle, this.Suffix, this.output.Tuples);
        }




    }
    }
