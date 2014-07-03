using Pigeoid.Unit;
using Pigeoid.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Pigeoid.CoordinateOperation
{
    public abstract class NamedParameterSelector
    {

        public class ParameterData
        {

            public ParameterData(INamedParameter namedParameter, string normalizedName) {
                if (namedParameter == null) throw new ArgumentNullException("namedParameter");
                if (String.IsNullOrEmpty(normalizedName)) throw new ArgumentException("Invalid parameter name", "normalizedName");
                Contract.EndContractBlock();
                NamedParameter = namedParameter;
                NormalizedName = normalizedName;
            }

            [ContractInvariantMethod]
            private void CodeContractInvariants() {
                Contract.Invariant(NamedParameter != null);
                Contract.Invariant(!String.IsNullOrEmpty(NormalizedName));
            }

            public INamedParameter NamedParameter { get; private set; }
            public string NormalizedName { get; private set; }
        }

        public static bool AllAreSelected(params NamedParameterSelector[] selectors) {
            Contract.Requires(selectors != null);
            Contract.Requires(Contract.ForAll(selectors, x => x != null));
            if (selectors.Length == 0)
                return false;
            for (var i = 0; i < selectors.Length; i++) {
                if (!selectors[i].IsSelected)
                    return false;
            }
            return true;
        }

        protected NamedParameterSelector() {
            Selection = null;
        }

        public bool IsSelected { get { return null != Selection; } }
        public INamedParameter Selection { get; private set; }

        // TODO: Contract.Requires(parameterData != null)
        public abstract int Score(ParameterData parameterData);

        public bool Select(INamedParameter namedParameter) {
            if (null != Selection)
                return false;

            Selection = namedParameter;
            return true;
        }

        public bool? GetValueAsBoolean() {
            if (IsSelected) {
                var rawValue = Selection.Value;
                return ConversionUtil.ConvertBooleanMultiCulture(rawValue);
            }
            return null;
        }

        public int? GetValueAsInt32()
        {
            if (IsSelected)
            {
                var rawValue = Selection.Value;
                int value;
                if (ConversionUtil.TryConvertInt32MultiCulture(rawValue, out value))
                    return value;
            }
            return null;
        }

        public double? GetValueAsDouble() {
            if (IsSelected) {
                var rawValue = Selection.Value;
                double value;
                if (ConversionUtil.TryConvertDoubleMultiCulture(rawValue, out value))
                    return value;
            }
            return null;
        }

        public double? GetValueAsDouble(IUnit unit) {
            var value = GetValueAsDouble();
            if (unit != null && Selection.Unit != null && value.HasValue && !unit.Equals(Selection.Unit)) {
                var conversion = SimpleUnitConversionGenerator.FindConversion(Selection.Unit, unit);
                if (conversion == null)
                    throw new InvalidOperationException();

                value = conversion.TransformValue(value.GetValueOrDefault());
            }
            return value;
        }

    }

    public class FullMatchParameterSelector : NamedParameterSelector
    {

        public FullMatchParameterSelector(string match) {
            if (String.IsNullOrEmpty(match)) throw new ArgumentNullException("match");
            Contract.EndContractBlock();
            Match = match;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(!String.IsNullOrEmpty(Match));
        }

        public string Match { get; private set; }

        public override int Score(ParameterData parameterData) {
            var parameterName = parameterData.NormalizedName;
            return Match.Equals(parameterName) ? 1 : 0;
        }
    }

    public class KeywordNamedParameterSelector : NamedParameterSelector
    {

        private readonly string[] _keywords;

        public KeywordNamedParameterSelector(params string[] keywords) {
            if(keywords == null) throw new ArgumentNullException("keywords");
            Contract.Requires(Contract.ForAll(keywords, x => x != null));
            _keywords = keywords;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(_keywords != null);
            Contract.Invariant(Contract.ForAll(_keywords, x => x != null));
        }

        public ReadOnlyCollection<string> Keywords {
            get {
                Contract.Ensures(Contract.Result<ReadOnlyCollection<string>>() != null);
                return new ReadOnlyCollection<string>(_keywords);
            }
        }

        public override int Score(ParameterData parameterData) {
            Contract.Ensures(Contract.Result<int>() >= 0);
            var parameterName = parameterData.NormalizedName;
            if (String.IsNullOrEmpty(parameterName)) // TODO: this check is redundant
                return 0;

            var score = 0;
            for (int keywordIndex = 0; keywordIndex < _keywords.Length; keywordIndex++) {
                if (parameterName.Contains(_keywords[keywordIndex]))
                    score++;
            }
            return score;
        }
    }

}
