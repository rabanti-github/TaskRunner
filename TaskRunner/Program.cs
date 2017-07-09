using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TaskRunner
{
    /// <summary>
    /// Task Runner - (c)2016 - Raphael Stoeckli
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
            }
            // DEBUG
            /*
             args = new string[5];
             args[0] = "--run";
             args[1] = "startProgram_test.xml";
             args[2] = "-o";
             args[3] = "-l";
             args[4] = "test.txt";
             */

            ArgsTuple a = new ArgsTuple();
            Task t = null;
            ArgsTuple.ArgType type = ArgsTuple.ArgType.flag;

            for (int i = 0; i < args.Length; i++)
            {
                type = CheckArgs(ref a, args[i], type);
            }

            if (a.Demo == true)
            {
                Console.WriteLine(Usage(true));
                Console.WriteLine("Generating demo files in program folder...");
                Task.CreateDemoFile("DEMO_DeleteFiles.xml", Task.TaskType.DeleteFile);
                Task.CreateDemoFile("DEMO_DeleteRegKeys.xml", Task.TaskType.DeleteRegKey);
                Task.CreateDemoFile("DEMO_WriteLog.xml", Task.TaskType.WriteLog);
                Task.CreateDemoFile("DEMO_StartProgram.xml", Task.TaskType.StartProgram);
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
                t.Run(a.HaltOnError, a.Output);
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

Path to the configuration: A relative or absolute path to the
configuration as XML file

Flags / Options
---------------
-r | --run:    Runs a task defined in the subsequent config file (path)
-d | --demo:   Runs the demo command and generates example
               configurations in the program folder
-o | --output: Enables the output mode. The results of the task
               will be displayed in the command shell
-h | --halt:   The task runner stops after an error, othewise all
               sub-tasks are executed until the end of the configuration
-l | --log:    Enables logging. After the flag a valid path
               (absolute or relative) to a logfile must be defined 

Possible Tasks
--------------
Please look at the demo files for all parameters.

DeleteFileTask:
The tasks deletes one or several files. There are no additional options.
At the moment, no wildcards are allowed.

DeleteRegKeyTask:
The task deletes a value of a reg key in the Windows registry. Several
hives like HKLM or HKCU can be defined. Note that write permission to
the registry mus be granted to execute such a task.

WriteLogTask:
Writes a defined text with the time stamp of the execution time into
the defined log file. The logfile header is optional and can be passed
as argument (see demo files).

StartProgramTask:
Starts one or several programs with optional arguments. It is possible
to define whether the sub tasks are executed synchronous or asynchronous.
The later can cause freezing of the task runner if an executed application
is not terminated (process still running).
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
                else if (arg == "--output" || arg == "-o")
                {
                    tuple.Output = true;
                    nextArgIs = ArgsTuple.ArgType.flag;
                }
                else if (arg == "--halt" || arg == "-h")
                {
                    tuple.HaltOnError = true;
                    nextArgIs = ArgsTuple.ArgType.flag;
                }
                else if (arg == "--log" || arg == "-l")
                {
                    tuple.Log = true;
                    nextArgIs = ArgsTuple.ArgType.logFile;
                }
                else if (arg == "--demo" || arg == "-d")
                {
                    tuple.Demo = true;
                    nextArgIs = ArgsTuple.ArgType.flag;
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
