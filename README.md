# TaskRunner
Task Runner is a simple Windows command line tool to execute several tasks controlled by XML config files, such as deleting files, deleting registry entries, write logfile entries or execute programs.

**Usage**
---------------
<b>Normal Usage:</b>

<code>TaskRunner.exe -r [path to configuration] &lt;options&gt;</code>

<b>Generation of example files of the configuration:</b>

<code>TaskRunner.exe -d</code>

Path to the configuration: A relative or absolute path to the configuration as XML file

**Flags / Options**
---------------
<code>-r | --run</code>:    Runs a task defined in the subsequent config file (path)

<code>-d | --demo</code>:   Runs the demo command and generates example configurations in the program folder

<code>-o | --output</code>: Enables the output mode. The results of the task will be displayed in the command shell

<code>-h | --halt</code>:   The task runner stops after an error, othewise all sub-tasks are executed until the end of the configuration

<code>-l | --log</code>:    Enables logging. After the flag a valid path (absolute or relative) to a logfile must be defined 


**Possible Tasks**
--------------
Please look at the demo files for all parameters.

<b>DeleteFileTask:</b>

The tasks deletes one or several files. There are no additional options. At the moment, no wildcards are allowed.

<b>DeleteRegKeyTask:</b>

The task deletes a value of a reg key in the Windows registry. Several hives like HKLM or HKCU can be defined. Note that write permission to the registry mus be granted to execute such a task.

<b>WriteLogTask:</b>

Writes a defined text with the time stamp of the execution time into the defined log file. The logfile header is optional and can be passed as argument (see demo files).

<b>StartProgramTask:</b>

Starts one or several programs with optional arguments. It is possible to define whether the sub tasks are executed synchronous or asynchronous. The later can cause freezing of the task runner if a executed application is not ended.
