using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pigeoid.Contracts;
using Pigeoid.Interop;
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
			return BuildConversionMap(units.Where(x => null != x).Select(x => x.ConversionMap).Where(x => null != x).Distinct());
		}

		private static IUnitConversionMap<double> BuildConversionMap([NotNull] IEnumerable<IUnitConversionMap<double>> maps) {
			var pending = new Queue<IUnitConversionMap<double>>(maps.Where(x => null != x));
			var visited = new HashSet<IUnitConversionMap<double>>();
			var visitedUnits = new HashSet<IUnit>(UnitEqualityComparer.Default);

			while(pending.Count > 0) {
				var current = pending.Dequeue();
				if(visited.Contains(current))
					continue;
				
				visited.Add(current);
				var currentUnits = current.AllUnits.Where(x => !visitedUnits.Contains(x)).ToList();
				foreach (var unit in currentUnits)
					visitedUnits.Add(unit);

				foreach (var subMap in currentUnits.Select(x => x.ConversionMap).Where(x => null != x).Distinct()) {
					if(visited.Contains(subMap))
						continue;
					pending.Enqueue(subMap);
				}
			}

			return new UnitConversionMapCollection(visited);
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
			var resultConversions = Linearize(result.Skip(1).Select(x => x.Edge)).ToList();
			if (resultConversions.Count == 0)
				return null;
			if (resultConversions.Count == 1)
				return resultConversions[0];
			return new ConcatenatedUnitConversion(resultConversions);
		}

		private IEnumerable<IUnitConversion<double>> Linearize([NotNull] IEnumerable<IUnitConversion<double>> conversions) {
			return conversions.SelectMany(Linearize);
		}

		private IEnumerable<IUnitConversion<double>> Linearize([NotNull] IUnitConversion<double> conversion) {
			var catConv = conversion as ConcatenatedUnitConversion;
			if (null == catConv)
				return new[] {conversion};
			return Linearize(catConv.Conversions);
		}

	}
}
