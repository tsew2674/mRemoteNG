using System;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using mRemoteNG.App;
using mRemoteNG.UI.Forms;


namespace mRemoteNG.UI.Window
{
	public partial class ActiveDirectoryImportWindow : BaseWindow
	{
        #region Constructors

	    private frmMain _mainForm;
		public ActiveDirectoryImportWindow(DockContent panel, frmMain mainForm)
		{
		    _mainForm = mainForm;
            InitializeComponent();
			Runtime.FontOverride(this);
			WindowType = WindowType.ActiveDirectoryImport;
			DockPnl = panel;
		}
        #endregion
				
        #region Private Methods
        #region Event Handlers
		public void ADImport_Load(object sender, EventArgs e)
		{
			ApplyLanguage();
			txtDomain.Text = ActiveDirectoryTree.Domain;
			EnableDisableImportButton();
		}
				
		public void btnImport_Click(object sender, EventArgs e)
		{
            var import = new Import(_mainForm);
            import.ImportFromActiveDirectory(ActiveDirectoryTree.ADPath);
			DialogResult = DialogResult.OK;
			Close();
		}
				
		public static void txtDomain_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				e.IsInputKey = true;
			}
		}
				
		public void txtDomain_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				ChangeDomain();
				e.SuppressKeyPress = true;
			}
		}
				
		public void btnChangeDomain_Click(object sender, EventArgs e)
		{
			ChangeDomain();
		}
				
		public void ActiveDirectoryTree_ADPathChanged(object sender)
		{
			EnableDisableImportButton();
		}
        #endregion
				
		private void ApplyLanguage()
		{
			btnImport.Text = Language.strButtonImport;
			lblDomain.Text = Language.strLabelDomain;
			btnChangeDomain.Text = Language.strButtonChange;
		}
				
		private void ChangeDomain()
		{
			ActiveDirectoryTree.Domain = txtDomain.Text;
			ActiveDirectoryTree.Refresh();
		}
				
		private void EnableDisableImportButton()
		{
			btnImport.Enabled = !string.IsNullOrEmpty(ActiveDirectoryTree.ADPath);
		}
        #endregion
	}
}