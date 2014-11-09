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

        public static bool TryConvert(IAuthorityTag authorityTag, out EpsgAuthorityTag epsgTag) {
            if (authorityTag != null) {
                if (authorityTag is EpsgAuthorityTag) {
                    epsgTag = (EpsgAuthorityTag)authorityTag;
                    return true;
                }
                if (EpsgName.Equals(authorityTag.Name, StringComparison.OrdinalIgnoreCase)) {
                    int codeNumber;
                    if (Int32.TryParse(authorityTag.Code, out codeNumber)) {
                        epsgTag = new EpsgAuthorityTag(codeNumber);
                        return true;
                    }
                }
            }
            epsgTag = new EpsgAuthorityTag();
            return false;
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

        public int NumericalCode {
            get { return _code; }
        }

        public string Code {
            get {
                Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
                var codeText = _code.ToString(CultureInfo.InvariantCulture);
                Contract.Assume(!String.IsNullOrEmpty(codeText));
                return codeText;
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
            var result = Name + ':' + _code;
            Contract.Assume(!String.IsNullOrEmpty(result));
            return result;
        }

    }
}
