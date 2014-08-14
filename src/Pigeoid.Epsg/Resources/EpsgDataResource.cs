using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;

namespace Pigeoid.Epsg.Resources
{
    [System.Obsolete]
	internal static class EpsgDataResource
	{

		private static readonly string ResourceBaseName = typeof(EpsgDataResource).Namespace + '.';
		private static readonly Assembly ResourceAssembly = typeof(EpsgDataResource).Assembly;

        [Obsolete]
		public static Stream CreateStream(string resourceName) {
            Contract.Ensures(Contract.Result<Stream>() != null);
            var stream = ResourceAssembly.GetManifestResourceStream(ResourceBaseName + resourceName);
            if(stream == null)
                throw new FileNotFoundException("Resource file not found: " + resourceName);
		    return stream;
		}

        [Obsolete]
		public static BinaryReader CreateBinaryReader(string resourceName) {
            Contract.Ensures(Contract.Result<BinaryReader>() != null);
			return new BinaryReader(CreateStream(resourceName));
		}

	}
}
