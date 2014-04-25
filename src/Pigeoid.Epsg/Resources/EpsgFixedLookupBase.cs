using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Pigeoid.Epsg.Resources
{
	internal class EpsgFixedLookUpBase<TKey, TValue> :
		EpsgLookUpBase<TKey, TValue>
		where TValue : class
	{

		protected readonly SortedDictionary<TKey, TValue> LookUpCore;

		/// <summary>
		/// Concrete classes must initialize the <c>Lookup</c> field from their constructor.
		/// </summary>
		internal EpsgFixedLookUpBase(SortedDictionary<TKey, TValue> lookUpCore) {
			if(null == lookUpCore)
				throw new ArgumentNullException();

			LookUpCore = lookUpCore;
		}

        [Pure]
		public override TValue Get(TKey key) {
			TValue item;
			LookUpCore.TryGetValue(key, out item);
			return item;
		}

		internal override IEnumerable<TKey> Keys {
			get { return LookUpCore.Keys; }
		}

        internal override IEnumerable<TValue> Values {
			get { return LookUpCore.Values; }
		}

	}
}
