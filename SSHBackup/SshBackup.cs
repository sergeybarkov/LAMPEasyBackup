using System;
using System.Collections.Generic;
using Renci.SshNet;
using System.IO;

namespace SSHBackup
{
    class SshBackup
    {

        public delegate void SshBackupHandler(string msg);
        public event SshBackupHandler OnMessage;
        public event SshBackupHandler OnStart;
        public event SshBackupHandler OnFinish;

        String sshHost;
        String sshUser;
        String sshPassword;
        String mysqlUser;
        String mysqlPassword;
        String localFolder;
        List<String> dbList;

        public SshBackup(String sshHost, String sshUser, String sshPassword, String mysqlUser, String mysqlPassword, String localFolder, List<String> dbList)
        {
            this.sshHost = sshHost;
            this.sshUser = sshUser;
            this.sshPassword = sshPassword;
            this.mysqlUser = mysqlUser;
            this.mysqlPassword = mysqlPassword;
            this.localFolder = localFolder;
            this.dbList = dbList;
        }

        public void Run()
        {

            if (OnStart != null)
            {
                OnStart("Started...");
            }

            try {


            var backupFolder = localFolder;

                using (var client = new SshClient(sshHost, sshUser, sshPassword))
                {
                    client.Connect();

                    foreach (var dbn in dbList)
                    {

                        var dbname = dbn.Trim();

                        var dumpname = String.Format("{0}_{1}.sql.gz", dbname, DateTime.Now.ToString("yyyy-MM-dd_HH-mm"));


                        var commandResult = client.RunCommand(String.Format("mysqldump -u{2} -p{3} {0} | gzip > {1}", dbname, dumpname, mysqlUser, mysqlPassword));

                        if (!String.IsNullOrEmpty(commandResult.Error))
                        {
                            if (OnMessage != null)
                            {
                                OnMessage(commandResult.Error);
                            }
                            continue;
                        }

                        using (var sftp = new SftpClient(client.ConnectionInfo))
                        {
                            sftp.Connect();

                            var fullBackupPath = backupFolder + "/" + dbname;

                            if (!Directory.Exists(fullBackupPath))
                            {
                                Directory.CreateDirectory(fullBackupPath);
                            }

                            using (FileStream fs = new FileStream(fullBackupPath + "/" + dumpname, FileMode.Create))
                            {
                                sftp.DownloadFile("./" + dumpname, fs);
                                if (OnMessage != null)
                                {
                                    OnMessage(dumpname + " - success");
                                }

                                sftp.DeleteFile("./" + dumpname);
                            }

                            sftp.Disconnect();
                        }

                    }

                    client.Disconnect();

            }


            }
            catch (Exception ex)
            {
                if (OnMessage != null)
                {
                    OnMessage(ex.Message);
                }
            }

            if (OnFinish != null)
            {
                OnFinish("Finished.");
            }

        }

    }
}
