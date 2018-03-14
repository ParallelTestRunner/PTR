using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUtil
{
	internal class En
	{
		private static char[] charReplaceArray = new char[10];

		private static int EN_VALUE1 = 3;
		private static int EN_VALUE2 = 5;

		private static int MAX_POS_DISTRIBUTION = 7;
		private static int[] PosSwap = new int[MAX_POS_DISTRIBUTION];
		private static int[] PrimePos = new int[MAX_POS_DISTRIBUTION];
		static En()
		{
			PosSwap[0] = 4;
			PosSwap[1] = 2;
			PosSwap[2] = 5;
			PosSwap[3] = 0;
			PosSwap[4] = 3;
			PosSwap[5] = 6;
			PosSwap[6] = 1;

			PrimePos[0] = 5;
			PrimePos[1] = 7;
			PrimePos[2] = 12;
			PrimePos[3] = 16;
			PrimePos[4] = 23;
			PrimePos[5] = 25;
			PrimePos[6] = 28;

			charReplaceArray[0] = 'a';
			charReplaceArray[1] = 's';
			charReplaceArray[2] = '4';
			charReplaceArray[3] = 'A';
			charReplaceArray[4] = '7';
			charReplaceArray[5] = 'i';
			charReplaceArray[6] = '1';
			charReplaceArray[7] = 'k';
			charReplaceArray[8] = 'N';
			charReplaceArray[9] = '0';
		}

		private static void charReplaceF(ref char ch)
		{
			if (ch == 'a')
			{
				ch = '0';
			}
			else if (ch == 's')
			{
				ch = '1';
			}
			else if (ch == '4')
			{
				ch = '2';
			}
			else if (ch == 'A')
			{
				ch = '3';
			}
			else if (ch == '7')
			{
				ch = '4';
			}
			else if (ch == 'i')
			{
				ch = '5';
			}
			else if (ch == '1')
			{
				ch = '6';
			}
			else if (ch == 'k')
			{
				ch = '7';
			}
			else if (ch == 'N')
			{
				ch = '8';
			}
			else if (ch == '0')
			{
				ch = '9';
			}
		}

		// Separator
		static public string SEP
		{
			get
			{
				StringBuilder sbLicF = new StringBuilder("wg4hkX6t7nVjsnuWpy3agwmxbdol5n@maUfz48dnKapEcnj");
				sbLicF.Remove(0, 11);
				sbLicF.Remove(1, 5);
				sbLicF.Remove(2, 7);
				sbLicF.Remove(3, 2);
				sbLicF.Remove(4, 2);
				sbLicF.Remove(5, 2);
				sbLicF.Remove(6, 4);
				sbLicF.Remove(8, sbLicF.Length - 8);
				sbLicF.Insert(3, "0%");
				return sbLicF.ToString();
			}
		}

		private static Random RandomNumberGenerator = new Random();
		private static string GetNextValue(List<string> strListSubNumericTime, bool bFromTheListItself, int iMinRandomDigit, int iMaxRandomDigit)
		{
			int iValue = En.RandomNumberGenerator.Next(iMinRandomDigit, iMaxRandomDigit);
			if (bFromTheListItself)
			{
				return strListSubNumericTime[iValue % strListSubNumericTime.Count];
			}

			return iValue.ToString();
		}

		public static string GetKey(string strFormat)
		{
			StringBuilder sbLockKey = new StringBuilder();
			int iValue = En.RandomNumberGenerator.Next(4, 9);
			int k = 0;
			while (0 != iValue)
			{
				k = En.RandomNumberGenerator.Next(11, 101);
				if (0 == k % 4)
				{
					sbLockKey.Append(En.RandomNumberGenerator.Next(11, 101).ToString());
				}
				else if (0 == k % 3)
				{
					sbLockKey.Append(En.RandomNumberGenerator.Next(101, 1100).ToString());
				}
				else
				{
					sbLockKey.Append(En.RandomNumberGenerator.Next(1100, 11000).ToString());
				}

				iValue--;
			}
			sbLockKey.Append(En.SEP);

			sbLockKey.Append(strFormat);

			sbLockKey.Append(En.SEP);
			iValue = En.RandomNumberGenerator.Next(3, 6);
			k = 0;
			while (0 != iValue)
			{
				k = En.RandomNumberGenerator.Next(11, 101);
				if (0 == k % 4)
				{
					sbLockKey.Append(En.RandomNumberGenerator.Next(101, 1100).ToString());
				}
				else if (0 == k % 3)
				{
					sbLockKey.Append(En.RandomNumberGenerator.Next(1100, 11000).ToString());
				}
				else
				{
					sbLockKey.Append(En.RandomNumberGenerator.Next(11, 101).ToString());
				}

				iValue--;
			}
			strFormat = sbLockKey.ToString();

			char[] charArray = null;
			int i = 0;
			List<string> strListSubNumericTime = new List<string>();
			int iMinRandomDigit = 2;
			int iMaxRandomDigit = 3;

			int iEnCharValue = 0;
			foreach (char ch in strFormat)
			{
				if (0 == i)
				{
					charArray = new char[En.MAX_POS_DISTRIBUTION];
				}
				charArray[i] = ch;

				if (i == En.MAX_POS_DISTRIBUTION - 1)
				{
					for (k = 0; k < En.MAX_POS_DISTRIBUTION; k++)
					{
						iEnCharValue = System.Convert.ToInt32(charArray[En.PosSwap[k]]);
						iEnCharValue += En.EN_VALUE1;
						strListSubNumericTime.Add(iEnCharValue.ToString());
						if (iMinRandomDigit > strListSubNumericTime[strListSubNumericTime.Count - 1].Length)
						{
							iMinRandomDigit = strListSubNumericTime[strListSubNumericTime.Count - 1].Length;
						}
						if (iMaxRandomDigit < strListSubNumericTime[strListSubNumericTime.Count - 1].Length)
						{
							iMaxRandomDigit = strListSubNumericTime[strListSubNumericTime.Count - 1].Length;
						}
					}
					charArray = null;
					i = 0;
				}
				else
				{
					i++;
				}
			}

			if (i > 0)
			{
				while (i > 0)
				{
					iEnCharValue = System.Convert.ToInt32(charArray[--i]);
					iEnCharValue += En.EN_VALUE2;
					strListSubNumericTime.Add(iEnCharValue.ToString());
				}
			}

			iMinRandomDigit = System.Convert.ToInt32(Math.Pow(10, iMinRandomDigit - 1));
			iMaxRandomDigit = System.Convert.ToInt32(Math.Pow(10, iMaxRandomDigit)) - 1;

			string strLastRandom = "0";
			int iLastRandomTotal = 0;
			int m = 0;
			k = 0;
			int PrimePosIndex = 0;
			List<string> strListEnSubNumericTime = new List<string>();
			StringBuilder sbEnSubNumericTime = new StringBuilder();
			for (i = 0; i < strListSubNumericTime.Count; i++)
			{
				iLastRandomTotal = 0;
				for (m = PrimePosIndex; m < En.PrimePos[k]; m++)
				{
					strLastRandom = En.GetNextValue(strListSubNumericTime, 0 == m % 2, iMinRandomDigit, iMaxRandomDigit);
					sbEnSubNumericTime.Append(strLastRandom);
					sbEnSubNumericTime.Append(" ");
				}

				foreach (char ch in strLastRandom)
				{
					iLastRandomTotal = (iLastRandomTotal * 10) + Int32.Parse(ch.ToString());
				}

				if (iLastRandomTotal != 299)
				{
					iLastRandomTotal %= 299;
					iLastRandomTotal = (iLastRandomTotal >= 21) ? iLastRandomTotal : (iLastRandomTotal + 51);
				}

				iLastRandomTotal += Int32.Parse(strListSubNumericTime[i]);

				sbEnSubNumericTime.Append(iLastRandomTotal.ToString());
				sbEnSubNumericTime.Append(" ");

				PrimePosIndex = PrimePos[k] + 1;
				k++;
				if (k == En.MAX_POS_DISTRIBUTION)
				{
					sbEnSubNumericTime.Append(En.GetNextValue(strListSubNumericTime, false, iMinRandomDigit, iMaxRandomDigit));
					sbEnSubNumericTime.Append(" ");

					strListEnSubNumericTime.Add(sbEnSubNumericTime.ToString());
					k = 0;
					PrimePosIndex = 0;
					sbEnSubNumericTime.Remove(0, sbEnSubNumericTime.Length);
				}
			}

			if (k > 0)
			{
				strListEnSubNumericTime.Add(sbEnSubNumericTime.ToString());
				sbEnSubNumericTime.Remove(0, sbEnSubNumericTime.Length);
			}

			for (i = 0; i < strListEnSubNumericTime.Count; i++)
			{
				sbEnSubNumericTime.Append(strListEnSubNumericTime[i]);
			}

			sbEnSubNumericTime.Remove(sbEnSubNumericTime.Length - 1, 1);

			strFormat = sbEnSubNumericTime.ToString();
			i = 0;
			char charReplace;
			foreach (char ch in strFormat)
			{
				if (ch != ' ')
				{
					iEnCharValue = Int32.Parse(ch.ToString());
					charReplace = En.charReplaceArray[iEnCharValue];
					sbEnSubNumericTime.Replace(ch, charReplace, i, 1);
				}
				i++;
			}

			return sbEnSubNumericTime.ToString();
		}

		public static string GetLicenseInfo(string strTime, string strFormat)
		{
			char charReplace;
			StringBuilder sbCharReplace = new StringBuilder();

			string strLastRandom = "0";
			int iLastRandomTotal = 0;
			List<string> strListSubNumericTime = new List<string>();
			string[] strArrayEnSubNumericTime = strFormat.Split();
			int iStartingIndex = 0;
			int k = 0;
			int i = En.PrimePos[k];
			while ((i + iStartingIndex) < strArrayEnSubNumericTime.Length)
			{
				strTime = strArrayEnSubNumericTime[i + iStartingIndex];
				foreach (char ch in strTime)
				{
					charReplace = ch;
					En.charReplaceF(ref charReplace);
					sbCharReplace.Append(charReplace);
				}

				iLastRandomTotal = 0;
				strLastRandom = strArrayEnSubNumericTime[i + iStartingIndex - 1];
				foreach (char ch in strLastRandom)
				{
					charReplace = ch;
					En.charReplaceF(ref charReplace);
					iLastRandomTotal = (iLastRandomTotal * 10) + Int32.Parse(charReplace.ToString());
				}

				if (iLastRandomTotal != 299)
				{
					iLastRandomTotal %= 299;
					iLastRandomTotal = (iLastRandomTotal >= 21) ? iLastRandomTotal : (iLastRandomTotal + 51);
				}

				iLastRandomTotal = (Int32.Parse(sbCharReplace.ToString())) - iLastRandomTotal;

				strListSubNumericTime.Add(iLastRandomTotal.ToString());
				sbCharReplace.Remove(0, sbCharReplace.Length);

				if (i == En.PrimePos[En.PrimePos.Length - 1])
				{
					k = 0;
					iStartingIndex += En.PrimePos[En.PrimePos.Length - 1] + 2;
				}
				else
				{
					k++;
				}
				i = En.PrimePos[k];
			}

			i = 0;
			iStartingIndex = 0;
			char[] strArrOrderedSubNumericTime = new char[strListSubNumericTime.Count];
			while (i < strListSubNumericTime.Count)
			{
				if (strListSubNumericTime.Count - iStartingIndex < En.PosSwap.Length)
				{
					break;
				}

				k = Int32.Parse(strListSubNumericTime[i]);
				k -= En.EN_VALUE1;
				strArrOrderedSubNumericTime[iStartingIndex + En.PosSwap[i - iStartingIndex]] = System.Convert.ToChar(k);

				if ((i - iStartingIndex) == (En.PosSwap.Length - 1))
				{
					iStartingIndex += PosSwap.Length;
				}
				i++;
			}

			if (strListSubNumericTime.Count - iStartingIndex < En.PosSwap.Length)
			{
				int m = i;
				k = strListSubNumericTime.Count - 1;
				while (k >= i)
				{
					iStartingIndex = Int32.Parse(strListSubNumericTime[k--]);
					iStartingIndex -= En.EN_VALUE2;
					strArrOrderedSubNumericTime[m++] = System.Convert.ToChar(iStartingIndex);
				}
			}

			StringBuilder sbTime = new StringBuilder();
			for (i = 0; i < strArrOrderedSubNumericTime.Length; i++)
			{
				sbTime.Append(strArrOrderedSubNumericTime[i]);
			}

			strTime = sbTime.ToString();

			strTime = strTime.Substring(strTime.IndexOf(En.SEP) + En.SEP.Length);
			strTime = strTime.Substring(0, strTime.LastIndexOf(En.SEP));

			return strTime;
		}
	}
}
