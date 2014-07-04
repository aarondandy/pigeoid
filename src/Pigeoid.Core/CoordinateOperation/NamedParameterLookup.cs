using System.Diagnostics.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Pigeoid.Interop;

namespace Pigeoid.CoordinateOperation
{
    public class NamedParameterLookup
    {

        private readonly NamedParameterSelector.ParameterData[] _data;

        public NamedParameterLookup(params INamedParameter[] parameters) {
            if(parameters == null) throw new ArgumentNullException("parameters");
            Contract.EndContractBlock();

            ParameterNameNormalizedComparer = ParameterNameNormalizedComparer.Default;
            _data = Array.ConvertAll(
                parameters,
                x => new NamedParameterSelector.ParameterData(x, ParameterNameNormalizedComparer.Normalize(x.Name))
            );
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(_data != null);
            Contract.Invariant(Contract.ForAll(_data, x => x != null));
            Contract.Invariant(ParameterNameNormalizedComparer != null);
        }

        public ParameterNameNormalizedComparer ParameterNameNormalizedComparer { get; private set; }

        [Obsolete]
        private static IEnumerable<INamedParameter> ToNamedParameters( IParameterizedCoordinateOperationInfo operationInfo) {
            Contract.Ensures(Contract.Result<IEnumerable<INamedParameter>>() != null);
            return operationInfo != null && operationInfo.Parameters != null
                ? operationInfo.Parameters
                : Enumerable.Empty<INamedParameter>();
        }

        [Obsolete]
        public NamedParameterLookup(ICoordinateOperationInfo operationInfo)
            : this(operationInfo as IParameterizedCoordinateOperationInfo) { }

        [Obsolete]
        public NamedParameterLookup(IParameterizedCoordinateOperationInfo operationInfo)
            : this(ToNamedParameters(operationInfo)) { }

        public NamedParameterLookup(IEnumerable<INamedParameter> parameters) : this(parameters.ToArray()) {
            Contract.Requires(parameters != null);
        }

        private struct SelectorScore
        {
            public int Score;
            public NamedParameterSelector Selector;
        }

        private struct ParamSelectorData
        {
            public int ParameterIndex;
            public List<SelectorScore> Selectors;

            public SelectorScore GetBestSelector() {
                return Selectors.Where(x => !x.Selector.IsSelected).FirstOrDefault();
            }

        }

        public bool Assign(params NamedParameterSelector[] selectors) {
            if(selectors == null) throw new ArgumentNullException("selectors");
            Contract.Requires(Contract.ForAll(selectors, x => x != null));

            /*var paramsToSearch = _data.ToList();
            foreach (var selector in selectors) {
                Contract.Assume(selector != null);
                if (paramsToSearch.Count == 0)
                    break;

                int bestScore = 0;
                int bestIndex = -1;
                for (int i = 0; i < paramsToSearch.Count; i++) {
                    int score = selector.Score(paramsToSearch[i]);
                    if (score > 0 && score > bestScore) {
                        bestScore = score;
                        bestIndex = i;
                    }
                }
                if (bestIndex >= 0) {
                    Contract.Assume(bestIndex < paramsToSearch.Count);
                    selector.Select(paramsToSearch[bestIndex].NamedParameter);
                    paramsToSearch.RemoveAt(bestIndex);
                }
            }
            return selectors.All(x => x.IsSelected);*/

            var scoredSelectors = new List<ParamSelectorData>(_data.Length);
            for (int parameterIndex = 0; parameterIndex < _data.Length; ++parameterIndex) {
                var parameter = _data[parameterIndex];
                var localSelectors = new List<SelectorScore>();

                for (int selectorIndex = 0; selectorIndex < selectors.Length; ++selectorIndex) {
                    var selector = selectors[selectorIndex];
                    var score = selector.Score(parameter);
                    if (score > 0)
                        localSelectors.Add(new SelectorScore {
                            Selector = selector,
                            Score = score });
                }

                if (localSelectors.Count > 0) {
                    localSelectors.Sort((a, b) => b.Score.CompareTo(a.Score));
                    scoredSelectors.Add(new ParamSelectorData {
                        ParameterIndex = parameterIndex,
                        Selectors = localSelectors
                    });
                }
            }

            var checkRequired = true;
            while (checkRequired) {
                checkRequired = false;
                int highScore = 0;
                int bestRootIndex = -1;
                NamedParameterSelector bestSelector = null;
                for (int i = 0; i < scoredSelectors.Count; i++) {
                    var paramSelectorData = scoredSelectors[i];
                    var best = paramSelectorData.GetBestSelector();
                    if (best.Selector != null && best.Score > highScore) {
                        highScore = best.Score;
                        bestSelector = best.Selector;
                        bestRootIndex = i;
                    }
                }

                if (bestRootIndex >= 0 && bestSelector != null) {
                    var parameter = _data[scoredSelectors[bestRootIndex].ParameterIndex];
                    bestSelector.Select(parameter.NamedParameter);
                    scoredSelectors.RemoveAt(bestRootIndex);
                    checkRequired = scoredSelectors.Count > 0;
                }

            }

            return selectors.All(x => x.IsSelected);
        }

    }
}
