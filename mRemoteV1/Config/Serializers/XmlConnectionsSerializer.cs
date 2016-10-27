﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using mRemoteNG.App;
using mRemoteNG.Connection;
using mRemoteNG.Security;
using mRemoteNG.Tree;
using mRemoteNG.Tree.Root;

namespace mRemoteNG.Config.Serializers
{
    public class XmlConnectionsSerializer : ISerializer<string>
    {
        private readonly ICryptographyProvider _cryptographyProvider;

        public bool Export { get; set; }
        public SaveFilter SaveFilter { get; set; } = new SaveFilter();
        public bool UseFullEncryption { get; set; }

        public XmlConnectionsSerializer(ICryptographyProvider cryptographyProvider)
        {
            _cryptographyProvider = cryptographyProvider;
        }

        public string Serialize(ConnectionTreeModel connectionTreeModel)
        {
            var rootNode = (RootNodeInfo)connectionTreeModel.RootNodes.First(node => node is RootNodeInfo);
            return SerializeConnectionsData(rootNode);
        }

        public string Serialize(ConnectionInfo serializationTarget)
        {
            return SerializeConnectionsData(serializationTarget);
        }

        private string SerializeConnectionsData(ConnectionInfo serializationTarget)
        {
            var xml = "";
            try
            {
                var documentCompiler = new XmlConnectionsDocumentCompiler(_cryptographyProvider);
                var xmlDocument = documentCompiler.CompileDocument(serializationTarget, UseFullEncryption, Export);
                xml = WriteXmlToString(xmlDocument);
            }
            catch (Exception ex)
            {
                Runtime.MessageCollector.AddExceptionStackTrace("SaveToXml failed", ex);
            }
            return xml;
        }

        private string WriteXmlToString(XNode xmlDocument)
        {
            string xmlString;
            var xmlWriterSettings = new XmlWriterSettings { Indent = true, IndentChars = "    ", Encoding = Encoding.UTF8 };
            using (var memoryStream = new MemoryStream())
            using (var xmlTextWriter = XmlWriter.Create(memoryStream, xmlWriterSettings))
            {
                xmlDocument.WriteTo(xmlTextWriter);
                xmlTextWriter.Flush();
                var streamReader = new StreamReader(memoryStream, Encoding.UTF8, true);
                memoryStream.Seek(0, SeekOrigin.Begin);
                xmlString = streamReader.ReadToEnd();
            }
            return xmlString;
        }
    }
}