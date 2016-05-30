using mRemoteNG.App;
using mRemoteNG.Connection.Protocol;
using mRemoteNG.Images;
using mRemoteNG.Messages;
using mRemoteNG.Tools;
using mRemoteNG.Tree;
using mRemoteNG.UI.Forms;
using mRemoteNG.UI.Window;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TabPage = Crownwood.Magic.Controls.TabPage;

namespace mRemoteNG.Connection
{
    public class ConnectionInitiator
    {
        public ConnectionInitiator()
        {
        }

        public void InitiateConnection(ConnectionTreeNode connectionTreeNode = null, ConnectionInfo.Force Force = ConnectionInfo.Force.None, Form connectionForm = null)
        {
            if (connectionTreeNode.GetNodeType() == TreeNodeType.Connection || connectionTreeNode.GetNodeType() == TreeNodeType.PuttySession)
            {
                InitiateConnection(connectionTreeNode.ConnectionInfo, Force, connectionForm);
            }
            else if (connectionTreeNode.GetNodeType() == TreeNodeType.Container)
            {
                InitiateConnection(connectionTreeNode.GetChildNodesOfType(TreeNodeType.Connection), Force, connectionForm);
            }
        }

        public void InitiateConnection(List<ConnectionTreeNode> ConnectionTreeNodes = null, ConnectionInfo.Force Force = ConnectionInfo.Force.None, Form ConnectionForm = null)
        {
            foreach (ConnectionTreeNode treeNode in ConnectionTreeNodes)
            {
                InitiateConnection(treeNode.ConnectionInfo, Force, ConnectionForm);
            }
        }

        public void InitiateConnection(ConnectionInfo connectionInfo = null, ConnectionInfo.Force Force = ConnectionInfo.Force.None, Form ConnectionForm = null)
        {
            try
            {
                if (connectionInfo.Hostname == "" && connectionInfo.Protocol != ProtocolType.IntApp)
                {
                    Runtime.MessageCollector.AddMessage(MessageClass.WarningMsg, Language.strConnectionOpenFailedNoHostname);
                    return;
                }

                StartPreConnectionExternalApp(connectionInfo);

                if ((Force & ConnectionInfo.Force.DoNotJump) != ConnectionInfo.Force.DoNotJump)
                {
                    if (Runtime.SwitchToOpenConnection(connectionInfo))
                    {
                        return;
                    }
                }

                ProtocolFactory protocolFactory = new ProtocolFactory();
                ProtocolBase newProtocol = protocolFactory.CreateProtocol(connectionInfo);

                string connectionPanel = SetConnectionPanel(connectionInfo, Force);
                Form connectionForm = SetConnectionForm(ConnectionForm, connectionPanel);
                Control connectionContainer = SetConnectionContainer(connectionInfo, connectionForm);
                SetConnectionFormEventHandlers(newProtocol, connectionForm);
                SetConnectionEventHandlers(newProtocol);
                BuildConnectionInterfaceController(connectionInfo, newProtocol, connectionContainer);

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

                connectionInfo.OpenConnections.Add(newProtocol);
                SetTreeNodeImages(connectionInfo);
                frmMain.Default.SelectedConnection = connectionInfo;
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