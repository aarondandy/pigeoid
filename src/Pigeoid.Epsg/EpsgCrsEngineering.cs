using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Pigeoid.Unit;

namespace Pigeoid.Epsg
{
    public class EpsgCrsEngineering : EpsgCrsDatumBased, ICrsLocal
    {

        internal EpsgCrsEngineering(
            int code, string name, EpsgArea area, bool deprecated,
            EpsgCoordinateSystem cs, EpsgDatumEngineering datum
        ) : base(code, name, area, deprecated, cs) {
            Contract.Requires(code >= 0);
            Contract.Requires(!String.IsNullOrEmpty(name));
            Contract.Requires(area != null);
            Contract.Requires(cs != null);
            Contract.Requires(datum != null);
            EngineeringDatum = datum;
        }

        [ContractInvariantMethod]
        private void ObjectInvariants() {
            Contract.Invariant(EngineeringDatum != null);
        }

        public override EpsgDatum Datum {
            get {
                Contract.Ensures(Contract.Result<EpsgDatum>() != null);
                return EngineeringDatum;
            }
        }

        public EpsgDatumEngineering EngineeringDatum { get; private set; }

        IDatum ICrsLocal.Datum { get { return Datum; } }

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

        IUnit ICrsLocal.Unit { get { return Unit; } }

        public IList<EpsgAxis> Axes {
            get {
                Contract.Ensures(Contract.Result<IList<EpsgAxis>>() != null);
                return CoordinateSystem.Axes.ToArray();
            }
        }

        IList<IAxis> ICrsLocal.Axes { get { return Axes.Cast<IAxis>().ToArray(); } }

        public override EpsgCrsKind Kind {
            get { return EpsgCrsKind.Engineering; }
        }
    }
}
