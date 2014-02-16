using System;
using System.Diagnostics.Contracts;
using Pigeoid.CoordinateOperation.Transformation;
using Vertesaur;
using Vertesaur.Transformation;

namespace Pigeoid.CoordinateOperation.Projection
{
    public class Krovak : SpheroidProjectionBase
    {

        internal static double KrovakLongitudeRadiansOffset = -(17.0 + (2.0 / 3.0)) * Math.PI / 180.0;

        internal class Inverse : InvertedTransformationBase<Krovak, Point2, GeographicCoordinate>
        {

            public Inverse(Krovak core) : base(core) {
                Contract.Requires(core != null);
            }

            public override GeographicCoordinate TransformValue(Point2 value) {
                // ReSharper disable CompareOfFloatsByEqualityOperator
                var x = value.X - Core.FalseProjectedOffset.X;
                var y = value.Y - Core.FalseProjectedOffset.Y;
                var r = Math.Sqrt((x * x) + (y * y));
                var theta = Math.Atan(y / x);
                var d = theta / Core.SinLatitudeOfPseudoStandardParallel;
                var t = (
                    Math.Atan(
                        Math.Pow(Core.ROrigin / r, Core.InverseN)
                        * Core.TanPseudoLatCalculationValue
                    )
                    - QuarterPi
                ) * 2.0;
                var cosT = Math.Cos(t);
                var u = Math.Asin(
                    (Math.Sin(t) * Core.CosAzimuthOfInitialLine)
                    - (Math.Cos(d) * Core.SinAzimuthOfInitialLine * cosT)
                );
                var lat = u;
                var uCalculation = Math.Pow(Math.Tan((u / 2.0) + QuarterPi), Core.InverseB)
                    * Core.TOriginCalculation;
                for (int i = 0; i < 16; i++) {
                    var oldLat = lat;
                    lat = (
                        Math.Atan(
                            Math.Pow(
                                ((Math.Sin(lat) * Core.E) + 1.0)
                                / (1.0 - (Math.Sin(lat) * Core.E)),
                                Core.E / 2.0
                            )
                            * uCalculation
                        ) - QuarterPi
                    ) * 2.0;

                    if (lat == oldLat)
                        break;
                }
                var lon = Core.GeographicOrigin.Longitude - (
                    Math.Asin(
                        (Math.Sin(d) * cosT)
                        / Math.Cos(u)
                    ) / Core.B
                );
                return new GeographicCoordinate(lat, lon + KrovakLongitudeRadiansOffset);
            }
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        protected readonly GeographicCoordinate GeographicOrigin;
        protected readonly double LatitudeOfPseudoStandardParallel;
        protected readonly double SinLatitudeOfPseudoStandardParallel;
        protected readonly double TanPseudoLatCalculationValue;
        protected readonly double ScaleFactor;
        protected readonly double AzimuthOfInitialLine;
        protected readonly double CosAzimuthOfInitialLine;
        protected readonly double SinAzimuthOfInitialLine;
        protected readonly double A;
        protected readonly double B;
        protected readonly double InverseB;
        protected readonly double NegativeInverseB;
        protected readonly double HalfBe;
        protected readonly double N;
        protected readonly double InverseN;
        protected readonly double YOrigin;
        protected readonly double TOrigin;
        protected readonly double ROrigin;
        protected readonly double RNumerator;
        protected readonly double TOriginCalculation;

        public Krovak(
            GeographicCoordinate geographicOrigin,
            double latitudeOfPseudoStandardParallel,
            double azimuthOfInitialLine,
            double scaleFactor,
            Vector2 falseProjectedOffset,
            ISpheroid<double> spheroid
        )
            : base(falseProjectedOffset, spheroid)
        {
            Contract.Requires(spheroid != null);
            // TODO: should the longitude be adjusted for Ferro here and accepted as relative to Greenwich?
            GeographicOrigin = geographicOrigin;
            LatitudeOfPseudoStandardParallel = latitudeOfPseudoStandardParallel;
            SinLatitudeOfPseudoStandardParallel = Math.Sin(LatitudeOfPseudoStandardParallel);
            AzimuthOfInitialLine = azimuthOfInitialLine;
            CosAzimuthOfInitialLine = Math.Cos(AzimuthOfInitialLine);
            SinAzimuthOfInitialLine = Math.Sin(AzimuthOfInitialLine);
            ScaleFactor = scaleFactor;
            var sinLatCenter = Math.Sin(GeographicOrigin.Latitude);
            var cosLatCenter = Math.Cos(GeographicOrigin.Latitude);
            A = (spheroid.A * Math.Sqrt(1.0 - spheroid.ESquared))
                / (1.0 - (spheroid.ESquared * sinLatCenter * sinLatCenter));
            B = Math.Sqrt(1.0 + (
                (spheroid.ESquared * cosLatCenter * cosLatCenter * cosLatCenter * cosLatCenter)
                / (1.0 - spheroid.ESquared)
            ));
            InverseB = 1.0 / B;
            NegativeInverseB = -InverseB;
            HalfBe = (B * spheroid.E) / 2.0;
            YOrigin = Math.Asin(Math.Sin(GeographicOrigin.Latitude) / B);
            TOrigin = (
                Math.Tan((Math.PI / 4.0) + (YOrigin / 2.0))
                * Math.Pow(
                    (1.0 + (spheroid.E * Math.Sin(GeographicOrigin.Latitude)))
                    / (1.0 - (spheroid.E * Math.Sin(geographicOrigin.Latitude)))
                , spheroid.E * B / 2.0)
            )
            / Math.Pow(Math.Tan((Math.PI / 4.0) + (GeographicOrigin.Latitude / 2.0)), B);
            TOriginCalculation = Math.Pow(TOrigin, NegativeInverseB);
            N = Math.Sin(LatitudeOfPseudoStandardParallel);
            InverseN = 1.0 / N;
            ROrigin = (ScaleFactor * A) / Math.Tan(LatitudeOfPseudoStandardParallel);
            TanPseudoLatCalculationValue = Math.Tan((LatitudeOfPseudoStandardParallel / 2.0) + QuarterPi);
            RNumerator = Math.Pow(TanPseudoLatCalculationValue, N);
        }

        public override Point2 TransformValue(GeographicCoordinate source) {
            var eSinLat = Math.Sin(source.Latitude) * Spheroid.E;
            var u = (
                Math.Atan(
                    (
                        Math.Pow(
                            Math.Tan((source.Latitude / 2.0) + QuarterPi),
                            B
                        ) * TOrigin
                    )
                    / Math.Pow(
                        (1.0 + eSinLat) / (1.0 - eSinLat),
                        HalfBe
                    )
                )
                - QuarterPi
            ) * 2.0;
            var cosU = Math.Cos(u);
            var v = (GeographicOrigin.Longitude - (source.Longitude - KrovakLongitudeRadiansOffset)) * B;
            var t = Math.Asin(
                (Math.Sin(u) * CosAzimuthOfInitialLine)
                + (Math.Cos(v) * cosU * SinAzimuthOfInitialLine)
            );
            var d = Math.Asin(
                (Math.Sin(v) * cosU)
                / Math.Cos(t)
            );
            var theta = N * d;
            var r = (
                RNumerator
                / Math.Pow(Math.Tan((t / 2.0) + QuarterPi), N)
            ) * ROrigin;
            return new Point2(
                (Math.Cos(theta) * r) + FalseProjectedOffset.X,
                (Math.Sin(theta) * r) + FalseProjectedOffset.Y
            );
        }

        public override ITransformation<Point2, GeographicCoordinate> GetInverse() {
            Contract.Ensures(Contract.Result<ITransformation<Point2, GeographicCoordinate>>() != null);
            return new Inverse(this);
        }

        public override bool HasInverse {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            [Pure] get {
                return 0 != SinLatitudeOfPseudoStandardParallel
                    && !Double.IsNaN(SinLatitudeOfPseudoStandardParallel);
            }
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

    }
}
