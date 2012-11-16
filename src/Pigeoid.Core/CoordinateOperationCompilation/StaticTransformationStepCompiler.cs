using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Pigeoid.Contracts;
using Pigeoid.Interop;
using Pigeoid.Ogc;
using Pigeoid.Transformation;
using Pigeoid.Unit;
using Vertesaur;
using Vertesaur.Contracts;
using Vertesaur.Transformation;

namespace Pigeoid.CoordinateOperationCompilation
{
	class StaticTransformationStepCompiler : StaticCoordinateOperationCompiler.IStepOperationCompiler
	{

		private class TransformationCompilationParams
		{

			public TransformationCompilationParams(
				[NotNull] StaticCoordinateOperationCompiler.StepCompilationParameters stepParams,
				[NotNull] NamedParameterLookup parameterLookup,
				string operationName)
			{
				StepParams = stepParams;
				ParameterLookup = parameterLookup;
				OperationName = operationName;
			}

			public StaticCoordinateOperationCompiler.StepCompilationParameters StepParams;
			public NamedParameterLookup ParameterLookup;
			public string OperationName;

		}

		private static readonly StaticTransformationStepCompiler DefaultValue = new StaticTransformationStepCompiler();
		public static StaticTransformationStepCompiler Default { get { return DefaultValue; } }

		private static IUnit ExtractUnit(ICrs crs) {
			var geodetic = crs as ICrsGeodetic;
			if (null != geodetic)
				return geodetic.Unit;
			return null;
		}

		private static bool ConvertIfVaild(IUnit from, IUnit to, ref double value) {
			if (null == from || null == to)
				return false;
			var conv = SimpleUnitConversionGenerator.FindConversion(from, to);
			if (null == conv)
				return false;

			value = conv.TransformValue(value);
			return true;
		}

		private static bool TryCrateVector3(INamedParameter xParam, INamedParameter yParam, INamedParameter zParam, out Vector3 result) {
			return TryCrateVector2(xParam, yParam, zParam, null, out result);
		}

		private static bool TryCrateVector2(INamedParameter xParam, INamedParameter yParam, INamedParameter zParam, IUnit linearUnit, out Vector3 result) {
			double x, y, z;
			if (NamedParameter.TryGetDouble(xParam, out x) && NamedParameter.TryGetDouble(yParam, out y) && NamedParameter.TryGetDouble(yParam, out z)) {
				if (null != linearUnit) {
					ConvertIfVaild(xParam.Unit, linearUnit, ref x);
					ConvertIfVaild(yParam.Unit, linearUnit, ref y);
					ConvertIfVaild(zParam.Unit, linearUnit, ref z);
				}
				result = new Vector3(x, y, z);
				return true;
			}
			result = Vector3.Invalid;
			return false;
		}

		private static ISpheroidInfo ExtractSpheroid(ICrs crs) {
			var geodetic = crs as ICrsGeodetic;
			if (null == geodetic)
				return null;
			var datum = geodetic.Datum;
			if (null == datum)
				return null;
			return datum.Spheroid;
		}

		private static ITransformation CreatePositionVectorTransformation(TransformationCompilationParams opData) {
			var xTransParam = new KeywordNamedParameterSelector("XAXIS", "TRANS");
			var yTransParam = new KeywordNamedParameterSelector("YAXIS", "TRANS");
			var zTransParam = new KeywordNamedParameterSelector("ZAXIS", "TRANS");
			var xRotParam = new KeywordNamedParameterSelector("XAXIS", "ROT");
			var yRotParam = new KeywordNamedParameterSelector("YAXIS", "ROT");
			var zRotParam = new KeywordNamedParameterSelector("ZAXIS", "ROT");
			var scaleParam = new KeywordNamedParameterSelector("SCALE");

			if (!opData.ParameterLookup.Assign(xTransParam, yTransParam, zTransParam, xRotParam, yRotParam, zRotParam, scaleParam))
				return null;

			Vector3 translation, rotation;
			double scale;

			if (!TryCrateVector3(xTransParam.Selection, yTransParam.Selection, zTransParam.Selection, out translation))
				return null;
			if (!TryCrateVector3(xRotParam.Selection, yRotParam.Selection, zRotParam.Selection, out rotation))
				return null;
			if (!NamedParameter.TryGetDouble(scaleParam.Selection, out scale))
				return null;

			ConvertIfVaild(scaleParam.Selection.Unit, ScaleUnitPartsPerMillion.Value, ref scale);

			var helmert = new Helmert7Transformation(translation, rotation, scale);

			if (opData.StepParams.RelatedInputCrs is ICrsGeocentric && opData.StepParams.RelatedOutputCrs is ICrsGeocentric)
				return helmert;

			var spheroidFrom = ExtractSpheroid(opData.StepParams.RelatedInputCrs) ?? ExtractSpheroid(opData.StepParams.RelatedOutputCrs);
			if (null == spheroidFrom)
				return null;

			var spheroidTo = ExtractSpheroid(opData.StepParams.RelatedOutputCrs) ?? spheroidFrom;

			return new Helmert7GeographicTransformation(spheroidFrom, helmert, spheroidTo);
		}


		private readonly INameNormalizedComparer _coordinateOperationNameComparer;

		public StaticTransformationStepCompiler(INameNormalizedComparer coordinateOperationNameComparer = null){
			_coordinateOperationNameComparer = coordinateOperationNameComparer ?? CoordinateOperationNameNormalizedComparer.Default;
		}

		public StaticCoordinateOperationCompiler.StepCompilationResult Compile(StaticCoordinateOperationCompiler.StepCompilationParameters stepParameters) {
			return CompileInverse(stepParameters)
				?? CompileForwards(stepParameters);
		}

		private StaticCoordinateOperationCompiler.StepCompilationResult CompileForwards([NotNull] StaticCoordinateOperationCompiler.StepCompilationParameters stepParameters){
			var forwardCompiledOperation = CompileForwardToTransform(stepParameters);
			if (null == forwardCompiledOperation)
				return null;

			ITransformation resultTransformation = forwardCompiledOperation;

			// make sure that the input units are correct
			var actualInputUnits = stepParameters.InputUnit;
			if (null != actualInputUnits) {
				var desiredInputUnits = stepParameters.InputUnit;
				if (null != desiredInputUnits && !UnitEqualityComparer.Default.Equals(actualInputUnits, desiredInputUnits)) {
					var conv = SimpleUnitConversionGenerator.FindConversion(actualInputUnits, desiredInputUnits);
					if (null != conv){
						ITransformation conversionTransformation = null;
						if (UnitEqualityComparer.Default.NameNormalizedComparer.Equals("LENGTH", actualInputUnits.Type)) {
							conversionTransformation = new LinearElementTransformation(conv);
						}
						else if (UnitEqualityComparer.Default.NameNormalizedComparer.Equals("ANGLE", actualInputUnits.Type)) {
							conversionTransformation = new AngularElementTransformation(conv);
						}
						if (null != conversionTransformation){
							resultTransformation = new ConcatenatedTransformation(new[]{conversionTransformation, resultTransformation});
						}
					}
				}
			}

			var outputUnits = ExtractUnit(stepParameters.RelatedOutputCrs)
					?? ExtractUnit(stepParameters.RelatedInputCrs);

			return new StaticCoordinateOperationCompiler.StepCompilationResult(
				stepParameters,
				outputUnits,
				resultTransformation
			);
		}

		private StaticCoordinateOperationCompiler.StepCompilationResult CompileInverse([NotNull] StaticCoordinateOperationCompiler.StepCompilationParameters stepParameters){
			if (!stepParameters.CoordinateOperationInfo.IsInverseOfDefinition || !stepParameters.CoordinateOperationInfo.HasInverse)
				return null;

			var inverseOperationInfo = stepParameters.CoordinateOperationInfo.GetInverse();
			if (null == inverseOperationInfo)
				return null;

			var expectedOutputUnits = ExtractUnit(stepParameters.RelatedOutputCrs)
				?? ExtractUnit(stepParameters.RelatedInputCrs);

			var forwardCompiledOperation = CompileForwardToTransform(new StaticCoordinateOperationCompiler.StepCompilationParameters(
				inverseOperationInfo,
				expectedOutputUnits,
				stepParameters.RelatedOutputCrs,
				stepParameters.RelatedInputCrs
			));

			if (null == forwardCompiledOperation || !forwardCompiledOperation.HasInverse)
				return null;

			var inverseCompiledOperation = forwardCompiledOperation.GetInverse();
			ITransformation resultTransformation = inverseCompiledOperation;

			// make sure that the input units are correct
			var actualInputUnits = stepParameters.InputUnit;
			if (null != actualInputUnits) {
				var desiredInputUnits = ExtractUnit(stepParameters.RelatedInputCrs)
					?? ExtractUnit(stepParameters.RelatedOutputCrs);
				if (null != desiredInputUnits && !UnitEqualityComparer.Default.Equals(actualInputUnits, desiredInputUnits)) {
					var conv = SimpleUnitConversionGenerator.FindConversion(actualInputUnits, desiredInputUnits);
					if (null != conv) {
						ITransformation conversionTransformation = null;
						if (UnitEqualityComparer.Default.NameNormalizedComparer.Equals("LENGTH", actualInputUnits.Type)) {
							conversionTransformation = new LinearElementTransformation(conv);
						}
						else if (UnitEqualityComparer.Default.NameNormalizedComparer.Equals("ANGLE", actualInputUnits.Type)) {
							conversionTransformation = new AngularElementTransformation(conv);
						}
						if (null != conversionTransformation) {
							resultTransformation = new ConcatenatedTransformation(new[] { conversionTransformation, resultTransformation });
						}
					}
				}
			}

			return new StaticCoordinateOperationCompiler.StepCompilationResult(
				stepParameters,
				expectedOutputUnits,
				resultTransformation
			);
		}

		private ITransformation CompileForwardToTransform([NotNull] StaticCoordinateOperationCompiler.StepCompilationParameters stepParameters){
			string operationName = null;
			IEnumerable<INamedParameter> parameters = null;

			var parameterizedOperation = stepParameters.CoordinateOperationInfo as IParameterizedCoordinateOperationInfo;
			if (null != parameterizedOperation) {
				if (null != parameterizedOperation.Method)
					operationName = parameterizedOperation.Method.Name;
				parameters = parameterizedOperation.Parameters;
			}

			if (null == operationName)
				operationName = stepParameters.CoordinateOperationInfo.Name;

			if (null == operationName)
				return null;

			var parameterLookup = new NamedParameterLookup(parameters ?? Enumerable.Empty<INamedParameter>());
			var compilationParams = new TransformationCompilationParams(stepParameters, parameterLookup, operationName);

			var normalizedName = _coordinateOperationNameComparer.Normalize(compilationParams.OperationName);

			if (normalizedName.StartsWith("POSITIONVECTORTRANSFORMATION"))
				return CreatePositionVectorTransformation(compilationParams);


			return null;
		}

	}
}
