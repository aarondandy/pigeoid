using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Pigeoid.Contracts;
using Pigeoid.Interop;
using Pigeoid.Ogc;
using Pigeoid.Projection;
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

		private static bool ConvertIfVaild(IUnit from, IUnit to, ref double value) {
			if (null == from || null == to)
				return false;
			var conv = SimpleUnitConversionGenerator.FindConversion(from, to);
			if (null == conv)
				return false;

			value = conv.TransformValue(value);
			return true;
		}

		private static bool TryGetDouble(INamedParameter parameter, IUnit unit, out double value) {
			if (!NamedParameter.TryGetDouble(parameter, out value))
				return false;

			ConvertIfVaild(parameter.Unit, unit, ref value);
			return true;
		}

		private static bool TryCrateVector3(INamedParameter xParam, INamedParameter yParam, INamedParameter zParam, out Vector3 result) {
			return TryCrateVector3(xParam, yParam, zParam, null, out result);
		}

		private static bool TryCrateVector3(INamedParameter xParam, INamedParameter yParam, INamedParameter zParam, IUnit linearUnit, out Vector3 result) {
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

		private static bool TryCreateGeographicCoordinate(INamedParameter latParam, INamedParameter lonParam, out GeographicCoordinate result) {
			double lat, lon;
			if (NamedParameter.TryGetDouble(latParam, out lat) && NamedParameter.TryGetDouble(lonParam, out lon)) {
				ConvertIfVaild(latParam.Unit, OgcAngularUnit.DefaultRadians, ref lat);
				ConvertIfVaild(lonParam.Unit, OgcAngularUnit.DefaultRadians, ref lon);
				result = new GeographicCoordinate(lat, lon);
				return true;
			}
			result = default(GeographicCoordinate);
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

		private static bool ExtractHelmertParams(TransformationCompilationParams opData, out Vector3 translation, out Vector3 rotation, out double scale){
			var xTransParam = new KeywordNamedParameterSelector("XAXIS", "TRANS");
			var yTransParam = new KeywordNamedParameterSelector("YAXIS", "TRANS");
			var zTransParam = new KeywordNamedParameterSelector("ZAXIS", "TRANS");
			var xRotParam = new KeywordNamedParameterSelector("XAXIS", "ROT");
			var yRotParam = new KeywordNamedParameterSelector("YAXIS", "ROT");
			var zRotParam = new KeywordNamedParameterSelector("ZAXIS", "ROT");
			var scaleParam = new KeywordNamedParameterSelector("SCALE");

			translation = Vector3.Invalid;
			rotation = Vector3.Invalid;
			scale = Double.NaN;

			if(!opData.ParameterLookup.Assign(xTransParam, yTransParam, zTransParam, xRotParam, yRotParam, zRotParam, scaleParam))
				return false;
			if(!TryCrateVector3(xTransParam.Selection, yTransParam.Selection, zTransParam.Selection, out translation))
				return false;
			if(!TryCrateVector3(xRotParam.Selection, yRotParam.Selection, zRotParam.Selection, OgcAngularUnit.DefaultArcSeconds, out rotation))
				return false;
			if(!NamedParameter.TryGetDouble(scaleParam.Selection, out scale))
				return false;

			ConvertIfVaild(scaleParam.Selection.Unit, ScaleUnitPartsPerMillion.Value, ref scale);
			return true;
		}

		private static StaticCoordinateOperationCompiler.StepCompilationResult CreateHelmertStep(TransformationCompilationParams opData, Helmert7Transformation helmert) {
			if (opData.StepParams.RelatedInputCrs is ICrsGeocentric && opData.StepParams.RelatedOutputCrs is ICrsGeocentric)
				return new StaticCoordinateOperationCompiler.StepCompilationResult(
					opData.StepParams,
					opData.StepParams.RelatedOutputCrsUnit ?? opData.StepParams.RelatedInputCrsUnit ?? opData.StepParams.InputUnit,
					helmert);

			var spheroidFrom = ExtractSpheroid(opData.StepParams.RelatedInputCrs) ?? ExtractSpheroid(opData.StepParams.RelatedOutputCrs);
			if (null == spheroidFrom)
				return null;

			var spheroidTo = ExtractSpheroid(opData.StepParams.RelatedOutputCrs) ?? spheroidFrom;

			ITransformation transformation = new Helmert7GeographicTransformation(spheroidFrom, helmert, spheroidTo);
			var unitConversion = StaticCoordinateOperationCompiler.CreateCoordinateUnitConversion(opData.StepParams.InputUnit, OgcAngularUnit.DefaultRadians);
			if (null != unitConversion)
				transformation = new ConcatenatedTransformation(new[] { unitConversion, transformation });

			return new StaticCoordinateOperationCompiler.StepCompilationResult(
				opData.StepParams,
				OgcAngularUnit.DefaultRadians,
				transformation);
		}

		private static StaticCoordinateOperationCompiler.StepCompilationResult CreatePositionVectorTransformation(TransformationCompilationParams opData) {
			Vector3 translation, rotation;
			double scale;
			if (!ExtractHelmertParams(opData, out translation, out rotation, out scale))
				return null;

			return CreateHelmertStep(opData, Helmert7Transformation.CreatePositionVectorFormat(translation, rotation, scale));
		}

		private static StaticCoordinateOperationCompiler.StepCompilationResult CreateCoordinateFrameRotationTransformation(TransformationCompilationParams opData) {
			Vector3 translation, rotation;
			double scale;
			if (!ExtractHelmertParams(opData, out translation, out rotation, out scale))
				return null;

			return CreateHelmertStep(opData, Helmert7Transformation.CreateCoordinateFrameRotationFormat(translation, rotation, scale));
		}

		private static StaticCoordinateOperationCompiler.StepCompilationResult CreateGeographicOffset(TransformationCompilationParams opData) {
			var latParam = new KeywordNamedParameterSelector("LAT");
			var lonParam = new KeywordNamedParameterSelector("LON");
			opData.ParameterLookup.Assign(latParam, lonParam);

			ITransformation transformation = null;
			if (latParam.IsSelected && lonParam.IsSelected) {
				GeographicCoordinate offset;
				if (TryCreateGeographicCoordinate(latParam.Selection, lonParam.Selection, out offset))
					transformation = new GeographicOffset(offset);
			}
			else if (latParam.IsSelected) {
				double value;
				if(TryGetDouble(latParam.Selection, OgcAngularUnit.DefaultRadians, out value))
					transformation = new GeographicOffset(new GeographicCoordinate(value, 0));
			}
			else if (lonParam.IsSelected) {
				double value;
				if(TryGetDouble(lonParam.Selection, OgcAngularUnit.DefaultRadians, out value))
					transformation = new GeographicOffset(new GeographicCoordinate(0, value));
			}

			if(null == transformation)
				return null; // no parameters

			var unitConversion = StaticCoordinateOperationCompiler.CreateCoordinateUnitConversion(opData.StepParams.InputUnit, OgcAngularUnit.DefaultRadians);
			if (null != unitConversion)
				transformation = new ConcatenatedTransformation(new[] { unitConversion, transformation });

			return new StaticCoordinateOperationCompiler.StepCompilationResult(
				opData.StepParams,
				OgcAngularUnit.DefaultRadians,
				transformation);
		}

		private static StaticCoordinateOperationCompiler.StepCompilationResult CreateGeocentricTranslation(TransformationCompilationParams opData) {
			var xParam = new KeywordNamedParameterSelector("XAXIS", "X");
			var yParam = new KeywordNamedParameterSelector("YAXIS", "Y");
			var zParam = new KeywordNamedParameterSelector("ZAXIS", "Z");
			if (!opData.ParameterLookup.Assign(xParam, yParam, zParam))
				return null;

			Vector3 delta;
			if (!TryCrateVector3(xParam.Selection, yParam.Selection, zParam.Selection, out delta))
				return null;

			if (opData.StepParams.RelatedInputCrs is ICrsGeocentric && opData.StepParams.RelatedOutputCrs is ICrsGeocentric) {
				// assume the units are correct
				return new StaticCoordinateOperationCompiler.StepCompilationResult(
					opData.StepParams,
					opData.StepParams.RelatedOutputCrsUnit ?? opData.StepParams.RelatedInputCrsUnit ?? opData.StepParams.InputUnit,
					new GeocentricTranslation(delta));
			}

			var inputSpheroid = opData.StepParams.RelatedInputSpheroid;
			if (null == inputSpheroid)
				return null;
			var outputSpheroid = opData.StepParams.RelatedOutputSpheroid;
			if (null == outputSpheroid)
				return null;
			ITransformation transformation = new GeographicGeocentricTranslation(inputSpheroid, delta, outputSpheroid);
			var conv = StaticCoordinateOperationCompiler.CreateCoordinateUnitConversion(opData.StepParams.InputUnit, OgcAngularUnit.DefaultRadians);
			if (null != conv)
				transformation = new ConcatenatedTransformation(new[] {conv, transformation});
			return new StaticCoordinateOperationCompiler.StepCompilationResult(
				opData.StepParams,
				OgcAngularUnit.DefaultRadians,
				transformation);
		}

		private static StaticCoordinateOperationCompiler.StepCompilationResult CreateVerticalOffset(TransformationCompilationParams opData) {
			double offsetValue = 0;
			
			var offsetParam = new KeywordNamedParameterSelector("OFFSET", "VERTICAL");
			if (opData.ParameterLookup.Assign(offsetParam)) {
				if (!NamedParameter.TryGetDouble(offsetParam.Selection, out offsetValue))
					offsetValue = 0;
			}

			return new StaticCoordinateOperationCompiler.StepCompilationResult(
				opData.StepParams,
				opData.StepParams.InputUnit,
				new VerticalOffset(offsetValue));
		}

		private static bool TryGetCoefficientValue(INamedParameter parameter, out double value) {
			if (null != parameter && NamedParameter.TryGetDouble(parameter, out value)) {
				ConvertIfVaild(parameter.Unit, ScaleUnitUnity.Value, ref value);
				return true;
			}
			value = default(double);
			return false;
		}

		private static StaticCoordinateOperationCompiler.StepCompilationResult CreateMadridToEd50Polynomial(TransformationCompilationParams opData) {
			var a0Param = new FullMatchParameterSelector("A0");
			var a1Param = new FullMatchParameterSelector("A1");
			var a2Param = new FullMatchParameterSelector("A2");
			var a3Param = new FullMatchParameterSelector("A3");
			var b00Param = new FullMatchParameterSelector("B00");
			var b0Param = new FullMatchParameterSelector("B0");
			var b1Param = new FullMatchParameterSelector("B1");
			var b2Param = new FullMatchParameterSelector("B2");
			var b3Param = new FullMatchParameterSelector("B3");
			opData.ParameterLookup.Assign(a0Param, a1Param, a2Param, a3Param, b00Param, b0Param, b1Param, b2Param, b3Param);

			double a0, a1, a2, a3, b00, b0, b1, b2, b3;
			TryGetCoefficientValue(a0Param.Selection, out a0);
			TryGetCoefficientValue(a1Param.Selection, out a1);
			TryGetCoefficientValue(a2Param.Selection, out a2);
			TryGetCoefficientValue(a3Param.Selection, out a3);
			TryGetCoefficientValue(b00Param.Selection, out b00);
			TryGetCoefficientValue(b0Param.Selection, out b0);
			TryGetCoefficientValue(b1Param.Selection, out b1);
			TryGetCoefficientValue(b2Param.Selection, out b2);
			TryGetCoefficientValue(b3Param.Selection, out b3);

			ITransformation transformation = new MadridEd50Polynomial(a0, a1, a2, a3, b00, b0, b1, b2, b3);

			var conv = StaticCoordinateOperationCompiler.CreateCoordinateUnitConversion(opData.StepParams.InputUnit, OgcAngularUnit.DefaultDegrees);
			if (null != conv)
				transformation = new ConcatenatedTransformation(new[] { conv, transformation });

			return new StaticCoordinateOperationCompiler.StepCompilationResult(
				opData.StepParams,
				OgcAngularUnit.DefaultDegrees,
				transformation);
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
			var forwardCompiledStep = CompileForwardToTransform(stepParameters);
			if (null == forwardCompiledStep)
				return null;

			return new StaticCoordinateOperationCompiler.StepCompilationResult(
				stepParameters,
				forwardCompiledStep.OutputUnit,
				forwardCompiledStep.Transformation
			);
		}

		private StaticCoordinateOperationCompiler.StepCompilationResult CompileInverse([NotNull] StaticCoordinateOperationCompiler.StepCompilationParameters stepParameters){
			if (!stepParameters.CoordinateOperationInfo.IsInverseOfDefinition || !stepParameters.CoordinateOperationInfo.HasInverse)
				return null;

			var inverseOperationInfo = stepParameters.CoordinateOperationInfo.GetInverse();
			if (null == inverseOperationInfo)
				return null;

			var expectedOutputUnits = stepParameters.RelatedOutputCrsUnit
				?? stepParameters.RelatedInputCrsUnit
				?? stepParameters.InputUnit;

			var forwardCompiledStep = CompileForwardToTransform(new StaticCoordinateOperationCompiler.StepCompilationParameters(
				inverseOperationInfo,
				expectedOutputUnits,
				stepParameters.RelatedOutputCrs,
				stepParameters.RelatedInputCrs
			));

			var forwardCompiledOperation = forwardCompiledStep.Transformation;
			if (!forwardCompiledOperation.HasInverse)
				return null;

			var inverseCompiledOperation = forwardCompiledOperation.GetInverse();
			var resultTransformation = inverseCompiledOperation;

			// make sure that the input units are correct
			var unitConversion = StaticCoordinateOperationCompiler.CreateCoordinateUnitConversion(stepParameters.InputUnit, forwardCompiledStep.OutputUnit);
			if(null != unitConversion)
				resultTransformation = new ConcatenatedTransformation(new[]{unitConversion, resultTransformation});

			return new StaticCoordinateOperationCompiler.StepCompilationResult(
				stepParameters,
				expectedOutputUnits,
				resultTransformation
			);
		}

		private StaticCoordinateOperationCompiler.StepCompilationResult CompileForwardToTransform([NotNull] StaticCoordinateOperationCompiler.StepCompilationParameters stepParameters) {
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
			if (normalizedName.StartsWith("COORDINATEFRAMEROTATION"))
				return CreateCoordinateFrameRotationTransformation(compilationParams);
			if (normalizedName.Equals("GEOGRAPHICOFFSET"))
				return CreateGeographicOffset(compilationParams);
			if (normalizedName.StartsWith("GEOCENTRICTRANSLATION"))
				return CreateGeocentricTranslation(compilationParams);
			if (normalizedName.Equals("VERTICALOFFSET"))
				return CreateVerticalOffset(compilationParams);
			if (normalizedName.Equals("MADRIDTOED50POLYNOMIAL"))
				return CreateMadridToEd50Polynomial(compilationParams);
			return null;
		}

	}
}
