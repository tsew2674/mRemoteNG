using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using mRemoteNG.App;
using mRemoteNG.App.Info;
using mRemoteNG.Messages;
using mRemoteNG.Security;
using mRemoteNG.Tools;
using mRemoteNG.UI.Forms;

namespace mRemoteNG.Config.Settings
{
    public class SettingsSaver
    {
        private frmMain _parentForm;
        public SettingsSaver(frmMain parentForm)
        {
            _parentForm = parentForm;
        }

        public void SaveSettings()
        {
            try
            {
                var windowPlacement = new WindowPlacement(_parentForm);
                if (_parentForm.WindowState == FormWindowState.Minimized & windowPlacement.RestoreToMaximized)
                {
                    _parentForm.Opacity = 0;
                    _parentForm.WindowState = FormWindowState.Maximized;
                }

                mRemoteNG.Settings.Default.MainFormLocation = _parentForm.Location;
                mRemoteNG.Settings.Default.MainFormSize = _parentForm.Size;

                if (_parentForm.WindowState != FormWindowState.Normal)
                {
                    mRemoteNG.Settings.Default.MainFormRestoreLocation = _parentForm.RestoreBounds.Location;
                    mRemoteNG.Settings.Default.MainFormRestoreSize = _parentForm.RestoreBounds.Size;
                }

                mRemoteNG.Settings.Default.MainFormState = _parentForm.WindowState;

                if (_parentForm.Fullscreen != null)
                {
                    mRemoteNG.Settings.Default.MainFormKiosk = _parentForm.Fullscreen.Value;
                }

                mRemoteNG.Settings.Default.FirstStart = false;
                mRemoteNG.Settings.Default.ResetPanels = false;
                mRemoteNG.Settings.Default.ResetToolbars = false;
                mRemoteNG.Settings.Default.NoReconnect = false;

                mRemoteNG.Settings.Default.ExtAppsTBLocation = _parentForm.tsExternalTools.Location;
                if (_parentForm.tsExternalTools.Parent != null)
                {
                    mRemoteNG.Settings.Default.ExtAppsTBParentDock = _parentForm.tsExternalTools.Parent.Dock.ToString();
                }
                mRemoteNG.Settings.Default.ExtAppsTBVisible = _parentForm.tsExternalTools.Visible;
                mRemoteNG.Settings.Default.ExtAppsTBShowText = _parentForm.cMenToolbarShowText.Checked;

                mRemoteNG.Settings.Default.QuickyTBLocation = _parentForm.tsQuickConnect.Location;
                if (_parentForm.tsQuickConnect.Parent != null)
                {
                    mRemoteNG.Settings.Default.QuickyTBParentDock = _parentForm.tsQuickConnect.Parent.Dock.ToString();
                }
                mRemoteNG.Settings.Default.QuickyTBVisible = _parentForm.tsQuickConnect.Visible;

                mRemoteNG.Settings.Default.ConDefaultPassword =
                    Crypt.Encrypt(Convert.ToString(mRemoteNG.Settings.Default.ConDefaultPassword), GeneralAppInfo.EncryptionKey);

                mRemoteNG.Settings.Default.Save();

                SavePanelsToXml();
                SaveExternalAppsToXML();
            }
            catch (Exception ex)
            {
                Runtime.MessageCollector.AddMessage(MessageClass.ErrorMsg,
                    "Saving settings failed" + Environment.NewLine + Environment.NewLine + ex.Message, false);
            }
        }

        public void SavePanelsToXml()
        {
            try
            {
                if (Directory.Exists(SettingsFileInfo.SettingsPath) == false)
                {
                    Directory.CreateDirectory(SettingsFileInfo.SettingsPath);
                }

                _parentForm.pnlDock.SaveAsXml(SettingsFileInfo.SettingsPath + "\\" + SettingsFileInfo.LayoutFileName);
            }
            catch (Exception ex)
            {
                Runtime.MessageCollector.AddMessage(MessageClass.ErrorMsg,
                    "SavePanelsToXML failed" + Environment.NewLine + Environment.NewLine + ex.Message, false);
            }
        }

        public static void SaveExternalAppsToXML()
        {
            try
            {
                if (Directory.Exists(SettingsFileInfo.SettingsPath) == false)
                {
                    Directory.CreateDirectory(SettingsFileInfo.SettingsPath);
                }

                var xmlTextWriter =
                    new XmlTextWriter(SettingsFileInfo.SettingsPath + "\\" + SettingsFileInfo.ExtAppsFilesName,
                        Encoding.UTF8);
                xmlTextWriter.Formatting = Formatting.Indented;
                xmlTextWriter.Indentation = 4;

                xmlTextWriter.WriteStartDocument();
                xmlTextWriter.WriteStartElement("Apps");

                foreach (ExternalTool extA in Runtime.ExternalTools)
                {
                    xmlTextWriter.WriteStartElement("App");
                    xmlTextWriter.WriteAttributeString("DisplayName", "", extA.DisplayName);
                    xmlTextWriter.WriteAttributeString("FileName", "", extA.FileName);
                    xmlTextWriter.WriteAttributeString("Arguments", "", extA.Arguments);
                    xmlTextWriter.WriteAttributeString("WaitForExit", "", Convert.ToString(extA.WaitForExit));
                    xmlTextWriter.WriteAttributeString("TryToIntegrate", "", Convert.ToString(extA.TryIntegrate));
                    xmlTextWriter.WriteEndElement();
                }

                xmlTextWriter.WriteEndElement();
                xmlTextWriter.WriteEndDocument();

                xmlTextWriter.Close();
            }
            catch (Exception ex)
            {
                Runtime.MessageCollector.AddMessage(MessageClass.ErrorMsg,
                    "SaveExternalAppsToXML failed" + Environment.NewLine + Environment.NewLine + ex.Message, false);
            }
        }
    }
}