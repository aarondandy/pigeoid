using System;
using System.Diagnostics.Contracts;
using Pigeoid.CoordinateOperation;
using Vertesaur;

namespace Pigeoid.Epsg
{
    public abstract class EpsgCoordinateOperationInfoBase : ICoordinateOperationInfo, IAuthorityBoundEntity
    {

        private readonly ushort _code;
        private readonly ushort _areaCode;
        private readonly bool _deprecated;

        internal EpsgCoordinateOperationInfoBase(ushort code, ushort areaCode, bool deprecated, string name) {
            Contract.Requires(!String.IsNullOrEmpty(name));
            _code = code;
            _areaCode = areaCode;
            _deprecated = deprecated;
            Name = name;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(!String.IsNullOrEmpty(Name));
        }

        public string Name { get; private set; }
        public int Code { get { return _code; } }
        public EpsgArea Area { get { return EpsgMicroDatabase.Default.GetArea(_areaCode); } }
        public bool Deprecated { get { return _deprecated; } }

        public abstract bool HasInverse { get; }

        string ICoordinateOperationInfo.Name { get { return Name; } }

        public ICoordinateOperationInfo GetInverse() {
            if (!HasInverse) throw new NoInverseException();
            Contract.Ensures(Contract.Result<ICoordinateOperationInfo>() != null);
            return new EpsgCoordinateOperationInverse(this);
        }

        bool ICoordinateOperationInfo.IsInverseOfDefinition {
            get { return false; }
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
