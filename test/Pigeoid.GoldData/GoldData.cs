using System.IO;
using System.Reflection;
using NUnit.Framework;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.GoldData
{
	public static class GoldData
	{

		private static readonly Assembly ThisAssembly = Assembly.GetAssembly(typeof(GoldData));

		private static StreamReader GetEmbeddedStreamReader(string name)
		{
			var stream = ThisAssembly.GetManifestResourceStream(name)
				?? ThisAssembly.GetManifestResourceStream(typeof(GoldData).Namespace + ".Data." + name);
			
			if (null == stream)
				Assert.Inconclusive("Could not load resource: " + name, name);
			
			return new StreamReader(stream);
		}

		public static GeoTransGoldDataReader GetReadyReader(string name)
		{
			var reader = new GeoTransGoldDataReader(GetEmbeddedStreamReader(name));
			if (!reader.Read())
				Assert.Inconclusive("Could not read header: " + name, name);

			return reader;
		}

		public static ISpheroid<double> GenerateSpheroid(string name)
		{
			if ("WGE".Equals(name.ToUpper()) || "WE".Equals(name.ToUpper()))
				return new SpheroidEquatorialInvF(6378137, 298.257223563);

			Assert.Inconclusive("Spheroid not found: " + name, name);
			return null;
		}

	}
}
