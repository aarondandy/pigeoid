using System.IO;
using System.Text;

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
			var wktReader = new WktReader(reader, Options);
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

		public void Serialize(object entity,TextWriter writer){
			var wktWriter = new WktWriter(writer, Options);
			wktWriter.WriteEntity(entity);
		}

		public string Serialize(object entity){
			var builder = new StringBuilder();
			using(var writer = new StringWriter(builder)){
				Serialize(entity, writer);
			}
			return builder.ToString();
		}

	}
}
