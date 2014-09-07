using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using Pigeoid.CoordinateOperation;
using Pigeoid.Epsg.Resources;
using Pigeoid.Unit;

namespace Pigeoid.Epsg
{
    public class EpsgCrsProjected : EpsgCrsGeodetic, ICrsProjected
    {

        internal EpsgCrsProjected(int code, string name, EpsgArea area, bool deprecated, EpsgCoordinateSystem cs, EpsgDatumGeodetic datum, EpsgCrsGeodetic baseCrs, int projectionCode)
            : base(code, name, area, deprecated, cs, datum, baseCrs, projectionCode) {
            Contract.Requires(code >= 0);
            Contract.Requires(!String.IsNullOrEmpty(name));
            Contract.Requires(area != null);
            Contract.Requires(baseCrs != null);
            Contract.Requires(cs != null);
            Contract.Requires(datum != null);
        }

        [ContractInvariantMethod]
        private void ObjectInvariants() {
            Contract.Invariant(BaseCrs != null);
            Contract.Invariant(CoordinateSystem != null);
            Contract.Invariant(HasBaseOperationCode);
        }


        ICrsGeodetic ICrsProjected.BaseCrs { get { return BaseCrs; } }

        public EpsgCoordinateOperationInfo Projection {
            get {
                return EpsgMicroDatabase.Default.GetSingleCoordinateOperationInfo(BaseOperationCode);
            }
        }

        ICoordinateOperationInfo ICrsProjected.Projection { get { return Projection; } }

        public override EpsgCrsKind Kind { get { return EpsgCrsKind.Projected; } }
    }
}
