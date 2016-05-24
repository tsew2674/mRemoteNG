using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using mRemoteNG.Messages;
using mRemoteNG.Tools;
using mRemoteNG.UI.Forms;

namespace mRemoteNG.App
{
    public static class ProgramRoot
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            ParseCommandLineArgs();
            var mainForm = new frmMain();
            Application.Run(mainForm);
        }

        public static void ParseCommandLineArgs()
        {
            try
            {
                var cmd = new CmdArgumentsInterpreter(Environment.GetCommandLineArgs());

                string consParam = "";
                if (cmd["cons"] != null)
                {
                    consParam = "cons";
                }
                if (cmd["c"] != null)
                {
                    consParam = "c";
                }

                string resetPosParam = "";
                if (cmd["resetpos"] != null)
                {
                    resetPosParam = "resetpos";
                }
                if (cmd["rp"] != null)
                {
                    resetPosParam = "rp";
                }

                string resetPanelsParam = "";
                if (cmd["resetpanels"] != null)
                {
                    resetPanelsParam = "resetpanels";
                }
                if (cmd["rpnl"] != null)
                {
                    resetPanelsParam = "rpnl";
                }

                string resetToolbarsParam = "";
                if (cmd["resettoolbar"] != null)
                {
                    resetToolbarsParam = "resettoolbar";
                }
                if (cmd["rtbr"] != null)
                {
                    resetToolbarsParam = "rtbr";
                }

                if (cmd["reset"] != null)
                {
                    resetPosParam = "rp";
                    resetPanelsParam = "rpnl";
                    resetToolbarsParam = "rtbr";
                }

                string noReconnectParam = "";
                if (cmd["noreconnect"] != null)
                {
                    noReconnectParam = "noreconnect";
                }
                if (cmd["norc"] != null)
                {
                    noReconnectParam = "norc";
                }

                if (!string.IsNullOrEmpty(consParam))
                {
                    if (File.Exists(cmd[consParam]) == false)
                    {
                        if (File.Exists((new Microsoft.VisualBasic.ApplicationServices.WindowsFormsApplicationBase()).Info.DirectoryPath + "\\" + cmd[consParam]))
                        {
                            Settings.Default.LoadConsFromCustomLocation = true;
                            Settings.Default.CustomConsPath = (new Microsoft.VisualBasic.ApplicationServices.WindowsFormsApplicationBase()).Info.DirectoryPath + "\\" + cmd[consParam];
                            return;
                        }
                        else if (File.Exists(Info.ConnectionsFileInfo.DefaultConnectionsPath + "\\" + cmd[consParam]))
                        {
                            Settings.Default.LoadConsFromCustomLocation = true;
                            Settings.Default.CustomConsPath = Info.ConnectionsFileInfo.DefaultConnectionsPath + "\\" + cmd[consParam];
                            return;
                        }
                    }
                    else
                    {
                        Settings.Default.LoadConsFromCustomLocation = true;
                        Settings.Default.CustomConsPath = cmd[consParam];
                        return;
                    }
                }

                if (!string.IsNullOrEmpty(resetPosParam))
                {
                    Settings.Default.MainFormKiosk = false;
                    Settings.Default.MainFormLocation = new Point(999, 999);
                    Settings.Default.MainFormSize = new Size(900, 600);
                    Settings.Default.MainFormState = FormWindowState.Normal;
                }

                if (!string.IsNullOrEmpty(resetPanelsParam))
                {
                    Settings.Default.ResetPanels = true;
                }

                if (!string.IsNullOrEmpty(noReconnectParam))
                {
                    Settings.Default.NoReconnect = true;
                }

                if (!string.IsNullOrEmpty(resetToolbarsParam))
                {
                    Settings.Default.ResetToolbars = true;
                }
            }
            catch (Exception ex)
            {
                Runtime.MessageCollector.AddMessage(MessageClass.ErrorMsg, Language.strCommandLineArgsCouldNotBeParsed + Environment.NewLine + ex.Message);
            }
        }

    }
}