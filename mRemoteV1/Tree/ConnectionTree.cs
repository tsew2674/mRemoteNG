using mRemoteNG.App;
using mRemoteNG.Connection;
using mRemoteNG.Messages;
using mRemoteNG.Tools.Sorting;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace mRemoteNG.Tree
{
    public class ConnectionTree : TreeView
    {
        private static readonly ConnectionTree _instance = new ConnectionTree();
        private static TreeNode SetNodeToolTip_old_node;
        private static TreeNode treeNodeToBeSelected;


        public static ConnectionTree Instance
        {
            get { return _instance; }
        }

        private ConnectionTree()
        {
        }

        static ConnectionTree()
        { }

        public void DeleteSelectedNode()
        {
            try
            {
                if (!SelectedNodeIsAValidDeletionTarget())
                    return;

                if (ConnectionTreeNode.GetNodeType(SelectedNode) == TreeNodeType.Container)
                {
                    if (ConnectionTreeNode.IsEmpty(SelectedNode))
                    {
                        if (UserConfirmsEmptyFolderDeletion())
                            SelectedNode.Remove();
                    }
                    else
                    {
                        if (UserConfirmsNonEmptyFolderDeletion())
                        {
                            BeginUpdate();
                            SelectedNode.Nodes.Clear();
                            SelectedNode.Remove();
                            EndUpdate();
                        }
                    }
                }
                else if (ConnectionTreeNode.GetNodeType(SelectedNode) == TreeNodeType.Connection)
                {
                    if (UserConfirmsConnectionDeletion())
                        SelectedNode.Remove();
                }
                else
                {
                    Runtime.MessageCollector.AddMessage(MessageClass.WarningMsg, "Tree item type is unknown so it cannot be deleted!");
                }
            }
            catch (Exception ex)
            {
                Runtime.MessageCollector.AddMessage(MessageClass.ErrorMsg, "Deleting selected node failed" + Environment.NewLine + ex.Message, true);
            }
        }

        private bool SelectedNodeIsAValidDeletionTarget()
        {
            bool validDeletionTarget = true;
            if (SelectedNode == null)
                validDeletionTarget = false;
            else if (ConnectionTreeNode.GetNodeType(SelectedNode) == TreeNodeType.Root)
            {
                validDeletionTarget = false;
                Runtime.MessageCollector.AddMessage(MessageClass.WarningMsg, "The root item cannot be deleted!");
            }
            return validDeletionTarget;
        }

        private bool UserConfirmsEmptyFolderDeletion()
        {
            string messagePrompt = string.Format(Language.strConfirmDeleteNodeFolder, SelectedNode.Text);
            return PromptUser(messagePrompt);
        }

        private bool UserConfirmsNonEmptyFolderDeletion()
        {
            string messagePrompt = string.Format(Language.strConfirmDeleteNodeFolderNotEmpty, SelectedNode.Text);
            return PromptUser(messagePrompt);
        }

        private bool UserConfirmsConnectionDeletion()
        {
            string messagePrompt = string.Format(Language.strConfirmDeleteNodeConnection, SelectedNode.Text);
            return PromptUser(messagePrompt);
        }

        private bool PromptUser(string PromptMessage)
        {
            DialogResult msgBoxResponse = MessageBox.Show(PromptMessage, Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            return (msgBoxResponse == DialogResult.Yes);
        }

        public void StartRenameSelectedNode()
        {
            SelectedNode?.BeginEdit();
        }

        public void FinishRenameSelectedNode(string newName)
        {
            ConnectionInfo connectionInfo = SelectedNode.Tag as ConnectionInfo;
            if (connectionInfo != null)
                ConnectionTreeNode.RenameNode(connectionInfo, newName);
        }

        public void SetNodeToolTip(MouseEventArgs e, ToolTip tTip)
        {
            try
            {
                if (!Settings.Default.ShowDescriptionTooltipsInTree) return;
                //Find the node under the mouse.
                TreeNode new_node = GetNodeAt(e.X, e.Y);
                if (new_node == null || new_node.Equals(SetNodeToolTip_old_node))
                {
                    return;
                }
                SetNodeToolTip_old_node = new_node;

                //See if we have a node.
                if (SetNodeToolTip_old_node == null)
                {
                    tTip.SetToolTip(this, "");
                }
                else
                {
                    //Get this node's object data.
                    if (ConnectionTreeNode.GetNodeType(SetNodeToolTip_old_node) == TreeNodeType.Connection)
                    {
                        tTip.SetToolTip(this, ((ConnectionInfo) SetNodeToolTip_old_node.Tag).Description);
                    }
                }
            }
            catch (Exception ex)
            {
                Runtime.MessageCollector.AddMessage(MessageClass.ErrorMsg, "SetNodeToolTip failed" + Environment.NewLine + ex.Message, true);
            }
        }

        public void ExpandAllNodes()
        {
            BeginUpdate();
            ExpandAll();
            EndUpdate();
        }

        public void CollapseAllNodes()
        {
            BeginUpdate();
            foreach (TreeNode treeNode in Nodes[0].Nodes)
            {
                treeNode.Collapse(false);
            }
            EndUpdate();
        }

        public void MoveNodeDown()
        {
            try
            {
                if (SelectedNode?.NextNode == null) return;
                BeginUpdate();
                Sorted = false;

                TreeNode newNode = (TreeNode)SelectedNode.Clone();
                SelectedNode.Parent.Nodes.Insert(SelectedNode.Index + 2, newNode);
                SelectedNode.Remove();
                SelectedNode = newNode;

                EndUpdate();
            }
            catch (Exception ex)
            {
                Runtime.MessageCollector.AddMessage(MessageClass.ErrorMsg, "MoveNodeDown failed" + Environment.NewLine + ex.Message, true);
            }
        }

        public void MoveNodeUp()
        {
            try
            {
                if (SelectedNode?.PrevNode == null) return;
                BeginUpdate();
                Sorted = false;

                TreeNode newNode = (TreeNode)SelectedNode.Clone();
                SelectedNode.Parent.Nodes.Insert(SelectedNode.Index - 1, newNode);
                SelectedNode.Remove();
                SelectedNode = newNode;

                EndUpdate();
            }
            catch (Exception ex)
            {
                Runtime.MessageCollector.AddMessage(MessageClass.ErrorMsg, "MoveNodeUp failed" + Environment.NewLine + ex.Message, true);
            }
        }

        public void Sort(TreeNode treeNode, SortOrder sorting)
        {
            BeginUpdate();

            if (treeNode == null)
            {
                if (Nodes.Count > 0)
                    treeNode = Nodes[0];
                else
                    return;
            }
            else if (ConnectionTreeNode.GetNodeType(treeNode) == TreeNodeType.Connection)
            {
                treeNode = treeNode.Parent;
                if (treeNode == null)
                    return;
            }

            Sort(treeNode, new TreeNodeSorter(sorting));
            EndUpdate();
        }

        private void Sort(TreeNode treeNode, TreeNodeSorter nodeSorter)
        {
            // Adapted from http://www.codeproject.com/Tips/252234/ASP-NET-TreeView-Sort
            foreach (TreeNode childNode in treeNode.Nodes)
            {
                Sort(childNode, nodeSorter);
            }

            try
            {
                List<TreeNode> sortedNodes = new List<TreeNode>();
                TreeNode currentNode = null;
                while (treeNode.Nodes.Count > 0)
                {
                    foreach (TreeNode childNode in treeNode.Nodes)
                    {
                        if (currentNode == null || nodeSorter.Compare(childNode, currentNode) < 0)
                        {
                            currentNode = childNode;
                        }
                    }
                    if (currentNode != null)
                    {
                        treeNode.Nodes.Remove(currentNode);
                        sortedNodes.Add(currentNode);
                    }
                    currentNode = null;
                }

                foreach (TreeNode childNode in sortedNodes)
                {
                    treeNode.Nodes.Add(childNode);
                }
            }
            catch (Exception ex)
            {
                Runtime.MessageCollector.AddMessage(MessageClass.ErrorMsg, "Sort nodes failed" + Environment.NewLine + ex.Message, true);
            }
        }

        public TreeNode Find(TreeNode treeNode, string searchFor)
        {
            
            try
            {
                if (IsThisTheNodeWeAreSearchingFor(treeNode, searchFor))
                    return treeNode;

                foreach (TreeNode childNode in treeNode.Nodes)
                {
                    TreeNode tmpNode = Find(childNode, searchFor);
                    if (tmpNode != null)
                    {
                        return tmpNode;
                    }
                }
            }
            catch (Exception ex)
            {
                Runtime.MessageCollector.AddMessage(MessageClass.ErrorMsg, "Find node failed" + Environment.NewLine + ex.Message, true);
            }

            return null;
        }

        private bool IsThisTheNodeWeAreSearchingFor(TreeNode treeNode, string searchFor)
        {
            return ((treeNode.Text.ToLower()).IndexOf(searchFor.ToLower()) + 1 > 0);
        }

        public TreeNode Find(TreeNode treeNode, ConnectionInfo conInfo)
        {
            try
            {
                if (treeNode.Tag == conInfo)
                    return treeNode;

                foreach (TreeNode childNode in treeNode.Nodes)
                {
                    TreeNode tmpNode = Find(childNode, conInfo);
                    if (tmpNode != null)
                        return tmpNode;
                }
            }
            catch (Exception ex)
            {
                Runtime.MessageCollector.AddMessage(MessageClass.ErrorMsg, "Find node failed" + Environment.NewLine + ex.Message, true);
            }

            return null;
        }

        private delegate void ResetTreeDelegate();
        public void ResetTree()
        {
            if (InvokeRequired)
            {
                ResetTreeDelegate resetTreeDelegate = ResetTree;
                Windows.treeForm.Invoke(resetTreeDelegate);
            }
            else
            {
                BeginUpdate();
                Nodes.Clear();
                Nodes.Add(Language.strConnections);
                EndUpdate();
            }
        }

        private delegate void SelectNodeCB();
        private void SelectNode()
        {
            if (InvokeRequired)
            {
                SelectNodeCB d = SelectNode;
                Invoke(d);
            }
            else
            {
                SelectedNode = treeNodeToBeSelected;
            }
        }
    }
}
