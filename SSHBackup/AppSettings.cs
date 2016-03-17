using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Windows;

namespace SSHBackup
{
    [Serializable]
    public class AppSettings
    {
        public List<BackupTask> tasks { get; set; }

        //[NonSerialized]
        //public List<BackupIntervalType> backupIntervalTypes;


        public AppSettings() { tasks = new List<BackupTask>(); }

        public static AppSettings Init()
        {
            AppSettings settings = null;

            var data = Properties.Settings.Default.data;
            //var data = "";

            XmlSerializer formatter = new XmlSerializer(typeof(AppSettings));

            using (var st = new MemoryStream(Encoding.UTF8.GetBytes(data)))
            {

                try
                {
                    settings = (AppSettings)formatter.Deserialize(st);
                }
                catch (Exception ex)
                {
                    settings = new AppSettings();
                    settings.tasks.Add(new BackupTask { taskName = "default"} );
                    return settings;
                }
                
            }


            return settings;
        }

        public void Save()
        {
            XmlSerializer formatter = new XmlSerializer(typeof(AppSettings));

            using (var st = new MemoryStream())
            {
                formatter.Serialize(st, this);
                var data = Encoding.UTF8.GetString(st.ToArray());
                Properties.Settings.Default.data = data;
                Properties.Settings.Default.Save();
            }
        }

        public bool IsDirty()
        {
            var savedSettings = Properties.Settings.Default.data;

            XmlSerializer formatter = new XmlSerializer(typeof(AppSettings));
            var currentSettings = String.Empty;

            using (var st = new MemoryStream())
            {
                formatter.Serialize(st, this);
                currentSettings = Encoding.UTF8.GetString(st.ToArray());
            }

            return savedSettings != currentSettings;
        }

        public bool SaveMe(string msg)
        {
            if (IsDirty())
            {
                MessageBoxResult messageBoxResult = MessageBox.Show(msg, "Save changes?", MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    Save();
                    return true;
                }
                return false;
            }

            return true;

        }


    }

    public class BackupTask
    {
        public string taskName { get; set; }
        public string hostName { get; set; }
        public string sshUser { get; set; }
        public string sshPassword { get; set; }
        public string dbUser { get; set; }
        public string dbPassword { get; set; }
        public string dbList { get; set; }
        public string localFolder { get; set; }

        public bool schedule { get; set; }
        public short scheduleInterval { get; set; }
        public BackupIntervalType scheduleIntervalType { get; set; }
        public DateTime scheduleStartFrom { get; set; } 
    }

    public class BackupIntervalType
    {
        public string name { get; set; }
        public int value { get; set; }
    }
}
