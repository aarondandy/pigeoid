using System;
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

			public ICoordinateOperationInfo CoordinateOperationInfo { [ContractAnnotation("=>notnull")] get; private set; }

			public IUnit InputUnit { [ContractAnnotation("=>notnull")] get; private set; }

			public ICrs RelatedInputCrs { get; private set; }

			public ICrs RelatedOutputCrs { get; private set; }

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

			[Obsolete("This may be useless")]
			public StepCompilationParameters Parameters { [ContractAnnotation("=>notnull")] get; private set; }

			public ITransformation Transformation { [ContractAnnotation("=>notnull")] get; private set; }

			public IUnit OutputUnit { [ContractAnnotation("=>notnull")] get; private set; }

		}

		public static IUnit ExtractUnit(ICrs crs) {
			var crsGeodetic = crs as ICrsGeodetic;
			return null != crsGeodetic ? crsGeodetic.Unit : null;
		}

		private readonly IStepOperationCompiler[] _stepCompilers;

		public StaticCoordinateOperationCompiler(){
			_stepCompilers = new IStepOperationCompiler[]{
				StaticProjectionStepCompiler.Default
			};
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

			// make sure that the input units are correct
			ITransformation outputUnitConversion = null;
			if (null != currentUnit) {
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
			}

			// TODO: maybe make sure operations are linearized so they can be better optimized?
			var resultTransformations = stepResults.Select(x => x.Transformation).ToList();
			if(null != outputUnitConversion)
				resultTransformations.Add(outputUnitConversion);

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
