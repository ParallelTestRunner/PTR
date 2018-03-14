using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTR
{
	static class InputOutputOperationsExtensions
	{
		public static string GetFullPath(this string strPath,
											IInputOutputUtil IInputOutputUtil,
											string strAnyOtherParentPathThatCouldBeUsedToGetFullPath = null)
		{
			if (string.IsNullOrEmpty(strPath) || strPath == ".")
			{
				if (!string.IsNullOrEmpty(strAnyOtherParentPathThatCouldBeUsedToGetFullPath) && File.Exists(strAnyOtherParentPathThatCouldBeUsedToGetFullPath))
				{
					return strAnyOtherParentPathThatCouldBeUsedToGetFullPath;
				}
				return IInputOutputUtil.CurrentWorkingDirectory;
			}

			string strTempPath = string.Empty;
			if (!string.IsNullOrEmpty(strAnyOtherParentPathThatCouldBeUsedToGetFullPath) &&
					ComputePathRelativeToGivenParentPath(strAnyOtherParentPathThatCouldBeUsedToGetFullPath, strPath, ref strTempPath))
			{
				return strTempPath;
			}

			if (strPath == "..")
			{
				if (!string.IsNullOrEmpty(strAnyOtherParentPathThatCouldBeUsedToGetFullPath) && File.Exists(strAnyOtherParentPathThatCouldBeUsedToGetFullPath))
				{
					return Directory.GetParent(strAnyOtherParentPathThatCouldBeUsedToGetFullPath).FullName;
				}
				return Directory.GetParent(IInputOutputUtil.CurrentWorkingDirectory).FullName;
			}

			if (ComputePathRelativeToGivenParentPath(IInputOutputUtil.CurrentWorkingDirectory, strPath, ref strTempPath))
			{
				return strTempPath;
			}

			return strPath;
		}

		private static bool ComputePathRelativeToGivenParentPath(string strParentPath, string strPath, ref string strComputedPath)
		{
			string strTempPath = Path.Combine(strParentPath.Trim('\\'), strPath.Trim('\\'));
			if (File.Exists(strTempPath) || Directory.Exists(strTempPath))
			{
				strComputedPath = strTempPath;
				return true;
			}

			return false;
		}
	}
}
