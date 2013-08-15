﻿using System.Diagnostics.Contracts;
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
            Contract.Invariant(ParameterNameNormalizedComparer != null);
        }

        public ParameterNameNormalizedComparer ParameterNameNormalizedComparer { get; private set; }

        [Obsolete]
        public NamedParameterLookup(ICoordinateOperationInfo operationInfo)
            : this(operationInfo as IParameterizedCoordinateOperationInfo) { }

        [Obsolete]
        public NamedParameterLookup(IParameterizedCoordinateOperationInfo operationInfo)
            : this(null == operationInfo ? Enumerable.Empty<INamedParameter>() : operationInfo.Parameters) { }

        public NamedParameterLookup(IEnumerable<INamedParameter> parameters) : this(parameters.ToArray()) {
            Contract.Requires(parameters != null);
        }

        public bool Assign(params NamedParameterSelector[] selectors) {
            if(selectors == null) throw new ArgumentNullException("selectors");
            Contract.EndContractBlock();
            var paramsToSearch = _data.ToList();
            foreach (var selector in selectors) {
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
                    selector.Select(paramsToSearch[bestIndex].NamedParameter);
                    paramsToSearch.RemoveAt(bestIndex);
                }
            }
            return selectors.All(x => x.IsSelected);
        }

    }
}