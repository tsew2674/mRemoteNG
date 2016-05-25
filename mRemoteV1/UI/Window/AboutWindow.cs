using System;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using System.IO;
using mRemoteNG.App;
using mRemoteNG.UI.Forms;


namespace mRemoteNG.UI.Window
{
	public class AboutWindow : BaseWindow
	{
        #region Form Init
		internal Label LblCopyright;
		internal Label LblTitle;
		internal Label LblVersion;
		internal Label LblLicense;
		internal TextBox TxtChangeLog;
		internal Label LblChangeLog;
		internal Panel PnlBottom;
		internal PictureBox PbLogo;
		internal Label LblEdition;
		internal LinkLabel LlblFamfamfam;
		internal LinkLabel LlblMagicLibrary;
		internal LinkLabel LlblWeifenLuo;
		internal Panel PnlTop;

	    private frmMain _mainForm;

	    public AboutWindow(frmMain mainForm)
	    {
	        _mainForm = mainForm;
	    }
				
		private void InitializeComponent()
		{
			PnlTop = new Panel();
			Load += new EventHandler(About_Load);
			LblEdition = new Label();
			PbLogo = new PictureBox();
			PnlBottom = new Panel();
			LlblWeifenLuo = new LinkLabel();
			LlblWeifenLuo.LinkClicked += new LinkLabelLinkClickedEventHandler(llblWeifenLuo_LinkClicked);
			LlblMagicLibrary = new LinkLabel();
			LlblMagicLibrary.LinkClicked += new LinkLabelLinkClickedEventHandler(llblMagicLibrary_LinkClicked);
			LlblFamfamfam = new LinkLabel();
			LlblFamfamfam.LinkClicked += new LinkLabelLinkClickedEventHandler(llblFAMFAMFAM_LinkClicked);
			TxtChangeLog = new TextBox();
			LblTitle = new Label();
			LblVersion = new Label();
			LblChangeLog = new Label();
			LblLicense = new Label();
			LblCopyright = new Label();
			PnlTop.SuspendLayout();
			((System.ComponentModel.ISupportInitialize) PbLogo).BeginInit();
			PnlBottom.SuspendLayout();
			SuspendLayout();
			//
			//pnlTop
			//
			PnlTop.Anchor = (AnchorStyles) ((AnchorStyles.Top | AnchorStyles.Left) 
				| AnchorStyles.Right);
			PnlTop.BackColor = System.Drawing.Color.Black;
			PnlTop.Controls.Add(LblEdition);
			PnlTop.Controls.Add(PbLogo);
			PnlTop.ForeColor = System.Drawing.Color.White;
			PnlTop.Location = new System.Drawing.Point(-1, -1);
			PnlTop.Name = "PnlTop";
			PnlTop.Size = new System.Drawing.Size(788, 145);
			PnlTop.TabIndex = 0;
			//
			//lblEdition
			//
			LblEdition.Anchor = (AnchorStyles) (AnchorStyles.Top | AnchorStyles.Right);
			LblEdition.BackColor = System.Drawing.Color.Black;
			LblEdition.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (14.25F), System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, Convert.ToByte(0));
			LblEdition.ForeColor = System.Drawing.Color.White;
			LblEdition.Location = new System.Drawing.Point(512, 112);
			LblEdition.Name = "LblEdition";
			LblEdition.Size = new System.Drawing.Size(264, 24);
			LblEdition.TabIndex = 0;
			LblEdition.Text = "Edition";
			LblEdition.TextAlign = System.Drawing.ContentAlignment.BottomRight;
			LblEdition.Visible = false;
			//
			//pbLogo
			//
			PbLogo.Image = Resources.Logo;
			PbLogo.Location = new System.Drawing.Point(8, 8);
			PbLogo.Name = "PbLogo";
			PbLogo.Size = new System.Drawing.Size(492, 128);
			PbLogo.SizeMode = PictureBoxSizeMode.AutoSize;
			PbLogo.TabIndex = 1;
			PbLogo.TabStop = false;
			//
			//pnlBottom
			//
			PnlBottom.Anchor = (AnchorStyles) (((AnchorStyles.Top | AnchorStyles.Bottom) 
				| AnchorStyles.Left) 
				| AnchorStyles.Right);
			PnlBottom.BackColor = System.Drawing.SystemColors.Control;
			PnlBottom.Controls.Add(LlblWeifenLuo);
			PnlBottom.Controls.Add(LlblMagicLibrary);
			PnlBottom.Controls.Add(LlblFamfamfam);
			PnlBottom.Controls.Add(TxtChangeLog);
			PnlBottom.Controls.Add(LblTitle);
			PnlBottom.Controls.Add(LblVersion);
			PnlBottom.Controls.Add(LblChangeLog);
			PnlBottom.Controls.Add(LblLicense);
			PnlBottom.Controls.Add(LblCopyright);
			PnlBottom.ForeColor = System.Drawing.SystemColors.ControlText;
			PnlBottom.Location = new System.Drawing.Point(-1, 144);
			PnlBottom.Name = "PnlBottom";
			PnlBottom.Size = new System.Drawing.Size(788, 418);
			PnlBottom.TabIndex = 1;
			//
			//llblWeifenLuo
			//
			LlblWeifenLuo.AutoSize = true;
			LlblWeifenLuo.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (11.0F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, Convert.ToByte(0));
			LlblWeifenLuo.ForeColor = System.Drawing.SystemColors.ControlText;
			LlblWeifenLuo.LinkColor = System.Drawing.Color.Blue;
			LlblWeifenLuo.Location = new System.Drawing.Point(16, 158);
			LlblWeifenLuo.Name = "LlblWeifenLuo";
			LlblWeifenLuo.Size = new System.Drawing.Size(78, 22);
			LlblWeifenLuo.TabIndex = 9;
			LlblWeifenLuo.TabStop = true;
			LlblWeifenLuo.Text = "WeifenLuo";
			LlblWeifenLuo.UseCompatibleTextRendering = true;
			//
			//llblMagicLibrary
			//
			LlblMagicLibrary.AutoSize = true;
			LlblMagicLibrary.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (11.0F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, Convert.ToByte(0));
			LlblMagicLibrary.ForeColor = System.Drawing.SystemColors.ControlText;
			LlblMagicLibrary.LinkColor = System.Drawing.Color.Blue;
			LlblMagicLibrary.Location = new System.Drawing.Point(16, 136);
			LlblMagicLibrary.Name = "LlblMagicLibrary";
			LlblMagicLibrary.Size = new System.Drawing.Size(92, 22);
			LlblMagicLibrary.TabIndex = 8;
			LlblMagicLibrary.TabStop = true;
			LlblMagicLibrary.Text = "MagicLibrary";
			LlblMagicLibrary.UseCompatibleTextRendering = true;
			//
			//llblFAMFAMFAM
			//
			LlblFamfamfam.AutoSize = true;
			LlblFamfamfam.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (11.0F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, Convert.ToByte(0));
			LlblFamfamfam.ForeColor = System.Drawing.SystemColors.ControlText;
			LlblFamfamfam.LinkColor = System.Drawing.Color.Blue;
			LlblFamfamfam.Location = new System.Drawing.Point(16, 116);
			LlblFamfamfam.Name = "LlblFamfamfam";
			LlblFamfamfam.Size = new System.Drawing.Size(101, 22);
			LlblFamfamfam.TabIndex = 4;
			LlblFamfamfam.TabStop = true;
			LlblFamfamfam.Text = "FAMFAMFAM";
			LlblFamfamfam.UseCompatibleTextRendering = true;
			//
			//txtChangeLog
			//
			TxtChangeLog.Anchor = (AnchorStyles) (((AnchorStyles.Top | AnchorStyles.Bottom) 
				| AnchorStyles.Left) 
				| AnchorStyles.Right);
			TxtChangeLog.BackColor = System.Drawing.SystemColors.Control;
			TxtChangeLog.BorderStyle = BorderStyle.None;
			TxtChangeLog.Cursor = Cursors.Default;
			TxtChangeLog.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (9.0F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, Convert.ToByte(0));
			TxtChangeLog.ForeColor = System.Drawing.SystemColors.ControlText;
			TxtChangeLog.Location = new System.Drawing.Point(24, 224);
			TxtChangeLog.Multiline = true;
			TxtChangeLog.Name = "TxtChangeLog";
			TxtChangeLog.ReadOnly = true;
			TxtChangeLog.ScrollBars = ScrollBars.Vertical;
			TxtChangeLog.Size = new System.Drawing.Size(760, 192);
			TxtChangeLog.TabIndex = 7;
			TxtChangeLog.TabStop = false;
			//
			//lblTitle
			//
			LblTitle.AutoSize = true;
			LblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (14.0F), System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, Convert.ToByte(0));
			LblTitle.ForeColor = System.Drawing.SystemColors.ControlText;
			LblTitle.Location = new System.Drawing.Point(16, 16);
			LblTitle.Name = "LblTitle";
			LblTitle.Size = new System.Drawing.Size(122, 27);
			LblTitle.TabIndex = 0;
			LblTitle.Text = "mRemoteNG";
			LblTitle.UseCompatibleTextRendering = true;
			//
			//lblVersion
			//
			LblVersion.AutoSize = true;
			LblVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (11.0F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, Convert.ToByte(0));
			LblVersion.ForeColor = System.Drawing.SystemColors.ControlText;
			LblVersion.Location = new System.Drawing.Point(16, 56);
			LblVersion.Name = "LblVersion";
			LblVersion.Size = new System.Drawing.Size(57, 22);
			LblVersion.TabIndex = 1;
			LblVersion.Text = "Version";
			LblVersion.UseCompatibleTextRendering = true;
			//
			//lblChangeLog
			//
			LblChangeLog.AutoSize = true;
			LblChangeLog.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (11.0F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, Convert.ToByte(0));
			LblChangeLog.ForeColor = System.Drawing.SystemColors.ControlText;
			LblChangeLog.Location = new System.Drawing.Point(16, 199);
			LblChangeLog.Name = "LblChangeLog";
			LblChangeLog.Size = new System.Drawing.Size(92, 22);
			LblChangeLog.TabIndex = 6;
			LblChangeLog.Text = "Change Log:";
			LblChangeLog.UseCompatibleTextRendering = true;
			//
			//lblLicense
			//
			LblLicense.AutoSize = true;
			LblLicense.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (11.0F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, Convert.ToByte(0));
			LblLicense.ForeColor = System.Drawing.SystemColors.ControlText;
			LblLicense.Location = new System.Drawing.Point(16, 96);
			LblLicense.Name = "LblLicense";
			LblLicense.Size = new System.Drawing.Size(58, 22);
			LblLicense.TabIndex = 5;
			LblLicense.Text = "License";
			LblLicense.UseCompatibleTextRendering = true;
			//
			//lblCopyright
			//
			LblCopyright.AutoSize = true;
			LblCopyright.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (11.0F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, Convert.ToByte(0));
			LblCopyright.ForeColor = System.Drawing.SystemColors.ControlText;
			LblCopyright.Location = new System.Drawing.Point(16, 76);
			LblCopyright.Name = "LblCopyright";
			LblCopyright.Size = new System.Drawing.Size(70, 22);
			LblCopyright.TabIndex = 2;
			LblCopyright.Text = "Copyright";
			LblCopyright.UseCompatibleTextRendering = true;
			//
			//About
			//
			BackColor = System.Drawing.SystemColors.Control;
			ClientSize = new System.Drawing.Size(784, 564);
			Controls.Add(PnlTop);
			Controls.Add(PnlBottom);
			Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (8.25F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, Convert.ToByte(0));
			ForeColor = System.Drawing.SystemColors.ControlText;
			Icon = Resources.mRemote_Icon;
			MaximumSize = new System.Drawing.Size(20000, 10000);
			Name = "About";
			TabText = "About";
			Text = "About";
			PnlTop.ResumeLayout(false);
			PnlTop.PerformLayout();
			((System.ComponentModel.ISupportInitialize) PbLogo).EndInit();
			PnlBottom.ResumeLayout(false);
			PnlBottom.PerformLayout();
			ResumeLayout(false);
					
		}
        #endregion
				
        #region Public Methods
		public AboutWindow(DockContent panel)
		{
			WindowType = WindowType.About;
			DockPnl = panel;
			InitializeComponent();
			Runtime.FontOverride(this);
		}
        #endregion
				
        #region Private Methods
		private void ApplyLanguage()
		{
			LblLicense.Text = Language.strLabelReleasedUnderGPL;
			LblChangeLog.Text = Language.strLabelChangeLog;
			TabText = Language.strAbout;
			Text = Language.strAbout;
		}
				
		private void ApplyEditions()
		{
            #if PORTABLE
			LblEdition.Text = Language.strLabelPortableEdition;
			LblEdition.Visible = true;
            #endif
		}
				
		private void FillLinkLabel(LinkLabel llbl, string text, string url)
		{
			llbl.Links.Clear();
					
			int open = text.IndexOf("[");
			int close = 0;
			while (open != -1)
			{
				text = text.Remove(open, 1);
				close = text.IndexOf("]", open);
				if (close == -1)
				{
					break;
				}
				text = text.Remove(close, 1);
				llbl.Links.Add(open, close - open, url);
				open = text.IndexOf("[", open);
			}
					
			llbl.Text = text;
		}
        #endregion
				
        #region Form Stuff
		private void About_Load(object sender, EventArgs e)
		{
			ApplyLanguage();
			ApplyEditions();
					
			try
			{
				LblCopyright.Text = (new Microsoft.VisualBasic.ApplicationServices.WindowsFormsApplicationBase()).Info.Copyright;
						
				LblVersion.Text = "Version " + (new Microsoft.VisualBasic.ApplicationServices.WindowsFormsApplicationBase()).Info.Version.ToString();
						
				FillLinkLabel(LlblFamfamfam, Language.strFAMFAMFAMAttribution, Language.strFAMFAMFAMAttributionURL);
				FillLinkLabel(LlblMagicLibrary, Language.strMagicLibraryAttribution, Language.strMagicLibraryAttributionURL);
				FillLinkLabel(LlblWeifenLuo, Language.strWeifenLuoAttribution, Language.strWeifenLuoAttributionURL);
						
				if (File.Exists((new Microsoft.VisualBasic.ApplicationServices.WindowsFormsApplicationBase()).Info.DirectoryPath + "\\CHANGELOG.TXT"))
				{
					StreamReader sR = new StreamReader((new Microsoft.VisualBasic.ApplicationServices.WindowsFormsApplicationBase()).Info.DirectoryPath + "\\CHANGELOG.TXT");
					TxtChangeLog.Text = sR.ReadToEnd();
					sR.Close();
				}
			}
			catch (Exception ex)
			{
				Runtime.MessageCollector.AddMessage(Messages.MessageClass.ErrorMsg, "Loading About failed" + Environment.NewLine + ex.Message, true);
			}
		}
				
		private void llblFAMFAMFAM_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			//Runtime.GoToUrl(Language.strFAMFAMFAMAttributionURL);
            var runtime = new Runtime(_mainForm);
            runtime.GoToUrl(Language.strFAMFAMFAMAttributionURL);
        }
				
		private void llblMagicLibrary_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
            var runtime = new Runtime(_mainForm);
            runtime.GoToUrl(Language.strMagicLibraryAttributionURL);
		}
				
		private void llblWeifenLuo_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
            var runtime = new Runtime(_mainForm);
            runtime.GoToUrl(Language.strWeifenLuoAttributionURL);
		}
        #endregion
	}
}
