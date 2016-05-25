using System;
using System.Windows.Forms;
using mRemoteNG.Forms;
using mRemoteNG.Config.Connections;
using mRemoteNG.UI.Forms;


namespace mRemoteNG.App
{
	public class Export
	{
	    private frmMain _mainForm;

	    public Export(frmMain mainForm)
	    {
	        _mainForm = mainForm;
	    }

		public void ExportToFile(TreeNode rootTreeNode, TreeNode selectedTreeNode)
		{
			try
			{
			    var saveSecurity = new Security.Save();
					
				using (var exportForm = new ExportForm())
				{
					if (Tree.ConnectionTreeNode.GetNodeType(selectedTreeNode) == Tree.TreeNodeType.Container)
					{
						exportForm.SelectedFolder = selectedTreeNode;
					}
					else if (Tree.ConnectionTreeNode.GetNodeType(selectedTreeNode) == Tree.TreeNodeType.Connection)
					{
						if (Tree.ConnectionTreeNode.GetNodeType(selectedTreeNode.Parent) == Tree.TreeNodeType.Container)
						{
							exportForm.SelectedFolder = selectedTreeNode.Parent;
						}
						exportForm.SelectedConnection = selectedTreeNode;
					}
						
					if (exportForm.ShowDialog(_mainForm) != DialogResult.OK)
					{
						return ;
					}

				    TreeNode exportTreeNode;
				    switch (exportForm.Scope)
					{
						case mRemoteNG.Forms.ExportForm.ExportScope.SelectedFolder:
							exportTreeNode = exportForm.SelectedFolder;
							break;
                        case mRemoteNG.Forms.ExportForm.ExportScope.SelectedConnection:
							exportTreeNode = exportForm.SelectedConnection;
							break;
						default:
							exportTreeNode = rootTreeNode;
							break;
					}
						
					saveSecurity.Username = exportForm.IncludeUsername;
					saveSecurity.Password = exportForm.IncludePassword;
					saveSecurity.Domain = exportForm.IncludeDomain;
					saveSecurity.Inheritance = exportForm.IncludeInheritance;
						
					SaveExportFile(exportForm.FileName, exportForm.SaveFormat, exportTreeNode, saveSecurity);
				}
					
			}
			catch (Exception ex)
			{
                Runtime.MessageCollector.AddExceptionMessage(message: "App.Export.ExportToFile() failed.", ex: ex, logOnly: true);
			}
		}
			
		private void SaveExportFile(string fileName, ConnectionsSaver.Format saveFormat, TreeNode rootNode, Security.Save saveSecurity)
		{
			try
			{
                if (Runtime.SqlConnProvider != null)
				{
                    Runtime.SqlConnProvider.Disable();
				}

			    var connectionsSave = new ConnectionsSaver(_mainForm)
			    {
			        Export = true,
			        ConnectionFileName = fileName,
			        SaveFormat = saveFormat,
			        ConnectionList = Runtime.ConnectionList,
			        ContainerList = Runtime.ContainerList,
			        RootTreeNode = rootNode,
			        SaveSecurity = saveSecurity
			    };
			    connectionsSave.SaveConnections();
			}
			catch (Exception ex)
			{
				Runtime.MessageCollector.AddExceptionMessage($"Export.SaveExportFile(\"{fileName}\") failed.", ex);
			}
			finally
			{
                if (Runtime.SqlConnProvider != null)
				{
                    Runtime.SqlConnProvider.Enable();
				}
			}
		}
	}
}