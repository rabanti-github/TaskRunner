using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TaskRunner.SubTasks;

namespace TaskRunner
{
    /// <summary>
    /// Task Runner - (c) 2018 - Raphael Stoeckli
    /// This program and its code is released under the MIT license
    /// -----------------------------------------------------------
    /// Main class
    /// </summary>
    class Program
    {
        /// <summary>
        /// Main method
        /// </summary>
        /// <param name="args">Arguments like -r, -d, -o, h or -l</param>
        static void Main(string[] args)
        {
            //args = new string[] { "--run", "KillProcs.xml", "-o", "--log", "test.log" };
            //Parameter.RegisterSystemParameters();
            //Evaluation.ParseCondition("(22 == SYSTEM_TIME_START)", false);

             if (args.Length < 1)
             {
                 Console.WriteLine(Usage(false));
                 return;
             }

            Arguments a = new Arguments();
            Task t = null;
            Arguments.ArgType type = Arguments.ArgType.flag;

            for (int i = 0; i < args.Length; i++)
            {
                type = Arguments.CheckArgs(ref a, args[i], type);
                if (type == Arguments.ArgType.undefined)
                {
                    Console.WriteLine("Unknown flag or invalid value '" + args[i] + "'");
                    Console.WriteLine(Usage(false));
                    return;
                }
            }
            if (a.Help == true)
            {
                Console.WriteLine(Usage(false));
                return;
            }
            if (a.Docs == true)
            {
                Documentation();
                return;
            }
            if (a.Markdown == true)
            {
                Console.WriteLine(Usage(true));
                Console.WriteLine("Generating markdown files in current folder...");
                List<SubTask> types = Task.EnumerateSubTasks();
                foreach (SubTask subtask in types)
                {
                    subtask.SaveMarkdown(subtask.MarkdownFileName);
                }
                return;
            }
            if (a.Demo == true)
            {
                Console.WriteLine(Usage(true));
                Console.WriteLine("Generating demo files in current folder...");
                List<SubTask> types = Task.EnumerateSubTasks();
                foreach(SubTask subtask in types)
                {
                    Task.CreateDemoFile(subtask.DemoFileName, subtask.Type);
                }
                return;
            }
            if (a.Utilities == true)
            {
                Utilities();
                return;
            }
            if (a.ConfigFilePath == "" && a.Run == true)
            {
                Console.WriteLine("Error: No config file was defined");
                return;
            }
            if (a.LogFilePath == "" && a.Log == true)
            {
                Console.WriteLine("Error: No logfile path was defined");
                return;
            }
            if (a.Run == true)
            {
       
                Parameter.RegisterSystemParameters();
                //Parameter.UpdateSystemParameters(Parameter.SysParam.SYSTEM_TIME_START, DateTime.Now);
                if (a.Output == true)
                {
                    Console.WriteLine(Usage(true));
                }
                t = Task.Deserialize(a.ConfigFilePath);
                if (t == null) { return; }
                if (t.Valid == false) { return; }

                int iteration = 0;
                Task.Status status;
                while (true)
                {
                    if (a.DelayExecution == true && a.DelayAmount > 0 && (a.NoInitialDelay == false || (a.NoInitialDelay == true && iteration > 0)))
                    {
                        if (a.Output == true)
                        {
                            Console.WriteLine("Waiting for " + a.DelayAmount + " milliseconds to execute the task...");
                        }
                        Thread.Sleep(a.DelayAmount);
                    }
                    Parameter.UpdateSystemParameter(Parameter.SysParam.SYSTEM_TIME_START, DateTime.Now);
                    status = t.Run(a.HaltOnError, a.Output, a.Log, a.LogFilePath);
                    if (status == Task.Status.terminate)
                    {
                        if (a.Output == true && a.Iterative == true)
                        {
                            Console.WriteLine("The iteration of the task execution was interrupted due to an error");
                        }
                        break;
                    }

                    iteration++;
                    if (iteration >= a.NumberOfIterations && a.NumberOfIterations != 0)
                    {
                        break;
                    }
                }

                
            }
        }

        /// <summary>
        /// Method returns the header and usage of the program as text
        /// </summary>
        /// <param name="headerOnly">If true, only the header is returned, otherwise the usage is added after the header</param>
        /// <returns>Header and/or usage of the program as text</returns>
        private static string Usage(bool headerOnly)
        {
            string title = Output.GetSectionTitle("Task Runner - Run tasks, controlled by config files\n(c) 2018 - Raphael Stoeckli\nhttps://github.com/rabanti-github/TaskRunner");
            string disclaimer = Output.GetSectionTitle("DISCLAIMER: USE THIS SOFTWARE AT YOUR OWN RISK.\nTHE AUTHOR(S) OF THIS SOFTWARE IS/ARE NOT LIABLE FOR\nANY DAMAGE OR OTHER NEGATIVE EFFECTS ARISING FROM\nTHE USAGE OF TASKRUNNER. --> License: MIT", true);
            string header = title + "\n"+ disclaimer;

            Output o = new Output(Console.WindowWidth - 1, "Usage");
            o.AddLine("Task execution:");
            o.AddLine("TaskRunner.exe -r [path to configuration] <options>");
            o.AddLine("Task execution with log entry:");
            o.AddLine("TaskRunner.exe -r [path to configuration] -l [path to log file] <options>");
            o.AddLine("Generation of example files of the configuration:");
            o.AddLine("TaskRunner.exe -d");
            o.AddLine("Generation of markdown files of the documentation:");
            o.AddLine("TaskRunner.exe -m");
            o.AddLine("Iteration example (10 times, delay of 10 seconds):");
            o.AddLine("Taskrunner.exe -r iterativeTask.xml -w 10000 -i 10 -n <options>");
            o.AddLine("Wizard to start common Windows system utilities:");
            o.AddLine("Taskrunner.exe -u");
            o.AddLine("\nPath to configuration: A relative or absolute path to the configuration as XML file");
            o.AddLine("Path to log file: A relative or absolute path to the log as text file");
            o.Flush("\n\n");

            o.Title = "Flags / Options";
            o.AddTuple("-r | --run", "Runs a task defined in the subsequent config file (path)");
            o.AddTuple("-i | --iterate", "Iterates a task the number of times defined by the following number. If 0 (zero), the number is infinite");
            o.AddTuple("-w | --wait", "Waits with the execution of a task for the following number of millisecond. Useful in combination with -i");
            o.AddTuple("-n | --nodelay", "Executes the first task after the start of TaskRunner without a defined delay(-w | --wait)");
            o.AddTuple("-e | --example", "Runs the demo command and generates example configurations in the current selected folder");
            o.AddTuple("-o | --output", "Enables the output mode. The results of the task will be displayed in the command shell");
            o.AddTuple("-s | --stop", "The task runner stops after an error, otherwise all sub-tasks are executed until the end of the configuration");
            o.AddTuple("-l | --log", "Enables logging. After the flag a valid path (absolute or relative) to a logfile must be defined");
            o.AddTuple("-h | --help", "Shows the general program help (this text)");
            o.AddTuple("-d | --docs", "Shows the menu to get the task documentation");
            o.AddTuple("-m | --markdown", "Saves the documentation of all task types to markdown files in the current folder");
            o.AddTuple("-u | --utils", "Shows the menu with several Windows utility programs (can be started there)");
            o.AddTuple("-p | --param", "Stores a temporary variable while runtime. The variable Can be used by the Tasks");
            o.Flush("\n\n");

            o.Title = "Parameter Handling";
            o.Description = "Syntax: -p|--param:<data type>:<param name>:<param value>\nThe parameter is only valid during the execution of the loaded task. The parameter flag contains 3 or 4 parts, delimited by colons: ";          
            o.AddTuple("- 1", "Flag Identifier (-p or --param)");
            o.AddTuple("- 2", "(Optional) Data Type. Valid values are 's' for string, 'b' for boolean and 'n' for number(double). If this part is omitted, the value will be handled as string");
            o.AddTuple("- 3", "Parameter Name (unique string [a-Z, 0-9, underscore] without spaces or colons)");
            o.AddTuple("- 4", "Parameter Value (The value will be parsed to boolean or double in case of the data types 'b' or 'n'). Note: Very small or big numbers can cause arithmetic errors. Use MIN or MAX to define the minimum or maximum value of a double number (+/-1.797~e+308)");
            o.Flush("\n\n");

            //o = new Output(Console.WindowWidth - 1);
            o.Description = "Some parameter names, starting with 'SYSTEM_', 'TASK_' and 'SUBTASK_' are read-only variables, provided by task runner. Variable names, starting with 'ENV_' are environment variables of task runner an can be overwritten. See documentation.\nExamples:";
            o.AddLine("-p:n:NUMBER_OF_FILES:8");
            o.AddLine("-p:n:LIMIT_NUMBER:MAX <- represents the highest possible number of a double value");
            o.AddLine("--param:b:MATCH:true");
            o.AddLine("--param:NAME:machine1");
            o.AddLine("-p:s:NAME:\"Name with spaces\"");
            o.AddLine("--param:COMMENT:'Other quotes are also OK'");
            o.AddLine("-p:n:ENV_MAX_TASK_ITERATIONS:5  <- overrides environment variable");
            o.Flush("\n\n");

            o.Title = "Task Documentation";
            o.Description = "Use the flag -d / --docs for the documentation.\nUse the flag -m / --markdown to save the documentation as markdown files\nPlease have also a look at the demo files for a practical implementation of parameters, using the the flag -e / --example (to generate the files).\nAvailable documentation:";
            o.AddLine("- Description");
            o.AddLine("- Tag documentation (of xml configuration)");
            o.AddLine("- Documentation of Tag-Attributes (of xml configuration)");
            o.AddLine("- Status codes");

            string usage = o.PrintAll();

            if (headerOnly == true)
            {
                return header;
            }
            else
            {
                return header + "\n" + usage;
            }
        }

        /// <summary>
        /// Method to handle he output of the documentation
        /// </summary>
        private static void Documentation()
        {
            List<SubTask> types = Task.EnumerateSubTasks();
            int len = types.Count;
            string input, text;
            int number, number2;
            bool exit;
            Console.Clear();
            string title = Output.GetSectionTitle("Task Runner - Run tasks, controlled by config files\n(c) 2018 - Raphael Stoeckli\nhttps://github.com/rabanti-github/TaskRunner");
            Console.WriteLine(title);
            bool firstTurn = true;
            while (true)
            {
                if (firstTurn == false) {Console.Clear();}
                else { firstTurn = false; }
                exit = false;
                Console.WriteLine(Output.GetSectionTitle("D O C U M E N T A T I O N"));
                Output oTop = new Output(Console.WindowWidth - 1, "Task Selection", null, "Please insert a number between 1 and " + len.ToString() + " or X to exit...");
                for (int i = 0; i < len; i++)
                {
                    oTop.AddTuple("[" + (i + 1).ToString() + "]", types[i].GetDocumentationDescription().Title);
                    //Console.WriteLine("[" + (i + 1).ToString() + "] " + types[i].GetDocumentationDescription().Title);
                }
                oTop.AddTuple("[x]", "Exit");
                Console.WriteLine(oTop.Print());
                input = Console.ReadLine();
                if (input.ToLower() == "x") { break; }
                if (int.TryParse(input, out number) == false)
                {
                    Console.WriteLine("Invalid input. Please retry...");
                    continue;
                }
                if (number < 1 || number > len)
                {
                    Console.WriteLine("Invalid input. Please retry...");
                    continue;
                }
                while (true)
                {
                    Console.Clear();
                    Output o = new Output(Console.WindowWidth -1, "Documentation Menu", types[number].GetDocumentationDescription().Title, "Please insert a number between 1 and 5, M for main menu or X to exit...");
                    o.AddTuple("[1]", "All documentation");
                    o.AddTuple("[2]", "Description");
                    o.AddTuple("[3]", "Tag documentation");
                    o.AddTuple("[4]", "Tag-Attribute documentation");
                    o.AddTuple("[5]", "Status codes");
                    o.AddTuple("[m]", "Main menu");
                    o.AddTuple("[x]", "Exit");
                    Console.WriteLine(o.Print());
                    input = Console.ReadLine();
                    if (input.ToLower() == "x") { exit = true;  break; }
                    if (input.ToLower() == "m") { exit = false; break; }
                    if (int.TryParse(input, out number2) == false)
                    {
                        Console.WriteLine("Invalid input. Please retry...");
                        continue;
                    }
                    if (number2 < 1 || number2 > 5)
                    {
                        Console.WriteLine("Invalid input. Please retry...");
                        continue;
                    }
                    Console.Clear();
                    if (number2 == 2 || number2 == 1)
                    {
                        text = types[number - 1].GetDocumentation(SubTask.DocumentationType.Description, Console.WindowWidth -1);
                        Console.WriteLine(text + "\n");
                    }
                    if (number2 == 3 || number2 == 1)
                    {
                        text = types[number - 1].GetDocumentation(SubTask.DocumentationType.Tags, Console.WindowWidth -1);
                        Console.WriteLine(text + "\n");
                    }
                    if (number2 == 4 || number2 == 1)
                    {
                        text = types[number - 1].GetDocumentation(SubTask.DocumentationType.Attributes, Console.WindowWidth -1);
                        Console.WriteLine(text + "\n");
                    }
                    if (number2 == 5 || number2 == 1)
                    {
                        text = types[number - 1].GetDocumentation(SubTask.DocumentationType.StatusCodes, Console.WindowWidth -1);
                        Console.WriteLine(text + "\n");
                    }
                    Console.WriteLine("\nPress any key to continue...");
                    input = Console.ReadLine();
                }
                if (exit == true) { break; }
            }

        }

        private static void Utilities()
        {
            bool exit;
            Console.Clear();
            string title = Output.GetSectionTitle("Task Runner - Run tasks, controlled by config files\n(c) 2018 - Raphael Stoeckli\nhttps://github.com/rabanti-github/TaskRunner");
            Console.WriteLine(title);
            bool firstTurn = true;
            while (true)
            {
                if (firstTurn == false) { Console.Clear(); }
                else { firstTurn = false; }
                string input;
                int number;
                SysUtils s = new SysUtils();
                int len = s.Utilities.Count;
                exit = false;
                Console.WriteLine(Output.GetSectionTitle("U T I L I T I E S"));
                Output o = new Output(Console.WindowWidth -1, "Menu");
                o.Description = "Administrative privileges may be necessary to start these utilities. The UAC or a password prompt may pop up.\nPlease insert a number between 1 and " + len.ToString() + " or X to exit...";
                for (int i = 0; i < len; i++)
                {
                    o.AddTuple("[" + (i + 1).ToString() + "]", s.Utilities[i].Name);
                }
                o.AddTuple("[x]", "Exit");
                Console.WriteLine(o.Print());

                input = Console.ReadLine();
                if (input.ToLower() == "x") { break; }
                if (int.TryParse(input, out number) == false)
                {
                    Console.WriteLine("Invalid input. Please retry...");
                    continue;
                }
                if (number < 1 || number > len)
                {
                    Console.WriteLine("Invalid input. Please retry...");
                    continue;
                }
                number--;
                o.ClearAll();
                o.Title = "Starting utility: " + s.Utilities[number].Name;
                o.Description = s.Utilities[number].Description;
                o.AddLine("The application will be started now...");
                o.AddLine("System call: " + s.Utilities[number].Command);
                if (string.IsNullOrEmpty(s.Utilities[number].Arguments) == false)
                {
                    o.AddLine("Arguments: " + s.Utilities[number].Arguments);
                }
                Console.WriteLine(o.Print());
                s.Utilities[number].Run();
                o.ClearAll();
                o.AddLine(s.Utilities[number].Name + " should now be running");
                o.AddLine("Please check your privileges or the availability of the application if " + s.Utilities[number].Name + " was not started.");
                o.AddLine("Press any key to continue...");
                Console.WriteLine(o.Print());
                input = Console.ReadLine();
            }
        }
    }
}
