using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Pigeoid.Epsg.Resources
{

    [ContractClass(typeof(EpsgLookUpBaseContracts<,>))]
	internal abstract class EpsgLookUpBase<TKey, TValue>
	{

        public abstract TValue Get(TKey key);

		internal abstract IEnumerable<TKey> Keys { get; }

        internal abstract IEnumerable<TValue> Values { get; }

	}

    [ContractClassFor(typeof(EpsgLookUpBase<,>))]
    internal abstract class EpsgLookUpBaseContracts<TKey, TValue> : EpsgLookUpBase<TKey, TValue>
    {

        public abstract override TValue Get(TKey key);

        internal override IEnumerable<TKey> Keys {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<TKey>>() != null);
                throw new System.NotImplementedException();
            }
        }

        internal override IEnumerable<TValue> Values {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<TValue>>() != null);
                throw new System.NotImplementedException();
            }
        }
    }

}
