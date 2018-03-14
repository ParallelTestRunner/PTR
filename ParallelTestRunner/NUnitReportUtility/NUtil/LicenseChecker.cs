using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Management;

namespace NUtil
{
	class LicenseChecker : ILicenseChecker
	{
		private const string LockFile = "PTR.LOCK";
		private const string KeyFile = "PTR.KEY";

        private bool _bGiveFreeLicense = true;
        public bool DoesThisMachineHaveAValidLicense()
        {
            if (_bGiveFreeLicense)
            {
                return true;
            }

            if (!System.IO.File.Exists(LockFile))
            {
				StringBuilder sbLock = new StringBuilder(GetHardDriveSerialKey());
				sbLock.Append(En.SEP);
				sbLock.Append(GetMacAddress());
				string strLock = En.GetKey(sbLock.ToString());
				using (System.IO.FileStream FileStream = System.IO.File.Open(LockFile, System.IO.FileMode.CreateNew, System.IO.FileAccess.Write))
				{
					System.IO.StreamWriter StreamWriter = new System.IO.StreamWriter(FileStream);
					StreamWriter.WriteLine(strLock);
					StreamWriter.Flush();
					FileStream.Flush();
				}
			}

			if (!System.IO.File.Exists(KeyFile))
			{
				StringBuilder sbKey = new StringBuilder(GetHardDriveSerialKey());
				sbKey.Append(En.SEP);
				sbKey.Append(GetMacAddress());
				string strKey = En.GetKey(sbKey.ToString());
				using (System.IO.FileStream FileStream = System.IO.File.Open(KeyFile, System.IO.FileMode.CreateNew, System.IO.FileAccess.Write))
				{
					System.IO.StreamWriter StreamWriter = new System.IO.StreamWriter(FileStream);
					StreamWriter.WriteLine(strKey);
					StreamWriter.WriteLine(@"ik 4Ak 1a 4ak ssA s11 1a ss7 is kN 1a 40A A7i 0iN is Nas 41a i77 ik 71s i0 iN i1 ssi i0 ssA is si4 4s4 k17 i4 417 i7 7Ai Na sAA iN ssi i0 ka4 sai N4 sA7 A71 ii 0N4 s7i 71k 7a Ais is 1a4 is sai ssA skA i4 k17 444 7is i7 ii7 i1 sis is ssa 1a ssi 1s 1i0 i0 40i Ai7 7ks is 07s sas 4A7 ii 4ki ik 04s i4 sa0 i7 ss4 7a 71i 441 ks1 i0 004 iA ka1 i7 sa0 i7 sss saA kk1 ss4 ss s4a Nss i7 Nk0 AA4 70a ik 4k 1a k4k i7 sak Na sN0 is i4s 4kA 4A7 1a s4k kN AAk ia sa1 7a 0s 7a 0s ia 7i0 4k4 NAk Na sik 4Ns ksN s47 AsA iA 0Ni i1 01 1a s1i i7 sA0 474 1AA ii kN7 ss4 0aA saA siA ss4 s1A ss4 477 i0 11a ssA k1N i0 iN0 7aA k0a sai 1NA ik NN7 i4 sa7 1a ss4 i1 i4 sAa sk i1 AaN 1s 74A ss4 sks is sa4 saA iN7 i1 ANa ss1 704 1a iis Aa4 NNa is kis is 4AN saA sii i7 sa1 sai ANN s74 1A0 is i44 ik Ass iA sa7 7a 0s ii 0ia i0 NsN 4ks kk1 is 1a7 ssN isk is ikN 1a s1a i4 ssA 1a 0i ia sak siN 4a4 sa0 Nas is s7A is si7 iA s14 iA AN0 i1 siA 4a7 11k i4 0aa s44 011 iA NA0 i1 1kN i4 sk1 i7 07 ik kkA 4ii NA0 ik 47A i7 1iN is s40 iA siN i4 1ss iA 14i Ni AAa i7 ia0 411 7a1 i7 i4N i7 i1k iN sks i4 saN saA 0A1 sis Nia 1s 1ss i4 As0 iA saN is sa1 is sak i4 7s0 sk4 01s i4 N00 ss4 4sa iA ksa is 7is is sai Na sAs is iNa AAi Aa4 1a 774 is si0 iA saN 1a ss4 i1 s0A ik si s4i A04 i1 i1 ss1 si7 saA 4as i4 1iA iN sa0 i1 ssA 1a i7k Aai i1A");
					StreamWriter.Flush();
					FileStream.Flush();
				}
			}

			System.IO.TextReader TextReader = new System.IO.StreamReader(KeyFile);

			string strKeyInfo = TextReader.ReadLine();
			string strDecryptedLock = En.GetLicenseInfo("", strKeyInfo);
			int iIndex = 0;
			string strHSDN = strDecryptedLock.Substring(iIndex, strDecryptedLock.IndexOf(En.SEP, iIndex) - iIndex);
			iIndex += strHSDN.Length;
			iIndex += En.SEP.Length;
			string strMANIC = strDecryptedLock.Substring(iIndex);

			if (InternetTime.GetInternetTime() <= DateTime.Parse(En.GetLicenseInfo("", TextReader.ReadLine())))
			{
				return true;
			}

			return false;
		}

		private string GetHardDriveSerialKey()
		{
			try
			{
				ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMedia");
				foreach (ManagementObject wmi_HD in searcher.Get())
				{
					// get the hardware serial no.
					if (wmi_HD["SerialNumber"] != null)
					{
						return wmi_HD["SerialNumber"].ToString().Trim();
					}
				}
			}
			catch(Exception)
			{ }

			return "NoSerialNumberFound";
		}

		private string GetMacAddress()
		{
			try
			{
				var networkInterface = GetNetworkInterface();
				if (networkInterface != null)
				{
					return networkInterface.GetPhysicalAddress().ToString().Trim();
				}
			}
			catch (Exception)
			{ }

			return "MacDoesNotExists";
		}

		private NetworkInterface GetNetworkInterface()
		{
			IPGlobalProperties computerProperties = IPGlobalProperties.GetIPGlobalProperties();
			if (computerProperties == null)
				return null;

			NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
			if (nics == null || nics.Length < 1)
				return null;

			NetworkInterface best = null;
			foreach (NetworkInterface adapter in nics)
			{
				if (adapter.NetworkInterfaceType == NetworkInterfaceType.Loopback || adapter.NetworkInterfaceType == NetworkInterfaceType.Unknown)
					continue;
				if (!adapter.Supports(NetworkInterfaceComponent.IPv4))
					continue;
				if (best == null)
					best = adapter;
				if (adapter.OperationalStatus != OperationalStatus.Up)
					continue;

				return adapter;
			}
			return best;
		}
	}
}
