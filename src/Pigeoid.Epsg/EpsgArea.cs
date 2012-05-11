// TODO: source header

using System.Collections.Generic;
using System.IO;
using Pigeoid.Contracts;

namespace Pigeoid.Epsg
{
	public class EpsgArea
	{

		private readonly ushort _code;
		private readonly string _iso2;
		private readonly string _iso3;
		private readonly string _name;

		internal EpsgArea(ushort code, string name, string iso2, string iso3) {
			_code = code;
			_name = name;
			_iso2 = iso2;
			_iso3 = iso3;
		}

		public int Code { get { return _code; } }

		public string Name { get { return _name; } }

		public string Iso2 { get { return _iso2; } }

		public string Iso3 { get { return _iso3; } }

		public IAuthorityTag Authority {
			get { return new EpsgAuthorityTag(_code); }
		}

	}
}
