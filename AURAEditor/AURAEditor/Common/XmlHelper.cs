using System.Xml;

namespace AuraEditor.Common
{
    static class XmlHelper
    {
        static private XmlDocument m_FileXmlDoc;

        static XmlHelper()
        {
            m_FileXmlDoc = new XmlDocument();
        }
        static public XmlNode CreateXmlNodeOfFile(string nodeName)
        {
            XmlNode Node = m_FileXmlDoc.CreateElement(nodeName);
            return Node;
        }
        static public XmlAttribute CreateXmlAttributeOfFile(string attributeName)
        {
            XmlAttribute attribute = m_FileXmlDoc.CreateAttribute(attributeName);
            return attribute;
        }
    }
}
