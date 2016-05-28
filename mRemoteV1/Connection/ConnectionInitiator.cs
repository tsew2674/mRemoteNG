using mRemoteNG.App;
using mRemoteNG.Connection.Protocol;
using mRemoteNG.Images;
using mRemoteNG.Messages;
using mRemoteNG.Tools;
using mRemoteNG.Tree;
using mRemoteNG.UI.Forms;
using mRemoteNG.UI.Window;
using System;
using System.Windows.Forms;
using TabPage = Crownwood.Magic.Controls.TabPage;

namespace mRemoteNG.Connection
{
    public class ConnectionInitiator
    {
        private ConnectionInfo _connectionInfo = null;

        public ConnectionInfo ConnectionInfo
        {
            get
            {
                if (_connectionInfo == null)
                    _connectionInfo = (ConnectionInfo)ConnectionTree.Instance.SelectedNode.Tag;
                return _connectionInfo;
            }
            set
            {
                _connectionInfo = value;
            }
        }

        public ConnectionInfo.Force Force { get; set; }
        public Form ConnectionForm { get; set; }


        public ConnectionInitiator(ConnectionInfo connectionInfo = null, ConnectionInfo.Force force = ConnectionInfo.Force.None, Form connectionForm = null)
        {
            _connectionInfo = connectionInfo;
            Force = force;
            ConnectionForm = connectionForm;
        }


        public void InitiateConnection()
        {
            try
            {
                if (_connectionInfo.Hostname == "" && _connectionInfo.Protocol != ProtocolType.IntApp)
                {
                    Runtime.MessageCollector.AddMessage(MessageClass.WarningMsg, Language.strConnectionOpenFailedNoHostname);
                    return;
                }

                StartPreConnectionExternalApp(_connectionInfo);

                if ((Force & ConnectionInfo.Force.DoNotJump) != ConnectionInfo.Force.DoNotJump)
                {
                    if (Runtime.SwitchToOpenConnection(_connectionInfo))
                    {
                        return;
                    }
                }

                ProtocolFactory protocolFactory = new ProtocolFactory();
                ProtocolBase newProtocol = protocolFactory.CreateProtocol(_connectionInfo);

                string connectionPanel = SetConnectionPanel(_connectionInfo, Force);
                Form connectionForm = SetConnectionForm(ConnectionForm, connectionPanel);
                Control connectionContainer = SetConnectionContainer(_connectionInfo, connectionForm);
                SetConnectionFormEventHandlers(newProtocol, connectionForm);
                SetConnectionEventHandlers(newProtocol);
                BuildConnectionInterfaceController(_connectionInfo, newProtocol, connectionContainer);

                newProtocol.Force = Force;

                if (newProtocol.Initialize() == false)
                {
                    newProtocol.Close();
                    return;
                }

                if (newProtocol.Connect() == false)
                {
                    newProtocol.Close();
                    return;
                }

                _connectionInfo.OpenConnections.Add(newProtocol);
                SetTreeNodeImages(_connectionInfo);
                frmMain.Default.SelectedConnection = _connectionInfo;
            }
            catch (Exception ex)
            {
                Runtime.MessageCollector.AddMessage(MessageClass.ErrorMsg, Language.strConnectionOpenFailed + Environment.NewLine + ex.Message);
            }
        }

        private void StartPreConnectionExternalApp(ConnectionInfo ConnectionInfo)
        {
            if (ConnectionInfo.PreExtApp != "")
            {
                ExternalTool extA = Runtime.GetExtAppByName(ConnectionInfo.PreExtApp);
                if (extA != null)
                {
                    extA.Start(ConnectionInfo);
                }
            }
        }

        private string SetConnectionPanel(ConnectionInfo ConnectionInfo, ConnectionInfo.Force Force)
        {
            string connectionPanel = "";
            if (ConnectionInfo.Panel == "" || (Force & ConnectionInfo.Force.OverridePanel) == ConnectionInfo.Force.OverridePanel | Settings.Default.AlwaysShowPanelSelectionDlg)
            {
                frmChoosePanel frmPnl = new frmChoosePanel();
                if (frmPnl.ShowDialog() == DialogResult.OK)
                {
                    connectionPanel = frmPnl.Panel;
                }
            }
            else
            {
                connectionPanel = ConnectionInfo.Panel;
            }
            return connectionPanel;
        }

        private Form SetConnectionForm(Form ConForm, string connectionPanel)
        {
            Form connectionForm = default(Form);
            if (ConForm == null)
                connectionForm = Runtime.WindowList.FromString(connectionPanel);
            else
                connectionForm = ConForm;

            if (connectionForm == null)
                connectionForm = Runtime.AddPanel(connectionPanel);
            else
                ((ConnectionWindow)connectionForm).Show(frmMain.Default.pnlDock);

            connectionForm.Focus();
            return connectionForm;
        }

        private Control SetConnectionContainer(ConnectionInfo ConnectionInfo, Form connectionForm)
        {
            Control connectionContainer = default(Control);
            connectionContainer = ((ConnectionWindow)connectionForm).AddConnectionTab(ConnectionInfo);

            if (ConnectionInfo.Protocol == ProtocolType.IntApp)
            {
                if (Runtime.GetExtAppByName(ConnectionInfo.ExtApp).Icon != null)
                    ((TabPage) connectionContainer).Icon = Runtime.GetExtAppByName(ConnectionInfo.ExtApp).Icon;
            }
            return connectionContainer;
        }

        private void SetConnectionFormEventHandlers(ProtocolBase newProtocol, Form connectionForm)
        {
            newProtocol.Closed += ((ConnectionWindow)connectionForm).Prot_Event_Closed;
        }

        private void SetConnectionEventHandlers(ProtocolBase newProtocol)
        {
            newProtocol.Disconnected += Runtime.Prot_Event_Disconnected;
            newProtocol.Connected += Runtime.Prot_Event_Connected;
            newProtocol.Closed += Runtime.Prot_Event_Closed;
            newProtocol.ErrorOccured += Runtime.Prot_Event_ErrorOccured;
        }

        private void SetTreeNodeImages(ConnectionInfo ConnectionInfo)
        {
            if (ConnectionInfo.IsQuickConnect == false)
            {
                if (ConnectionInfo.Protocol != ProtocolType.IntApp)
                {
                    ConnectionTreeNode.SetNodeImage(ConnectionInfo.TreeNode, TreeImageType.ConnectionOpen);
                }
                else
                {
                    ExternalTool extApp = Runtime.GetExtAppByName(ConnectionInfo.ExtApp);
                    if (extApp != null)
                    {
                        if (extApp.TryIntegrate && ConnectionInfo.TreeNode != null)
                        {
                            ConnectionTreeNode.SetNodeImage(ConnectionInfo.TreeNode, TreeImageType.ConnectionOpen);
                        }
                    }
                }
            }
        }

        private void BuildConnectionInterfaceController(ConnectionInfo ConnectionInfo, ProtocolBase newProtocol, Control connectionContainer)
        {
            newProtocol.InterfaceControl = new InterfaceControl(connectionContainer, newProtocol, ConnectionInfo);
        }
    }
}