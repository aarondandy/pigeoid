using System;
using System.Diagnostics.Contracts;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Projection
{
    /// <summary>
    /// A Lambert Conic Conformal projection with 1 standard parallel.
    /// </summary>
    public class LambertConicConformal1Sp :
        LambertConicConformal,
        IEquatable<LambertConicConformal1Sp>
    {

        [Obsolete]
        internal new const string DefaultName = LambertConicConformal.DefaultName + " 1SP";

        /// <summary>
        /// Origin scale factor.
        /// </summary>
        public readonly double OriginScaleFactor;

        /// <summary>
        /// Constructs a new Lambert Conic Conformal projection from 1 standard parallel.
        /// </summary>
        /// <param name="geographicOrigin">The geographic origin.</param>
        /// <param name="originScaleFactor">The scale factor at the origin.</param>
        /// <param name="falseProjectedOffset">The false projected offset.</param>
        /// <param name="spheroid">The spheroid.</param>
        /// <exception cref="System.ArgumentException">An invalid N value was generated from the <paramref name="geographicOrigin">geographic origin</paramref> latitude.</exception>
        public LambertConicConformal1Sp(
            GeographicCoordinate geographicOrigin,
            double originScaleFactor,
            Vector2 falseProjectedOffset,
            ISpheroid<double> spheroid
        ) : base(geographicOrigin, falseProjectedOffset, spheroid) {
            Contract.Requires(spheroid != null);
            OriginScaleFactor = originScaleFactor;
            var originParallelSin = Math.Sin(geographicOrigin.Latitude);
            var eParallelSin = E * originParallelSin;
            var tOrigin = Math.Tan(QuarterPi - (geographicOrigin.Latitude / 2.0))
                / Math.Pow((1.0 - eParallelSin) / (1.0 + eParallelSin), EHalf);
            N = Math.Sin(geographicOrigin.Latitude);

            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (0 == N || Double.IsNaN(N)) throw new ArgumentException("Invalid N value.", "geographicOrigin");
            // ReSharper restore CompareOfFloatsByEqualityOperator

            F = (Math.Cos(geographicOrigin.Latitude) / Math.Sqrt(1.0 - (ESq * originParallelSin * originParallelSin)))
                / (N * Math.Pow(tOrigin, N));
            Af = MajorAxis * F * originScaleFactor;
            ROrigin = Af * Math.Pow(tOrigin, N);
            InvN = 1.0 / N;
            NorthingOffset = falseProjectedOffset.Y + ROrigin;
        }

        public bool Equals(LambertConicConformal1Sp other) {
            return !ReferenceEquals(other, null)
                && (
                    GeographicOrigin.Equals(other.GeographicOrigin)
                // ReSharper disable CompareOfFloatsByEqualityOperator
                    && OriginScaleFactor == other.OriginScaleFactor
                // ReSharper restore CompareOfFloatsByEqualityOperator
                    && FalseProjectedOffset.Equals(other.FalseProjectedOffset)
                    && Spheroid.Equals(other.Spheroid)
                )
            ;
        }

        public override bool Equals(object obj) {
            return null != obj
                && (ReferenceEquals(this, obj) || Equals(obj as LambertConicConformal1Sp));
        }

        public override int GetHashCode() {
            return -GeographicOrigin.GetHashCode() ^ OriginScaleFactor.GetHashCode();
        }

    }
}
