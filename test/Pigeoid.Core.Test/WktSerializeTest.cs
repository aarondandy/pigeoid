using System;
using NUnit.Framework;
using Pigeoid.Contracts;
using Pigeoid.Ogc;

namespace Pigeoid.Core.Test
{
	[TestFixture]
	public class WktSerializeTest
	{

		public readonly WktSerializer Default = new WktSerializer();
		public readonly WktSerializer Pretty = new WktSerializer(new WktOptions { Pretty = true });

		[Test]
		public void SerializePrettyConcatMathTransformTest() {
			var input = new ConcatenatedCoordinateOperationInfo(
				new[] {
					new CoordinateOperationInfo(
						"Helmert 7 Parameter Transformation",
						new INamedParameter[]{
							new NamedParameter<double>("dx",1),
							new NamedParameter<double>("dy",2),
							new NamedParameter<double>("dz",3),
							new NamedParameter<double>("rx",4),
							new NamedParameter<double>("ry",5),
							new NamedParameter<double>("rz",6),
							new NamedParameter<double>("m", 7)
						}
					),
					new CoordinateOperationInfo(
						"Ellipsoid To Geocentric",
						new INamedParameter[]{
							new NamedParameter<double>("semi major", 6378137),
							new NamedParameter<double>("semi minor", 6356752.31414035)
						}
					),
					new CoordinateOperationInfo(
						"Ellipsoid_To_Geocentric",
						new INamedParameter[]{
							new NamedParameter<double>("semi_major", 6378206.4),
							new NamedParameter<double>("semi_minor", 6356583.8)
						},
						true
					).GetInverse()
				}
			);
			var expectedPretty = String.Format(
				"CONCAT_MT[{0}" +
				"\tPARAM_MT[\"Helmert_7_Parameter_Transformation\",{0}" +
				"\t\tPARAMETER[\"dx\",1],{0}" +
				"\t\tPARAMETER[\"dy\",2],{0}" +
				"\t\tPARAMETER[\"dz\",3],{0}" +
				"\t\tPARAMETER[\"rx\",4],{0}" +
				"\t\tPARAMETER[\"ry\",5],{0}" +
				"\t\tPARAMETER[\"rz\",6],{0}" +
				"\t\tPARAMETER[\"m\",7]{0}" +
				"\t],{0}" +
				"\tPARAM_MT[\"Ellipsoid_To_Geocentric\",{0}" +
				"\t\tPARAMETER[\"semi_major\",6378137],{0}" +
				"\t\tPARAMETER[\"semi_minor\",6356752.31414035]{0}" +
				"\t],{0}" +
				"\tINVERSE_MT[{0}" +
				"\t\tPARAM_MT[\"Ellipsoid_To_Geocentric\",{0}" +
				"\t\t\tPARAMETER[\"semi_major\",6378206.4],{0}" +
				"\t\t\tPARAMETER[\"semi_minor\",6356583.8]{0}" +
				"\t\t]{0}" +
				"\t]{0}" +
				"]",
				Environment.NewLine
			);
			var expectedDefault = expectedPretty.Replace(Environment.NewLine, "").Replace("\t", "");

			Assert.AreEqual(expectedDefault, Default.Serialize(input));
			Assert.AreEqual(expectedPretty, Pretty.Serialize(input));
		}

	}
}
