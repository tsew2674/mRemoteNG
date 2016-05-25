using System;
using mRemoteNG.App;
using mRemoteNG.App.Info;
using mRemoteNG.Config.Connections;
using mRemoteNG.My;
using mRemoteNG.Security;

namespace mRemoteNG.UI.Forms.OptionsPages
{
    public partial class SqlServerPage
    {
        private frmMain _mainform;
        public SqlServerPage(frmMain mainForm)
        {
            _mainform = mainForm;
            InitializeComponent();
        }

        public override string PageName
        {
            get { return Language.strSQLServer.TrimEnd(':'); }
            set { }
        }

        public override void ApplyLanguage()
        {
            base.ApplyLanguage();

            lblExperimental.Text = Language.strExperimental.ToUpper();
            lblSQLInfo.Text = Language.strSQLInfo;

            chkUseSQLServer.Text = Language.strUseSQLServer;
            lblSQLServer.Text = Language.strLabelHostname;
            lblSQLDatabaseName.Text = Language.strLabelSQLServerDatabaseName;
            lblSQLUsername.Text = Language.strLabelUsername;
            lblSQLPassword.Text = Language.strLabelPassword;
        }

        public override void LoadSettings()
        {
            base.SaveSettings();

            chkUseSQLServer.Checked = Settings.Default.UseSQLServer;
            txtSQLServer.Text = Settings.Default.SQLHost;
            txtSQLDatabaseName.Text = Settings.Default.SQLDatabaseName;
            txtSQLUsername.Text = Settings.Default.SQLUser;
            txtSQLPassword.Text = Crypt.Decrypt(Settings.Default.SQLPass, GeneralAppInfo.EncryptionKey);
        }

        public override void SaveSettings()
        {
            base.SaveSettings();

            Settings.Default.UseSQLServer = chkUseSQLServer.Checked;
            Settings.Default.SQLHost = txtSQLServer.Text;
            Settings.Default.SQLDatabaseName = txtSQLDatabaseName.Text;
            Settings.Default.SQLUser = txtSQLUsername.Text;
            Settings.Default.SQLPass = Crypt.Encrypt(txtSQLPassword.Text, GeneralAppInfo.EncryptionKey);
            ReinitializeSqlUpdater();
        }

        private void ReinitializeSqlUpdater()
        {
            if (Runtime.SqlConnProvider != null)
            {
                Runtime.SqlConnProvider.Dispose();
                _mainform.AreWeUsingSqlServerForSavingConnections = Settings.Default.UseSQLServer;
                if (Settings.Default.UseSQLServer)
                {

                    Runtime.SqlConnProvider = new SqlConnectionsProvider(_mainform);
                    Runtime.SqlConnProvider.Enable();
                }
            }
        }

        public void chkUseSQLServer_CheckedChanged(object sender, EventArgs e)
        {
            lblSQLServer.Enabled = chkUseSQLServer.Checked;
            lblSQLDatabaseName.Enabled = chkUseSQLServer.Checked;
            lblSQLUsername.Enabled = chkUseSQLServer.Checked;
            lblSQLPassword.Enabled = chkUseSQLServer.Checked;
            txtSQLServer.Enabled = chkUseSQLServer.Checked;
            txtSQLDatabaseName.Enabled = chkUseSQLServer.Checked;
            txtSQLUsername.Enabled = chkUseSQLServer.Checked;
            txtSQLPassword.Enabled = chkUseSQLServer.Checked;
        }
    }
}