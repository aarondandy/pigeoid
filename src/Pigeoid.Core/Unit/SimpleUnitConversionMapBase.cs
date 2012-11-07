using System.Collections.Generic;
using System.Linq;
using Pigeoid.Contracts;
using Pigeoid.Interop;

namespace Pigeoid.Unit
{
	public abstract class SimpleUnitConversionMapBase : IUnitConversionMap<double>
	{

		private readonly IEqualityComparer<IUnit> _unitEqualityComparer;

		protected SimpleUnitConversionMapBase(IEqualityComparer<IUnit> unitEqualityComparer = null) {
			_unitEqualityComparer = unitEqualityComparer ?? UnitEqualityComparer.Default;
		}

		public IEqualityComparer<IUnit> EqualityComparer { get { return _unitEqualityComparer; } }

		protected bool AreUnitsMatching(IUnit a, IUnit b) {
			return _unitEqualityComparer.Equals(a, b);
		}

		public abstract IEnumerable<IUnit> AllUnits { get; }

		public abstract IEnumerable<IUnitConversion<double>> GetConversionsTo(IUnit to);

		public abstract IEnumerable<IUnitConversion<double>> GetConversionsFrom(IUnit from);

	}
}
