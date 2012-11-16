using Pigeoid.Contracts;
using System;

namespace Pigeoid.Unit
{
	public class ScaleUnitPartsPerMillion : IUnit
	{

		public static readonly ScaleUnitPartsPerMillion Value = new ScaleUnitPartsPerMillion();

		private ScaleUnitPartsPerMillion() { }

		public string Name {
			get { return "parts per million"; }
		}

		public string Type {
			get { return "Scale"; }
		}

		public IUnitConversionMap<double> ConversionMap {
			get { return BasicScaleUnitConversionMap.Default; }
		}
	}
}
