using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pigeoid.Contracts;

namespace Pigeoid.Unit
{
	[Obsolete("I don't see a use for this. Yagni?")]
	public class SingleUnityUnitConversionMap : SimpleUnitConversionMapBase
	{

		private readonly IUnit _singleUnit;

		public SingleUnityUnitConversionMap([NotNull] IUnit unit, IEqualityComparer<IUnit> unitEqualityComparer = null)
			: base(unitEqualityComparer)
		{
			if (null == unit)
				throw new ArgumentNullException("unit");
			_singleUnit = unit;
		}

		public override IEnumerable<IUnit> AllUnits {
			get { return new[] {_singleUnit}; }
		}

		public override IEnumerable<IUnitConversion<double>> GetConversionsTo(IUnit to) {
			return AreUnitsMatching(_singleUnit, to)
				? new[] { new UnitUnityConversion(_singleUnit, to) }
				: Enumerable.Empty<IUnitConversion<double>>();
		}

		public override IEnumerable<IUnitConversion<double>> GetConversionsFrom(IUnit from) {
			return AreUnitsMatching(_singleUnit, from)
				? new[] {new UnitUnityConversion(from, _singleUnit)}
				: Enumerable.Empty<IUnitConversion<double>>();
		}
	}
}
