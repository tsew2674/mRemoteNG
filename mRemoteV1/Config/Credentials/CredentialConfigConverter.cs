using System.Collections.Generic;
using mRemoteNG.Credential;


namespace mRemoteNG.Config.Credentials
{
    public class CredentialConfigConverter
    {
        private IList<CredentialInfo> _credentialList;

        public CredentialConfigConverter()
        {
            _credentialList = new List<CredentialInfo>();
        }

        public IList<CredentialInfo> BuildCredentialListFromConnectionFile(string connectionsFilePath)
        {
            GetUniqueCredentialsFromConnectionsFile();
            return _credentialList;
        }

        private void GetUniqueCredentialsFromConnectionsFile()
        {
            var connectionList = new CredentialList();
        }
    }
}