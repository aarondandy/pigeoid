// TODO: source header

using Pigeoid.Contracts;

namespace Pigeoid.Ogc
{
	/// <summary>
	/// A datum.
	/// </summary>
	public class OgcDatum : OgcNamedAuthorityBoundEntity, IDatum
    {

        private readonly OgcDatumType _ogcType;
        /// <summary>
        /// Constructs a local datum.
        /// </summary>
        /// <param name="name">The datum name.</param>
        /// <param name="type">The datum type code.</param>
        /// <param name="authority">The authority.</param>
        public OgcDatum(string name, OgcDatumType type, IAuthorityTag authority) : base(name, authority)
        {
            _ogcType = type;
        }

		public OgcDatumType OgcType { get { return _ogcType; } }

		public string Type {
			get { return _ogcType.ToString(); }
		}

    }
}
