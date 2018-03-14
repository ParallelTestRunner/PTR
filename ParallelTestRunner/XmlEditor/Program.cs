using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace XmlEditor
{
	class Program
	{
		private static Dictionary<string, string> _dicValuesByTheirVariableNames = new Dictionary<string, string>();
		static void Main(string[] args)
		{
			try
			{
                string strFileToEdit = args[0];
                string strTextFileWithXPaths = args[1];

                //string strTextFileWithXPaths = @"C:\Users\aseem\Desktop\XPaths.txt";
                //string strFileToEdit = @"C:\Users\aseem\Desktop\XmlDoc.xml";

                //string strBackupOfFileToEdit = strFileToEdit.Replace(Path.GetFileName(strFileToEdit), Path.GetFileNameWithoutExtension(strFileToEdit) + "_BK");
                //if (!File.Exists(strBackupOfFileToEdit))
                //{
                //	File.Copy(strFileToEdit, strBackupOfFileToEdit, true);
                //}
                //else
                //{
                //	File.Copy(strBackupOfFileToEdit, strFileToEdit, true);
                //}

                StringBuilder sbSharedResources = new StringBuilder();
				for (int i = 2; i < args.Length; i++)
				{
					sbSharedResources.Append(args[i].Trim() + " ; ");
				}
				var sharedResources = sbSharedResources.ToString().Split(';').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x));
				foreach (string strResources in sharedResources)
				{
					var arr = strResources.Split(new string[] { ":=:" }, StringSplitOptions.None).Select(x => x.Trim()).ToArray();
					_dicValuesByTheirVariableNames.Add(arr[0], arr[1]);
				}

				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(strFileToEdit);

				var lines = File.ReadAllLines(strTextFileWithXPaths);
				foreach (var line in lines)
				{
					if (string.IsNullOrWhiteSpace(line) || line.Trim().StartsWith("#"))
					{
						continue;
					}

					var arr = line.Split(new string[] { ":=:" }, StringSplitOptions.None).Select(x => x.Trim()).ToArray();
					var node = xmlDocument.DocumentElement.SelectSingleNode(arr[0]);
					if (node == null)
					{
						var nodeArray = arr[0].Split('/');
						if (nodeArray[nodeArray.Length - 1].StartsWith("@"))
						{
							node = xmlDocument.DocumentElement.SelectSingleNode(arr[0].Substring(0, arr[0].Length - nodeArray[nodeArray.Length -1].Length -1));
							if (node != null)
							{
								node.SetAttributeValue(nodeArray[nodeArray.Length - 1].Trim('@'), "");
								node = xmlDocument.DocumentElement.SelectSingleNode(arr[0]);
							}
						}
					}

					if (node != null)
					{
						string strValue = arr[1].Trim().Trim('\"');
						if (strValue.StartsWith("%") && strValue.EndsWith("%") && _dicValuesByTheirVariableNames.ContainsKey(strValue.Trim('%')))
						{
							strValue = _dicValuesByTheirVariableNames[strValue.Trim('%')];
						}

                        if (!strValue.StartsWith("%") || !strValue.EndsWith("%"))
                        {
                            node.InnerText = strValue;
                        }
					}
				}

				xmlDocument.Save(strFileToEdit);
            }
			catch
			{ }
		}
	}
}
