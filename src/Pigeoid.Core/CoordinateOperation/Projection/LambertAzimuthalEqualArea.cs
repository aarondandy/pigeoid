using System;
using System.Diagnostics.Contracts;
using Pigeoid.CoordinateOperation.Transformation;
using Vertesaur;
using Vertesaur.Transformation;

namespace Pigeoid.CoordinateOperation.Projection
{

    public class LambertAzimuthalEqualAreaPolar : LambertAzimuthalEqualArea
    {
        public LambertAzimuthalEqualAreaPolar(
            GeographicCoordinate geogOrigin,
            Vector2 falseOffset,
            ISpheroid<double> spheroid
        ) : base(geogOrigin, falseOffset, spheroid) {
            IsNorth = geogOrigin.Latitude >= 0;
        }

        protected readonly bool IsNorth;

        private class Inverted : InvertedTransformationBase<LambertAzimuthalEqualAreaPolar, Point2, GeographicCoordinate>
        {
            public Inverted(LambertAzimuthalEqualAreaPolar core) : base(core) {
                Contract.Requires(core != null);
                Contract.Requires(core.HasInverse);
                E4 = Core.ESq * Core.ESq;
                E6 = E4 * Core.ESq;
                A2 = Core.MajorAxis * Core.MajorAxis;
                LogOneMinusOverPlusE = Math.Log(Core.OneMinusE / Core.OnePlusE);
            }

            protected readonly double E4;
            protected readonly double E6;
            protected readonly double A2;
            protected readonly double LogOneMinusOverPlusE;

            public override GeographicCoordinate TransformValue(Point2 value) {
                var deltaX = value.X - Core.FalseProjectedOffset.X;
                var deltaY = value.Y - Core.FalseProjectedOffset.Y;
                
                var radiusOfCurvature = new Vector2(deltaX, deltaY).GetMagnitude();

                var beta = Math.Asin(1.0 - (radiusOfCurvature * radiusOfCurvature / (A2 * (1.0 - ((Core.OneMinusESq / (2.0 * Core.E)) * LogOneMinusOverPlusE)))));
                if (Core.IsNorth ? beta < 0 : beta > 0)
                    beta = -beta; // take the sign of laitude of origin

                var lat = beta
                    + (((Core.ESq / 3.0) + (E4 * 31.0 / 180.0) + (E6 * 517.0 / 5040)) * Math.Sin(2 * beta))
                    + (((E4 / 23.0 / 360.0) + (E6 * 251.0 / 3780.0)) * Math.Sin(4 * beta))
                    + ((E6 * 761.0 / 45360.0) * Math.Sin(6 * beta));

                var lon = Core.GeographicOrigin.Longitude
                    + (Core.IsNorth ? Math.Atan2(deltaX, -deltaY) : Math.Atan2(deltaX, deltaY));

                return new GeographicCoordinate(lat, lon);
            }
        }

        public override Point2 TransformValue(GeographicCoordinate source) {
            var deltaLon = source.Longitude - GeographicOrigin.Longitude;
            var sinLat = Math.Sin(source.Latitude);
            var q = OneMinusESq * ((sinLat / (1.0 - (ESq * sinLat * sinLat))) - (OneOverTwoE * Math.Log((1.0 - (E * sinLat)) / (1.0 + (E * sinLat)))));
            var radiusOfCurvature = MajorAxis * Math.Sqrt(QParallel - q);

            var east = FalseProjectedOffset.X + (radiusOfCurvature * Math.Sin(deltaLon));

            var north = radiusOfCurvature * Math.Cos(deltaLon);
            north = IsNorth
                ? FalseProjectedOffset.Y - north
                : FalseProjectedOffset.Y + north;

            return new Point2(east, north);
        }

        public override bool HasInverse {
            [Pure] get {
                return true;
            }
        }

        public override ITransformation<Point2, GeographicCoordinate> GetInverse() {
            if (!HasInverse) throw new NoInverseException();
            Contract.Ensures(Contract.Result<ITransformation<Point2, GeographicCoordinate>>() != null);
            return new Inverted(this);
        }
    }

    public class LambertAzimuthalEqualAreaOblique : LambertAzimuthalEqualArea
    {
        private class Inverted : InvertedTransformationBase<LambertAzimuthalEqualAreaOblique, Point2, GeographicCoordinate>
        {
            public Inverted(LambertAzimuthalEqualAreaOblique core)
                : base(core) {
                Contract.Requires(core != null);
                Contract.Requires(core.HasInverse);
                E4 = Core.ESq * Core.ESq;
                E6 = E4 * Core.ESq;
            }

            protected readonly double E4;
            protected readonly double E6;

            public override GeographicCoordinate TransformValue(Point2 value) {
                var xDelta = value.X - Core.FalseProjectedOffset.X;
                var yDelta = value.Y - Core.FalseProjectedOffset.Y;
                var dYDelta = Core.D * yDelta;
                var radiusOfCurvature = new Vector2(xDelta / Core.D, Core.D * dYDelta).GetMagnitude();
                var c = 2.0 * Math.Asin(radiusOfCurvature / (2.0 * Core.QRadiusParallel));
                var cosC = Math.Cos(c);
                var sinC = Math.Sin(c);
                var beta = Math.Asin(
                    (cosC * Core.SinBetaOrigin)
                    + ((dYDelta * sinC * Core.CosBetaOrigin) / radiusOfCurvature)
                );

                var lat = beta
                    + (((Core.ESq / 3.0) + (E4 * 31.0 / 180.0) + (E6 * 517.0 / 5040)) * Math.Sin(2 * beta))
                    + (((E4 / 23.0 / 360.0) + (E6 * 251.0 / 3780.0)) * Math.Sin(4 * beta))
                    + ((E6 * 761.0 / 45360.0) * Math.Sin(6 * beta));
                var lon = Core.GeographicOrigin.Longitude + Math.Atan2(
                    xDelta * sinC , (
                        (Core.D * radiusOfCurvature * Core.CosBetaOrigin * cosC)
                        - (Core.D * Core.D * yDelta * Core.SinBetaOrigin * sinC)
                    )
                );
                return new GeographicCoordinate(lat, lon);
            }
        }

        public LambertAzimuthalEqualAreaOblique(
            GeographicCoordinate geogOrigin,
            Vector2 falseOffset,
            ISpheroid<double> spheroid
        ) : base(geogOrigin, falseOffset, spheroid) {
            QOrigin = OneMinusESq * ((SinLatOrigin / (1.0 - (ESq * SinLatOrigin * SinLatOrigin))) - (OneOverTwoE * Math.Log((1.0 - ESinLatOrigin)/(1.0 + ESinLatOrigin))));
            QRadiusParallel = MajorAxis * Math.Sqrt(QParallel / 2.0);
            BetaOrigin = Math.Asin(QOrigin / QParallel);
            SinBetaOrigin = Math.Sin(BetaOrigin);
            CosBetaOrigin = Math.Cos(BetaOrigin);
            D = MajorAxis
                * (CosLatOrigin / Math.Sqrt(1.0 - (ESq * SinLatOrigin * SinLatOrigin)))
                / (QRadiusParallel * CosBetaOrigin);
            ;
        }

        protected readonly double QOrigin;
        protected readonly double QRadiusParallel;
        protected readonly double BetaOrigin;
        protected readonly double SinBetaOrigin;
        protected readonly double CosBetaOrigin;
        protected readonly double D;

        public override Point2 TransformValue(GeographicCoordinate source) {
            var deltaLon = source.Longitude - GeographicOrigin.Longitude;
            var cosDeltaLon = Math.Cos(deltaLon);
            var sinDeltaLon = Math.Sin(deltaLon);
            var sinLat = Math.Sin(source.Latitude);
            var q = OneMinusESq * ((sinLat / (1.0 - (ESq * sinLat * sinLat))) - (OneOverTwoE * Math.Log((1.0 - (E * sinLat))/(1.0 + (E * sinLat)))));
            var beta = Math.Asin(q / QParallel);
            var cosBeta = Math.Cos(beta);
            var sinBeta = Math.Sin(beta);
            var b = QRadiusParallel * Math.Sqrt(2.0 / (1.0 + (SinBetaOrigin * sinBeta) + (CosBetaOrigin * cosBeta * cosDeltaLon)));
            var east = FalseProjectedOffset.X + ((b * D) * (cosBeta * sinDeltaLon));
            var north = FalseProjectedOffset.Y  + ((b / D) * ((CosBetaOrigin * sinBeta) - (SinBetaOrigin * cosBeta * cosDeltaLon)));
            return new Point2(east, north);
        }

        public override bool HasInverse {
            [Pure]
            get {
                return 0 != D && !Double.IsNaN(D);
            }
        }

        public override ITransformation<Point2, GeographicCoordinate> GetInverse() {
            if (!HasInverse) throw new NoInverseException();
            Contract.Ensures(Contract.Result<ITransformation<Point2, GeographicCoordinate>>() != null);
            return new Inverted(this);
        }
    }


    public abstract class LambertAzimuthalEqualArea : SpheroidProjectionBase
    {

        protected LambertAzimuthalEqualArea(
            GeographicCoordinate geogOrigin,
            Vector2 falseOffset,
            ISpheroid<double> spheroid
        ) : base(falseOffset, spheroid) {
            GeographicOrigin = geogOrigin;
            OneMinusESq = 1.0 - ESq;
            OneMinusE = 1.0 - E;
            OnePlusE = 1.0 + E;
            OneOverTwoE = 1.0 / (2.0 * E);
            SinLatOrigin = Math.Sin(GeographicOrigin.Latitude);
            CosLatOrigin = Math.Cos(GeographicOrigin.Latitude);
            ESinLatOrigin = E * SinLatOrigin;
            QParallel = OneMinusESq * ((1.0 / OneMinusESq) - (OneOverTwoE * Math.Log(OneMinusE / OnePlusE)));
        }

        public readonly GeographicCoordinate GeographicOrigin;
        protected readonly double OneMinusESq;
        protected readonly double OneMinusE;
        protected readonly double OnePlusE;
        protected readonly double SinLatOrigin;
        protected readonly double CosLatOrigin;
        protected readonly double ESinLatOrigin;
        protected readonly double OneOverTwoE;
        protected readonly double QParallel;

        public abstract override Point2 TransformValue(GeographicCoordinate source);

        public abstract override bool HasInverse { get; }

        public abstract override ITransformation<Point2, GeographicCoordinate> GetInverse();

        /*
        private class Inverted : InvertedTransformationBase<LambertAzimuthalEqualArea, Point2, GeographicCoordinate>
        {

            public Inverted(LambertAzimuthalEqualArea core) : base(core) {
                Contract.Requires(core != null);
            }

            public override GeographicCoordinate TransformValue(Point2 source) {
                var xDelta = source.X - Core.FalseProjectedOffset.X;
                var yDelta = source.Y - Core.FalseProjectedOffset.Y;
                var dYDelta = Core.D * yDelta;
                var radiusOfCurvature = new Vector2(xDelta / Core.D, Core.D * dYDelta).GetMagnitude();
                var c = 2.0 * Math.Asin(radiusOfCurvature / (2.0 * Core.RadiusQ));
                var cosC = Math.Cos(c);
                var sinC = Math.Sin(c);
                var beta = Math.Asin(
                    (cosC * Core.SinBetaOrigin)
                    + ((dYDelta * sinC * Core.CosBetaOrigin) / radiusOfCurvature)
                );

                var e4 = Core.ESq * Core.ESq;
                var e6 = e4 * Core.ESq;
                var lat = beta
                    + (((Core.ESq / 3.0) + (e4 * 31.0 / 180.0) + (e6 * 517.0 / 5040)) * Math.Sin(2 * beta))
                    + (((e4 / 23.0 / 360.0) + (e6 * 251.0 / 3780.0)) * Math.Sin(4 * beta))
                    + ((e6 * 761.0 / 45360.0) * Math.Sin(6 * beta));
                var lon = Core.GeographicOrigin.Longitude + Math.Atan(
                    xDelta * sinC / (
                        (Core.D * radiusOfCurvature * Core.CosBetaOrigin * cosC)
                        - (Core.D * Core.D * yDelta * Core.SinBetaOrigin * sinC)
                    )
                );
                return new GeographicCoordinate(lat, lon);
            }
        }

        public readonly GeographicCoordinate GeographicOrigin;

        protected readonly double QParallel;
        protected readonly double SinBetaOrigin;
        protected readonly double CosBetaOrigin;
        protected readonly double RadiusQ;
        protected readonly double D;
        protected readonly double OneMinusESq;
        protected readonly double OneMinusESqSinLatOriginSq;
        protected readonly double OneOverTwoE;

        public LambertAzimuthalEqualArea(
            GeographicCoordinate geogOrigin,
            Vector2 falseOffset,
            ISpheroid<double> spheroid
        ) : base(falseOffset, spheroid) {
            Contract.Requires(spheroid != null);
            GeographicOrigin = geogOrigin;
            OneMinusESq = 1 - ESq;
            var sinLatOrigin = Math.Sin(geogOrigin.Latitude);
            var cosLatOrigin = Math.Cos(geogOrigin.Latitude);
            var eSqSinLatOriginSq = ESq * sinLatOrigin * sinLatOrigin;
            OneMinusESqSinLatOriginSq = 1.0 - eSqSinLatOriginSq;
            OneOverTwoE = 1.0 / (2.0 * E);
            var sinLatOriginE = sinLatOrigin * E;
            var qOrigin = OneMinusESq * (
                (sinLatOrigin / OneMinusESqSinLatOriginSq)
                - (OneOverTwoE * Math.Log((1 - sinLatOriginE) / (1 + sinLatOriginE)))
            );
            QParallel = OneMinusESq * (
                (1 / OneMinusESq)
                - (OneOverTwoE * Math.Log((1 - E) / (1 + E)))
            );
            RadiusQ = MajorAxis * Math.Sqrt(QParallel / 2.0);

            var betaOrigin = Math.Asin(qOrigin / QParallel);
            SinBetaOrigin = Math.Sin(betaOrigin);
            CosBetaOrigin = Math.Cos(betaOrigin);
            D = MajorAxis
                * (cosLatOrigin / Math.Sqrt(1 - eSqSinLatOriginSq))
                / (RadiusQ * CosBetaOrigin);
            ;
        }

        public override Point2 TransformValue(GeographicCoordinate source) {
            var sinLat = Math.Sin(source.Latitude);
            var eSinLat = sinLat * E;
            var q = OneMinusESq * (
                (sinLat / (1 - (ESq * sinLat * sinLat)))
                - (OneOverTwoE * Math.Log((1 - eSinLat) / (1 + eSinLat)))
            );
            var beta = Math.Asin(q / QParallel);
            var sinBeta = Math.Sin(beta);
            var cosBeta = Math.Cos(beta);
            var deltaLon = source.Longitude - GeographicOrigin.Longitude;
            var cosDeltaLon = Math.Cos(deltaLon);
            var b = RadiusQ * Math.Sqrt(
                2.0 / (
                    1
                    + (SinBetaOrigin * sinBeta)
                    + (CosBetaOrigin * cosBeta * cosDeltaLon)
                )
            );
            return new Point2(
                FalseProjectedOffset.X + (b * D * cosBeta * Math.Sin(deltaLon)),
                FalseProjectedOffset.Y + (
                    (b / D)
                    * (
                        (CosBetaOrigin * sinBeta)
                        - (SinBetaOrigin * cosBeta * cosDeltaLon)
                    )
                )
            );
        }

        public override ITransformation<Point2, GeographicCoordinate> GetInverse() {
            if (!HasInverse) throw new NoInverseException();
            Contract.Ensures(Contract.Result<ITransformation<Point2, GeographicCoordinate>>() != null);
            return new Inverted(this);
        }

        public override bool HasInverse {
            [Pure] get {
                // ReSharper disable CompareOfFloatsByEqualityOperator
                return 0 != D && !Double.IsNaN(D);
                // ReSharper restore CompareOfFloatsByEqualityOperator
            }
        }*/

    }
}
