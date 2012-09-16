using System.IO;

namespace Pigeoid.Ogc
{
	/// <summary>
	/// Used to serialize and deserialize WKT data. 
	/// </summary>
	public class WktSerializer
	{

		private readonly WktOptions _options;

		public WktSerializer() : this(null) { }

		public WktSerializer(WktOptions options) {
			_options = options ?? new WktOptions();
		}

        public WktOptions Options { get { return _options; }}

		/// <summary>
		/// Parse WKT entities from the <paramref name="reader"/>.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <returns>A WKT entity.</returns>
		public object Parse(TextReader reader) {
			var wktReader = new WktReader(reader, _options);
			return wktReader.MoveNext()
				? wktReader.ReadEntity()
				: null;
		}

		/// <summary>
		/// Parse WKT entities from the given text string.
		/// </summary>
		/// <param name="wkText">The well known text to parse.</param>
		/// <returns>A WKT entity.</returns>
		public object Parse(string wkText) {
			using (var sr = new StringReader(wkText)) {
				return Parse(sr);
			}
		}

	}
}
