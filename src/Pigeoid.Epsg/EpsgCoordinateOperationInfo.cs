using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Pigeoid.CoordinateOperation;

namespace Pigeoid.Epsg
{

    public class EpsgCoordinateOperationInfo : EpsgCoordinateOperationInfoBase, IParameterizedCoordinateOperationInfo
    {

        private readonly ushort _opMethodCode;

        internal EpsgCoordinateOperationInfo(ushort code, ushort opMethodCode, ushort areaCode, bool deprecated, string name)
            : base(code, areaCode, deprecated, name) {
            Contract.Requires(!String.IsNullOrEmpty(name));
            _opMethodCode = opMethodCode;
        }

        public EpsgCoordinateOperationMethodInfo Method {
            get {
                Contract.Ensures(Contract.Result<EpsgCoordinateOperationMethodInfo>() != null);
                var method = EpsgCoordinateOperationMethodInfo.Get(_opMethodCode);
                Contract.Assume(method != null); // because _opMethodCode comes from a trusted source
                return method;
            }
        }

        ICoordinateOperationMethodInfo IParameterizedCoordinateOperationInfo.Method { get { return Method; } }

        public IEnumerable<INamedParameter> Parameters {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<INamedParameter>>() != null);
                return Method.GetOperationParameters(Code)
                    ?? Enumerable.Empty<INamedParameter>();
            }
        }

        public override bool HasInverse {
            get { return Method.CanReverse; }
        }



    }

}
