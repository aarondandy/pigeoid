using System;
using NUnit.Framework;
using Pigeoid.Contracts;
using Pigeoid.Ogc;
using Pigeoid.Transformation;
using Vertesaur;

namespace Pigeoid.Core.Test
{
	[TestFixture]
	public class WktSerializeTest
	{

		public readonly WktSerializer Default = new WktSerializer();
		public readonly WktSerializer Pretty = new WktSerializer(new WktOptions {Pretty = true});
		public readonly WktSerializer Throws = new WktSerializer(new WktOptions {ThrowOnError = true});

		public WktSerializeTest() {
			AllSerializers = new[] {Default, Pretty, Throws};
			SerializerNames = new[] {"Default", "Pretty", "Throws"};
		}

		public WktSerializer[] AllSerializers { get; private set; }

		public string[] SerializerNames { get; private set; }

		[Test]
		public void SerializeAuthorityTest([ValueSource("AllSerializers")] WktSerializer serializer) {
			Assert.AreEqual(@"AUTHORITY[""EPSG"",""9001""]", serializer.Serialize(new AuthorityTag("EPSG", "9001")));
			Assert.AreEqual(@"AUTHORITY[""AbC"",""""]", serializer.Serialize(new AuthorityTag("AbC", null)));
			Assert.AreEqual(@"AUTHORITY[""DEF"",""ghi""]", serializer.Serialize(new AuthorityTag("DEF", "ghi")));
		}

		[Test]
		public void SerializeNamedParameterTest([ValueSource("AllSerializers")] WktSerializer serializer) {
			Assert.AreEqual(@"PARAMETER[""a"",1234]", serializer.Serialize(new NamedParameter<double>("a", 1234)));
			Assert.AreEqual(@"PARAMETER[""b"",""test""]", serializer.Serialize(new NamedParameter<string>("B", "test")));
			Assert.AreEqual(@"PARAMETER[""blank"",""""]", serializer.Serialize(new NamedParameter<object>("blank", null)));
		}

		[Test]
		public void SerializeSpheroidTest([ValueSource("AllSerializers")] WktSerializer serializer) {
			Assert.AreEqual(
				@"SPHEROID[""round"",12345,278,AUTHORITY[""PIGEOID"",""?!#$""]]",
				serializer.Serialize(new OgcSpheroid(
					new SpheroidEquatorialInvF(12345,278),
					"round",
					new AuthorityTag("PIGEOID","?!#$")
				))
			);
		}

		[Test]
		public void SerializePrimerMeridianTest([ValueSource("AllSerializers")] WktSerializer serializer) {
			Assert.AreEqual(
				@"PRIMEM[""ummm"",0.1,AUTHORITY[""PIGEOID"",""?!#$""]]",
				serializer.Serialize(new OgcPrimeMeridian(
					"ummm",
					0.1,
					new AuthorityTag("PIGEOID", "?!#$")
				))
			);
		}

		[Test]
		public void SerializeToWgs84Test([ValueSource("AllSerializers")] WktSerializer serializer) {
			Assert.AreEqual(
				@"TOWGS84[1,2,3,4,5,6,7]",
				serializer.Serialize(new Helmert7Transformation(
					new Vector3(1,2,3),
					new Vector3(4,5,6),
					7
				))
			);
		}

		[Test]
		public void SerializeDatumTest([ValueSource("AllSerializers")] WktSerializer serializer) {
			Assert.Inconclusive();
		}

		[Test]
		public void SerializeProjectedCrsTest([ValueSource("AllSerializers")] WktSerializer serializer) {
			Assert.Inconclusive();
		}

		[Test]
		public void SerializeUomTest([ValueSource("AllSerializers")] WktSerializer serializer) {
			Assert.Inconclusive();
		}

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
