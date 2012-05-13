using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Pigeoid.Epsg.Resources
{
	internal abstract class EpsgDynamicLookupBase<TKey, TValue> : EpsgLookupBase<TKey, TValue>
	{

		private readonly ConcurrentDictionary<TKey, TValue> _lookup;
		private readonly TKey[] _orderedKeys;

		private bool _fullReadPerformed;
		private readonly object _fullReadMutex; // TODO: interlocked may be better

		/// <summary>
		/// Concrete classes must initialize the <c>AllOrderedKeys</c> field from their constructor.
		/// </summary>
		protected EpsgDynamicLookupBase(TKey[] orderedKeys) {
			if(null == orderedKeys)
				throw new ArgumentNullException();

			// TODO: ONLY if in debug, make sure the keys are ordered?

			_lookup = new ConcurrentDictionary<TKey, TValue>();
			_fullReadPerformed = false;
			_fullReadMutex = new object();
			_orderedKeys = orderedKeys;
		}

		protected int GetKeyIndex(TKey key) {
			var result = Array.BinarySearch(_orderedKeys, key);
			if(result < 0)
				throw new InvalidDataException();

			return result;
		}

		public override IEnumerable<TValue> Values {
			get {
				if(!_fullReadPerformed)
					SingleFullRead();

				return _lookup.Values.OrderBy(GetKeyForItem);
			}
		}

		public override IEnumerable<TKey> Keys {
			get { return _orderedKeys.AsEnumerable(); }
		}

		public override TValue Get(TKey key) {
			return _lookup.GetOrAdd(key, Create);
		}

		protected abstract TValue Create(TKey key);

		protected abstract TKey GetKeyForItem(TValue value);

		private void SingleFullRead() {
			lock(_fullReadMutex) {
				if (_fullReadPerformed)
					return; // could happen, so we check again to be sure

				foreach (var key in _orderedKeys) {
					Get(key);
				}
				_fullReadPerformed = true;
			}
		}

	}
}
