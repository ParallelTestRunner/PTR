using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PTR
{
	class InputOutputUtil : IInputOutputUtil
	{
		private string _currentWorkingDirectory;
		public string CurrentWorkingDirectory
		{
			get
			{
				if (string.IsNullOrEmpty(_currentWorkingDirectory))
				{
					_currentWorkingDirectory = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
				}
				return _currentWorkingDirectory;
			}
		}

		public void CreateDirectoryIfDoesNotExist(string strDirectory)
		{
			if (!Directory.Exists(strDirectory))
			{
				Directory.CreateDirectory(strDirectory);
			}
		}

		public void CopyDirectoryWithAllItsContent(string strSourceDirectory, string strDestinationDirectory, List<string> lstFoldersOrFilesToBeIgnored = null)
		{
			DirectoryInfo diSource = new DirectoryInfo(strSourceDirectory);
			DirectoryInfo diTarget = new DirectoryInfo(strDestinationDirectory);

			if (lstFoldersOrFilesToBeIgnored != null)
			{
				lstFoldersOrFilesToBeIgnored = lstFoldersOrFilesToBeIgnored.Select(x => x.NormalisedPath()).ToList();
			}

			this.CopyAllContent(diSource, diTarget, lstFoldersOrFilesToBeIgnored);
		}

		public void CopyAllContent(DirectoryInfo diSource, DirectoryInfo diTarget, List<string> lstFoldersOrFilesToBeIgnored)
		{
			Directory.CreateDirectory(diTarget.FullName);

			foreach (FileInfo fi in diSource.GetFiles())
			{
				string strFile = Path.Combine(diTarget.FullName, fi.Name);
				if (lstFoldersOrFilesToBeIgnored == null || lstFoldersOrFilesToBeIgnored.Contains(strFile.NormalisedPath()))
				{
					continue;
				}
				fi.CopyTo(strFile, true);
			}

			foreach (DirectoryInfo diSourceSubDir in diSource.GetDirectories())
			{
				if (lstFoldersOrFilesToBeIgnored == null || lstFoldersOrFilesToBeIgnored.Contains(diSourceSubDir.FullName.NormalisedPath()))
				{
					continue;
				}
				DirectoryInfo nextTargetSubDir = diTarget.CreateSubdirectory(diSourceSubDir.Name);
				CopyAllContent(diSourceSubDir, nextTargetSubDir, lstFoldersOrFilesToBeIgnored);
			}
		}

		public void DeleteDirectoryAndAllItsContent(string strDirectory, List<string> lstFolderOrFileNamesToBeIgnored = null)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(strDirectory);
			bool bDeleteCurrentDirectory = true;
			foreach (FileInfo fi in directoryInfo.GetFiles())
			{
				string strFile = Path.Combine(directoryInfo.FullName, fi.Name);
				if (lstFolderOrFileNamesToBeIgnored != null)
				{
					bool bDeleteCurrentFile = true;
					foreach (var folderOrFileName in lstFolderOrFileNamesToBeIgnored)
					{
						if (strFile.Contains(folderOrFileName))
						{
							bDeleteCurrentDirectory = false;
							bDeleteCurrentFile = false;
							break;
						}
					}

					if (!bDeleteCurrentFile)
					{
						continue;
					}
				}
				try
				{
					File.Delete(strFile);
				}
				catch
				{ }
			}

			foreach (DirectoryInfo diSubDir in directoryInfo.GetDirectories())
			{
				DeleteDirectoryAndAllItsContent(diSubDir.FullName, lstFolderOrFileNamesToBeIgnored);
			}

			if (bDeleteCurrentDirectory &&
				lstFolderOrFileNamesToBeIgnored != null &&
				directoryInfo.GetDirectories().Count() <= 0 &&
				directoryInfo.GetFiles().Count() <= 0)
			{
				foreach (var folderOrFileName in lstFolderOrFileNamesToBeIgnored)
				{
					if (strDirectory.Contains(folderOrFileName))
					{
						bDeleteCurrentDirectory = false;
						break;
					}
				}
			}

			if (bDeleteCurrentDirectory && directoryInfo.GetDirectories().Count() <= 0 && directoryInfo.GetFiles().Count() <= 0)
			{
				try
				{
					Directory.Delete(strDirectory);
				}
				catch
				{ }
			}
		}
	}
}
