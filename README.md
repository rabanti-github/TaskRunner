# TaskRunner
Task Runner is a simple Windows command line tool to execute several tasks controlled by XML config files, such as deleting files, terminate processes, deleting registry entries, write logfile entries or execute programs.

<b>Please have a look into the <a href="../../wiki">Wiki</a> for more information.</b>

**Purpose**
----------------------
<b>Preamble</b>

It was a nice Sunday in July 2017 when I suddenly noticed that <code>&lt;Insert your favorite Foe Application&gt;</code> changed something after the last update. The <code>&lt;FoeApp&gt;</code> hooked itself into the Windows auto-start and the option to disable this seemed to be broken. The only way to prevent the auto-start of <code>&lt;FoeApp&gt;</code> was to use one of the obvious Windows optimization utilities or to delete an entry in the Windows registry under <i>HKCU\Software\Microsoft\Windows\CurrentVersion\Run</i>. But <code>&lt;FoeApp&gt;</code> did not care! After starting the app, the registry entry was there again! After fiddling around, I decided to write a small utility to delete this registry entry on each logoff. And while developing, I added further features to my utility, because there are enough <code>&lt;FoeApps&gt;</code> out there with various misbehaviors.

<b>How to use</b>

TaskRunner is meant to be added as a task in the Windows task scheduler. The advantage of using this utility instead of the task scheduler directly is that the configuration of TaskRunner can be done simply by altering the XML code of the configuration. Furthermore, the configuration can contain an arbitrary number of Sub-Tasks.

The TaskRunner has only to be registered once in the task scheduler with the proper arguments. E.g.:
<code>TaskRuner.exe --run C:/tasks/config.xml --log C:/tasks/logfile.log</code>

All Sub-Tasks are now defined in the file <i>C:/tasks/config.xml</i>. Furthermore, the result of each task execution will be logged into <i>C:/tasks/logfile.log</i>. The task in the Windows task Scheduler don’t have to be altered anymore.

The utility can be used of course as standalone application e.g. in combination with a BAT file to run repeating tasks just by double-clicking on the BAT file.

**Prerequisites**
----------------------
TaskRunner was written in C# and relies on Windows functionalities. The only prerequisites are:

* A Windows System (7 / 8.x / 10)
* An installed .NET Framework (at least .NET 4.5) 

Furthermore, you need the following knowledge when using TaskRunner:

* Basic knowledge of XML (tags, attributes, escaping etc.)
* Basic knowledge how to run a command line application
* Basic knowledge about the Windows task scheduler or the local group policy editor (gpedit.msc) if you want to use TaskRunner as proxy app


**Usage**
---------------
<b>Normal Usage:</b>

<code>TaskRunner.exe -r [path to configuration] &lt;options&gt;</code>

<b>Generation of example files of the configuration:</b>

<code>TaskRunner.exe -e</code>

<b>Generation of markdown files of the documentation:</b>

<code>TaskRunner.exe -m</code>

Path to the configuration: A relative or absolute path to the configuration as XML file

**Parameter Handling**
---------------
The flag <code>-p</code> or <code>--param</code> delivers a temporary parameter to the TaskRunner. The parameter is only valid during the execution of the loaded task.

Syntax: <code>-p|--param:<i>&lt;data type&gt</i>:<i>&lt;param name&gt;</i>:<i>&lt;param value&gt;</i></code>

The parameter flag contains 3 or 4 parts, delimited by colons:
* 1: <b>Flag Identifier</b> (-p or --param)
* 2: (Optional) <b>Data Type</b>. Valid values are '<code>s</code>' for string, '<code>b</code>' for boolean and '<code>n</code>' for number (double). If this part is omitted, the value will be handled as string
* 3: <b>Parameter Name</b> (unique string, without spaces or colons)
* 4: <b>Parameter Value</b> (The value will be parsed to boolean or double in case of the data types '<code>b</code>' or '<code>n</code>')

<b>Examples:</b>
<code>
-p:n:NUMBER_OF_FILES:8

--param:b:MATCH:true

--param:NAME:machien1

-p:s:NAME:""Name with spaces""

--param:COMMENT:'Other quotes are also OK'

</code>

**Flags / Options**
---------------
<code>-r | --run</code>:      Runs a task defined in the subsequent config file (path)

<code>-e | --example</code>:  Runs the demo command and generates example configurations in the program folder

<code>-o | --output</code>:   Enables the output mode. The results of the task will be displayed in the command shell

<code>-s | --stop</code>:     The task runner stops after an error, otherwise all sub-tasks are executed until the end of the configuration

<code>-l | --log</code>:      Enables logging. After the flag a valid path (absolute or relative) to a logfile must be defined 

<code>-h | --help</code>:     Shows the program help

<code>-d | --docs</code>:     Shows the menu with the documentation

<code>-m | --markdown</code>: Saves the documentation as markdown files 


**Possible Tasks**
--------------
Please look at the demo files and into the <a href="../../wiki/Possible-Tasks-&-Configuration">Wiki</a> for all parameters and a detailed description of the configuration.

<b>DeleteFileTask:</b>

The tasks deletes one or several files. There are no additional options. At the moment, no wildcards are allowed.

<b>DeleteRegKeyTask:</b>

The task deletes a value of a reg key in the Windows registry. Several hives like HKLM or HKCU can be defined. Note that write permission to the registry mus be granted to execute such a task.

<b>WriteLogTask:</b>

Writes a defined text with the time stamp of the execution time into the defined log file. The logfile header is optional and can be passed as argument (see demo files).

<b>StartProgramTask:</b>

Starts one or several programs with optional arguments. It is possible to define whether the sub tasks are executed synchronous or asynchronous. The later can cause freezing of the task runner if a executed application is not terminated (process is still running).

<b>ControlServiceTask:</b>

Starts, restarts, pauses, resumes or stops a Windows Servcie by its name. It is possible to define a remote machine name. Furthermore a specific tmeout period can be defined.

<b>KillProcessTask:</b>

Termines one or more processes by its name. It is possible to define a remote machine name.

<b>MetaTask:</b>

 A MetaTask loads the configuration files (xml) of other TaskRunner configurations and executes them
 
 
**Mixed Tasks**
--------------
If the attribute <code>type</code> in the <code>&lt;task&gt;</code> tag is set to the value <b>MixedTask</b>, any of the available Tasks can be described in one configuration file. Otherwise, only one task type (as Sub-Tasks) is used in one configuration file.
