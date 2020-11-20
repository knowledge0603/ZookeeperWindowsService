using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Text;

namespace ZookeeperWindowsService
{
    public partial class ZookeeperService : ServiceBase
    {

        private Process zookeeperProcess = null;
        public ZookeeperService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            System.IO.Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            wLog("zookeeper Service Start");
            startZookeeper();
        }

        protected override void OnStop()
        {
            KillProcessAndChildren(zookeeperProcess.Id);
            wLog("zookeeper  Service Stop\n");
        }
        
        private void startZookeeper(Object sender = null, EventArgs e = null)
        {
             wLog("Zookeeper  service start ");
             wLog(Constants.ZookeeperProcess);
            zookeeperProcess = StartProcess(Constants.ZookeeperProcess);
        }
        private static Process StartProcess(string command)
        {
            var processInfo = new ProcessStartInfo("cmd.exe", "/c " + command)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                Verb = "runas"
            };

            return Process.Start(processInfo);
        }
        private static void KillProcessAndChildren(int pid)
        {
            using (var searcher = new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + pid))
            {
                var managementObjects = searcher.Get();

                foreach (var obj in managementObjects)
                {
                    var managementObject = (ManagementObject)obj;
                    KillProcessAndChildren(Convert.ToInt32(managementObject["ProcessID"]));
                }

                try
                {
                    var proc = Process.GetProcessById(pid);
                    proc.Kill();
                }
                catch (ArgumentException)
                {
                    // Process already exited.
                }
            }
        }
        private void CloseZookeeper()
        {
            wLog("Kill zookeeper  service");
            if (null == zookeeperProcess)
                return;
            zookeeperProcess.Kill();
            zookeeperProcess.Close();
            zookeeperProcess = null;
        }
        private void wLog(string logStr, bool wTime = true)
        {
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\ZookeeperAutoService.log", true))
            {
                string timeStr = wTime == true ? DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss ") : "";
                sw.WriteLine(timeStr + logStr);
            }
        }
    }
}
