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
    internal class StaticTransformationStepCompiler : StaticCoordinateOperationCompiler.IStepOperationCompiler
    {

        private class TransformationCompilationParams
        {

            public TransformationCompilationParams(
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

        private static bool TryCreatePoint2(INamedParameter xParam, INamedParameter yParam, out Point2 result) {
            return TryCreatePoint2(xParam, yParam, null, out result);
        }

        private static bool TryCreatePoint2(INamedParameter xParam, INamedParameter yParam, IUnit linearUnit, out Point2 result) {
            double x, y;
            if (NamedParameter.TryGetDouble(xParam, out x) && NamedParameter.TryGetDouble(yParam, out y)) {
                if (null != linearUnit) {
                    ConvertIfVaild(xParam.Unit, linearUnit, ref x);
                    ConvertIfVaild(yParam.Unit, linearUnit, ref y);
                }
                result = new Point2(x, y);
                return true;
            }
            result = Point2.Invalid;
            return false;
        }

        private static bool TryCreatePoint3(INamedParameter xParam, INamedParameter yParam, INamedParameter zParam, out Point3 result) {
            return TryCreatePoint3(xParam, yParam, zParam, null, out result);
        }

        private static bool TryCreatePoint3(INamedParameter xParam, INamedParameter yParam, INamedParameter zParam, IUnit linearUnit, out Point3 result) {
            double x, y, z;
            if (NamedParameter.TryGetDouble(xParam, out x) && NamedParameter.TryGetDouble(yParam, out y) && NamedParameter.TryGetDouble(zParam, out z)) {
                if (null != linearUnit) {
                    ConvertIfVaild(xParam.Unit, linearUnit, ref x);
                    ConvertIfVaild(yParam.Unit, linearUnit, ref y);
                    ConvertIfVaild(zParam.Unit, linearUnit, ref z);
                }
                result = new Point3(x, y, z);
                return true;
            }
            result = Point3.Invalid;
            return false;
        }

        private static bool TryCreateVector3(INamedParameter xParam, INamedParameter yParam, INamedParameter zParam, out Vector3 result) {
            return TryCreateVector3(xParam, yParam, zParam, null, out result);
        }

        private static bool TryCreateVector3(INamedParameter xParam, INamedParameter yParam, INamedParameter zParam, IUnit linearUnit, out Vector3 result) {
            double x, y, z;
            if (NamedParameter.TryGetDouble(xParam, out x) && NamedParameter.TryGetDouble(yParam, out y) && NamedParameter.TryGetDouble(zParam, out z)) {
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

        private static bool TryCreateGeographicHeightCoordinate(INamedParameter latParam, INamedParameter lonParam, INamedParameter heightParam, IUnit angularUnit, IUnit linearUnit, out GeographicHeightCoordinate result) {
            double lat, lon, h;
            if (TryGetDouble(latParam, angularUnit, out lat) | TryGetDouble(lonParam, angularUnit, out lon) | TryGetDouble(heightParam, linearUnit, out h)) {
                result = new GeographicHeightCoordinate(lat, lon, h);
                return true;
            }
            result = GeographicHeightCoordinate.Invalid;
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

        private static bool ExtractHelmertParams(TransformationCompilationParams opData, out Vector3 translation, out Vector3 rotation, out double scale) {
            Contract.Requires(opData != null);
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

            if (!opData.ParameterLookup.Assign(xTransParam, yTransParam, zTransParam, xRotParam, yRotParam, zRotParam, scaleParam))
                return false;
            if (!TryCreateVector3(xTransParam.Selection, yTransParam.Selection, zTransParam.Selection, out translation))
                return false;
            if (!TryCreateVector3(xRotParam.Selection, yRotParam.Selection, zRotParam.Selection, OgcAngularUnit.DefaultArcSeconds, out rotation))
                return false;
            if (!NamedParameter.TryGetDouble(scaleParam.Selection, out scale))
                return false;

            ConvertIfVaild(scaleParam.Selection.Unit, ScaleUnitPartsPerMillion.Value, ref scale);
            return true;
        }

        private static StaticCoordinateOperationCompiler.StepCompilationResult CreateHelmertStep(TransformationCompilationParams opData, Helmert7Transformation helmert) {
            Contract.Requires(opData != null);
            Contract.Requires(helmert != null);
            if (opData.StepParams.RelatedInputCrs is ICrsGeocentric && opData.StepParams.RelatedOutputCrs is ICrsGeocentric)
                return new StaticCoordinateOperationCompiler.StepCompilationResult(
                    opData.StepParams,
                    opData.StepParams.RelatedOutputCrsUnit ?? opData.StepParams.RelatedInputCrsUnit ?? opData.StepParams.InputUnit,
                    helmert);

            var spheroidFrom = ExtractSpheroid(opData.StepParams.RelatedInputCrs) ?? ExtractSpheroid(opData.StepParams.RelatedOutputCrs);
            if (null == spheroidFrom)
                return null;

            var spheroidTo = ExtractSpheroid(opData.StepParams.RelatedOutputCrs) ?? spheroidFrom;

            ITransformation transformation = new GeocentricTransformationGeographicWrapper(spheroidFrom, spheroidTo, helmert);
            var unitConversion = StaticCoordinateOperationCompiler.CreateCoordinateUnitConversion(opData.StepParams.InputUnit, OgcAngularUnit.DefaultRadians);
            if (null != unitConversion)
                transformation = new ConcatenatedTransformation(new[] { unitConversion, transformation });

            return new StaticCoordinateOperationCompiler.StepCompilationResult(
                opData.StepParams,
                OgcAngularUnit.DefaultRadians,
                transformation);
        }

        private static StaticCoordinateOperationCompiler.StepCompilationResult CreatePositionVectorTransformation(TransformationCompilationParams opData) {
            Contract.Requires(opData != null);
            Vector3 translation, rotation;
            double scale;
            if (!ExtractHelmertParams(opData, out translation, out rotation, out scale))
                return null;

            return CreateHelmertStep(opData, Helmert7Transformation.CreatePositionVectorFormat(translation, rotation, scale));
        }

        private static StaticCoordinateOperationCompiler.StepCompilationResult CreateCoordinateFrameRotationTransformation(TransformationCompilationParams opData) {
            Contract.Requires(opData != null);
            Vector3 translation, rotation;
            double scale;
            if (!ExtractHelmertParams(opData, out translation, out rotation, out scale))
                return null;

            return CreateHelmertStep(opData, Helmert7Transformation.CreateCoordinateFrameRotationFormat(translation, rotation, scale));
        }

        private static StaticCoordinateOperationCompiler.StepCompilationResult CreateGeographicOffset(TransformationCompilationParams opData) {
            Contract.Requires(opData != null);
            var latParam = new KeywordNamedParameterSelector("LAT");
            var lonParam = new KeywordNamedParameterSelector("LON");
            var heightParam = new KeywordNamedParameterSelector("HEIGHT");
            opData.ParameterLookup.Assign(latParam, lonParam);


            var deltaLatitude = 0.0;
            var deltaLongitude = 0.0;
            var deltaHeight = 0.0;

            if (!latParam.IsSelected && !lonParam.IsSelected && !heightParam.IsSelected)
                return null;

            if (latParam.IsSelected)
                TryGetDouble(latParam.Selection, opData.StepParams.InputUnit, out deltaLatitude);
            if (lonParam.IsSelected)
                TryGetDouble(lonParam.Selection, opData.StepParams.InputUnit, out deltaLongitude);
            if (heightParam.IsSelected)
                NamedParameter.TryGetDouble(heightParam.Selection, out deltaHeight);

            var transformation = new GeographicOffset(deltaLatitude, deltaLongitude, deltaHeight);

            return new StaticCoordinateOperationCompiler.StepCompilationResult(
                opData.StepParams,
                opData.StepParams.InputUnit,
                transformation);
        }

        private static StaticCoordinateOperationCompiler.StepCompilationResult CreateGeocentricTranslation(TransformationCompilationParams opData) {
            Contract.Requires(opData != null);
            var xParam = new KeywordNamedParameterSelector("XAXIS", "X");
            var yParam = new KeywordNamedParameterSelector("YAXIS", "Y");
            var zParam = new KeywordNamedParameterSelector("ZAXIS", "Z");
            opData.ParameterLookup.Assign(xParam, yParam, zParam);

            Vector3 delta;
            if (!TryCreateVector3(xParam.Selection, yParam.Selection, zParam.Selection, out delta))
                return null;

            if (opData.StepParams.RelatedInputCrs is ICrsGeocentric && opData.StepParams.RelatedOutputCrs is ICrsGeocentric) {
                // assume the units are correct
                return new StaticCoordinateOperationCompiler.StepCompilationResult(
                    opData.StepParams,
                    opData.StepParams.RelatedOutputCrsUnit ?? opData.StepParams.RelatedInputCrsUnit ?? opData.StepParams.InputUnit,
                    new GeocentricTranslation(delta));
            }

            // TODO: need to handle unit here
            var inputSpheroid = opData.StepParams.RelatedInputSpheroid;
            if (null == inputSpheroid)
                return null;

            // TODO: may even need to convert units while translating

            // TODO: need to handle unit here
            var outputSpheroid = opData.StepParams.RelatedOutputSpheroid;
            if (null == outputSpheroid)
                return null;

            ITransformation transformation = new GeographicGeocentricTranslation(inputSpheroid, delta, outputSpheroid);
            var conv = StaticCoordinateOperationCompiler.CreateCoordinateUnitConversion(opData.StepParams.InputUnit, OgcAngularUnit.DefaultRadians);
            if (null != conv)
                transformation = new ConcatenatedTransformation(new[] { conv, transformation });
            return new StaticCoordinateOperationCompiler.StepCompilationResult(
                opData.StepParams,
                OgcAngularUnit.DefaultRadians,
                transformation);
        }

        private static StaticCoordinateOperationCompiler.StepCompilationResult CreateVerticalOffset(TransformationCompilationParams opData) {
            Contract.Requires(opData != null);
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

        private static StaticCoordinateOperationCompiler.StepCompilationResult CreateVerticalPerspective(TransformationCompilationParams opData) {
            Contract.Requires(opData != null);
            var outputUnit = opData.StepParams.RelatedOutputCrsUnit;
            if (null == outputUnit)
                return null;

            var latParam = new KeywordNamedParameterSelector("LAT");
            var lonParam = new KeywordNamedParameterSelector("LON");
            var hOriginParam = new KeywordNamedParameterSelector("H", "HEIGHT", "ORIGIN");
            var hViewParam = new KeywordNamedParameterSelector("H", "HEIGHT", "VIEW");
            opData.ParameterLookup.Assign(latParam, lonParam, hOriginParam, hViewParam);

            GeographicHeightCoordinate origin;
            if (!TryCreateGeographicHeightCoordinate(latParam.Selection, lonParam.Selection, hOriginParam.Selection, OgcAngularUnit.DefaultRadians, outputUnit, out origin))
                origin = GeographicHeightCoordinate.Zero;

            double viewHeight;
            if (!TryGetDouble(hViewParam.Selection, outputUnit, out viewHeight))
                viewHeight = Double.NaN;

            var spheroidIn = opData.StepParams.ConvertRelatedInputSpheroidUnit(outputUnit);
            if (null == spheroidIn)
                return null;

            ITransformation transformation = new VerticalPerspective(origin, viewHeight, spheroidIn);

            return new StaticCoordinateOperationCompiler.StepCompilationResult(
                opData.StepParams,
                outputUnit,
                transformation
            );
        }

        private static StaticCoordinateOperationCompiler.StepCompilationResult CreateGeographicTopocentric(TransformationCompilationParams opData) {
            Contract.Requires(opData != null);
            var outputUnit = opData.StepParams.RelatedOutputCrsUnit;
            if (null == outputUnit)
                return null;

            var latParam = new KeywordNamedParameterSelector("LAT");
            var lonParam = new KeywordNamedParameterSelector("LON");
            var hParam = new KeywordNamedParameterSelector("H");
            opData.ParameterLookup.Assign(latParam, lonParam, hParam);

            GeographicHeightCoordinate origin;
            if (!TryCreateGeographicHeightCoordinate(latParam.Selection, lonParam.Selection, hParam.Selection, OgcAngularUnit.DefaultRadians, opData.StepParams.RelatedOutputCrsUnit, out origin))
                origin = GeographicHeightCoordinate.Zero;

            var spheroidIn = opData.StepParams.ConvertRelatedInputSpheroidUnit(outputUnit);
            if (null == spheroidIn)
                return null;

            var spheroidOut = opData.StepParams.ConvertRelatedOutputSpheroidUnit(outputUnit);
            if (null == spheroidOut)
                return null;

            var geographicGeocentric = new GeographicGeocentricTransformation(spheroidIn);
            var topocentricOrigin = geographicGeocentric.TransformValue(origin);

            var transformations = new List<ITransformation>() {
				geographicGeocentric,
				new GeocentricTopocentricTransformation(topocentricOrigin, spheroidOut)
			};

            var inputUnitConversion = StaticCoordinateOperationCompiler.CreateCoordinateUnitConversion(opData.StepParams.InputUnit, OgcAngularUnit.DefaultRadians);
            if (null != inputUnitConversion)
                transformations.Insert(0, inputUnitConversion);

            return new StaticCoordinateOperationCompiler.StepCompilationResult(
                opData.StepParams,
                outputUnit,
                new ConcatenatedTransformation(transformations)
            );
        }

        private static StaticCoordinateOperationCompiler.StepCompilationResult CreateGeocentricTopocentric(TransformationCompilationParams opData) {
            Contract.Requires(opData != null);
            var xParam = new KeywordNamedParameterSelector("XAXIS", "X");
            var yParam = new KeywordNamedParameterSelector("YAXIS", "Y");
            var zParam = new KeywordNamedParameterSelector("ZAXIS", "Z");
            opData.ParameterLookup.Assign(xParam, yParam, zParam);

            Point3 origin;
            if (!TryCreatePoint3(xParam.Selection, yParam.Selection, zParam.Selection, opData.StepParams.InputUnit, out origin))
                origin = Point3.Zero;

            var spheroid = opData.StepParams.ConvertRelatedInputSpheroidUnit(opData.StepParams.InputUnit);
            if (null == spheroid)
                return null;

            return new StaticCoordinateOperationCompiler.StepCompilationResult(
                opData.StepParams,
                opData.StepParams.InputUnit,
                new GeocentricTopocentricTransformation(origin, spheroid));
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
            Contract.Requires(opData != null);
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

        private static StaticCoordinateOperationCompiler.StepCompilationResult CreateSimilarityTransformation(TransformationCompilationParams opData) {
            Contract.Requires(opData != null);
            var xParam = new KeywordNamedParameterSelector("ORDINATE1");
            var yParam = new KeywordNamedParameterSelector("ORDINATE2");
            var mParam = new KeywordNamedParameterSelector("SCALE");
            var rParam = new KeywordNamedParameterSelector("ROTATION");
            if (!opData.ParameterLookup.Assign(xParam, yParam, mParam, rParam))
                return null;

            Point2 origin;
            if (!TryCreatePoint2(xParam.Selection, yParam.Selection, out origin))
                return null;

            double scale, rotation;
            if (!NamedParameter.TryGetDouble(mParam.Selection, out scale))
                return null;
            if (!NamedParameter.TryGetDouble(rParam.Selection, out rotation))
                return null;

            ConvertIfVaild(rParam.Selection.Unit, OgcAngularUnit.DefaultRadians, ref rotation);

            return new StaticCoordinateOperationCompiler.StepCompilationResult(
                opData.StepParams,
                opData.StepParams.RelatedOutputCrsUnit ?? opData.StepParams.RelatedInputCrsUnit ?? opData.StepParams.InputUnit,
                new SimilarityTransformation(origin, scale, rotation));
        }

        private static StaticCoordinateOperationCompiler.StepCompilationResult CreateAffineParametricTransformation(TransformationCompilationParams opData) {
            Contract.Requires(opData != null);
            var a0Param = new KeywordNamedParameterSelector("A0");
            var a1Param = new KeywordNamedParameterSelector("A1");
            var a2Param = new KeywordNamedParameterSelector("A2");
            var b0Param = new KeywordNamedParameterSelector("B0");
            var b1Param = new KeywordNamedParameterSelector("B1");
            var b2Param = new KeywordNamedParameterSelector("B2");
            if (!opData.ParameterLookup.Assign(a0Param, a1Param, a2Param, b0Param, b1Param, b2Param))
                return null;

            Vector3 a, b;
            if (!TryCreateVector3(a0Param.Selection, a1Param.Selection, a2Param.Selection, out a))
                return null;
            if (!TryCreateVector3(b0Param.Selection, b1Param.Selection, b2Param.Selection, out b))
                return null;

            return new StaticCoordinateOperationCompiler.StepCompilationResult(
                opData.StepParams,
                opData.StepParams.InputUnit,
                new AffineParametricTransformation(a.X, a.Y, a.Z, b.X, b.Y, b.Z));
        }

        private static StaticCoordinateOperationCompiler.StepCompilationResult CreateTunisiaMiningGrid(TransformationCompilationParams opData) {
            Contract.Requires(opData != null);
            ITransformation transformation = new TunisiaMiningGrid();
            var conv = StaticCoordinateOperationCompiler.CreateCoordinateUnitConversion(opData.StepParams.InputUnit, OgcAngularUnit.DefaultGrads);
            if (null != conv)
                transformation = new ConcatenatedTransformation(new[] { conv, transformation });

            return new StaticCoordinateOperationCompiler.StepCompilationResult(
                opData.StepParams,
                OgcLinearUnit.DefaultKilometer,
                transformation);
        }

        private static StaticCoordinateOperationCompiler.StepCompilationResult CreateMolodenskyBadekas(TransformationCompilationParams opData) {
            Contract.Requires(opData != null);
            var xTransParam = new KeywordNamedParameterSelector("XAXIS", "TRANS");
            var yTransParam = new KeywordNamedParameterSelector("YAXIS", "TRANS");
            var zTransParam = new KeywordNamedParameterSelector("ZAXIS", "TRANS");
            var xRotParam = new KeywordNamedParameterSelector("XAXIS", "ROT");
            var yRotParam = new KeywordNamedParameterSelector("YAXIS", "ROT");
            var zRotParam = new KeywordNamedParameterSelector("ZAXIS", "ROT");
            var scaleParam = new KeywordNamedParameterSelector("SCALE");
            var ord1Param = new KeywordNamedParameterSelector("ORDINATE1");
            var ord2Param = new KeywordNamedParameterSelector("ORDINATE2");
            var ord3Param = new KeywordNamedParameterSelector("ORDINATE3");

            if (!opData.ParameterLookup.Assign(xTransParam, yTransParam, zTransParam, xRotParam, yRotParam, zRotParam, scaleParam, ord1Param, ord2Param, ord3Param))
                return null;

            Vector3 translation, rotation;
            Point3 ordinate;
            double scale;

            if (!TryCreateVector3(xTransParam.Selection, yTransParam.Selection, zTransParam.Selection, out translation))
                return null;
            if (!TryCreateVector3(xRotParam.Selection, yRotParam.Selection, zRotParam.Selection, OgcAngularUnit.DefaultRadians, out rotation))
                return null;
            if (!TryCreatePoint3(ord1Param.Selection, ord2Param.Selection, ord3Param.Selection, out ordinate))
                return null;
            if (!NamedParameter.TryGetDouble(scaleParam.Selection, out scale))
                return null;

            var molodensky = new MolodenskyBadekasTransformation(translation, rotation, ordinate, scale);

            if (opData.StepParams.RelatedInputCrs is ICrsGeocentric && opData.StepParams.RelatedOutputCrs is ICrsGeocentric)
                return new StaticCoordinateOperationCompiler.StepCompilationResult(
                    opData.StepParams,
                    opData.StepParams.RelatedOutputCrsUnit ?? opData.StepParams.RelatedInputCrsUnit ?? opData.StepParams.InputUnit,
                    molodensky);

            // TODO: need to handle units here
            var spheroidFrom = opData.StepParams.RelatedInputSpheroid;
            if (null == spheroidFrom)
                return null;

            // TODO: need to handle units here
            var spheroidTo = opData.StepParams.RelatedOutputSpheroid;
            if (null == spheroidTo)
                return null;

            ITransformation transformation = new GeocentricTransformationGeographicWrapper(spheroidFrom, spheroidTo, molodensky);
            var conv = StaticCoordinateOperationCompiler.CreateCoordinateUnitConversion(opData.StepParams.InputUnit, OgcAngularUnit.DefaultRadians);
            if (null != conv)
                transformation = new ConcatenatedTransformation(new[] { conv, transformation });

            return new StaticCoordinateOperationCompiler.StepCompilationResult(
                opData.StepParams,
                OgcAngularUnit.DefaultRadians,
                transformation);
        }

        private static StaticCoordinateOperationCompiler.StepCompilationResult CreateGeographicDimensionChange(TransformationCompilationParams opData) {
            Contract.Requires(opData != null);
            return new StaticCoordinateOperationCompiler.StepCompilationResult(
                opData.StepParams,
                opData.StepParams.InputUnit,
                new GeographicDimensionChange());
        }

        private readonly INameNormalizedComparer _coordinateOperationNameComparer;

        public StaticTransformationStepCompiler(INameNormalizedComparer coordinateOperationNameComparer = null) {
            _coordinateOperationNameComparer = coordinateOperationNameComparer ?? CoordinateOperationNameNormalizedComparer.Default;
        }

        public StaticCoordinateOperationCompiler.StepCompilationResult Compile(StaticCoordinateOperationCompiler.StepCompilationParameters stepParameters) {
            if(stepParameters == null) throw new ArgumentNullException("stepParameters");
            Contract.EndContractBlock();
            return CompileInverse(stepParameters)
                ?? CompileForwards(stepParameters);
        }

        private StaticCoordinateOperationCompiler.StepCompilationResult CompileForwards(StaticCoordinateOperationCompiler.StepCompilationParameters stepParameters) {
            Contract.Requires(stepParameters != null);
            var forwardCompiledStep = CompileForwardToTransform(stepParameters);
            if (null == forwardCompiledStep)
                return null;

            return new StaticCoordinateOperationCompiler.StepCompilationResult(
                stepParameters,
                forwardCompiledStep.OutputUnit,
                forwardCompiledStep.Transformation
            );
        }

        private StaticCoordinateOperationCompiler.StepCompilationResult CompileInverse(StaticCoordinateOperationCompiler.StepCompilationParameters stepParameters) {
            Contract.Requires(stepParameters != null);
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

            if (null == forwardCompiledStep)
                return null;

            var forwardCompiledOperation = forwardCompiledStep.Transformation;
            if (!forwardCompiledOperation.HasInverse)
                return null;

            var inverseCompiledOperation = forwardCompiledOperation.GetInverse();
            var resultTransformation = inverseCompiledOperation;

            // make sure that the input units are correct
            var unitConversion = StaticCoordinateOperationCompiler.CreateCoordinateUnitConversion(stepParameters.InputUnit, forwardCompiledStep.OutputUnit);
            if (null != unitConversion)
                resultTransformation = new ConcatenatedTransformation(new[] { unitConversion, resultTransformation });

            return new StaticCoordinateOperationCompiler.StepCompilationResult(
                stepParameters,
                expectedOutputUnits,
                resultTransformation
            );
        }

        private StaticCoordinateOperationCompiler.StepCompilationResult CompileForwardToTransform(StaticCoordinateOperationCompiler.StepCompilationParameters stepParameters) {
            Contract.Requires(stepParameters != null);
            string operationName = null;
            IEnumerable<INamedParameter> parameters = null;

            var parameterizedOperation = stepParameters.CoordinateOperationInfo as IParameterizedCoordinateOperationInfo;
            if (null != parameterizedOperation) {
                if (null != parameterizedOperation.Method)
                    operationName = parameterizedOperation.Method.Name;
                parameters = parameterizedOperation.Parameters;
            }

            if (String.IsNullOrEmpty(operationName))
                operationName = stepParameters.CoordinateOperationInfo.Name;
            if (String.IsNullOrEmpty(operationName))
                return null;

            var parameterLookup = new NamedParameterLookup(parameters ?? Enumerable.Empty<INamedParameter>());
            var compilationParams = new TransformationCompilationParams(stepParameters, parameterLookup, operationName);

            var normalizedName = _coordinateOperationNameComparer.Normalize(compilationParams.OperationName);
            if (normalizedName.StartsWith("POSITIONVECTORTRANSFORMATION"))
                return CreatePositionVectorTransformation(compilationParams);
            if (normalizedName.StartsWith("COORDINATEFRAMEROTATION"))
                return CreateCoordinateFrameRotationTransformation(compilationParams);
            if (normalizedName.StartsWith("MOLODENSKYBADEKAS"))
                return CreateMolodenskyBadekas(compilationParams);
            if (normalizedName.Equals("GEOGRAPHICOFFSET") || (normalizedName.StartsWith("GEOGRAPHIC") && normalizedName.EndsWith("OFFSET")))
                return CreateGeographicOffset(compilationParams);
            if (normalizedName.StartsWith("GEOCENTRICTRANSLATION"))
                return CreateGeocentricTranslation(compilationParams);
            if (normalizedName.Equals("VERTICALOFFSET"))
                return CreateVerticalOffset(compilationParams);
            if (normalizedName.Equals("MADRIDTOED50POLYNOMIAL"))
                return CreateMadridToEd50Polynomial(compilationParams);
            if (normalizedName.Equals("SIMILARITYTRANSFORMATION"))
                return CreateSimilarityTransformation(compilationParams);
            if (normalizedName.Equals("AFFINEPARAMETRICTRANSFORMATION"))
                return CreateAffineParametricTransformation(compilationParams);
            if (normalizedName.Equals("GEOGRAPHIC3DTO2DCONVERSION") || normalizedName.Equals("GEOGRAPHIC2DTO3DCONVERSION"))
                return CreateGeographicDimensionChange(compilationParams);
            if (normalizedName.Equals("TUNISIAMININGGRID"))
                return CreateTunisiaMiningGrid(compilationParams);
            if (normalizedName.StartsWith("GEOCENTRICTOPOCENTRIC"))
                return CreateGeocentricTopocentric(compilationParams);
            if (normalizedName.StartsWith("GEOGRAPHICTOPOCENTRIC"))
                return CreateGeographicTopocentric(compilationParams);
            if (normalizedName.StartsWith("VERTICALPERSPECTIVE")) {
                return CreateVerticalPerspective(compilationParams);
            }
            return null;
        }

    }
}
