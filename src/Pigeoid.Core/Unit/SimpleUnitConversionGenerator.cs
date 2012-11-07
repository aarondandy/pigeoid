using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pigeoid.Contracts;
using Vertesaur.Search;

namespace Pigeoid.Unit
{
	public class SimpleUnitConversionGenerator : IUnitConversionGenerator<double>
	{

		private class UnitConversionGraph : DynamicGraphBase<IUnit, int, IUnitConversion<double>>
		{

			private readonly IUnitConversionMap<double> _conversionMap;

			public UnitConversionGraph([NotNull] IUnitConversionMap<double> conversionMap)
				: base(null, conversionMap.EqualityComparer) {
				_conversionMap = conversionMap;
			}

			public override IEnumerable<DynamicGraphNodeData<IUnit, int, IUnitConversion<double>>> GetNeighborInfo(IUnit node, int currentCost) {
				var costPlusOne = currentCost + 1;
				var conversions = _conversionMap.GetConversionsFrom(node);
				if (null == conversions)
					return Enumerable.Empty<DynamicGraphNodeData<IUnit, int, IUnitConversion<double>>>();
				return conversions.Select(x => new DynamicGraphNodeData<IUnit, int, IUnitConversion<double>>(x.To, costPlusOne, x));
			}

		}

		private static IUnitConversionMap<double> BuildConversionMap([NotNull] IEnumerable<IUnit> units) {
			var maps = units
				.Where(x => null != x)
				.Select(x => x.ConversionMap)
				.Where(x => null != x)
				.Distinct()
				.ToList();
			if (maps.Count == 0)
				return null;
			if (maps.Count == 1)
				return maps[0];
			return new UnitConversionMapCollection(maps);
		}

		public static IUnitConversion<double> FindConversion(IUnit from, IUnit to) {
			var pathGenerator = new SimpleUnitConversionGenerator(from, to);
			return pathGenerator.GenerateConversion(from, to);
		}

		private readonly IUnitConversionMap<double> _conversionMap;
		private readonly UnitConversionGraph _conversionGraph;

		public SimpleUnitConversionGenerator([NotNull] IUnitConversionMap<double> conversionMap) {
			if(null == conversionMap)
				throw new ArgumentNullException("conversionMap");
			_conversionMap = conversionMap;
			_conversionGraph = new UnitConversionGraph(conversionMap);
		}

		public SimpleUnitConversionGenerator([NotNull] IEnumerable<IUnit> involvedUnits)
			: this(BuildConversionMap(involvedUnits)) { }

		public SimpleUnitConversionGenerator([NotNull] IUnit a, [NotNull] IUnit b)
			: this(new[]{a,b}) { }

		public IUnitConversion<double> GenerateConversion(IUnit from, IUnit to) {
			if(ReferenceEquals(from, to) || _conversionMap.EqualityComparer.Equals(from, to))
				return new UnitUnityConversion(from, to);
			if (null == from || null == to)
				return null;
			var result = _conversionGraph.FindPath(from, to);
			if (null == result)
				return null;
			return new ConcatenatedUnitConversion(result.Select(x => x.Edge));
		}

	}
}
