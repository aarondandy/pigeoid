using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Pigeoid.CoordinateOperation;

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
            Contract.Requires(stepCodes != null);
            _sourceCrsCode = sourceCrsCode;
            _targetCrsCode = targetCrsCode;
            _stepCodes = stepCodes;
        }

        [ContractInvariantMethod]
        private void ObjectInvariants() {
            Contract.Invariant(_stepCodes != null);
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
                var source = EpsgCrs.Get(_sourceCrsCode);
                Contract.Assume(source != null); // _sourceCrsCode comes from a trusted source
                return source;
            }
        }

        public EpsgCrs TargetCrs {
            get {
                Contract.Ensures(Contract.Result<EpsgCrs>() != null);
                var target = EpsgCrs.Get(_targetCrsCode);
                Contract.Assume(target != null); // _targetCrsCode comes from a trusted source
                return target;
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
