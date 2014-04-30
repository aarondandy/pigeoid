using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Pigeoid.CoordinateOperation;
using Pigeoid.Utility;
using Vertesaur;

namespace Pigeoid.Ogc
{
    public class OgcPassThroughCoordinateOperationInfo : IPassThroughCoordinateOperationInfo
    {

        public OgcPassThroughCoordinateOperationInfo(ICoordinateOperationInfo core, int firstAffectedOrdinate) {
            if (null == core) throw new ArgumentNullException("core");
            Contract.EndContractBlock();
            Core = core;
            FirstAffectedOrdinate = firstAffectedOrdinate;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(Core != null);
        }

        public ICoordinateOperationInfo Core { get; private set; }

        public int FirstAffectedOrdinate { get; private set; }

        public IEnumerable<ICoordinateOperationInfo> Steps {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<ICoordinateOperationInfo>>() != null);
                Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<ICoordinateOperationInfo>>(), x => x != null));
                return ArrayUtil.CreateSingleElementArray(Core);
            }
        }

        public string Name {
            get {
                Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
                return "Pass-through";
            }
        }

        public bool HasInverse {
            [Pure] get {
                return FirstAffectedOrdinate == 0 && Core.HasInverse;
            }
        }

        public ICoordinateOperationInfo GetInverse() {
            if (!HasInverse) throw new NoInverseException();
            Contract.Ensures(Contract.Result<ICoordinateOperationInfo>() != null);
            return Core.GetInverse();
        }

        public bool IsInverseOfDefinition {
            [Pure] get { return false; }
        }
    }
}
