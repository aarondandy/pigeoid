using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Vertesaur.Search;

namespace Pigeoid.Unit
{
    public class SimpleUnitConversionGenerator : IUnitConversionGenerator<double>
    {

        private class UnitConversionGraph : DynamicGraphBase<IUnit, int, IUnitConversion<double>>
        {

            private readonly IUnitConversionMap<double> _conversionMap;

            public UnitConversionGraph(IUnitConversionMap<double> conversionMap)
                : base(null, conversionMap.EqualityComparer) {
                Contract.Requires(conversionMap != null);
                _conversionMap = conversionMap;
            }

            [ContractInvariantMethod]
            private void CodeContractInvariants() {
                Contract.Invariant(_conversionMap != null);
            }

            protected override IEnumerable<DynamicGraphNodeData<IUnit, int, IUnitConversion<double>>> GetNeighborInfo(IUnit node, int currentCost) {
                Contract.Ensures(Contract.Result<IEnumerable<DynamicGraphNodeData<IUnit, int, IUnitConversion<double>>>>() != null);
                var conversions = _conversionMap.GetConversionsFrom(node);
                if (null == conversions)
                    return Enumerable.Empty<DynamicGraphNodeData<IUnit, int, IUnitConversion<double>>>();
                var costPlusOne = currentCost + 1;
                return conversions.Select(x => new DynamicGraphNodeData<IUnit, int, IUnitConversion<double>>(x.To, costPlusOne, x));
            }

        }

        private static IUnitConversionMap<double> BuildConversionMap(IEnumerable<IUnit> units) {
            Contract.Requires(units != null);
            Contract.Ensures(Contract.Result<IUnitConversionMap<double>>() != null);
            return BuildConversionMap(units.Where(x => null != x).Select(x => x.ConversionMap).Where(x => null != x).Distinct());
        }

        private static IUnitConversionMap<double> BuildConversionMap(IEnumerable<IUnitConversionMap<double>> maps) {
            Contract.Requires(maps != null);
            Contract.Ensures(Contract.Result<IUnitConversionMap<double>>() != null);
            var pending = new Queue<IUnitConversionMap<double>>(maps.Where(x => null != x));
            var visited = new HashSet<IUnitConversionMap<double>>();
            var visitedUnits = new HashSet<IUnit>(UnitEqualityComparer.Default);

            while (pending.Count > 0) {
                var current = pending.Dequeue();
                if (visited.Contains(current))
                    continue;

                visited.Add(current);
                var currentUnits = current.AllUnits.Where(x => !visitedUnits.Contains(x)).ToList();
                foreach (var unit in currentUnits)
                    visitedUnits.Add(unit);

                foreach (var subMap in currentUnits.Select(x => x.ConversionMap).Where(x => null != x).Distinct()) {
                    if (visited.Contains(subMap))
                        continue;
                    pending.Enqueue(subMap);
                }
            }

            return new UnitConversionMapCollection(visited);
        }

        public static IUnitConversion<double> FindConversion(IUnit from, IUnit to) {
            if(from == null) throw new ArgumentNullException("from");
            if(to == null) throw new ArgumentNullException("to");
            Contract.EndContractBlock();
            var pathGenerator = new SimpleUnitConversionGenerator(from, to);
            return pathGenerator.GenerateConversion(from, to);
        }

        private readonly IUnitConversionMap<double> _conversionMap;
        private readonly UnitConversionGraph _conversionGraph;

        public SimpleUnitConversionGenerator(IUnitConversionMap<double> conversionMap) {
            if (null == conversionMap) throw new ArgumentNullException("conversionMap");
            Contract.EndContractBlock();
            _conversionMap = conversionMap;
            _conversionGraph = new UnitConversionGraph(conversionMap);
        }

        public SimpleUnitConversionGenerator(IEnumerable<IUnit> involvedUnits)
            : this(BuildConversionMap(involvedUnits)) { Contract.Requires(involvedUnits != null);}

        public SimpleUnitConversionGenerator(IUnit a, IUnit b)
            : this(new[] { a, b }) { }

        [ContractInvariantMethod]
        private void ObjectInvariants() {
            Contract.Invariant(_conversionGraph != null);
            Contract.Invariant(_conversionMap != null);
        }

        public IUnitConversion<double> GenerateConversion(IUnit from, IUnit to) {
            if (null == from || null == to)
                return null;
            if (ReferenceEquals(from, to) || _conversionMap.EqualityComparer.Equals(from, to))
                return new UnitUnityConversion(from, to);
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

        private IEnumerable<IUnitConversion<double>> Linearize(IEnumerable<IUnitConversion<double>> conversions) {
            Contract.Requires(conversions != null);
            Contract.Ensures(Contract.Result<IEnumerable<IUnitConversion<double>>>() != null);
            return conversions.SelectMany(Linearize);
        }

        private IEnumerable<IUnitConversion<double>> Linearize(IUnitConversion<double> conversion) {
            Contract.Requires(conversion != null);
            Contract.Ensures(Contract.Result<IEnumerable<IUnitConversion<double>>>() != null);
            var catConv = conversion as ConcatenatedUnitConversion;
            if (null == catConv)
                return new[] { conversion };
            return Linearize(catConv.Conversions);
        }

    }
}
