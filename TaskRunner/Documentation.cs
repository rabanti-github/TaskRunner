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
    /// Class for handling of the console documentation
    /// </summary>
    public class Documentation
    {
        /// <summary>
        /// Globally used new line sequence
        /// </summary>
        public static string NL = System.Environment.NewLine;

        /// <summary>
        /// Tuples of the documentation object
        /// </summary>
        public List<T> Tuples { get; set; }
        /// <summary>
        /// Title of the documentation object
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Sub title of the document object
        /// </summary>
        public string SubTitle { get; set; }

        /// <summary>
        /// Description of the documentation object
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Suffix of the documentation object
        /// </summary>
        public string Suffix { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Documentation()
        {
            this.Tuples = new List<T>();
        }

        /// <summary>
        /// Constructor with parameter
        /// </summary>
        /// <param name="title">Title of the documentation object</param>
        /// <param name="subtitle">Sub title of the documentation object</param>
        public Documentation(string title, string subtitle)
            : this()
        {
            this.Title = title;
            this.SubTitle = subtitle;
        }

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="title">Title of the documentation object</param>
        /// <param name="description">Description of the documentation object</param>
        /// <param name="subtitle">Sub title of the documentation object</param>
        public Documentation(string title, string subtitle, string description) : this()
        {
            this.Title = title;
            this.SubTitle = subtitle;
            this.Description = description;
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
        /// <param name="overrodeTagFormatting">If true, the tuple will be displayed with its raw value and not as XML tag</param>
        public void AddTuple(string value, string description, bool overrodeTagFormatting)
        {
            this.Tuples.Add(new T(value, description, overrodeTagFormatting));
        }

        /// <summary>
        /// Returns the documentation object as string
        /// </summary>
        /// <param name="maxLength">>Length in numbers of characters (console width)</param>
        /// <returns></returns>
        public string GetDocumentation(int maxLength)
        {
          return GetDocumentation(maxLength, false, true, false, false);
        }

        /// <summary>
        /// Returns the documentation object as string
        /// </summary>
        /// <param name="maxLength">>Length in numbers of characters (console width)</param>
        /// <param name="asTagDoc">If true, the documentation will be rendered as documentation for tags</param>
        /// <returns></returns>
        public string GetDocumentation(int maxLength, bool asTagDoc)
        {
            return GetDocumentation(maxLength, asTagDoc, false, false, false);
        }

        /// <summary>
        /// Returns the documentation object as string
        /// </summary>
        /// <param name="maxLength">Length in numbers of characters (console width)</param>
        /// <param name="asTagDoc">If true, the documentation will be rendered as documentation for tags</param>
        /// <param name="asMarkdown">If true, the documentation will be rendered as markdown, The lengthparameter will be skipped</param>
        /// <returns></returns>
        public string GetDocumentation(int maxLength, bool asTagDoc, bool asMarkdow)
        {
            return GetDocumentation(maxLength, asTagDoc, false, asMarkdow, false);
        }

        /// <summary>
        /// Returns the documentation object as string
        /// </summary>
        /// <param name="maxLength">Length in numbers of characters (console width)</param>
        /// <param name="asTagDoc">If true, the documentation will be rendered as documentation for tags</param>
        /// <param name="asHeader">If true, the header will be rendered</param>
        /// <param name="asMarkdown">If true, the documentation will be rendered as markdown, The length parameter will be skipped</param>
        /// <param name="skipTitle">If true, no Title will be rendered</param>
        /// <returns></returns>
        public string GetDocumentation(int maxLength, bool asTagDoc, bool asHeader, bool asMarkdown, bool skipTitle)
        {
            StringBuilder sb = new StringBuilder();
            if (string.IsNullOrEmpty(this.Title) == false)
            {
                if (asMarkdown == true)
                {
                    if (skipTitle == false)
                    {
                        sb.Append("# ");
                        sb.Append(this.Title);
                        sb.Append(NL);
                    }
                    if (string.IsNullOrEmpty(this.SubTitle) == false)
                    {
                        sb.Append(NL);
                        sb.Append("## ");
                        sb.Append(this.SubTitle);
                        sb.Append(NL);
                    }
                    sb.Append(NL);
                }
                else
                {
                    sb.Append(NL);
                    sb.Append(this.Title);
                    if (string.IsNullOrEmpty(this.SubTitle) == false)
                    {
                        sb.Append(" - ");
                        sb.Append(this.SubTitle);
                        sb.Append(NL);
                        sb.Append(new string('#', this.Title.Length + this.SubTitle.Length + 3));
                    }
                    else
                    {
                        sb.Append(NL);
                        sb.Append(new string('#', this.Title.Length));
                    }
                    

                    sb.Append(NL + NL);
                }
            }
            else
            {
                if (asMarkdown == false)
                { 
                    sb.Append(NL); 
                }
                else
                {
                    if (string.IsNullOrEmpty(this.SubTitle) == false)
                    {
                        sb.Append(NL);
                        sb.Append("## ");
                        sb.Append(this.SubTitle);
                        sb.Append(NL + NL);
                    }
                }
            }
            if (string.IsNullOrEmpty(this.Description) == false)
            {
                if (asMarkdown == true)
                {
                    sb.Append(this.Description);
                    sb.Append(NL + NL);
                    if (asHeader == false && string.IsNullOrEmpty(this.SubTitle) == false)
                    {
                        sb.Append("***");
                        sb.Append(NL + NL);
                    }
                }
                else
                {
                    ConcatLine(ref sb, "", this.Description, 0, maxLength, true);
                    if (asHeader == false)
                    {
                        sb.Append(NL);
                        sb.Append(new string('-', maxLength));
                        sb.Append(NL);
                    }
                }
            }
            if (asHeader == false)
            {
                if (asMarkdown == true)
                {
                    sb.Append(GetMarkdownTable(asTagDoc, "Value", "Description"));
                }
                else
                {
                    sb.Append(FormatLines(maxLength, asTagDoc));
                }
            }
            if (string.IsNullOrEmpty(this.Suffix) == false)
            {
                if (asMarkdown == true)
                {
                    sb.Append(NL);
                    sb.Append(this.Suffix);
                }
                else
                {
                    sb.Append(NL);
                    sb.Append(new string('-', maxLength));
                    sb.Append(NL);
                    ConcatLine(ref sb, "", this.Suffix, 0, maxLength, true);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Returns the tuples as markdown table
        /// </summary>
        /// <param name="asTag">If true, the line will be rendered as documentation for tags</param>
        /// <param name="leftHeader">Left header text</param>
        /// <param name="rightHeader">Right header text</param>
        /// <returns>Formatted markdown table</returns>
        private string GetMarkdownTable(bool asTag, string leftHeader, string rightHeader)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(leftHeader + " | " + rightHeader);
            sb.Append(NL);
            sb.Append("--- | ---");
            sb.Append(NL);
            string val, desc;
            foreach (T tuple in Tuples)
            {
                val = tuple.Value.Replace("|", "&#124;");
                desc = tuple.Description.Replace("|", "&#124;");
                if (asTag == true && tuple.OverrideTagFormatting == false)
                {
                    sb.Append("<" + val + ">");
                }
                else
                {
                    sb.Append(val);
                }
                sb.Append(" | ");
                sb.Append(desc);
                sb.Append(NL);
            }
            return sb.ToString();
        }


        /// <summary>
        /// Returns the a formatted string with line breaks
        /// </summary>
        /// <param name="maxlength">Length in numbers of characters (console width)</param>
        /// <param name="asTag">If true, the line will be rendered as documentation for tags</param>
        /// <returns>Formatted string</returns>
        public string FormatLines(int maxlength, bool asTag)
        {
            int split = GetSplitPosition();
            StringBuilder sb = new StringBuilder();
            string item;
            int len;
            foreach(T tuple in Tuples)
            {
                if (asTag == true && tuple.OverrideTagFormatting == false) 
                {
                    item = "<" + tuple.Value + ">";
                    len = split + 2;
                }
                else if (asTag == true && tuple.OverrideTagFormatting == true)
                {
                    item = tuple.Value;
                    len = split + 2;
                }
                else
                {
                    item = tuple.Value;
                    len = split;
                }
                ConcatLine(ref sb, item, tuple.Description, len, maxlength, false);
                sb.Append(NL + NL);
            }
            char[] trim = NL.ToCharArray();
            return sb.ToString().TrimEnd(trim);
        }

        /// <summary>
        /// Returns the number of characters where the text splits between value and description (over all tuples)
        /// </summary>
        /// <returns>Number of characters</returns>
        private int GetSplitPosition()
        {
            int l = 0;
            foreach(T item in Tuples)
            {
                if (item.Value.Length > l) { l = item.Value.Length; }
            }
            return l;
        }

        /// <summary>
        /// Concatenates lines according to windows widths an split values
        /// </summary>
        /// <param name="sb">StringBuilder as reference</param>
        /// <param name="item">Input value</param>
        /// <param name="description">Description text</param>
        /// <param name="split">Number of characters to splits</param>
        /// <param name="lenght">Window width in number of characters</param>
        /// <param name="noDelimitation">If true, no delimitation will be applied</param>
        private void ConcatLine(ref StringBuilder sb, string item, string description, int split, int lenght, bool noDelimitation)
        {
            sb.Append(item);
            string[] desc = description.Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);
            int maxDLength;
            int Dlength= 0;
            if (noDelimitation == false)
            {
                sb.Append(": ");
                sb.Append(new string(' ', split - item.Length));
                maxDLength = lenght - split - 2;
            }
            else
            {
                maxDLength = lenght - split;
            }
            foreach (string token in desc)
            {
                if (Dlength+ (token.Length + 1) > maxDLength)
                {

                    sb.Append(NL);
                    if (noDelimitation == false)
                    {
                        sb.Append(new string(' ', split + 2));
                    }
                    else
                    {
                        sb.Append(new string(' ', split));
                    }
                    Dlength= 0;
                }
                sb.Append(token);
                sb.Append(' ');
                Dlength+= (token.Length + 1);
            }
        }

        /// <summary>
        /// Subclass representing a general purpose tuple for the documentation
        /// </summary>
        public class T
        {
            /// <summary>
            /// Value of the tuple
            /// </summary>
            public string Value { get; set; }
            /// <summary>
            /// Description of the tuple
            /// </summary>
            public string Description { get; set; }
            /// <summary>
            /// If true, the tuple will be displayed with its raw value and not as XML tag
            /// </summary>
            public bool OverrideTagFormatting { get; set; }

            /// <summary>
            /// Default constructor
            /// </summary>
            public T()
            { }

            /// <summary>
            /// Constructor with parameters
            /// </summary>
            /// <param name="value">Value of the tuple</param>
            /// <param name="description">Description of the tuple</param>
            public T(string value, string description)
            {
                this.Value = value;
                this.Description = description;
            }

            /// <summary>
            /// Constructor with all parameters
            /// </summary>
            /// <param name="value">Value of the tuple</param>
            /// <param name="description">Description of the tuple</param>
            /// <param name="overrideTagFormatting">If true, the tuple will be displayed with its raw value and not as XML tag</param>
            public T (string value, string description, bool overrideTagFormatting): this(value, description)
            {
                this.OverrideTagFormatting = overrideTagFormatting;
            }
        }
    }
}
