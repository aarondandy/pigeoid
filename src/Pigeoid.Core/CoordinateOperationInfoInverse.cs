using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Pigeoid.Contracts;

namespace Pigeoid
{
    public class CoordinateOperationInfoInverse : IParameterizedCoordinateOperationInfo
    {

        public CoordinateOperationInfoInverse(ICoordinateOperationInfo core) {
            if (null == core) throw new ArgumentNullException("core");
            Contract.EndContractBlock();
            Core = core;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(Core != null);
        }

        public ICoordinateOperationInfo Core { get; private set; }

        public IParameterizedCoordinateOperationInfo ParameterizedCore { get { return Core as IParameterizedCoordinateOperationInfo; } }

        public string Name { get { return "Inverse " + Core.Name; } }

        public IEnumerable<INamedParameter> Parameters {
            get {
                var parameterizedOperationInfo = ParameterizedCore;
                return null != parameterizedOperationInfo
                    ? parameterizedOperationInfo.Parameters
                    : Enumerable.Empty<INamedParameter>();
            }
        }

        public bool HasInverse { [Pure] get { return true; } }

        public ICoordinateOperationInfo GetInverse() {
            Contract.Ensures(Contract.Result<ICoordinateOperationInfo>() != null);
            return Core;
        }

        public bool IsInverseOfDefinition { [Pure] get { return true; } }

        public ICoordinateOperationMethodInfo Method {
            get {
                var paramOp = ParameterizedCore;
                return null != paramOp
                    ? paramOp.Method
                    : null;
            }
        }
    }

}
