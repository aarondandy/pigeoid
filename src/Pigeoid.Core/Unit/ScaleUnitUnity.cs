using Pigeoid.Contracts;

namespace Pigeoid.Unit
{
	public class ScaleUnitUnity : IUnit
	{

		public static readonly ScaleUnitUnity Value = new ScaleUnitUnity();

		private ScaleUnitUnity() { }

		public string Name {
			get { return "unity"; }
		}

		public string Type {
			get { return "Scale"; }
		}

		public IUnitConversionMap<double> ConversionMap {
			get { return BasicScaleUnitConversionMap.Default; }
		}
	}
}
