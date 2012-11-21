using System;
using JetBrains.Annotations;
using Pigeoid.Contracts;
using Vertesaur.Contracts;

namespace Pigeoid
{
	public class SpheroidLinearUnitConversionWrapper : ISpheroidInfo
	{

		private readonly string _name;
		private readonly IUnit _unit;
		private readonly double _a;
		private readonly double _b;
		private readonly ISpheroid<double> _shapeData;

		public SpheroidLinearUnitConversionWrapper([NotNull] ISpheroidInfo spheroid, [NotNull] IUnitConversion<double> conversion)
			: this(spheroid, conversion.To, conversion.TransformValue(spheroid.A), conversion.TransformValue(spheroid.B)) { }

		public SpheroidLinearUnitConversionWrapper([NotNull] ISpheroidInfo spheroid, [NotNull] IUnit unit, double a, double b)
			: this(spheroid.Name, unit, spheroid, a, b) { }

		public SpheroidLinearUnitConversionWrapper(string name, [NotNull] IUnit unit, [NotNull] ISpheroid<double> shapeData, double a, double b) {
			_name = name;
			_unit = unit;
			_shapeData = shapeData;
			_a = a;
			_b = b;
		}

		public string Name {
			get { return String.Concat(_name, " unit converted to ", AxisUnit); }
		}

		public IUnit AxisUnit {
			get { return _unit; }
		}

		public double A {
			get { return _a; }
		}

		public double B {
			get { return _b; }
		}

		public double E {
			get { return _shapeData.E; }
		}

		public double ESecond {
			get { return _shapeData.ESecond; }
		}

		public double ESecondSquared {
			get { return _shapeData.ESecondSquared; }
		}

		public double ESquared {
			get { return _shapeData.ESquared; }
		}

		public double F {
			get { return _shapeData.F; }
		}

		public double InvF {
			get { return _shapeData.InvF; }
		}


		public IAuthorityTag Authority {
			get {
				return null; // NOTE: whatever authority tag it used to have, it no longer has!
			}
		}
	}
}
