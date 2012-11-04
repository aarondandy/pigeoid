using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pigeoid.Contracts;
using Pigeoid.Interop;
using Pigeoid.Projection;
using Pigeoid.Transformation;
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
					x => ParameterNameComparer.Default.Normalize(x.Name));
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
			if(NamedParameter.TryGetDouble(latParam, out lat) && NamedParameter.TryGetDouble(lonParam, out lon)) {
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

		private static GeographicGeocentricTranslation CreateGeographicTransformation(OperationGenerationParams opData) {
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

		private readonly Dictionary<string, Func<OperationGenerationParams, ITransformation>> _transformationCreator;

		public BasicCoordinateOperationToTransformationGenerator() {
			_transformationCreator = new Dictionary<string, Func<OperationGenerationParams, ITransformation>>(CoordinateOperationNameComparer.Default) {
				{CoordinateOperationStandardNames.AlbersEqualAreaConic,null},
				{CoordinateOperationStandardNames.AzimuthalEquidistant,null},
				{CoordinateOperationStandardNames.CassiniSoldner, CreateCassiniSoldner},
				{CoordinateOperationStandardNames.CylindricalEqualArea,null},
				{CoordinateOperationStandardNames.Eckert4,null},
				{CoordinateOperationStandardNames.Eckert6,null},
				{CoordinateOperationStandardNames.EquidistantConic,null},
				{CoordinateOperationStandardNames.Equirectangular,null},
				{CoordinateOperationStandardNames.GallStereographic,null},
				{CoordinateOperationStandardNames.GeocentricTranslationsGeog2D, CreateGeographicTransformation},
				{CoordinateOperationStandardNames.GeographicOffsets, CreateGeographicOffset},
				{CoordinateOperationStandardNames.Geos,null},
				{CoordinateOperationStandardNames.Gnomonic,null},
				{CoordinateOperationStandardNames.HotineObliqueMercator,null},
				{CoordinateOperationStandardNames.KrovakObliqueConicConformal,null},
				{CoordinateOperationStandardNames.LabordeObliqueMercator,CreateLabordeObliqueMercator},
				{CoordinateOperationStandardNames.LambertAzimuthalEqualArea,null},
				{CoordinateOperationStandardNames.LambertConicConformal1Sp,null},
				{CoordinateOperationStandardNames.LambertConicConformal2Sp,null},
				{CoordinateOperationStandardNames.Mercator1Sp,CreateMercator},
				{CoordinateOperationStandardNames.Mercator2Sp,CreateMercator},
				{CoordinateOperationStandardNames.MercatorVariantA,CreateMercator},
				{CoordinateOperationStandardNames.MercatorVariantB,CreateMercator},
				{CoordinateOperationStandardNames.MercatorVariantC,CreateMercator},
				{CoordinateOperationStandardNames.MillerCylindrical,null},
				{CoordinateOperationStandardNames.Mollweide,null},
				{CoordinateOperationStandardNames.NewZealandMapGrid,null},
				{CoordinateOperationStandardNames.ObliqueMercator,null},
				{CoordinateOperationStandardNames.ObliqueStereographic,null},
				{CoordinateOperationStandardNames.Orthographic,null},
				{CoordinateOperationStandardNames.PolarStereographic,null},
				{CoordinateOperationStandardNames.Polyconic,null},
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

		private bool TryExtractSpheroid(NamedParameterLookup namedParameters, out ISpheroid<double> result) {
			throw new NotImplementedException();
		}

		private bool TryExtractFalseProjectedOffset(NamedParameterLookup namedParameters, out Vector2 result) {
			throw new NotImplementedException();
		}

		private bool TryExtractNaturalOrigin(NamedParameterLookup namedParameters, out GeographicCoordinate result) {
			throw new NotImplementedException();
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
			if(_transformationCreator.TryGetValue(operationName, out transformationCreator) && null != transformationCreator) {
				var result = transformationCreator(new OperationGenerationParams {
					OperationName = operationName,
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
