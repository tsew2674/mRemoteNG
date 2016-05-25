using log4net;
using Microsoft.VisualBasic;
using mRemoteNG.App.Info;
using mRemoteNG.Config.Connections;
using mRemoteNG.Connection;
using mRemoteNG.Connection.Protocol;
using mRemoteNG.Connection.Protocol.RDP;
using mRemoteNG.Container;
using mRemoteNG.Credential;
using mRemoteNG.Images;
using mRemoteNG.Messages;
using mRemoteNG.Tools;
using mRemoteNG.Tree;
using mRemoteNG.UI.Window;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using mRemoteNG.UI.Forms;
using mRemoteNG.UI.TaskDialog;
using WeifenLuo.WinFormsUI.Docking;
using TabPage = Crownwood.Magic.Controls.TabPage;


namespace mRemoteNG.App
{
    public class Runtime
    {
        private frmMain _mainForm;
        public Runtime(frmMain mainForm)
        {
            _mainForm = mainForm;
        }
        #region Private Variables

        //private static System.Timers.Timer _timerSqlWatcher;

        #endregion

        #region Public Properties

        public static ConnectionList ConnectionList { get; set; }

        public static ConnectionList PreviousConnectionList { get; set; }

        public static ContainerList ContainerList { get; set; }

        public static ContainerList PreviousContainerList { get; set; }

        public static CredentialList CredentialList { get; set; }

        public static CredentialList PreviousCredentialList { get; set; }

        public static WindowList WindowList { get; set; }

        public static MessageCollector MessageCollector { get; set; }

        public static Tools.Controls.NotificationAreaIcon NotificationAreaIcon { get; set; }

        public static SystemMenu SystemMenu { get; set; }

        public static ILog Log { get; set; }

        public static bool IsConnectionsFileLoaded { get; set; }


        public static SqlConnectionsProvider SqlConnProvider { get; set; }

        /*
        public static System.Timers.Timer TimerSqlWatcher
		{
			get { return _timerSqlWatcher; }
			set
			{
				_timerSqlWatcher = value;
				_timerSqlWatcher.Elapsed += tmrSqlWatcher_Elapsed;
			}
		}
         */

        public static DateTime LastSqlUpdate { get; set; }

        public static string LastSelected { get; set; }

        public static ConnectionInfo DefaultConnection { get; set; }

        public static ConnectionInfoInheritance DefaultInheritance { get; set; }

        public static ArrayList ExternalTools { get; set; } = new ArrayList();

        #endregion

        #region Default Connection
        public static ConnectionInfo DefaultConnectionFromSettings()
        {
            DefaultConnection = new ConnectionInfo();
            DefaultConnection.IsDefault = true;
            return DefaultConnection;
        }

        public static void DefaultConnectionToSettings()
        {
            Settings.Default.ConDefaultDescription = DefaultConnection.Description;
            Settings.Default.ConDefaultIcon = DefaultConnection.Icon;
            Settings.Default.ConDefaultUsername = DefaultConnection.Username;
            Settings.Default.ConDefaultPassword = DefaultConnection.Password;
            Settings.Default.ConDefaultDomain = DefaultConnection.Domain;
            Settings.Default.ConDefaultProtocol = DefaultConnection.Protocol.ToString();
            Settings.Default.ConDefaultPuttySession = DefaultConnection.PuttySession;
            Settings.Default.ConDefaultICAEncryptionStrength = DefaultConnection.ICAEncryption.ToString();
            Settings.Default.ConDefaultRDPAuthenticationLevel = DefaultConnection.RDPAuthenticationLevel.ToString();
            Settings.Default.ConDefaultLoadBalanceInfo = DefaultConnection.LoadBalanceInfo;
            Settings.Default.ConDefaultUseConsoleSession = DefaultConnection.UseConsoleSession;
            Settings.Default.ConDefaultUseCredSsp = DefaultConnection.UseCredSsp;
            Settings.Default.ConDefaultRenderingEngine = DefaultConnection.RenderingEngine.ToString();
            Settings.Default.ConDefaultResolution = DefaultConnection.Resolution.ToString();
            Settings.Default.ConDefaultAutomaticResize = DefaultConnection.AutomaticResize;
            Settings.Default.ConDefaultColors = DefaultConnection.Colors.ToString();
            Settings.Default.ConDefaultCacheBitmaps = DefaultConnection.CacheBitmaps;
            Settings.Default.ConDefaultDisplayWallpaper = DefaultConnection.DisplayWallpaper;
            Settings.Default.ConDefaultDisplayThemes = DefaultConnection.DisplayThemes;
            Settings.Default.ConDefaultEnableFontSmoothing = DefaultConnection.EnableFontSmoothing;
            Settings.Default.ConDefaultEnableDesktopComposition = DefaultConnection.EnableDesktopComposition;
            Settings.Default.ConDefaultRedirectKeys = DefaultConnection.RedirectKeys;
            Settings.Default.ConDefaultRedirectDiskDrives = DefaultConnection.RedirectDiskDrives;
            Settings.Default.ConDefaultRedirectPrinters = DefaultConnection.RedirectPrinters;
            Settings.Default.ConDefaultRedirectPorts = DefaultConnection.RedirectPorts;
            Settings.Default.ConDefaultRedirectSmartCards = DefaultConnection.RedirectSmartCards;
            Settings.Default.ConDefaultRedirectSound = DefaultConnection.RedirectSound.ToString();
            Settings.Default.ConDefaultPreExtApp = DefaultConnection.PreExtApp;
            Settings.Default.ConDefaultPostExtApp = DefaultConnection.PostExtApp;
            Settings.Default.ConDefaultMacAddress = DefaultConnection.MacAddress;
            Settings.Default.ConDefaultUserField = DefaultConnection.UserField;
            Settings.Default.ConDefaultVNCAuthMode = DefaultConnection.VNCAuthMode.ToString();
            Settings.Default.ConDefaultVNCColors = DefaultConnection.VNCColors.ToString();
            Settings.Default.ConDefaultVNCCompression = DefaultConnection.VNCCompression.ToString();
            Settings.Default.ConDefaultVNCEncoding = DefaultConnection.VNCEncoding.ToString();
            Settings.Default.ConDefaultVNCProxyIP = DefaultConnection.VNCProxyIP;
            Settings.Default.ConDefaultVNCProxyPassword = DefaultConnection.VNCProxyPassword;
            Settings.Default.ConDefaultVNCProxyPort = DefaultConnection.VNCProxyPort;
            Settings.Default.ConDefaultVNCProxyType = DefaultConnection.VNCProxyType.ToString();
            Settings.Default.ConDefaultVNCProxyUsername = DefaultConnection.VNCProxyUsername;
            Settings.Default.ConDefaultVNCSmartSizeMode = DefaultConnection.VNCSmartSizeMode.ToString();
            Settings.Default.ConDefaultVNCViewOnly = DefaultConnection.VNCViewOnly;
            Settings.Default.ConDefaultExtApp = DefaultConnection.ExtApp;
            Settings.Default.ConDefaultRDGatewayUsageMethod = DefaultConnection.RDGatewayUsageMethod.ToString();
            Settings.Default.ConDefaultRDGatewayHostname = DefaultConnection.RDGatewayHostname;
            Settings.Default.ConDefaultRDGatewayUsername = DefaultConnection.RDGatewayUsername;
            Settings.Default.ConDefaultRDGatewayPassword = DefaultConnection.RDGatewayPassword;
            Settings.Default.ConDefaultRDGatewayDomain = DefaultConnection.RDGatewayDomain;
            Settings.Default.ConDefaultRDGatewayUseConnectionCredentials = DefaultConnection.RDGatewayUseConnectionCredentials.ToString();
        }
        #endregion

        #region Default Inheritance
        public static ConnectionInfoInheritance DefaultInheritanceFromSettings()
        {
            DefaultInheritance = new ConnectionInfoInheritance(null);
            DefaultInheritance.IsDefault = true;
            return DefaultInheritance;
        }

        public static void DefaultInheritanceToSettings()
        {
            Settings.Default.InhDefaultDescription = DefaultInheritance.Description;
            Settings.Default.InhDefaultIcon = DefaultInheritance.Icon;
            Settings.Default.InhDefaultPanel = DefaultInheritance.Panel;
            Settings.Default.InhDefaultUsername = DefaultInheritance.Username;
            Settings.Default.InhDefaultPassword = DefaultInheritance.Password;
            Settings.Default.InhDefaultDomain = DefaultInheritance.Domain;
            Settings.Default.InhDefaultProtocol = DefaultInheritance.Protocol;
            Settings.Default.InhDefaultPort = DefaultInheritance.Port;
            Settings.Default.InhDefaultPuttySession = DefaultInheritance.PuttySession;
            Settings.Default.InhDefaultUseConsoleSession = DefaultInheritance.UseConsoleSession;
            Settings.Default.InhDefaultUseCredSsp = DefaultInheritance.UseCredSsp;
            Settings.Default.InhDefaultRenderingEngine = DefaultInheritance.RenderingEngine;
            Settings.Default.InhDefaultICAEncryptionStrength = DefaultInheritance.ICAEncryption;
            Settings.Default.InhDefaultRDPAuthenticationLevel = DefaultInheritance.RDPAuthenticationLevel;
            Settings.Default.InhDefaultLoadBalanceInfo = DefaultInheritance.LoadBalanceInfo;
            Settings.Default.InhDefaultResolution = DefaultInheritance.Resolution;
            Settings.Default.InhDefaultAutomaticResize = DefaultInheritance.AutomaticResize;
            Settings.Default.InhDefaultColors = DefaultInheritance.Colors;
            Settings.Default.InhDefaultCacheBitmaps = DefaultInheritance.CacheBitmaps;
            Settings.Default.InhDefaultDisplayWallpaper = DefaultInheritance.DisplayWallpaper;
            Settings.Default.InhDefaultDisplayThemes = DefaultInheritance.DisplayThemes;
            Settings.Default.InhDefaultEnableFontSmoothing = DefaultInheritance.EnableFontSmoothing;
            Settings.Default.InhDefaultEnableDesktopComposition = DefaultInheritance.EnableDesktopComposition;
            Settings.Default.InhDefaultRedirectKeys = DefaultInheritance.RedirectKeys;
            Settings.Default.InhDefaultRedirectDiskDrives = DefaultInheritance.RedirectDiskDrives;
            Settings.Default.InhDefaultRedirectPrinters = DefaultInheritance.RedirectPrinters;
            Settings.Default.InhDefaultRedirectPorts = DefaultInheritance.RedirectPorts;
            Settings.Default.InhDefaultRedirectSmartCards = DefaultInheritance.RedirectSmartCards;
            Settings.Default.InhDefaultRedirectSound = DefaultInheritance.RedirectSound;
            Settings.Default.InhDefaultPreExtApp = DefaultInheritance.PreExtApp;
            Settings.Default.InhDefaultPostExtApp = DefaultInheritance.PostExtApp;
            Settings.Default.InhDefaultMacAddress = DefaultInheritance.MacAddress;
            Settings.Default.InhDefaultUserField = DefaultInheritance.UserField;
            // VNC inheritance
            Settings.Default.InhDefaultVNCAuthMode = DefaultInheritance.VNCAuthMode;
            Settings.Default.InhDefaultVNCColors = DefaultInheritance.VNCColors;
            Settings.Default.InhDefaultVNCCompression = DefaultInheritance.VNCCompression;
            Settings.Default.InhDefaultVNCEncoding = DefaultInheritance.VNCEncoding;
            Settings.Default.InhDefaultVNCProxyIP = DefaultInheritance.VNCProxyIP;
            Settings.Default.InhDefaultVNCProxyPassword = DefaultInheritance.VNCProxyPassword;
            Settings.Default.InhDefaultVNCProxyPort = DefaultInheritance.VNCProxyPort;
            Settings.Default.InhDefaultVNCProxyType = DefaultInheritance.VNCProxyType;
            Settings.Default.InhDefaultVNCProxyUsername = DefaultInheritance.VNCProxyUsername;
            Settings.Default.InhDefaultVNCSmartSizeMode = DefaultInheritance.VNCSmartSizeMode;
            Settings.Default.InhDefaultVNCViewOnly = DefaultInheritance.VNCViewOnly;
            // Ext. App inheritance
            Settings.Default.InhDefaultExtApp = DefaultInheritance.ExtApp;
            // RDP gateway inheritance
            Settings.Default.InhDefaultRDGatewayUsageMethod = DefaultInheritance.RDGatewayUsageMethod;
            Settings.Default.InhDefaultRDGatewayHostname = DefaultInheritance.RDGatewayHostname;
            Settings.Default.InhDefaultRDGatewayUsername = DefaultInheritance.RDGatewayUsername;
            Settings.Default.InhDefaultRDGatewayPassword = DefaultInheritance.RDGatewayPassword;
            Settings.Default.InhDefaultRDGatewayDomain = DefaultInheritance.RDGatewayDomain;
            Settings.Default.InhDefaultRDGatewayUseConnectionCredentials = DefaultInheritance.RDGatewayUseConnectionCredentials;
        }
        #endregion

        #region Panels
        public Form AddPanel(string title = "", bool noTabber = false)
        {
            try
            {
                var connectionForm = new ConnectionWindow(new DockContent(), _mainForm);
                BuildConnectionWindowContextMenu(connectionForm);
                SetConnectionWindowTitle(title, connectionForm);
                connectionForm.Show(_mainForm.pnlDock, DockState.Document);
                PrepareTabControllerSupport(noTabber, connectionForm);
                return connectionForm;
            }
            catch (Exception ex)
            {
                MessageCollector.AddMessage(MessageClass.ErrorMsg, "Couldn\'t add panel" + Environment.NewLine + ex.Message);
                return null;
            }
        }

        private static void PrepareTabControllerSupport(bool noTabber, ConnectionWindow connectionForm)
        {
            if (noTabber)
                connectionForm.TabController.Dispose();
            else
                WindowList.Add(connectionForm);
        }

        private static void SetConnectionWindowTitle(string title, ConnectionWindow connectionForm)
        {
            if (title == "")
                title = Language.strNewPanel;
            connectionForm.SetFormText(title.Replace("&", "&&"));
        }

        private static void BuildConnectionWindowContextMenu(DockContent pnlcForm)
        {
            var cMen = new ContextMenuStrip();
            var cMenRen = CreateRenameMenuItem(pnlcForm);
            var cMenScreens = CreateScreensMenuItem(pnlcForm);
            cMen.Items.AddRange(new ToolStripMenuItem[] { cMenRen, cMenScreens });
            pnlcForm.TabPageContextMenuStrip = cMen;
        }

        private static ToolStripMenuItem CreateScreensMenuItem(DockContent pnlcForm)
        {
            var cMenScreens = new ToolStripMenuItem
            {
                Text = Language.strSendTo,
                Image = Resources.Monitor,
                Tag = pnlcForm
            };
            cMenScreens.DropDownItems.Add("Dummy");
            cMenScreens.DropDownOpening += cMenConnectionPanelScreens_DropDownOpening;
            return cMenScreens;
        }

        private static ToolStripMenuItem CreateRenameMenuItem(DockContent pnlcForm)
        {
            var cMenRen = new ToolStripMenuItem();
            cMenRen.Text = Language.strRename;
            cMenRen.Image = Resources.Rename;
            cMenRen.Tag = pnlcForm;
            cMenRen.Click += cMenConnectionPanelRename_Click;
            return cMenRen;
        }

        private static void cMenConnectionPanelRename_Click(Object sender, EventArgs e)
        {
            try
            {
                var conW = default(ConnectionWindow);
                conW = (ConnectionWindow)((Control)sender).Tag;

                var nTitle = Interaction.InputBox(Prompt: Language.strNewTitle + ":", DefaultResponse: Convert.ToString(((Control)((Control)sender).Tag).Text.Replace("&&", "&")));

                if (!string.IsNullOrEmpty(nTitle))
                {
                    conW.SetFormText(nTitle.Replace("&", "&&"));
                }
            }
            catch (Exception ex)
            {
                MessageCollector.AddMessage(MessageClass.ErrorMsg, "Couldn\'t rename panel" + Environment.NewLine + ex.Message);
            }
        }

        private static void cMenConnectionPanelScreens_DropDownOpening(Object sender, EventArgs e)
        {
            try
            {
                var cMenScreens = (ToolStripMenuItem)sender;
                cMenScreens.DropDownItems.Clear();

                for (var i = 0; i <= Screen.AllScreens.Length - 1; i++)
                {
                    var cMenScreen = new ToolStripMenuItem(Language.strScreen + " " + Convert.ToString(i + 1));
                    cMenScreen.Tag = new ArrayList();
                    cMenScreen.Image = Resources.Monitor_GoTo;
                    (cMenScreen.Tag as ArrayList).Add(Screen.AllScreens[i]);
                    (cMenScreen.Tag as ArrayList).Add(cMenScreens.Tag);
                    cMenScreen.Click += cMenConnectionPanelScreen_Click;
                    cMenScreens.DropDownItems.Add(cMenScreen);
                }
            }
            catch (Exception ex)
            {
                MessageCollector.AddMessage(MessageClass.ErrorMsg, "Couldn\'t enumerate screens" + Environment.NewLine + ex.Message);
            }
        }

        private static void cMenConnectionPanelScreen_Click(object sender, EventArgs e)
        {
            Screen screen = null;
            DockContent panel = null;
            try
            {
                var tagEnumeration = (IEnumerable)((ToolStripMenuItem)sender).Tag;
                if (tagEnumeration != null)
                {
                    foreach (var obj in tagEnumeration)
                    {
                        if (obj is Screen)
                        {
                            screen = (Screen)obj;
                        }
                        else if (obj is DockContent)
                        {
                            panel = (DockContent)obj;
                        }
                    }
                    Screens.SendPanelToScreen(panel, screen);
                }
            }
            catch (Exception ex)
            {
                MessageCollector.AddMessage(MessageClass.ErrorMsg, "Caught Exception: " + Environment.NewLine + ex.Message);
            }
        }
        #endregion

        #region Connections Loading/Saving
        public static void NewConnections(string filename)
        {
            try
            {
                ConnectionList = new ConnectionList();
                ContainerList = new ContainerList();
                var connectionsLoader = new ConnectionsLoader();

                if (filename == GetDefaultStartupConnectionFileName())
                {
                    Settings.Default.LoadConsFromCustomLocation = false;
                }
                else
                {
                    Settings.Default.LoadConsFromCustomLocation = true;
                    Settings.Default.CustomConsPath = filename;
                }

                Directory.CreateDirectory(Path.GetDirectoryName(filename));

                // Use File.Open with FileMode.CreateNew so that we don't overwrite an existing file
                using (var fileStream = File.Open(filename, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    using (var xmlTextWriter = new XmlTextWriter(fileStream, System.Text.Encoding.UTF8))
                    {
                        xmlTextWriter.Formatting = Formatting.Indented;
                        xmlTextWriter.Indentation = 4;
                        xmlTextWriter.WriteStartDocument();
                        xmlTextWriter.WriteStartElement("Connections"); // Do not localize
                        xmlTextWriter.WriteAttributeString("Name", Language.strConnections);
                        xmlTextWriter.WriteAttributeString("Export", "", "False");
                        xmlTextWriter.WriteAttributeString("Protected", "", "GiUis20DIbnYzWPcdaQKfjE2H5jh//L5v4RGrJMGNXuIq2CttB/d/BxaBP2LwRhY");
                        xmlTextWriter.WriteAttributeString("ConfVersion", "", "2.5");
                        xmlTextWriter.WriteEndElement();
                        xmlTextWriter.WriteEndDocument();
                        xmlTextWriter.Close();
                    }

                }

                connectionsLoader.ConnectionList = ConnectionList;
                connectionsLoader.ContainerList = ContainerList;
                ConnectionTree.ResetTree();
                connectionsLoader.RootTreeNode = Windows.treeForm.tvConnections.Nodes[0];

                // Load config
                connectionsLoader.ConnectionFileName = filename;
                connectionsLoader.LoadConnections(false);
                Windows.treeForm.tvConnections.SelectedNode = connectionsLoader.RootTreeNode;
            }
            catch (Exception ex)
            {
                MessageCollector.AddExceptionMessage(Language.strCouldNotCreateNewConnectionsFile, ex, MessageClass.ErrorMsg);
            }
        }

        public void LoadConnectionsBg(bool withDialog = false, bool update = false)
        {
            _withDialog = false;
            _loadUpdate = true;

            var t = new Thread(LoadConnectionsBGd);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        private static bool _withDialog;
        private static bool _loadUpdate;
        private void LoadConnectionsBGd()
        {
            LoadConnections(_withDialog, _loadUpdate);
        }

        public void LoadConnections(bool withDialog = false, bool update = false)
        {
            var connectionsLoader = new ConnectionsLoader(_mainForm);
            try
            {
                // disable sql update checking while we are loading updates
                SqlConnProvider?.Disable();

                if (ConnectionList != null && ContainerList != null)
                {
                    PreviousConnectionList = ConnectionList.Copy();
                    PreviousContainerList = ContainerList.Copy();
                }

                ConnectionList = new ConnectionList();
                ContainerList = new ContainerList();

                if (!Settings.Default.UseSQLServer)
                {
                    if (withDialog)
                    {
                        var loadDialog = Tools.Controls.ConnectionsLoadDialog();
                        if (loadDialog.ShowDialog() == DialogResult.OK)
                        {
                            connectionsLoader.ConnectionFileName = loadDialog.FileName;
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        connectionsLoader.ConnectionFileName = GetStartupConnectionFileName();
                    }

                    CreateBackupFile(Convert.ToString(connectionsLoader.ConnectionFileName));
                }

                connectionsLoader.ConnectionList = ConnectionList;
                connectionsLoader.ContainerList = ContainerList;

                if (PreviousConnectionList != null && PreviousContainerList != null)
                {
                    connectionsLoader.PreviousConnectionList = PreviousConnectionList;
                    connectionsLoader.PreviousContainerList = PreviousContainerList;
                }

                if (update)
                {
                    connectionsLoader.PreviousSelected = LastSelected;
                }

                ConnectionTree.ResetTree();

                connectionsLoader.RootTreeNode = Windows.treeForm.tvConnections.Nodes[0];
                connectionsLoader.UseSql = Settings.Default.UseSQLServer;
                connectionsLoader.SqlHost = Settings.Default.SQLHost;
                connectionsLoader.SqlDatabaseName = Settings.Default.SQLDatabaseName;
                connectionsLoader.SqlUsername = Settings.Default.SQLUser;
                connectionsLoader.SqlPassword = Security.Crypt.Decrypt(Convert.ToString(Settings.Default.SQLPass), GeneralAppInfo.EncryptionKey);
                connectionsLoader.SqlUpdate = update;
                connectionsLoader.LoadConnections(false);

                if (Settings.Default.UseSQLServer)
                {
                    LastSqlUpdate = DateTime.Now;
                }
                else
                {
                    if (connectionsLoader.ConnectionFileName == GetDefaultStartupConnectionFileName())
                    {
                        Settings.Default.LoadConsFromCustomLocation = false;
                    }
                    else
                    {
                        Settings.Default.LoadConsFromCustomLocation = true;
                        Settings.Default.CustomConsPath = connectionsLoader.ConnectionFileName;
                    }
                }

                // re-enable sql update checking after updates are loaded
                if (Settings.Default.UseSQLServer)
                {
                    SqlConnProvider?.Enable();
                }
            }
            catch (Exception ex)
            {
                if (Settings.Default.UseSQLServer)
                {
                    MessageCollector.AddExceptionMessage(Language.strLoadFromSqlFailed, ex);
                    var commandButtons = string.Join("|", new[] { Language.strCommandTryAgain, Language.strCommandOpenConnectionFile, string.Format(Language.strCommandExitProgram, Application.ProductName) });
                    CTaskDialog.ShowCommandBox(Application.ProductName, Language.strLoadFromSqlFailed, Language.strLoadFromSqlFailedContent, MiscTools.GetExceptionMessageRecursive(ex), "", "", commandButtons, false, ESysIcons.Error, ESysIcons.Error);
                    switch (CTaskDialog.CommandButtonResult)
                    {
                        case 0:
                            LoadConnections(withDialog, update);
                            return;
                        case 1:
                            Settings.Default.UseSQLServer = false;
                            LoadConnections(true, update);
                            return;
                        default:
                            Application.Exit();
                            return;
                    }
                }

                if (ex is FileNotFoundException && !withDialog)
                {
                    MessageCollector.AddExceptionMessage(string.Format(Language.strConnectionsFileCouldNotBeLoadedNew, connectionsLoader.ConnectionFileName), ex, MessageClass.InformationMsg);
                    NewConnections(Convert.ToString(connectionsLoader.ConnectionFileName));
                    return;
                }

                MessageCollector.AddExceptionMessage(string.Format(Language.strConnectionsFileCouldNotBeLoaded, connectionsLoader.ConnectionFileName), ex);
                if (connectionsLoader.ConnectionFileName != GetStartupConnectionFileName())
                {
                    LoadConnections(withDialog, update);
                    return;
                }

                Interaction.MsgBox(string.Format(Language.strErrorStartupConnectionFileLoad, Environment.NewLine, Application.ProductName, GetStartupConnectionFileName(), MiscTools.GetExceptionMessageRecursive(ex)), (int)MsgBoxStyle.OkOnly + MsgBoxStyle.Critical, null);
                Application.Exit();
                return;
            }
        }

        protected static void CreateBackupFile(string fileName)
        {
            // This intentionally doesn't prune any existing backup files. We just assume the user doesn't want any new ones created.
            if (Settings.Default.BackupFileKeepCount == 0)
            {
                return;
            }

            try
            {
                var backupFileName = string.Format(Settings.Default.BackupFileNameFormat, fileName, DateTime.UtcNow);
                File.Copy(fileName, backupFileName);
                PruneBackupFiles(fileName);
            }
            catch (Exception ex)
            {
                MessageCollector.AddExceptionMessage(Language.strConnectionsFileBackupFailed, ex, MessageClass.WarningMsg);
                throw;
            }
        }

        protected static void PruneBackupFiles(string baseName)
        {
            var fileName = Path.GetFileName(baseName);
            var directoryName = Path.GetDirectoryName(baseName);

            if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(directoryName))
            {
                return;
            }

            var searchPattern = string.Format(Settings.Default.BackupFileNameFormat, fileName, "*");
            var files = Directory.GetFiles(directoryName, searchPattern);

            if (files.Length <= Settings.Default.BackupFileKeepCount)
            {
                return;
            }

            Array.Sort(files);
            Array.Resize(ref files, files.Length - Settings.Default.BackupFileKeepCount);

            foreach (var file in files)
            {
                File.Delete(file);
            }
        }

        public static string GetDefaultStartupConnectionFileName()
        {
            var newPath = ConnectionsFileInfo.DefaultConnectionsPath + "\\" + ConnectionsFileInfo.DefaultConnectionsFile;
#if !PORTABLE
			string oldPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\" + Application.ProductName + "\\" + ConnectionsFileInfo.DefaultConnectionsFile;
			if (File.Exists(oldPath))
			{
				return oldPath;
			}
#endif
            return newPath;
        }

        public string GetStartupConnectionFileName()
        {
            if (!Settings.Default.LoadConsFromCustomLocation)
            {
                return GetDefaultStartupConnectionFileName();
            }
            return Settings.Default.CustomConsPath;
        }

        public void SaveConnectionsBg()
        {
            _saveUpdate = true;
            var t = new Thread(SaveConnectionsBGd);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        private static bool _saveUpdate;
        private static object _saveLock = new object();
        private void SaveConnectionsBGd()
        {
            Monitor.Enter(_saveLock);
            SaveConnections(_saveUpdate);
            Monitor.Exit(_saveLock);
        }

        public void SaveConnections(bool update = false)
        {
            if (!IsConnectionsFileLoaded)
            {
                return;
            }

            try
            {
                if (update && Settings.Default.UseSQLServer == false)
                {
                    return;
                }

                if (SqlConnProvider != null)
                {
                    SqlConnProvider.Disable();
                }

                var conS = new ConnectionsSaver(_mainForm);

                if (!Settings.Default.UseSQLServer)
                {
                    conS.ConnectionFileName = GetStartupConnectionFileName();
                }

                conS.ConnectionList = ConnectionList;
                conS.ContainerList = ContainerList;
                conS.Export = false;
                conS.SaveSecurity = new Security.Save(false);
                conS.RootTreeNode = Windows.treeForm.tvConnections.Nodes[0];

                if (Settings.Default.UseSQLServer)
                {
                    conS.SaveFormat = ConnectionsSaver.Format.Sql;
                    conS.SqlHost = Convert.ToString(Settings.Default.SQLHost);
                    conS.SqlDatabaseName = Convert.ToString(Settings.Default.SQLDatabaseName);
                    conS.SqlUsername = Convert.ToString(Settings.Default.SQLUser);
                    conS.SqlPassword = Security.Crypt.Decrypt(Convert.ToString(Settings.Default.SQLPass), GeneralAppInfo.EncryptionKey);
                }

                conS.SaveConnections();

                if (Settings.Default.UseSQLServer)
                {
                    LastSqlUpdate = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                MessageCollector.AddMessage(MessageClass.ErrorMsg, Language.strConnectionsFileCouldNotBeSaved + Environment.NewLine + ex.Message);
            }
            finally
            {
                if (SqlConnProvider != null)
                {
                    SqlConnProvider.Enable();
                }
            }
        }

        public void SaveConnectionsAs()
        {
            var connectionsSave = new ConnectionsSaver(_mainForm);

            try
            {
                SqlConnProvider?.Disable();

                using (var saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.CheckPathExists = true;
                    saveFileDialog.InitialDirectory = ConnectionsFileInfo.DefaultConnectionsPath;
                    saveFileDialog.FileName = ConnectionsFileInfo.DefaultConnectionsFile;
                    saveFileDialog.OverwritePrompt = true;

                    var fileTypes = new List<string>();
                    fileTypes.AddRange(new[] { Language.strFiltermRemoteXML, "*.xml" });
                    fileTypes.AddRange(new[] { Language.strFilterAll, "*.*" });

                    saveFileDialog.Filter = string.Join("|", fileTypes.ToArray());

                    if (!(saveFileDialog.ShowDialog(_mainForm) == DialogResult.OK))
                    {
                        return;
                    }

                    connectionsSave.SaveFormat = ConnectionsSaver.Format.MRxml;
                    connectionsSave.ConnectionFileName = saveFileDialog.FileName;
                    connectionsSave.Export = false;
                    connectionsSave.SaveSecurity = new Security.Save();
                    connectionsSave.ConnectionList = ConnectionList;
                    connectionsSave.ContainerList = ContainerList;
                    connectionsSave.RootTreeNode = Windows.treeForm.tvConnections.Nodes[0];

                    connectionsSave.SaveConnections();

                    if (saveFileDialog.FileName == GetDefaultStartupConnectionFileName())
                    {
                        Settings.Default.LoadConsFromCustomLocation = false;
                    }
                    else
                    {
                        Settings.Default.LoadConsFromCustomLocation = true;
                        Settings.Default.CustomConsPath = saveFileDialog.FileName;
                    }
                }

            }
            catch (Exception ex)
            {
                MessageCollector.AddExceptionMessage(string.Format(Language.strConnectionsFileCouldNotSaveAs, connectionsSave.ConnectionFileName), ex);
            }
            finally
            {
                SqlConnProvider?.Enable();
            }
        }
        #endregion

        #region Opening Connection
        public static ConnectionInfo CreateQuickConnect(string connectionString, ProtocolType protocol)
        {
            try
            {
                var uri = new Uri("dummyscheme" + Uri.SchemeDelimiter + connectionString);
                if (string.IsNullOrEmpty(uri.Host))
                {
                    return null;
                }

                var newConnectionInfo = new ConnectionInfo();

                if (Settings.Default.IdentifyQuickConnectTabs)
                {
                    newConnectionInfo.Name = string.Format(Language.strQuick, uri.Host);
                }
                else
                {
                    newConnectionInfo.Name = uri.Host;
                }

                newConnectionInfo.Protocol = protocol;
                newConnectionInfo.Hostname = uri.Host;
                if (uri.Port == -1)
                {
                    newConnectionInfo.SetDefaultPort();
                }
                else
                {
                    newConnectionInfo.Port = uri.Port;
                }
                newConnectionInfo.IsQuickConnect = true;

                return newConnectionInfo;
            }
            catch (Exception ex)
            {
                MessageCollector.AddExceptionMessage(Language.strQuickConnectFailed, ex, MessageClass.ErrorMsg);
                return null;
            }
        }

        public void OpenConnection()
        {
            try
            {
                OpenConnection(ConnectionInfo.Force.None);
            }
            catch (Exception ex)
            {
                MessageCollector.AddMessage(MessageClass.ErrorMsg, Language.strConnectionOpenFailed + Environment.NewLine + ex.Message);
            }
        }

        public void OpenConnection(ConnectionInfo.Force force)
        {
            try
            {
                if (Windows.treeForm.tvConnections.SelectedNode.Tag == null)
                {
                    return;
                }

                if (ConnectionTreeNode.GetNodeType(ConnectionTree.SelectedNode) == TreeNodeType.Connection | ConnectionTreeNode.GetNodeType(ConnectionTree.SelectedNode) == TreeNodeType.PuttySession)
                {
                    OpenConnection((ConnectionInfo)Windows.treeForm.tvConnections.SelectedNode.Tag, force);
                }
                else if (ConnectionTreeNode.GetNodeType(ConnectionTree.SelectedNode) == TreeNodeType.Container)
                {
                    foreach (TreeNode tNode in ConnectionTree.SelectedNode.Nodes)
                    {
                        if (ConnectionTreeNode.GetNodeType(tNode) == TreeNodeType.Connection | ConnectionTreeNode.GetNodeType(ConnectionTree.SelectedNode) == TreeNodeType.PuttySession)
                        {
                            if (tNode.Tag != null)
                            {
                                OpenConnection((ConnectionInfo)tNode.Tag, force);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageCollector.AddMessage(MessageClass.ErrorMsg, Language.strConnectionOpenFailed + Environment.NewLine + ex.Message);
            }
        }

        public void OpenConnection(ConnectionInfo connectionInfo)
        {
            try
            {
                OpenConnection(connectionInfo, ConnectionInfo.Force.None);
            }
            catch (Exception ex)
            {
                MessageCollector.AddMessage(MessageClass.ErrorMsg, Language.strConnectionOpenFailed + Environment.NewLine + ex.Message);
            }
        }

        public void OpenConnection(ConnectionInfo connectionInfo, Form connectionForm)
        {
            try
            {
                OpenConnectionFinal(connectionInfo, ConnectionInfo.Force.None, connectionForm);
            }
            catch (Exception ex)
            {
                MessageCollector.AddMessage(MessageClass.ErrorMsg, Language.strConnectionOpenFailed + Environment.NewLine + ex.Message);
            }
        }

        public void OpenConnection(ConnectionInfo connectionInfo, Form connectionForm, ConnectionInfo.Force force)
        {
            try
            {
                OpenConnectionFinal(connectionInfo, force, connectionForm);
            }
            catch (Exception ex)
            {
                MessageCollector.AddMessage(MessageClass.ErrorMsg, Language.strConnectionOpenFailed + Environment.NewLine + ex.Message);
            }
        }

        public void OpenConnection(ConnectionInfo connectionInfo, ConnectionInfo.Force force)
        {
            try
            {
                OpenConnectionFinal(connectionInfo, force, null);
            }
            catch (Exception ex)
            {
                MessageCollector.AddMessage(MessageClass.ErrorMsg, Language.strConnectionOpenFailed + Environment.NewLine + ex.Message);
            }
        }

        private void OpenConnectionFinal(ConnectionInfo connectionInfo, ConnectionInfo.Force force, Form conForm)
        {
            try
            {
                if (connectionInfo.Hostname == "" && connectionInfo.Protocol != ProtocolType.IntApp)
                {
                    MessageCollector.AddMessage(MessageClass.WarningMsg, Language.strConnectionOpenFailedNoHostname);
                    return;
                }

                StartPreConnectionExternalApp(connectionInfo);

                if ((force & ConnectionInfo.Force.DoNotJump) != ConnectionInfo.Force.DoNotJump)
                {
                    if (SwitchToOpenConnection(connectionInfo))
                    {
                        return;
                    }
                }

                var protocolFactory = new ProtocolFactory(_mainForm);
                var newProtocol = protocolFactory.CreateProtocol(connectionInfo);

                var connectionPanel = SetConnectionPanel(connectionInfo, force);
                var connectionForm = SetConnectionForm(conForm, connectionPanel);
                var connectionContainer = SetConnectionContainer(connectionInfo, connectionForm);
                SetConnectionFormEventHandlers(newProtocol, connectionForm);
                SetConnectionEventHandlers(newProtocol);
                BuildConnectionInterfaceController(connectionInfo, newProtocol, connectionContainer);

                newProtocol.Force = force;

                if (!newProtocol.Initialize())
                {
                    newProtocol.Close();
                    return;
                }

                if (!newProtocol.Connect())
                {
                    newProtocol.Close();
                    return;
                }

                connectionInfo.OpenConnections.Add(newProtocol);
                SetTreeNodeImages(connectionInfo);
                _mainForm.SelectedConnection = connectionInfo;
            }
            catch (Exception ex)
            {
                MessageCollector.AddMessage(MessageClass.ErrorMsg, Language.strConnectionOpenFailed + Environment.NewLine + ex.Message);
            }
        }

        private static void BuildConnectionInterfaceController(ConnectionInfo connectionInfo, ProtocolBase newProtocol, Control connectionContainer)
        {
            newProtocol.InterfaceControl = new InterfaceControl(connectionContainer, newProtocol, connectionInfo);
        }

        private static void SetConnectionFormEventHandlers(ProtocolBase newProtocol, Form connectionForm)
        {
            newProtocol.Closed += ((ConnectionWindow)connectionForm).Prot_Event_Closed;
        }

        private static Control SetConnectionContainer(ConnectionInfo connectionInfo, Form connectionForm)
        {
            var connectionContainer = default(Control);
            connectionContainer = ((ConnectionWindow)connectionForm).AddConnectionTab(connectionInfo);

            if (connectionInfo.Protocol == ProtocolType.IntApp)
            {
                if (GetExtAppByName(connectionInfo.ExtApp).Icon != null)
                    ((TabPage) connectionContainer).Icon = GetExtAppByName(connectionInfo.ExtApp).Icon;
            }
            return connectionContainer;
        }

        private static void SetTreeNodeImages(ConnectionInfo connectionInfo)
        {
            if (connectionInfo.IsQuickConnect == false)
            {
                if (connectionInfo.Protocol != ProtocolType.IntApp)
                {
                    ConnectionTreeNode.SetNodeImage(connectionInfo.TreeNode, TreeImageType.ConnectionOpen);
                }
                else
                {
                    var extApp = GetExtAppByName(connectionInfo.ExtApp);
                    if (extApp != null)
                    {
                        if (extApp.TryIntegrate && connectionInfo.TreeNode != null)
                        {
                            ConnectionTreeNode.SetNodeImage(connectionInfo.TreeNode, TreeImageType.ConnectionOpen);
                        }
                    }
                }
            }
        }

        private void SetConnectionEventHandlers(ProtocolBase newProtocol)
        {
            newProtocol.Disconnected += Prot_Event_Disconnected;
            newProtocol.Connected += Prot_Event_Connected;
            newProtocol.Closed += Prot_Event_Closed;
            newProtocol.ErrorOccured += Prot_Event_ErrorOccured;
        }

        private Form SetConnectionForm(Form conForm, string connectionPanel)
        {
            var connectionForm = conForm ?? WindowList.FromString(connectionPanel);

            if (connectionForm == null)
                connectionForm = AddPanel(connectionPanel);
            else
                ((ConnectionWindow)connectionForm).Show(_mainForm.pnlDock);

            connectionForm.Focus();
            return connectionForm;
        }

        private string SetConnectionPanel(ConnectionInfo connectionInfo, ConnectionInfo.Force force)
        {
            var connectionPanel = "";
            if (connectionInfo.Panel == "" || (force & ConnectionInfo.Force.OverridePanel) == ConnectionInfo.Force.OverridePanel | Settings.Default.AlwaysShowPanelSelectionDlg)
            {
                var frmPnl = new frmChoosePanel(_mainForm);
                if (frmPnl.ShowDialog() == DialogResult.OK)
                {
                    connectionPanel = frmPnl.Panel;
                }
            }
            else
            {
                connectionPanel = connectionInfo.Panel;
            }
            return connectionPanel;
        }

        private static void StartPreConnectionExternalApp(ConnectionInfo connectionInfo)
        {
            if (connectionInfo.PreExtApp != "")
            {
                var extA = GetExtAppByName(connectionInfo.PreExtApp);
                if (extA != null)
                {
                    extA.Start(connectionInfo);
                }
            }
        }

        public bool SwitchToOpenConnection(ConnectionInfo nCi)
        {
            var ic = FindConnectionContainer(nCi);
            if (ic != null)
            {
                ((ConnectionWindow)ic.FindForm()).Focus();
                ((ConnectionWindow)ic.FindForm()).Show(_mainForm.pnlDock);
                var tabPage = (TabPage)ic.Parent;
                tabPage.Selected = true;
                return true;
            }
            return false;
        }
        #endregion

        #region Event Handlers
        public static void Prot_Event_Disconnected(object sender, string disconnectedMessage)
        {
            try
            {
                MessageCollector.AddMessage(MessageClass.InformationMsg, string.Format(Language.strProtocolEventDisconnected, disconnectedMessage), true);

                var prot = (ProtocolBase)sender;
                if (prot.InterfaceControl.Info.Protocol == ProtocolType.RDP)
                {
                    var reason = disconnectedMessage.Split("\r\n".ToCharArray());
                    var reasonCode = reason[0];
                    var reasonDescription = reason[1];
                    if (Convert.ToInt32(reasonCode) > 3)
                    {
                        if (!string.IsNullOrEmpty(reasonDescription))
                        {
                            MessageCollector.AddMessage(MessageClass.WarningMsg, Language.strRdpDisconnected + Environment.NewLine + reasonDescription + Environment.NewLine + string.Format(Language.strErrorCode, reasonCode));
                        }
                        else
                        {
                            MessageCollector.AddMessage(MessageClass.WarningMsg, Language.strRdpDisconnected + Environment.NewLine + string.Format(Language.strErrorCode, reasonCode));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageCollector.AddMessage(MessageClass.ErrorMsg, string.Format(Language.strProtocolEventDisconnectFailed, ex.Message), true);
            }
        }

        public static void Prot_Event_Closed(object sender)
        {
            try
            {
                var prot = (ProtocolBase)sender;
                MessageCollector.AddMessage(MessageClass.InformationMsg, Language.strConnenctionCloseEvent, true);
                MessageCollector.AddMessage(MessageClass.ReportMsg, string.Format(Language.strConnenctionClosedByUser, prot.InterfaceControl.Info.Hostname, prot.InterfaceControl.Info.Protocol.ToString(), Environment.UserName));
                prot.InterfaceControl.Info.OpenConnections.Remove(prot);

                if (prot.InterfaceControl.Info.OpenConnections.Count < 1 && prot.InterfaceControl.Info.IsQuickConnect == false)
                {
                    ConnectionTreeNode.SetNodeImage(prot.InterfaceControl.Info.TreeNode, TreeImageType.ConnectionClosed);
                }

                if (prot.InterfaceControl.Info.PostExtApp != "")
                {
                    var extA = GetExtAppByName(prot.InterfaceControl.Info.PostExtApp);
                    if (extA != null)
                    {
                        extA.Start(prot.InterfaceControl.Info);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageCollector.AddMessage(MessageClass.ErrorMsg, Language.strConnenctionCloseEventFailed + Environment.NewLine + ex.Message, true);
            }
        }

        public static void Prot_Event_Connected(object sender)
        {
            var prot = (ProtocolBase)sender;
            MessageCollector.AddMessage(MessageClass.InformationMsg, Language.strConnectionEventConnected, true);
            MessageCollector.AddMessage(MessageClass.ReportMsg, string.Format(Language.strConnectionEventConnectedDetail, prot.InterfaceControl.Info.Hostname, prot.InterfaceControl.Info.Protocol.ToString(), Environment.UserName, prot.InterfaceControl.Info.Description, prot.InterfaceControl.Info.UserField));
        }

        public static void Prot_Event_ErrorOccured(object sender, string errorMessage)
        {
            try
            {
                MessageCollector.AddMessage(MessageClass.InformationMsg, Language.strConnectionEventErrorOccured, true);
                var prot = (ProtocolBase)sender;

                if (prot.InterfaceControl.Info.Protocol == ProtocolType.RDP)
                {
                    if (Convert.ToInt32(errorMessage) > -1)
                    {
                        MessageCollector.AddMessage(MessageClass.WarningMsg, string.Format(Language.strConnectionRdpErrorDetail, errorMessage, ProtocolRDP.FatalErrors.GetError(errorMessage)));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageCollector.AddMessage(MessageClass.ErrorMsg, Language.strConnectionEventConnectionFailed + Environment.NewLine + ex.Message, true);
            }
        }
        #endregion

        #region External Apps
        public static ExternalTool GetExtAppByName(string name)
        {
            foreach (ExternalTool extA in ExternalTools)
            {
                if (extA.DisplayName == name)
                    return extA;
            }
            return null;
        }
        #endregion

        #region Misc
        public void GoToUrl(string url)
        {
            var connectionInfo = new ConnectionInfo();

            connectionInfo.Name = "";
            connectionInfo.Hostname = url;
            if (url.StartsWith("https:"))
            {
                connectionInfo.Protocol = ProtocolType.HTTPS;
            }
            else
            {
                connectionInfo.Protocol = ProtocolType.HTTP;
            }
            connectionInfo.SetDefaultPort();
            connectionInfo.IsQuickConnect = true;
            OpenConnection(connectionInfo, ConnectionInfo.Force.DoNotJump);
        }

        public void GoToWebsite()
        {
            GoToUrl(GeneralAppInfo.UrlHome);
        }

        public void GoToDonate()
        {
            GoToUrl(GeneralAppInfo.UrlDonate);
        }

        public void GoToForum()
        {
            GoToUrl(GeneralAppInfo.UrlForum);
        }

        public void GoToBugs()
        {
            GoToUrl(GeneralAppInfo.UrlBugs);
        }

        public void Report(string text)
        {
            try
            {
                var sWr = new StreamWriter(SettingsFileInfo.exePath + "\\Report.log", true);
                sWr.WriteLine(text);
                sWr.Close();
            }
            catch (Exception)
            {
                MessageCollector.AddMessage(MessageClass.ErrorMsg, Language.strLogWriteToFileFailed);
            }
        }

        public static bool SaveReport()
        {
            StreamReader streamReader = null;
            StreamWriter streamWriter = null;
            try
            {
                streamReader = new StreamReader(SettingsFileInfo.exePath + "\\Report.log");
                var text = streamReader.ReadToEnd();
                streamReader.Close();
                streamWriter = new StreamWriter(GeneralAppInfo.ReportingFilePath, true);
                streamWriter.Write(text);
                return true;
            }
            catch (Exception ex)
            {
                MessageCollector.AddMessage(MessageClass.ErrorMsg, Language.strLogWriteToFileFinalLocationFailed + Environment.NewLine + ex.Message, true);
                return false;
            }
            finally
            {
                if (streamReader != null)
                {
                    streamReader.Close();
                    streamReader.Dispose();
                }
                if (streamWriter != null)
                {
                    streamWriter.Close();
                    streamWriter.Dispose();
                }
            }
        }

        public static InterfaceControl FindConnectionContainer(ConnectionInfo connectionInfo)
        {
            if (connectionInfo.OpenConnections.Count > 0)
            {
                for (var i = 0; i <= WindowList.Count - 1; i++)
                {
                    if (WindowList[i] is ConnectionWindow)
                    {
                        var connectionWindow = (ConnectionWindow)WindowList[i];
                        if (connectionWindow.TabController != null)
                        {
                            foreach (TabPage t in connectionWindow.TabController.TabPages)
                            {
                                if (t.Controls[0] != null && t.Controls[0] is InterfaceControl)
                                {
                                    var ic = (InterfaceControl)t.Controls[0];
                                    if (ic.Info == connectionInfo)
                                    {
                                        return ic;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        // Override the font of all controls in a container with the default font based on the OS version
        public static void FontOverride(Control ctlParent)
        {
            var ctlChild = default(Control);
            foreach (Control tempLoopVarCtlChild in ctlParent.Controls)
            {
                ctlChild = tempLoopVarCtlChild;
                ctlChild.Font = new Font(SystemFonts.MessageBoxFont.Name, ctlChild.Font.Size, ctlChild.Font.Style, ctlChild.Font.Unit, ctlChild.Font.GdiCharSet);
                if (ctlChild.Controls.Count > 0)
                {
                    FontOverride(ctlChild);
                }
            }
        }
        #endregion
    }
}