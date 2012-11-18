using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;
using Pigeoid.Contracts;

namespace Pigeoid.Unit
{
	public class UnitConversionMapCollection<TValue> : Collection<IUnitConversionMap<TValue>>, IUnitConversionMap<TValue>
	{

		public UnitConversionMapCollection() : base() { } 

		public UnitConversionMapCollection(IEnumerable<IUnitConversionMap<TValue>> maps)
			: base(null == maps ? new List<IUnitConversionMap<TValue>>() : maps.ToList()) { }

		public IEnumerable<IUnit> AllUnits {
			get { return this.SelectMany(x => x.AllUnits).Distinct(EqualityComparer); }
		}

		[NotNull] public IEqualityComparer<IUnit> EqualityComparer { get { return this.Select(x => x.EqualityComparer).FirstOrDefault() ?? UnitEqualityComparer.Default; } }

		public IEnumerable<IUnitConversion<TValue>> GetConversionsTo(IUnit to) {
			return this
				.Select(x => x.GetConversionsTo(to))
				.Where(x => null != x)
				.SelectMany(x => x);
		}

		public IEnumerable<IUnitConversion<TValue>> GetConversionsFrom(IUnit from) {
			return this
				.Select(x => x.GetConversionsFrom(from))
				.Where(x => null != x)
				.SelectMany(x => x);
		}
	}

	public class UnitConversionMapCollection : UnitConversionMapCollection<double> 
	{
		public UnitConversionMapCollection() : base() { }
		public UnitConversionMapCollection(IEnumerable<IUnitConversionMap<double>> maps) : base(maps) { }
	}

}
