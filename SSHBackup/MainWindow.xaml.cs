using System;
using System.Linq;
using System.Windows;
using System.Threading;
using System.Collections.Generic;

namespace SSHBackup
{
   
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        AppSettings settings;

        public MainWindow()
        {
            InitializeComponent();

            settings = AppSettings.Init();

            listTasks.ItemsSource = settings.tasks;
            //cmbScheduleIntervalType.ItemsSource = settings.backupIntervalTypes;


            /*txtHost.Text = Properties.Settings.Default.sshHost;
            txtSshUser.Text = Properties.Settings.Default.sshUser;
            txtSshPassword.Password = Properties.Settings.Default.sshPassword;
            txtDbUser.Text = Properties.Settings.Default.dbUser;
            txtDbPassword.Password = Properties.Settings.Default.dbPassword;
            txtDbList.Text = Properties.Settings.Default.dbList;
            txtLocalFolder.Text = Properties.Settings.Default.localFolder;

            chkShedule.IsChecked = Properties.Settings.Default.schedule;
            txtScheduleInterval.Text = Properties.Settings.Default.schedulePeriod.ToString();

            //cmbScheduleIntervalType.Selected


            txtScheduleTime.Text = Properties.Settings.Default.scheduleStartTime;*/

            var commandline = Environment.GetCommandLineArgs();
            if(commandline.Length > 2 && commandline[1] == "/background")
            {
                var taskName = commandline[2];

                var task = settings.tasks.Where(t => t.taskName == taskName).FirstOrDefault();

                if (task != null)
                {
                    runTask(task);
                }
                Application.Current.Shutdown();
            }


        }

        private void runTask(BackupTask task)
        {
            var backup = new SshBackup(task.hostName, task.sshUser, task.sshPassword, task.dbUser, task.dbPassword, task.localFolder, task.dbList.Split('\n').ToList());
            backup.OnMessage += Backup_OnMessage;
            backup.OnStart += Backup_OnStart;
            backup.OnFinish += Backup_OnFinish;

            new Thread(() => backup.Run()).Start();
        }

        /*private void runTask()
        {
            var backup = new SshBackup(txtHost.Text, txtSshUser.Text, txtSshPassword.Password, txtDbUser.Text, txtDbPassword.Password, txtLocalFolder.Text, txtDbList.Text.Split('\n').ToList());
            backup.OnMessage += Backup_OnMessage;
            backup.OnStart += Backup_OnStart;
            backup.OnFinish += Backup_OnFinish;

            new Thread(() => backup.Run()).Start();
        }*/

        private void button_Click(object sender, RoutedEventArgs e)
        {

            if(settings.SaveMe("You must save changes before run task! Save it?"))
            {
                runTask(listTasks.SelectedItem as BackupTask);
            }


        }

        private void Backup_OnFinish(string msg)
        {
            Dispatcher.BeginInvoke(new Action(delegate ()
            {
                cmdRun.IsEnabled = true;
                cmdSave.IsEnabled = true;
                txtConsole.Text += (msg + "\n");
            }));

        }

        private void Backup_OnStart(string msg)
        {
            Dispatcher.BeginInvoke(new Action(delegate ()
            {
                cmdRun.IsEnabled = false;
                cmdSave.IsEnabled = false;
                tabControl.SelectedItem = tabConsole;
                txtConsole.Text += (msg + "\n");
            }));


        }

        private void Backup_OnMessage(string msg)
        {
            Dispatcher.BeginInvoke(new Action(delegate ()
            {
                txtConsole.Text += (msg + "\n");
            }));
        }


        private void cmdSave_Click(object sender, RoutedEventArgs e)
        {
            settings.Save();

            BackupSchedule.AddTasks(settings.tasks);


           /* Properties.Settings.Default.sshHost = txtHost.Text;
            Properties.Settings.Default.sshUser = txtSshUser.Text;
            Properties.Settings.Default.sshPassword = txtSshPassword.Password;
            Properties.Settings.Default.dbUser = txtDbUser.Text;
            Properties.Settings.Default.dbPassword = txtDbPassword.Password;
            Properties.Settings.Default.dbList = txtDbList.Text;
            Properties.Settings.Default.localFolder = txtLocalFolder.Text;

            Properties.Settings.Default.schedule = chkShedule.IsChecked ?? false;
            Properties.Settings.Default.schedulePeriod = int.Parse(txtScheduleInterval.Text);
            Properties.Settings.Default.schedulePeriodType = (int)cmbScheduleIntervalType.Tag;
            Properties.Settings.Default.scheduleStartTime = txtScheduleTime.Text;


            Properties.Settings.Default.Save();*/
        }

       
        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void chkShedule_Checked(object sender, RoutedEventArgs e)
        {
            //gridSchedule.IsEnabled = chkShedule.IsChecked.Value;
        }

        private void cmdNew_Click(object sender, RoutedEventArgs e)
        {
            
            settings.tasks.Add(new BackupTask { taskName = "New task"});
            listTasks.Items.Refresh();
        }

        private void cmdDel_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure?", "Delete Confirmation", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                settings.tasks.Remove((BackupTask)listTasks.SelectedItem);
                listTasks.Items.Refresh();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            cmdNew.Focus();
            settings.SaveMe("Save settings before exit?");
        }

        private void cmdBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.SelectedPath = txtLocalFolder.Text;
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if(result == System.Windows.Forms.DialogResult.OK)
            {
                txtLocalFolder.Text = dialog.SelectedPath;
            }

        }
    }
}
