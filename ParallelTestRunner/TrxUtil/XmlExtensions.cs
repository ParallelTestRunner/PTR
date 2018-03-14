using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TrxUtil
{
	static class XmlExtensions
	{
		public static string GetAttributeValue(this XmlNode xmlNode, string strAttributeName)
		{
			if (xmlNode.Attributes[strAttributeName] != null)
			{
				return xmlNode.Attributes[strAttributeName].Value;
			}
			return null;
		}

		public static void SetAttributeValue(this XmlNode xmlNode, string strAttributeName, string strAttributeValue)
		{
			if (xmlNode.Attributes[strAttributeName] == null)
			{
				XmlAttribute xmlAttrbute = xmlNode.OwnerDocument.CreateAttribute(strAttributeName);
				xmlNode.Attributes.Append(xmlAttrbute);
			}
			xmlNode.Attributes[strAttributeName].Value = strAttributeValue;
		}
	}
}
