using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Pigeoid.CoordinateOperation;
using Vertesaur.Search;

namespace Pigeoid.Epsg
{
    [Obsolete("Use EpsgCrsCoordinateOperationPathGenerator")]
    internal class EpsgCrsCoordinateOperationPathGeneratorOld /*:
        ICoordinateOperationPathGenerator<ICrs>,
        ICoordinateOperationPathGenerator<EpsgCrs>*/
    {

        public class SharedOptions
        {

            public abstract class AreaValidatorBase
            {

                protected AreaValidatorBase(EpsgArea fromArea, EpsgArea toArea) {
                    FromArea = fromArea;
                    ToArea = toArea;
                }

                public EpsgArea FromArea { get; protected set; }
                public EpsgArea ToArea { get; protected set; }

                protected virtual bool IsValid(EpsgArea area) {
                    return area == null
                        || FromArea == null
                        || FromArea.Intersects(area)
                        || ToArea == null
                        || ToArea.Intersects(area);
                }
            }

            public class CrsValidator : AreaValidatorBase
            {

                public CrsValidator(EpsgArea fromArea, EpsgArea toArea) : base(fromArea, toArea) { }

                public virtual bool IsValid(EpsgCrs crs) {
                    if(crs == null) throw new ArgumentNullException("crs");
                    Contract.EndContractBlock();
                    return IsValid(crs.Area);
                }

            }

            public class OperationValidator : AreaValidatorBase
            {

                public OperationValidator(EpsgArea fromArea, EpsgArea toArea) : base(fromArea, toArea) { }

                public virtual bool IsValid(EpsgCoordinateOperationInfoBase operation) {
                    if(operation == null) throw new ArgumentNullException("operation");
                    Contract.EndContractBlock();
                    return IsValid(operation.Area);
                }
            }

            public virtual CrsValidator CreateCrsValidator(EpsgArea fromArea, EpsgArea toArea) {
                Contract.Ensures(Contract.Result<CrsValidator>() != null);
                return new CrsValidator(fromArea, toArea);
            }

            public virtual OperationValidator CreateOperationValidator(EpsgArea fromArea, EpsgArea toArea) {
                Contract.Ensures(Contract.Result<OperationValidator>() != null);
                return new OperationValidator(fromArea, toArea);
            }

        }

        public class SharedOptionsAreaPredicate : SharedOptions
        {
            private class CrsPredicateValidator : CrsValidator
            {
                private readonly Predicate<EpsgCrs> _predicate;

                public CrsPredicateValidator(Predicate<EpsgCrs> predicate, EpsgArea fromArea, EpsgArea toArea)
                    : base(fromArea, toArea) {
                    Contract.Requires(predicate != null);
                    _predicate = predicate;
                }

                [ContractInvariantMethod]
                private void CodeContractInvariants() {
                    Contract.Invariant(_predicate != null);
                }

                public override bool IsValid(EpsgCrs crs) {
                    if (crs == null) throw new ArgumentNullException("crs");
                    Contract.EndContractBlock();
                    return base.IsValid(crs) && _predicate(crs);
                }
            }

            private class OperationPredicateValidator : OperationValidator
            {
                private readonly Predicate<EpsgCoordinateOperationInfoBase> _predicate;

                public OperationPredicateValidator(Predicate<EpsgCoordinateOperationInfoBase> predicate, EpsgArea fromArea, EpsgArea toArea)
                    : base(fromArea, toArea) {
                    Contract.Requires(predicate != null);
                    _predicate = predicate;
                }

                [ContractInvariantMethod]
                private void CodeContractInvariants() {
                    Contract.Invariant(_predicate != null);
                }

                public override bool IsValid(EpsgCoordinateOperationInfoBase operation) {
                    if (operation == null) throw new ArgumentNullException("operation");
                    Contract.EndContractBlock();
                    return base.IsValid(operation) && _predicate(operation);
                }
            }

            private readonly Predicate<EpsgCrs> _crsPredicate;
            private readonly Predicate<EpsgCoordinateOperationInfoBase> _opPredicate;

            public SharedOptionsAreaPredicate(Predicate<EpsgCrs> crsPredicate, Predicate<EpsgCoordinateOperationInfoBase> operationPredicate) {
                if (null == crsPredicate) throw new ArgumentNullException("crsPredicate");
                if (null == operationPredicate) throw new ArgumentNullException("operationPredicate");
                Contract.EndContractBlock();

                _crsPredicate = crsPredicate;
                _opPredicate = operationPredicate;
            }

            [ContractInvariantMethod]
            private void CodeContractInvariants() {
                Contract.Invariant(_crsPredicate != null);
                Contract.Invariant(_opPredicate != null);
            }

            public override CrsValidator CreateCrsValidator(EpsgArea fromArea, EpsgArea toArea) {
                return new CrsPredicateValidator(_crsPredicate, fromArea, toArea);
            }

            public override OperationValidator CreateOperationValidator(EpsgArea fromArea, EpsgArea toArea) {
                return new OperationPredicateValidator(_opPredicate, fromArea, toArea);
            }

        }

        private class EpsgTransformGraph : DynamicGraphBase<EpsgCrs, int, ICoordinateOperationInfo>
        {

            private readonly SharedOptions _options;
            private readonly SharedOptions.CrsValidator _crsValidator;
            private readonly SharedOptions.OperationValidator _opValidator;

            public EpsgTransformGraph(EpsgArea fromArea, EpsgArea toArea, SharedOptions options) {
                Contract.Requires(fromArea != null);
                Contract.Requires(toArea != null);
                Contract.Requires(options != null);
                _options = options;
                _crsValidator = _options.CreateCrsValidator(fromArea, toArea);
                _opValidator = _options.CreateOperationValidator(fromArea, toArea);
            }

            [ContractInvariantMethod]
            private void CodeContractInvariants() {
                Contract.Invariant(_options != null);
                Contract.Invariant(_opValidator != null);
                Contract.Invariant(_crsValidator != null);
            }

            protected override IEnumerable<DynamicGraphNodeData<EpsgCrs, int, ICoordinateOperationInfo>> GetNeighborInfo(EpsgCrs node, int currentCost) {
                Contract.Ensures(Contract.Result<IEnumerable<DynamicGraphNodeData<EpsgCrs, int, ICoordinateOperationInfo>>>() != null);
                var costPlusOne = currentCost + 1;
                if (costPlusOne > 4)
                    yield break;

                var nodeCode = node.Code;

                foreach (var op in EpsgCoordinateOperationInfoRepository.GetConcatenatedForwardReferenced(nodeCode)) {
                    if (!_opValidator.IsValid(op))
                        continue;
                    var crs = op.TargetCrs;
                    if (!_crsValidator.IsValid(crs))
                        continue;
                    yield return new DynamicGraphNodeData<EpsgCrs, int, ICoordinateOperationInfo>(crs, costPlusOne, op);
                }

                foreach (var op in EpsgCoordinateOperationInfoRepository.GetConcatenatedReverseReferenced(nodeCode)) {
                    if (!_opValidator.IsValid(op))
                        continue;
                    if (!op.HasInverse)
                        continue;
                    var crs = op.SourceCrs;
                    if (!_crsValidator.IsValid(crs))
                        continue;
                    yield return new DynamicGraphNodeData<EpsgCrs, int, ICoordinateOperationInfo>(crs, costPlusOne, op.GetInverse());
                }

                foreach (var op in EpsgCoordinateOperationInfoRepository.GetTransformForwardReferenced(nodeCode)) {
                    if (!_opValidator.IsValid(op))
                        continue;
                    var crs = op.TargetCrs;
                    if (!_crsValidator.IsValid(crs))
                        continue;
                    yield return new DynamicGraphNodeData<EpsgCrs, int, ICoordinateOperationInfo>(crs, costPlusOne, op);
                }

                foreach (var op in EpsgCoordinateOperationInfoRepository.GetTransformReverseReferenced(nodeCode)) {
                    if (!_opValidator.IsValid(op))
                        continue;
                    if (!op.HasInverse)
                        continue;
                    var crs = op.SourceCrs;
                    if (!_crsValidator.IsValid(crs))
                        continue;
                    yield return new DynamicGraphNodeData<EpsgCrs, int, ICoordinateOperationInfo>(crs, costPlusOne, op.GetInverse());
                }

            }
        }

        private class CrsOperationRelation
        {

            public static List<CrsOperationRelation> BuildSourceSearchList(EpsgCrs source) {
                Contract.Requires(source != null);
                Contract.Ensures(Contract.Result<List<CrsOperationRelation>>() != null);
                Contract.Ensures(Contract.ForAll(Contract.Result<List<CrsOperationRelation>>(), x => x != null));
                var cost = 0;
                var current = source;
                var path = new CoordinateOperationCrsPathInfo(source);
                var result = new List<CrsOperationRelation> { new CrsOperationRelation {Cost = cost, RelatedCrs = current, Path = path} };
                while (current is EpsgCrsProjected) {
                    var projected = current as EpsgCrsProjected;
                    var projection = projected.Projection;
                    if (null == projection || !projection.HasInverse)
                        break;

                    cost++;
                    current = projected.BaseCrs;
                    path = path.Append(current, projection.GetInverse());
                    result.Add(new CrsOperationRelation { Cost = cost, RelatedCrs = current, Path = path });
                }
                Contract.Assume(Contract.ForAll(result, x => x != null));
                return result;
            }

            public static List<CrsOperationRelation> BuildTargetSearchList(EpsgCrs target) {
                Contract.Requires(target != null);
                Contract.Ensures(Contract.Result<List<CrsOperationRelation>>() != null);
                Contract.Ensures(Contract.ForAll(Contract.Result<List<CrsOperationRelation>>(), x => x != null));
                var cost = 0;
                var current = target;
                var path = new CoordinateOperationCrsPathInfo(target);
                var result = new List<CrsOperationRelation> {
					new CrsOperationRelation {Cost = cost, RelatedCrs = current, Path = path}
				};
                while (current is EpsgCrsProjected) {
                    var projected = current as EpsgCrsProjected;
                    var projection = projected.Projection;
                    if (null == projection)
                        break;

                    cost++;
                    current = projected.BaseCrs;
                    path = path.Prepend(current, projected.Projection);
                    result.Add(new CrsOperationRelation { Cost = cost, RelatedCrs = current, Path = path });
                }
                Contract.Assume(Contract.ForAll(result, x => x != null));
                return result;
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

        public EpsgCrsCoordinateOperationPathGeneratorOld() : this(null) { }

        public EpsgCrsCoordinateOperationPathGeneratorOld(SharedOptions options) {
            Options = options ?? new SharedOptions();
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(Options != null);
        }

        public SharedOptions Options { get; private set; }

        public ICoordinateOperationCrsPathInfo Generate(EpsgCrs from, EpsgCrs to) {
            if (from == null || to == null)
                return null;
            // see if there is a direct path above the source
            var sourceList = CrsOperationRelation.BuildSourceSearchList(from);
            foreach (var source in sourceList) {
                if (source.RelatedCrs == to) {
                    return source.Path;
                }
            }

            // see if there is a direct path above the target
            var targetList = CrsOperationRelation.BuildTargetSearchList(to);
            foreach (var target in targetList) {
                if (target.RelatedCrs == from) {
                    return target.Path;
                }
            }

            // try to find the best path from any source to any target
            var graph = new EpsgTransformGraph(from.Area, to.Area, Options);
            var results = new List<CrsPathResult>();
            var lowestCost = Int32.MaxValue;
            foreach (var source in sourceList) {
                foreach (var target in targetList) {
                    var pathCost = source.Cost + target.Cost;
                    if (pathCost >= lowestCost)
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

                    if ((pathCost + 1) >= lowestCost)
                        continue;

                    var path = graph.FindPath(source.RelatedCrs, target.RelatedCrs);
                    if (null == path)
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

                    if (pathCost < lowestCost)
                        lowestCost = pathCost;
                }
            }

            // find the smallest result;
            ICoordinateOperationCrsPathInfo bestResult = null;
            lowestCost = Int32.MaxValue;
            foreach (var result in results) {
                if (result.Cost < lowestCost) {
                    lowestCost = result.Cost;
                    bestResult = result.Path;
                }
            }
            return bestResult;
        }

        public ICoordinateOperationCrsPathInfo Generate(ICrs from, ICrs to) {
            if (from is EpsgCrs && to is EpsgCrs) {
                return Generate(from as EpsgCrs, to as EpsgCrs);
            }
            // TODO: if one is not an EpsgCrs we should try making it one (but really it should already have been... so maybe not)
            // TODO: if one is EpsgCrs and the other is not, we need to find the nearest EpsgCrs along the way and use standard methods to get us there
            throw new NotImplementedException("Currently only 'EpsgCrs' to 'EpsgCrs' is supported."); // TODO: just return null if we don't know what to do with it?
        }

    }

}
