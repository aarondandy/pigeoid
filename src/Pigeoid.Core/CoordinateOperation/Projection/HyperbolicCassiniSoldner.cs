using System;
using System.Diagnostics.Contracts;
using Pigeoid.CoordinateOperation.Transformation;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.CoordinateOperation.Projection
{
    /// <summary>
    /// A hyperbolic Cassini Soldner projection.
    /// </summary>
    public class HyperbolicCassiniSoldner :
        CassiniSoldner,
        IEquatable<HyperbolicCassiniSoldner>
    {

        protected readonly double OneMinusESqMajorAxisSq6;

        /// <summary>
        /// The inverse of a hyperbolic Cassini Soldner projection.
        /// </summary>
        private class Inverted : InvertedTransformationBase<HyperbolicCassiniSoldner, Point2, GeographicCoordinate>
        {

            public Inverted(HyperbolicCassiniSoldner core)
                : base(core)
            {
                Contract.Requires(core != null);
            }

            public override GeographicCoordinate TransformValue(Point2 coordinate) {
                var dx = coordinate.X - Core.FalseProjectedOffset.X;
                var dy = coordinate.Y - Core.FalseProjectedOffset.Y;
                var e1 = (1 - Math.Sqrt(1 - Core.ESq)) / (1 + Math.Sqrt(1 - Core.ESq));
                var e2 = e1 * e1;
                var e3 = e2 * e1;
                var e4 = e3 * e1;
                var lat1Prime = Core.NaturalOrigin.Latitude + (dy / 315320.0); // what is with this magic number?
                var sinLat1Prime = Math.Sin(lat1Prime);
                var sinLat1PrimeSq = sinLat1Prime * sinLat1Prime;
                var vPrime = Core.MajorAxis / Math.Sqrt(1 - (Core.ESq * sinLat1PrimeSq));
                var pPrime = Core.MajorAxis * (1 - Core.ESq) / Math.Pow(1 - (Core.ESq * sinLat1PrimeSq), 1.5);
                var qPrime = dy * dy * dy / (6.0 * pPrime * vPrime);
                var qPrimeDy = (dy + qPrime);
                var q = (qPrimeDy * qPrimeDy * qPrimeDy) / (6.0 * pPrime * vPrime);
                var m = Core.MOrigin + dy + q;
                var u = m / (Core.MajorAxis * Core.MLineCoefficient1);

                var lat1 = u
                    + (((e1 * 3.0 / 2.0) - (e3 * 27.0 / 32.0)) * Math.Sin(2.0 * u))
                    + (((e2 * 21.0 / 16.0) - (e4 * 55.0 / 32.0)) * Math.Sin(4.0 * u))
                    + ((e3 * 151.0 / 96.0) * Math.Sin(6.0 * u))
                    + ((e4 * 1097.0 / 512.0) * Math.Sin(8.0 * u));
                var sinLat1 = Math.Sin(lat1);
                var sinLat1Sq = sinLat1 * sinLat1;
                var p = Core.MajorAxis * (1 - Core.ESq) * Math.Pow(1 - (Core.ESq * sinLat1Sq), 1.5);
                var v = Core.MajorAxis / Math.Sqrt(1 - (Core.ESq * sinLat1Sq));
                var tanLat1 = Math.Tan(lat1);
                var t = tanLat1 * tanLat1;
                var d = dx / v;
                var lat = lat1 - (
                    (v * tanLat1 / p)
                    * ((d * d / 2.0) - ((1 + t * t * t) * d * d * d * d / 24.0)));
                var lon = Core.NaturalOrigin.Longitude + (
                    (
                        d
                        - (t * d * d * d / 3.0)
                        + ((1.0 + (3.0 * t)) * t * d * d * d * d * d / 15.0)
                    )
                    / Math.Cos(lat1)
                );
                return new GeographicCoordinate(lat, lon);
            }

        }

        /// <summary>
        /// Constructs a new hyperbolic cassini soldner projection.
        /// </summary>
        /// <param name="naturalOrigin">The natural origin.</param>
        /// <param name="falseProjectedOffset">The false projected offset.</param>
        /// <param name="spheroid">The spheroid.</param>
        public HyperbolicCassiniSoldner(
            GeographicCoordinate naturalOrigin,
            Vector2 falseProjectedOffset,
            ISpheroid<double> spheroid
        )
            : base(naturalOrigin, falseProjectedOffset, spheroid)
        {
            Contract.Requires(spheroid != null);
            OneMinusESqMajorAxisSq6 = OneMinusESq * MajorAxis * MajorAxis * 6.0;
        }

        public override ITransformation<Point2, GeographicCoordinate> GetInverse() {
            Contract.Ensures(Contract.Result<ITransformation<Point2, GeographicCoordinate>>() != null);
            return new Inverted(this);
        }

        public override bool HasInverse {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            [Pure] get { return base.HasInverse && 0 != OneMinusESq && !Double.IsNaN(OneMinusESq); }
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        public override Point2 TransformValue(GeographicCoordinate coordinate) {
            var sinLat = Math.Sin(coordinate.Latitude);
            var c = Math.Cos(coordinate.Latitude);
            var a = (coordinate.Longitude - NaturalOrigin.Longitude) * c;
            c = (ESecSq * c * c);
            var tanLat = Math.Tan(coordinate.Latitude);
            var t = tanLat * tanLat;
            var aSq = a * a;
            var lesq = Math.Sin(coordinate.Latitude); lesq = 1.0 - (ESq * lesq * lesq);
            var v = MajorAxis / Math.Sqrt(lesq);

            var m = MajorAxis * (
                (MLineCoefficient1 * coordinate.Latitude)
                - (MLineCoefficient2 * Math.Sin(2.0 * coordinate.Latitude))
                + (MLineCoefficient3 * Math.Sin(4.0 * coordinate.Latitude))
                - (MLineCoefficient4 * Math.Sin(6.0 * coordinate.Latitude))
            );

            var a4 = aSq * aSq;

            var x = m - MOrigin + (Math.Tan(coordinate.Latitude) * v * (
                (aSq / 2.0)
                + ((5 - t + (c * 6.0)) * a4 / 24.0)
            ));

            var p = (MajorAxis * (1 - ESq)) / Math.Pow(1 - (ESq * sinLat * sinLat), 1.5);

            return new Point2(
                FalseProjectedOffset.X + (v * (
                    a
                    - (t * a * a * a / 6.0)
                    - ((8.0 - t + (8.0 * c)) * t * a4 * a / 120.0)
                )),
                FalseProjectedOffset.Y + x - (x * x * x / (6.0 * p * v))
            );
        }

        public bool Equals(HyperbolicCassiniSoldner other) {
            return !ReferenceEquals(other, null)
                && (
                    NaturalOrigin.Equals(other.NaturalOrigin)
                    && FalseProjectedOffset.Equals(other.FalseProjectedOffset)
                    && Spheroid.Equals(other.Spheroid)
                )
            ;
        }

        public override bool Equals(object obj) {
            return null != obj
                && (ReferenceEquals(this, obj) || Equals(obj as HyperbolicCassiniSoldner));
        }

        public override int GetHashCode() {
            return NaturalOrigin.GetHashCode() ^ (FalseProjectedOffset.GetHashCode());
        }

    }
}
