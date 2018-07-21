using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TaskRunner
{
    public class Output
    {
        private const int CONSOLE_DEFAULT_WIDTH = 240;

        /// <summary>
        /// Globally used new line sequence
        /// </summary>
        public static string NL = System.Environment.NewLine;

        private List<Tuple<Output,string>> metaOutput;

        public string Title { get; set; }
        public string SubTitle { get; set; }

        public string Description { get; set; }

        public List<T> Tuples { get; set; }

        public int Width { get; set; }

        public Output(int width = 0, string title = null, string subTitle = null, string descriptionText = "")
        {
            if (width == 0)
            {
                if (Console.IsOutputRedirected == true)
                {
                    this.Width = CONSOLE_DEFAULT_WIDTH;
                }
                else
                {
                    this.Width = Console.WindowWidth;
                }
            }
            else
            {
                this.Width = width;
            }
            this.Title = title;
            this.SubTitle = subTitle;
            this.Description = descriptionText;
            this.Tuples = new List<T>();
            this.metaOutput = new List<Tuple<Output, string>>();
        }

        /// <summary>
        /// Internal copy constructor
        /// </summary>
        /// <param name="copyFrom">The object to copy</param>
        private Output(Output copyFrom) : this(copyFrom.Width)
        {
            this.Title = copyFrom.Title;
            this.Description = copyFrom.Description;
            this.SubTitle = copyFrom.SubTitle;
            foreach (T tuple in copyFrom.Tuples)
            {
                this.Tuples.Add(tuple);
            }
            this.Width = copyFrom.Width;
        }


        public void AddLine(string line)
        {
            this.Tuples.Add(new T("", line));
        }

        public void AddTuple(string value, string description, bool overrideTagFormatting = false)
        {
            this.Tuples.Add(new T(value, description, overrideTagFormatting));
        }

        public string Print(bool isHeaderSection = false)
        {
            bool skipTitle = string.IsNullOrEmpty(this.Title);
            return Output.GetOutput(this.Width, false, isHeaderSection, false, skipTitle, this.Description, this.Title, this.SubTitle, null, this.Tuples);
        }

        public string PrintAll()
        {
            Tuple<Output, string> lastTuple = new Tuple<Output, string>(new Output(this), "");
            StringBuilder sb = new StringBuilder();
            foreach (Tuple<Output, string> tuple in metaOutput)
            {
                sb.Append(tuple.Item1.Print());
                if (string.IsNullOrEmpty(tuple.Item2))
                {
                    sb.Append(Output.NL);
                }
                else
                {
                    sb.Append(tuple.Item2);
                }
            }
            sb.Append(lastTuple.Item1.Print());
            return sb.ToString();
        }

        public void Flush(string append = "")
        {
            metaOutput.Add(new Tuple<Output, string>(new Output(this), append));
            this.Description = null;
            this.SubTitle = null;
            this.Title = null;
            this.Tuples.Clear();
        }

        public void ClearAll()
        {
            this.metaOutput.Clear();
            this.Description = null;
            this.SubTitle = null;
            this.Title = null;
            this.Tuples.Clear();
        }

        public static string GetSectionTitle(string sectionTitle, bool boldLine = false)
        {
            string[] split = sectionTitle.Split('\n');
            List<T> lines = new List<T>();
            foreach (string s in split)
            {
                lines.Add(new T(s, ""));
            }

            int width = GetSplitPosition(lines, false);
            if (boldLine == true)
            {
                return GetSectionTitle(lines, width + 2, '╔', '╗', '╚', '╝', '║', '═');
            }
            else
            {
                return GetSectionTitle(lines, width + 2, '┌', '┐', '└', '┘', '│', '─');
            }
            
        }

        private static string GetSectionTitle(List<T> lines, int width, char ul, char ur, char bl, char br, char v, char h)
        {
            StringBuilder sb = new StringBuilder();
            string line;
            sb.Append(h, width);
            line = sb.ToString();
            sb.Clear();
            sb.Append(ul);
            sb.Append(line);
            sb.Append(ur+ NL);
            foreach (T l in lines)
            {
                sb.Append(v + " ");
                sb.Append(l.Value);
                sb.Append(new string(' ', width - l.Value.Length - 2));
                sb.Append(" " + v + NL);
            }
            sb.Append(bl);
            sb.Append(line);
            sb.Append(br);
            return sb.ToString();
        }

        public static string GetDefaultOutput(int width, bool isHeaderSection, string text, string title = null, string subTitle = null)
        {
            bool skipTitle = string.IsNullOrEmpty(title);
            return GetOutput(width, false, isHeaderSection, false, skipTitle, text, title, subTitle);
        }

        /// <summary>
        /// Returns the documentation object as string
        /// </summary>
        /// <param name="maxLength">Length in numbers of characters (console width)</param>
        /// <param name="isTagDocSection">If true, the documentation will be rendered as documentation for tags</param>
        /// <param name="isHeaderSection">If true, the header will be rendered</param>
        /// <param name="formatAsMarkdown">If true, the documentation will be rendered as markdown, The length parameter will be skipped</param>
        /// <param name="skipTitle">If true, no Title will be rendered</param>
        /// <returns></returns>
        public static string GetOutput(int maxLength, bool isTagDocSection, bool isHeaderSection, bool formatAsMarkdown, bool skipTitle, string text, string title = null, string subTitle = null, string suffix = null, List<T> tuples = null)
        {
            StringBuilder sb = new StringBuilder();
            if (string.IsNullOrEmpty(title) == false)
            {
                if (formatAsMarkdown == true)
                {
                    if (skipTitle == false)
                    {
                        sb.Append("# ");
                        sb.Append(title);
                        sb.Append(NL);
                    }
                    if (string.IsNullOrEmpty(subTitle) == false)
                    {
                        sb.Append(NL);
                        sb.Append("## ");
                        sb.Append(subTitle);
                        sb.Append(NL);
                    }
                    sb.Append(NL);
                }
                else if (skipTitle == false)
                {
                        //sb.Append(NL);
                        sb.Append(title);
                        if (string.IsNullOrEmpty(subTitle) == false)
                        {
                            sb.Append(" - ");
                            sb.Append(subTitle);
                            sb.Append(NL);
                            sb.Append(new string('═', title.Length + subTitle.Length + 3));
                        }
                        else
                        {
                            sb.Append(NL);
                            sb.Append(new string('═', title.Length));
                        }

                        sb.Append(NL + NL);
                }
            }
            else
            {
                if (formatAsMarkdown == false && skipTitle == false)
                {
                    sb.Append(NL);
                }
                else
                {
                    if (string.IsNullOrEmpty(subTitle) == false)
                    {
                        sb.Append(NL);
                        sb.Append("## ");
                        sb.Append(subTitle);
                        sb.Append(NL + NL);
                    }
                }
            }
            if (string.IsNullOrEmpty(text) == false)
            {
                if (formatAsMarkdown == true)
                {
                    sb.Append(text);
                    sb.Append(NL + NL);
                    if (isHeaderSection == false)
                    {
                        if (string.IsNullOrEmpty(subTitle) == false)
                        {
                            sb.Append("***");
                            sb.Append(NL + NL);
                        }
                    }
                }
                else
                {
                    Output.ConcatLine(ref sb, "", text, 0, maxLength, true);
                    if (isHeaderSection == false)
                    {
                        sb.Append(NL);
                        sb.Append(new string('─', maxLength));
                        sb.Append(NL);
                    }
                }
            }
            if (isHeaderSection == false && tuples != null && tuples.Count > 0)
            {
                if (formatAsMarkdown == true)
                {
                    sb.Append(NL);
                    sb.Append(Output.GetMarkdownTable(isTagDocSection, "Value", "Description", tuples));
                }
                else
                {
                    sb.Append(Output.FormatLines(maxLength, isTagDocSection, tuples));
                }
            }
            if (string.IsNullOrEmpty(suffix) == false)
            {
                if (formatAsMarkdown == true)
                {
                    sb.Append(NL);
                    sb.Append(suffix);
                }
                else
                {
                    sb.Append(NL);
                    sb.Append(new string('─', maxLength));
                    sb.Append(NL);
                    ConcatLine(ref sb, "", suffix, 0, maxLength, true);
                }
            }
            return sb.ToString();
        }


        /// <summary>
        /// Returns the a formatted string with line breaks
        /// </summary>
        /// <param name="maxlength">Length in numbers of characters (console width)</param>
        /// <param name="asTag">If true, the line will be rendered as documentation for tags</param>
        /// <returns>Formatted string</returns>
        private static string FormatLines(int maxlength, bool asTag, List<T> tuples)
        {
            int split = Output.GetSplitPosition(tuples, asTag);
            StringBuilder sb = new StringBuilder();
            string item;
            foreach (T tuple in tuples)
            {
                if (asTag == true && tuple.OverrideTagFormatting == false)
                {
                    item = "<" + tuple.Value + ">";
                }
                else if (asTag == true && tuple.OverrideTagFormatting == true)
                {
                    item = tuple.Value;
                }
                else
                {
                    item = tuple.Value;
                }
                ConcatLine(ref sb, item, tuple.Description, split, maxlength, false);
                sb.Append(NL);
            }
            char[] trim = NL.ToCharArray();
            return sb.ToString().TrimEnd(trim);
        }

        /// <summary>
        /// Returns the number of characters where the text splits between value and description (over all tuples)
        /// </summary>
        /// <returns>Number of characters</returns>
        private static int GetSplitPosition(List<T> tuples, bool asTag)
        {
            int l = 0;
            foreach (T item in tuples)
            {
                if (item.OverrideTagFormatting == true || asTag == false)
                {
                    if (item.Value.Length > l) { l = item.Value.Length; }
                }
                else // as tag
                {
                    if ((item.Value.Length + 2) > l) { l = item.Value.Length + 2; }
                }
            }
            return l;
        }


        /// <summary>
        /// Concatenates lines according to window widths and split values
        /// </summary>
        /// <param name="sb">StringBuilder as reference</param>
        /// <param name="item">Input value</param>
        /// <param name="description">Description text</param>
        /// <param name="split">Number of characters to splits</param>
        /// <param name="lenght">Window width in number of characters</param>
        /// <param name="noDelimitation">If true, no delimitation will be applied</param>
        private static void ConcatLine(ref StringBuilder sb, string item, string description, int split, int lenght, bool noDelimitation)
        {
            sb.Append(item);
            string[] desc = description.Split(new char[] { ' ' }, StringSplitOptions.None);
            int maxDLength;
            int Dlength = 0;
            if (noDelimitation == false)
            {
                if (string.IsNullOrEmpty(item) == false)
                {
                    sb.Append(": ");
                    sb.Append(new string(' ', split - item.Length));
                }
                maxDLength = lenght - split - 2;
            }
            else
            {
                maxDLength = lenght - split;
            }

            for (int i = 0; i < desc.Length; i++)
            {
                string token = desc[i];
                string word;
                if (token.Contains("\n"))
                {
                    string[] split2 = token.Split('\n');
                    for (int j = 0; j < split2.Length - 1; j++)
                    {
                        if (Dlength + (split2[j].Length + 1) > maxDLength)
                        {
                            AddPadding(ref sb, noDelimitation, split);
                        }
                        sb.Append(split2[j]);
                        AddPadding(ref sb, noDelimitation, split);
                        Dlength = 0;
                    }

                    word = split2[split2.Length - 1];
                    Dlength = word.Length + 1;
                }
                else
                {
                    word = token;
                }

                if (Dlength + (word.Length + 1) > maxDLength)
                {
                    AddPadding(ref sb, noDelimitation, split);
                    Dlength = 0;
                }
                sb.Append(word);
                if (i < desc.Length - 1)
                {
                    sb.Append(' ');
                    Dlength += (word.Length + 1);
                }
                else
                {
                    Dlength += word.Length;
                }
                
            }
        }

        private static void AddPadding(ref StringBuilder sb, bool noDelimitation, int split)
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
        }

        /// <summary>
        /// Returns the tuples as markdown table
        /// </summary>
        /// <param name="asTag">If true, the line will be rendered as documentation for tags</param>
        /// <param name="leftHeader">Left header text</param>
        /// <param name="rightHeader">Right header text</param>
        /// <returns>Formatted markdown table</returns>
        private static string GetMarkdownTable(bool asTag, string leftHeader, string rightHeader, List<T> tuples)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(leftHeader + " | " + rightHeader);
            sb.Append(NL);
            sb.Append("--- | ---");
            sb.Append(NL);
            string val, desc;
            foreach (T tuple in tuples)
            {
                val = tuple.Value.Replace("|", "&#124;");
                val = val.Replace("<", "&lt;");
                val = val.Replace(">", "&gt;");
                desc = tuple.Description.Replace("|", "&#124;");
                desc = desc.Replace("<", "&lt;");
                desc = desc.Replace(">", "&gt;");
                if (tuple.OverrideTagFormatting == false && asTag == true)
                {
                    sb.Append("&lt;" + val + "&gt;");
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
        /// Subclass representing a general purpose tuple for the output
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
                if (value == null)
                {
                    this.Value = string.Empty;
                }
                else
                {
                    this.Value = value;
                }
                if (description == null)
                {
                    this.Description = string.Empty;
                }
                else
                {
                    this.Description = description;
                }
            }

            /// <summary>
            /// Constructor with all parameters
            /// </summary>
            /// <param name="value">Value of the tuple</param>
            /// <param name="description">Description of the tuple</param>
            /// <param name="overrideTagFormatting">If true, the tuple will be displayed with its raw value and not as XML tag</param>
            public T(string value, string description, bool overrideTagFormatting) : this(value, description)
            {
                this.OverrideTagFormatting = overrideTagFormatting;
            }
        }

    }
}
