using System.Collections.Generic;

namespace Pigeoid.Epsg.Resources
{
	internal abstract class EpsgLookUpBase<TKey, TValue>
	{

        public abstract TValue Get(TKey key);

		internal abstract IEnumerable<TKey> Keys { get; }

        internal abstract IEnumerable<TValue> Values { get; }

	}
}
