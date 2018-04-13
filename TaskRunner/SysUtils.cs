using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskRunner
{
    public class SysUtils
    {

        private List<SysUtil> utilities;

        public List<SysUtil> Utilities { get { return utilities; } }

        public SysUtils()
        {
            utilities = new List<SysUtil>();
            utilities.Add(new SysUtil("Group Policy Editor", "GPEdit.msc is a tool to define tasks and policies", "gpedit.msc"));
            utilities.Add(new SysUtil("Task Scheduler", "The Task Scheduler is a tool to define tasks", "control", "schedtasks"));
            utilities.Add(new SysUtil("Regedit", "Regedit is a tool to maintain the Windows Registry", "regedit"));
            utilities.Add(new SysUtil("Resource Monitor", "ResMon is a tool to display currently used system resources", "resmon"));
            utilities.Add(new SysUtil("Service Manager", "services.msc is a tool to manage Windows services", "services.msc"));
            utilities.Add(new SysUtil("Computer Management", "compmgmt.msc is a tool for Windows management", "compmgmt.msc"));
        }

        public class SysUtil
        {
            public string Command { get; private set; }
            public string Arguments { get; private set; }
            public string Name { get; private set; }
            public string Description { get; private set; }

            public SysUtil(string name, string description, string cmd = "cmd", string args = "")
            {
                this.Name = name;
                this.Description = description;
                this.Command = cmd;
                this.Arguments = args;
            }

            public void Run()
            {
                SubTasks.StartProgramTask task = new SubTasks.StartProgramTask();
                task.MainValue = Command;
                if (Arguments != "")
                {
                    task.Arguments.Add(Arguments);
                }
                task.Asynchronous = false;
                task.GetDocumentationStatusCodes();
                task.Run();
            }

        }

    }
}
