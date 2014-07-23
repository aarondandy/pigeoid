using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Pigeoid.CoordinateOperation.Projection;
using Pigeoid.CoordinateOperation.Transformation;
using Pigeoid.Interop;
using Pigeoid.Ogc;
using Pigeoid.Unit;
using Vertesaur;
using Vertesaur.Transformation;

namespace Pigeoid.CoordinateOperation
{
    public class StaticProjectionStepCompiler : StaticCoordinateOperationCompiler.IStepOperationCompiler
    {

        private class ProjectionCompilationParams
        {

            public ProjectionCompilationParams(
                StaticCoordinateOperationCompiler.StepCompilationParameters stepParams,
                NamedParameterLookup parameterLookup,
                string operationName
            ) {
                Contract.Requires(stepParams != null);
                Contract.Requires(parameterLookup != null);
                Contract.Requires(!String.IsNullOrEmpty(operationName));
                StepParams = stepParams;
                ParameterLookup = parameterLookup;
                OperationName = operationName;
            }

            [ContractInvariantMethod]
            private void CodeContractInvariants() {
                Contract.Invariant(StepParams != null);
                Contract.Invariant(ParameterLookup != null);
                Contract.Invariant(!String.IsNullOrEmpty(OperationName));
            }

            public StaticCoordinateOperationCompiler.StepCompilationParameters StepParams { get; private set; }
            public NamedParameterLookup ParameterLookup { get; private set; }
            public string OperationName { get; private set; }

        }

        private static readonly StaticProjectionStepCompiler DefaultValue = new StaticProjectionStepCompiler();
        public static StaticProjectionStepCompiler Default { get { return DefaultValue; } }

        private static bool TryConvert(IUnit from, IUnit to, ref double value) {
            if (null == from || null == to)
                return false;
            var conversion = SimpleUnitConversionGenerator.FindConversion(from, to);
            if (null == conversion)
                return false;

            value = conversion.TransformValue(value);
            return true;
        }

        private static bool TryGetDouble(INamedParameter parameter, out double value, double defaultValue = 0) {
            if (null != parameter && NamedParameter.TryGetDouble(parameter, out value)) {
                return true;
            }
            value = defaultValue;
            return false;
        }

        private static bool TryGetDouble(INamedParameter parameter, IUnit unit, out double value, double defaultValue = 0) {
            if (null != parameter && NamedParameter.TryGetDouble(parameter, out value)) {
                return TryConvert(parameter.Unit, unit, ref value);
            }
            value = defaultValue;
            return false;
        }

        private static bool TryCreateGeographicCoordinate(INamedParameter latParam, INamedParameter lonParam, out GeographicCoordinate result) {
            double lat, lon;
            if (TryGetDouble(latParam, OgcAngularUnit.DefaultRadians, out lat) | TryGetDouble(lonParam, OgcAngularUnit.DefaultRadians, out lon)) {
                result = new GeographicCoordinate(lat, lon);
                return true;
            }
            result = GeographicCoordinate.Zero;
            return false;
        }

        private static bool TryCreateVector2(INamedParameter xParam, INamedParameter yParam, out Vector2 result) {
            double x, y;
            if (TryGetDouble(xParam, out x) | TryGetDouble(yParam, out y)) {
                result = new Vector2(x, y);
                return true;
            }
            result = Vector2.Zero;
            return false;
        }

        private static bool TryCreateVector2(INamedParameter xParam, INamedParameter yParam, IUnit linearUnit, out Vector2 result) {
            double x, y;
            if (TryGetDouble(xParam, linearUnit, out x) | TryGetDouble(yParam, linearUnit, out y)) {
                result = new Vector2(x, y);
                return true;
            }
            result = Vector2.Zero;
            return false;
        }

        private static bool TryCreatePoint2(INamedParameter xParam, INamedParameter yParam, out Point2 result) {
            double x, y;
            if (TryGetDouble(xParam, out x) | TryGetDouble(yParam, out y)) {
                result = new Point2(x, y);
                return true;
            }
            result = Point2.Zero;
            return false;
        }

        private SpheroidProjectionBase CreatePolarStereographic(ProjectionCompilationParams opData) {
            Contract.Requires(opData != null);
            var latParam = new MultiParameterSelector(
                new LatitudeOfNaturalOriginParameterSelector(),
                new LatitudeOfTrueScaleParameterSelector());
            var originLonParam = new MultiParameterSelector(
                new LongitudeOfNaturalOriginParameterSelector(),
                new FullMatchParameterSelector("LONDOWNFROMPOLE"));// new KeywordNamedParameterSelector("LON", "ORIGIN");
            var offsetXParam = new FalseEastingParameterSelector();
            var offsetYParam = new FalseNorthingParameterSelector();
            var standardParallelParam = new StandardParallelParameterSelector();
            var scaleFactorParam = new ScaleFactorParameterSelector();
            opData.ParameterLookup.Assign(latParam, originLonParam, offsetXParam, offsetYParam, standardParallelParam, scaleFactorParam);

            var linearUnit = opData.StepParams.RelatedOutputCrsUnit;

            var fromSpheroid = opData.StepParams.ConvertRelatedOutputSpheroidUnit(linearUnit);
            if (null == fromSpheroid)
                return null;

            double latitude, longitude;
            if (!latParam.TryGetValueAsDouble(OgcAngularUnit.DefaultRadians, out latitude))
                latitude = 0;
            if (!originLonParam.TryGetValueAsDouble(OgcAngularUnit.DefaultRadians, out longitude))
                longitude = 0;

            double xOffset, yOffset;
            var offset = (offsetXParam.TryGetValueAsDouble(linearUnit, out xOffset) && offsetYParam.TryGetValueAsDouble(linearUnit, out yOffset))
                ? new Vector2(xOffset, yOffset)
                : Vector2.Zero;

            double scaleFactor;
            if (scaleFactorParam.TryGetValueAsDouble(ScaleUnitUnity.Value, out scaleFactor))
                return new PolarStereographic(new GeographicCoordinate(latitude,longitude), scaleFactor, offset, fromSpheroid);

            double standardParallel;
            if (standardParallelParam.TryGetValueAsDouble(OgcAngularUnit.DefaultRadians, out standardParallel)) {
                if (
                    (offsetXParam.IsSelected && opData.ParameterLookup.ParameterNameNormalizedComparer.Normalize(offsetXParam.Selection.Name).Contains("ORIGIN"))
                    || (offsetYParam.IsSelected && opData.ParameterLookup.ParameterNameNormalizedComparer.Normalize(offsetYParam.Selection.Name).Contains("ORIGIN"))
                ) {
                    return PolarStereographic.CreateFromStandardParallelAndFalseOffsetAtOrigin(longitude, standardParallel, offset, fromSpheroid);
                }
                return PolarStereographic.CreateFromStandardParallel(longitude, standardParallel, offset, fromSpheroid);
            }

            throw new InvalidOperationException();
        }

        private static SpheroidProjectionBase CreateTransverseMercator(ProjectionCompilationParams opData) {
            Contract.Requires(opData != null);
            var originLatParam = new LatitudeOfNaturalOriginParameterSelector();
            var originLonParam = new MultiParameterSelector(
                new CentralMeridianParameterSelector(),
                new LongitudeOfNaturalOriginParameterSelector());
            var offsetXParam = new FalseEastingParameterSelector();
            var offsetYParam = new FalseNorthingParameterSelector();
            var scaleFactorParam = new ScaleFactorParameterSelector();
            opData.ParameterLookup.Assign(originLatParam, originLonParam, offsetXParam, offsetYParam, scaleFactorParam);

            var fromSpheroid = opData.StepParams.ConvertRelatedOutputSpheroidUnit(opData.StepParams.RelatedOutputCrsUnit);
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

        private static SpheroidProjectionBase CreateTransverseMercatorSouth(ProjectionCompilationParams opData) {
            Contract.Requires(opData != null);
            var originLatParam = new LatitudeOfNaturalOriginParameterSelector();
            var originLonParam = new MultiParameterSelector(
                new CentralMeridianParameterSelector(),
                new LongitudeOfNaturalOriginParameterSelector());
            var offsetXParam = new FalseEastingParameterSelector();
            var offsetYParam = new FalseNorthingParameterSelector();
            var scaleFactorParam = new ScaleFactorParameterSelector();
            opData.ParameterLookup.Assign(originLatParam, originLonParam, offsetXParam, offsetYParam, scaleFactorParam);

            var fromSpheroid = opData.StepParams.ConvertRelatedOutputSpheroidUnit(opData.StepParams.RelatedOutputCrsUnit);
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
            Contract.Requires(opData != null);
            var originLatParam = new LatitudeOfNaturalOriginParameterSelector();
            var originLonParam = new LongitudeOfNaturalOriginParameterSelector();
            var offsetXParam = new FalseEastingParameterSelector();
            var offsetYParam = new FalseNorthingParameterSelector();
            if (!opData.ParameterLookup.Assign(originLatParam, originLonParam, offsetXParam, offsetYParam))
                return null;

            var spheroid = opData.StepParams.ConvertRelatedOutputSpheroidUnit(opData.StepParams.RelatedOutputCrsUnit);
            if (null == spheroid)
                return null;

            GeographicCoordinate origin;
            Vector2 offset;

            TryCreateGeographicCoordinate(originLatParam.Selection, originLonParam.Selection, out origin);
            TryCreateVector2(offsetXParam.Selection, offsetYParam.Selection, out offset);
            return new PopularVisualizationPseudoMercator(origin, offset, spheroid);
        }

        private SpheroidProjectionBase CreateLambertAzimuthalEqualArea(ProjectionCompilationParams opData) {
            Contract.Requires(opData != null);
            var originLatParam = new LatitudeOfNaturalOriginParameterSelector();
            var originLonParam = new LongitudeOfNaturalOriginParameterSelector();
            var offsetXParam = new FalseEastingParameterSelector();
            var offsetYParam = new FalseNorthingParameterSelector();
            opData.ParameterLookup.Assign(originLatParam, originLonParam, offsetXParam, offsetYParam);

            var spheroid = opData.StepParams.ConvertRelatedOutputSpheroidUnit(opData.StepParams.RelatedOutputCrsUnit);
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
                ? (SpheroidProjectionBase)new LambertAzimuthalEqualAreaSpherical(origin, offset, spheroid)
                : new LambertAzimuthalEqualArea(origin, offset, spheroid);
        }

        private SpheroidProjectionBase CreateEquidistantCylindrical(ProjectionCompilationParams opData) {
            Contract.Requires(opData != null);
            var originLatParam = new KeywordNamedParameterSelector("LAT", "PARALLEL");
            var originLonParam = new LongitudeOfNaturalOriginParameterSelector();
            var offsetXParam = new FalseEastingParameterSelector();
            var offsetYParam = new FalseNorthingParameterSelector();
            opData.ParameterLookup.Assign(originLatParam, originLonParam, offsetXParam, offsetYParam);

            var spheroid = opData.StepParams.ConvertRelatedOutputSpheroidUnit(opData.StepParams.RelatedOutputCrsUnit);
            if (null == spheroid)
                return null;

            GeographicCoordinate origin;
            if (!originLatParam.IsSelected || !originLonParam.IsSelected || !TryCreateGeographicCoordinate(originLatParam.Selection, originLonParam.Selection, out origin))
                origin = GeographicCoordinate.Zero;

            Vector2 offset;
            if (!offsetXParam.IsSelected || !offsetYParam.IsSelected || !TryCreateVector2(offsetXParam.Selection, offsetYParam.Selection, out offset))
                offset = Vector2.Zero;

            // ReSharper disable CompareOfFloatsByEqualityOperator
            var spherical = _coordinateOperationNameComparer.Normalize(opData.OperationName).EndsWith("SPHERICAL")
                || spheroid.E == 0;
            // ReSharper restore CompareOfFloatsByEqualityOperator

            return spherical
                ? (SpheroidProjectionBase)new EquidistantCylindricalSpherical(origin, offset, spheroid)
                : new EquidistantCylindrical(origin, offset, spheroid);
        }

        private ITransformation<GeographicCoordinate, Point2> CreateCassiniSoldner(ProjectionCompilationParams opData) {
            Contract.Requires(opData != null);
            var originLatParam = new LatitudeOfNaturalOriginParameterSelector();
            var originLonParam = new LongitudeOfNaturalOriginParameterSelector();
            var offsetXParam = new FalseEastingParameterSelector();
            var offsetYParam = new FalseNorthingParameterSelector();
            opData.ParameterLookup.Assign(originLatParam, originLonParam, offsetXParam, offsetYParam);

            var spheroid = opData.StepParams.ConvertRelatedOutputSpheroidUnit(opData.StepParams.RelatedOutputCrsUnit);
            if (null == spheroid)
                return null;

            GeographicCoordinate origin;
            if (!(originLatParam.IsSelected || originLonParam.IsSelected) || !TryCreateGeographicCoordinate(originLatParam.Selection, originLonParam.Selection, out origin))
                origin = GeographicCoordinate.Zero;

            Vector2 offset;
            if (!(offsetXParam.IsSelected || offsetYParam.IsSelected) || !TryCreateVector2(offsetXParam.Selection, offsetYParam.Selection, opData.StepParams.RelatedOutputCrsUnit, out offset))
                offset = Vector2.Zero;

            return new CassiniSoldner(origin, offset, spheroid);
        }

        private ITransformation<GeographicCoordinate, Point2> CreateMercator(ProjectionCompilationParams opData) {
            Contract.Requires(opData != null);
            var originLatParam = new MultiParameterSelector(
                new LatitudeOfNaturalOriginParameterSelector(),
                new LatitudeOfTrueScaleParameterSelector(),
                new StandardParallelParameterSelector());
            var originLonParam = new MultiParameterSelector(
                new CentralMeridianParameterSelector(),
                new LongitudeOfNaturalOriginParameterSelector());
            var offsetXParam = new FalseEastingParameterSelector();
            var offsetYParam = new FalseNorthingParameterSelector();
            var scaleFactorParam = new ScaleFactorParameterSelector();
            opData.ParameterLookup.Assign(originLatParam, originLonParam, offsetXParam, offsetYParam, scaleFactorParam);

            var spheroid = opData.StepParams.ConvertRelatedOutputSpheroidUnit(opData.StepParams.RelatedOutputCrsUnit);
            if (null == spheroid)
                return null;

            GeographicCoordinate origin;
            if (!(originLatParam.IsSelected || originLonParam.IsSelected) || !TryCreateGeographicCoordinate(originLatParam.Selection, originLonParam.Selection, out origin))
                origin = GeographicCoordinate.Zero;

            Vector2 offset;
            if (!(offsetXParam.IsSelected || offsetYParam.IsSelected) || !TryCreateVector2(offsetXParam.Selection, offsetYParam.Selection, opData.StepParams.RelatedOutputCrsUnit, out offset))
                offset = Vector2.Zero;

            // ReSharper disable CompareOfFloatsByEqualityOperator
            double scaleFactor;
            if (scaleFactorParam.IsSelected && TryGetDouble(scaleFactorParam.Selection, ScaleUnitUnity.Value, out scaleFactor)) {
                if (1 == scaleFactor && 0 == spheroid.E)
                    return new MercatorSpherical(offset, origin.Longitude, spheroid.A);
                return new Mercator(origin.Longitude, scaleFactor, offset, spheroid);
            }

            if (0 == spheroid.E)
                return new MercatorSpherical(offset, origin.Longitude, spheroid.A);
            return new Mercator(origin, offset, spheroid);
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        private ITransformation<GeographicCoordinate, Point2> CreateLambertConicConformal(ProjectionCompilationParams opData) {
            Contract.Requires(opData != null);
            var originLatParam = new KeywordNamedParameterSelector("LAT", "ORIGIN");
            var originLonParam = new KeywordNamedParameterSelector("LON", "ORIGIN", "CENTRAL", "MERIDIAN");
            var parallel1Param = new KeywordNamedParameterSelector("LAT", "1", "PARALLEL");
            var parallel2Param = new KeywordNamedParameterSelector("LAT", "2", "PARALLEL");
            var offsetXParam = new FalseEastingParameterSelector();
            var offsetYParam = new FalseNorthingParameterSelector();
            var scaleFactorParam = new ScaleFactorParameterSelector();
            opData.ParameterLookup.Assign(originLatParam, originLonParam, offsetXParam, offsetYParam, scaleFactorParam, parallel1Param, parallel2Param);

            var spheroid = opData.StepParams.ConvertRelatedOutputSpheroidUnit(opData.StepParams.RelatedOutputCrsUnit);
            if (null == spheroid)
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
                    return new LambertConicConformalBelgium(origin, parallel1, parallel2, offset, spheroid);

                return new LambertConicConformal2Sp(origin, parallel1, parallel2, offset, spheroid);
            }

            if (scaleFactorParam.IsSelected){
                double scaleFactor;
                if(TryGetDouble(scaleFactorParam.Selection, ScaleUnitUnity.Value, out scaleFactor))
                    return new LambertConicConformal1Sp(origin, scaleFactor, offset, spheroid);
            }

            return null;
        }

        private SpheroidProjectionBase CreateLambertConicNearConformal(ProjectionCompilationParams opData) {
            Contract.Requires(opData != null);
            var originLatParam = new LatitudeOfNaturalOriginParameterSelector();
            var originLonParam = new LongitudeOfNaturalOriginParameterSelector();
            var offsetXParam = new FalseEastingParameterSelector();
            var offsetYParam = new FalseNorthingParameterSelector();
            var scaleFactorParam = new ScaleFactorParameterSelector();
            opData.ParameterLookup.Assign(originLatParam, originLonParam, offsetXParam, offsetYParam, scaleFactorParam);

            var spheroid = opData.StepParams.ConvertRelatedOutputSpheroidUnit(opData.StepParams.RelatedOutputCrsUnit);
            if (null == spheroid)
                return null;

            GeographicCoordinate origin;
            if (!(originLatParam.IsSelected || originLonParam.IsSelected) || !TryCreateGeographicCoordinate(originLatParam.Selection, originLonParam.Selection, out origin))
                origin = GeographicCoordinate.Zero;

            Vector2 offset;
            if (!(offsetXParam.IsSelected || offsetYParam.IsSelected) || !TryCreateVector2(offsetXParam.Selection, offsetYParam.Selection, opData.StepParams.RelatedOutputCrsUnit, out offset))
                offset = Vector2.Zero;

            double scaleFactor;
            if (scaleFactorParam.IsSelected && TryGetDouble(scaleFactorParam.Selection, ScaleUnitUnity.Value, out scaleFactor))
                return new LambertConicNearConformal(origin, scaleFactor, offset, spheroid);

            return null;
        }

        private ITransformation<GeographicCoordinate, Point2> CreateKrovak(ProjectionCompilationParams opData) {
            Contract.Requires(opData != null);
            var latConeAxisParam = new KeywordNamedParameterSelector("CO", "LAT", "CONE", "AXIS"); // Co-latitude of cone axis
            var latProjectionCenterParam = new KeywordNamedParameterSelector("LAT", "CENTER"); // Latitude of projection centre
            var latPseudoParallelParam = new KeywordNamedParameterSelector("LAT", "PSEUDO", "PARALLEL"); // Latitude of pseudo standard parallel
            var scaleFactorParallelParam = new KeywordNamedParameterSelector("SCALE", "PARALLEL"); // Scale factor on pseudo standard parallel
            var originLonParam = new KeywordNamedParameterSelector("LON", "ORIGIN"); // Longitude of origin
            var offsetXParam = new FalseEastingParameterSelector();
            var offsetYParam = new FalseNorthingParameterSelector();
            var evalXParam = new KeywordNamedParameterSelector("ORDINATE1", "EVALUATION", "POINT");
            var evalYParam = new KeywordNamedParameterSelector("ORDINATE2", "EVALUATION", "POINT");
            opData.ParameterLookup.Assign(latConeAxisParam, latProjectionCenterParam, latPseudoParallelParam, scaleFactorParallelParam, originLonParam, offsetXParam, offsetYParam, evalXParam, evalYParam);

            var spheroid = opData.StepParams.ConvertRelatedOutputSpheroidUnit(opData.StepParams.RelatedOutputCrsUnit);
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
                    Contract.Assume(constants.Length == constantParams.Length);
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

        private SpheroidProjectionBase CreateObliqueStereographic(ProjectionCompilationParams opData) {
            Contract.Requires(opData != null);
            var originLatParam = new LatitudeOfNaturalOriginParameterSelector();
            var originLonParam = new LongitudeOfNaturalOriginParameterSelector();
            var offsetXParam = new FalseEastingParameterSelector();
            var offsetYParam = new FalseNorthingParameterSelector();
            var scaleFactorParam = new ScaleFactorParameterSelector();
            opData.ParameterLookup.Assign(originLatParam, originLonParam, offsetXParam, offsetYParam, scaleFactorParam);

            var spheroid = opData.StepParams.ConvertRelatedOutputSpheroidUnit(opData.StepParams.RelatedOutputCrsUnit);
            if (null == spheroid)
                return null;

            GeographicCoordinate origin;
            if (!(originLatParam.IsSelected || originLonParam.IsSelected) || !TryCreateGeographicCoordinate(originLatParam.Selection, originLonParam.Selection, out origin))
                origin = GeographicCoordinate.Zero;

            Vector2 offset;
            if (!(offsetXParam.IsSelected || offsetYParam.IsSelected) || !TryCreateVector2(offsetXParam.Selection, offsetYParam.Selection, opData.StepParams.RelatedOutputCrsUnit, out offset))
                offset = Vector2.Zero;

            double scaleFactor;
            if (scaleFactorParam.IsSelected && TryGetDouble(scaleFactorParam.Selection, ScaleUnitUnity.Value, out scaleFactor))
                return new ObliqueStereographic(origin, scaleFactor, offset, spheroid);

            return null;
        }

        private static SpheroidProjectionBase CreateGuam(ProjectionCompilationParams opData) {
            Contract.Requires(opData != null);
            var originLatParam = new LatitudeOfNaturalOriginParameterSelector();
            var originLonParam = new LongitudeOfNaturalOriginParameterSelector();
            var offsetXParam = new FalseEastingParameterSelector();
            var offsetYParam = new FalseNorthingParameterSelector();
            opData.ParameterLookup.Assign(originLatParam, originLonParam, offsetXParam, offsetYParam);

            var spheroid = opData.StepParams.ConvertRelatedOutputSpheroidUnit(opData.StepParams.RelatedOutputCrsUnit);
            if (null == spheroid)
                return null;

            GeographicCoordinate origin;
            if (!(originLatParam.IsSelected || originLonParam.IsSelected) || !TryCreateGeographicCoordinate(originLatParam.Selection, originLonParam.Selection, out origin))
                origin = GeographicCoordinate.Zero;

            Vector2 offset;
            if (!(offsetXParam.IsSelected || offsetYParam.IsSelected) || !TryCreateVector2(offsetXParam.Selection, offsetYParam.Selection, opData.StepParams.RelatedOutputCrsUnit, out offset))
                offset = Vector2.Zero;

            return new Guam(origin, offset, spheroid);
        }

        private static SpheroidProjectionBase CreateHyperbolicCassiniSoldner(ProjectionCompilationParams opData) {
            Contract.Requires(opData != null);
            var originLatParam = new LatitudeOfNaturalOriginParameterSelector();
            var originLonParam = new LongitudeOfNaturalOriginParameterSelector();
            var offsetXParam = new FalseEastingParameterSelector();
            var offsetYParam = new FalseNorthingParameterSelector();
            opData.ParameterLookup.Assign(originLatParam, originLonParam, offsetXParam, offsetYParam);

            var spheroid = opData.StepParams.ConvertRelatedOutputSpheroidUnit(opData.StepParams.RelatedOutputCrsUnit);
            if (null == spheroid)
                return null;

            GeographicCoordinate origin;
            if (!(originLatParam.IsSelected || originLonParam.IsSelected) || !TryCreateGeographicCoordinate(originLatParam.Selection, originLonParam.Selection, out origin))
                origin = GeographicCoordinate.Zero;

            Vector2 offset;
            if (!(offsetXParam.IsSelected || offsetYParam.IsSelected) || !TryCreateVector2(offsetXParam.Selection, offsetYParam.Selection, opData.StepParams.RelatedOutputCrsUnit, out offset))
                offset = Vector2.Zero;

            return new HyperbolicCassiniSoldner(origin, offset, spheroid);
        }

        private static SpheroidProjectionBase CreateModifiedAzimuthalEquidistant(ProjectionCompilationParams opData) {
            Contract.Requires(opData != null);
            var originLatParam = new LatitudeOfNaturalOriginParameterSelector();
            var originLonParam = new LongitudeOfNaturalOriginParameterSelector();
            var offsetXParam = new FalseEastingParameterSelector();
            var offsetYParam = new FalseNorthingParameterSelector();
            opData.ParameterLookup.Assign(originLatParam, originLonParam, offsetXParam, offsetYParam);

            var spheroid = opData.StepParams.ConvertRelatedOutputSpheroidUnit(opData.StepParams.RelatedOutputCrsUnit);
            if (null == spheroid)
                return null;

            GeographicCoordinate origin;
            if (!(originLatParam.IsSelected || originLonParam.IsSelected) || !TryCreateGeographicCoordinate(originLatParam.Selection, originLonParam.Selection, out origin))
                origin = GeographicCoordinate.Zero;

            Vector2 offset;
            if (!(offsetXParam.IsSelected || offsetYParam.IsSelected) || !TryCreateVector2(offsetXParam.Selection, offsetYParam.Selection, opData.StepParams.RelatedOutputCrsUnit, out offset))
                offset = Vector2.Zero;

            return new ModifiedAzimuthalEquidistant(origin, offset, spheroid);
        }

        private static SpheroidProjectionBase CreateLambertCylindricalEqualAreaSpherical(ProjectionCompilationParams opData) {
            Contract.Requires(opData != null);
            var originLatParam = new MultiParameterSelector(
                new LatitudeOfNaturalOriginParameterSelector(),
                new StandardParallelParameterSelector());
            var originLonParam = new LongitudeOfNaturalOriginParameterSelector();
            var offsetXParam = new FalseEastingParameterSelector();
            var offsetYParam = new FalseNorthingParameterSelector();
            opData.ParameterLookup.Assign(originLatParam, originLonParam, offsetXParam, offsetYParam);

            var spheroid = opData.StepParams.ConvertRelatedOutputSpheroidUnit(opData.StepParams.RelatedOutputCrsUnit);
            if (null == spheroid)
                return null;

            GeographicCoordinate origin;
            if (!(originLatParam.IsSelected || originLonParam.IsSelected) || !TryCreateGeographicCoordinate(originLatParam.Selection, originLonParam.Selection, out origin))
                origin = GeographicCoordinate.Zero;

            Vector2 offset;
            if (!(offsetXParam.IsSelected || offsetYParam.IsSelected) || !TryCreateVector2(offsetXParam.Selection, offsetYParam.Selection, opData.StepParams.RelatedOutputCrsUnit, out offset))
                offset = Vector2.Zero;

            return new LambertCylindricalEqualAreaSpherical(origin, offset, spheroid);
        }

        private SpheroidProjectionBase CreateLabordeObliqueMercator(ProjectionCompilationParams opData) {
            Contract.Requires(opData != null);
            var originLatParam = new KeywordNamedParameterSelector("LAT", "CENTER");
            var originLonParam = new KeywordNamedParameterSelector("LON", "CENTER");
            var offsetXParam = new FalseEastingParameterSelector();
            var offsetYParam = new FalseNorthingParameterSelector();
            var scaleFactorParam = new ScaleFactorParameterSelector();
            var azimuthParam = new KeywordNamedParameterSelector("AZIMUTH");
            opData.ParameterLookup.Assign(originLatParam, originLonParam, offsetXParam, offsetYParam, scaleFactorParam, azimuthParam);

            var spheroid = opData.StepParams.ConvertRelatedOutputSpheroidUnit(opData.StepParams.RelatedOutputCrsUnit);
            if (null == spheroid)
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

            return new LabordeObliqueMercator(origin, azimuth, scaleFactor, spheroid, offset);
        }

        private SpheroidProjectionBase CreateHotineObliqueMercator(ProjectionCompilationParams opData) {
            Contract.Requires(opData != null);

            var latParam = new MultiParameterSelector(
                new LatitudeOfCenterParameterSelector(),
                new LatitudeOfNaturalOriginParameterSelector());
            var lonParam = new MultiParameterSelector(
                new LongitudeOfCenterParameterSelector(),
                new LongitudeOfNaturalOriginParameterSelector());

            var scaleFactorParam = new ScaleFactorParameterSelector();
            var azimuthParam = new KeywordNamedParameterSelector("AZIMUTH");
            var angleSkewParam = new KeywordNamedParameterSelector("ANGLE", "SKEW");

            var offsetXParam = new FalseEastingParameterSelector();
            var offsetYParam = new FalseNorthingParameterSelector();
            var xAtOriginParam = new EastingAtCenterParameterSelector();
            var yAtOriginParam = new NorthingAtCenterParameterSelector();

            opData.ParameterLookup.Assign(latParam, lonParam, offsetXParam, offsetYParam, xAtOriginParam, yAtOriginParam, scaleFactorParam, azimuthParam, angleSkewParam);

            var spheroid = opData.StepParams.ConvertRelatedOutputSpheroidUnit(opData.StepParams.RelatedOutputCrsUnit);
            if (null == spheroid)
                return null;

            double lat, lon;
            if(!latParam.IsSelected || !latParam.TryGetValueAsDouble(OgcAngularUnit.DefaultRadians, out lat))
                lat = 0;
            if(!lonParam.IsSelected || !lonParam.TryGetValueAsDouble(OgcAngularUnit.DefaultRadians, out lon))
                lon = 0;
            var origin = new GeographicCoordinate(lat, lon);

            double scaleFactor;
            if (!scaleFactorParam.IsSelected || !TryGetDouble(scaleFactorParam.Selection, ScaleUnitUnity.Value, out scaleFactor))
                scaleFactor = 1.0;

            double azimuth;
            if (!azimuthParam.IsSelected || !TryGetDouble(azimuthParam.Selection, OgcAngularUnit.DefaultRadians, out azimuth))
                azimuth = 0;

            double angleSkew;
            if (!angleSkewParam.IsSelected || !TryGetDouble(angleSkewParam.Selection, OgcAngularUnit.DefaultRadians, out angleSkew))
                angleSkew = 0;

            var outputUnit = opData.StepParams.RelatedOutputCrsUnit;
            double x,y;
            if(xAtOriginParam.IsSelected || yAtOriginParam.IsSelected){
                if (!xAtOriginParam.IsSelected || !xAtOriginParam.TryGetValueAsDouble(outputUnit, out x))
                    x = 0;
                if (!yAtOriginParam.IsSelected || !yAtOriginParam.TryGetValueAsDouble(outputUnit, out y))
                    y = 0;
                return new HotineObliqueMercator.VariantB(new GeographicCoordinate(lat, lon), azimuth, angleSkew, scaleFactor, new Vector2(x,y), spheroid);
            }
            else {
                if (!offsetXParam.IsSelected || !offsetXParam.TryGetValueAsDouble(outputUnit, out x))
                    x = 0;
                if (!offsetYParam.IsSelected || !offsetYParam.TryGetValueAsDouble(outputUnit, out y))
                    y = 0;
                return new HotineObliqueMercator.VariantA(new GeographicCoordinate(lat, lon), azimuth, angleSkew, scaleFactor, new Vector2(x,y), spheroid);
            }
        }

        public StaticProjectionStepCompiler(INameNormalizedComparer coordinateOperationNameComparer = null) {
            _coordinateOperationNameComparer = coordinateOperationNameComparer ?? CoordinateOperationNameNormalizedComparer.Default;
            _transformationCreatorLookup = new Dictionary<string, Func<ProjectionCompilationParams, ITransformation<GeographicCoordinate, Point2>>>(_coordinateOperationNameComparer) {
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
                {CoordinateOperationStandardNames.Guam,CreateGuam},
                {CoordinateOperationStandardNames.HotineObliqueMercator,CreateHotineObliqueMercator},
                {CoordinateOperationStandardNames.HyperbolicCassiniSoldner,CreateHyperbolicCassiniSoldner},
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
                {CoordinateOperationStandardNames.LambertCylindricalEqualAreaSpherical, CreateLambertCylindricalEqualAreaSpherical},
                {CoordinateOperationStandardNames.Mercator1Sp,CreateMercator},
                {CoordinateOperationStandardNames.Mercator2Sp,CreateMercator},
                {CoordinateOperationStandardNames.MillerCylindrical,null},
                {CoordinateOperationStandardNames.ModifiedAzimuthalEquidistant,CreateModifiedAzimuthalEquidistant},
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

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(_coordinateOperationNameComparer != null);
            Contract.Invariant(_transformationCreatorLookup != null);
        }

        private readonly INameNormalizedComparer _coordinateOperationNameComparer;
        private readonly Dictionary<string, Func<ProjectionCompilationParams, ITransformation<GeographicCoordinate, Point2>>> _transformationCreatorLookup;

        public StaticCoordinateOperationCompiler.StepCompilationResult Compile(StaticCoordinateOperationCompiler.StepCompilationParameters stepParameters) {
            if(stepParameters == null) throw new ArgumentNullException("stepParameters");
            Contract.EndContractBlock();
            return CompileInverse(stepParameters)
                ?? CompileForwards(stepParameters);
        }

        private StaticCoordinateOperationCompiler.StepCompilationResult CompileForwards(StaticCoordinateOperationCompiler.StepCompilationParameters stepParameters) {
            Contract.Requires(stepParameters != null);
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

        private StaticCoordinateOperationCompiler.StepCompilationResult CompileInverse(StaticCoordinateOperationCompiler.StepCompilationParameters stepParameters) {
            Contract.Requires(stepParameters != null);
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
            if (null != desiredInputUnits && !UnitEqualityComparer.Default.Equals(actualInputUnits, desiredInputUnits)) {
                var conv = SimpleUnitConversionGenerator.FindConversion(actualInputUnits, desiredInputUnits);
                if (null != conv) {
                    var conversionTransformation = new LinearElementTransformation(conv);
                    resultTransformation = new ConcatenatedTransformation(new[] { conversionTransformation, resultTransformation });
                }
            }

            // the result units will be radians
            return new StaticCoordinateOperationCompiler.StepCompilationResult(
                stepParameters,
                OgcAngularUnit.DefaultRadians,
                resultTransformation
            );
        }

        private ITransformation<GeographicCoordinate, Point2> CompileForwardToTransform(StaticCoordinateOperationCompiler.StepCompilationParameters stepParameters) {
            Contract.Requires(stepParameters != null);
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

            Func<ProjectionCompilationParams, ITransformation<GeographicCoordinate, Point2>> projectionCompiler;
            if (String.IsNullOrEmpty(operationName) || !_transformationCreatorLookup.TryGetValue(operationName, out projectionCompiler) || null == projectionCompiler)
                return null;

            var parameterLookup = new NamedParameterLookup(parameters ?? Enumerable.Empty<INamedParameter>());
            var projectionCompilationParams = new ProjectionCompilationParams(stepParameters, parameterLookup, operationName);

            return projectionCompiler(projectionCompilationParams);
        }



    }
}
