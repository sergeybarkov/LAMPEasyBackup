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
                txtLocalFolder.Focus();
            }

        }
    }
}
