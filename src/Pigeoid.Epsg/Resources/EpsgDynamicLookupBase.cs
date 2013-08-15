// TODO: source header

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Pigeoid.Epsg.Resources
{
    [ContractClass(typeof(EpsgDynamicLookUpBaseContract<,>))]
	internal abstract class EpsgDynamicLookUpBase<TKey, TValue> :
		EpsgLookUpBase<TKey, TValue>
		where TValue : class
	{

		private readonly ConcurrentDictionary<TKey, TValue> _lookUp;
		private readonly TKey[] _orderedKeys;

		private bool _fullReadPerformed;
		private readonly object _fullReadMutex; // TODO: interlocked may be better

		/// <summary>
		/// Concrete classes must initialize the <c>AllOrderedKeys</c> field from their constructor.
		/// </summary>
		protected EpsgDynamicLookUpBase(TKey[] orderedKeys) {
            Contract.Requires(orderedKeys != null);

			// TODO: ONLY if in debug, make sure the keys are ordered?

			_lookUp = new ConcurrentDictionary<TKey, TValue>();
			_fullReadPerformed = false;
			_fullReadMutex = new object();
			_orderedKeys = orderedKeys;
		}

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(_lookUp != null);
            Contract.Invariant(_orderedKeys != null);
            Contract.Invariant(_fullReadMutex != null);
        }

		internal override IEnumerable<TValue> Values {
			get {
                Contract.Ensures(Contract.Result<IEnumerable<TValue>>() != null);
				if(!_fullReadPerformed)
					SingleFullRead();
				// TODO: should this collection be cached?
				return _lookUp.Values.OrderBy(GetKeyForItem);
			}
		}

		internal override IEnumerable<TKey> Keys {
			get {
                Contract.Ensures(Contract.Result<IEnumerable<TKey>>() != null);
			    return _orderedKeys;
			}
		}

		public override TValue Get(TKey key) {
			// try to find the item
			TValue item;
			if (_lookUp.TryGetValue(key, out item))
				return item;
			// see if we have a key for it
			var i = Array.BinarySearch(_orderedKeys, key);
			if (i < 0)
				return default(TValue);
			// if we do have a key for it, try a get add
			// we do a GetOrAdd instead of TryAdd just in case somebody beat us to it
            Contract.Assume(key != null);
			return _lookUp.GetOrAdd(key, k => Create(k,i));
		}

		protected abstract TValue Create(TKey key, int index);

		protected abstract TKey GetKeyForItem(TValue value);

		private void SingleFullRead() {
			lock(_fullReadMutex) {
				// this is a very heavy operation so we want to be sure it has not been done before
				if (_fullReadPerformed)
					return; // could happen, so we check again to be sure

				for (int i = 0; i < _orderedKeys.Length; i++) {
					// if TryAdd used a delegate we could have use that, but we want to only have one reference for each code
					// call directly on the look-up to avoid another key look-up
					var key = _orderedKeys[i];
					var localIndex = i;
                    Contract.Assume(key != null);
					_lookUp.GetOrAdd(key, k => Create(k, localIndex));
				}
				_fullReadPerformed = true;
			}
		}

	}

    [ContractClassFor(typeof(EpsgDynamicLookUpBase<,>))]
    internal abstract class EpsgDynamicLookUpBaseContract<TKey, TValue> :
        EpsgDynamicLookUpBase<TKey, TValue>
        where TValue : class
    {
        public EpsgDynamicLookUpBaseContract() : base(null) { }

        protected override TKey GetKeyForItem(TValue value) {
            Contract.Requires(value != null);
            return default(TKey);
        }

    }

}
