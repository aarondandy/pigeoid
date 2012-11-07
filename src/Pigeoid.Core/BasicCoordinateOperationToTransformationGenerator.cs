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
using Pigeoid.Utility;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid
{
	public class BasicCoordinateOperationToTransformationGenerator : ICoordinateOperationToTransformationGenerator
	{

		private class ParamSelector
		{
			public ParamSelector(params string[] keywords) {
				Keywords = keywords;
			}

			public INamedParameter Selection;
			public readonly string[] Keywords;

			public bool IsSelected { get { return null != Selection; } }

			public virtual int Score(string name) {
				return Keywords.Count(name.Contains);
			}

			internal static bool AllAreSelected([NotNull] params ParamSelector[] selectors) {
				return selectors.All(x => x.IsSelected);
			}
		}

		private class NamedParameterLookup
		{
			
			private readonly INamedParameter[] _core;
			private readonly string[] _normalizedNames;
			private readonly Dictionary<string, INamedParameter> _normalizedLookup; 

			public NamedParameterLookup([NotNull] INamedParameter[] core) {
				_core = core;
				_normalizedNames = Array.ConvertAll(
					core,
					x => ParameterNameNormalizedComparer.Default.Normalize(x.Name));
				_normalizedLookup = new Dictionary<string, INamedParameter>(StringComparer.OrdinalIgnoreCase);
				for (int i = 0; i < _core.Length; i++)
					_normalizedLookup[_normalizedNames[i]] = _core[i];
			}

			public bool Assign(params ParamSelector[] selectors) {
				var paramsToSearch = _normalizedLookup.ToList();
				foreach(var selector in selectors) {
					if (paramsToSearch.Count == 0)
						return true;

					int bestScore = 0;
					int bestIndex = -1;
					for(int i = 0; i < paramsToSearch.Count; i++) {
						int score = selector.Score(paramsToSearch[i].Key);
						if(score > 0 && score > bestScore) {
							bestScore = score;
							bestIndex = i;
						}
					}
					if(bestIndex >= 0) {
						selector.Selection = paramsToSearch[bestIndex].Value;
						paramsToSearch.RemoveAt(bestIndex);
					}
				}
				return paramsToSearch.Count == 0;
			}

		}

		private class OperationGenerationParams
		{
			public NamedParameterLookup ParamLookup;
			public string OperationName;
			public string OperationNameNormalized;
			public ICrs CrsFrom;
			public ICrs CrsTo;
		}

		private static ISpheroid<double> ExtractSpheroid(ICrs crs) {
			var geodetic = crs as ICrsGeodetic;
			if (geodetic != null && geodetic.Datum != null)
				return geodetic.Datum.Spheroid;
			return null;
		}

		private static bool TryCreateGeographicCoordinate(INamedParameter latParam, INamedParameter lonParam, out GeographicCoordinate result) {
			double lat, lon;
			if(NamedParameter.TryGetDouble(latParam, out lat) && NamedParameter.TryGetDouble(lonParam, out lon)){

				var latUnit = latParam.Unit;
				if (null != latUnit){
					var conv = SimpleUnitConversionGenerator.FindConversion(latUnit, OgcAngularUnit.DefaultRadians);
					if(null != conv)
						lat = conv.TransformValue(lat);
				}

				var lonUnit = lonParam.Unit;
				if(null != lonUnit){
					var conv = SimpleUnitConversionGenerator.FindConversion(lonUnit, OgcAngularUnit.DefaultRadians);
					if(null != conv)
						lon = conv.TransformValue(lon);
				}

				result = new GeographicCoordinate(lat, lon);
				return true;
			}
			result = default(GeographicCoordinate);
			return false;
		}

		private static bool TryCrateVector2(INamedParameter xParam, INamedParameter yParam, out Vector2 result) {
			double x, y;
			if(NamedParameter.TryGetDouble(xParam, out x) && NamedParameter.TryGetDouble(yParam, out y)) {
				result = new Vector2(x,y);
				return true;
			}
			result = Vector2.Invalid;
			return false;
		}

		private static bool TryCrateVector3(INamedParameter xParam, INamedParameter yParam, INamedParameter zParam, out Vector3 result) {
			double x, y, z;
			if (NamedParameter.TryGetDouble(xParam, out x) && NamedParameter.TryGetDouble(yParam, out y) && NamedParameter.TryGetDouble(zParam, out z)) {
				result = new Vector3(x, y, z);
				return true;
			}
			result = Vector3.Invalid;
			return false;
		}

		private static bool TryCratePoint2(INamedParameter xParam, INamedParameter yParam, out Point2 result) {
			double x, y;
			if (NamedParameter.TryGetDouble(xParam, out x) && NamedParameter.TryGetDouble(yParam, out y)) {
				result = new Point2(x, y);
				return true;
			}
			result = Point2.Invalid;
			return false;
		}

		private static bool TryCratePoint3(INamedParameter xParam, INamedParameter yParam, INamedParameter zParam, out Point3 result) {
			double x, y, z;
			if (NamedParameter.TryGetDouble(xParam, out x) && NamedParameter.TryGetDouble(yParam, out y) && NamedParameter.TryGetDouble(zParam, out z)) {
				result = new Point3(x, y, z);
				return true;
			}
			result = Point3.Invalid;
			return false;
		}

		private static CassiniSoldner CreateCassiniSoldner(OperationGenerationParams opData) {
			var originLatParam = new ParamSelector("LAT", "NATURALORIGIN");
			var originLonParam = new ParamSelector("LON", "NATURALORIGIN");
			var offsetXParam = new ParamSelector("FALSE", "OFFSET", "X", "EAST");
			var offsetYParam = new ParamSelector("FALSE", "OFFSET", "Y", "NORTH");
			if (!opData.ParamLookup.Assign(originLatParam, originLonParam, offsetXParam, offsetYParam))
				return null;

			var spheroid = ExtractSpheroid(opData.CrsFrom) ?? ExtractSpheroid(opData.CrsTo);
			if (null == spheroid)
				return null;

			GeographicCoordinate origin;
			Vector2 offset;

			if(
				TryCreateGeographicCoordinate(originLatParam.Selection, originLonParam.Selection, out origin)
				&& TryCrateVector2(offsetXParam.Selection, offsetYParam.Selection, out offset)
			) {
				return new CassiniSoldner(origin, offset, spheroid);
			}
			return null;
		}

		private static LabordeObliqueMercator CreateLabordeObliqueMercator(OperationGenerationParams opData) {
			return null;
		}

		private static TransverseMercator CreateTransverseMercator(OperationGenerationParams opData) {
			var originLatParam = new ParamSelector("LAT", "NATURALORIGIN");
			var originLonParam = new ParamSelector("LON", "NATURALORIGIN");
			var originScaleFactorParam = new ParamSelector("SCALE", "NATURALORIGIN");
			var offsetXParam = new ParamSelector("FALSE", "OFFSET", "X", "EAST");
			var offsetYParam = new ParamSelector("FALSE", "OFFSET", "Y", "NORTH");

			if (!opData.ParamLookup.Assign(originLatParam, originLonParam, originScaleFactorParam, offsetXParam, offsetYParam))
				return null;

			GeographicCoordinate origin;
			Vector2 offset;
			double scale;

			var spheroid = ExtractSpheroid(opData.CrsFrom) ?? ExtractSpheroid(opData.CrsTo);
			if (null == spheroid)
				return null;

			if(
				TryCreateGeographicCoordinate(originLatParam.Selection, originLonParam.Selection, out origin)
				&& TryCrateVector2(offsetXParam.Selection, offsetYParam.Selection, out offset)
				&& ConversionUtil.TryConvertDoubleMultiCulture(originScaleFactorParam.Selection.Value, out scale)
			) {
				return new TransverseMercator(origin, offset, scale, spheroid);
			}

			return null;
		}

		private static GeographicOffset CreateGeographicOffset(OperationGenerationParams opData) {
			var deltaLatParam = new ParamSelector("LAT");
			var deltaLonParam = new ParamSelector("LON");
			if (!opData.ParamLookup.Assign(deltaLatParam, deltaLonParam))
				return null;

			var spheroid = ExtractSpheroid(opData.CrsFrom) ?? ExtractSpheroid(opData.CrsTo);
			if (null == spheroid)
				return null;

			GeographicCoordinate delta;
			if(TryCreateGeographicCoordinate(deltaLatParam.Selection, deltaLonParam.Selection, out delta))
				return new GeographicOffset(delta);

			return null;
		}

		[Obsolete("TODO: make this more generic to handle geocentric operations as well")]
		private static GeographicGeocentricTranslation CreateGeographicGeocentricTranslation(OperationGenerationParams opData) {
			var deltaXParam = new ParamSelector("XAXIS");
			var deltaYParam = new ParamSelector("YAXIS");
			var deltaZParam = new ParamSelector("ZAXIS");

			if (!opData.ParamLookup.Assign(deltaXParam, deltaYParam, deltaZParam))
				return null;

			var spheroidFrom = ExtractSpheroid(opData.CrsFrom) ?? ExtractSpheroid(opData.CrsTo);
			if (null == spheroidFrom)
				return null;

			var spheroidTo = ExtractSpheroid(opData.CrsTo) ?? spheroidFrom;

			Vector3 delta;
			if(TryCrateVector3(deltaXParam.Selection, deltaYParam.Selection, deltaZParam.Selection, out delta))
				return new GeographicGeocentricTranslation(spheroidFrom, delta, spheroidTo);

			return null;
		}

		private static Mercator CreateMercator(OperationGenerationParams opData) {
			var originLatParam = new ParamSelector("LAT", "NATURALORIGIN");
			var originLonParam = new ParamSelector("LON", "NATURALORIGIN");
			var originScaleFactorParam = new ParamSelector("SCALE", "NATURALORIGIN");
			var offsetXParam = new ParamSelector("FALSE", "OFFSET", "X", "EAST");
			var offsetYParam = new ParamSelector("FALSE", "OFFSET", "Y", "NORTH");
			opData.ParamLookup.Assign(originLatParam, originLonParam, originScaleFactorParam, offsetXParam, offsetYParam);

			var spheroid = ExtractSpheroid(opData.CrsFrom) ?? ExtractSpheroid(opData.CrsTo);
			if (null == spheroid)
				return null;

			GeographicCoordinate? origin = null;
			if(originLatParam.IsSelected && originLonParam.IsSelected) {
				GeographicCoordinate geogValue;
				if (TryCreateGeographicCoordinate(originLatParam.Selection, originLonParam.Selection, out geogValue))
					origin = geogValue;
			}
			Vector2? offset = null;
			if(offsetXParam.IsSelected && offsetYParam.IsSelected) {
				Vector2 vector2;
				if (TryCrateVector2(offsetXParam.Selection, offsetYParam.Selection, out vector2))
					offset = vector2;
			}
			double? scale = null;
			if(originScaleFactorParam.IsSelected) {
				double doubleValue;
				if (NamedParameter.TryGetDouble(originScaleFactorParam.Selection, out doubleValue))
					scale = doubleValue;
			}

			if(origin.HasValue && offset.HasValue && scale.HasValue) {
				// method A
				return new Mercator(origin.Value.Longitude, scale.Value, offset.Value, spheroid);
			}

			return null;
		}

		private static EquidistantCylindrical CreateEquidistantCylindrical(OperationGenerationParams opData){
			var originLatParam = new ParamSelector("LAT", "NATURALORIGIN");
			var originLonParam = new ParamSelector("LON", "NATURALORIGIN");
			var offsetXParam = new ParamSelector("FALSE", "OFFSET", "X", "EAST");
			var offsetYParam = new ParamSelector("FALSE", "OFFSET", "Y", "NORTH");
			if (!opData.ParamLookup.Assign(originLatParam, originLonParam, offsetXParam, offsetYParam))
				return null;

			var spheroid = ExtractSpheroid(opData.CrsFrom) ?? ExtractSpheroid(opData.CrsTo);
			if (null == spheroid)
				return null;

			GeographicCoordinate origin;
			Vector2 offset;

			if (
				TryCreateGeographicCoordinate(originLatParam.Selection, originLonParam.Selection, out origin)
				&& TryCrateVector2(offsetXParam.Selection, offsetYParam.Selection, out offset)
			) {
				return new EquidistantCylindrical(origin, offset, spheroid);
			}
			return null;
		}

		private static ITransformation CreatePositionVectorTransformation(OperationGenerationParams opData){
			var xTransParam = new ParamSelector("XAXIS", "TRANS");
			var yTransParam = new ParamSelector("YAXIS", "TRANS");
			var zTransParam = new ParamSelector("ZAXIS", "TRANS");
			var xRotParam = new ParamSelector("XAXIS", "ROT");
			var yRotParam = new ParamSelector("YAXIS", "ROT");
			var zRotParam = new ParamSelector("ZAXIS", "ROT");
			var scaleParam = new ParamSelector("SCALE");

			if (!opData.ParamLookup.Assign(xTransParam, yTransParam, zTransParam, xRotParam, yRotParam, zRotParam, scaleParam))
				return null;

			Vector3 translation, rotation;
			double scale;

			if (!TryCrateVector3(xTransParam.Selection, yTransParam.Selection, zTransParam.Selection, out translation))
				return null;
			if (!TryCrateVector3(xRotParam.Selection, yRotParam.Selection, zRotParam.Selection, out rotation))
				return null;
			if (!NamedParameter.TryGetDouble(scaleParam.Selection, out scale))
				return null;

			var helmert = new Helmert7Transformation(translation, rotation, scale);

			if (opData.CrsFrom is ICrsGeocentric && opData.CrsTo is ICrsGeocentric)
				return helmert;

			var spheroidFrom = ExtractSpheroid(opData.CrsFrom) ?? ExtractSpheroid(opData.CrsTo);
			if (null == spheroidFrom)
				return null;

			var spheroidTo = ExtractSpheroid(opData.CrsTo) ?? spheroidFrom;

			return new Helmert7GeographicTransformation(spheroidFrom, helmert, spheroidTo);
		}

		private static ITransformation CreateCoordinateFrameTransformation(OperationGenerationParams opData){
			var xTransParam = new ParamSelector("XAXIS", "TRANS");
			var yTransParam = new ParamSelector("YAXIS", "TRANS");
			var zTransParam = new ParamSelector("ZAXIS", "TRANS");
			var xRotParam = new ParamSelector("XAXIS", "ROT");
			var yRotParam = new ParamSelector("YAXIS", "ROT");
			var zRotParam = new ParamSelector("ZAXIS", "ROT");
			var scaleParam = new ParamSelector("SCALE");

			if (!opData.ParamLookup.Assign(xTransParam, yTransParam, zTransParam, xRotParam, yRotParam, zRotParam, scaleParam))
				return null;

			Vector3 translation, rotation;
			double scale;

			if (!TryCrateVector3(xTransParam.Selection, yTransParam.Selection, zTransParam.Selection, out translation))
				return null;
			if (!TryCrateVector3(xRotParam.Selection, yRotParam.Selection, zRotParam.Selection, out rotation))
				return null;
			if (!NamedParameter.TryGetDouble(scaleParam.Selection, out scale))
				return null;

			var helmert = new Helmert7Transformation(translation, rotation.GetNegative(), scale);

			if(opData.CrsFrom is ICrsGeocentric && opData.CrsTo is ICrsGeocentric)
				return helmert;

			var spheroidFrom = ExtractSpheroid(opData.CrsFrom) ?? ExtractSpheroid(opData.CrsTo);
			if (null == spheroidFrom)
				return null;

			var spheroidTo = ExtractSpheroid(opData.CrsTo) ?? spheroidFrom;

			return new Helmert7GeographicTransformation(spheroidFrom, helmert, spheroidTo);
		}

		private static ITransformation CreateMolodenskyBadekasTransformation(OperationGenerationParams opData) {
			var xTransParam = new ParamSelector("XAXIS", "TRANS");
			var yTransParam = new ParamSelector("YAXIS", "TRANS");
			var zTransParam = new ParamSelector("ZAXIS", "TRANS");
			var xRotParam = new ParamSelector("XAXIS", "ROT");
			var yRotParam = new ParamSelector("YAXIS", "ROT");
			var zRotParam = new ParamSelector("ZAXIS", "ROT");
			var scaleParam = new ParamSelector("SCALE");
			var ord1Param = new ParamSelector("ORDINATE1");
			var ord2Param = new ParamSelector("ORDINATE2");
			var ord3Param = new ParamSelector("ORDINATE3");

			if (!opData.ParamLookup.Assign(xTransParam, yTransParam, zTransParam, xRotParam, yRotParam, zRotParam, scaleParam, ord1Param, ord2Param, ord3Param))
				return null;

			Vector3 translation, rotation;
			Point3 ordinate;
			double scale;

			if (!TryCrateVector3(xTransParam.Selection, yTransParam.Selection, zTransParam.Selection, out translation))
				return null;
			if (!TryCrateVector3(xRotParam.Selection, yRotParam.Selection, zRotParam.Selection, out rotation))
				return null;
			if (!TryCratePoint3(ord1Param.Selection, ord2Param.Selection, ord3Param.Selection, out ordinate))
				return null;
			if (!NamedParameter.TryGetDouble(scaleParam.Selection, out scale))
				return null;

			var molodensky = new MolodenskyBadekasTransformation(translation, rotation, ordinate, scale);

			if (opData.CrsFrom is ICrsGeocentric && opData.CrsTo is ICrsGeocentric)
				return molodensky;

			var spheroidFrom = ExtractSpheroid(opData.CrsFrom) ?? ExtractSpheroid(opData.CrsTo);
			if (null == spheroidFrom)
				return null;

			var spheroidTo = ExtractSpheroid(opData.CrsTo) ?? spheroidFrom;

			return new MolodenskyBadekasGeographicTransformation(spheroidFrom, molodensky, spheroidTo);
		}

		private static PopularVisualizationPseudoMercator CreatePopularVisualisationPseudoMercator(OperationGenerationParams opData) {
			var originLatParam = new ParamSelector("LAT", "NATURALORIGIN");
			var originLonParam = new ParamSelector("LON", "NATURALORIGIN");
			var offsetXParam = new ParamSelector("FALSE", "OFFSET", "X", "EAST");
			var offsetYParam = new ParamSelector("FALSE", "OFFSET", "Y", "NORTH");
			if (!opData.ParamLookup.Assign(originLatParam, originLonParam, offsetXParam, offsetYParam))
				return null;

			var spheroid = ExtractSpheroid(opData.CrsFrom) ?? ExtractSpheroid(opData.CrsTo);
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

		private static ITransformation CreateLambertAzimuthalEqualArea(OperationGenerationParams opData){
			var originLatParam = new ParamSelector("LAT", "NATURALORIGIN");
			var originLonParam = new ParamSelector("LON", "NATURALORIGIN");
			var offsetXParam = new ParamSelector("FALSE", "OFFSET", "X", "EAST");
			var offsetYParam = new ParamSelector("FALSE", "OFFSET", "Y", "NORTH");
			var hasParams = opData.ParamLookup.Assign(originLatParam, originLonParam, offsetXParam, offsetYParam);

			var spheroid = ExtractSpheroid(opData.CrsFrom) ?? ExtractSpheroid(opData.CrsTo);
			if (null == spheroid)
				return null;

			if (hasParams && opData.OperationNameNormalized.EndsWith("SPHERICAL")){
				GeographicCoordinate origin;
				Vector2 offset;
				if (
					TryCreateGeographicCoordinate(originLatParam.Selection, originLonParam.Selection, out origin)
					&& TryCrateVector2(offsetXParam.Selection, offsetYParam.Selection, out offset)
				){
					return new LambertAzimuthalEqualAreaSpherical(origin, offset, spheroid);
				}
			}

			return new LambertAzimuthalEqualArea(spheroid);
		}

		private readonly CoordinateOperationNameNormalizedComparer _coordinateOperationNameNormalizer;
		private readonly Dictionary<string, Func<OperationGenerationParams, ITransformation>> _transformationCreatorLookup;

		public BasicCoordinateOperationToTransformationGenerator(){
			_coordinateOperationNameNormalizer = CoordinateOperationNameNormalizedComparer.Default;
			_transformationCreatorLookup = new Dictionary<string, Func<OperationGenerationParams, ITransformation>>(_coordinateOperationNameNormalizer) {
				{CoordinateOperationStandardNames.AlbersEqualAreaConic,null},
				{CoordinateOperationStandardNames.AzimuthalEquidistant,null},
				{CoordinateOperationStandardNames.CassiniSoldner, CreateCassiniSoldner},
				{CoordinateOperationStandardNames.CylindricalEqualArea,null},
				{CoordinateOperationStandardNames.Eckert4,null},
				{CoordinateOperationStandardNames.Eckert6,null},
				{CoordinateOperationStandardNames.EquidistantConic,null},
				{CoordinateOperationStandardNames.EquidistantCylindrical,CreateEquidistantCylindrical},
				{CoordinateOperationStandardNames.Equirectangular,null},
				{CoordinateOperationStandardNames.GallStereographic,null},
				{CoordinateOperationStandardNames.GeographicOffsets, CreateGeographicOffset},
				{CoordinateOperationStandardNames.Geos,null},
				{CoordinateOperationStandardNames.Gnomonic,null},
				{CoordinateOperationStandardNames.HotineObliqueMercator,null},
				{CoordinateOperationStandardNames.KrovakObliqueConicConformal,null},
				{CoordinateOperationStandardNames.LabordeObliqueMercator,CreateLabordeObliqueMercator},
				{CoordinateOperationStandardNames.LambertAzimuthalEqualArea,CreateLambertAzimuthalEqualArea},
				{CoordinateOperationStandardNames.LambertAzimuthalEqualAreaSpherical,CreateLambertAzimuthalEqualArea},
				{CoordinateOperationStandardNames.LambertConicConformal1Sp,null},
				{CoordinateOperationStandardNames.LambertConicConformal2Sp,null},
				{CoordinateOperationStandardNames.Mercator1Sp,CreateMercator},
				{CoordinateOperationStandardNames.Mercator2Sp,CreateMercator},
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
				{CoordinateOperationStandardNames.TransverseMercator,CreateTransverseMercator},
				{CoordinateOperationStandardNames.TransverseMercatorSouthOriented,null},
				{CoordinateOperationStandardNames.TunisiaMiningGrid,null},
				{CoordinateOperationStandardNames.VanDerGrinten,null}
			};
		}

		public ITransformation Create([NotNull] ICoordinateOperationCrsPathInfo operationPath) {
			if(null == operationPath)
				throw new ArgumentNullException("operationPath");

			var allOps = operationPath.CoordinateOperations.ToList();
			var allCrs = operationPath.CoordinateReferenceSystems.ToList();
			if(allCrs.Count == 0)
				throw new ArgumentException("operationPath contains no CRSs", "operationPath");

			var firstCrs = allCrs[0];
			var lastCrs = allCrs[allCrs.Count - 1];

			var firstCoordinateType = ChooseGeometryType(firstCrs);
			if (null == firstCoordinateType)
				return null;

			var lastCoordinateType = ChooseGeometryType(lastCrs);
			if (null == lastCoordinateType)
				return null;

			var transforms = new ITransformation[allOps.Count];
			for (int i = 0; i < transforms.Length; i++) {
				var currentOperation = allOps[i];
				var transformation = /*currentOperation as ITransformation
					??*/ CreateTransform(currentOperation, allCrs[i], allCrs[i + 1]); // TODO: use the operation as is, only if it is a transformation already

				if (null == transformation)
					return null; // not supported

				transforms[i] = transformation;
			}

			return null;// throw new NotImplementedException();
		}

		private bool TryGetTransformCreator(string operationName, out Func<OperationGenerationParams, ITransformation> transformationCreator){
			if (_transformationCreatorLookup.TryGetValue(operationName, out transformationCreator))
				return true;

			var normalizedName = _coordinateOperationNameNormalizer.Normalize(operationName);
			if (normalizedName.StartsWith("MERCATOR")){
				transformationCreator = CreateMercator;
				return true;
			}
			if(normalizedName.StartsWith("GEOCENTRICTRANSLATIONSGEOG")){
				transformationCreator = CreateGeographicGeocentricTranslation;
				return true;
			}
			if(normalizedName.StartsWith("COORDINATEFRAMEROTATION")){
				transformationCreator = CreateCoordinateFrameTransformation;
				return true;
			}
			if (normalizedName.StartsWith("MOLODENSKYBADEKAS")){
				transformationCreator = CreateMolodenskyBadekasTransformation;
				return true;
			}
			if(normalizedName.StartsWith("POSITIONVECTORTRANSFORMATION")){
				transformationCreator = CreatePositionVectorTransformation;
				return true;
			}

			return false;
		}

		private ITransformation CreateTransform([NotNull] ICoordinateOperationInfo operationInfo, ICrs crsFrom, ICrs crsTo) {
			if (operationInfo.IsInverseOfDefinition && operationInfo.HasInverse) {
				var result = CreateTransform(operationInfo.GetInverse(), crsTo, crsFrom);
				if(result != null && result.HasInverse)
					return result.GetInverse();
			}

			var parameterizedOperationInfo = operationInfo as IParameterizedCoordinateOperationInfo;
			if (parameterizedOperationInfo == null)
				return null;

			var operationName = parameterizedOperationInfo.Method != null
				? parameterizedOperationInfo.Method.Name
				: parameterizedOperationInfo.Name;

			var operationParams = parameterizedOperationInfo.Parameters.ToArray();

			Func<OperationGenerationParams, ITransformation> transformationCreator;
			if (TryGetTransformCreator(operationName, out transformationCreator) && null != transformationCreator) {
				var result = transformationCreator(new OperationGenerationParams {
					OperationName = operationName,
					OperationNameNormalized = _coordinateOperationNameNormalizer.Normalize(operationName),
					ParamLookup = new NamedParameterLookup(operationParams),
					CrsFrom = crsFrom,
					CrsTo = crsTo
				});
				if (null != result)
					return result;
			}
			return null;
		}

		private Type ChooseGeometryType(ICrs crs) {
			if(crs is ICrsGeodetic)
				return ChooseGeometryType(crs as ICrsGeodetic);

			return null;
		}

		private Type ChooseGeometryType(ICrsGeodetic crs) {
			if (crs.Axes.Count == 2) {
				// 2D or geog
				if(crs is ICrsProjected) {
					return typeof(Point2);
				}
				return typeof(GeographicCoordinate);
			}
			if (crs.Axes.Count == 3) {
				// 3D or geog-H
				if(crs is ICrsGeocentric)
					return typeof(Point3);
				return typeof(GeographicHeightCoordinate);
			}
			return null;
		}

	}
}
