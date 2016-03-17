using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Win32.TaskScheduler;
using System.Text.RegularExpressions;

namespace SSHBackup
{
    class BackupSchedule
    {

        public static void AddTasks(List<BackupTask> tasks)
        {
            string taskPrefix = "sergeybarkov.SSHBackup.";

            var appFullPath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;

            using (TaskService ts = new TaskService())
            {
                var currentTasks = ts.FindAllTasks(new Regex(taskPrefix));

                foreach(var task in currentTasks)
                {
                    ts.RootFolder.DeleteTask(task.Name);
                }

                foreach (var task in tasks)
                {
                    if (task.schedule)
                    {
                        ts.Execute(appFullPath).WithArguments("/background " + task.taskName).Every(task.scheduleInterval).Days().Starting(DateTime.Now.ToString("yyyy-MM-dd") + " " + task.scheduleStartFrom.ToString("HH:mm")).AsTask(taskPrefix + task.taskName);
                    }
                }
            }
        }

       

    }
}
