using System;
using mRemoteNG.App;
using mRemoteNG.UI.Forms;
using WeifenLuo.WinFormsUI.Docking;


namespace mRemoteNG.UI.Window
{
	public class UltraVNCWindow : BaseWindow
	{
	    private frmMain _mainForm;
	    public UltraVNCWindow(frmMain mainForm)
	    {
	        _mainForm = mainForm;
	    }
        #region Form Init
		internal System.Windows.Forms.ToolStrip tsMain;
		internal System.Windows.Forms.Panel pnlContainer;
		internal System.Windows.Forms.ToolStripButton btnDisconnect;
				
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UltraVNCWindow));
			tsMain = new System.Windows.Forms.ToolStrip();
			Load += new EventHandler(UltraVNCSC_Load);
			btnDisconnect = new System.Windows.Forms.ToolStripButton();
			btnDisconnect.Click += new EventHandler(btnDisconnect_Click);
			pnlContainer = new System.Windows.Forms.Panel();
			tsMain.SuspendLayout();
			SuspendLayout();
			//
			//tsMain
			//
			tsMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			tsMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {btnDisconnect});
			tsMain.Location = new System.Drawing.Point(0, 0);
			tsMain.Name = "tsMain";
			tsMain.Size = new System.Drawing.Size(446, 25);
			tsMain.TabIndex = 0;
			tsMain.Text = "ToolStrip1";
			//
			//btnDisconnect
			//
			btnDisconnect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			btnDisconnect.Image = (System.Drawing.Image) (resources.GetObject("btnDisconnect.Image"));
			btnDisconnect.ImageTransparentColor = System.Drawing.Color.Magenta;
			btnDisconnect.Name = "btnDisconnect";
			btnDisconnect.Size = new System.Drawing.Size(63, 22);
			btnDisconnect.Text = "Disconnect";
			//
			//pnlContainer
			//
			pnlContainer.Anchor = (System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			pnlContainer.Location = new System.Drawing.Point(0, 27);
			pnlContainer.Name = "pnlContainer";
			pnlContainer.Size = new System.Drawing.Size(446, 335);
			pnlContainer.TabIndex = 1;
			//
			//UltraVNCSC
			//
			ClientSize = new System.Drawing.Size(446, 362);
			Controls.Add(pnlContainer);
			Controls.Add(tsMain);
			Icon = Resources.UVNC_SC_Icon;
			Name = "UltraVNCSC";
			TabText = "UltraVNC SC";
			Text = "UltraVNC SC";
			tsMain.ResumeLayout(false);
			tsMain.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
					
		}
        #endregion
		
        #region Declarations
		//Private WithEvents vnc As AxCSC_ViewerXControl
        #endregion
		
        #region Public Methods
		public UltraVNCWindow(DockContent Panel)
		{
			WindowType = WindowType.UltraVNCSC;
			DockPnl = Panel;
			InitializeComponent();
		}
        #endregion
		
        #region Private Methods
		private void UltraVNCSC_Load(object sender, EventArgs e)
		{
			ApplyLanguage();
					
			StartListening();
		}
				
		private void ApplyLanguage()
		{
			btnDisconnect.Text = Language.strButtonDisconnect;
		}
				
		private void StartListening()
		{
			try
			{
				//If vnc IsNot Nothing Then
				//    vnc.Dispose()
				//    vnc = Nothing
				//End If
						
				//vnc = New AxCSC_ViewerXControl()
				//SetupLicense()
						
				//vnc.Parent = pnlContainer
				//vnc.Dock = DockStyle.Fill
				//vnc.Show()
						
				//vnc.StretchMode = ViewerX.ScreenStretchMode.SSM_ASPECT
				//vnc.ListeningText = Language.strInheritListeningForIncomingVNCConnections & " " & Settings.UVNCSCPort
						
				//vnc.ListenEx(Settings.UVNCSCPort)
			}
			catch (Exception ex)
			{
				Runtime.MessageCollector.AddMessage(Messages.MessageClass.ErrorMsg, "StartListening (UI.Window.UltraVNCSC) failed" + Environment.NewLine + ex.Message, false);
				Close();
			}
		}
				
		private void SetupLicense()
		{
			try
			{
				//Dim f As System.Reflection.FieldInfo
				//f = GetType(AxHost).GetField("licenseKey", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance)
				//f.SetValue(vnc, "{072169039103041044176252035252117103057101225235137221179204110241121074}")
			}
			catch (Exception ex)
			{
				Runtime.MessageCollector.AddMessage(Messages.MessageClass.ErrorMsg, "VNC SetupLicense failed (UI.Window.UltraVNCSC)" + Environment.NewLine + ex.Message, true);
			}
		}
				
		//Private Sub vnc_ConnectionAccepted(ByVal sender As Object, ByVal e As AxViewerX._ISmartCodeVNCViewerEvents_ConnectionAcceptedEvent) Handles vnc.ConnectionAccepted
		//    mC.AddMessage(Messages.MessageClass.InformationMsg, e.bstrServerAddress & " is now connected to your UltraVNC SingleClick panel!")
		//End Sub
				
		//Private Sub vnc_Disconnected(ByVal sender As Object, ByVal e As System.EventArgs) Handles vnc.Disconnected
		//    StartListening()
		//End Sub
				
		private void btnDisconnect_Click(object sender, EventArgs e)
		{
			//vnc.Dispose()
			Dispose();
            var windows = new Windows(_mainForm);
            windows.Show(WindowType.UltraVNCSC, _mainForm.pnlDock);
		}
        #endregion
	}
}