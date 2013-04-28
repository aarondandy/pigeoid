using System;

namespace Pigeoid
{
    /// <summary>
    /// An authority tag used to identify an object.
    /// </summary>
    public class AuthorityTag : IAuthorityTag
    {

        /// <summary>
        /// Constructs a new authority tag.
        /// </summary>
        /// <param name="name">The authority name.</param>
        /// <param name="code">The authority code.</param>
        public AuthorityTag(string name, string code) {
            Name = name;
            Code = code;
        }

        /// <inheritdoc/>
        public string Name { get; private set; }

        /// <inheritdoc/>
        public string Code { get; private set; }

        public bool Equals(IAuthorityTag other) {
            return null != other
                && String.Equals(Name, other.Name)
                && String.Equals(Code, other.Code);
        }

        public override bool Equals(object obj) {
            return ReferenceEquals(this, obj) || Equals(obj as IAuthorityTag);
        }

        public override int GetHashCode() {
            return null != Code ? Code.GetHashCode() : 0;
        }

        public override string ToString() {
            if (String.IsNullOrEmpty(Name))
                return String.IsNullOrEmpty(Code) ? "Unknown" : Code;
            if (String.IsNullOrEmpty(Code))
                return Name;
            return String.Concat(Name, ':', Code);
        }
    }
}
