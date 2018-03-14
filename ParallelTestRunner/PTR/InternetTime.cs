using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace PTR
{
	class InternetTime
	{
		private static int m_Index = 5;
		private static List<string> lstStrTimeServer = new List<string>();
		private static void SetTimeServers()
		{
			if (lstStrTimeServer.Count > 0)
			{
				return;
			}

			lstStrTimeServer.Add("tick.usno.navy.mil");
			lstStrTimeServer.Add("time.windows.com");
			lstStrTimeServer.Add("time.nist.gov");
			lstStrTimeServer.Add("europe.pool.ntp.org");
			lstStrTimeServer.Add("north-america.pool.ntp.org");
			lstStrTimeServer.Add("it.pool.ntp.org");

			// Below is just for testing
			//lstStrTimeServer.Add("tick.usno.navy.mil");
			//lstStrTimeServer.Add("time.nist.gov");
			//lstStrTimeServer.Add("europe.pool.ntp.org");
			//lstStrTimeServer.Add("north-america.pool.ntp.org");
			//lstStrTimeServer.Add("it.pool.ntp.org");
			//lstStrTimeServer.Add("time.windows.com");
		}

		private static bool m_bGotTimeFromInternet = false;
		public static bool GotTimeFromInternet
		{
			get { return m_bGotTimeFromInternet; }
		}
		private static DateTime m_DateTime = DateTime.Now;
		public static DateTime PreviousInternetDateTime
		{
			get { return m_DateTime; }
		}
		public static DateTime GetInternetTime()
		{
			InternetTime.m_Index = new Random().Next(0, 5);
			//InternetTime.lstStrTimeServer.Clear();
			InternetTime.SetTimeServers();
			InternetTime.m_DateTime = DateTime.Now;
			InternetTime.m_bGotTimeFromInternet = false;

			//while (InternetTime.m_Index >= 4)
			{
				InternetTime.TimeThread();
				//Thread.Sleep(1500);
				if (InternetTime.m_bGotTimeFromInternet)
				{
					InternetTime.m_thread = null;
					return InternetTime.m_DateTime;
				}
				else
				{
					try
					{
						InternetTime.m_thread.Priority = ThreadPriority.Lowest;
						InternetTime.m_thread.Abort();
					}
					catch
					{ }
					InternetTime.m_Index--;
				}
			}

			InternetTime.lstStrTimeServer.Clear();
			return InternetTime.m_DateTime;
		}

		private static Thread m_thread = null;
		private static void TimeThread()
		{
			try
			{
				InternetTime.m_thread = new Thread(new ThreadStart(InternetTime.SetInternetTimeUsingNTPClient));
				InternetTime.m_thread.IsBackground = true;
				InternetTime.m_thread.Start();
			}
			catch
			{ }
		}

		private static void SetInternetTimeUsingNTPClient()
		{
			try
			{
				NTPClient NTPClient = new NTPClient();
				NTPClient.GetInternetTime(InternetTime.lstStrTimeServer[InternetTime.m_Index]);
				lock (typeof(InternetTime))
				{
					if (!InternetTime.m_bGotTimeFromInternet)
					{
						InternetTime.m_DateTime = NTPClient.TransmitTimestamp;
						InternetTime.m_bGotTimeFromInternet = true;
					}
				}
			}
			catch
			{ }
		}
	}
}
