using NUnit.Framework;

namespace Pigeoid.GoldData
{
	[TestFixture]
	public class GeneralTransformationGenerationTest
	{

		[Test]
		[TestCase("Wgs84.Lat_Lon.csv", "Wgs84.LCC_14.csv", 0.000001)]
		public void Test(string sourceResourceName, string targetResourceName, double allowedDelta) {

			var sourceData = GoldData.GetReadyReader(sourceResourceName);
			var targetData = GoldData.GetReadyReader(targetResourceName);
			var sourceCrs = GoldData.GetCrs(sourceData);
			var targetCrs = GoldData.GetCrs(targetData);

			Assert.IsNotNull(sourceCrs);
			Assert.IsNotNull(targetCrs);

			var generator = new GeneralTransformationGenerator();
			var transform = generator.Generate(sourceCrs, targetCrs);
			Assert.IsNotNull(transform);

		}


	}
}
