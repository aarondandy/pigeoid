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
            Contract.Ensures(Contract.Result<Stream>() != null);
            var stream = ResourceAssembly.GetManifestResourceStream(ResourceBaseName + resourceName);
            if(stream == null)
                throw new FileNotFoundException("Resource file not found: " + resourceName);
		    return stream;
		}

		public static BinaryReader CreateBinaryReader(string resourceName) {
            Contract.Ensures(Contract.Result<BinaryReader>() != null);
			return new BinaryReader(CreateStream(resourceName));
		}

	}
}
