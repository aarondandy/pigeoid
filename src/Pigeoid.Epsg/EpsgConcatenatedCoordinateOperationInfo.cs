using System;
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
            Contract.Requires(!String.IsNullOrEmpty(name));
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
                var source = EpsgMicroDatabase.Default.GetCrs(_sourceCrsCode);
                Contract.Assume(source != null); // _sourceCrsCode comes from a trusted source
                return source;
            }
        }

        public EpsgCrs TargetCrs {
            get {
                Contract.Ensures(Contract.Result<EpsgCrs>() != null);
                var target = EpsgMicroDatabase.Default.GetCrs(_targetCrsCode);
                Contract.Assume(target != null); // _targetCrsCode comes from a trusted source
                return target;
            }
        }

        public EpsgCoordinateOperationInfo[] Steps {
            get {
                Contract.Ensures(Contract.Result<EpsgCoordinateOperationInfo[]>() != null);
                Contract.Ensures(Contract.ForAll(Contract.Result<EpsgCoordinateOperationInfo[]>(), x => x != null));
                var result = Array.ConvertAll(_stepCodes, code => EpsgCoordinateOperationInfoRepository.GetSingleOperationInfo(code));
                Contract.Assume(Contract.ForAll(result, x => x != null));
                return result;
            }
        }

        IEnumerable<ICoordinateOperationInfo> IConcatenatedCoordinateOperationInfo.Steps { get { return Steps; } }

        public override bool HasInverse { get { return Steps.All(step => step.HasInverse); } }
    }
}
