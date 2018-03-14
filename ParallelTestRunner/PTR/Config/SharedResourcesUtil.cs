using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTR
{
	class SharedResourcesUtil
	{
		private static Object _lockedObject = new object();
		private static SharedResourcesSection _sharedResourcesSection;
		private static Dictionary<string, int> _dicResourceIDsByTheirUsageCount;
		public static ResourceSection GetLeastUsedResource(IEnumerable<string> IDs)
		{
			InitializeResourceUsages();

			string resourceIdWithLeastUsage = string.Empty;
			int iUsageCount = 9999;
			lock (_lockedObject)
			{
				foreach (string strID in IDs)
				{
					if (_dicResourceIDsByTheirUsageCount[strID] < iUsageCount)
					{
						resourceIdWithLeastUsage = strID;
						iUsageCount = _dicResourceIDsByTheirUsageCount[strID];
					}
				}

				_dicResourceIDsByTheirUsageCount[resourceIdWithLeastUsage]++;
			}

			return _sharedResourcesSection.ResourcekEntries.Where(x => x.ID == resourceIdWithLeastUsage).First();
		}

		public static void ResourceUsageCompleted(ResourceSection resource)
		{
			lock (_lockedObject)
			{
				_dicResourceIDsByTheirUsageCount[resource.ID]--;
			}
		}

		public static IEnumerable<ResourceSection> SharedResources
		{
			get
			{
				InitializeResourceUsages();
				return _sharedResourcesSection.ResourcekEntries;
			}
		}

		private static void InitializeResourceUsages()
		{
			lock (_lockedObject)
			{
				if (_sharedResourcesSection == null)
				{
					_sharedResourcesSection = (SharedResourcesSection)ConfigurationManager.GetSection("SharedResources");
					_dicResourceIDsByTheirUsageCount = new Dictionary<string, int>();
					foreach (ResourceSection resource in _sharedResourcesSection.ResourcekEntries)
					{
						_dicResourceIDsByTheirUsageCount.Add(resource.ID, 0);
					}
				}
			}
		}
	}
}
