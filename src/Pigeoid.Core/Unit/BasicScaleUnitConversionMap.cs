namespace Pigeoid.Unit
{
	internal class BasicScaleUnitConversionMap : BinaryUnitConversionMap
	{

		public static readonly BasicScaleUnitConversionMap Default = new BasicScaleUnitConversionMap();

		private BasicScaleUnitConversionMap() : base(new UnitScalarConversion(ScaleUnitUnity.Value, ScaleUnitPartsPerMillion.Value, 1000000)) { }

	}
}
