using System;
using System.Diagnostics.Contracts;

namespace Pigeoid.Epsg
{

    public class EpsgCoordinateTransformInfo : EpsgCoordinateOperationInfo
    {

        private readonly ushort _sourceCrsCode;
        private readonly ushort _targetCrsCode;
        private readonly double _accuracy;

        internal EpsgCoordinateTransformInfo(
            ushort code,
            ushort sourceCrsCode, ushort targetCrsCode,
            ushort opMethodCode,
            double accuracy,
            ushort areaCode, bool deprecated, string name
        )
            : base(code, opMethodCode, areaCode, deprecated, name) {
            Contract.Requires(!String.IsNullOrEmpty(name));
            _sourceCrsCode = sourceCrsCode;
            _targetCrsCode = targetCrsCode;
            _accuracy = accuracy;
        }

        public int SourceCrsCode { get { return _sourceCrsCode; } }

        public int TargetCrsCode { get { return _targetCrsCode; } }

        public EpsgCrs SourceCrs {
            get {
                Contract.Ensures(Contract.Result<EpsgCrs>() != null);
                return EpsgCrs.Get(_sourceCrsCode);
            }
        }

        public EpsgCrs TargetCrs {
            get {
                Contract.Ensures(Contract.Result<EpsgCrs>() != null);
                return EpsgCrs.Get(_targetCrsCode);
            }
        }

        public double Accuracy { get { return _accuracy; } }

    }

}
