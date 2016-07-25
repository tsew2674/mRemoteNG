using System.IO;
using mRemoteNG.Config.Credentials;
using NUnit.Framework;

namespace mRemoteNGTests.Config.Credentials
{
    public class CredentialConfigConverterTests
    {
        private CredentialConfigConverter _credentialConfigConverter;

        [SetUp]
        public void Setup()
        {
            _credentialConfigConverter = new CredentialConfigConverter();
        }

        [TearDown]
        public void Teardown()
        {
            _credentialConfigConverter = null;
        }

        private string CreateTestConfConsFile()
        {
            var consFilePath = Path.GetTempFileName();
            File.WriteAllText(consFilePath, Resources.testConfCons);
            return consFilePath;
        }

        [Test]
        public void EnsureCreatingATestConfConsFileWorks()
        {
            var confConsFilePath = CreateTestConfConsFile();
            Assert.That(File.ReadAllText(confConsFilePath), Is.Not.Null);
        }
    }
}