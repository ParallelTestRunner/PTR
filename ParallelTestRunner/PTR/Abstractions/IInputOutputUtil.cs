using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTR
{
	interface IInputOutputUtil
	{
		string CurrentWorkingDirectory { get; }
		void CreateDirectoryIfDoesNotExist(string strDirectory);
		void CopyDirectoryWithAllItsContent(string strSourceDirectory, string strDestinationDirectory, List<string> lstFoldersOrFilesToBeIgnored = null);
		void DeleteDirectoryAndAllItsContent(string strDirectory, List<string> lstFolderOrFileNamesToBeIgnored = null);
	}
}
