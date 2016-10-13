﻿using System.Security;
using mRemoteNG.Security;
using mRemoteNG.Security.SymmetricEncryption;
using mRemoteNGTests.Properties;
using NUnit.Framework;


namespace mRemoteNGTests.Security
{
    public class LegacyRijndaelCryptographyProviderTests
    {
        private ICryptographyProvider _rijndaelCryptographyProvider;
        private SecureString _encryptionKey;
        private string _plainText;
        private string _importedCipherText = Resources.legacyrijndael_ciphertext;


        [SetUp]
        public void SetUp()
        {
            _rijndaelCryptographyProvider = new LegacyRijndaelCryptographyProvider();
            _encryptionKey = "mR3m".ConvertToSecureString();
            _plainText = "MySecret!";
        }

        [TearDown]
        public void Teardown()
        {
            _rijndaelCryptographyProvider = null;
        }

        [Test]
        public void GetBlockSizeReturnsProperValueForRijndael()
        {
            Assert.That(_rijndaelCryptographyProvider.BlockSizeInBytes, Is.EqualTo(16));
        }

        [Test]
        public void EncryptionOutputsBase64String()
        {
            var cipherText = _rijndaelCryptographyProvider.Encrypt(_plainText, _encryptionKey);
            Assert.That(cipherText.IsBase64String, Is.True);
        }

        [Test]
        public void DecryptedTextIsEqualToOriginalPlainText()
        {
            var cipherText = _rijndaelCryptographyProvider.Encrypt(_plainText, _encryptionKey);
            var decryptedCipherText = _rijndaelCryptographyProvider.Decrypt(cipherText, _encryptionKey);
            Assert.That(decryptedCipherText, Is.EqualTo(_plainText));
        }

        [Test]
        public void EncryptingTheSameValueReturnsNewCipherTextEachTime()
        {
            var cipherText1 = _rijndaelCryptographyProvider.Encrypt(_plainText, _encryptionKey);
            var cipherText2 = _rijndaelCryptographyProvider.Encrypt(_plainText, _encryptionKey);
            Assert.That(cipherText1, Is.Not.EqualTo(cipherText2));
        }

        [Test]
        public void DecryptingFromPreviousApplicationExecutionWorks()
        {
            var decryptedCipherText = _rijndaelCryptographyProvider.Decrypt(_importedCipherText, _encryptionKey);
            Assert.That(decryptedCipherText, Is.EqualTo(_plainText));
        }
    }
}