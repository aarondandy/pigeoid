using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Pigeoid.Unit;

namespace Pigeoid.Epsg
{
    public abstract class EpsgCrsGeodetic : EpsgCrsDatumBased, ICrsGeodetic
    {

        internal EpsgCrsGeodetic(int code, string name, EpsgArea area, bool deprecated, EpsgCoordinateSystem cs, EpsgDatumGeodetic geodeticDatum, EpsgCrsGeodetic baseCrs, int baseOperationCode)
            : base(code, name, area, deprecated, cs) {
            Contract.Requires(code >= 0);
            Contract.Requires(!String.IsNullOrEmpty(name));
            Contract.Requires(area != null);
            Contract.Requires(cs != null);
            Contract.Requires(geodeticDatum != null);
            GeodeticDatum = geodeticDatum;
            BaseCrs = baseCrs;
            BaseOperationCode = baseOperationCode;
        }

        [ContractInvariantMethod]
        private void ObjectInvariants() {
            Contract.Invariant(GeodeticDatum != null);
        }

        public override EpsgDatum Datum {
            get {
                Contract.Ensures(Contract.Result<EpsgDatum>() != null);
                return GeodeticDatum;
            }
        }

        public EpsgCrsGeodetic BaseCrs { get; private set; }

        public bool HasBaseOperationCode { get { return BaseOperationCode > 0; } }

        public int BaseOperationCode { get; private set; }

        public EpsgDatumGeodetic GeodeticDatum { get; private set; }

        IDatumGeodetic ICrsGeodetic.Datum { get { return GeodeticDatum; } }

        public EpsgUnit Unit {
            get {
                Contract.Ensures(Contract.Result<EpsgUnit>() != null);
                var axes = CoordinateSystem.Axes;
                if (axes.Count == 0)
                    throw new NotImplementedException();
                Contract.Assume(axes[0] != null);
                return axes[0].Unit;
            }
        }

        IUnit ICrsGeodetic.Unit { get { return Unit; } }

        public IList<EpsgAxis> Axes {
            get {
                Contract.Ensures(Contract.Result<IList<EpsgAxis>>() != null);
                return CoordinateSystem.Axes.ToArray();
            }
        }

        IList<IAxis> ICrsGeodetic.Axes { get { return Axes.Cast<IAxis>().ToArray(); } }

        public abstract override EpsgCrsKind Kind { get; }
    }

    public class EpsgCrsGeocentric : EpsgCrsGeodetic, ICrsGeocentric
    {
        internal EpsgCrsGeocentric(int code, string name, EpsgArea area, bool deprecated, EpsgCoordinateSystem cs, EpsgDatumGeodetic geodeticDatum, EpsgCrsGeodetic baseCrs, int baseOperationCode)
            : base(code, name, area, deprecated, cs, geodeticDatum, baseCrs, baseOperationCode) {
            Contract.Requires(code >= 0);
            Contract.Requires(!String.IsNullOrEmpty(name));
            Contract.Requires(area != null);
            Contract.Requires(cs != null);
            Contract.Requires(geodeticDatum != null);
        }

        public override EpsgCrsKind Kind { get { return EpsgCrsKind.Geocentric; } }
    }

    public class EpsgCrsGeographic : EpsgCrsGeodetic, ICrsGeographic
    {

        private readonly EpsgCrsKind _kind;

        internal EpsgCrsGeographic(int code, string name, EpsgArea area, bool deprecated, EpsgCoordinateSystem cs, EpsgDatumGeodetic geodeticDatum, EpsgCrsGeodetic baseCrs, int baseOperationCode, EpsgCrsKind kind)
            : base(code, name, area, deprecated, cs, geodeticDatum, baseCrs, baseOperationCode) {
            Contract.Requires(code >= 0);
            Contract.Requires(!String.IsNullOrEmpty(name));
            Contract.Requires(area != null);
            Contract.Requires(cs != null);
            Contract.Requires(geodeticDatum != null);
            _kind = kind;
        }

        public override EpsgCrsKind Kind { get { return _kind; } }
    }

}
