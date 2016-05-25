using mRemoteNG.UI.Forms;
using WeifenLuo.WinFormsUI.Docking;


namespace mRemoteNG.UI.Window
{
	public class BaseWindow : DockContent
    {
        #region Private Variables

	    #endregion

        #region Constructors
        public BaseWindow()
		{
			//InitializeComponent();
		}
        #endregion

        #region Public Properties
        public WindowType WindowType { get; set; }

	    public DockContent DockPnl { get; set; }

	    #endregion
				
        #region Public Methods
		public void SetFormText(string Text)
		{
			this.Text = Text;
			TabText = Text;
		}
        #endregion
				
  //      #region Private Methods
		//private void Base_Load(object sender, System.EventArgs e)
		//{
		//	_mainForm.ShowHidePanelTabs();
		//}
				
		//private void Base_FormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
		//{
		//	_mainForm.ShowHidePanelTabs(this);
		//}
  //      #endregion
	}
}