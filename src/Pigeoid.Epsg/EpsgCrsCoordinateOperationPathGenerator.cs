using System;
using System.Collections.Generic;
using System.Linq;
using Pigeoid.Contracts;
using Vertesaur.Search;

namespace Pigeoid.Epsg
{
	public class EpsgCrsCoordinateOperationPathGenerator :
		ICoordinateOperationPathGenerator<ICrs>,
		ICoordinateOperationPathGenerator<EpsgCrs>
	{

		private class EpsgTransformGraph : DynamicGraphBase<EpsgCrs, int, ICoordinateOperationInfo>
		{

			private readonly EpsgArea _fromArea;
			private readonly EpsgArea _toArea;

			public EpsgTransformGraph(EpsgArea fromArea, EpsgArea toArea) {
				_fromArea = fromArea;
				_toArea = toArea;
			}

			private bool AreaIntersectionPasses(EpsgArea area) {
				return null == area
					|| null == _fromArea
					|| null == _toArea
					|| _fromArea.Intersects(area)
					|| _toArea.Intersects(area);
			}

			public override IEnumerable<DynamicGraphNodeData<EpsgCrs, int, ICoordinateOperationInfo>> GetNeighborInfo(EpsgCrs node, int currentCost) {
				var costPlusOne = currentCost + 1;
				if (costPlusOne > 3)
					return Enumerable.Empty<DynamicGraphNodeData<EpsgCrs, int, ICoordinateOperationInfo>>(); 
				
				var nodeCode = node.Code;
				var results = new List<DynamicGraphNodeData<EpsgCrs, int, ICoordinateOperationInfo>>();

				foreach(var op in EpsgCoordinateOperationInfoRepository.GetConcatenatedForwardReferenced(nodeCode)) {
					var crs = op.TargetCrs;
					if(!AreaIntersectionPasses(crs.Area))
						continue;
					results.Add(new DynamicGraphNodeData<EpsgCrs, int, ICoordinateOperationInfo>(
						crs, costPlusOne, op));
				}

				foreach(var op in EpsgCoordinateOperationInfoRepository.GetConcatenatedReverseReferenced(nodeCode)) {
					if(!op.HasInverse)
						continue;
					var crs = op.SourceCrs;
					if(!AreaIntersectionPasses(crs.Area))
						continue;
					results.Add(new DynamicGraphNodeData<EpsgCrs, int, ICoordinateOperationInfo>(
						crs, costPlusOne, op.GetInverse()));
				}

				foreach(var op in EpsgCoordinateOperationInfoRepository.GetTransformForwardReferenced(nodeCode)) {
					var crs = op.TargetCrs;
					if(!AreaIntersectionPasses(crs.Area))
						continue;
					results.Add(new DynamicGraphNodeData<EpsgCrs, int, ICoordinateOperationInfo>(
						crs, costPlusOne, op));
				}

				foreach(var op in EpsgCoordinateOperationInfoRepository.GetTransformReverseReferenced(nodeCode)) {
					if(!op.HasInverse)
						continue;
					var crs = op.SourceCrs;
					if(!AreaIntersectionPasses(crs.Area))
						continue;
					results.Add(new DynamicGraphNodeData<EpsgCrs, int, ICoordinateOperationInfo>(
						crs, costPlusOne, op.GetInverse()));
				}

				if(results.Any(x => null == x || null == x.Edge || null == x.Node))
					throw new Exception();

				return results;
			}
		}

		private static ICoordinateOperationInfo GenerateConcatenated(List<ICoordinateOperationInfo> operations) {
			if (null == operations)
				return null;
			if (operations.Count == 0)
				return null;
			if (operations.Count == 1)
				return operations[0];
			return new ConcatenatedCoordinateOperationInfo(operations);
		}

		private class CrsOperationRelation
		{

			public static List<CrsOperationRelation> BuildSourceSearchList(EpsgCrs source) {
				var result = new List<CrsOperationRelation>();
				var cost = 0;
				var current = source;
				var operations = new List<ICoordinateOperationInfo>();
				while(current is EpsgCrsProjected) {
					var projected = current as EpsgCrsProjected;
					result.Add(new CrsOperationRelation { Cost = cost, RelatedCrs = current, Operations = operations.ToArray()});
					current = projected.BaseCrs;
					cost++;
					operations.Add(projected.Projection.GetInverse());
				}
				if(current is EpsgCrsGeodetic) {
					result.Add(new CrsOperationRelation{ Cost = cost, RelatedCrs = current, Operations = operations.ToArray()});
				}
				return result;
			}

			public static List<CrsOperationRelation> BuildTargetSearchList(EpsgCrs target) {
				var result = new List<CrsOperationRelation>();
				var cost = 0;
				var current = target;
				var operations = new List<ICoordinateOperationInfo>();
				while (current is EpsgCrsProjected) {
					var projected = current as EpsgCrsProjected;
					result.Add(new CrsOperationRelation { Cost = cost, RelatedCrs = current, Operations = operations.ToArray() });
					current = projected.BaseCrs;
					cost++;
					operations.Insert(0, projected.Projection);
				}
				if (current is EpsgCrsGeodetic) {
					result.Add(new CrsOperationRelation { Cost = cost, RelatedCrs = current, Operations = operations.ToArray() });
				}
				return result;
			} 

			public EpsgCrs RelatedCrs;
			public IEnumerable<ICoordinateOperationInfo> Operations;
			public int Cost;
		}

		private class CrsPathResult
		{
			public int Cost;
			public IEnumerable<ICoordinateOperationInfo> Operations;
		}

		public ICoordinateOperationInfo Generate(EpsgCrs from, EpsgCrs to) {
			var result = GenerateCore(from, to);
			return null == result ? null : GenerateConcatenated(result.ToList());
		}

		public IEnumerable<ICoordinateOperationInfo> GenerateCore(EpsgCrs from, EpsgCrs to) {
			// see if there is a direct path above the source
			var sourceList = CrsOperationRelation.BuildSourceSearchList(from);
			foreach(var source in sourceList) {
				if(source.RelatedCrs == to) {
					return source.Operations;
				}
			}

			// see if there is a direct path above the target
			var targetList = CrsOperationRelation.BuildTargetSearchList(to);
			foreach(var target in targetList) {
				if(target.RelatedCrs == from) {
					return target.Operations;
				}
			}
			
			// try to find the best path from any source to any target
			var graph = new EpsgTransformGraph(from.Area, to.Area);
			var results = new List<CrsPathResult>();
			foreach(var source in sourceList) {
				foreach(var target in targetList) {

					if (source.RelatedCrs == target.RelatedCrs) {
						results.Add(new CrsPathResult {
							Cost = source.Cost + target.Cost,
							Operations = source.Operations.Concat(target.Operations)
						});
					}

					var path = graph.FindPath(source.RelatedCrs, target.RelatedCrs);
					if(null == path)
						continue; // no path found

					int pathCost = source.Cost + target.Cost;
					var pathOperations = source.Operations.ToList();
					foreach(var pathPart in path) {
						pathCost += pathPart.Cost;
						pathOperations.Add(pathPart.Edge);
					}
					pathOperations.AddRange(target.Operations);

					results.Add(new CrsPathResult {
						Cost = pathCost,
						Operations = source.Operations.Concat(pathOperations).Concat(target.Operations)
					});
				}
			}

			// use a stable sort to get the best result
			var bestPath = results.OrderBy(x => x.Cost).FirstOrDefault();
			return null == bestPath ? null : bestPath.Operations;
		}

		public ICoordinateOperationInfo Generate(ICrs from, ICrs to) {
			if(from is EpsgCrs && to is EpsgCrs) {
				return Generate(from as EpsgCrs, to as EpsgCrs);
			}
			// TODO: if one is not an EpsgCrs we should try making it one (but really it should already have been... so maybe not)
			// TODO: if one is EpsgCrs and the other is not, we need to find the nearest EpsgCrs along the way and use standard methods to get us there
			throw new NotImplementedException("Currently only EpsgCrs to EpsgCrs is supported."); // TODO: just return null if we don't know what to do with it?
			return null; // TODO: or maybe just return null and move on?
		}

	}

}
