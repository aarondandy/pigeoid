// TODO: source header

using System.IO;
using System.Reflection;

namespace Pigeoid.Epsg.Resources
{
	internal static class EpsgDataResource
	{

		private static readonly string _resourceBaseName = typeof(EpsgDataResource).Namespace + '.';
		private static readonly Assembly _resourceAssembly = typeof(EpsgDataResource).Assembly;

		public static Stream CreateStream(string resourceName) {
			return _resourceAssembly.GetManifestResourceStream(_resourceBaseName + resourceName);
		}

		public static BinaryReader CreateBinaryReader(string resourceName) {
			return new BinaryReader(CreateStream(resourceName));
		}

	}
}
