using System;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace Pigeoid.Epsg
{
    /// <summary>
    /// An EPSG authority tag.
    /// </summary>
    public struct EpsgAuthorityTag :
        IAuthorityTag,
        IEquatable<EpsgAuthorityTag>
    {

        public static bool operator ==(EpsgAuthorityTag a, EpsgAuthorityTag b) {
            return a.Equals(b);
        }

        public static bool operator !=(EpsgAuthorityTag a, EpsgAuthorityTag b) {
            return !a.Equals(b);
        }

        internal const string EpsgName = "EPSG";

        private readonly int _code;

        internal EpsgAuthorityTag(int code) {
            Contract.Requires(code >= 0);
            _code = code;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(_code >= 0);
        }

        public string Code {
            get {
                Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
                return _code.ToString(CultureInfo.InvariantCulture);
            }
        }

        public string Name {
            get {
                Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
                return EpsgName;
            }
        }

        public bool Equals(EpsgAuthorityTag other) {
            return other._code == _code;
        }

        public bool Equals(IAuthorityTag other) {
            return null != other
                && Name.Equals(other.Name)
                && Code.Equals(other.Code);
        }

        public override bool Equals(object obj) {
            return obj is EpsgAuthorityTag
                ? Equals((EpsgAuthorityTag)obj)
                : Equals(obj as IAuthorityTag);
        }

        public override int GetHashCode() {
            return _code;
        }

        public override string ToString() {
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
            return Name + ':' + _code;
        }

    }
}
