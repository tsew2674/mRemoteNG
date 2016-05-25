using mRemoteNG.App;
using mRemoteNG.Connection.Protocol.Http;
using mRemoteNG.Connection.Protocol.ICA;
using mRemoteNG.Connection.Protocol.VNC;
using mRemoteNG.Connection.Protocol.RDP;
using mRemoteNG.My;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using mRemoteNG.Tree;
using mRemoteNG.Connection;
using mRemoteNG.Container;
using mRemoteNG.Connection.Protocol;
using mRemoteNG.Images;
using mRemoteNG.UI.Forms;
using mRemoteNG.Tree.Root;
using mRemoteNG.UI.TaskDialog;

namespace mRemoteNG.Config.Connections
{
	public class ConnectionsLoader
	{
        #region Private Properties
		private XmlDocument _xDom;
		private double _confVersion;
		private string _pW = "mR3m";
		private SqlConnection _sqlCon;
		private SqlCommand _sqlQuery;
		private SqlDataReader _sqlRd;
		private TreeNode _selectedTreeNode;

	    #endregion

	    private frmMain _mainForm;

	    public ConnectionsLoader()
	    {
	        
	    }

	    public ConnectionsLoader(frmMain mainForm)
	    {
	        _mainForm = mainForm;
	    }
				
        #region Public Properties
        public bool UseSql { get; set; }

	    public string SqlHost { get; set; }

	    public string SqlDatabaseName { get; set; }

	    public string SqlUsername { get; set; }

	    public string SqlPassword { get; set; }

	    public bool SqlUpdate { get; set; }

	    public string PreviousSelected { get; set; }

	    public string ConnectionFileName { get; set; }

	    public TreeNode RootTreeNode {get; set;}
		
		public ConnectionList ConnectionList {get; set;}
		
        public ContainerList ContainerList { get; set; }

	    public ConnectionList PreviousConnectionList { get; set; }

	    public ContainerList PreviousContainerList { get; set; }

	    #endregion
				
        #region Public Methods
		public void LoadConnections(bool import)
		{
			if (UseSql)
			{
				LoadFromSql();
			}
			else
			{
				string connections = DecryptCompleteFile();
				LoadFromXml(connections, import);
			}
					
			_mainForm.AreWeUsingSqlServerForSavingConnections = UseSql;
            _mainForm.ConnectionsFileName = ConnectionFileName;
					
			if (!import)
			{
				Putty.Sessions.AddSessionsToTree();
			}
		}
        #endregion
				
        #region SQL
		private delegate void LoadFromSqlDelegate();
		private void LoadFromSql()
		{
            if (Windows.treeForm == null || Windows.treeForm.tvConnections == null)
			{
				return ;
			}
            if (Windows.treeForm.tvConnections.InvokeRequired)
			{
                Windows.treeForm.tvConnections.Invoke(new LoadFromSqlDelegate(LoadFromSql));
				return ;
			}
					
			try
			{
                Runtime.IsConnectionsFileLoaded = false;
						
				if (!string.IsNullOrEmpty(SqlUsername))
				{
					_sqlCon = new SqlConnection("Data Source=" + SqlHost + ";Initial Catalog=" + SqlDatabaseName + ";User Id=" + SqlUsername + ";Password=" + SqlPassword);
				}
				else
				{
					_sqlCon = new SqlConnection("Data Source=" + SqlHost + ";Initial Catalog=" + SqlDatabaseName + ";Integrated Security=True");
				}
						
				_sqlCon.Open();
						
				_sqlQuery = new SqlCommand("SELECT * FROM tblRoot", _sqlCon);
				_sqlRd = _sqlQuery.ExecuteReader(CommandBehavior.CloseConnection);
						
				_sqlRd.Read();
						
				if (!_sqlRd.HasRows)
				{
				    var runtime = new Runtime(_mainForm);
                    runtime.SaveConnections();
							
					_sqlQuery = new SqlCommand("SELECT * FROM tblRoot", _sqlCon);
					_sqlRd = _sqlQuery.ExecuteReader(CommandBehavior.CloseConnection);
							
					_sqlRd.Read();
				}
						
				_confVersion = Convert.ToDouble(_sqlRd["confVersion"], CultureInfo.InvariantCulture);
				const double maxSupportedSchemaVersion = 2.5;
				if (_confVersion > maxSupportedSchemaVersion)
				{
                    CTaskDialog.ShowTaskDialogBox(
                        _mainForm, 
                        Application.ProductName, 
                        "Incompatible database schema",
                        $"The database schema on the server is not supported. Please upgrade to a newer version of {Application.ProductName}.", 
                        string.Format("Schema Version: {1}{0}Highest Supported Version: {2}", Environment.NewLine, _confVersion.ToString(CultureInfo.InvariantCulture), maxSupportedSchemaVersion.ToString(CultureInfo.InvariantCulture)), 
                        "", 
                        "", 
                        "", 
                        "", 
                        ETaskDialogButtons.Ok, 
                        ESysIcons.Error, 
                        ESysIcons.Error
                    );
					throw (new Exception($"Incompatible database schema (schema version {_confVersion})."));
				}
						
				RootTreeNode.Name = Convert.ToString(_sqlRd["Name"]);
						
				var rootInfo = new RootNodeInfo(RootNodeType.Connection);
				rootInfo.Name = RootTreeNode.Name;
				rootInfo.TreeNode = RootTreeNode;
						
				RootTreeNode.Tag = rootInfo;
				RootTreeNode.ImageIndex = (int)TreeImageType.Root;
                RootTreeNode.SelectedImageIndex = (int)TreeImageType.Root;
						
				if (Security.Crypt.Decrypt(Convert.ToString(_sqlRd["Protected"]), _pW) != "ThisIsNotProtected")
				{
					if (Authenticate(Convert.ToString(_sqlRd["Protected"]), false, rootInfo) == false)
					{
						mRemoteNG.Settings.Default.LoadConsFromCustomLocation = false;
                        mRemoteNG.Settings.Default.CustomConsPath = "";
						RootTreeNode.Remove();
						return;
					}
				}
						
				_sqlRd.Close();

                Windows.treeForm.tvConnections.BeginUpdate();
						
				// SECTION 3. Populate the TreeView with the DOM nodes.
				AddNodesFromSql(RootTreeNode);
						
				RootTreeNode.Expand();
						
				//expand containers
				foreach (ContainerInfo contI in ContainerList)
				{
					if (contI.IsExpanded == true)
					{
						contI.TreeNode.Expand();
					}
				}

                Windows.treeForm.tvConnections.EndUpdate();
						
				//open connections from last mremote session
			    if (mRemoteNG.Settings.Default.OpenConsFromLastSession && !mRemoteNG.Settings.Default.NoReconnect)
			    {
			        foreach (ConnectionInfo conI in ConnectionList)
			        {
			            if (conI.PleaseConnect)
			            {

			                //Runtime.OpenConnection(conI);
			                var runtime = new Runtime(_mainForm);
			                runtime.OpenConnection(conI);
			            }
			        }

			        Runtime.IsConnectionsFileLoaded = true;
			        Windows.treeForm.InitialRefresh();
			        SetSelectedNode(_selectedTreeNode);
			    }
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				if (_sqlCon != null)
				{
					_sqlCon.Close();
				}
			}
		}
				
		private delegate void SetSelectedNodeDelegate(TreeNode treeNode);
		private static void SetSelectedNode(TreeNode treeNode)
		{
            if (ConnectionTree.TreeView != null && ConnectionTree.TreeView.InvokeRequired)
			{
                Windows.treeForm.Invoke(new SetSelectedNodeDelegate(SetSelectedNode), new object[] { treeNode });
				return ;
			}
            Windows.treeForm.tvConnections.SelectedNode = treeNode;
		}
				
		private void AddNodesFromSql(TreeNode baseNode)
		{
			try
			{
				_sqlCon.Open();
				_sqlQuery = new SqlCommand("SELECT * FROM tblCons ORDER BY PositionID ASC", _sqlCon);
				_sqlRd = _sqlQuery.ExecuteReader(CommandBehavior.CloseConnection);
						
				if (_sqlRd.HasRows == false)
				{
					return;
				}
						
				TreeNode tNode = default(TreeNode);
						
				while (_sqlRd.Read())
				{
					tNode = new TreeNode(Convert.ToString(_sqlRd["Name"]));
					//baseNode.Nodes.Add(tNode)
							
					if (ConnectionTreeNode.GetNodeTypeFromString(Convert.ToString(_sqlRd["Type"])) == TreeNodeType.Connection)
					{
                        ConnectionInfo conI = GetConnectionInfoFromSql();
						conI.TreeNode = tNode;
						//conI.Parent = _previousContainer 'NEW
								
						ConnectionList.Add(conI);
								
						tNode.Tag = conI;
								
						if (SqlUpdate == true)
						{
                            ConnectionInfo prevCon = PreviousConnectionList.FindByConstantID(conI.ConstantID);
									
							if (prevCon != null)
							{
								foreach (ProtocolBase prot in prevCon.OpenConnections)
								{
									prot.InterfaceControl.Info = conI;
									conI.OpenConnections.Add(prot);
								}
										
								if (conI.OpenConnections.Count > 0)
								{
                                    tNode.ImageIndex = (int)TreeImageType.ConnectionOpen;
                                    tNode.SelectedImageIndex = (int)TreeImageType.ConnectionOpen;
								}
								else
								{
                                    tNode.ImageIndex = (int)TreeImageType.ConnectionClosed;
                                    tNode.SelectedImageIndex = (int)TreeImageType.ConnectionClosed;
								}
							}
							else
							{
                                tNode.ImageIndex = (int)TreeImageType.ConnectionClosed;
                                tNode.SelectedImageIndex = (int)TreeImageType.ConnectionClosed;
							}
									
							if (conI.ConstantID == PreviousSelected)
							{
								_selectedTreeNode = tNode;
							}
						}
						else
						{
                            tNode.ImageIndex = (int)TreeImageType.ConnectionClosed;
                            tNode.SelectedImageIndex = (int)TreeImageType.ConnectionClosed;
						}
					}
					else if (ConnectionTreeNode.GetNodeTypeFromString(Convert.ToString(_sqlRd["Type"])) == TreeNodeType.Container)
					{
                        ContainerInfo contI = new ContainerInfo();
						//If tNode.Parent IsNot Nothing Then
						//    If Tree.Node.GetNodeType(tNode.Parent) = Tree.Node.Type.Container Then
						//        contI.Parent = tNode.Parent.Tag
						//    End If
						//End If
						//_previousContainer = contI 'NEW
						contI.TreeNode = tNode;
								
						contI.Name = Convert.ToString(_sqlRd["Name"]);

                        ConnectionInfo conI = default(ConnectionInfo);
								
						conI = GetConnectionInfoFromSql();
								
						conI.Parent = contI;
						conI.IsContainer = true;
						contI.ConnectionInfo = conI;
								
						if (SqlUpdate == true)
						{
                            ContainerInfo prevCont = PreviousContainerList.FindByConstantID(conI.ConstantID);
							if (prevCont != null)
							{
								contI.IsExpanded = prevCont.IsExpanded;
							}
									
							if (conI.ConstantID == PreviousSelected)
							{
								_selectedTreeNode = tNode;
							}
						}
						else
						{
							if (Convert.ToBoolean(_sqlRd["Expanded"]) == true)
							{
								contI.IsExpanded = true;
							}
							else
							{
								contI.IsExpanded = false;
							}
						}
								
						ContainerList.Add(contI);
						ConnectionList.Add(conI);
								
						tNode.Tag = contI;
                        tNode.ImageIndex = (int)TreeImageType.Container;
                        tNode.SelectedImageIndex = (int)TreeImageType.Container;
					}
							
					string parentId = Convert.ToString(_sqlRd["ParentID"].ToString().Trim());
					if (string.IsNullOrEmpty(parentId) || parentId == "0")
					{
						baseNode.Nodes.Add(tNode);
					}
					else
					{
						TreeNode pNode = ConnectionTreeNode.GetNodeFromConstantID(Convert.ToString(_sqlRd["ParentID"]));
								
						if (pNode != null)
						{
							pNode.Nodes.Add(tNode);
									
							if (ConnectionTreeNode.GetNodeType(tNode) == TreeNodeType.Connection)
							{
								(tNode.Tag as ConnectionInfo).Parent = (ContainerInfo)pNode.Tag;
							}
							else if (ConnectionTreeNode.GetNodeType(tNode) == TreeNodeType.Container)
							{
								(tNode.Tag as ContainerInfo).Parent = (ContainerInfo)pNode.Tag;
							}
						}
						else
						{
							baseNode.Nodes.Add(tNode);
						}
					}
							
					//AddNodesFromSQL(tNode)
				}
			}
			catch (Exception ex)
			{
				Runtime.MessageCollector.AddMessage(Messages.MessageClass.ErrorMsg, Language.strAddNodesFromSqlFailed + Environment.NewLine + ex.Message, true);
			}
		}
				
		private ConnectionInfo GetConnectionInfoFromSql()
		{
			try
			{
				ConnectionInfo connectionInfo = new ConnectionInfo();
						
				connectionInfo.PositionID = Convert.ToInt32(_sqlRd["PositionID"]);
				connectionInfo.ConstantID = Convert.ToString(_sqlRd["ConstantID"]);
				connectionInfo.Name = Convert.ToString(_sqlRd["Name"]);
				connectionInfo.Description = Convert.ToString(_sqlRd["Description"]);
				connectionInfo.Hostname = Convert.ToString(_sqlRd["Hostname"]);
				connectionInfo.Username = Convert.ToString(_sqlRd["Username"]);
				connectionInfo.Password = Security.Crypt.Decrypt(Convert.ToString(_sqlRd["Password"]), _pW);
				connectionInfo.Domain = Convert.ToString(_sqlRd["DomainName"]);
				connectionInfo.DisplayWallpaper = Convert.ToBoolean(_sqlRd["DisplayWallpaper"]);
				connectionInfo.DisplayThemes = Convert.ToBoolean(_sqlRd["DisplayThemes"]);
				connectionInfo.CacheBitmaps = Convert.ToBoolean(_sqlRd["CacheBitmaps"]);
				connectionInfo.UseConsoleSession = Convert.ToBoolean(_sqlRd["ConnectToConsole"]);
				connectionInfo.RedirectDiskDrives = Convert.ToBoolean(_sqlRd["RedirectDiskDrives"]);
				connectionInfo.RedirectPrinters = Convert.ToBoolean(_sqlRd["RedirectPrinters"]);
				connectionInfo.RedirectPorts = Convert.ToBoolean(_sqlRd["RedirectPorts"]);
				connectionInfo.RedirectSmartCards = Convert.ToBoolean(_sqlRd["RedirectSmartCards"]);
				connectionInfo.RedirectKeys = Convert.ToBoolean(_sqlRd["RedirectKeys"]);
                connectionInfo.RedirectSound = (ProtocolRDP.RDPSounds)Tools.MiscTools.StringToEnum(typeof(ProtocolRDP.RDPSounds), Convert.ToString(_sqlRd["RedirectSound"]));
                connectionInfo.Protocol = (ProtocolType)Tools.MiscTools.StringToEnum(typeof(ProtocolType), Convert.ToString(_sqlRd["Protocol"]));
				connectionInfo.Port = Convert.ToInt32(_sqlRd["Port"]);
				connectionInfo.PuttySession = Convert.ToString(_sqlRd["PuttySession"]);
                connectionInfo.Colors = (ProtocolRDP.RDPColors)Tools.MiscTools.StringToEnum(typeof(ProtocolRDP.RDPColors), Convert.ToString(_sqlRd["Colors"]));
                connectionInfo.Resolution = (ProtocolRDP.RDPResolutions)Tools.MiscTools.StringToEnum(typeof(ProtocolRDP.RDPResolutions), Convert.ToString(_sqlRd["Resolution"]));
				connectionInfo.Inheritance = new ConnectionInfoInheritance(connectionInfo);
				connectionInfo.Inheritance.CacheBitmaps = Convert.ToBoolean(_sqlRd["InheritCacheBitmaps"]);
				connectionInfo.Inheritance.Colors = Convert.ToBoolean(_sqlRd["InheritColors"]);
				connectionInfo.Inheritance.Description = Convert.ToBoolean(_sqlRd["InheritDescription"]);
				connectionInfo.Inheritance.DisplayThemes = Convert.ToBoolean(_sqlRd["InheritDisplayThemes"]);
				connectionInfo.Inheritance.DisplayWallpaper = Convert.ToBoolean(_sqlRd["InheritDisplayWallpaper"]);
				connectionInfo.Inheritance.Domain = Convert.ToBoolean(_sqlRd["InheritDomain"]);
				connectionInfo.Inheritance.Icon = Convert.ToBoolean(_sqlRd["InheritIcon"]);
				connectionInfo.Inheritance.Panel = Convert.ToBoolean(_sqlRd["InheritPanel"]);
				connectionInfo.Inheritance.Password = Convert.ToBoolean(_sqlRd["InheritPassword"]);
				connectionInfo.Inheritance.Port = Convert.ToBoolean(_sqlRd["InheritPort"]);
				connectionInfo.Inheritance.Protocol = Convert.ToBoolean(_sqlRd["InheritProtocol"]);
				connectionInfo.Inheritance.PuttySession = Convert.ToBoolean(_sqlRd["InheritPuttySession"]);
				connectionInfo.Inheritance.RedirectDiskDrives = Convert.ToBoolean(_sqlRd["InheritRedirectDiskDrives"]);
				connectionInfo.Inheritance.RedirectKeys = Convert.ToBoolean(_sqlRd["InheritRedirectKeys"]);
				connectionInfo.Inheritance.RedirectPorts = Convert.ToBoolean(_sqlRd["InheritRedirectPorts"]);
				connectionInfo.Inheritance.RedirectPrinters = Convert.ToBoolean(_sqlRd["InheritRedirectPrinters"]);
				connectionInfo.Inheritance.RedirectSmartCards = Convert.ToBoolean(_sqlRd["InheritRedirectSmartCards"]);
				connectionInfo.Inheritance.RedirectSound = Convert.ToBoolean(_sqlRd["InheritRedirectSound"]);
				connectionInfo.Inheritance.Resolution = Convert.ToBoolean(_sqlRd["InheritResolution"]);
				connectionInfo.Inheritance.UseConsoleSession = Convert.ToBoolean(_sqlRd["InheritUseConsoleSession"]);
				connectionInfo.Inheritance.Username = Convert.ToBoolean(_sqlRd["InheritUsername"]);
				connectionInfo.Icon = Convert.ToString(_sqlRd["Icon"]);
				connectionInfo.Panel = Convert.ToString(_sqlRd["Panel"]);
						
				if (_confVersion > 1.5) //1.6
				{
                    connectionInfo.ICAEncryption = (ProtocolICA.EncryptionStrength)Tools.MiscTools.StringToEnum(typeof(ProtocolICA.EncryptionStrength), Convert.ToString(_sqlRd["ICAEncryptionStrength"]));
					connectionInfo.Inheritance.ICAEncryption = Convert.ToBoolean(_sqlRd["InheritICAEncryptionStrength"]);
					connectionInfo.PreExtApp = Convert.ToString(_sqlRd["PreExtApp"]);
					connectionInfo.PostExtApp = Convert.ToString(_sqlRd["PostExtApp"]);
					connectionInfo.Inheritance.PreExtApp = Convert.ToBoolean(_sqlRd["InheritPreExtApp"]);
					connectionInfo.Inheritance.PostExtApp = Convert.ToBoolean(_sqlRd["InheritPostExtApp"]);
				}
						
				if (_confVersion > 1.6) //1.7
				{
                    connectionInfo.VNCCompression = (ProtocolVNC.Compression)Tools.MiscTools.StringToEnum(typeof(ProtocolVNC.Compression), Convert.ToString(_sqlRd["VNCCompression"]));
                    connectionInfo.VNCEncoding = (ProtocolVNC.Encoding)Tools.MiscTools.StringToEnum(typeof(ProtocolVNC.Encoding), Convert.ToString(_sqlRd["VNCEncoding"]));
                    connectionInfo.VNCAuthMode = (ProtocolVNC.AuthMode)Tools.MiscTools.StringToEnum(typeof(ProtocolVNC.AuthMode), Convert.ToString(_sqlRd["VNCAuthMode"]));
                    connectionInfo.VNCProxyType = (ProtocolVNC.ProxyType)Tools.MiscTools.StringToEnum(typeof(ProtocolVNC.ProxyType), Convert.ToString(_sqlRd["VNCProxyType"]));
					connectionInfo.VNCProxyIP = Convert.ToString(_sqlRd["VNCProxyIP"]);
					connectionInfo.VNCProxyPort = Convert.ToInt32(_sqlRd["VNCProxyPort"]);
					connectionInfo.VNCProxyUsername = Convert.ToString(_sqlRd["VNCProxyUsername"]);
					connectionInfo.VNCProxyPassword = Security.Crypt.Decrypt(Convert.ToString(_sqlRd["VNCProxyPassword"]), _pW);
                    connectionInfo.VNCColors = (ProtocolVNC.Colors)Tools.MiscTools.StringToEnum(typeof(ProtocolVNC.Colors), Convert.ToString(_sqlRd["VNCColors"]));
                    connectionInfo.VNCSmartSizeMode = (ProtocolVNC.SmartSizeMode)Tools.MiscTools.StringToEnum(typeof(ProtocolVNC.SmartSizeMode), Convert.ToString(_sqlRd["VNCSmartSizeMode"]));
					connectionInfo.VNCViewOnly = Convert.ToBoolean(_sqlRd["VNCViewOnly"]);
					connectionInfo.Inheritance.VNCCompression = Convert.ToBoolean(_sqlRd["InheritVNCCompression"]);
					connectionInfo.Inheritance.VNCEncoding = Convert.ToBoolean(_sqlRd["InheritVNCEncoding"]);
					connectionInfo.Inheritance.VNCAuthMode = Convert.ToBoolean(_sqlRd["InheritVNCAuthMode"]);
					connectionInfo.Inheritance.VNCProxyType = Convert.ToBoolean(_sqlRd["InheritVNCProxyType"]);
					connectionInfo.Inheritance.VNCProxyIP = Convert.ToBoolean(_sqlRd["InheritVNCProxyIP"]);
					connectionInfo.Inheritance.VNCProxyPort = Convert.ToBoolean(_sqlRd["InheritVNCProxyPort"]);
					connectionInfo.Inheritance.VNCProxyUsername = Convert.ToBoolean(_sqlRd["InheritVNCProxyUsername"]);
					connectionInfo.Inheritance.VNCProxyPassword = Convert.ToBoolean(_sqlRd["InheritVNCProxyPassword"]);
					connectionInfo.Inheritance.VNCColors = Convert.ToBoolean(_sqlRd["InheritVNCColors"]);
					connectionInfo.Inheritance.VNCSmartSizeMode = Convert.ToBoolean(_sqlRd["InheritVNCSmartSizeMode"]);
					connectionInfo.Inheritance.VNCViewOnly = Convert.ToBoolean(_sqlRd["InheritVNCViewOnly"]);
				}
				
				if (_confVersion > 1.7) //1.8
				{
                    connectionInfo.RDPAuthenticationLevel = (ProtocolRDP.AuthenticationLevel)Tools.MiscTools.StringToEnum(typeof(ProtocolRDP.AuthenticationLevel), Convert.ToString(_sqlRd["RDPAuthenticationLevel"]));
					connectionInfo.Inheritance.RDPAuthenticationLevel = Convert.ToBoolean(_sqlRd["InheritRDPAuthenticationLevel"]);
				}
				
				if (_confVersion > 1.8) //1.9
				{
                    connectionInfo.RenderingEngine = (HTTPBase.RenderingEngine)Tools.MiscTools.StringToEnum(typeof(HTTPBase.RenderingEngine), Convert.ToString(_sqlRd["RenderingEngine"]));
					connectionInfo.MacAddress = Convert.ToString(_sqlRd["MacAddress"]);
					connectionInfo.Inheritance.RenderingEngine = Convert.ToBoolean(_sqlRd["InheritRenderingEngine"]);
					connectionInfo.Inheritance.MacAddress = Convert.ToBoolean(_sqlRd["InheritMacAddress"]);
				}
				
				if (_confVersion > 1.9) //2.0
				{
					connectionInfo.UserField = Convert.ToString(_sqlRd["UserField"]);
					connectionInfo.Inheritance.UserField = Convert.ToBoolean(_sqlRd["InheritUserField"]);
				}
				
				if (_confVersion > 2.0) //2.1
				{
					connectionInfo.ExtApp = Convert.ToString(_sqlRd["ExtApp"]);
					connectionInfo.Inheritance.ExtApp = Convert.ToBoolean(_sqlRd["InheritExtApp"]);
				}
						
				if (_confVersion >= 2.2)
				{
                    connectionInfo.RDGatewayUsageMethod = (ProtocolRDP.RDGatewayUsageMethod)Tools.MiscTools.StringToEnum(typeof(ProtocolRDP.RDGatewayUsageMethod), Convert.ToString(_sqlRd["RDGatewayUsageMethod"]));
					connectionInfo.RDGatewayHostname = Convert.ToString(_sqlRd["RDGatewayHostname"]);
                    connectionInfo.RDGatewayUseConnectionCredentials = (ProtocolRDP.RDGatewayUseConnectionCredentials)Tools.MiscTools.StringToEnum(typeof(ProtocolRDP.RDGatewayUseConnectionCredentials), Convert.ToString(_sqlRd["RDGatewayUseConnectionCredentials"]));
					connectionInfo.RDGatewayUsername = Convert.ToString(_sqlRd["RDGatewayUsername"]);
					connectionInfo.RDGatewayPassword = Security.Crypt.Decrypt(Convert.ToString(_sqlRd["RDGatewayPassword"]), _pW);
					connectionInfo.RDGatewayDomain = Convert.ToString(_sqlRd["RDGatewayDomain"]);
					connectionInfo.Inheritance.RDGatewayUsageMethod = Convert.ToBoolean(_sqlRd["InheritRDGatewayUsageMethod"]);
					connectionInfo.Inheritance.RDGatewayHostname = Convert.ToBoolean(_sqlRd["InheritRDGatewayHostname"]);
					connectionInfo.Inheritance.RDGatewayUsername = Convert.ToBoolean(_sqlRd["InheritRDGatewayUsername"]);
					connectionInfo.Inheritance.RDGatewayPassword = Convert.ToBoolean(_sqlRd["InheritRDGatewayPassword"]);
					connectionInfo.Inheritance.RDGatewayDomain = Convert.ToBoolean(_sqlRd["InheritRDGatewayDomain"]);
				}
						
				if (_confVersion >= 2.3)
				{
					connectionInfo.EnableFontSmoothing = Convert.ToBoolean(_sqlRd["EnableFontSmoothing"]);
					connectionInfo.EnableDesktopComposition = Convert.ToBoolean(_sqlRd["EnableDesktopComposition"]);
					connectionInfo.Inheritance.EnableFontSmoothing = Convert.ToBoolean(_sqlRd["InheritEnableFontSmoothing"]);
					connectionInfo.Inheritance.EnableDesktopComposition = Convert.ToBoolean(_sqlRd["InheritEnableDesktopComposition"]);
				}
						
				if (_confVersion >= 2.4)
				{
					connectionInfo.UseCredSsp = Convert.ToBoolean(_sqlRd["UseCredSsp"]);
					connectionInfo.Inheritance.UseCredSsp = Convert.ToBoolean(_sqlRd["InheritUseCredSsp"]);
				}
						
				if (_confVersion >= 2.5)
				{
					connectionInfo.LoadBalanceInfo = Convert.ToString(_sqlRd["LoadBalanceInfo"]);
					connectionInfo.AutomaticResize = Convert.ToBoolean(_sqlRd["AutomaticResize"]);
					connectionInfo.Inheritance.LoadBalanceInfo = Convert.ToBoolean(_sqlRd["InheritLoadBalanceInfo"]);
					connectionInfo.Inheritance.AutomaticResize = Convert.ToBoolean(_sqlRd["InheritAutomaticResize"]);
				}
						
				if (SqlUpdate == true)
				{
					connectionInfo.PleaseConnect = Convert.ToBoolean(_sqlRd["Connected"]);
				}
						
				return connectionInfo;
			}
			catch (Exception ex)
			{
				Runtime.MessageCollector.AddMessage(Messages.MessageClass.ErrorMsg, Language.strGetConnectionInfoFromSqlFailed + Environment.NewLine + ex.Message, true);
			}
					
			return null;
		}
        #endregion
				
        #region XML
		private string DecryptCompleteFile()
		{
			StreamReader sRd = new StreamReader(ConnectionFileName);
					
			string strCons = "";
			strCons = sRd.ReadToEnd();
			sRd.Close();
					
			if (!string.IsNullOrEmpty(strCons))
			{
				string strDecr = "";
				bool notDecr = true;
						
				if (strCons.Contains("<?xml version=\"1.0\" encoding=\"utf-8\"?>"))
				{
					strDecr = strCons;
					return strDecr;
				}
						
				try
				{
					strDecr = Security.Crypt.Decrypt(strCons, _pW);
							
					if (strDecr != strCons)
						notDecr = false;
					else
						notDecr = true;
				}
				catch (Exception)
				{
					notDecr = true;
				}
						
				if (notDecr)
				{
					if (Authenticate(strCons, true) == true)
					{
						strDecr = Security.Crypt.Decrypt(strCons, _pW);
						notDecr = false;
					}
					else
					{
						notDecr = true;
					}
							
					if (notDecr == false)
					{
						return strDecr;
					}
				}
				else
				{
					return strDecr;
				}
			}
					
			return "";
		}
				
		private void LoadFromXml(string cons, bool import)
		{
			try
			{
				if (!import)
				{
					Runtime.IsConnectionsFileLoaded = false;
				}
						
				// SECTION 1. Create a DOM Document and load the XML data into it.
				_xDom = new XmlDocument();
				if (cons != "")
				{
					_xDom.LoadXml(cons);
				}
				else
				{
					_xDom.Load(ConnectionFileName);
				}
						
				if (_xDom.DocumentElement.HasAttribute("ConfVersion"))
				{
					_confVersion = Convert.ToDouble(_xDom.DocumentElement.Attributes["ConfVersion"].Value.Replace(",", "."), CultureInfo.InvariantCulture);
				}
				else
				{
					Runtime.MessageCollector.AddMessage(Messages.MessageClass.WarningMsg, Language.strOldConffile);
				}
						
				const double maxSupportedConfVersion = 2.5;
				if (_confVersion > maxSupportedConfVersion)
				{
                    CTaskDialog.ShowTaskDialogBox(
                        _mainForm,
                        Application.ProductName, 
                        "Incompatible connection file format", 
                        string.Format("The format of this connection file is not supported. Please upgrade to a newer version of {0}.", Application.ProductName), 
                        string.Format("{1}{0}File Format Version: {2}{0}Highest Supported Version: {3}", Environment.NewLine, ConnectionFileName, _confVersion.ToString(), maxSupportedConfVersion.ToString()),
                        "", 
                        "", 
                        "", 
                        "", 
                        ETaskDialogButtons.Ok, 
                        ESysIcons.Error,
                        ESysIcons.Error
                    );
					throw (new Exception(string.Format("Incompatible connection file format (file format version {0}).", _confVersion)));
				}
						
				// SECTION 2. Initialize the treeview control.
				RootNodeInfo rootInfo = default(RootNodeInfo);
				if (import)
				{
					rootInfo = null;
				}
				else
				{
					string rootNodeName = "";
					if (_xDom.DocumentElement.HasAttribute("Name"))
					{
						rootNodeName = Convert.ToString(_xDom.DocumentElement.Attributes["Name"].Value.Trim());
					}
					if (!string.IsNullOrEmpty(rootNodeName))
					{
						RootTreeNode.Name = rootNodeName;
					}
					else
					{
						RootTreeNode.Name = _xDom.DocumentElement.Name;
					}
					RootTreeNode.Text = RootTreeNode.Name;
							
					rootInfo = new RootNodeInfo(RootNodeType.Connection);
					rootInfo.Name = RootTreeNode.Name;
					rootInfo.TreeNode = RootTreeNode;
							
					RootTreeNode.Tag = rootInfo;
				}
						
				if (_confVersion > 1.3) //1.4
				{
					if (Security.Crypt.Decrypt(Convert.ToString(_xDom.DocumentElement.Attributes["Protected"].Value), _pW) != "ThisIsNotProtected")
					{
						if (Authenticate(Convert.ToString(_xDom.DocumentElement.Attributes["Protected"].Value), false, rootInfo) == false)
						{
                            mRemoteNG.Settings.Default.LoadConsFromCustomLocation = false;
                            mRemoteNG.Settings.Default.CustomConsPath = "";
							RootTreeNode.Remove();
							return;
						}
					}
				}
						
				bool isExportFile = false;
				if (_confVersion >= 1.0)
				{
					if (Convert.ToBoolean(_xDom.DocumentElement.Attributes["Export"].Value) == true)
					{
						isExportFile = true;
					}
				}
						
				if (import && !isExportFile)
				{
					Runtime.MessageCollector.AddMessage(Messages.MessageClass.InformationMsg, Language.strCannotImportNormalSessionFile);
					return ;
				}
						
				if (!isExportFile)
				{
                    RootTreeNode.ImageIndex = (int)TreeImageType.Root;
                    RootTreeNode.SelectedImageIndex = (int)TreeImageType.Root;
				}

                Windows.treeForm.tvConnections.BeginUpdate();
						
				// SECTION 3. Populate the TreeView with the DOM nodes.
				AddNodeFromXml(_xDom.DocumentElement, RootTreeNode);
						
				RootTreeNode.Expand();
						
				//expand containers
				foreach (ContainerInfo contI in ContainerList)
				{
					if (contI.IsExpanded == true)
					{
						contI.TreeNode.Expand();
					}
				}

                Windows.treeForm.tvConnections.EndUpdate();
						
				//open connections from last mremote session
				if (mRemoteNG.Settings.Default.OpenConsFromLastSession && !mRemoteNG.Settings.Default.NoReconnect)
				{
					foreach (ConnectionInfo conI in ConnectionList)
					{
						if (conI.PleaseConnect)
						{
							//Runtime.OpenConnection(conI);
                            var runtime = new Runtime(_mainForm);
                            runtime.OpenConnection(conI);
						}
					}
				}
						
				RootTreeNode.EnsureVisible();
						
				if (!import)
				{
                    Runtime.IsConnectionsFileLoaded = true;
				}
                Windows.treeForm.InitialRefresh();
				SetSelectedNode(RootTreeNode);
			}
			catch (Exception ex)
			{
				Runtime.IsConnectionsFileLoaded = false;
				Runtime.MessageCollector.AddMessage(Messages.MessageClass.ErrorMsg, Language.strLoadFromXmlFailed + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace, true);
				throw;
			}
		}
				
		private ContainerInfo _previousContainer;
		private void AddNodeFromXml(XmlNode parentXmlNode, TreeNode parentTreeNode)
		{
			try
			{
				// Loop through the XML nodes until the leaf is reached.
				// Add the nodes to the TreeView during the looping process.
				if (parentXmlNode.HasChildNodes)
				{
					foreach (XmlNode xmlNode in parentXmlNode.ChildNodes)
					{
						TreeNode treeNode = new TreeNode(xmlNode.Attributes["Name"].Value);
						parentTreeNode.Nodes.Add(treeNode);
								
						if (ConnectionTreeNode.GetNodeTypeFromString(xmlNode.Attributes["Type"].Value) == TreeNodeType.Connection) //connection info
						{
							ConnectionInfo connectionInfo = GetConnectionInfoFromXml(xmlNode);
							connectionInfo.TreeNode = treeNode;
							connectionInfo.Parent = _previousContainer; //NEW
									
							ConnectionList.Add(connectionInfo);
									
							treeNode.Tag = connectionInfo;
                            treeNode.ImageIndex = (int)TreeImageType.ConnectionClosed;
                            treeNode.SelectedImageIndex = (int)TreeImageType.ConnectionClosed;
						}
						else if (ConnectionTreeNode.GetNodeTypeFromString(xmlNode.Attributes["Type"].Value) == TreeNodeType.Container) //container info
						{
							ContainerInfo containerInfo = new ContainerInfo();
							if (treeNode.Parent != null)
							{
								if (ConnectionTreeNode.GetNodeType(treeNode.Parent) == TreeNodeType.Container)
								{
									containerInfo.Parent = (ContainerInfo)treeNode.Parent.Tag;
								}
							}
							_previousContainer = containerInfo; //NEW
							containerInfo.TreeNode = treeNode;
									
							containerInfo.Name = xmlNode.Attributes["Name"].Value;
									
							if (_confVersion >= 0.8)
							{
								if (xmlNode.Attributes["Expanded"].Value == "True")
								{
									containerInfo.IsExpanded = true;
								}
								else
								{
									containerInfo.IsExpanded = false;
								}
							}
									
							ConnectionInfo connectionInfo = default(ConnectionInfo);
							if (_confVersion >= 0.9)
							{
								connectionInfo = GetConnectionInfoFromXml(xmlNode);
							}
							else
							{
								connectionInfo = new ConnectionInfo();
							}
									
							connectionInfo.Parent = containerInfo;
							connectionInfo.IsContainer = true;
							containerInfo.ConnectionInfo = connectionInfo;
									
							ContainerList.Add(containerInfo);
									
							treeNode.Tag = containerInfo;
                            treeNode.ImageIndex = (int)TreeImageType.Container;
                            treeNode.SelectedImageIndex = (int)TreeImageType.Container;
						}
								
						AddNodeFromXml(xmlNode, treeNode);
					}
				}
				else
				{
					string nodeName = "";
					XmlAttribute nameAttribute = parentXmlNode.Attributes["Name"];
					if (!(nameAttribute == null))
					{
						nodeName = nameAttribute.Value.Trim();
					}
					if (!string.IsNullOrEmpty(nodeName))
					{
						parentTreeNode.Text = nodeName;
					}
					else
					{
						parentTreeNode.Text = parentXmlNode.Name;
					}
				}
			}
			catch (Exception ex)
			{
				Runtime.MessageCollector.AddMessage(Messages.MessageClass.ErrorMsg, Language.strAddNodeFromXmlFailed + Environment.NewLine + ex.Message + ex.StackTrace, true);
				throw;
			}
		}
				
		private ConnectionInfo GetConnectionInfoFromXml(XmlNode xxNode)
		{
			ConnectionInfo connectionInfo = new ConnectionInfo();
			try
			{
				XmlNode xmlnode = xxNode;
				if (_confVersion > 0.1) //0.2
				{
					connectionInfo.Name = xmlnode.Attributes["Name"].Value;
					connectionInfo.Description = xmlnode.Attributes["Descr"].Value;
					connectionInfo.Hostname = xmlnode.Attributes["Hostname"].Value;
					connectionInfo.Username = xmlnode.Attributes["Username"].Value;
					connectionInfo.Password = Security.Crypt.Decrypt(xmlnode.Attributes["Password"].Value, _pW);
					connectionInfo.Domain = xmlnode.Attributes["Domain"].Value;
					connectionInfo.DisplayWallpaper = bool.Parse(xmlnode.Attributes["DisplayWallpaper"].Value);
					connectionInfo.DisplayThemes = bool.Parse(xmlnode.Attributes["DisplayThemes"].Value);
					connectionInfo.CacheBitmaps = bool.Parse(xmlnode.Attributes["CacheBitmaps"].Value);
							
					if (_confVersion < 1.1) //1.0 - 0.1
					{
						if (Convert.ToBoolean(xmlnode.Attributes["Fullscreen"].Value) == true)
						{
							connectionInfo.Resolution = ProtocolRDP.RDPResolutions.Fullscreen;
						}
						else
						{
							connectionInfo.Resolution = ProtocolRDP.RDPResolutions.FitToWindow;
						}
					}
				}
						
				if (_confVersion > 0.2) //0.3
				{
					if (_confVersion < 0.7)
					{
						if (Convert.ToBoolean(xmlnode.Attributes["UseVNC"].Value) == true)
						{
							connectionInfo.Protocol = ProtocolType.VNC;
							connectionInfo.Port = Convert.ToInt32(xmlnode.Attributes["VNCPort"].Value);
						}
						else
						{
							connectionInfo.Protocol = ProtocolType.RDP;
						}
					}
				}
				else
				{
					connectionInfo.Port = (int)ProtocolRDP.Defaults.Port;
					connectionInfo.Protocol = ProtocolType.RDP;
				}
						
				if (_confVersion > 0.3) //0.4
				{
					if (_confVersion < 0.7)
					{
						if (Convert.ToBoolean(xmlnode.Attributes["UseVNC"].Value) == true)
                            connectionInfo.Port = Convert.ToInt32(xmlnode.Attributes["VNCPort"].Value);
						else
                            connectionInfo.Port = Convert.ToInt32(xmlnode.Attributes["RDPPort"].Value);
					}
							
					connectionInfo.UseConsoleSession = bool.Parse(xmlnode.Attributes["ConnectToConsole"].Value);
				}
				else
				{
					if (_confVersion < 0.7)
					{
						if (Convert.ToBoolean(xmlnode.Attributes["UseVNC"].Value) == true)
							connectionInfo.Port = (int)ProtocolVNC.Defaults.Port;
						else
							connectionInfo.Port = (int)ProtocolRDP.Defaults.Port;
					}
					connectionInfo.UseConsoleSession = false;
				}
				
				if (_confVersion > 0.4) //0.5 and 0.6
				{
					connectionInfo.RedirectDiskDrives = bool.Parse(xmlnode.Attributes["RedirectDiskDrives"].Value);
					connectionInfo.RedirectPrinters = bool.Parse(xmlnode.Attributes["RedirectPrinters"].Value);
					connectionInfo.RedirectPorts = bool.Parse(xmlnode.Attributes["RedirectPorts"].Value);
					connectionInfo.RedirectSmartCards = bool.Parse(xmlnode.Attributes["RedirectSmartCards"].Value);
				}
				else
				{
					connectionInfo.RedirectDiskDrives = false;
					connectionInfo.RedirectPrinters = false;
					connectionInfo.RedirectPorts = false;
					connectionInfo.RedirectSmartCards = false;
				}
				
				if (_confVersion > 0.6) //0.7
				{
                    connectionInfo.Protocol = (ProtocolType)Tools.MiscTools.StringToEnum(typeof(ProtocolType), xmlnode.Attributes["Protocol"].Value);
                    connectionInfo.Port = Convert.ToInt32(xmlnode.Attributes["Port"].Value);
				}
				
				if (_confVersion > 0.9) //1.0
				{
					connectionInfo.RedirectKeys = bool.Parse(xmlnode.Attributes["RedirectKeys"].Value);
				}
				
				if (_confVersion > 1.1) //1.2
				{
					connectionInfo.PuttySession = xmlnode.Attributes["PuttySession"].Value;
				}
				
				if (_confVersion > 1.2) //1.3
				{
                    connectionInfo.Colors = (ProtocolRDP.RDPColors)Tools.MiscTools.StringToEnum(typeof(ProtocolRDP.RDPColors), xmlnode.Attributes["Colors"].Value);
                    connectionInfo.Resolution = (ProtocolRDP.RDPResolutions)Tools.MiscTools.StringToEnum(typeof(ProtocolRDP.RDPResolutions), Convert.ToString(xmlnode.Attributes["Resolution"].Value));
                    connectionInfo.RedirectSound = (ProtocolRDP.RDPSounds)Tools.MiscTools.StringToEnum(typeof(ProtocolRDP.RDPSounds), Convert.ToString(xmlnode.Attributes["RedirectSound"].Value));
				}
				else
				{
					switch (Convert.ToInt32(xmlnode.Attributes["Colors"].Value))
					{
						case 0:
							connectionInfo.Colors = ProtocolRDP.RDPColors.Colors256;
							break;
						case 1:
							connectionInfo.Colors = ProtocolRDP.RDPColors.Colors16Bit;
							break;
						case 2:
							connectionInfo.Colors = ProtocolRDP.RDPColors.Colors24Bit;
							break;
						case 3:
							connectionInfo.Colors = ProtocolRDP.RDPColors.Colors32Bit;
							break;
						case 4:
							connectionInfo.Colors = ProtocolRDP.RDPColors.Colors15Bit;
							break;
					}
							
					connectionInfo.RedirectSound = (ProtocolRDP.RDPSounds) Convert.ToInt32(xmlnode.Attributes["RedirectSound"].Value);
				}
				
				if (_confVersion > 1.2) //1.3
				{
					connectionInfo.Inheritance = new ConnectionInfoInheritance(connectionInfo);
					connectionInfo.Inheritance.CacheBitmaps = bool.Parse(xmlnode.Attributes["InheritCacheBitmaps"].Value);
					connectionInfo.Inheritance.Colors = bool.Parse(xmlnode.Attributes["InheritColors"].Value);
					connectionInfo.Inheritance.Description = bool.Parse(xmlnode.Attributes["InheritDescription"].Value);
					connectionInfo.Inheritance.DisplayThemes = bool.Parse(xmlnode.Attributes["InheritDisplayThemes"].Value);
					connectionInfo.Inheritance.DisplayWallpaper = bool.Parse(xmlnode.Attributes["InheritDisplayWallpaper"].Value);
					connectionInfo.Inheritance.Domain = bool.Parse(xmlnode.Attributes["InheritDomain"].Value);
					connectionInfo.Inheritance.Icon = bool.Parse(xmlnode.Attributes["InheritIcon"].Value);
					connectionInfo.Inheritance.Panel = bool.Parse(xmlnode.Attributes["InheritPanel"].Value);
					connectionInfo.Inheritance.Password = bool.Parse(xmlnode.Attributes["InheritPassword"].Value);
					connectionInfo.Inheritance.Port = bool.Parse(xmlnode.Attributes["InheritPort"].Value);
					connectionInfo.Inheritance.Protocol = bool.Parse(xmlnode.Attributes["InheritProtocol"].Value);
					connectionInfo.Inheritance.PuttySession = bool.Parse(xmlnode.Attributes["InheritPuttySession"].Value);
					connectionInfo.Inheritance.RedirectDiskDrives = bool.Parse(xmlnode.Attributes["InheritRedirectDiskDrives"].Value);
					connectionInfo.Inheritance.RedirectKeys = bool.Parse(xmlnode.Attributes["InheritRedirectKeys"].Value);
					connectionInfo.Inheritance.RedirectPorts = bool.Parse(xmlnode.Attributes["InheritRedirectPorts"].Value);
					connectionInfo.Inheritance.RedirectPrinters = bool.Parse(xmlnode.Attributes["InheritRedirectPrinters"].Value);
					connectionInfo.Inheritance.RedirectSmartCards = bool.Parse(xmlnode.Attributes["InheritRedirectSmartCards"].Value);
					connectionInfo.Inheritance.RedirectSound = bool.Parse(xmlnode.Attributes["InheritRedirectSound"].Value);
					connectionInfo.Inheritance.Resolution = bool.Parse(xmlnode.Attributes["InheritResolution"].Value);
					connectionInfo.Inheritance.UseConsoleSession = bool.Parse(xmlnode.Attributes["InheritUseConsoleSession"].Value);
					connectionInfo.Inheritance.Username = bool.Parse(xmlnode.Attributes["InheritUsername"].Value);
					connectionInfo.Icon = xmlnode.Attributes["Icon"].Value;
					connectionInfo.Panel = xmlnode.Attributes["Panel"].Value;
				}
				else
				{
                    connectionInfo.Inheritance = new ConnectionInfoInheritance(connectionInfo, Convert.ToBoolean(xmlnode.Attributes["Inherit"].Value));
					connectionInfo.Icon = Convert.ToString(xmlnode.Attributes["Icon"].Value.Replace(".ico", ""));
					connectionInfo.Panel = Language.strGeneral;
				}
				
				if (_confVersion > 1.4) //1.5
				{
					connectionInfo.PleaseConnect = bool.Parse(xmlnode.Attributes["Connected"].Value);
				}
				
				if (_confVersion > 1.5) //1.6
				{
                    connectionInfo.ICAEncryption = (ProtocolICA.EncryptionStrength)Tools.MiscTools.StringToEnum(typeof(ProtocolICA.EncryptionStrength), xmlnode.Attributes["ICAEncryptionStrength"].Value);
					connectionInfo.Inheritance.ICAEncryption = bool.Parse(xmlnode.Attributes["InheritICAEncryptionStrength"].Value);
					connectionInfo.PreExtApp = xmlnode.Attributes["PreExtApp"].Value;
					connectionInfo.PostExtApp = xmlnode.Attributes["PostExtApp"].Value;
					connectionInfo.Inheritance.PreExtApp = bool.Parse(xmlnode.Attributes["InheritPreExtApp"].Value);
					connectionInfo.Inheritance.PostExtApp = bool.Parse(xmlnode.Attributes["InheritPostExtApp"].Value);
				}
				
				if (_confVersion > 1.6) //1.7
				{
                    connectionInfo.VNCCompression = (ProtocolVNC.Compression)Tools.MiscTools.StringToEnum(typeof(ProtocolVNC.Compression), xmlnode.Attributes["VNCCompression"].Value);
                    connectionInfo.VNCEncoding = (ProtocolVNC.Encoding)Tools.MiscTools.StringToEnum(typeof(ProtocolVNC.Encoding), Convert.ToString(xmlnode.Attributes["VNCEncoding"].Value));
                    connectionInfo.VNCAuthMode = (ProtocolVNC.AuthMode)Tools.MiscTools.StringToEnum(typeof(ProtocolVNC.AuthMode), xmlnode.Attributes["VNCAuthMode"].Value);
                    connectionInfo.VNCProxyType = (ProtocolVNC.ProxyType)Tools.MiscTools.StringToEnum(typeof(ProtocolVNC.ProxyType), xmlnode.Attributes["VNCProxyType"].Value);
					connectionInfo.VNCProxyIP = xmlnode.Attributes["VNCProxyIP"].Value;
                    connectionInfo.VNCProxyPort = Convert.ToInt32(xmlnode.Attributes["VNCProxyPort"].Value);
					connectionInfo.VNCProxyUsername = xmlnode.Attributes["VNCProxyUsername"].Value;
					connectionInfo.VNCProxyPassword = Security.Crypt.Decrypt(xmlnode.Attributes["VNCProxyPassword"].Value, _pW);
                    connectionInfo.VNCColors = (ProtocolVNC.Colors)Tools.MiscTools.StringToEnum(typeof(ProtocolVNC.Colors), xmlnode.Attributes["VNCColors"].Value);
                    connectionInfo.VNCSmartSizeMode = (ProtocolVNC.SmartSizeMode)Tools.MiscTools.StringToEnum(typeof(ProtocolVNC.SmartSizeMode), xmlnode.Attributes["VNCSmartSizeMode"].Value);
					connectionInfo.VNCViewOnly = bool.Parse(xmlnode.Attributes["VNCViewOnly"].Value);
					connectionInfo.Inheritance.VNCCompression = bool.Parse(xmlnode.Attributes["InheritVNCCompression"].Value);
					connectionInfo.Inheritance.VNCEncoding = bool.Parse(xmlnode.Attributes["InheritVNCEncoding"].Value);
					connectionInfo.Inheritance.VNCAuthMode = bool.Parse(xmlnode.Attributes["InheritVNCAuthMode"].Value);
					connectionInfo.Inheritance.VNCProxyType = bool.Parse(xmlnode.Attributes["InheritVNCProxyType"].Value);
					connectionInfo.Inheritance.VNCProxyIP = bool.Parse(xmlnode.Attributes["InheritVNCProxyIP"].Value);
					connectionInfo.Inheritance.VNCProxyPort = bool.Parse(xmlnode.Attributes["InheritVNCProxyPort"].Value);
					connectionInfo.Inheritance.VNCProxyUsername = bool.Parse(xmlnode.Attributes["InheritVNCProxyUsername"].Value);
					connectionInfo.Inheritance.VNCProxyPassword = bool.Parse(xmlnode.Attributes["InheritVNCProxyPassword"].Value);
					connectionInfo.Inheritance.VNCColors = bool.Parse(xmlnode.Attributes["InheritVNCColors"].Value);
					connectionInfo.Inheritance.VNCSmartSizeMode = bool.Parse(xmlnode.Attributes["InheritVNCSmartSizeMode"].Value);
					connectionInfo.Inheritance.VNCViewOnly = bool.Parse(xmlnode.Attributes["InheritVNCViewOnly"].Value);
				}
				
				if (_confVersion > 1.7) //1.8
				{
                    connectionInfo.RDPAuthenticationLevel = (ProtocolRDP.AuthenticationLevel)Tools.MiscTools.StringToEnum(typeof(ProtocolRDP.AuthenticationLevel), xmlnode.Attributes["RDPAuthenticationLevel"].Value);
					connectionInfo.Inheritance.RDPAuthenticationLevel = bool.Parse(xmlnode.Attributes["InheritRDPAuthenticationLevel"].Value);
				}
				
				if (_confVersion > 1.8) //1.9
				{
                    connectionInfo.RenderingEngine = (HTTPBase.RenderingEngine)Tools.MiscTools.StringToEnum(typeof(HTTPBase.RenderingEngine), xmlnode.Attributes["RenderingEngine"].Value);
					connectionInfo.MacAddress = xmlnode.Attributes["MacAddress"].Value;
					connectionInfo.Inheritance.RenderingEngine = bool.Parse(xmlnode.Attributes["InheritRenderingEngine"].Value);
					connectionInfo.Inheritance.MacAddress = bool.Parse(xmlnode.Attributes["InheritMacAddress"].Value);
				}
				
				if (_confVersion > 1.9) //2.0
				{
					connectionInfo.UserField = xmlnode.Attributes["UserField"].Value;
					connectionInfo.Inheritance.UserField = bool.Parse(xmlnode.Attributes["InheritUserField"].Value);
				}
				
				if (_confVersion > 2.0) //2.1
				{
					connectionInfo.ExtApp = xmlnode.Attributes["ExtApp"].Value;
					connectionInfo.Inheritance.ExtApp = bool.Parse(xmlnode.Attributes["InheritExtApp"].Value);
				}
				
				if (_confVersion > 2.1) //2.2
				{
					// Get settings
                    connectionInfo.RDGatewayUsageMethod = (ProtocolRDP.RDGatewayUsageMethod)Tools.MiscTools.StringToEnum(typeof(ProtocolRDP.RDGatewayUsageMethod), Convert.ToString(xmlnode.Attributes["RDGatewayUsageMethod"].Value));
					connectionInfo.RDGatewayHostname = xmlnode.Attributes["RDGatewayHostname"].Value;
                    connectionInfo.RDGatewayUseConnectionCredentials = (ProtocolRDP.RDGatewayUseConnectionCredentials)Tools.MiscTools.StringToEnum(typeof(ProtocolRDP.RDGatewayUseConnectionCredentials), Convert.ToString(xmlnode.Attributes["RDGatewayUseConnectionCredentials"].Value));
					connectionInfo.RDGatewayUsername = xmlnode.Attributes["RDGatewayUsername"].Value;
					connectionInfo.RDGatewayPassword = Security.Crypt.Decrypt(Convert.ToString(xmlnode.Attributes["RDGatewayPassword"].Value), _pW);
					connectionInfo.RDGatewayDomain = xmlnode.Attributes["RDGatewayDomain"].Value;
							
					// Get inheritance settings
					connectionInfo.Inheritance.RDGatewayUsageMethod = bool.Parse(xmlnode.Attributes["InheritRDGatewayUsageMethod"].Value);
					connectionInfo.Inheritance.RDGatewayHostname = bool.Parse(xmlnode.Attributes["InheritRDGatewayHostname"].Value);
					connectionInfo.Inheritance.RDGatewayUseConnectionCredentials = bool.Parse(xmlnode.Attributes["InheritRDGatewayUseConnectionCredentials"].Value);
					connectionInfo.Inheritance.RDGatewayUsername = bool.Parse(xmlnode.Attributes["InheritRDGatewayUsername"].Value);
					connectionInfo.Inheritance.RDGatewayPassword = bool.Parse(xmlnode.Attributes["InheritRDGatewayPassword"].Value);
					connectionInfo.Inheritance.RDGatewayDomain = bool.Parse(xmlnode.Attributes["InheritRDGatewayDomain"].Value);
				}
				
				if (_confVersion > 2.2) //2.3
				{
					// Get settings
					connectionInfo.EnableFontSmoothing = bool.Parse(xmlnode.Attributes["EnableFontSmoothing"].Value);
					connectionInfo.EnableDesktopComposition = bool.Parse(xmlnode.Attributes["EnableDesktopComposition"].Value);
							
					// Get inheritance settings
					connectionInfo.Inheritance.EnableFontSmoothing = bool.Parse(xmlnode.Attributes["InheritEnableFontSmoothing"].Value);
					connectionInfo.Inheritance.EnableDesktopComposition = bool.Parse(xmlnode.Attributes["InheritEnableDesktopComposition"].Value);
				}
				
				if (_confVersion >= 2.4)
				{
					connectionInfo.UseCredSsp = bool.Parse(xmlnode.Attributes["UseCredSsp"].Value);
					connectionInfo.Inheritance.UseCredSsp = bool.Parse(xmlnode.Attributes["InheritUseCredSsp"].Value);
				}
				
				if (_confVersion >= 2.5)
				{
					connectionInfo.LoadBalanceInfo = xmlnode.Attributes["LoadBalanceInfo"].Value;
					connectionInfo.AutomaticResize = bool.Parse(xmlnode.Attributes["AutomaticResize"].Value);
					connectionInfo.Inheritance.LoadBalanceInfo = bool.Parse(xmlnode.Attributes["InheritLoadBalanceInfo"].Value);
					connectionInfo.Inheritance.AutomaticResize = bool.Parse(xmlnode.Attributes["InheritAutomaticResize"].Value);
				}
			}
			catch (Exception ex)
			{
				Runtime.MessageCollector.AddMessage(Messages.MessageClass.ErrorMsg, string.Format(Language.strGetConnectionInfoFromXmlFailed, connectionInfo.Name, ConnectionFileName, ex.Message), false);
			}
			return connectionInfo;
		}
				
		private bool Authenticate(string value, bool compareToOriginalValue, RootNodeInfo rootInfo = null)
		{
			string passwordName = "";
			if (UseSql)
			{
				passwordName = Language.strSQLServer.TrimEnd(':');
			}
			else
			{
				passwordName = Path.GetFileName(ConnectionFileName);
			}
					
			if (compareToOriginalValue)
			{
				while (!(Security.Crypt.Decrypt(value, _pW) != value))
				{
					_pW = Tools.MiscTools.PasswordDialog(passwordName, false);
							
					if (string.IsNullOrEmpty(_pW))
					{
						return false;
					}
				}
			}
			else
			{
				while (!(Security.Crypt.Decrypt(value, _pW) == "ThisIsProtected"))
				{
					_pW = Tools.MiscTools.PasswordDialog(passwordName, false);
							
					if (string.IsNullOrEmpty(_pW))
					{
						return false;
					}
				}
						
				if (rootInfo != null)
				{
					rootInfo.Password = true;
					rootInfo.PasswordString = _pW;
				}
			}
					
			return true;
		}
        #endregion
	}
}