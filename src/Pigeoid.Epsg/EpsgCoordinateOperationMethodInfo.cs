using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using Pigeoid.CoordinateOperation;
using Pigeoid.Epsg.Resources;

namespace Pigeoid.Epsg
{
    public class EpsgCoordinateOperationMethodInfo : ICoordinateOperationMethodInfo
    {

        private readonly ushort _code;
        
        [Obsolete("Having this here makes serialization hard.")]
        private readonly EpsgDataResourceReaderParameterValues _paramValuesReader;
        
        internal EpsgCoordinateOperationMethodInfo(ushort code, string name, bool canReverse) {
            Contract.Requires(!String.IsNullOrEmpty(name));
            _code = code;
            Name = name;
            CanReverse = canReverse;
            _paramValuesReader = new EpsgDataResourceReaderParameterValues(code);
        }

        [ContractInvariantMethod]
        private void ObjectInvariants() {
            Contract.Invariant(!String.IsNullOrEmpty(Name));
            Contract.Invariant(_paramValuesReader != null);
        }

        public int Code {
            get {
                Contract.Ensures(Contract.Result<int>() >= 0);
                return _code;
            }
        }
        public string Name { get; private set; }
        public bool CanReverse { get; private set; }

        public EpsgParameterUsage[] ParameterUsage {
            get {
                Contract.Ensures(Contract.Result<EpsgParameterUsage[]>() != null);
                return _paramValuesReader.GetParameterUsages();
            }
        }

        public List<INamedParameter> GetOperationParameters(int operationCode) {
            if (operationCode <= 0 || operationCode > UInt16.MaxValue)
                return null;
            return _paramValuesReader.ReadParameters(unchecked((ushort)operationCode));
        }

        public IAuthorityTag Authority {
            get {
                Contract.Ensures(Contract.Result<IAuthorityTag>() != null);
                return new EpsgAuthorityTag(_code);
            }
        }

        public override string ToString() {
            return Code.ToString() + " " + Name;
        }

    }
}
