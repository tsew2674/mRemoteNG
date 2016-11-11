﻿using System.Linq;
using mRemoteNG.Config.Serializers;
using mRemoteNG.Connection;
using mRemoteNG.Container;
using mRemoteNG.Security;
using mRemoteNG.Tree;
using mRemoteNG.Tree.Root;
using NUnit.Framework;


namespace mRemoteNGTests.IntegrationTests
{
    public class XmlSerializationLifeCycleTests
    {
        private XmlConnectionsSerializer _serializer;
        private XmlConnectionsDeserializer _deserializer;

        [SetUp]
        public void Setup()
        {
            var cryptoProvider = new CryptographyProviderFactory().CreateAeadCryptographyProvider(BlockCipherEngines.AES, BlockCipherModes.GCM);
            _serializer = new XmlConnectionsSerializer(cryptoProvider);
        }

        [TearDown]
        public void Teardown()
        {
            _serializer = null;
            _deserializer = null;
        }

        [Test]
        public void SerializeThenDeserialize()
        {
            var originalModel = SetupConnectionTreeModel();
            var serializedContent = _serializer.Serialize(originalModel);
            _deserializer = new XmlConnectionsDeserializer(serializedContent);
            var deserializedModel = _deserializer.Deserialize();
            var nodeNamesFromDeserializedModel = deserializedModel.GetRecursiveChildList().Select(node => node.Name);
            var nodeNamesFromOriginalModel = originalModel.GetRecursiveChildList().Select(node => node.Name);
            Assert.That(nodeNamesFromDeserializedModel, Is.EquivalentTo(nodeNamesFromOriginalModel));
        }

        [Test]
        public void SerializeThenDeserializeWithFullEncryption()
        {
            var originalModel = SetupConnectionTreeModel();
            _serializer.UseFullEncryption = true;
            var serializedContent = _serializer.Serialize(originalModel);
            _deserializer = new XmlConnectionsDeserializer(serializedContent);
            var deserializedModel = _deserializer.Deserialize();
            var nodeNamesFromDeserializedModel = deserializedModel.GetRecursiveChildList().Select(node => node.Name);
            var nodeNamesFromOriginalModel = originalModel.GetRecursiveChildList().Select(node => node.Name);
            Assert.That(nodeNamesFromDeserializedModel, Is.EquivalentTo(nodeNamesFromOriginalModel));
        }

        [Test]
        public void SerializeAndDeserializePropertiesWithInternationalCharacters()
        {
            var originalConnectionInfo = new ConnectionInfo {Name = "con1", Password = "£°úg¶┬ä" };
            var serializedContent = _serializer.Serialize(originalConnectionInfo);
            _deserializer = new XmlConnectionsDeserializer(serializedContent);
            var deserializedModel = _deserializer.Deserialize();
            var deserializedConnectionInfo = deserializedModel.GetRecursiveChildList().First(node => node.Name == originalConnectionInfo.Name);
            Assert.That(deserializedConnectionInfo.Password, Is.EqualTo(originalConnectionInfo.Password));
        }


        [Test]
        public void SerializeAndDeserializeWithCustomKdfIterationsValue()
        {
            var cryptoProvider = new CryptographyProviderFactory().CreateAeadCryptographyProvider(BlockCipherEngines.AES, BlockCipherModes.GCM);
            cryptoProvider.KeyDerivationIterations = 5000;
            _serializer = new XmlConnectionsSerializer(cryptoProvider);
            var originalModel = SetupConnectionTreeModel();
            var serializedContent = _serializer.Serialize(originalModel);
            _deserializer = new XmlConnectionsDeserializer(serializedContent);
            var deserializedModel = _deserializer.Deserialize();
            var nodeNamesFromDeserializedModel = deserializedModel.GetRecursiveChildList().Select(node => node.Name);
            var nodeNamesFromOriginalModel = originalModel.GetRecursiveChildList().Select(node => node.Name);
            Assert.That(nodeNamesFromDeserializedModel, Is.EquivalentTo(nodeNamesFromOriginalModel));
        }


        private ConnectionTreeModel SetupConnectionTreeModel()
        {
            /*
             * Root
             * |--- con0
             * |--- folder1
             * |    L--- con1
             * L--- folder2
             *      |--- con2
             *      L--- folder3
             *           |--- con3
             *           L--- con4
             */
            var connectionTreeModel = new ConnectionTreeModel();
            var rootNode = new RootNodeInfo(RootNodeType.Connection);
            var folder1 = new ContainerInfo { Name = "folder1" };
            var folder2 = new ContainerInfo { Name = "folder2" };
            var folder3 = new ContainerInfo { Name = "folder3" };
            var con0 = new ConnectionInfo { Name = "con0" };
            var con1 = new ConnectionInfo { Name = "con1" };
            var con2 = new ConnectionInfo { Name = "con2" };
            var con3 = new ConnectionInfo { Name = "con3" };
            var con4 = new ConnectionInfo { Name = "con4" };
            rootNode.AddChild(folder1);
            rootNode.AddChild(folder2);
            rootNode.AddChild(con0);
            folder1.AddChild(con1);
            folder2.AddChild(con2);
            folder2.AddChild(folder3);
            folder3.AddChild(con3);
            folder3.AddChild(con4);
            connectionTreeModel.AddRootNode(rootNode);
            return connectionTreeModel;
        }
    }
}