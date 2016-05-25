using System;
using System.Diagnostics;
using Microsoft.VisualBasic;
using System.Windows.Forms;
using mRemoteNG.My;
using mRemoteNG.UI.Window;
using mRemoteNG.App;
using mRemoteNG.UI.Forms;


namespace mRemoteNG.Messages
{
	public class MessageCollector
    {
        private Timer _timer;
        private ErrorAndInfoWindow _MCForm;

	    private frmMain _mainForm;
        public MessageCollector(ErrorAndInfoWindow messageCollectorForm, frmMain mainForm)
        {
            _MCForm = messageCollectorForm;
            _mainForm = mainForm;
            CreateTimer();
        }

        #region Public Methods
        public void AddMessage(MessageClass MsgClass, string MsgText, bool OnlyLog = false)
        {
            Message nMsg = new Message(MsgClass, MsgText, DateTime.Now);

            if (nMsg.MsgClass == MessageClass.ReportMsg)
            {
                AddReportMessage(nMsg);
                return;
            }

            if (Settings.Default.SwitchToMCOnInformation && nMsg.MsgClass == MessageClass.InformationMsg)
                AddInfoMessage(OnlyLog, nMsg);
            
            if (Settings.Default.SwitchToMCOnWarning && nMsg.MsgClass == MessageClass.WarningMsg)
                AddWarningMessage(OnlyLog, nMsg);

            if (Settings.Default.SwitchToMCOnError && nMsg.MsgClass == MessageClass.ErrorMsg)
                AddErrorMessage(OnlyLog, nMsg);

            if (!OnlyLog)
            {
                if (Settings.Default.ShowNoMessageBoxes)
                    _timer.Enabled = true;
                else
                    ShowMessageBox(nMsg);

                ListViewItem lvItem = BuildListViewItem(nMsg);
                AddToList(lvItem);
            }
        }

        private void AddInfoMessage(bool OnlyLog, Message nMsg)
        {
            Debug.Print("Info: " + nMsg.MsgText);
            if (Settings.Default.WriteLogFile)
                Runtime.Log.Info(nMsg.MsgText);
        }

        private void AddWarningMessage(bool OnlyLog, Message nMsg)
        {
            Debug.Print("Warning: " + nMsg.MsgText);
            if (Settings.Default.WriteLogFile)
                Runtime.Log.Warn(nMsg.MsgText);
        }

        private void AddErrorMessage(bool OnlyLog, Message nMsg)
        {
            Debug.Print("Error: " + nMsg.MsgText);
            Runtime.Log.Error(nMsg.MsgText);
        }

        private static void AddReportMessage(Message nMsg)
        {
            Debug.Print("Report: " + nMsg.MsgText);
            if (Settings.Default.WriteLogFile)
                Runtime.Log.Info(nMsg.MsgText);
        }

        private static ListViewItem BuildListViewItem(Message nMsg)
        {
            ListViewItem lvItem = new ListViewItem();
            lvItem.ImageIndex = Convert.ToInt32(nMsg.MsgClass);
            lvItem.Text = nMsg.MsgText.Replace(Environment.NewLine, "  ");
            lvItem.Tag = nMsg;
            return lvItem;
        }

        public void AddExceptionMessage(string message, Exception ex, MessageClass msgClass = MessageClass.ErrorMsg, bool logOnly = false)
        {
            AddMessage(msgClass, message + Environment.NewLine + Tools.MiscTools.GetExceptionMessageRecursive(ex), logOnly);
        }
        #endregion

        #region Private Methods
        private void CreateTimer()
        {
            _timer = new Timer();
            _timer.Enabled = false;
            _timer.Interval = 300;
            _timer.Tick += SwitchTimerTick;
        }

        private void SwitchTimerTick(object sender, EventArgs e)
        {
            SwitchToMessage();
            _timer.Enabled = false;
        }

        private void SwitchToMessage()
        {
            _MCForm.PreviousActiveForm = (WeifenLuo.WinFormsUI.Docking.DockContent)_mainForm.pnlDock.ActiveContent;
            ShowMCForm();
            _MCForm.lvErrorCollector.Focus();
            _MCForm.lvErrorCollector.SelectedItems.Clear();
            _MCForm.lvErrorCollector.Items[0].Selected = true;
            _MCForm.lvErrorCollector.FocusedItem = _MCForm.lvErrorCollector.Items[0];
        }

        private static void ShowMessageBox(Message Msg)
        {
            switch (Msg.MsgClass)
            {
                case MessageClass.InformationMsg:
                    MessageBox.Show(Msg.MsgText, string.Format(Language.strTitleInformation, Msg.MsgDate), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case MessageClass.WarningMsg:
                    MessageBox.Show(Msg.MsgText, string.Format(Language.strTitleWarning, Msg.MsgDate), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                case MessageClass.ErrorMsg:
                    MessageBox.Show(Msg.MsgText, string.Format(Language.strTitleError, Msg.MsgDate), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
        }
        #endregion
		
        #region Delegates
		private delegate void ShowMCFormCB();
		private void ShowMCForm()
		{
			if (_mainForm.pnlDock.InvokeRequired)
			{
				ShowMCFormCB d = new ShowMCFormCB(ShowMCForm);
				_mainForm.pnlDock.Invoke(d);
			}
			else
			{
				_MCForm.Show(_mainForm.pnlDock);
			}
		}

        delegate void AddToListCB(ListViewItem lvItem);
        private void AddToList(ListViewItem lvItem)
        {
            if (_MCForm.lvErrorCollector.InvokeRequired)
            {
                AddToListCB d = new AddToListCB(AddToList);
                _MCForm.lvErrorCollector.Invoke(d, new object[] { lvItem });
            }
            else
            {
                _MCForm.lvErrorCollector.Items.Insert(0, lvItem);
            }
        }
        #endregion
	}
}