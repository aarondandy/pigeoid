using System;
using System.Diagnostics.Contracts;
using Pigeoid.CoordinateOperation.Transformation;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.CoordinateOperation.Projection
{
    public class LabordeObliqueMercator : ProjectionBase
    {

        private const double ParisLongitudeOffset = 2.5969212963 * Math.PI / 200.0;

        private class Inverse : InvertedTransformationBase<LabordeObliqueMercator, Point2, GeographicCoordinate>
        {
            public Inverse(LabordeObliqueMercator core) : base(core) {
                Contract.Requires(core != null);
            }

            public override GeographicCoordinate TransformValue(Point2 value) {
                var g = (1 - Math.Cos(2.0 * Core.Azimuth)) / 12.0;
                var gi = Math.Sin(2.0 * Core.Azimuth) / 12.0;
                var h0 = (value.Y - Core.FalseProjectedOffset.Y) / Core.R;
                var hi0 = (value.X - Core.FalseProjectedOffset.X) / Core.R;
                var h = h0 / (h0 + (g * h0 * h0 * h0));
                var hi = hi0 / (hi0 + (gi * hi0 * hi0 * hi0));
                for (int i = 0; i < 8; i++) {
                    var ghh = g * h * h;
                    var ghhh = ghh * h;
                    if (h0 - h == ghhh)
                        break;
                    var temp = (h0 + (2.0 * ghhh)) / ((3.0 * ghh) + 1);
                    var tempi = (hi0 + (2.0 * gi * hi * hi * hi)) / ((3.0 * gi * hi * hi * hi) + 1);
                    if (temp == h && tempi == hi)
                        break;
                    h = temp;
                    hi = tempi;
                }
                var lPrime = -h;
                var pPrime = (Math.Atan(Math.Pow(Math.E, hi)) * 2.0) - HalfPi;
                var uPrime = (Math.Cos(pPrime) * Math.Cos(lPrime) * Math.Cos(Core.SLatitude))
                    + (Math.Cos(pPrime) * Math.Sin(lPrime) * Math.Sin(Core.SLatitude));
                var vPrime = Math.Sin(pPrime);
                var wPrime = (Math.Cos(pPrime) * Math.Cos(lPrime) * Math.Sin(Core.SLatitude))
                    - (Math.Cos(pPrime) * Math.Sin(lPrime) * Math.Cos(Core.SLatitude));
                var d = Math.Sqrt((uPrime * uPrime) + (vPrime * vPrime));
                double l, p;
                if (d == 0) {
                    l = 0;
                    p = wPrime < 0 ? -HalfPi : HalfPi;
                }
                else {
                    l = Math.Atan(vPrime / (uPrime + d)) * 2.0;
                    p = Math.Atan(wPrime / d);
                }

                var lon = Core.GreenwichProjectionCenter.Longitude + (l / Core.Beta) - ParisLongitudeOffset;
                var qPrime = (Math.Log(Math.Tan(QuarterPi + (p / 2.0))) - Core.C) / Core.Beta;
                var lat = (Math.Atan(Math.Pow(Math.E, qPrime)) * 2.0) - HalfPi;
                for (int i = 0; i < 8; i++) {
                    var eSinLat = Core.E * Math.Sin(lat);
                    var temp = (2.0 * Math.Atan(Math.Pow((1 + eSinLat) / (1 - eSinLat), Core.EHalf) * Math.Pow(Math.E, qPrime))) - HalfPi;
                    if (temp == lat) break;
                    lat = temp;
                }

                return new GeographicCoordinate(lat, lon);
            }
        }

        protected GeographicCoordinate GreenwichProjectionCenter;
        protected double Beta;
        protected double SLatitude;
        protected double R;
        protected double C;
        protected double Azimuth;

        public LabordeObliqueMercator(
            GeographicCoordinate projectionCenter,
            double azimuthOfInitialLine,
            double scaleFactor,
            ISpheroid<double> spheroid,
            Vector2 falseProjectedOffset
        ) : base(falseProjectedOffset, spheroid) {
            Contract.Requires(spheroid != null);
            Azimuth = azimuthOfInitialLine;
            GreenwichProjectionCenter = new GeographicCoordinate(projectionCenter.Latitude, projectionCenter.Longitude + ParisLongitudeOffset);
            var cosLatCenter = Math.Cos(GreenwichProjectionCenter.Latitude);
            var sinLatCenter = Math.Sin(GreenwichProjectionCenter.Latitude);
            var oneMinusESq = 1 - ESq;
            var eSinLatCenter = E * sinLatCenter;
            Beta = Math.Sqrt(
                (
                    (ESq * cosLatCenter * cosLatCenter * cosLatCenter * cosLatCenter)
                    / oneMinusESq
                )
                + 1);
            SLatitude = Math.Asin(sinLatCenter / Beta);
            R = MajorAxis * scaleFactor * (
                Math.Sqrt(oneMinusESq)
                / (1 - (ESq * sinLatCenter * sinLatCenter))
            );
            C = Math.Log(Math.Tan(QuarterPi + (SLatitude / 2.0)))
                - (
                    Beta
                    * Math.Log(
                        Math.Tan(QuarterPi + (GreenwichProjectionCenter.Latitude / 2.0))
                        * Math.Pow(
                            (1 - eSinLatCenter) / (1 + eSinLatCenter),
                            EHalf
                        )
                    )
                );
            ;
        }

        public override Point2 TransformValue(GeographicCoordinate source) {
            var greenwichLon = source.Longitude + ParisLongitudeOffset;


            var sinLat = Math.Sin(source.Latitude);
            var eSinLat = E * sinLat;
            var halfLat = source.Latitude / 2.0;
            var betaLonDiff = Beta * (greenwichLon - GreenwichProjectionCenter.Longitude);
            var q = C + (
                Beta
                * Math.Log(
                    Math.Tan(QuarterPi + halfLat)
                    * Math.Pow((1 - eSinLat) / (1 + eSinLat), EHalf)
                )
            );
            var p = (Math.Atan(Math.Pow(Math.E, q)) * 2.0) - HalfPi;
            var cosP = Math.Cos(p);
            var sinP = Math.Sin(p);
            var cosL = Math.Cos(betaLonDiff);
            var sinL = Math.Sin(betaLonDiff);
            var u = (cosP * cosL * Math.Cos(SLatitude)) + (sinP * Math.Sin(SLatitude));
            var v = (cosP * cosL * Math.Sin(SLatitude)) - (sinP * Math.Cos(SLatitude));
            var w = cosP * sinL;
            var d = Math.Sqrt((u * u) + (v * v));
            double lPrime, pPrime;
            if (0 == d) {
                lPrime = 0;
                pPrime = w < 0 ? -HalfPi : HalfPi;
            }
            else {
                lPrime = Math.Atan(v / (u + d)) * 2.0;
                pPrime = Math.Atan(w / d);
            }
            var h = -lPrime;
            var hi = Math.Log(Math.Tan(QuarterPi + (pPrime / 2.0)));
            var g = (1 - Math.Cos(2.0 * Azimuth)) / 12.0;
            var gi = Math.Sin(2.0 * Azimuth) / 12.0;

            var x = FalseProjectedOffset.X + (R * (hi + (gi * hi * hi * hi)));
            var y = FalseProjectedOffset.Y + (R * (h + (g * h * h * h)));
            return new Point2(x, y);
        }

        public override ITransformation<Point2, GeographicCoordinate> GetInverse() {
            if (!HasInverse) throw new NoInverseException();
            Contract.Ensures(Contract.Result<ITransformation<Point2, GeographicCoordinate>>() != null);
            return new Inverse(this);
        }

        public override bool HasInverse {
            [Pure] get {
                // ReSharper disable CompareOfFloatsByEqualityOperator
                return 0 != R && !Double.IsNaN(R)
                    && 0 != Beta && !Double.IsNaN(Beta);
                // ReSharper restore CompareOfFloatsByEqualityOperator
            }
        }

    }
}
