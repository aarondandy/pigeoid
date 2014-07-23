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

            public override string ToString() {
                return NormalizedName + ": " + NamedParameter.Value;
            }
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

        public bool TryGetValueAsDouble(out double value) {
            if (IsSelected) {
                var rawValue = Selection.Value;
                return ConversionUtil.TryConvertDoubleMultiCulture(rawValue, out value);
            }
            value = default(double);
            return false;
        }

        public bool TryGetValueAsDouble(IUnit unit, out double value) {
            if (!TryGetValueAsDouble(out value))
                return false;
            if (unit != null && Selection.Unit != null && !unit.Equals(Selection.Unit)) {
                var conversion = SimpleUnitConversionGenerator.FindConversion(Selection.Unit, unit);
                if (conversion == null)
                    throw new InvalidOperationException();

                value = conversion.TransformValue(value);
            }
            return true;

        }

    }

    public class MultiParameterSelector : NamedParameterSelector
    {
        private readonly NamedParameterSelector[] _selectors;

        public MultiParameterSelector(params NamedParameterSelector[] selectors) {
            if (selectors == null) throw new ArgumentNullException();
            Contract.EndContractBlock();
            _selectors = selectors;
        }

        [ContractInvariantMethod]
        private void ObjectInvariants() {
            Contract.Invariant(_selectors != null);
        }

        public override int Score(ParameterData parameterData) {
            int highScore = 0;
            foreach (var selector in _selectors) {
                var score = selector.Score(parameterData);
                if (score > highScore)
                    highScore = score;
            }
            return highScore;
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
            return Match.Equals(parameterData.NormalizedName) ? 100 : 0;
        }
    }

    public class KeywordNamedParameterSelector : NamedParameterSelector
    {

        private readonly string[] _keywords;

        [Obsolete("Keywords will need to be weighted")]
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

            var score = 0;
            for (int keywordIndex = 0; keywordIndex < _keywords.Length; keywordIndex++) {
                if (parameterName.Contains(_keywords[keywordIndex]))
                    score++;
            }
            return score;
        }
    }

    public class LatitudeOfCenterParameterSelector : NamedParameterSelector
    {

        public override int Score(ParameterData parameterData) {
            var parameterName = parameterData.NormalizedName;
            if ("LATOFPROJECTIONCENTER".Equals(parameterName))
                return 100;

            int startPoints;
            if (parameterName.StartsWith("LATITUDE"))
                startPoints = 10;
            else if (parameterName.StartsWith("LAT"))
                startPoints = 5;
            else
                return 0;

            int endPoints;
            if (parameterName.EndsWith("ORIGIN") || parameterName.EndsWith("CENTER") || parameterName.EndsWith("CENTRE"))
                endPoints = 10;
            else
                return 0;

            var middlePoint = 0;
            if (parameterName.Contains("PROJECTED") || parameterName.Contains("PROJECTION"))
                middlePoint = 10;

            if (startPoints > 0 && endPoints > 0)
                return startPoints + endPoints + middlePoint;

            return 0;
        }
    }

    public class LatitudeOfNaturalOriginParameterSelector : NamedParameterSelector
    {
        public override int Score(NamedParameterSelector.ParameterData parameterData) {
            var parameterName = parameterData.NormalizedName;
            if ("LATOFNATURALORIGIN".Equals(parameterName))
                return 100;
            if ("LATOFORIGIN".Equals(parameterName))
                return 50;

            int startPoints;
            if (parameterName.StartsWith("LATITUDE"))
                startPoints = 10;
            else if (parameterName.StartsWith("LAT"))
                startPoints = 5;
            else
                return 0;

            int endPoints;
            if (parameterName.EndsWith("ORIGIN"))
                endPoints = 10;
            else
                return 0;

            var middlePoint = 0;
            if (parameterName.Contains("NATURAL"))
                middlePoint += 10;

            if (startPoints > 0 && endPoints > 0)
                return startPoints + endPoints + middlePoint;

            return 0;
        }
    }

    public class LatitudeOfTrueScaleParameterSelector : NamedParameterSelector
    {
        public override int Score(ParameterData parameterData) {
            var parameterName = parameterData.NormalizedName;
            if ("LATOFTRUESCALE".Equals(parameterName))
                return 100;

            int startPoints;
            if (parameterName.StartsWith("LATITUDE"))
                startPoints = 10;
            else if (parameterName.StartsWith("LAT"))
                startPoints = 5;
            else
                return 0;

            int endPoints;
            if (parameterName.EndsWith("SCALE"))
                endPoints = 10;
            else
                return 0;

            var middlePoints = 0;
            if (parameterName.Contains("TRUE"))
                middlePoints += 10;

            if (startPoints > 0 && endPoints > 0)
                return startPoints + endPoints + middlePoints;

            return 0;
        }
    }

    public class StandardParallelParameterSelector : NamedParameterSelector
    {

        public readonly int? _number;

        public StandardParallelParameterSelector() {
            _number = null;
        }

        public StandardParallelParameterSelector(int number) {
            _number = number;
        }

        private static readonly string[] StandardParallelStrings = new[] {
            "STANDARDPARALLEL",
            "STANDARD",
            "PARALLEL"
        };

        private static bool StartsWithStandardParallel(string normalizedParamemterName, out int points) {
            foreach (var searchText in StandardParallelStrings) {
                if (normalizedParamemterName.StartsWith(searchText)) {
                    points = searchText.Length;
                    return true;
                }
            }
            points = 0;
            return false;
        }

        private static bool EndsWithStandardParallel(string normalizedParamemterName, out int points) {
            foreach (var searchText in StandardParallelStrings) {
                if (normalizedParamemterName.EndsWith(searchText)) {
                    points = searchText.Length;
                    return true;
                }
            }
            points = 0;
            return false;
        }

        public override int Score(ParameterData parameterData) {
            var parameterName = parameterData.NormalizedName;
            if (_number.HasValue) {
                if("STANDARDPARALLEL".Equals(parameterName))
                    return 1;// if the number is set, it must also match

                int spPoints, numberPoints;
                if (parameterName.Contains("STANDARDPARALLEL")) {
                    spPoints = 50;
                }
                else {
                    spPoints = 0;
                    if (parameterName.Contains("STANDARD"))
                        spPoints += 25;
                    if (parameterName.Contains("PARALLEL"))
                        spPoints += 25;
                }

                if (spPoints <= 0)
                    return 0;

                var number = _number.GetValueOrDefault();
                if(number == 1 && (parameterName.Contains("ONE") || parameterName.Contains("1ST")))
                    numberPoints = 50;
                else if(number == 2 && (parameterName.Contains("TWO") || parameterName.Contains("2ND")))
                    numberPoints = 50;
                else if(number == 3 && (parameterName.Contains("THREE") || parameterName.Contains("3RD")))
                    numberPoints = 50;
                else if (parameterName.Contains(number.ToString()))
                    numberPoints = 25;
                else
                    return 0;
                
                if(spPoints > 0 && numberPoints > 0)
                    return spPoints + numberPoints;
            }
            else {
                int startPoints, endPoints;
                if ("STANDARDPARALLEL".Equals(parameterName))
                    return 100;
                if (parameterName.EndsWith("STANDARDPARALLEL"))
                    return 50;
                if (parameterName.StartsWith("STANDARDPARALLEL"))
                    return 50;

                if (parameterName.StartsWith("STANDARD"))
                    startPoints = 25;
                else
                    return 0;

                if (parameterName.EndsWith("PARALLEL"))
                    endPoints = 25;
                else if (parameterName.Contains("PARALLEL"))
                    endPoints = 5; // because it may not know about the ordinal suffix
                else
                    return 0;
                if (startPoints > 0 && endPoints > 0)
                    return startPoints + endPoints;
            }

            return 0;
        }
    }

    public class LongitudeOfCenterParameterSelector : NamedParameterSelector
    {

        public override int Score(NamedParameterSelector.ParameterData parameterData) {
            var parameterName = parameterData.NormalizedName;
            if ("LONOFPROJECTIONCENTER".Equals(parameterName))
                return 100;

            int startPoints;
            if (parameterName.StartsWith("LONGITUDE"))
                startPoints = 10;
            else if (parameterName.StartsWith("LON"))
                startPoints = 5;
            else
                return 0;

            int endPoints;
            if (parameterName.EndsWith("CENTER") || parameterName.EndsWith("CENTRE"))
                endPoints = 10;
            else
                return 0;

            var middlePoint = 0;
            if (parameterName.Contains("PROJECTED") || parameterName.Contains("PROJECTION"))
                middlePoint = 10;

            if (startPoints > 0 && endPoints > 0)
                return startPoints + endPoints + middlePoint;

            return 0;
        }
    }

    public class LongitudeOfNaturalOriginParameterSelector : NamedParameterSelector
    {
        public override int Score(NamedParameterSelector.ParameterData parameterData) {
            var parameterName = parameterData.NormalizedName;
            if ("LONOFNATURALORIGIN".Equals(parameterName))
                return 100;
            if ("LONOFORIGIN".Equals(parameterName))
                return 50;

            int startPoints;
            if (parameterName.StartsWith("LONGITUDE"))
                startPoints = 10;
            else if (parameterName.StartsWith("LON"))
                startPoints = 5;
            else
                return 0;

            int endPoints;
            if (parameterName.EndsWith("ORIGIN"))
                endPoints = 10;
            else
                return 0;

            var middlePoint = 0;
            if (parameterName.Contains("NATURAL"))
                middlePoint += 10;

            if (startPoints > 0 && endPoints > 0)
                return startPoints + endPoints + middlePoint;

            return 0;
        }
    }

    public class CentralMeridianParameterSelector : NamedParameterSelector
    {
        public override int Score(NamedParameterSelector.ParameterData parameterData) {
            var parameterName = parameterData.NormalizedName;
            if ("CENTRALMERIDIAN".Equals(parameterName))
                return 100;

            int startPoints;
            if (parameterName.StartsWith("CENTRAL"))
                startPoints = 10;
            else
                return 0;

            int endPoints;
            if (parameterName.EndsWith("MERIDIAN") || parameterName.EndsWith("MERIDIAN"))
                endPoints = 10;
            else
                return 0;

            if (startPoints > 0 && endPoints > 0)
                return startPoints + endPoints;

            return 0;
        }
    }

    public class ScaleFactorParameterSelector : NamedParameterSelector
    {
        public override int Score(NamedParameterSelector.ParameterData parameterData) {
            var parameterName = parameterData.NormalizedName;
            if ("SCALEFACTOR".Equals(parameterName))
                return 100;
            if ("SCALE".Equals(parameterName))
                return 75;
            if (parameterName.StartsWith("SCALE") || parameterName.EndsWith("SCALE"))
                return 1;
            return 0;
        }
    }

    public class FalseEastingParameterSelector : NamedParameterSelector
    {
        public override int Score(ParameterData parameterData) {
            var parameterName = parameterData.NormalizedName;
            if ("FALSEEASTING".Equals(parameterName))
                return 100;
            if ("EASTINGOFFSET".Equals(parameterName))
                return 75;

            int score = 0;
            if (parameterName.Contains("EASTING")) {
                score = 25;
                if (parameterName.Contains("OFFSET"))
                    score+=25;
                if (parameterName.Contains("FALSE"))
                    score+=25;
            }
            return score;
        }
    }

    public class FalseNorthingParameterSelector : NamedParameterSelector
    {
        public override int Score(ParameterData parameterData) {
            var parameterName = parameterData.NormalizedName;
            if ("FALSENORTHING".Equals(parameterName))
                return 100;
            if ("NORTHINGOFFSET".Equals(parameterName))
                return 75;

            int score = 0;
            if (parameterName.Contains("NORTHING")) {
                score = 25;
                if (parameterName.Contains("OFFSET"))
                    score+=25;
                if (parameterName.Contains("FALSE"))
                    score+=25;
            }
            return score;
        }
    }

    public class EastingAtCenterParameterSelector : NamedParameterSelector
    {

        public override int Score(NamedParameterSelector.ParameterData parameterData) {
            var parameterName = parameterData.NormalizedName;
            if ("EASTINGATPROJECTIONCENTER".Equals(parameterName))
                return 100;
            if ("EASTINGATFALSEORIGIN".Equals(parameterName))
                return 75;
            
            int score = 0;
            if (parameterName.Contains("EASTING")) {
                score = 25;
                if (parameterName.Contains("PROJECTIONCENTER"))
                    score+=25;
                if (parameterName.Contains("FALSEORIGIN"))
                    score+=25;
            }
            return score;
        }
    }

    public class NorthingAtCenterParameterSelector : NamedParameterSelector
    {

        public override int Score(NamedParameterSelector.ParameterData parameterData) {
            var parameterName = parameterData.NormalizedName;
            if ("NORTHINGATPROJECTIONCENTER".Equals(parameterName))
                return 100;
            if ("NORTHINGATFALSEORIGIN".Equals(parameterName))
                return 75;

            int score = 0;
            if (parameterName.Contains("NORTHING")) {
                score = 25;
                if (parameterName.Contains("PROJECTIONCENTER"))
                    score += 25;
                if (parameterName.Contains("FALSEORIGIN"))
                    score += 25;
            }
            return score;
        }
    }

}
