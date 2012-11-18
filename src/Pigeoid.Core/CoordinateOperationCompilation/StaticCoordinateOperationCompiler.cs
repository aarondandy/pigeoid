using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
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
				[NotNull] ICoordinateOperationInfo coordinateOperationInfo,
				[NotNull] IUnit inputUnit,
				ICrs relatedInputCrs,
				ICrs relatedOutputCrs
			){
				if (null == coordinateOperationInfo)
					throw new ArgumentNullException("coordinateOperationInfo");
				if(null == inputUnit)
					throw new ArgumentNullException("inputUnit");

				CoordinateOperationInfo = coordinateOperationInfo;
				InputUnit = inputUnit;
				RelatedInputCrs = relatedInputCrs;
				RelatedOutputCrs = relatedOutputCrs;
			}

			[NotNull] public ICoordinateOperationInfo CoordinateOperationInfo { get; private set; }

			[NotNull] public IUnit InputUnit { get; private set; }

			public ICrs RelatedInputCrs { get; private set; }

			public ICrs RelatedOutputCrs { get; private set; }

			[CanBeNull] public IUnit RelatedInputCrsUnit {
				get { return ExtractUnit(RelatedInputCrs) ?? ExtractUnit(RelatedOutputCrs) ?? InputUnit; }
			}

			[CanBeNull] public IUnit RelatedOutputCrsUnit {
				get { return ExtractUnit(RelatedOutputCrs) ?? ExtractUnit(RelatedInputCrs) ?? InputUnit; }
			}

			[CanBeNull] public ISpheroid<double> RelatedInputSpheroid {
				get { return ExtractSpheroid(RelatedInputCrs) ?? ExtractSpheroid(RelatedOutputCrs); }
			}

			[CanBeNull] public ISpheroid<double> RelatedOutputSpheroid {
				get { return ExtractSpheroid(RelatedOutputCrs) ?? ExtractSpheroid(RelatedInputCrs); }
			} 
		}

		public sealed class StepCompilationResult
		{

			public StepCompilationResult(
				[NotNull] StepCompilationParameters parameters,
				[NotNull] IUnit outputUnit,
				[NotNull] ITransformation transformation
			){
				if(null == parameters)
					throw new ArgumentNullException("parameters");
				if(null == outputUnit)
					throw new ArgumentNullException("outputUnit");

				Parameters = parameters;
				Transformation = transformation;
				OutputUnit = outputUnit;
			}

			[Obsolete("This may be useless"), NotNull] public StepCompilationParameters Parameters { get; private set; }
			[NotNull] public ITransformation Transformation { get; private set; }
			[NotNull] public IUnit OutputUnit { get; private set; }

		}

		public static IEnumerable<ITransformation> Linearize(IEnumerable<ITransformation> transformation) {
			return transformation.SelectMany(Linearize);
		}

		public static IEnumerable<ITransformation> Linearize(ITransformation transformation) {
			var concatTransformation = transformation as ConcatenatedTransformation;
			if (concatTransformation != null) {
				return concatTransformation.Transformations.SelectMany(Linearize);
			}
			return new[] {transformation};
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

		public StaticCoordinateOperationCompiler(){
			_stepCompilers = new IStepOperationCompiler[]{
				StaticProjectionStepCompiler.Default,
				StaticTransformationStepCompiler.Default
			};
		}

		[CanBeNull]
		[ContractAnnotation("to:null=>null;from:null=>null;")]
		public static ITransformation CreateCoordinateUnitConversion([CanBeNull] IUnit from, [CanBeNull] IUnit to) {
			if (null == from || null == to)
				return null;
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
			if (null == operationPath)
				throw new ArgumentNullException("operationPath");

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
					allCrs[operationIndex+1]
				));

				if (null == stepResult)
					return null; // not supported

				stepResults[operationIndex] = stepResult;
				currentUnit = stepResult.OutputUnit;
			}

			// make sure that the output units are correct
			ITransformation outputUnitConversion = CreateCoordinateUnitConversion(currentUnit, ExtractUnit(lastCrs));
			/*if (null != currentUnit) {
				var desiredOutputUnits = ExtractUnit(lastCrs);
				if (
					null != desiredOutputUnits
					&& !UnitEqualityComparer.Default.Equals(currentUnit, desiredOutputUnits)
					&& UnitEqualityComparer.Default.AreSameType(currentUnit, desiredOutputUnits)
				) {
					var conv = SimpleUnitConversionGenerator.FindConversion(currentUnit, desiredOutputUnits);
					if (null != conv) {
						if(UnitEqualityComparer.Default.NameNormalizedComparer.Equals("LENGTH", currentUnit.Type)){
							outputUnitConversion = new LinearElementTransformation(conv);
						}
						else if(UnitEqualityComparer.Default.NameNormalizedComparer.Equals("ANGLE", currentUnit.Type)){
							outputUnitConversion = new AngularElementTransformation(conv);
						}
					}
				}
			}*/

			var resultTransformations = stepResults.Select(x => x.Transformation).ToList();
			if(null != outputUnitConversion)
				resultTransformations.Add(outputUnitConversion);

			resultTransformations = Linearize(resultTransformations).ToList();

			if (resultTransformations.Count == 0)
				return null;
			if (resultTransformations.Count == 1)
				return resultTransformations[0];
			return new ConcatenatedTransformation(resultTransformations);
		}

		private StepCompilationResult CompileStep([NotNull] StepCompilationParameters stepParams){
			var operations = ConcatenatedCoordinateOperationInfo.LinearizeOperations(stepParams.CoordinateOperationInfo).ToArray();
			if (operations.Length == 0)
				return null;

			var currentUnit = stepParams.InputUnit;
			var partResults = new StepCompilationResult[operations.Length];
			for (int operationIndex = 0; operationIndex < operations.Length; operationIndex++){
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

		private StepCompilationResult CompilePart([NotNull] StepCompilationParameters partParams){
			for (int compilerIndex = 0; compilerIndex < _stepCompilers.Length; compilerIndex++){
				var stepCompiler = _stepCompilers[compilerIndex];
				var compiledStep = stepCompiler.Compile(partParams);
				if (null != compiledStep)
					return compiledStep;
			}
			return null;
		}
	}
}
