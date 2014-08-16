using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using Pigeoid.Epsg.Resources;
using System.Collections.ObjectModel;

namespace Pigeoid.Epsg
{
    public class EpsgCoordinateSystem
    {

        [Obsolete]
        internal static readonly EpsgDataResourceReaderCoordinateSystem Reader = new EpsgDataResourceReaderCoordinateSystem();

        [Obsolete]
        public static EpsgCoordinateSystem Get(int code) {
            return code >= 0 && code <= UInt16.MaxValue
                ? Reader.GetByKey(unchecked((ushort)code))
                : null;
        }

        [Obsolete]
        public static IEnumerable<EpsgCoordinateSystem> Values {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<EpsgCoordinateSystem>>() != null);
                return Reader.ReadAllValues();
            }
        }

        private readonly ushort _code;
        private readonly int _dimension;
        private readonly bool _deprecated;
        private readonly EpsgCoordinateSystemKind _csType;

        internal EpsgCoordinateSystem(ushort code, string name, int dimension, bool deprecated, EpsgCoordinateSystemKind csType) {
            Contract.Requires(!String.IsNullOrEmpty(name));
            _code = code;
            Name = name;
            _dimension = dimension;
            _deprecated = deprecated;
            _csType = csType;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(!String.IsNullOrEmpty(Name));
        }

        public int Code { get { return _code; } }

        public string Name { get; private set; }

        public int Dimension { get { return _dimension; } }

        public bool Deprecated { get { return _deprecated; } }

        public EpsgCoordinateSystemKind Type { get { return _csType; } }

        public IList<EpsgAxis> Axes {
            get {
                Contract.Ensures(Contract.Result<IList<EpsgAxis>>() != null);
                Contract.Ensures(Contract.ForAll(Contract.Result<IList<EpsgAxis>>(), x => x != null));
                return EpsgAxis.Get(_code);
            }
        }

        public IAuthorityTag Authority {
            get {
                Contract.Ensures(Contract.Result<IAuthorityTag>() != null);
                return new EpsgAuthorityTag(_code);
            }
        }

    }
}
