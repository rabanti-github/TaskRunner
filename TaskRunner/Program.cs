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
            string header = @"
Task Runner - Run tasks controlled by config files
(c) 2018 - Raphael Stoeckli
https://github.com/rabanti-github/TaskRunner
--------------------------------------------------
DISCLAIMER: USE THIS SOFTWARE AT YOUR OWN RISK.
THE AUTHOR(S) OF THIS SOFTWARE IS/ARE NOT LIABLE FOR
ANY DAMAGE OR OTHER NEGATIVE EFFECTS ARISING FROM
THE USAGE OF TASKRUNNER. --> License: MIT 
--------------------------------------------------
";
            string usage =
            @"Normal Usage:
TaskRunner.exe -r [path to configuration] <options>
Generation of example files of the configuration:
TaskRunner.exe -d
Generation of markdown files of the documentation:
TaskRunner.exe -m
Iteration example (10 times, delay of 10 seconds):
Taskrunner.exe -r iterativeTask.xml -w 10000 -i 10 -n

Path to the configuration: A relative or absolute path to the
configuration as XML file

Flags / Options
---------------
-r | --run:      Runs a task defined in the subsequent config file (path)
-i | --iterate:  Iterates a task the number of times defined by the
                 following number. If 0 (zero), the number is infinite
-w | --wait:     Waits with the execution of a task for the following
                 number of millisecond. Useful in combination with -i
-n | --nodelay:  Executes the first task after the start of TaskRunner
                 without a defined delay (-w | --wait)
-e | --example:  Runs the demo command and generates example
                 configurations in the current selected folder
-o | --output:   Enables the output mode. The results of the task
                 will be displayed in the command shell
-s | --stop:     The task runner stops after an error, otherwise all
                 sub-tasks are executed until the end of the configuration
-l | --log:      Enables logging. After the flag a valid path
                 (absolute or relative) to a logfile must be defined
-h | --help:     Shows the program help (this text) 
-d | --docs:     Shows the menu with the task documentation
-u | --utils:    Shows the menu with several Windows utility programs
-m | --markdown: Saves the documentation of all task types to markdown
                 files in the current folder
-p | --param     Stores a temporary variable while runtime. The variable
                 Can be used by the Tasks

Parameter Handling
------------------
Syntax: -p|--param:<data type>:<param name>:<param value>
The flag -p or --param delivers a temporary parameter to the TaskRunner.
The parameter is only valid during the execution of the loaded task. The
parameter flag contains 3 or 4 parts, delimited by colons:
- 1: Flag Identifier (-p or --param)
- 2: (Optional) Data Type. Valid values are 's' for string, 'b' for boolean
     and 'n' for number (double). If this part is omitted, the value will
     be handled as string
- 3: Parameter Name (unique string, without spaces or colons)
- 4: Parameter Value (The value will be parsed to boolean or double in case
     of the data types 'b' or 'n')

Examples:
-p:n:NUMBER_OF_FILES:8
--param:b:MATCH:true
--param:NAME:machine1
-p:s:NAME:""Name with spaces""
--param:COMMENT:'Other quotes are also OK'

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
            List<SubTask> types = Task.EnumerateSubTasks();
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
            while (true)
            {
                string input;
                int number;
                SysUtils s = new SysUtils();
                int len = s.Utilities.Count;
                exit = false;
                Console.WriteLine("\n #####################");
                Console.WriteLine(" # U T I L I T I E S #");
                Console.WriteLine(" #####################");
                for (int i = 0; i < len; i++)
                {
                    Console.WriteLine("[" + (i + 1).ToString() + "] " + s.Utilities[i].Name);
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
                number--;
                Console.WriteLine("Starting utility: " + s.Utilities[number].Name + "...");
                Console.WriteLine("-----------------");
                Console.WriteLine(s.Utilities[number].Description);
                s.Utilities[number].Run();

                Console.WriteLine("\nPress any key to continue...");
                input = Console.ReadLine();
            }
        }
    }
}
