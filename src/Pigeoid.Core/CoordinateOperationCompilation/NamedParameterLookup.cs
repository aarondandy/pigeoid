using JetBrains.Annotations;
using Pigeoid.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Pigeoid.Interop;

namespace Pigeoid.CoordinateOperationCompilation
{
	public class NamedParameterLookup
	{

		private readonly NamedParameterSelector.ParameterData[] _data;
		private readonly ParameterNameNormalizedComparer _parameterNameNormalizedComparer;

		public NamedParameterLookup([NotNull] params INamedParameter[] parameters) {
			_parameterNameNormalizedComparer = ParameterNameNormalizedComparer.Default;
			_data = Array.ConvertAll(
				parameters,
				x => new NamedParameterSelector.ParameterData(x, _parameterNameNormalizedComparer.Normalize(x.Name))
			);
		}

		public ParameterNameNormalizedComparer ParameterNameNormalizedComparer { get { return _parameterNameNormalizedComparer; } }

		public NamedParameterLookup(ICoordinateOperationInfo operationInfo)
			: this(operationInfo as IParameterizedCoordinateOperationInfo) { }

		public NamedParameterLookup(IParameterizedCoordinateOperationInfo operationInfo)
			: this(null == operationInfo ? Enumerable.Empty<INamedParameter>() : operationInfo.Parameters) { }

		public NamedParameterLookup([NotNull, InstantHandle] IEnumerable<INamedParameter> parameters)
			: this(parameters.ToArray()) { }

		public bool Assign(params NamedParameterSelector[] selectors) {
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
