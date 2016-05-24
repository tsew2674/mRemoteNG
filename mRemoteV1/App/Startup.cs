using Microsoft.Win32;
using mRemoteNG.App.Update;
using mRemoteNG.Connection;
using mRemoteNG.Messages;
using mRemoteNG.My;
using mRemoteNG.Tools;
using mRemoteNG.Tree;
using mRemoteNG.UI.Window;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Management;
using System.Threading;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using mRemoteNG.Config.Connections;
using mRemoteNG.UI.Forms;
using mRemoteNG.UI.TaskDialog;

namespace mRemoteNG.App
{
    public class Startup
    {
        private static AppUpdater _appUpdate;

        private frmMain _mainForm;

        public Startup(frmMain mainForm)
        {
            _mainForm = mainForm;
        }

        public void CreatePanels()
        {
            Windows.configForm = new ConfigWindow(Windows.configPanel);
            Windows.configPanel = Windows.configForm;

            Windows.treeForm = new ConnectionTreeWindow(Windows.treePanel);
            Windows.treePanel = Windows.treeForm;
            ConnectionTree.TreeView = Windows.treeForm.tvConnections;

            Windows.errorsForm = new ErrorAndInfoWindow(Windows.errorsPanel);
            Windows.errorsPanel = Windows.errorsForm;

            Windows.screenshotForm = new ScreenshotManagerWindow(Windows.screenshotPanel);
            Windows.screenshotPanel = Windows.screenshotForm;

            Windows.updateForm = new UpdateWindow(Windows.updatePanel);
            Windows.updatePanel = Windows.updateForm;

            Windows.AnnouncementForm = new AnnouncementWindow(Windows.AnnouncementPanel);
            Windows.AnnouncementPanel = Windows.AnnouncementForm;
        }
        //public static void SetDefaultLayout()
        //{
            //frmMain.Default.pnlDock.Visible = false;

            //frmMain.Default.pnlDock.DockLeftPortion = frmMain.Default.pnlDock.Width * 0.2;
            //frmMain.Default.pnlDock.DockRightPortion = frmMain.Default.pnlDock.Width * 0.2;
            //frmMain.Default.pnlDock.DockTopPortion = frmMain.Default.pnlDock.Height * 0.25;
            //frmMain.Default.pnlDock.DockBottomPortion = frmMain.Default.pnlDock.Height * 0.25;

            //Windows.treePanel.Show(frmMain.Default.pnlDock, DockState.DockLeft);
            //Windows.configPanel.Show(frmMain.Default.pnlDock);
            //Windows.configPanel.DockTo(Windows.treePanel.Pane, DockStyle.Bottom, -1);

            //Windows.screenshotForm.Hide();

            //frmMain.Default.pnlDock.Visible = true;
        //}
        public static void GetConnectionIcons()
        {
            var iPath = (new Microsoft.VisualBasic.ApplicationServices.WindowsFormsApplicationBase()).Info.DirectoryPath + "\\Icons\\";
            if (Directory.Exists(iPath) == false)
            {
                return;
            }

            foreach (var f in Directory.GetFiles(iPath, "*.ico", SearchOption.AllDirectories))
            {
                var fInfo = new FileInfo(f);
                Array.Resize(ref ConnectionIcon.Icons, ConnectionIcon.Icons.Length + 1);
                ConnectionIcon.Icons.SetValue(fInfo.Name.Replace(".ico", ""), ConnectionIcon.Icons.Length - 1);
            }
        }


        public static void CreateLogger()
        {
            Runtime.Log = Logger.GetSingletonInstance();
        }
        public static void LogStartupData()
        {
            if (Settings.Default.WriteLogFile)
            {
                LogApplicationData();
                LogCmdLineArgs();
                LogSystemData();
                LogCLRData();
                LogCultureData();
            }
        }
        private static void LogSystemData()
        {
            var osData = GetOperatingSystemData();
            var architecture = GetArchitectureData();
            Runtime.Log.InfoFormat(string.Join(" ", Array.FindAll(new string[] { osData, architecture }, s => !string.IsNullOrEmpty(Convert.ToString(s)))));
        }
        private static string GetOperatingSystemData()
        {
            var osVersion = string.Empty;
            var servicePack = string.Empty;
            var osData = string.Empty;

            try
            {
                foreach (ManagementObject managementObject in new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem WHERE Primary=True").Get())
                {
                    osVersion = GetOSVersion(osVersion, managementObject);
                    servicePack = GetOSServicePack(servicePack, managementObject);
                }
            }
            catch (Exception ex)
            {
                Runtime.Log.WarnFormat("Error retrieving operating system information from WMI. {0}", ex.Message);
            }
            osData = string.Join(" ", new string[] { osVersion, servicePack });
            return osData;
        }
        private static string GetOSVersion(string osVersion, ManagementObject managementObject)
        {
            osVersion = Convert.ToString(managementObject.GetPropertyValue("Caption")).Trim();
            return osVersion;
        }
        private static string GetOSServicePack(string servicePack, ManagementObject managementObject)
        {
            var servicePackNumber = Convert.ToInt32(managementObject.GetPropertyValue("ServicePackMajorVersion"));
            if (servicePackNumber != 0)
            {
                servicePack = $"Service Pack {servicePackNumber}";
            }
            return servicePack;
        }
        private static string GetArchitectureData()
        {
            var architecture = string.Empty;
            try
            {
                foreach (ManagementObject managementObject in new ManagementObjectSearcher("SELECT * FROM Win32_Processor WHERE DeviceID=\'CPU0\'").Get())
                {
                    var addressWidth = Convert.ToInt32(managementObject.GetPropertyValue("AddressWidth"));
                    architecture = $"{addressWidth}-bit";
                }
            }
            catch (Exception ex)
            {
                Runtime.Log.WarnFormat("Error retrieving operating system address width from WMI. {0}", ex.Message);
            }
            return architecture;
        }
        private static void LogApplicationData()
        {
#if !PORTABLE
            Runtime.Log.InfoFormat("{0} {1} starting.", System.Windows.Forms.Application.ProductName, System.Windows.Forms.Application.ProductVersion);
#else
            Runtime.Log.InfoFormat("{0} {1} {2} starting.", Application.ProductName, Application.ProductVersion, Language.strLabelPortableEdition);
#endif
        }
        private static void LogCmdLineArgs()
        {
            Runtime.Log.InfoFormat("Command Line: {0}", Environment.GetCommandLineArgs());
        }
        private static void LogCLRData()
        {
            Runtime.Log.InfoFormat("Microsoft .NET CLR {0}", Environment.Version.ToString());
        }
        private static void LogCultureData()
        {
            Runtime.Log.InfoFormat("System Culture: {0}/{1}", Thread.CurrentThread.CurrentUICulture.Name, Thread.CurrentThread.CurrentUICulture.NativeName);
        }


        public static void CreateConnectionsProvider()
        {
            if (Settings.Default.UseSQLServer == true)
            {
                var _sqlConnectionsProvider = new SqlConnectionsProvider();
            }
        }

        public static void CheckForUpdate()
        {
            if (_appUpdate == null)
            {
                _appUpdate = new AppUpdater();
            }
            else if (_appUpdate.IsGetUpdateInfoRunning)
            {
                return;
            }

            var nextUpdateCheck = Convert.ToDateTime(Settings.Default.CheckForUpdatesLastCheck.Add(TimeSpan.FromDays(Convert.ToDouble(Settings.Default.CheckForUpdatesFrequencyDays))));
            if (!Settings.Default.UpdatePending && DateTime.UtcNow < nextUpdateCheck)
            {
                return;
            }

            _appUpdate.GetUpdateInfoCompletedEvent += GetUpdateInfoCompleted;
            _appUpdate.GetUpdateInfoAsync();
        }
        private static void GetUpdateInfoCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (Runtime.MainForm.InvokeRequired)
            {
                Runtime.MainForm.Invoke(new AsyncCompletedEventHandler(GetUpdateInfoCompleted), new object[] { sender, e });
                return;
            }

            try
            {
                _appUpdate.GetUpdateInfoCompletedEvent -= GetUpdateInfoCompleted;

                if (e.Cancelled)
                {
                    return;
                }
                if (e.Error != null)
                {
                    throw (e.Error);
                }

                if (_appUpdate.IsUpdateAvailable())
                {
                    Windows.Show(WindowType.Update);
                }
            }
            catch (Exception ex)
            {
                Runtime.MessageCollector.AddExceptionMessage("GetUpdateInfoCompleted() failed.", ex, MessageClass.ErrorMsg, true);
            }
        }


        public static void CheckForAnnouncement()
        {
            if (_appUpdate == null)
                _appUpdate = new AppUpdater();
            else if (_appUpdate.IsGetAnnouncementInfoRunning)
                return;

            _appUpdate.GetAnnouncementInfoCompletedEvent += GetAnnouncementInfoCompleted;
            _appUpdate.GetAnnouncementInfoAsync();
        }
        private static void GetAnnouncementInfoCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (MainForm.InvokeRequired)
            {
                Runtime.MainForm.Invoke(new AsyncCompletedEventHandler(GetAnnouncementInfoCompleted), new object[] { sender, e });
                return;
            }

            try
            {
                _appUpdate.GetAnnouncementInfoCompletedEvent -= GetAnnouncementInfoCompleted;

                if (e.Cancelled)
                {
                    return;
                }
                if (e.Error != null)
                {
                    throw (e.Error);
                }

                if (_appUpdate.IsAnnouncementAvailable())
                {
                    Windows.Show(WindowType.Announcement);
                }
            }
            catch (Exception ex)
            {
                Runtime.MessageCollector.AddExceptionMessage("GetAnnouncementInfoCompleted() failed.", ex, MessageClass.ErrorMsg, true);
            }
        }
    }
}