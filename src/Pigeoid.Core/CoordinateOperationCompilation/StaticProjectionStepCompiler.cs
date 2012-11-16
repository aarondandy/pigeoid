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
	public class StaticProjectionStepCompiler : StaticCoordinateOperationCompiler.IStepOperationCompiler
	{

		private class ProjectionCompilationParams
		{

			public ProjectionCompilationParams(
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

		private static readonly StaticProjectionStepCompiler DefaultValue = new StaticProjectionStepCompiler();
		public static StaticProjectionStepCompiler Default { get { return DefaultValue;  } }

		private static ISpheroidInfo ExtractSpheroid(ICrs crs) {
			var geodetic = crs as ICrsGeodetic;
			if (null == geodetic)
				return null;
			var datum = geodetic.Datum;
			if (null == datum)
				return null;
			return datum.Spheroid;
		}

		private static ISpheroidInfo ExtractSpheroid([NotNull] ProjectionCompilationParams projectionParams) {
			return ExtractSpheroid(projectionParams.StepParams.RelatedInputCrs) ?? ExtractSpheroid(projectionParams.StepParams.RelatedOutputCrs);
		}

		private static IUnit ExtractLinearUnit(ICrs crs) {
			var geocentric = crs as ICrsGeocentric;
			if (null != geocentric)
				return geocentric.Unit;
			var projected = crs as ICrsProjected;
			if (null != projected) {
				var projectedUnit = projected.Unit;
				if (null != projectedUnit && UnitNameNormalizedComparer.Default.Equals("LENGTH", projectedUnit.Type))
					return projectedUnit;
			}
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


		private static bool TryCrateVector2(INamedParameter xParam, INamedParameter yParam, out Vector2 result) {
			return TryCrateVector2(xParam, yParam, null, out result);
		}

		private static bool TryCrateVector2(INamedParameter xParam, INamedParameter yParam, IUnit linearUnit, out Vector2 result) {
			double x, y;
			if (NamedParameter.TryGetDouble(xParam, out x) && NamedParameter.TryGetDouble(yParam, out y)) {
				if (null != linearUnit) {
					ConvertIfVaild(xParam.Unit, linearUnit, ref x);
					ConvertIfVaild(yParam.Unit, linearUnit, ref y);
				}
				result = new Vector2(x, y);
				return true;
			}
			result = Vector2.Invalid;
			return false;
		}

		private static PopularVisualizationPseudoMercator CreatePopularVisualisationPseudoMercator(ProjectionCompilationParams opData) {
			var originLatParam = new KeywordNamedParameterSelector("LAT", "NATURALORIGIN");
			var originLonParam = new KeywordNamedParameterSelector("LON", "NATURALORIGIN");
			var offsetXParam = new KeywordNamedParameterSelector("FALSE", "OFFSET", "X", "EAST");
			var offsetYParam = new KeywordNamedParameterSelector("FALSE", "OFFSET", "Y", "NORTH");
			if (!opData.ParameterLookup.Assign(originLatParam, originLonParam, offsetXParam, offsetYParam))
				return null;

			var spheroid = ExtractSpheroid(opData);
			if (null == spheroid)
				return null;

			GeographicCoordinate origin;
			Vector2 offset;

			if (
				TryCreateGeographicCoordinate(originLatParam.Selection, originLonParam.Selection, out origin)
				&& TryCrateVector2(offsetXParam.Selection, offsetYParam.Selection, out offset)
			) {
				return new PopularVisualizationPseudoMercator(origin, offset, spheroid);
			}
			return null;
		}

		private ProjectionBase CreateLambertAzimuthalEqualArea(ProjectionCompilationParams opData) {
			var originLatParam = new KeywordNamedParameterSelector("LAT", "NATURALORIGIN");
			var originLonParam = new KeywordNamedParameterSelector("LON", "NATURALORIGIN");
			var offsetXParam = new KeywordNamedParameterSelector("FALSE", "OFFSET", "X", "EAST");
			var offsetYParam = new KeywordNamedParameterSelector("FALSE", "OFFSET", "Y", "NORTH");
			opData.ParameterLookup.Assign(originLatParam, originLonParam, offsetXParam, offsetYParam);

			var spheroid = ExtractSpheroid(opData);
			if (null == spheroid)
				return null;

			GeographicCoordinate origin;
			if (!originLatParam.IsSelected || !originLonParam.IsSelected || !TryCreateGeographicCoordinate(originLatParam.Selection, originLonParam.Selection, out origin))
				origin = GeographicCoordinate.Zero;

			Vector2 offset;
			if (!offsetXParam.IsSelected || !offsetYParam.IsSelected || !TryCrateVector2(offsetXParam.Selection, offsetYParam.Selection, out offset))
				offset = Vector2.Zero;

			var spherical = _coordinateOperationNameComparer.Normalize(opData.OperationName).EndsWith("SPHERICAL");

			return spherical
				? (ProjectionBase) new LambertAzimuthalEqualAreaSpherical(origin, offset, spheroid)
				: new LambertAzimuthalEqualArea(origin, offset, spheroid);
		}

		private EquidistantCylindrical CreateEquidistantCylindrical(ProjectionCompilationParams opData) {
			var originLatParam = new KeywordNamedParameterSelector("LAT", "PARALLEL");
			var originLonParam = new KeywordNamedParameterSelector("LON", "NATURALORIGIN");
			var offsetXParam = new KeywordNamedParameterSelector("FALSE", "OFFSET", "X", "EAST");
			var offsetYParam = new KeywordNamedParameterSelector("FALSE", "OFFSET", "Y", "NORTH");
			opData.ParameterLookup.Assign(originLatParam, originLonParam, offsetXParam, offsetYParam);

			var spheroid = ExtractSpheroid(opData);
			if (null == spheroid)
				return null;

			GeographicCoordinate origin;
			if (!originLatParam.IsSelected || !originLonParam.IsSelected || !TryCreateGeographicCoordinate(originLatParam.Selection, originLonParam.Selection, out origin))
				origin = GeographicCoordinate.Zero;

			Vector2 offset;
			if (!offsetXParam.IsSelected || !offsetYParam.IsSelected || !TryCrateVector2(offsetXParam.Selection, offsetYParam.Selection, out offset))
				offset = Vector2.Zero;

			return new EquidistantCylindrical(origin, offset, spheroid);
		}

		private readonly INameNormalizedComparer _coordinateOperationNameComparer;
		private readonly Dictionary<string, Func<ProjectionCompilationParams, ITransformation<GeographicCoordinate, Point2>>> _transformationCreatorLookup;

		public StaticProjectionStepCompiler(INameNormalizedComparer coordinateOperationNameComparer = null) {
			_coordinateOperationNameComparer = coordinateOperationNameComparer ?? CoordinateOperationNameNormalizedComparer.Default;
			_transformationCreatorLookup = new Dictionary<string, Func<ProjectionCompilationParams, ITransformation<GeographicCoordinate,Point2>>>(_coordinateOperationNameComparer) {
				{CoordinateOperationStandardNames.AlbersEqualAreaConic,null},
				{CoordinateOperationStandardNames.AzimuthalEquidistant,null},
				{CoordinateOperationStandardNames.CassiniSoldner, null}, // CreateCassiniSoldner
				{CoordinateOperationStandardNames.CylindricalEqualArea,null},
				{CoordinateOperationStandardNames.Eckert4,null},
				{CoordinateOperationStandardNames.Eckert6,null},
				{CoordinateOperationStandardNames.EquidistantConic,null},
				{CoordinateOperationStandardNames.EquidistantCylindrical,CreateEquidistantCylindrical},
				{CoordinateOperationStandardNames.Equirectangular,null},
				{CoordinateOperationStandardNames.GallStereographic,null},
				{CoordinateOperationStandardNames.Geos,null},
				{CoordinateOperationStandardNames.Gnomonic,null},
				{CoordinateOperationStandardNames.HotineObliqueMercator,null},
				{CoordinateOperationStandardNames.KrovakObliqueConicConformal,null},
				{CoordinateOperationStandardNames.LabordeObliqueMercator,null}, // CreateLabordeObliqueMercator
				{CoordinateOperationStandardNames.LambertAzimuthalEqualArea,CreateLambertAzimuthalEqualArea},
				{CoordinateOperationStandardNames.LambertAzimuthalEqualAreaSpherical,CreateLambertAzimuthalEqualArea}, // CreateLambertAzimuthalEqualArea
				{CoordinateOperationStandardNames.LambertConicConformal1Sp,null},
				{CoordinateOperationStandardNames.LambertConicConformal2Sp,null},
				{CoordinateOperationStandardNames.Mercator1Sp,null}, // CreateMercator
				{CoordinateOperationStandardNames.Mercator2Sp,null}, // CreateMercator
				{CoordinateOperationStandardNames.MillerCylindrical,null},
				{CoordinateOperationStandardNames.Mollweide,null},
				{CoordinateOperationStandardNames.NewZealandMapGrid,null},
				{CoordinateOperationStandardNames.ObliqueMercator,null},
				{CoordinateOperationStandardNames.ObliqueStereographic,null},
				{CoordinateOperationStandardNames.Orthographic,null},
				{CoordinateOperationStandardNames.PolarStereographic,null},
				{CoordinateOperationStandardNames.Polyconic,null},
				{CoordinateOperationStandardNames.PopularVisualisationPseudoMercator, CreatePopularVisualisationPseudoMercator},
				{CoordinateOperationStandardNames.Robinson,null},
				{CoordinateOperationStandardNames.RosenmundObliqueMercator,null},
				{CoordinateOperationStandardNames.Sinusoidal,null},
				{CoordinateOperationStandardNames.SwissObliqueCylindrical,null},
				{CoordinateOperationStandardNames.Stereographic,null},
				{CoordinateOperationStandardNames.TransverseMercator,null}, // CreateTransverseMercator
				{CoordinateOperationStandardNames.TransverseMercatorSouthOriented,null},
				{CoordinateOperationStandardNames.TunisiaMiningGrid,null},
				{CoordinateOperationStandardNames.VanDerGrinten,null}
			};
		}

		public StaticCoordinateOperationCompiler.StepCompilationResult Compile([NotNull] StaticCoordinateOperationCompiler.StepCompilationParameters stepParameters){
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
				var desiredInputUnits = OgcAngularUnit.DefaultRadians;
				if (null != desiredInputUnits && !UnitEqualityComparer.Default.Equals(actualInputUnits, desiredInputUnits)) {
					var conv = SimpleUnitConversionGenerator.FindConversion(actualInputUnits, desiredInputUnits);
					if (null != conv) {
						var conversionTransformation = new AngularElementTransformation(conv);
						resultTransformation = new ConcatenatedTransformation(new[] { conversionTransformation, resultTransformation });
					}
				}
			}

			var outputUnits = ExtractLinearUnit(stepParameters.RelatedOutputCrs)
					?? ExtractLinearUnit(stepParameters.RelatedInputCrs);

			// the result units will be radians
			return new StaticCoordinateOperationCompiler.StepCompilationResult(
				stepParameters,
				outputUnits,
				resultTransformation
			);
		}

		private StaticCoordinateOperationCompiler.StepCompilationResult CompileInverse([NotNull] StaticCoordinateOperationCompiler.StepCompilationParameters stepParameters) {
			if (!stepParameters.CoordinateOperationInfo.IsInverseOfDefinition || !stepParameters.CoordinateOperationInfo.HasInverse)
				return null;

			var inverseOperationInfo = stepParameters.CoordinateOperationInfo.GetInverse();
			if (null == inverseOperationInfo)
				return null;

			var forwardCompiledOperation = CompileForwardToTransform(new StaticCoordinateOperationCompiler.StepCompilationParameters(
				inverseOperationInfo,
				OgcAngularUnit.DefaultRadians,
				stepParameters.RelatedOutputCrs,
				stepParameters.RelatedInputCrs
			));

			if (null == forwardCompiledOperation || !forwardCompiledOperation.HasInverse)
				return null;

			var inverseCompiledOperation = forwardCompiledOperation.GetInverse();
			ITransformation resultTransformation = inverseCompiledOperation;

			// make sure that the input units are correct
			var actualInputUnits = stepParameters.InputUnit;
			if (null != actualInputUnits){
				var desiredInputUnits = ExtractLinearUnit(stepParameters.RelatedInputCrs)
					?? ExtractLinearUnit(stepParameters.RelatedOutputCrs);
				if(null != desiredInputUnits && !UnitEqualityComparer.Default.Equals(actualInputUnits, desiredInputUnits)){
					var conv = SimpleUnitConversionGenerator.FindConversion(actualInputUnits, desiredInputUnits);
					if(null != conv){
						var conversionTransformation = new LinearElementTransformation(conv);
						resultTransformation = new ConcatenatedTransformation(new[]{conversionTransformation, resultTransformation});
					}
				}
			}

			// the result units will be radians
			return new StaticCoordinateOperationCompiler.StepCompilationResult(
				stepParameters,
				OgcAngularUnit.DefaultRadians,
				resultTransformation
			);
		}

		private ITransformation<GeographicCoordinate, Point2> CompileForwardToTransform([NotNull] StaticCoordinateOperationCompiler.StepCompilationParameters stepParameters) {
			string operationName = null;
			IEnumerable<INamedParameter> parameters = null;

			var parameterizedOperation = stepParameters.CoordinateOperationInfo as IParameterizedCoordinateOperationInfo;
			if(null != parameterizedOperation){
				if (null != parameterizedOperation.Method)
					operationName = parameterizedOperation.Method.Name;
				parameters = parameterizedOperation.Parameters;
			}

			if (null == operationName)
				operationName = stepParameters.CoordinateOperationInfo.Name;

			Func<ProjectionCompilationParams, ITransformation<GeographicCoordinate, Point2>> projectionCompiler;
			if (null == operationName || !_transformationCreatorLookup.TryGetValue(operationName, out projectionCompiler) || null == projectionCompiler)
				return null;

			var parameterLookup = new NamedParameterLookup(parameters ?? Enumerable.Empty<INamedParameter>());
			var projectionCompilationParams = new ProjectionCompilationParams(stepParameters, parameterLookup, operationName);

			return projectionCompiler(projectionCompilationParams);
		}

		

	}
}
