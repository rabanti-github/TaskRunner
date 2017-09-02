using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TaskRunner.SubTasks;

namespace TaskRunner
{
    /// <summary>
    /// Task Runner - (c) 2017 - Raphael Stoeckli
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
             if (args.Length < 1)
             {
                 Console.WriteLine(Usage(false));
                 return;
             }

            ArgsTuple a = new ArgsTuple();
            Task t = null;
            ArgsTuple.ArgType type = ArgsTuple.ArgType.flag;

            for (int i = 0; i < args.Length; i++)
            {
                type = CheckArgs(ref a, args[i], type);
                if (type == ArgsTuple.ArgType.undefined)
                {
                    Console.WriteLine("Unknown flag '" + args[i] + "'");
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
                List<SubTask> types = Task.EnumerateTaskTypes();
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
                List<SubTask> types = Task.EnumerateTaskTypes();
                foreach(SubTask subtask in types)
                {
                    Task.CreateDemoFile(subtask.DemoFileName, subtask.Type);
                }
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
                if (a.Output == true)
                {
                    Console.WriteLine(Usage(true));
                }
                t = Task.Deserialize(a.ConfigFilePath);
                if (t.Valid == false) { return; }
                t.Run(a.HaltOnError, a.Output, a.Log);
                if (a.Log == true)
                {
                    t.Log(a.LogFilePath);
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
            string header = @"
Task Runner - Run tasks controlled by config files
(c) 2017 - Raphael Stoeckli
https://github.com/rabanti-github/TaskRunner
--------------------------------------------------
";
            string usage =
            @"Normal Usage:
TaskRunner.exe -r [path to configuration] <options>
Generation of example files of the configuration:
TaskRunner.exe -d
Generation of markdown files of the documentation:
TaskRunner.exe -m

Path to the configuration: A relative or absolute path to the
configuration as XML file

Flags / Options
---------------
-r | --run:     Runs a task defined in the subsequent config file (path)
-e | --example: Runs the demo command and generates example
                configurations in the current selected folder
-o | --output:  Enables the output mode. The results of the task
                will be displayed in the command shell
-s | --stop:    The task runner stops after an error, otherwise all
                sub-tasks are executed until the end of the configuration
-l | --log:     Enables logging. After the flag a valid path
                (absolute or relative) to a logfile must be defined
-h | --help:    Shows the program help (this text) 
-d | --docs     Shows the menu with the task documentation
-m | --markdown Saves the documentation of all task types to markdown
                files in the current folder

Task Documentation
------------------
Please look at the demo files for a practical implementation of parameters.
Use the flag -e / --example to generate the demo files.
Use the flag -d / --docs for the documentation.
Use the flag -m / --markdown to save the documentation as markdown files
Available documentation:
- Description
- Documentation of Tags
- Documentation of Tag-Attributes
- Status codes
            ";
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
            List<SubTask> types = Task.EnumerateTaskTypes();
            int len = types.Count;
            string input, text;
            int number, number2;
            bool exit;
            while (true)
            {
                exit = false;
                Console.WriteLine("\n #############");
                Console.WriteLine(" # T A S K S #");
                Console.WriteLine(" #############");
                for (int i = 0; i < len; i++)
                {
                    Console.WriteLine("[" + (i + 1).ToString() + "] " + types[i].GetDocumentationDescription().Title);
                }
                Console.WriteLine("[x] Exit");
                Console.WriteLine("\nPlease select a number between 1 and " + len.ToString() + " or X to exit...");
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
                    Console.WriteLine("\nDocumentation Menu:\n-------------------\n[1] All documentation\n[2] Description\n[3] Tag documentation\n[4] Tag-Attribute documentation\n[5] Status codes\n[m] Main menu\n[x] Exit\n\nPlease select a number between 1 and 5, M for main menu or X to exit...");
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
                    if (number2 == 2 || number2 == 1)
                    {
                        text = types[number - 1].GetDocumentation(SubTask.DocumentationType.Description, Console.WindowWidth);
                        Console.WriteLine(text + "\n");
                    }
                    if (number2 == 3 || number2 == 1)
                    {
                        text = types[number - 1].GetDocumentation(SubTask.DocumentationType.Tags, Console.WindowWidth);
                        Console.WriteLine(text + "\n");
                    }
                    if (number2 == 4 || number2 == 1)
                    {
                        text = types[number - 1].GetDocumentation(SubTask.DocumentationType.Attributes, Console.WindowWidth);
                        Console.WriteLine(text + "\n");
                    }
                    if (number2 == 5 || number2 == 1)
                    {
                        text = types[number - 1].GetDocumentation(SubTask.DocumentationType.StatusCodes, Console.WindowWidth);
                        Console.WriteLine(text + "\n");
                    }
                    Console.WriteLine("\nPress any key to continue...");
                    input = Console.ReadLine();
                }
                if (exit == true) { break; }
            }

        }

        /// <summary>
        /// Method to check the passed arguments
        /// </summary>
        /// <param name="tuple">Argument tuple as reference</param>
        /// <param name="argValue">Passed argument value</param>
        /// <param name="argType">The expected type of the argument</param>
        /// <returns>The expected type of the next argument. In case of -r|--run and -l|--log, this is configFile or logFile</returns>
        private static ArgsTuple.ArgType CheckArgs(ref ArgsTuple tuple, string argValue, ArgsTuple.ArgType argType)
        {
            string arg = argValue;
            ArgsTuple.ArgType nextArgIs = ArgsTuple.ArgType.flag;
            if (argType == ArgsTuple.ArgType.flag)
            {

                arg = arg.ToLower();
                if (arg == "--run" || arg == "-r")
                {
                    tuple.Run = true;
                    nextArgIs = ArgsTuple.ArgType.configFile;
                }
                else if (arg == "--help" || arg == "-h")
                {
                    tuple.Help = true;
                    nextArgIs = ArgsTuple.ArgType.flag;
                }
                else if (arg == "--output" || arg == "-o")
                {
                    tuple.Output = true;
                    nextArgIs = ArgsTuple.ArgType.flag;
                }
                else if (arg == "--stop" || arg == "-s")
                {
                    tuple.HaltOnError = true;
                    nextArgIs = ArgsTuple.ArgType.flag;
                }
                else if (arg == "--log" || arg == "-l")
                {
                    tuple.Log = true;
                    nextArgIs = ArgsTuple.ArgType.logFile;
                }
                else if (arg == "--example" || arg == "-e")
                {
                    tuple.Demo = true;
                    nextArgIs = ArgsTuple.ArgType.flag;
                }
                else if (arg == "--docs" || arg == "-d")
                {
                    tuple.Docs = true;
                    nextArgIs = ArgsTuple.ArgType.flag;
                }
                else if (arg == "--markdown" || arg == "-m")
                {
                    tuple.Markdown = true;
                    nextArgIs = ArgsTuple.ArgType.flag;
                }
                else
                {
                    nextArgIs = ArgsTuple.ArgType.undefined;
                }
            }
            else if (argType == ArgsTuple.ArgType.configFile)
            {
                tuple.ConfigFilePath = arg;
                nextArgIs = ArgsTuple.ArgType.flag;
            }
            else if (argType == ArgsTuple.ArgType.logFile)
            {
                tuple.LogFilePath = arg;
                nextArgIs = ArgsTuple.ArgType.flag;
            }
            else
            {
                nextArgIs = ArgsTuple.ArgType.undefined;
            }
            return nextArgIs;
        }

    }
}
