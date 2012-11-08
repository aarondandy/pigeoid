using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pigeoid.Contracts;

namespace Pigeoid.Unit
{
	public class ReadOnlyUnitConversionMap : SimpleUnitConversionMapBase
	{

		private readonly IUnitConversion<double>[] _allConversions;
		private readonly Dictionary<IUnit, IUnitConversion<double>[]> _fromMap;
		private readonly Dictionary<IUnit, IUnitConversion<double>[]> _toMap;
		private readonly IUnit[] _allDistinctUnits;

		public ReadOnlyUnitConversionMap([NotNull] IEnumerable<IUnitConversion<double>> conversions, IEqualityComparer<IUnit> unitEqualityComparer = null)
			: base(unitEqualityComparer)
		{
			if(null == conversions)
				throw new ArgumentNullException("conversions");

			_allConversions = conversions.ToArray();
			_allDistinctUnits = _allConversions
				.Select(x => x.From)
				.Concat(_allConversions.Select(x => x.To))
				.Distinct(EqualityComparer)
				.ToArray();
			_fromMap = _allConversions
				.ToLookup(x => x.From, EqualityComparer)
				.ToDictionary(x => x.Key, x => x.ToArray(), EqualityComparer);
			_toMap = _allConversions
				.ToLookup(x => x.To, EqualityComparer)
				.ToDictionary(x => x.Key, x => x.ToArray(), EqualityComparer);
		}

		public override IEnumerable<IUnit> AllUnits { get { return Array.AsReadOnly(_allDistinctUnits); } }

		public override IEnumerable<IUnitConversion<double>> GetConversionsTo(IUnit to) {
			IUnitConversion<double>[] rawOperations;
			var results = new List<IUnitConversion<double>>();
			if (_toMap.TryGetValue(to, out rawOperations))
				results.AddRange(rawOperations);
			if (_fromMap.TryGetValue(to, out rawOperations))
				results.AddRange(rawOperations.Where(x => x.HasInverse).Select(x => x.GetInverse()));
			return results;
		}

		public override IEnumerable<IUnitConversion<double>> GetConversionsFrom(IUnit from) {
			IUnitConversion<double>[] rawOperations;
			var results = new List<IUnitConversion<double>>();
			if(_fromMap.TryGetValue(from, out rawOperations))
				results.AddRange(rawOperations);
			if(_toMap.TryGetValue(from, out rawOperations))
				results.AddRange(rawOperations.Where(x => x.HasInverse).Select(x =>  x.GetInverse()));
			return results;
		}

	}
}
