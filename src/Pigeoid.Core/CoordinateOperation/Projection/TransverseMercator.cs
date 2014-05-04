using System;
using System.Diagnostics.Contracts;
using Pigeoid.CoordinateOperation.Transformation;
using Vertesaur;
using Vertesaur.Transformation;

namespace Pigeoid.CoordinateOperation.Projection
{
    /// <summary>
    /// A transverse Mercator projection.
    /// </summary>
    public class TransverseMercator : SpheroidProjectionBase
    {

        private class Inverted : InvertedTransformationBase<TransverseMercator, Point2, GeographicCoordinate>
        {

            private readonly double InverseCoefficient1;
            private readonly double InverseCoefficient2;
            private readonly double InverseCoefficient3;
            private readonly double InverseCoefficient4;
            private readonly double PrimaryScaleFactor;

            public Inverted(TransverseMercator core) : base(core) {
                Contract.Requires(core != null);
                Contract.Requires(core.Spheroid != null);
                var n = core.Spheroid.F / (2.0 - core.Spheroid.F);
                var n2 = n * n;
                var n3 = n2 * n;
                var n4 = n3 * n;
                InverseCoefficient1 = (n / 2.0) - (n2 * 2.0 / 3.0) + (n3 * 37.0 / 96.0) - (n4 / 360.0);
                InverseCoefficient2 = (n2 / 48.0) + (n3 / 15.0) - (n4 * 437.0 / 1440.0);
                InverseCoefficient3 = (n3 * 17.0 / 480.0) - (n4 * 37.0 / 840.0);
                InverseCoefficient4 = (n4 * 4397 / 161280.0);
                PrimaryScaleFactor = core.ValueB * core.ScaleFactor;
            }

            public override GeographicCoordinate TransformValue(Point2 coordinate) {
                // ReSharper disable CompareOfFloatsByEqualityOperator
                var n = (coordinate.X - Core.FalseProjectedOffset.X)
                    / PrimaryScaleFactor;
                var c = (
                    (coordinate.Y - Core.FalseProjectedOffset.Y)
                    + (Core.ScaleFactor * Core.MOrigin)
                ) / PrimaryScaleFactor;

                var n1 = Math.Cos(c * 2.0) * Math.Sinh(n * 2.0) * InverseCoefficient1;
                var n2 = Math.Cos(c * 4.0) * Math.Sinh(n * 4.0) * InverseCoefficient2;
                var n3 = Math.Cos(c * 6.0) * Math.Sinh(n * 6.0) * InverseCoefficient3;
                var n4 = Math.Cos(c * 8.0) * Math.Sinh(n * 8.0) * InverseCoefficient4;
                var n0 = n - n1 - n2 - n3 - n4;

                var c1 = Math.Sin(c * 2.0) * Math.Cosh(n * 2.0) * InverseCoefficient1;
                var c2 = Math.Sin(c * 4.0) * Math.Cosh(n * 4.0) * InverseCoefficient2;
                var c3 = Math.Sin(c * 6.0) * Math.Cosh(n * 6.0) * InverseCoefficient3;
                var c4 = Math.Sin(c * 8.0) * Math.Cosh(n * 8.0) * InverseCoefficient4;
                var c0 = c - c1 - c2 - c3 - c4;

                var beta = Math.Asin(Math.Sin(c0) / Math.Cosh(n0));
                var qPrime = ArcSinH(Math.Tan(beta));
                var q = qPrime;
                for (int i = 0; i < 16; i++) {
                    var oldQ = q;
                    q = qPrime + (Core.E * ArcTanH(Core.E * Math.Tanh(q)));
                    if (q == oldQ)
                        break;
                }

                return new GeographicCoordinate(
                    Math.Atan(Math.Sinh(q)),
                    Core.NaturalOrigin.Longitude + Math.Asin(Math.Tanh(n0) / Math.Cos(beta))
                );
                // ReSharper restore CompareOfFloatsByEqualityOperator
            }

        }

        private static double ArcSinH(double x) {
            return Math.Log(Math.Sqrt((x * x) + 1) + x);
        }

        private static double ArcTanH(double x) {
            return Math.Log((1 + x) / (1 - x)) / 2.0;
        }

        protected readonly double MOrigin;
        protected readonly double ScaleFactor;
        protected readonly GeographicCoordinate NaturalOrigin;
        protected readonly double ValueB;
        protected readonly double Coefficient1;
        protected readonly double Coefficient2;
        protected readonly double Coefficient3;
        protected readonly double Coefficient4;

        /// <summary>
        /// Constructs a new Transverse Mercator projection.
        /// </summary>
        /// <param name="naturalOrigin">The natural origin.</param>
        /// <param name="falseProjectedOffset">The false projected offset.</param>
        /// <param name="scaleFactor">The scale factor.</param>
        /// <param name="spheroid">The spheroid.</param>
        public TransverseMercator(
            GeographicCoordinate naturalOrigin,
            Vector2 falseProjectedOffset,
            double scaleFactor,
            ISpheroid<double> spheroid
        ) : base(falseProjectedOffset, spheroid) {
            Contract.Requires(spheroid != null);
            // ReSharper disable CompareOfFloatsByEqualityOperator
            NaturalOrigin = naturalOrigin;
            ScaleFactor = scaleFactor;
            var n = spheroid.F / (2.0 - spheroid.F);
            var n2 = n * n;
            var n3 = n2 * n;
            var n4 = n3 * n;

            ValueB = (spheroid.A / (1 + n)) * (1 + (n * n / 4.0) + (n * n * n * n / 64.0));
            Coefficient1 = (n / 2.0) - (n2 * 2.0 / 3.0) + (n3 * 5.0 / 16.0) + (n4 * 41.0 / 180.0);
            Coefficient2 = (n2 * 13.0 / 48.0) - (n3 * 3.0 / 5.0) + (n4 * 557.0 / 1440.0);
            Coefficient3 = (n3 * 61.0 / 240.0) - (n4 * 103 / 140);
            Coefficient4 = n4 * 49561.0 / 161280.0;

            if (naturalOrigin.Latitude == 0)
                MOrigin = 0;
            else if (naturalOrigin.Latitude == HalfPi)
                MOrigin = ValueB * HalfPi;
            else if (naturalOrigin.Latitude == -HalfPi)
                MOrigin = ValueB * -HalfPi;
            else if (Math.Abs(Math.Asin(naturalOrigin.Latitude) - HalfPi) < 0.00000000001) {
                var e2 = ESq;
                var e4 = e2 * e2;
                var e6 = e4 * e2;
                var eCoefficient1 = 1 - (e2 / 4.0) - (e4 * 3.0 / 64.0) - (e6 * 5.0 / 256.0);
                var eCoefficient2 = (e2 * 3.0 / 8.0) + (e4 * 3.0 / 32.0) + (e6 * 45.0 / 1024.0);
                var eCoefficient4 = (e4 * 15.0 / 256.0) + (e6 * 45.0 / 1024.0);
                var eCoefficient6 = (e6 * 35.0 / 3072.0);
                MOrigin = (
                    (naturalOrigin.Latitude * eCoefficient1)
                    - (Math.Sin(naturalOrigin.Latitude * 2.0) * eCoefficient2)
                    + (Math.Sin(naturalOrigin.Latitude * 4.0) * eCoefficient4)
                    - (Math.Sin(naturalOrigin.Latitude * 6.0) * eCoefficient6)
                ) * spheroid.A;
            }
            else {
                var qOrigin = ArcSinH(Math.Tan(naturalOrigin.Latitude))
                    - (E * ArcTanH(E * Math.Sin(naturalOrigin.Latitude)));
                var betaOrigin = Math.Atan(Math.Sinh(qOrigin));
                var goo0 = Math.Asin(Math.Sin(betaOrigin)); // betaOrigin?
                var goo1 = Coefficient1 * Math.Sin(2 * goo0);
                var goo2 = Coefficient2 * Math.Sin(4 * goo0);
                var goo3 = Coefficient3 * Math.Sin(6 * goo0);
                var goo4 = Coefficient4 * Math.Sin(8 * goo0);
                MOrigin = (goo0 + goo1 + goo2 + goo3 + goo4) * ValueB;
            }
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        public override ITransformation<Point2, GeographicCoordinate> GetInverse() {
            Contract.Ensures(Contract.Result<ITransformation<Point2, GeographicCoordinate>>() != null);
            return new Inverted(this);
        }

        public override bool HasInverse {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            [Pure] get { return true; }
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        public override Point2 TransformValue(GeographicCoordinate coordinate) {
            var q = ArcSinH(Math.Tan(coordinate.Latitude))
                - (ArcTanH(Math.Sin(coordinate.Latitude) * E) * E);
            var beta = Math.Atan(Math.Sinh(q));
            var n0 = ArcTanH(
                Math.Cos(beta)
                * Math.Sin(coordinate.Longitude - NaturalOrigin.Longitude)
            );
            var c0 = Math.Asin(Math.Sin(beta) * Math.Cosh(n0));
            var n1 = Math.Cos(c0 * 2.0) * Math.Sinh(n0 * 2.0) * Coefficient1;
            var n2 = Math.Cos(c0 * 4.0) * Math.Sinh(n0 * 4.0) * Coefficient2;
            var n3 = Math.Cos(c0 * 6.0) * Math.Sinh(n0 * 6.0) * Coefficient3;
            var n4 = Math.Cos(c0 * 8.0) * Math.Sinh(n0 * 8.0) * Coefficient4;
            var n = n0 + n1 + n2 + n3 + n4;
            var c1 = Math.Sin(c0 * 2.0) * Math.Cosh(n0 * 2.0) * Coefficient1;
            var c2 = Math.Sin(c0 * 4.0) * Math.Cosh(n0 * 4.0) * Coefficient2;
            var c3 = Math.Sin(c0 * 6.0) * Math.Cosh(n0 * 6.0) * Coefficient3;
            var c4 = Math.Sin(c0 * 8.0) * Math.Cosh(n0 * 8.0) * Coefficient4;
            var c = c0 + c1 + c2 + c3 + c4;

            return new Point2(
                (ScaleFactor * ValueB * n) + FalseProjectedOffset.X,
                (((ValueB * c) - MOrigin) * ScaleFactor) + FalseProjectedOffset.Y
            );

        }

    }
}
