using System;
using System.Collections.Generic;

namespace Pigeoid.Epsg.Resources
{
	internal abstract class EpsgFixedLookupBase<TKey, TValue> : EpsgLookupBase<TKey, TValue>
	{

		protected readonly SortedDictionary<TKey, TValue> _lookup;

		/// <summary>
		/// Concrete classes must initialize the <c>Lookup</c> field from their constructor.
		/// </summary>
		protected EpsgFixedLookupBase(SortedDictionary<TKey, TValue> lookup) {
			if(null == lookup)
				throw new ArgumentNullException();

			_lookup = lookup;
		}

		public override TValue Get(TKey key) {
			return _lookup[key];
		}

		public override IEnumerable<TKey> Keys {
			get { return _lookup.Keys; }
		}

		public override IEnumerable<TValue> Values {
			get { return _lookup.Values; }
		}

	}
}
