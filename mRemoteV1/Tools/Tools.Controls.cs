using System;
using Microsoft.VisualBasic;
using System.Collections;
using System.Windows.Forms;
using mRemoteNG.App;
using mRemoteNG.My;
using mRemoteNG.UI.Forms;


namespace mRemoteNG.Tools
{
	public class Controls
	{
		public class ComboBoxItem
		{
		    public string Text { get; set; }

		    public object Tag { get; set; }

		    public ComboBoxItem(string Text, object Tag = null)
			{
				this.Text = Text;
				if (Tag != null)
				{
					this.Tag = Tag;
				}
			}
				
			public override string ToString()
			{
				return Text;
			}
		}
		
		public class NotificationAreaIcon
		{
			private NotifyIcon _nI;
				
			private ContextMenuStrip _cMen;
			private ToolStripMenuItem _cMenCons;
			private ToolStripSeparator _cMenSep1;
			private ToolStripMenuItem _cMenExit;

		    public bool Disposed { get; set; }

		    private frmMain _mainForm;
		    public NotificationAreaIcon(frmMain mainForm)
		    {
		        _mainForm = mainForm;
		    }


		    //Public Event MouseClick(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs)
			//Public Event MouseDoubleClick(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs)
				
				
			public NotificationAreaIcon()
			{
				try
				{
					_cMenCons = new ToolStripMenuItem();
					_cMenCons.Text = Language.strConnections;
					_cMenCons.Image = Resources.Root;
						
					_cMenSep1 = new ToolStripSeparator();
						
					_cMenExit = new ToolStripMenuItem();
					_cMenExit.Text = Language.strMenuExit;
					_cMenExit.Click += cMenExit_Click;
						
					_cMen = new ContextMenuStrip();
					_cMen.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (8.25F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, Convert.ToByte(0));
					_cMen.RenderMode = ToolStripRenderMode.Professional;
					_cMen.Items.AddRange(new ToolStripItem[] {_cMenCons, _cMenSep1, _cMenExit});
						
					_nI = new NotifyIcon();
					_nI.Text = "mRemote";
					_nI.BalloonTipText = "mRemote";
					_nI.Icon = Resources.mRemote_Icon;
					_nI.ContextMenuStrip = _cMen;
					_nI.Visible = true;
						
					_nI.MouseClick += nI_MouseClick;
					_nI.MouseDoubleClick += nI_MouseDoubleClick;
				}
				catch (Exception ex)
				{
					Runtime.MessageCollector.AddMessage(Messages.MessageClass.ErrorMsg, "Creating new SysTrayIcon failed" + Environment.NewLine + ex.Message, true);
				}
			}
				
			public void Dispose()
			{
				try
				{
					_nI.Visible = false;
					_nI.Dispose();
					_cMen.Dispose();
					Disposed = true;
				}
				catch (Exception ex)
				{
					Runtime.MessageCollector.AddMessage(Messages.MessageClass.ErrorMsg, "Disposing SysTrayIcon failed" + Environment.NewLine + ex.Message, true);
				}
			}
				
			private void nI_MouseClick(object sender, MouseEventArgs e)
			{
				if (e.Button == MouseButtons.Right)
				{
					_cMenCons.DropDownItems.Clear();
						
					foreach (TreeNode tNode in Windows.treeForm.tvConnections.Nodes)
					{
						AddNodeToMenu(tNode.Nodes, _cMenCons);
					}
				}
			}
				
			private void AddNodeToMenu(TreeNodeCollection tnc, ToolStripMenuItem menToolStrip)
			{
				try
				{
					foreach (TreeNode tNode in tnc)
					{
					    var tMenItem = new ToolStripMenuItem
					    {
					        Text = tNode.Text,
					        Tag = tNode
					    };

					    if (Tree.ConnectionTreeNode.GetNodeType(tNode) == Tree.TreeNodeType.Container)
						{
							tMenItem.Image = Resources.Folder;
							tMenItem.Tag = tNode.Tag;
								
							menToolStrip.DropDownItems.Add(tMenItem);
							AddNodeToMenu(tNode.Nodes, tMenItem);
						}
						else if (Tree.ConnectionTreeNode.GetNodeType(tNode) == Tree.TreeNodeType.Connection | Tree.ConnectionTreeNode.GetNodeType(tNode) == Tree.TreeNodeType.PuttySession)
						{
							tMenItem.Image = Windows.treeForm.imgListTree.Images[tNode.ImageIndex];
							tMenItem.Tag = tNode.Tag;
								
							menToolStrip.DropDownItems.Add(tMenItem);
						}
							
						tMenItem.MouseUp += ConMenItem_MouseUp;
					}
				}
				catch (Exception ex)
				{
					Runtime.MessageCollector.AddMessage(Messages.MessageClass.ErrorMsg, "AddNodeToMenu failed" + Environment.NewLine + ex.Message, true);
				}
			}
				
			private void nI_MouseDoubleClick(object sender, MouseEventArgs e)
			{
				if (_mainForm.Visible == true)
				{
					HideForm();
				}
				else
				{
					ShowForm();
				}
			}
				
			private void ShowForm()
			{
                _mainForm.Show();
                _mainForm.WindowState = _mainForm.PreviousWindowState;
					
				if (Settings.Default.ShowSystemTrayIcon == false)
				{
					Runtime.NotificationAreaIcon.Dispose();
					Runtime.NotificationAreaIcon = null;
				}
			}
				
			private void HideForm()
			{
                _mainForm.Hide();
                _mainForm.PreviousWindowState = _mainForm.WindowState;
			}
				
			private void ConMenItem_MouseUp(object sender, MouseEventArgs e)
			{
				if (e.Button == MouseButtons.Left)
				{
					if (((Control)sender).Tag is Connection.ConnectionInfo)
					{
                        //TODO:Raise an event to show form if it's not visible.
						//if (_mainForm.Visible == false)
						//{
						//	ShowForm();
						//}
                        var runtime = new Runtime(_mainForm);
                        runtime.OpenConnection((Connection.ConnectionInfo)((Control)sender).Tag);
					}
				}
			}
				
			private void cMenExit_Click(object sender, EventArgs e)
			{
				//Shutdown.Quit();
                //TODO:Raise event to close application
			}
		}
		
		public static SaveFileDialog ConnectionsSaveAsDialog()
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.CheckPathExists = true;
			saveFileDialog.InitialDirectory = App.Info.ConnectionsFileInfo.DefaultConnectionsPath;
			saveFileDialog.FileName = App.Info.ConnectionsFileInfo.DefaultConnectionsFile;
			saveFileDialog.OverwritePrompt = true;
				
			saveFileDialog.Filter = Language.strFiltermRemoteXML + "|*.xml|" + Language.strFilterAll + "|*.*";
				
			return saveFileDialog;
		}
		
		//public static SaveFileDialog ConnectionsExportDialog()
		//{
		//	SaveFileDialog saveFileDialog = new SaveFileDialog();
		//	saveFileDialog.CheckPathExists = true;
		//	saveFileDialog.InitialDirectory = App.Info.ConnectionsFileInfo.DefaultConnectionsPath;
		//	saveFileDialog.FileName = App.Info.ConnectionsFileInfo.DefaultConnectionsFile;
		//	saveFileDialog.OverwritePrompt = true;
				
		//	saveFileDialog.Filter = Language.strFiltermRemoteXML + "|*.xml|" + Language.strFiltermRemoteCSV + "|*.csv|" + Language.strFiltervRD2008CSV + "|*.csv|" + Language.strFilterAll + "|*.*";
				
		//	return saveFileDialog;
		//}
		
		public static OpenFileDialog ConnectionsLoadDialog()
		{
			OpenFileDialog lDlg = new OpenFileDialog();
			lDlg.CheckFileExists = true;
			lDlg.InitialDirectory = App.Info.ConnectionsFileInfo.DefaultConnectionsPath;
			lDlg.Filter = Language.strFiltermRemoteXML + "|*.xml|" + Language.strFilterAll + "|*.*";
				
			return lDlg;
		}
		
		public static OpenFileDialog ImportConnectionsRdpFileDialog()
		{
		    var openFileDialog = new OpenFileDialog
		    {
		        CheckFileExists = true,
		        InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal),
		        Filter = string.Join("|", new[] {Language.strFilterRDP, "*.rdp", Language.strFilterAll, "*.*"}),
		        Multiselect = true
		    };
		    return openFileDialog;
		}
	}
}