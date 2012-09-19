using Pigeoid.Contracts;

namespace Pigeoid.Ogc
{
	public class OgcCoordinateOperationMethodInfo : ICoordinateOperationMethodInfo
	{

		public OgcCoordinateOperationMethodInfo(string name, IAuthorityTag authorityTag = null) {
			Name = name;
			Authority = authorityTag;
		}

		public string Name { get; private set; }

		public IAuthorityTag Authority { get; private set; }
	}
}
