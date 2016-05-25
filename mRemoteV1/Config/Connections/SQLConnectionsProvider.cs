using mRemoteNG.App;
using mRemoteNG.Messages;
using mRemoteNG.Tools;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using mRemoteNG.My;
using mRemoteNG.UI.Forms;

namespace mRemoteNG.Config.Connections
{
    public class SqlConnectionsProvider : IDisposable
    {
        SqlUpdateTimer _updateTimer;
        SqlConnectionsUpdateChecker _sqlUpdateChecker;

        private frmMain _mainForm;
        public SqlConnectionsProvider(frmMain mainForm)
        {
            _mainForm = mainForm;
            _updateTimer = new SqlUpdateTimer();
            _sqlUpdateChecker = new SqlConnectionsUpdateChecker();
            SqlUpdateTimer.SqlUpdateTimerElapsed += SqlUpdateTimer_SqlUpdateTimerElapsed;
            SqlConnectionsUpdateChecker.SQLUpdateCheckFinished += SqlUpdateCheckFinished;
        }

        ~SqlConnectionsProvider()
        {
            Dispose(false);
        }

        public void Enable()
        {
            _updateTimer.Enable();
        }

        public void Disable()
        {
            _updateTimer.Disable();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool itIsSafeToAlsoFreeManagedObjects)
        {
            if (itIsSafeToAlsoFreeManagedObjects)
            {
                DestroySQLUpdateHandlers();
                _updateTimer.Dispose();
                _sqlUpdateChecker.Dispose();
            }
        }

        private void DestroySQLUpdateHandlers()
        {
            SqlUpdateTimer.SqlUpdateTimerElapsed -= SqlUpdateTimer_SqlUpdateTimerElapsed;
            SqlConnectionsUpdateChecker.SQLUpdateCheckFinished -= SqlUpdateCheckFinished;
        }

        private void SqlUpdateTimer_SqlUpdateTimerElapsed()
        {
            _sqlUpdateChecker.IsDatabaseUpdateAvailableAsync();
        }

        private void SqlUpdateCheckFinished(bool updateIsAvailable)
        {
            if (updateIsAvailable)
            {
                var runtime = new Runtime(_mainForm);
                runtime.LoadConnectionsBg();

                Runtime.MessageCollector.AddMessage(MessageClass.InformationMsg, Language.strSqlUpdateCheckUpdateAvailable, true);
            }
        }
    }
}