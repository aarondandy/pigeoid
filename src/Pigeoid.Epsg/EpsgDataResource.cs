// TODO: source header

using System.IO;
using System.Reflection;

namespace Pigeoid.Epsg
{
	internal static class EpsgDataResource
	{

		private static readonly string ResourceBaseName = typeof(EpsgDataResource).FullName + ".Resources.";
		private static readonly Assembly ResourceAssembly = typeof(EpsgDataResource).Assembly;

		public static Stream CreateStream(string resourceName) {
			return ResourceAssembly.GetManifestResourceStream(ResourceBaseName + resourceName);
		}

		public static BinaryReader CreteBinaryReader(string resourceName) {
			return new BinaryReader(CreateStream(resourceName));
		}

	}
}
