using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Pigeoid.Contracts;
using Pigeoid.Transformation;
using Vertesaur.Contracts;
using Vertesaur.Transformation;
using Pigeoid.Unit;

namespace Pigeoid.CoordinateOperationCompilation
{
    public class StaticCoordinateOperationCompiler : ICoordinateOperationCompiler
    {

        public interface IStepOperationCompiler
        {
            StepCompilationResult Compile(StepCompilationParameters stepParameters);
        }

        public sealed class StepCompilationParameters
        {

            public StepCompilationParameters(
                ICoordinateOperationInfo coordinateOperationInfo,
                IUnit inputUnit,
                ICrs relatedInputCrs,
                ICrs relatedOutputCrs
            ) {
                if (null == coordinateOperationInfo) throw new ArgumentNullException("coordinateOperationInfo");
                if (null == inputUnit) throw new ArgumentNullException("inputUnit");
                Contract.EndContractBlock();

                CoordinateOperationInfo = coordinateOperationInfo;
                InputUnit = inputUnit;
                RelatedInputCrs = relatedInputCrs;
                RelatedOutputCrs = relatedOutputCrs;
            }

            [ContractInvariantMethod]
            private void CodeContractInvariants() {
                Contract.Invariant(CoordinateOperationInfo != null);
                Contract.Invariant(InputUnit != null);
            }

            public ICoordinateOperationInfo CoordinateOperationInfo { get; private set; }

            public IUnit InputUnit { get; private set; }

            public ICrs RelatedInputCrs { get; private set; }

            public ICrs RelatedOutputCrs { get; private set; }

            public IUnit RelatedInputCrsUnit {
                get {
                    Contract.Ensures(Contract.Result<IUnit>() != null);
                    return ExtractUnit(RelatedInputCrs)
                        ?? ExtractUnit(RelatedOutputCrs)
                        ?? InputUnit;
                }
            }

            public IUnit RelatedOutputCrsUnit {
                get {
                    Contract.Ensures(Contract.Result<IUnit>() != null);
                    return ExtractUnit(RelatedOutputCrs)
                        ?? ExtractUnit(RelatedInputCrs)
                        ?? InputUnit;
                }
            }

            public ISpheroid<double> RelatedInputSpheroid {
                get { return ExtractSpheroid(RelatedInputCrs) ?? ExtractSpheroid(RelatedOutputCrs); }
            }

            public ISpheroid<double> RelatedOutputSpheroid {
                get { return ExtractSpheroid(RelatedOutputCrs) ?? ExtractSpheroid(RelatedInputCrs); }
            }

            public ISpheroid<double> ConvertRelatedInputSpheroidUnit(IUnit unit) {
                var spheroid = RelatedInputSpheroid;
                return null != spheroid && null != unit
                    ? ConvertSpheroidUnit(spheroid, unit)
                    : spheroid;
            }

            public ISpheroid<double> ConvertRelatedOutputSpheroidUnit(IUnit unit) {
                var spheroid = RelatedOutputSpheroid;
                return null != spheroid && null != unit
                    ? ConvertSpheroidUnit(spheroid, unit)
                    : spheroid;
            }

            private static ISpheroid<double> ConvertSpheroidUnit(ISpheroid<double> spheroid, IUnit toUnit) {
                Contract.Requires(spheroid != null);
                Contract.Ensures(Contract.Result<ISpheroid<double>>() != null);
                if (null == toUnit)
                    return spheroid;

                var spheroidInfo = spheroid as ISpheroidInfo;
                if (null == spheroidInfo)
                    return spheroid;

                var fromUnit = spheroidInfo.AxisUnit;
                if (null != fromUnit && !UnitEqualityComparer.Default.Equals(fromUnit, toUnit) && UnitEqualityComparer.Default.AreSameType(fromUnit, toUnit)) {
                    var conversion = SimpleUnitConversionGenerator.FindConversion(spheroidInfo.AxisUnit, toUnit);
                    if (null != conversion && !(conversion is UnitUnityConversion))
                        return new SpheroidLinearUnitConversionWrapper(spheroidInfo, conversion);
                }
                return spheroid;
            }

        }

        public sealed class StepCompilationResult
        {

            public StepCompilationResult(
                StepCompilationParameters parameters,
                IUnit outputUnit,
                ITransformation transformation
            ) {
                if (null == parameters) throw new ArgumentNullException("parameters");
                if (null == outputUnit) throw new ArgumentNullException("outputUnit");
                if (null == transformation) throw new ArgumentNullException("transformation");
                Contract.EndContractBlock();

                Parameters = parameters;
                Transformation = transformation;
                OutputUnit = outputUnit;
            }

            [ContractInvariantMethod]
            private void CodeContractInvariants() {
                Contract.Invariant(Parameters != null);
                Contract.Invariant(Transformation != null);
                Contract.Invariant(OutputUnit != null);
            }

            [Obsolete("This may be useless")]
            public StepCompilationParameters Parameters { get; private set; }
            public ITransformation Transformation { get; private set; }
            public IUnit OutputUnit { get; private set; }

        }

        public static IEnumerable<ITransformation> Linearize(IEnumerable<ITransformation> transformation) {
            Contract.Requires(transformation != null);
            Contract.Ensures(Contract.Result<IEnumerable<ITransformation>>() != null);
            return transformation.SelectMany(Linearize);
        }

        public static IEnumerable<ITransformation> Linearize(ITransformation transformation) {
            Contract.Requires(transformation != null);
            Contract.Ensures(Contract.Result<IEnumerable<ITransformation>>() != null);
            var concatTransformation = transformation as ConcatenatedTransformation;
            if (concatTransformation != null) {
                return concatTransformation.Transformations.SelectMany(Linearize);
            }
            return new[] { transformation };
        }

        public static IUnit ExtractUnit(ICrs crs) {
            var crsGeodetic = crs as ICrsGeodetic;
            if (null != crsGeodetic)
                return crsGeodetic.Unit;
            var crsVertical = crs as ICrsVertical;
            if (null != crsVertical)
                return crsVertical.Unit;
            var crsFitted = crs as ICrsFitted;
            if (null != crsFitted)
                return ExtractUnit(crsFitted.BaseCrs);
            var crsLocal = crs as ICrsLocal;
            if (null != crsLocal)
                return crsLocal.Unit;
            return null;
        }

        public static ISpheroidInfo ExtractSpheroid(ICrs crs) {
            var geodetic = crs as ICrsGeodetic;
            if (null == geodetic)
                return null;
            var datum = geodetic.Datum;
            if (null == datum)
                return null;
            return datum.Spheroid;
        }

        private readonly IStepOperationCompiler[] _stepCompilers;

        public StaticCoordinateOperationCompiler() {
            _stepCompilers = new IStepOperationCompiler[]{
                StaticProjectionStepCompiler.Default,
                StaticTransformationStepCompiler.Default
            };
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(_stepCompilers != null);
        }

        public static ITransformation CreateCoordinateUnitConversion(IUnit from, IUnit to) {
            if (from == null) throw new ArgumentNullException("from");
            if (to == null) throw new ArgumentNullException("to");
            Contract.EndContractBlock();
            // if (null == from || null == to)
            //    return null;
            if (!UnitEqualityComparer.Default.Equals(from, to) && UnitEqualityComparer.Default.AreSameType(from, to)) {
                var conv = SimpleUnitConversionGenerator.FindConversion(from, to);
                if (null != conv && !(conv is UnitUnityConversion)) {
                    if (UnitEqualityComparer.Default.NameNormalizedComparer.Equals("LENGTH", from.Type)) {
                        return new LinearElementTransformation(conv);
                    }
                    if (UnitEqualityComparer.Default.NameNormalizedComparer.Equals("ANGLE", from.Type)) {
                        return new AngularElementTransformation(conv);
                    }
                }
            }
            return null;
        }

        public ITransformation Compile(ICoordinateOperationCrsPathInfo operationPath) {
            if (operationPath == null) throw new ArgumentNullException("operationPath");
            Contract.EndContractBlock();

            var allOps = operationPath.CoordinateOperations.ToList();
            var allCrs = operationPath.CoordinateReferenceSystems.ToList();
            if (allCrs.Count == 0)
                throw new ArgumentException("operationPath contains no CRSs", "operationPath");

            var firstCrs = allCrs[0];
            var lastCrs = allCrs[allCrs.Count - 1];

            var stepResults = new StepCompilationResult[allOps.Count];
            var currentUnit = ExtractUnit(firstCrs);

            for (int operationIndex = 0; operationIndex < stepResults.Length; operationIndex++) {
                var stepResult = CompileStep(new StepCompilationParameters(
                    allOps[operationIndex],
                    currentUnit,
                    allCrs[operationIndex],
                    allCrs[operationIndex + 1]
                ));

                if (null == stepResult)
                    return null; // not supported

                stepResults[operationIndex] = stepResult;
                currentUnit = stepResult.OutputUnit;
            }

            // make sure that the output units are correct
            ITransformation outputUnitConversion = CreateCoordinateUnitConversion(currentUnit, ExtractUnit(lastCrs));

            var resultTransformations = stepResults.Select(x => x.Transformation).ToList();
            if (null != outputUnitConversion)
                resultTransformations.Add(outputUnitConversion);

            resultTransformations = Linearize(resultTransformations).ToList();

            if (resultTransformations.Count == 0)
                return null;
            if (resultTransformations.Count == 1)
                return resultTransformations[0];
            return new ConcatenatedTransformation(resultTransformations);
        }

        private StepCompilationResult CompileStep(StepCompilationParameters stepParams) {
            Contract.Requires(stepParams != null);

            var operations = ConcatenatedCoordinateOperationInfo.LinearizeOperations(stepParams.CoordinateOperationInfo).ToArray();
            if (operations.Length == 0)
                return null;

            var currentUnit = stepParams.InputUnit;
            var partResults = new StepCompilationResult[operations.Length];
            for (int operationIndex = 0; operationIndex < operations.Length; operationIndex++) {
                var partResult = CompilePart(new StepCompilationParameters(
                    operations[operationIndex],
                    currentUnit,
                    stepParams.RelatedInputCrs,
                    stepParams.RelatedOutputCrs
                ));

                if (null == partResult)
                    return null; // not supported

                partResults[operationIndex] = partResult;
                currentUnit = partResult.OutputUnit;
            }

            ITransformation transformation = partResults.Length == 1
                ? partResults[0].Transformation
                : new ConcatenatedTransformation(Array.ConvertAll(partResults, x => x.Transformation));

            return new StepCompilationResult(
                stepParams,
                currentUnit,
                transformation);
        }

        private StepCompilationResult CompilePart(StepCompilationParameters partParams) {
            Contract.Requires(partParams != null);
            for (int compilerIndex = 0; compilerIndex < _stepCompilers.Length; compilerIndex++) {
                var stepCompiler = _stepCompilers[compilerIndex];
                var compiledStep = stepCompiler.Compile(partParams);
                if (null != compiledStep)
                    return compiledStep;
            }
            return null;
        }
    }
}
