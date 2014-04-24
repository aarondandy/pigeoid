using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
                return EpsgCoordinateOperationMethodInfo.Get(_opMethodCode);
            }
        }

        ICoordinateOperationMethodInfo IParameterizedCoordinateOperationInfo.Method { get { return Method; } }

        public IEnumerable<INamedParameter> Parameters {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<INamedParameter>>() != null);
                return Method.GetOperationParameters(Code);
            }
        }

        public override bool HasInverse {
            get { return Method.CanReverse; }
        }



    }

}
