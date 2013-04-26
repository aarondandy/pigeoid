using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Pigeoid.Contracts;

namespace Pigeoid.Epsg
{
    public class EpsgConcatenatedCoordinateOperationInfo : EpsgCoordinateOperationInfoBase, IConcatenatedCoordinateOperationInfo
    {

        private readonly ushort _sourceCrsCode;
        private readonly ushort _targetCrsCode;
        private readonly ushort[] _stepCodes;

        internal EpsgConcatenatedCoordinateOperationInfo(
            ushort code, ushort sourceCrsCode, ushort targetCrsCode,
            ushort areaCode, bool deprecated, string name,
            ushort[] stepCodes
        )
            : base(code, areaCode, deprecated, name) {
            Contract.Requires(!string.IsNullOrEmpty(name));
            _sourceCrsCode = sourceCrsCode;
            _targetCrsCode = targetCrsCode;
            _stepCodes = stepCodes;
        }

        public int SourceCrsCode {
            get {
                Contract.Ensures(Contract.Result<int>() >= 0);
                return _sourceCrsCode;
            }
        }

        public int TargetCrsCode {
            get {
                Contract.Ensures(Contract.Result<int>() >= 0);
                return _targetCrsCode;
            }
        }

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

        public IEnumerable<EpsgCoordinateOperationInfo> Steps {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<EpsgCoordinateOperationInfo>>() != null);
                Contract.Ensures(Contract.ForAll(
                    Contract.Result<IEnumerable<EpsgCoordinateOperationInfo>>(),
                    x => x != null));
                return _stepCodes.Select(x => EpsgCoordinateOperationInfoRepository.GetOperationInfo(x));
            }
        }

        IEnumerable<ICoordinateOperationInfo> IConcatenatedCoordinateOperationInfo.Steps { get { return Steps; } }

        public override bool HasInverse { get { return Steps.All(step => step.HasInverse); } }
    }
}
