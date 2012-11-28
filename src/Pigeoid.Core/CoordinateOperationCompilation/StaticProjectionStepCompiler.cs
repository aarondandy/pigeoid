using System;
using System.Collections.Generic;
using System.Linq;
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
			if (null != parameter && NamedParameter.TryGetDouble(parameter, out value)) {
				ConvertIfVaild(parameter.Unit, unit, ref value);
				return true;
			}
			value = Double.NaN;
			return false;
		}

		private static bool TryCreateGeographicCoordinate([CanBeNull] INamedParameter latParam, [CanBeNull] INamedParameter lonParam, out GeographicCoordinate result) {
			double lat, lon;
			if (TryGetDouble(latParam, OgcAngularUnit.DefaultRadians, out lat) | TryGetDouble(lonParam, OgcAngularUnit.DefaultRadians, out lon)) {
				result = new GeographicCoordinate(lat, lon);
				return true;
			}
			result = GeographicCoordinate.Invalid;
			return false;
		}

		private static bool TryCreateVector2(INamedParameter xParam, INamedParameter yParam, out Vector2 result) {
			return TryCreateVector2(xParam, yParam, null, out result);
		}

		private static bool TryCreateVector2(INamedParameter xParam, INamedParameter yParam, IUnit linearUnit, out Vector2 result) {
			double x, y;
			if (TryGetDouble(xParam, linearUnit, out x) | TryGetDouble(yParam, linearUnit, out y)) {
				result = new Vector2(x, y);
				return true;
			}
			result = Vector2.Invalid;
			return false;
		}

		private static bool TryCreatePoint2(INamedParameter xParam, INamedParameter yParam, out Point2 result) {
			return TryCreatePoint2(xParam, yParam, null, out result);
		}

		private static bool TryCreatePoint2(INamedParameter xParam, INamedParameter yParam, IUnit linearUnit, out Point2 result) {
			double x, y;
			if (TryGetDouble(xParam, linearUnit, out x) | TryGetDouble(yParam, linearUnit, out y)) {
				result = new Point2(x, y);
				return true;
			}
			result = Point2.Invalid;
			return false;
		}

		private ProjectionBase CreatePolarStereographic(ProjectionCompilationParams opData) {
			var originLatParam = new KeywordNamedParameterSelector("LAT", "ORIGIN");
			var parallelLatParam = new KeywordNamedParameterSelector("LAT", "PARALLEL");
			var originLonParam = new KeywordNamedParameterSelector("LON", "ORIGIN");
			var offsetXParam = new KeywordNamedParameterSelector("FALSE", "OFFSET", "X", "EAST");
			var offsetYParam = new KeywordNamedParameterSelector("FALSE", "OFFSET", "Y", "NORTH");
			var scaleFactorParam = new KeywordNamedParameterSelector("SCALE");
			opData.ParameterLookup.Assign(originLatParam, originLonParam, parallelLatParam, offsetXParam, offsetYParam, scaleFactorParam);

			var fromSpheroid = opData.StepParams.RelatedOutputCrsUnitConvertedSpheroid;
			if (null == fromSpheroid)
				return null;

			GeographicCoordinate origin;
			if (!(originLatParam.IsSelected || originLonParam.IsSelected) || !TryCreateGeographicCoordinate(originLatParam.Selection, originLonParam.Selection, out origin))
				origin = GeographicCoordinate.Zero;

			Vector2 offset;
			if (!(offsetXParam.IsSelected || offsetYParam.IsSelected) || !TryCreateVector2(offsetXParam.Selection, offsetYParam.Selection, opData.StepParams.RelatedOutputCrsUnit, out offset))
				offset = Vector2.Zero;

			double latParallel;
			if (!parallelLatParam.IsSelected || !TryGetDouble(parallelLatParam.Selection, OgcAngularUnit.DefaultRadians, out latParallel))
				latParallel = origin.Latitude;

			double scaleFactor;
			if (scaleFactorParam.IsSelected && TryGetDouble(scaleFactorParam.Selection, ScaleUnitUnity.Value, out scaleFactor)) {
				return new PolarStereographic(origin, scaleFactor, offset, fromSpheroid);
			}

			if (
				(offsetXParam.IsSelected && opData.ParameterLookup.ParameterNameNormalizedComparer.Normalize(offsetXParam.Selection.Name).Contains("ORIGIN"))
				|| (offsetYParam.IsSelected && opData.ParameterLookup.ParameterNameNormalizedComparer.Normalize(offsetYParam.Selection.Name).Contains("ORIGIN"))
			) {
				return PolarStereographic.CreateFromStandardParallelAndFalseOffsetAtOrigin(latParallel, origin.Longitude, offset, fromSpheroid);
			}

			return PolarStereographic.CreateFromStandardParallel(latParallel, origin.Longitude, offset, fromSpheroid);
		}

		private static ProjectionBase CreateTransverseMercator(ProjectionCompilationParams opData) {
			var originLatParam = new KeywordNamedParameterSelector("LAT", "ORIGIN");
			var originLonParam = new KeywordNamedParameterSelector("LON", "ORIGIN");
			var offsetXParam = new KeywordNamedParameterSelector("FALSE", "OFFSET", "X", "EAST");
			var offsetYParam = new KeywordNamedParameterSelector("FALSE", "OFFSET", "Y", "NORTH");
			var scaleFactorParam = new KeywordNamedParameterSelector("SCALE");
			opData.ParameterLookup.Assign(originLatParam, originLonParam, offsetXParam, offsetYParam, scaleFactorParam);

			var fromSpheroid = opData.StepParams.RelatedOutputCrsUnitConvertedSpheroid;
			if (null == fromSpheroid)
				return null;

			GeographicCoordinate origin;
			if (!(originLatParam.IsSelected || originLonParam.IsSelected) || !TryCreateGeographicCoordinate(originLatParam.Selection, originLonParam.Selection, out origin))
				origin = GeographicCoordinate.Zero;

			Vector2 offset;
			if (!(offsetXParam.IsSelected || offsetYParam.IsSelected) || !TryCreateVector2(offsetXParam.Selection, offsetYParam.Selection, opData.StepParams.RelatedOutputCrsUnit, out offset))
				offset = Vector2.Zero;

			double scaleFactor;
			if (scaleFactorParam.IsSelected && TryGetDouble(scaleFactorParam.Selection, ScaleUnitUnity.Value, out scaleFactor))
				return new TransverseMercator(origin, offset, scaleFactor, fromSpheroid);

			return null;
		}

		private static ProjectionBase CreateTransverseMercatorSouth(ProjectionCompilationParams opData) {
			var originLatParam = new KeywordNamedParameterSelector("LAT", "ORIGIN");
			var originLonParam = new KeywordNamedParameterSelector("LON", "ORIGIN");
			var offsetXParam = new KeywordNamedParameterSelector("FALSE", "OFFSET", "X", "EAST");
			var offsetYParam = new KeywordNamedParameterSelector("FALSE", "OFFSET", "Y", "NORTH");
			var scaleFactorParam = new KeywordNamedParameterSelector("SCALE");
			opData.ParameterLookup.Assign(originLatParam, originLonParam, offsetXParam, offsetYParam, scaleFactorParam);

			var fromSpheroid = opData.StepParams.RelatedOutputCrsUnitConvertedSpheroid;
			if (null == fromSpheroid)
				return null;

			GeographicCoordinate origin;
			if (!(originLatParam.IsSelected || originLonParam.IsSelected) || !TryCreateGeographicCoordinate(originLatParam.Selection, originLonParam.Selection, out origin))
				origin = GeographicCoordinate.Zero;

			Vector2 offset;
			if (!(offsetXParam.IsSelected || offsetYParam.IsSelected) || !TryCreateVector2(offsetXParam.Selection, offsetYParam.Selection, opData.StepParams.RelatedOutputCrsUnit, out offset))
				offset = Vector2.Zero;

			double scaleFactor;
			if (scaleFactorParam.IsSelected && TryGetDouble(scaleFactorParam.Selection, ScaleUnitUnity.Value, out scaleFactor))
				return new TransverseMercatorSouth(origin, offset, scaleFactor, fromSpheroid);

			return null;
		}

		private static PopularVisualizationPseudoMercator CreatePopularVisualisationPseudoMercator(ProjectionCompilationParams opData) {
			var originLatParam = new KeywordNamedParameterSelector("LAT", "NATURALORIGIN");
			var originLonParam = new KeywordNamedParameterSelector("LON", "NATURALORIGIN");
			var offsetXParam = new KeywordNamedParameterSelector("FALSE", "OFFSET", "X", "EAST");
			var offsetYParam = new KeywordNamedParameterSelector("FALSE", "OFFSET", "Y", "NORTH");
			if (!opData.ParameterLookup.Assign(originLatParam, originLonParam, offsetXParam, offsetYParam))
				return null;

			var spheroid = opData.StepParams.RelatedOutputCrsUnitConvertedSpheroid;
			if (null == spheroid)
				return null;

			GeographicCoordinate origin;
			Vector2 offset;

			if (
				TryCreateGeographicCoordinate(originLatParam.Selection, originLonParam.Selection, out origin)
				&& TryCreateVector2(offsetXParam.Selection, offsetYParam.Selection, out offset)
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

			var spheroid = opData.StepParams.RelatedOutputCrsUnitConvertedSpheroid;
			if (null == spheroid)
				return null;

			GeographicCoordinate origin;
			if (!originLatParam.IsSelected || !originLonParam.IsSelected || !TryCreateGeographicCoordinate(originLatParam.Selection, originLonParam.Selection, out origin))
				origin = GeographicCoordinate.Zero;

			Vector2 offset;
			if (!offsetXParam.IsSelected || !offsetYParam.IsSelected || !TryCreateVector2(offsetXParam.Selection, offsetYParam.Selection, out offset))
				offset = Vector2.Zero;

			var spherical = _coordinateOperationNameComparer.Normalize(opData.OperationName).EndsWith("SPHERICAL");

			return spherical
				? (ProjectionBase) new LambertAzimuthalEqualAreaSpherical(origin, offset, spheroid)
				: new LambertAzimuthalEqualArea(origin, offset, spheroid);
		}

		private ProjectionBase CreateEquidistantCylindrical(ProjectionCompilationParams opData) {
			var originLatParam = new KeywordNamedParameterSelector("LAT", "PARALLEL");
			var originLonParam = new KeywordNamedParameterSelector("LON", "NATURALORIGIN");
			var offsetXParam = new KeywordNamedParameterSelector("FALSE", "OFFSET", "X", "EAST");
			var offsetYParam = new KeywordNamedParameterSelector("FALSE", "OFFSET", "Y", "NORTH");
			opData.ParameterLookup.Assign(originLatParam, originLonParam, offsetXParam, offsetYParam);

			var spheroid = opData.StepParams.RelatedOutputCrsUnitConvertedSpheroid;
			if (null == spheroid)
				return null;

			GeographicCoordinate origin;
			if (!originLatParam.IsSelected || !originLonParam.IsSelected || !TryCreateGeographicCoordinate(originLatParam.Selection, originLonParam.Selection, out origin))
				origin = GeographicCoordinate.Zero;

			Vector2 offset;
			if (!offsetXParam.IsSelected || !offsetYParam.IsSelected || !TryCreateVector2(offsetXParam.Selection, offsetYParam.Selection, out offset))
				offset = Vector2.Zero;

			var spherical = _coordinateOperationNameComparer.Normalize(opData.OperationName).EndsWith("SPHERICAL");

			return spherical
				? (ProjectionBase)new EquidistantCylindricalSpherical(origin, offset, spheroid)
				: new EquidistantCylindrical(origin, offset, spheroid);
		}

		private ITransformation<GeographicCoordinate, Point2> CreateCassiniSoldner(ProjectionCompilationParams opData) {
			var originLatParam = new KeywordNamedParameterSelector("LAT", "ORIGIN");
			var originLonParam = new KeywordNamedParameterSelector("LON", "ORIGIN");
			var offsetXParam = new KeywordNamedParameterSelector("FALSE", "OFFSET", "X", "EAST");
			var offsetYParam = new KeywordNamedParameterSelector("FALSE", "OFFSET", "Y", "NORTH");
			opData.ParameterLookup.Assign(originLatParam, originLonParam, offsetXParam, offsetYParam);

			var fromSpheroid = opData.StepParams.RelatedOutputCrsUnitConvertedSpheroid;
			if (null == fromSpheroid)
				return null;

			GeographicCoordinate origin;
			if (!(originLatParam.IsSelected || originLonParam.IsSelected) || !TryCreateGeographicCoordinate(originLatParam.Selection, originLonParam.Selection, out origin))
				origin = GeographicCoordinate.Zero;

			Vector2 offset;
			if (!(offsetXParam.IsSelected || offsetYParam.IsSelected) || !TryCreateVector2(offsetXParam.Selection, offsetYParam.Selection, opData.StepParams.RelatedOutputCrsUnit, out offset))
				offset = Vector2.Zero;

			return new CassiniSoldner(origin, offset, fromSpheroid);
		}

		private ITransformation<GeographicCoordinate, Point2> CreateMercator(ProjectionCompilationParams opData) {
			var originLatParam = new KeywordNamedParameterSelector("LAT", "ORIGIN");
			var originLonParam = new KeywordNamedParameterSelector("LON", "ORIGIN");
			var offsetXParam = new KeywordNamedParameterSelector("FALSE", "OFFSET", "X", "EAST");
			var offsetYParam = new KeywordNamedParameterSelector("FALSE", "OFFSET", "Y", "NORTH");
			var scaleFactorParam = new KeywordNamedParameterSelector("SCALE");
			opData.ParameterLookup.Assign(originLatParam, originLonParam, offsetXParam, offsetYParam, scaleFactorParam);

			var fromSpheroid = opData.StepParams.RelatedOutputCrsUnitConvertedSpheroid;
			if (null == fromSpheroid)
				return null;

			GeographicCoordinate origin;
			if (!(originLatParam.IsSelected || originLonParam.IsSelected) || !TryCreateGeographicCoordinate(originLatParam.Selection, originLonParam.Selection, out origin))
				origin = GeographicCoordinate.Zero;

			Vector2 offset;
			if (!(offsetXParam.IsSelected || offsetYParam.IsSelected) || !TryCreateVector2(offsetXParam.Selection, offsetYParam.Selection, opData.StepParams.RelatedOutputCrsUnit, out offset))
				offset = Vector2.Zero;

			double scaleFactor;
			if (scaleFactorParam.IsSelected && TryGetDouble(scaleFactorParam.Selection, ScaleUnitUnity.Value, out scaleFactor))
				return new Mercator(origin.Longitude, scaleFactor, offset, fromSpheroid);

			return new Mercator(origin, offset, fromSpheroid);
		}

		private ITransformation<GeographicCoordinate, Point2> CreateLambertConicConformal(ProjectionCompilationParams opData) {
			var originLatParam = new KeywordNamedParameterSelector("LAT", "ORIGIN");
			var originLonParam = new KeywordNamedParameterSelector("LON", "ORIGIN");
			var parallel1Param = new KeywordNamedParameterSelector("LAT", "1", "PARALLEL");
			var parallel2Param = new KeywordNamedParameterSelector("LAT", "2", "PARALLEL");
			var offsetXParam = new KeywordNamedParameterSelector("FALSE", "OFFSET", "X", "EAST");
			var offsetYParam = new KeywordNamedParameterSelector("FALSE", "OFFSET", "Y", "NORTH");
			var scaleFactorParam = new KeywordNamedParameterSelector("SCALE");
			opData.ParameterLookup.Assign(originLatParam, originLonParam, offsetXParam, offsetYParam, scaleFactorParam, parallel1Param, parallel2Param);

			var fromSpheroid = opData.StepParams.RelatedOutputCrsUnitConvertedSpheroid;
			if (null == fromSpheroid)
				return null;

			GeographicCoordinate origin;
			if (!(originLatParam.IsSelected || originLonParam.IsSelected) || !TryCreateGeographicCoordinate(originLatParam.Selection, originLonParam.Selection, out origin))
				origin = GeographicCoordinate.Zero;

			Vector2 offset;
			if (!(offsetXParam.IsSelected || offsetYParam.IsSelected) || !TryCreateVector2(offsetXParam.Selection, offsetYParam.Selection, opData.StepParams.RelatedOutputCrsUnit, out offset))
				offset = Vector2.Zero;

			double parallel1, parallel2;
			if (parallel1Param.IsSelected && parallel2Param.IsSelected && TryGetDouble(parallel1Param.Selection, OgcAngularUnit.DefaultRadians, out parallel1) && TryGetDouble(parallel2Param.Selection, OgcAngularUnit.DefaultRadians, out parallel2)) {
				if (_coordinateOperationNameComparer.Normalize(opData.OperationName).EndsWith("BELGIUM"))
					return new LambertConicConformalBelgium(origin, parallel1, parallel2, offset, fromSpheroid);

				return new LambertConicConformal2Sp(origin, parallel1, parallel2, offset, fromSpheroid);
			}

			double scaleFactor;
			if(scaleFactorParam.IsSelected && TryGetDouble(scaleFactorParam.Selection, ScaleUnitUnity.Value, out scaleFactor))
				return new LambertConicConformal1Sp(origin, scaleFactor, offset, fromSpheroid);

			return null;
		}

		private ProjectionBase CreateLambertConicNearConformal(ProjectionCompilationParams opData) {
			var originLatParam = new KeywordNamedParameterSelector("LAT", "ORIGIN");
			var originLonParam = new KeywordNamedParameterSelector("LON", "ORIGIN");
			var offsetXParam = new KeywordNamedParameterSelector("FALSE", "OFFSET", "X", "EAST");
			var offsetYParam = new KeywordNamedParameterSelector("FALSE", "OFFSET", "Y", "NORTH");
			var scaleFactorParam = new KeywordNamedParameterSelector("SCALE");
			opData.ParameterLookup.Assign(originLatParam, originLonParam, offsetXParam, offsetYParam, scaleFactorParam);

			var fromSpheroid = opData.StepParams.RelatedOutputCrsUnitConvertedSpheroid;
			if (null == fromSpheroid)
				return null;

			GeographicCoordinate origin;
			if (!(originLatParam.IsSelected || originLonParam.IsSelected) || !TryCreateGeographicCoordinate(originLatParam.Selection, originLonParam.Selection, out origin))
				origin = GeographicCoordinate.Zero;

			Vector2 offset;
			if (!(offsetXParam.IsSelected || offsetYParam.IsSelected) || !TryCreateVector2(offsetXParam.Selection, offsetYParam.Selection, opData.StepParams.RelatedOutputCrsUnit, out offset))
				offset = Vector2.Zero;

			double scaleFactor;
			if (scaleFactorParam.IsSelected && TryGetDouble(scaleFactorParam.Selection, ScaleUnitUnity.Value, out scaleFactor))
				return new LambertConicNearConformal(origin, scaleFactor, offset, fromSpheroid);

			return null;
		}

		private ITransformation<GeographicCoordinate, Point2> CreateKrovak(ProjectionCompilationParams opData) {
			var latConeAxisParam = new KeywordNamedParameterSelector("CO","LAT","CONE","AXIS"); // Co-latitude of cone axis
			var latProjectionCenterParam = new KeywordNamedParameterSelector("LAT", "CENTER"); // Latitude of projection centre
			var latPseudoParallelParam = new KeywordNamedParameterSelector("LAT", "PSEUDO", "PARALLEL"); // Latitude of pseudo standard parallel
			var scaleFactorParallelParam = new KeywordNamedParameterSelector("SCALE", "PARALLEL"); // Scale factor on pseudo standard parallel
			var originLonParam = new KeywordNamedParameterSelector("LON", "ORIGIN"); // Longitude of origin
			var offsetXParam = new KeywordNamedParameterSelector("FALSE", "OFFSET", "X", "EAST");
			var offsetYParam = new KeywordNamedParameterSelector("FALSE", "OFFSET", "Y", "NORTH");
			var evalXParam = new KeywordNamedParameterSelector("ORDINATE1", "EVALUATION", "POINT");
			var evalYParam = new KeywordNamedParameterSelector("ORDINATE2", "EVALUATION", "POINT");
			opData.ParameterLookup.Assign(latConeAxisParam, latProjectionCenterParam, latPseudoParallelParam, scaleFactorParallelParam, originLonParam, offsetXParam, offsetYParam, evalXParam, evalYParam);

			var spheroid = opData.StepParams.RelatedOutputCrsUnitConvertedSpheroid;
			if (null == spheroid)
				return null;

			GeographicCoordinate origin;
			if (!latProjectionCenterParam.IsSelected || !originLonParam.IsSelected || !TryCreateGeographicCoordinate(latProjectionCenterParam.Selection, originLonParam.Selection, out origin))
				origin = GeographicCoordinate.Zero;

			Vector2 offset;
			if (!offsetXParam.IsSelected || !offsetYParam.IsSelected || !TryCreateVector2(offsetXParam.Selection, offsetYParam.Selection, out offset))
				offset = Vector2.Zero;

			Point2 evalPoint;
			if (!evalXParam.IsSelected || !evalYParam.IsSelected || !TryCreatePoint2(evalXParam.Selection, evalYParam.Selection, out evalPoint))
				evalPoint = Point2.Zero;

			double azimuthOfInitialLine;
			if (!latConeAxisParam.IsSelected || !TryGetDouble(latConeAxisParam.Selection, OgcAngularUnit.DefaultRadians, out azimuthOfInitialLine))
				azimuthOfInitialLine = Double.NaN;

			double latitudeOfPseudoStandardParallel;
			if (!latPseudoParallelParam.IsSelected || !TryGetDouble(latPseudoParallelParam.Selection, OgcAngularUnit.DefaultRadians, out latitudeOfPseudoStandardParallel))
				latitudeOfPseudoStandardParallel = Double.NaN;

			double scaleFactor;
			if (!scaleFactorParallelParam.IsSelected || !TryGetDouble(scaleFactorParallelParam.Selection, ScaleUnitUnity.Value, out scaleFactor))
				scaleFactor = Double.NaN;

			var normalizedName = _coordinateOperationNameComparer.Normalize(opData.OperationName);

			double[] constants = null;
			if (normalizedName.StartsWith("KROVAKMODIFIED")) {
				var constantParams = new NamedParameterSelector[] {
					new FullMatchParameterSelector("C1"),
					new FullMatchParameterSelector("C2"),
					new FullMatchParameterSelector("C3"),
					new FullMatchParameterSelector("C4"),
					new FullMatchParameterSelector("C5"),
					new FullMatchParameterSelector("C6"),
					new FullMatchParameterSelector("C7"),
					new FullMatchParameterSelector("C8"),
					new FullMatchParameterSelector("C9"),
					new FullMatchParameterSelector("C10")
				};
				if (opData.ParameterLookup.Assign(constantParams)) {
					constants = constantParams
						.OrderBy(x => Int32.Parse(x.Selection.Name.Substring(1)))
						.Select(p => {
							double value;
							TryGetDouble(p.Selection, ScaleUnitUnity.Value, out value);
							return value;
						})
						.ToArray();
				}
			}

			if (normalizedName.Equals("KROVAKNORTH")) {
				return new KrovakNorth(origin, latitudeOfPseudoStandardParallel, azimuthOfInitialLine, scaleFactor, offset, spheroid);
			}
			if (null != constants) {
				if (normalizedName.Equals("KROVAKMODIFIED")) {
					return new KrovakModified(origin, latitudeOfPseudoStandardParallel, azimuthOfInitialLine, scaleFactor, offset, spheroid, evalPoint, constants);
				}
				if (normalizedName.Equals("KROVAKMODIFIEDNORTH")) {
					return new KrovakModifiedNorth(origin, latitudeOfPseudoStandardParallel, azimuthOfInitialLine, scaleFactor, offset, spheroid, evalPoint, constants);
				}
			}

			return new Krovak(origin, latitudeOfPseudoStandardParallel, azimuthOfInitialLine, scaleFactor, offset, spheroid);
		}

		private ProjectionBase CreateObliqueStereographic(ProjectionCompilationParams opData) {
			var originLatParam = new KeywordNamedParameterSelector("LAT", "ORIGIN");
			var originLonParam = new KeywordNamedParameterSelector("LON", "ORIGIN");
			var offsetXParam = new KeywordNamedParameterSelector("FALSE", "OFFSET", "X", "EAST");
			var offsetYParam = new KeywordNamedParameterSelector("FALSE", "OFFSET", "Y", "NORTH");
			var scaleFactorParam = new KeywordNamedParameterSelector("SCALE");
			opData.ParameterLookup.Assign(originLatParam, originLonParam, offsetXParam, offsetYParam, scaleFactorParam);

			var fromSpheroid = opData.StepParams.RelatedOutputCrsUnitConvertedSpheroid;
			if (null == fromSpheroid)
				return null;

			GeographicCoordinate origin;
			if (!(originLatParam.IsSelected || originLonParam.IsSelected) || !TryCreateGeographicCoordinate(originLatParam.Selection, originLonParam.Selection, out origin))
				origin = GeographicCoordinate.Zero;

			Vector2 offset;
			if (!(offsetXParam.IsSelected || offsetYParam.IsSelected) || !TryCreateVector2(offsetXParam.Selection, offsetYParam.Selection, opData.StepParams.RelatedOutputCrsUnit, out offset))
				offset = Vector2.Zero;

			double scaleFactor;
			if (scaleFactorParam.IsSelected && TryGetDouble(scaleFactorParam.Selection, ScaleUnitUnity.Value, out scaleFactor))
				return new ObliqueStereographic(origin, scaleFactor, offset, fromSpheroid);

			return null;
		}

		private ProjectionBase CreateLabordeObliqueMercator(ProjectionCompilationParams opData) {
			var originLatParam = new KeywordNamedParameterSelector("LAT", "CENTER");
			var originLonParam = new KeywordNamedParameterSelector("LON", "CENTER");
			var offsetXParam = new KeywordNamedParameterSelector("FALSE", "OFFSET", "X", "EAST");
			var offsetYParam = new KeywordNamedParameterSelector("FALSE", "OFFSET", "Y", "NORTH");
			var scaleFactorParam = new KeywordNamedParameterSelector("SCALE");
			var azimuthParam = new KeywordNamedParameterSelector("AZIMUTH");
			opData.ParameterLookup.Assign(originLatParam, originLonParam, offsetXParam, offsetYParam, scaleFactorParam, azimuthParam);

			var fromSpheroid = opData.StepParams.RelatedOutputCrsUnitConvertedSpheroid;
			if (null == fromSpheroid)
				return null;

			GeographicCoordinate origin;
			if (!(originLatParam.IsSelected || originLonParam.IsSelected) || !TryCreateGeographicCoordinate(originLatParam.Selection, originLonParam.Selection, out origin))
				origin = GeographicCoordinate.Zero;

			Vector2 offset;
			if (!(offsetXParam.IsSelected || offsetYParam.IsSelected) || !TryCreateVector2(offsetXParam.Selection, offsetYParam.Selection, opData.StepParams.RelatedOutputCrsUnit, out offset))
				offset = Vector2.Zero;

			double scaleFactor;
			if (!scaleFactorParam.IsSelected || !TryGetDouble(scaleFactorParam.Selection, ScaleUnitUnity.Value, out scaleFactor))
				scaleFactor = 1.0;

			double azimuth;
			if (!azimuthParam.IsSelected || !TryGetDouble(azimuthParam.Selection, OgcAngularUnit.DefaultRadians, out azimuth))
				azimuth = 0;

			return new LabordeObliqueMercator(origin,azimuth,scaleFactor,fromSpheroid, offset);
		}

		private ProjectionBase CreateHotineObliqueMercator(ProjectionCompilationParams opData){
			var originLatParam = new KeywordNamedParameterSelector("LAT", "CENTER");
			var originLonParam = new KeywordNamedParameterSelector("LON", "CENTER");
			var offsetXParam = new KeywordNamedParameterSelector("FALSE", "OFFSET", "X", "EAST");
			var offsetYParam = new KeywordNamedParameterSelector("FALSE", "OFFSET", "Y", "NORTH");
			var scaleFactorParam = new KeywordNamedParameterSelector("SCALE");
			var azimuthParam = new KeywordNamedParameterSelector("AZIMUTH");
			var angleSkewParam = new KeywordNamedParameterSelector("ANGLE","SKEW");
			opData.ParameterLookup.Assign(originLatParam, originLonParam, offsetXParam, offsetYParam, scaleFactorParam, azimuthParam, angleSkewParam);

			var fromSpheroid = opData.StepParams.RelatedOutputCrsUnitConvertedSpheroid;
			if (null == fromSpheroid)
				return null;

			GeographicCoordinate origin;
			if (!(originLatParam.IsSelected || originLonParam.IsSelected) || !TryCreateGeographicCoordinate(originLatParam.Selection, originLonParam.Selection, out origin))
				origin = GeographicCoordinate.Zero;

			Vector2 offset;
			if (!(offsetXParam.IsSelected || offsetYParam.IsSelected) || !TryCreateVector2(offsetXParam.Selection, offsetYParam.Selection, opData.StepParams.RelatedOutputCrsUnit, out offset))
				offset = Vector2.Zero;

			double scaleFactor;
			if (!scaleFactorParam.IsSelected || !TryGetDouble(scaleFactorParam.Selection, ScaleUnitUnity.Value, out scaleFactor))
				scaleFactor = 1.0;

			double azimuth;
			if (!azimuthParam.IsSelected || !TryGetDouble(azimuthParam.Selection, OgcAngularUnit.DefaultRadians, out azimuth))
				azimuth = 0;

			double angleSkew;
			if (!angleSkewParam.IsSelected || !TryGetDouble(angleSkewParam.Selection, OgcAngularUnit.DefaultRadians, out angleSkew))
				angleSkew = 0;

			// TODO: in which cases would variant A be used?

			return new HotineObliqueMercator.VariantB(origin, azimuth, angleSkew, scaleFactor, offset, fromSpheroid);
		}

		private readonly INameNormalizedComparer _coordinateOperationNameComparer;
		private readonly Dictionary<string, Func<ProjectionCompilationParams, ITransformation<GeographicCoordinate, Point2>>> _transformationCreatorLookup;

		public StaticProjectionStepCompiler(INameNormalizedComparer coordinateOperationNameComparer = null) {
			_coordinateOperationNameComparer = coordinateOperationNameComparer ?? CoordinateOperationNameNormalizedComparer.Default;
			_transformationCreatorLookup = new Dictionary<string, Func<ProjectionCompilationParams, ITransformation<GeographicCoordinate,Point2>>>(_coordinateOperationNameComparer) {
				{CoordinateOperationStandardNames.AlbersEqualAreaConic,null},
				{CoordinateOperationStandardNames.AzimuthalEquidistant,null},
				{CoordinateOperationStandardNames.CassiniSoldner, CreateCassiniSoldner},
				{CoordinateOperationStandardNames.CylindricalEqualArea,null},
				{CoordinateOperationStandardNames.Eckert4,null},
				{CoordinateOperationStandardNames.Eckert6,null},
				{CoordinateOperationStandardNames.EquidistantConic,null},
				{CoordinateOperationStandardNames.EquidistantCylindrical,CreateEquidistantCylindrical},
				{CoordinateOperationStandardNames.EquidistantCylindricalSpherical,CreateEquidistantCylindrical},
				{CoordinateOperationStandardNames.Equirectangular,null},
				{CoordinateOperationStandardNames.GallStereographic,null},
				{CoordinateOperationStandardNames.Geos,null},
				{CoordinateOperationStandardNames.Gnomonic,null},
				{CoordinateOperationStandardNames.HotineObliqueMercator,CreateHotineObliqueMercator},
				{CoordinateOperationStandardNames.Krovak,CreateKrovak},
				{CoordinateOperationStandardNames.KrovakNorth,CreateKrovak},
				{CoordinateOperationStandardNames.KrovakModifiedNorth,CreateKrovak},
				{CoordinateOperationStandardNames.KrovakModified,CreateKrovak},
				{CoordinateOperationStandardNames.KrovakObliqueConicConformal,null},
				{CoordinateOperationStandardNames.LabordeObliqueMercator,CreateLabordeObliqueMercator},
				{CoordinateOperationStandardNames.LambertAzimuthalEqualArea,CreateLambertAzimuthalEqualArea},
				{CoordinateOperationStandardNames.LambertAzimuthalEqualAreaSpherical,CreateLambertAzimuthalEqualArea},
				{CoordinateOperationStandardNames.LambertConicConformal1Sp,CreateLambertConicConformal},
				{CoordinateOperationStandardNames.LambertConicConformal2Sp,CreateLambertConicConformal},
				{CoordinateOperationStandardNames.LambertConicConformal2SpBelgium,CreateLambertConicConformal},
				{CoordinateOperationStandardNames.LambertConicNearConformal,CreateLambertConicNearConformal},
				{CoordinateOperationStandardNames.Mercator1Sp,CreateMercator},
				{CoordinateOperationStandardNames.Mercator2Sp,CreateMercator},
				{CoordinateOperationStandardNames.MillerCylindrical,null},
				{CoordinateOperationStandardNames.Mollweide,null},
				{CoordinateOperationStandardNames.NewZealandMapGrid,null},
				{CoordinateOperationStandardNames.ObliqueMercator,null},
				{CoordinateOperationStandardNames.ObliqueStereographic,CreateObliqueStereographic},
				{CoordinateOperationStandardNames.Orthographic,null},
				{CoordinateOperationStandardNames.PolarStereographic,CreatePolarStereographic},
				{CoordinateOperationStandardNames.Polyconic,null},
				{CoordinateOperationStandardNames.PopularVisualisationPseudoMercator, CreatePopularVisualisationPseudoMercator},
				{CoordinateOperationStandardNames.Robinson,null},
				{CoordinateOperationStandardNames.RosenmundObliqueMercator,null},
				{CoordinateOperationStandardNames.Sinusoidal,null},
				{CoordinateOperationStandardNames.SwissObliqueCylindrical,null},
				{CoordinateOperationStandardNames.Stereographic,null},
				{CoordinateOperationStandardNames.TransverseMercator,CreateTransverseMercator},
				{CoordinateOperationStandardNames.TransverseMercatorZonedGridSystem,CreateTransverseMercator},
				{CoordinateOperationStandardNames.TransverseMercatorSouthOriented,CreateTransverseMercatorSouth},
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
			var desiredInputUnits = OgcAngularUnit.DefaultRadians;
			if (null != desiredInputUnits && !UnitEqualityComparer.Default.Equals(actualInputUnits, desiredInputUnits)) {
				var conv = SimpleUnitConversionGenerator.FindConversion(actualInputUnits, desiredInputUnits);
				if (null != conv) {
					var conversionTransformation = new AngularElementTransformation(conv);
					resultTransformation = new ConcatenatedTransformation(new[] { conversionTransformation, resultTransformation });
				}
			}

			var outputUnits = stepParameters.RelatedOutputCrsUnit ?? stepParameters.RelatedInputCrsUnit ?? OgcLinearUnit.DefaultMeter;

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
			/*var projectionSpheroid = stepParameters.RelatedInputSpheroid as ISpheroidInfo;
			var desiredInputUnits = null == projectionSpheroid
				? stepParameters.RelatedInputCrsUnit
				: projectionSpheroid.AxisUnit;*/
			var desiredInputUnits = stepParameters.RelatedInputCrsUnit;
			if(null != desiredInputUnits && !UnitEqualityComparer.Default.Equals(actualInputUnits, desiredInputUnits)){
				var conv = SimpleUnitConversionGenerator.FindConversion(actualInputUnits, desiredInputUnits);
				if(null != conv){
					var conversionTransformation = new LinearElementTransformation(conv);
					resultTransformation = new ConcatenatedTransformation(new[]{conversionTransformation, resultTransformation});
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
