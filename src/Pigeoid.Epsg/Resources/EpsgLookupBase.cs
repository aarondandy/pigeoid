using System.Collections.Generic;

namespace Pigeoid.Epsg.Resources
{
	internal abstract class EpsgLookupBase<TKey, TValue>
	{

		public abstract TValue Get(TKey key);

		public abstract IEnumerable<TKey> Keys { get; }

		public abstract IEnumerable<TValue> Values { get; }

	}
}
