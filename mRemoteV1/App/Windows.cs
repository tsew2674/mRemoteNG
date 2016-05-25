using mRemoteNG.Forms;
using mRemoteNG.Messages;
using mRemoteNG.UI.Forms;
using mRemoteNG.UI.Forms.OptionsPages;
using mRemoteNG.UI.Window;
using System;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace mRemoteNG.App
{
    public class Windows
    {
        private frmMain _mainForm;
        public Windows(frmMain mainForm)
        {
            _mainForm = mainForm;
        }
        public static ConnectionTreeWindow treeForm;
        public static DockContent treePanel = new DockContent();
        public static ConfigWindow configForm;
        public static DockContent configPanel = new DockContent();
        public static ErrorAndInfoWindow errorsForm;
        public static DockContent errorsPanel = new DockContent();
        public static ScreenshotManagerWindow screenshotForm;
        public static DockContent screenshotPanel = new DockContent();
        public static ExportForm exportForm;
        public static DockContent exportPanel = new DockContent();
        public static AboutWindow aboutForm;
        public static DockContent aboutPanel = new DockContent();
        public static UpdateWindow updateForm;
        public static DockContent updatePanel = new DockContent();
        public static SSHTransferWindow sshtransferForm;
        public static DockContent sshtransferPanel = new DockContent();
        public static ActiveDirectoryImportWindow adimportForm;
        public static DockContent adimportPanel = new DockContent();
        public static HelpWindow helpForm;
        public static DockContent helpPanel = new DockContent();
        public static ExternalToolsWindow externalappsForm;
        public static DockContent externalappsPanel = new DockContent();
        public static PortScanWindow portscanForm;
        public static DockContent portscanPanel = new DockContent();
        public static UltraVNCWindow ultravncscForm;
        public static DockContent ultravncscPanel = new DockContent();
        public static ComponentsCheckWindow componentscheckForm;
        public static DockContent componentscheckPanel = new DockContent();
        public static AnnouncementWindow AnnouncementForm;
        public static DockContent AnnouncementPanel = new DockContent();

        public void Show(WindowType windowType, DockPanel dockPanel, bool portScanImport = false)
        {
            try
            {
                switch (windowType)
                {
                    case WindowType.About:
                        if (aboutForm == null || aboutForm.IsDisposed)
                        {
                            aboutForm = new AboutWindow(aboutPanel);
                            aboutPanel = aboutForm;
                        }
                        aboutForm.Show(dockPanel);
                        break;
                    case WindowType.Options:
                        using (var optionsForm = new frmOptions(_mainForm))
                        {
                            optionsForm.ShowDialog(dockPanel);
                        }
                        break;
                    case WindowType.Update:
                        if (updateForm == null || updateForm.IsDisposed)
                        {
                            updateForm = new UpdateWindow(updatePanel, _mainForm);
                            updatePanel = updateForm;
                        }
                        updateForm.Show(dockPanel);
                        break;
                    case WindowType.SSHTransfer:
                        sshtransferForm = new SSHTransferWindow(sshtransferPanel);
                        sshtransferPanel = sshtransferForm;
                        sshtransferForm.Show(dockPanel);
                        break;
                    case WindowType.ActiveDirectoryImport:
                        if (adimportForm == null || adimportForm.IsDisposed)
                        {
                            adimportForm = new ActiveDirectoryImportWindow(adimportPanel, _mainForm);
                            adimportPanel = adimportForm;
                        }
                        adimportPanel.Show(dockPanel);
                        break;
                    case WindowType.Help:
                        if (helpForm == null || helpForm.IsDisposed)
                        {
                            helpForm = new HelpWindow(helpPanel);
                            helpPanel = helpForm;
                        }
                        helpForm.Show(dockPanel);
                        break;
                    case WindowType.ExternalApps:
                        if (externalappsForm == null || externalappsForm.IsDisposed)
                        {
                            externalappsForm = new ExternalToolsWindow(externalappsPanel, _mainForm);
                            externalappsPanel = externalappsForm;
                        }
                        externalappsForm.Show(dockPanel);
                        break;
                    case WindowType.PortScan:
                        portscanForm = new PortScanWindow(portscanPanel, _mainForm, portScanImport);
                        portscanPanel = portscanForm;
                        portscanForm.Show(dockPanel);
                        break;
                    case WindowType.UltraVNCSC:
                        if (ultravncscForm == null || ultravncscForm.IsDisposed)
                        {
                            ultravncscForm = new UltraVNCWindow(ultravncscPanel);
                            ultravncscPanel = ultravncscForm;
                        }
                        ultravncscForm.Show(dockPanel);
                        break;
                    case WindowType.ComponentsCheck:
                        if (componentscheckForm == null || componentscheckForm.IsDisposed)
                        {
                            componentscheckForm = new ComponentsCheckWindow(componentscheckPanel);
                            componentscheckPanel = componentscheckForm;
                        }
                        componentscheckForm.Show(dockPanel);
                        break;
                    case WindowType.Announcement:
                        if (AnnouncementForm == null || AnnouncementForm.IsDisposed)
                        {
                            AnnouncementForm = new AnnouncementWindow(AnnouncementPanel);
                            AnnouncementPanel = AnnouncementForm;
                        }
                        AnnouncementForm.Show(dockPanel);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(windowType), windowType, null);
                }
            }
            catch (Exception ex)
            {
                Runtime.MessageCollector.AddMessage(MessageClass.ErrorMsg, "App.Runtime.Windows.Show() failed." + Environment.NewLine + ex.Message, true);
            }
        }
    }
}