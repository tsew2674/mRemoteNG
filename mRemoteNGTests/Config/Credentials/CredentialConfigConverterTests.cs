using System.IO;
using mRemoteNG.Config.Credentials;
using NUnit.Framework;

namespace mRemoteNGTests.Config.Credentials
{
    public class CredentialConfigConverterTests
    {
        private CredentialConfigConverter _credentialConfigConverter;
        private readonly string _confConsFilePath = CreateTestConfConsFile();

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

        private static string CreateTestConfConsFile()
        {
            var consFilePath = Path.GetTempFileName();
            File.WriteAllText(consFilePath, Resources.testConfCons);
            return consFilePath;
        }

        [Test]
        public void EnsureCreatingATestConfConsFileWorks()
        {
            Assert.That(File.ReadAllText(_confConsFilePath), Is.Not.Null);
        }
    }
}