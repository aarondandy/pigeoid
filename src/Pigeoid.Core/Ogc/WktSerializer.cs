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

		

	}
}
