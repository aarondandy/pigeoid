using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pigeoid.Contracts;

namespace Pigeoid.Ogc
{
	/// <summary>
	/// A datum.
	/// </summary>
	public class OgcDatum : OgcNamedAuthorityBoundEntity, IDatum
    {

        private readonly OgcDatumType _datumType;
        /// <summary>
        /// Constructs a local datum.
        /// </summary>
        /// <param name="name">The datum name.</param>
        /// <param name="type">The datum type code.</param>
        /// <param name="authority">The authority.</param>
        public OgcDatum(string name, OgcDatumType type, IAuthorityTag authority)
            : base(name, authority)
        {
            _datumType = type;
        }

        public OgcDatumType DatumType
        {
            get { return _datumType; }
        }

    }
}
