using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Pigeoid.Contracts;
using Vertesaur.Search;

namespace Pigeoid.Epsg
{
	public class EpsgCrsCoordinateOperationPathGenerator :
		ICoordinateOperationPathGenerator<ICrs>,
		ICoordinateOperationPathGenerator<EpsgCrs>
	{

		public class SharedOptions
		{
			public bool IgnoreDeprecatedCrs { get; set; }
			public bool IgnoreDeprecatedOperations { get; set; }
		}

		private class EpsgTransformGraph : DynamicGraphBase<EpsgCrs, int, ICoordinateOperationInfo>
		{

			private readonly EpsgArea _fromArea;
			private readonly EpsgArea _toArea;
			private readonly SharedOptions _options;

			public EpsgTransformGraph([NotNull] EpsgArea fromArea, [NotNull] EpsgArea toArea, [NotNull] SharedOptions options) {
				_fromArea = fromArea;
				_toArea = toArea;
				_options = options;
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
				if (costPlusOne > 4)
					yield break;
				
				var nodeCode = node.Code;

				foreach(var op in EpsgCoordinateOperationInfoRepository.GetConcatenatedForwardReferenced(nodeCode)) {
					if(_options.IgnoreDeprecatedOperations && op.Deprecated)
						continue;
					var crs = op.TargetCrs;
					if (_options.IgnoreDeprecatedCrs && crs.Deprecated)
						continue;
					if(!AreaIntersectionPasses(crs.Area))
						continue;
					yield return new DynamicGraphNodeData<EpsgCrs, int, ICoordinateOperationInfo>(crs, costPlusOne, op);
				}

				foreach(var op in EpsgCoordinateOperationInfoRepository.GetConcatenatedReverseReferenced(nodeCode)) {
					if (_options.IgnoreDeprecatedOperations && op.Deprecated)
						continue;
					if(!op.HasInverse)
						continue;
					var crs = op.SourceCrs;
					if (_options.IgnoreDeprecatedCrs && crs.Deprecated)
						continue;
					if(!AreaIntersectionPasses(crs.Area))
						continue;
					yield return new DynamicGraphNodeData<EpsgCrs, int, ICoordinateOperationInfo>(crs, costPlusOne, op.GetInverse());
				}

				foreach(var op in EpsgCoordinateOperationInfoRepository.GetTransformForwardReferenced(nodeCode)) {
					if (_options.IgnoreDeprecatedOperations && op.Deprecated)
						continue;
					var crs = op.TargetCrs;
					if (_options.IgnoreDeprecatedCrs && crs.Deprecated)
						continue;
					if(!AreaIntersectionPasses(crs.Area))
						continue;
					yield return new DynamicGraphNodeData<EpsgCrs, int, ICoordinateOperationInfo>(crs, costPlusOne, op);
				}

				foreach(var op in EpsgCoordinateOperationInfoRepository.GetTransformReverseReferenced(nodeCode)) {
					if (_options.IgnoreDeprecatedOperations && op.Deprecated)
						continue;
					if(!op.HasInverse)
						continue;
					var crs = op.SourceCrs;
					if (_options.IgnoreDeprecatedCrs && crs.Deprecated)
						continue;
					if(!AreaIntersectionPasses(crs.Area))
						continue;
					yield return new DynamicGraphNodeData<EpsgCrs, int, ICoordinateOperationInfo>(crs, costPlusOne, op.GetInverse());
				}

			}
		}

		private class CrsOperationRelation
		{

			public static List<CrsOperationRelation> BuildSourceSearchList(EpsgCrs source) {
				/*var result = new List<CrsOperationRelation>();
				var cost = 0;
				var current = source;
				var path = new CoordinateOperationCrsPathInfo(source);

				while(current is EpsgCrsProjected) {
					var projected = current as EpsgCrsProjected;
					result.Add(new CrsOperationRelation { Cost = cost, RelatedCrs = current, Path = path});
					current = projected.BaseCrs;
					cost++;
					path = path.Append(current, projected.Projection.GetInverse());
				}
				if(current is EpsgCrsGeodetic) {
					result.Add(new CrsOperationRelation{ Cost = cost, RelatedCrs = current, Path = path});
				}
				return result;*/

				var cost = 0;
				var current = source;
				var path = new CoordinateOperationCrsPathInfo(source);
				var result = new List<CrsOperationRelation> {
					new CrsOperationRelation {Cost = cost, RelatedCrs = current, Path = path}
				};
				while (current is EpsgCrsProjected) {
					var projected = current as EpsgCrsProjected;
					var projection = projected.Projection;
					if (null == projection || !projection.HasInverse)
						break;

					cost++;
					current = projected.BaseCrs;
					path = path.Append(current, projection.GetInverse());
					result.Add(new CrsOperationRelation {Cost = cost, RelatedCrs = current, Path = path});
				}
				return result;
			}

			public static List<CrsOperationRelation> BuildTargetSearchList(EpsgCrs target) {
				var cost = 0;
				var current = target;
				var path = new CoordinateOperationCrsPathInfo(target);
				var result = new List<CrsOperationRelation> {
					new CrsOperationRelation {Cost = cost, RelatedCrs = current, Path = path}
				};
				while(current is EpsgCrsProjected) {
					var projected = current as EpsgCrsProjected;
					var projection = projected.Projection;
					if (null == projection)
						break;

					cost++;
					current = projected.BaseCrs;
					path = path.Prepend(current, projected.Projection);
					result.Add(new CrsOperationRelation { Cost = cost, RelatedCrs = current, Path = path});
				}
				return result;
				/*var result = new List<CrsOperationRelation>();
				var cost = 0;
				var current = target;
				var path = new CoordinateOperationCrsPathInfo(target);
				while (current is EpsgCrsProjected) {
					var projected = current as EpsgCrsProjected;
					result.Add(new CrsOperationRelation { Cost = cost, RelatedCrs = current, Path = path });
					current = projected.BaseCrs;
					cost++;
					path = path.Prepend(current, projected.Projection);
				}
				if (current is EpsgCrsGeodetic) {
					result.Add(new CrsOperationRelation { Cost = cost, RelatedCrs = current, Path = path });
				}
				return result;*/
			} 

			public EpsgCrs RelatedCrs;
			public CoordinateOperationCrsPathInfo Path;
			public int Cost;
		}

		private class CrsPathResult
		{
			public int Cost;
			public CoordinateOperationCrsPathInfo Path;
		}

		public EpsgCrsCoordinateOperationPathGenerator() {
			Options = new SharedOptions();
		}

		public SharedOptions Options { get; private set; }

		public ICoordinateOperationCrsPathInfo Generate(EpsgCrs from, EpsgCrs to) {
			// see if there is a direct path above the source
			var sourceList = CrsOperationRelation.BuildSourceSearchList(from);
			foreach(var source in sourceList) {
				if(source.RelatedCrs == to) {
					return source.Path;
				}
			}

			// see if there is a direct path above the target
			var targetList = CrsOperationRelation.BuildTargetSearchList(to);
			foreach(var target in targetList) {
				if(target.RelatedCrs == from) {
					return target.Path;
				}
			}
			
			// try to find the best path from any source to any target
			var graph = new EpsgTransformGraph(from.Area, to.Area, Options);
			var results = new List<CrsPathResult>();
			var lowestCost = Int32.MaxValue;
			foreach(var source in sourceList) {
				foreach(var target in targetList) {
					var pathCost = source.Cost + target.Cost;
					if(pathCost >= lowestCost)
						continue;

					if (source.RelatedCrs == target.RelatedCrs) {
						results.Add(new CrsPathResult {
							Cost = pathCost,
							Path = source.Path.Append(target.Path)
						});
						if (pathCost < lowestCost)
							lowestCost = pathCost;

						continue;
					}

					if ((pathCost+1) >= lowestCost)
						continue;

					var path = graph.FindPath(source.RelatedCrs, target.RelatedCrs);
					if(null == path)
						continue; // no path found

					var pathOperations = source.Path;
					for (int partIndex = 1; partIndex < path.Count; partIndex++) {
						var part = path[partIndex];
						pathCost += part.Cost;
						pathOperations = pathOperations.Append(part.Node, part.Edge);
					}
					pathOperations = pathOperations.Append(target.Path);

					results.Add(new CrsPathResult {
						Cost = pathCost,
						Path = pathOperations //source.Path.Append(pathOperations).Append(target.Path)
					});

					if(pathCost < lowestCost)
						lowestCost = pathCost;
				}
			}

			// find the smallest result;
			ICoordinateOperationCrsPathInfo bestResult = null;
			lowestCost = Int32.MaxValue;
			foreach(var result in results){
				if(result.Cost < lowestCost){
					lowestCost = result.Cost;
					bestResult = result.Path;
				}
			}
			return bestResult;
		}

		public ICoordinateOperationCrsPathInfo Generate(ICrs from, ICrs to) {
			if(from is EpsgCrs && to is EpsgCrs) {
				return Generate(from as EpsgCrs, to as EpsgCrs);
			}
			// TODO: if one is not an EpsgCrs we should try making it one (but really it should already have been... so maybe not)
			// TODO: if one is EpsgCrs and the other is not, we need to find the nearest EpsgCrs along the way and use standard methods to get us there
			throw new NotImplementedException("Currently only 'EpsgCrs' to 'EpsgCrs' is supported."); // TODO: just return null if we don't know what to do with it?
			return null; // TODO: or maybe just return null and move on?
		}

	}

}
