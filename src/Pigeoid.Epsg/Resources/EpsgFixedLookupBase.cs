using System;
using System.Collections.Generic;

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

		public override TValue Get(TKey key) {
			TValue item;
			LookUpCore.TryGetValue(key, out item);
			return item;
		}

		public override IEnumerable<TKey> Keys {
			get { return LookUpCore.Keys; }
		}

		public override IEnumerable<TValue> Values {
			get { return LookUpCore.Values; }
		}

	}
}
