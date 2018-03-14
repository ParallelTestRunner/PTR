using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTR
{
	static class EnumerableExtensions
	{
		public static IEnumerable<IEnumerable<T>> SplitByChunk<T>(this IEnumerable<T> source, int chunkSize)
		{
			return source.Select((x, i) => new { data = x, indexgroup = i / chunkSize })
								.GroupBy(x => x.indexgroup, x => x.data)
								.Select(g => new List<T>(g));
		}

		public static IEnumerable<IEnumerable<T>> SplitByBinSize<T>(this IEnumerable<T> source, int binSize)
		{
			return source.Select((x, i) => new { data = x, indexgroup = i % binSize })
								.GroupBy(x => x.indexgroup, x => x.data)
								.Select(g => new List<T>(g));
		}

		public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
		{
			Random random = new Random();
			var sourceList = source.ToList();
			for (int i = 0; i < sourceList.Count; i++)
			{
				Swap(sourceList, i, random.Next(0, sourceList.Count - 1));
			}
			return sourceList;
		}

		private static void Swap<T>(List<T> list, int positionX, int positionY)
		{
			if (positionX == positionY)
			{
				return;
			}

			T atPositionX = list[positionX];
			list[positionX] = list[positionY];
			list[positionY] = atPositionX;
		}

		public static bool ContainsAll<T>(this IEnumerable<T> source, IEnumerable<T> values)
		{
			return !values.Except(source).Any();
		}
	}
}
