using System;
using System.Diagnostics.Contracts;
using Pigeoid.CoordinateOperation.Transformation;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.CoordinateOperation.Projection
{
    public abstract class HotineObliqueMercator : ProjectionBase
    {

        public class VariantA : HotineObliqueMercator
        {

            private class Inverse : InvertedTransformationBase<VariantA, Point2, GeographicCoordinate>
            {
                public Inverse(VariantA core) : base(core) {
                    Contract.Requires(core != null);
                }

                public override GeographicCoordinate TransformValue(Point2 value) {
                    // common
                    var dx = value.X - Core.FalseProjectedOffset.X;
                    var dy = value.Y - Core.FalseProjectedOffset.Y;
                    var vScalar = (dx * Math.Cos(Core.GammaOrigin)) - (dy * Math.Sin(Core.GammaOrigin));
                    var uScalar = (dy * Math.Cos(Core.GammaOrigin)) - (dx * Math.Sin(Core.GammaOrigin));

                    // common
                    var q = Math.Pow(Math.E, -(Core.B * vScalar / Core.A));
                    var invQ = 1.0 / q;
                    var sFactor = (q - invQ) / 2.0;
                    var tFactor = (q + invQ) / 2.0;
                    var vFactor = Math.Sin(Core.B * uScalar / Core.A);
                    var uFactor = ((vFactor * Math.Cos(Core.GammaOrigin)) + (sFactor * Math.Sin(Core.GammaOrigin))) / tFactor;
                    var tScalar = Math.Pow(Core.H / Math.Sqrt((1 + uFactor) / (1 - uFactor)), 1.0 / Core.B);
                    var chi = HalfPi - (2.0 * Math.Atan(tScalar));
                    var lat = chi
                        + (Math.Sin(2.0 * chi) * ((Core.ESq / 2.0)) + (Core._e4 * 5.0 / 24.0) + (Core._e6 / 12.0) + (Core._e8 * 12.0 / 360.0))
                        + (Math.Sin(4.0 * chi) * ((Core._e4 * 7.0 / 48.0) + (Core._e6 * 29.0 / 240.0) + (Core._e8 * 811.0 / 11520.0)))
                        + (Math.Sin(6.0 * chi) * ((Core._e6 * 7.0 / 120.0) + (Core._e8 * 81.0 / 1120.0)))
                        + (Math.Sin(8.0 * chi) * (Core._e8 * 4279.0 / 161280.0));
                    var lon = Core.LongitudeOrigin - (Math.Atan(((sFactor * Math.Cos(Core.GammaOrigin)) - (vFactor * Math.Sin(Core.GammaOrigin))) / Math.Cos(Core.B * uScalar / Core.A)) / Core.B);
                    return new GeographicCoordinate(lat, lon);
                }
            }

            public VariantA(GeographicCoordinate geographicCenter, double azimuthOfInitialLine, double angleFromRectified, double scaleFactor, Vector2 falseProjectedOffset, ISpheroid<double> spheroid)
                : base(geographicCenter, azimuthOfInitialLine, angleFromRectified, scaleFactor, falseProjectedOffset, spheroid)
            {
                Contract.Requires(spheroid != null);
            }

            public override Point2 TransformValue(GeographicCoordinate source) {
                // common
                var lonDelta = source.Longitude - LongitudeOrigin;
                var eSinLat = E * Math.Sin(source.Latitude);
                var theta = Math.Tan(QuarterPi - (source.Latitude / 2.0))
                    / Math.Pow((1 - eSinLat) / (1 + eSinLat), EHalf);
                var q = H / Math.Pow(theta, B); // Q
                var invQ = 1 / q;
                var sFactor = (q - invQ) / 2.0; // S
                var tFactor = (q + invQ) / 2.0; // T
                var vFactor = Math.Sin(B * (lonDelta)); // V
                var uFactor = ((-vFactor * Math.Cos(GammaOrigin)) + (sFactor * Math.Sin(GammaOrigin))) / tFactor; // U
                var vScalar = A * Math.Log((1 - uFactor) / (1 + uFactor)) / (2.0 * B); // v

                // variant A
                var uScalar = A * Math.Atan(((sFactor * Math.Cos(GammaOrigin)) + (vFactor * Math.Sin(GammaOrigin))) / Math.Cos(B * lonDelta)) / B;

                // common
                var x = (vScalar * Math.Cos(GammaOrigin)) + (uScalar * Math.Sin(GammaOrigin)) + FalseProjectedOffset.X;
                var y = (uScalar * Math.Cos(GammaOrigin)) - (vScalar * Math.Sin(GammaOrigin)) + FalseProjectedOffset.Y;
                return new Point2(x, y);
            }

            public override ITransformation<Point2, GeographicCoordinate> GetInverse() {
                if (!HasInverse) throw new NoInverseException();
                Contract.Ensures(Contract.Result<ITransformation<Point2, GeographicCoordinate>>() != null);
                return new Inverse(this);
            }

        }

        public class VariantB : HotineObliqueMercator
        {
            private class Inverse : InvertedTransformationBase<VariantB, Point2, GeographicCoordinate>
            {
                public Inverse(VariantB core) : base(core) {
                    Contract.Requires(core != null);
                }

                public override GeographicCoordinate TransformValue(Point2 value) {
                    // common
                    var dx = value.X - Core.FalseProjectedOffset.X;
                    var dy = value.Y - Core.FalseProjectedOffset.Y;
                    var vScalar = (dx * Math.Cos(Core.GammaOrigin)) - (dy * Math.Sin(Core.GammaOrigin));
                    var uScalar = (dy * Math.Cos(Core.GammaOrigin)) + (dx * Math.Sin(Core.GammaOrigin));

                    // variantB
                    uScalar = Core.Uc < 0 ? uScalar - Math.Abs(Core.Uc) : uScalar + Math.Abs(Core.Uc);

                    // common
                    var q = Math.Pow(Math.E, -(Core.B * vScalar / Core.A));
                    var invQ = 1.0 / q;
                    var sFactor = (q - invQ) / 2.0;
                    var tFactor = (q + invQ) / 2.0;
                    var vFactor = Math.Sin(Core.B * uScalar / Core.A);
                    var uFactor = ((vFactor * Math.Cos(Core.GammaOrigin)) + (sFactor * Math.Sin(Core.GammaOrigin))) / tFactor;
                    var tScalar = Math.Pow(Core.H / Math.Sqrt((1 + uFactor) / (1 - uFactor)), 1.0 / Core.B);
                    var chi = HalfPi - (2.0 * Math.Atan(tScalar));
                    var lat = chi
                        + (Math.Sin(2.0 * chi) * ((Core.ESq / 2.0)) + (Core._e4 * 5.0 / 24.0) + (Core._e6 / 12.0) + (Core._e8 * 12.0 / 360.0))
                        + (Math.Sin(4.0 * chi) * ((Core._e4 * 7.0 / 48.0) + (Core._e6 * 29.0 / 240.0) + (Core._e8 * 811.0 / 11520.0)))
                        + (Math.Sin(6.0 * chi) * ((Core._e6 * 7.0 / 120.0) + (Core._e8 * 81.0 / 1120.0)))
                        + (Math.Sin(8.0 * chi) * (Core._e8 * 4279.0 / 161280.0));
                    var lon = Core.LongitudeOrigin - (Math.Atan(((sFactor * Math.Cos(Core.GammaOrigin)) - (vFactor * Math.Sin(Core.GammaOrigin))) / Math.Cos(Core.B * uScalar / Core.A)) / Core.B);
                    return new GeographicCoordinate(lat, lon);
                }
            }

            public VariantB(GeographicCoordinate geographicCenter, double azimuthOfInitialLine, double angleFromRectified, double scaleFactor, Vector2 falseProjectedOffset, ISpheroid<double> spheroid)
                : base(geographicCenter, azimuthOfInitialLine, angleFromRectified, scaleFactor, falseProjectedOffset, spheroid)
            {
                Contract.Requires(spheroid != null);
            }

            public override Point2 TransformValue(GeographicCoordinate source) {
                // common
                var lonDelta = source.Longitude - LongitudeOrigin;
                var eSinLat = E * Math.Sin(source.Latitude);
                var theta = Math.Tan(QuarterPi - (source.Latitude / 2.0))
                    / Math.Pow((1 - eSinLat) / (1 + eSinLat), EHalf);
                var q = H / Math.Pow(theta, B); // Q
                var invQ = 1 / q;
                var sFactor = (q - invQ) / 2.0; // S
                var tFactor = (q + invQ) / 2.0; // T
                var vFactor = Math.Sin(B * (lonDelta)); // V
                var uFactor = ((-vFactor * Math.Cos(GammaOrigin)) + (sFactor * Math.Sin(GammaOrigin))) / tFactor; // U
                var vScalar = A * Math.Log((1 - uFactor) / (1 + uFactor)) / (2.0 * B); // v

                // variant B
                double uScalar;
                if (HasHighAzimuth) {
                    if (lonDelta == 0) {
                        uScalar = 0;
                    }
                    else {
                        uScalar = A * Math.Atan(((sFactor * Math.Cos(GammaOrigin)) + (vFactor * Math.Sin(GammaOrigin))) / Math.Cos(B * lonDelta)) / B;
                        var signLatNeg = GeographicCenter.Latitude < 0;
                        var signLonNeg = lonDelta < 0;
                        var negUc = signLatNeg ^ signLonNeg;
                        var absUc = Math.Abs(Uc);
                        uScalar = negUc ? uScalar + absUc : uScalar - absUc;
                    }
                }
                else {
                    uScalar = A * Math.Atan(((sFactor * Math.Cos(GammaOrigin)) + (vFactor * Math.Sin(GammaOrigin))) / Math.Cos(B * lonDelta)) / B;
                    var negUc = GeographicCenter.Latitude < 0;
                    var absUc = Math.Abs(Uc);
                    uScalar = negUc ? uScalar + absUc : uScalar - absUc;
                }

                // common
                var x = (vScalar * Math.Cos(GammaOrigin)) + (uScalar * Math.Sin(GammaOrigin)) + FalseProjectedOffset.X;
                var y = (uScalar * Math.Cos(GammaOrigin)) - (vScalar * Math.Sin(GammaOrigin)) + FalseProjectedOffset.Y;
                return new Point2(x, y);
            }

            public override ITransformation<Point2, GeographicCoordinate> GetInverse() {
                if (!HasInverse) throw new NoInverseException();
                Contract.Ensures(Contract.Result<ITransformation<Point2, GeographicCoordinate>>() != null);
                return new Inverse(this);
            }

        }

        protected GeographicCoordinate GeographicCenter;
        protected double AzimuthOfInitialLine;
        protected double AngleFromRectified;
        protected double ScaleFactor;

        protected double B;
        protected double A;
        protected double TOrigin;
        protected double D;
        protected double F;
        protected double H;
        protected double G;
        protected double Uc;
        protected double Vc;
        protected double GammaOrigin;
        protected double LongitudeOrigin;
        protected bool CenterLatitudeIsNegative;
        protected bool HasHighAzimuth;

        private readonly double _e4;
        private readonly double _e6;
        private readonly double _e8;

        protected HotineObliqueMercator(GeographicCoordinate geographicCenter, double azimuthOfInitialLine, double angleFromRectified, double scaleFactor, Vector2 falseProjectedOffset, ISpheroid<double> spheroid)
            : base(falseProjectedOffset, spheroid)
        {
            Contract.Requires(spheroid != null);
            GeographicCenter = geographicCenter;
            AzimuthOfInitialLine = azimuthOfInitialLine;
            var cosAzimuth = Math.Cos(AzimuthOfInitialLine);
            AngleFromRectified = angleFromRectified;
            ScaleFactor = scaleFactor;
            var cosLatCenter = Math.Cos(geographicCenter.Latitude);
            var sinLatCenter = Math.Sin(geographicCenter.Latitude);
            var eSinLatCenter = E * sinLatCenter;

            // ReSharper disable CompareOfFloatsByEqualityOperator
            HasHighAzimuth = AzimuthOfInitialLine >= HalfPi || AzimuthOfInitialLine <= -HalfPi || 0 == cosAzimuth;
            // ReSharper restore CompareOfFloatsByEqualityOperator

            B = Math.Sqrt(
                (
                    (ESq * cosLatCenter * cosLatCenter * cosLatCenter * cosLatCenter)
                    / (1 - ESq)
                )
                + 1.0
            );

            A = (MajorAxis * B * scaleFactor * Math.Sqrt(1 - ESq)) / (1 - (ESq * sinLatCenter * sinLatCenter));

            TOrigin = Math.Tan(QuarterPi - (geographicCenter.Latitude / 2.0))
                / Math.Pow(
                    (1 - eSinLatCenter)
                    / (1 + eSinLatCenter)
                , EHalf);

            D = (B * Math.Sqrt(1 - ESq)) / (cosLatCenter * Math.Sqrt(1 - (ESq * sinLatCenter * sinLatCenter)));

            CenterLatitudeIsNegative = geographicCenter.Latitude < 0;
            F = D >= 1
                ? D + Math.Sqrt((D * D) - 1)
                : D;
            if (CenterLatitudeIsNegative)
                F = -F;

            H = F * Math.Pow(TOrigin, B);

            G = (F - (1 / F)) / 2.0;

            GammaOrigin = Math.Asin(Math.Sin(AzimuthOfInitialLine) / D); // yo

            LongitudeOrigin = geographicCenter.Longitude - (Math.Asin(G * Math.Tan(GammaOrigin)) / B);

            Vc = 0;
            if (HasHighAzimuth) {
                Uc = A * (GeographicCenter.Longitude - LongitudeOrigin);
            }
            else {
                Uc = (A / B) * Math.Atan(Math.Sqrt((D * D) - 1.0) / cosAzimuth);
                if (CenterLatitudeIsNegative)
                    Uc = -Uc;
            }

            _e4 = ESq * ESq;
            _e6 = _e4 * ESq;
            _e8 = _e6 * ESq;
        }

        public abstract override Point2 TransformValue(GeographicCoordinate source);

        public abstract override ITransformation<Point2, GeographicCoordinate> GetInverse();

        public override bool HasInverse {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            get {
                return 0 != B && !Double.IsNaN(B)
                    && 0 != H && !Double.IsNaN(H)
                    && 0 != A && !Double.IsNaN(A);
            }
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }


    }
}
