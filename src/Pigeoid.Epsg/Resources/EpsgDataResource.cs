// TODO: source header

using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;

namespace Pigeoid.Epsg.Resources
{
	internal static class EpsgDataResource
	{

		private static readonly string ResourceBaseName = typeof(EpsgDataResource).Namespace + '.';
		private static readonly Assembly ResourceAssembly = typeof(EpsgDataResource).Assembly;

		public static Stream CreateStream(string resourceName) {
			return ResourceAssembly.GetManifestResourceStream(ResourceBaseName + resourceName);
		}

		public static BinaryReader CreateBinaryReader(string resourceName) {
            Contract.Ensures(Contract.Result<BinaryReader>() != null);
			return new BinaryReader(CreateStream(resourceName));
		}

	}
}
