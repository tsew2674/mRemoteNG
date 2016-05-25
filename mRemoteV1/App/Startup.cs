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

            Windows.treeForm = new ConnectionTreeWindow(Windows.treePanel, _mainForm);
            Windows.treePanel = Windows.treeForm;
            ConnectionTree.TreeView = Windows.treeForm.tvConnections;

            Windows.errorsForm = new ErrorAndInfoWindow(Windows.errorsPanel);
            Windows.errorsPanel = Windows.errorsForm;

            Windows.screenshotForm = new ScreenshotManagerWindow(Windows.screenshotPanel);
            Windows.screenshotPanel = Windows.screenshotForm;

            Windows.updateForm = new UpdateWindow(Windows.updatePanel, _mainForm);
            Windows.updatePanel = Windows.updateForm;

            Windows.AnnouncementForm = new AnnouncementWindow(Windows.AnnouncementPanel);
            Windows.AnnouncementPanel = Windows.AnnouncementForm;
        }
        //public static void SetDefaultLayout()
        //{
            //_mainForm.pnlDock.Visible = false;

            //_mainForm.pnlDock.DockLeftPortion = _mainForm.pnlDock.Width * 0.2;
            //_mainForm.pnlDock.DockRightPortion = _mainForm.pnlDock.Width * 0.2;
            //_mainForm.pnlDock.DockTopPortion = _mainForm.pnlDock.Height * 0.25;
            //_mainForm.pnlDock.DockBottomPortion = _mainForm.pnlDock.Height * 0.25;

            //Windows.treePanel.Show(_mainForm.pnlDock, DockState.DockLeft);
            //Windows.configPanel.Show(_mainForm.pnlDock);
            //Windows.configPanel.DockTo(Windows.treePanel.Pane, DockStyle.Bottom, -1);

            //Windows.screenshotForm.Hide();

            //_mainForm.pnlDock.Visible = true;
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
                LogClrData();
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
        private static void LogClrData()
        {
            Runtime.Log.InfoFormat("Microsoft .NET CLR {0}", Environment.Version);
        }
        private static void LogCultureData()
        {
            Runtime.Log.InfoFormat("System Culture: {0}/{1}", Thread.CurrentThread.CurrentUICulture.Name, Thread.CurrentThread.CurrentUICulture.NativeName);
        }


        public void CreateConnectionsProvider()
        {
            if (Settings.Default.UseSQLServer)
            {
                var _sqlConnectionsProvider = new SqlConnectionsProvider(_mainForm);
            }
        }

        private void GetUpdateInfoCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (_mainForm.InvokeRequired)
            {
                _mainForm.Invoke(new AsyncCompletedEventHandler(GetUpdateInfoCompleted), new object[] {sender, e});
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
                    var windows = new Windows(_mainForm);
                    windows.Show(WindowType.Update, _mainForm.pnlDock);
                }
            }
            catch (Exception ex)
            {
                Runtime.MessageCollector.AddExceptionMessage("GetUpdateInfoCompleted() failed.", ex, MessageClass.ErrorMsg, true);
            }
        }


        public void CheckForAnnouncement()
        {
            if (_appUpdate == null)
                _appUpdate = new AppUpdater();
            else if (_appUpdate.IsGetAnnouncementInfoRunning)
                return;

            _appUpdate.GetAnnouncementInfoCompletedEvent += GetAnnouncementInfoCompleted;
            _appUpdate.GetAnnouncementInfoAsync();
        }


        private void GetAnnouncementInfoCompleted(object sender, AsyncCompletedEventArgs e)
        {
            
            if (_mainForm.InvokeRequired)
            {
                _mainForm.Invoke(new AsyncCompletedEventHandler(GetAnnouncementInfoCompleted), new object[] { sender, e });
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
                    var windows = new Windows(_mainForm);
                    windows.Show(WindowType.Announcement, _mainForm.pnlDock);
                }
            }
            catch (Exception ex)
            {
                Runtime.MessageCollector.AddExceptionMessage("GetAnnouncementInfoCompleted() failed.", ex, MessageClass.ErrorMsg, true);
            }
        }
    }
}