using System;
using System.Diagnostics.Contracts;
using Pigeoid.CoordinateOperation.Transformation;
using Vertesaur;
using Vertesaur.Transformation;

namespace Pigeoid.CoordinateOperation.Projection
{
    public class AlbersEqualArea : SpheroidProjectionBase
    {

        private class Inverted : InvertedTransformationBase<AlbersEqualArea, Point2, GeographicCoordinate>
        {

            private readonly double E4;
            private readonly double E6;

            public Inverted(AlbersEqualArea core) : base(core) {
                Contract.Requires(core != null);
                E4 = Core.ESq*Core.ESq;
                E6 = E4*Core.ESq;
            }

            public override GeographicCoordinate TransformValue(Point2 value) {
                var delta = value - Core.FalseProjectedOffset;
                var poMinuxY = Core.POrigin - delta.Y;
                var theta = poMinuxY == 0 ? 0 : Math.Atan(delta.X / poMinuxY);
                var p = Math.Sqrt((delta.X * delta.X) + (poMinuxY * poMinuxY));
                var alphaPrime = (Core.C - (p * p * Core.N * Core.N / (Core.MajorAxis * Core.MajorAxis)))/Core.N;
                var betaPrime = Math.Asin(
                    alphaPrime / (
                        1.0
                        - (
                            ((1.0 - Core.ESq)/(2.0 * Core.E))
                            * Math.Log((1.0 - Core.E)/(1.0 + Core.E))
                        )
                    )
                );

                var lat = betaPrime
                    + (((Core.ESq / 3.0) + (E4 * 31.0 / 180.0) + (E6 * 517.0 / 5040.0)) * Math.Sin(2.0 * betaPrime))
                    + (((E4 * 23.0 / 360.0) + (E6 * 251.0 / 3780.0)) * Math.Sin(4.0 * betaPrime))
                    + ((E6 * 761.0 / 45360.0) * Math.Sin(6.0 * betaPrime));
                var lon = Core.GeographicOrigin.Longitude + (theta/Core.N);
                return new GeographicCoordinate(lat,lon);
            }
        }

        protected readonly GeographicCoordinate GeographicOrigin;
        protected readonly double Latitude1stStandardParallel;
        protected readonly double Latitude2ndStandardParallel;

        protected readonly double M1;
        protected readonly double M2;
        protected readonly double AlphaOrigin;
        protected readonly double Alpha1;
        protected readonly double Alpha2;
        protected readonly double InvTwoE;
        protected readonly double OneMinusESq;
        protected readonly double N;
        protected readonly double C;
        protected readonly double POrigin;

        public AlbersEqualArea(
            GeographicCoordinate geographicOrigin,
            double latitude1stStandardParallel,
            double latitude2ndStandardParallel,
            Vector2 falseProjectedOffset,
            ISpheroid<double> spheroid
        ) : base(falseProjectedOffset, spheroid) {
            Contract.Requires(spheroid != null);
            GeographicOrigin = geographicOrigin;
            Latitude1stStandardParallel = latitude1stStandardParallel;
            var sinLatParallel1 = Math.Sin(latitude1stStandardParallel);
            Latitude2ndStandardParallel = latitude2ndStandardParallel;
            var sinLatParallel2 = Math.Sin(latitude2ndStandardParallel);
            M1 = Math.Cos(latitude1stStandardParallel)/Math.Sqrt(1.0 - (ESq*sinLatParallel1*sinLatParallel1));
            M2 = Math.Cos(latitude2ndStandardParallel)/Math.Sqrt(1.0 - (ESq*sinLatParallel2*sinLatParallel2));
            InvTwoE = 1.0/(2.0*E);
            OneMinusESq = (1.0 - ESq);
            AlphaOrigin = CalculateAlpha(Math.Sin(geographicOrigin.Latitude));
            Alpha1 = CalculateAlpha(sinLatParallel1);
            Alpha2 = CalculateAlpha(sinLatParallel2);
            var m1Sq = M1*M1;
            var m2Sq = M2*M2;
            N = (m1Sq - m2Sq)/(Alpha2 - Alpha1);
            C = m1Sq + (N*Alpha1);
            POrigin = (MajorAxis * Math.Sqrt(C - (N * AlphaOrigin))) / N;
        }

        private double CalculateAlpha(double sinLat) {
            var eSinLat = E*sinLat;
            return OneMinusESq * (
                (sinLat / (1.0 - (ESq * sinLat * sinLat)))
                - (InvTwoE * Math.Log((1.0 - eSinLat) / (1.0 + eSinLat)))
            );
        }

        public override Point2 TransformValue(GeographicCoordinate source) {
            var sinLat = Math.Sin(source.Latitude);
            var alpha = CalculateAlpha(sinLat);
            var p = (MajorAxis*Math.Sqrt(C - (N*alpha)))/N;
            var theta = N*(source.Longitude - GeographicOrigin.Longitude);
            return new Point2(
                FalseProjectedOffset.X + (p * Math.Sin(theta)),
                FalseProjectedOffset.Y + POrigin - (p * Math.Cos(theta)));
        }

        public override ITransformation<Point2, GeographicCoordinate> GetInverse() {
            if (!HasInverse) throw new NoInverseException();
            Contract.Ensures(Contract.Result<ITransformation<Point2, GeographicCoordinate>>() != null);
            return new Inverted(this);
        }

        public override bool HasInverse {
            [Pure] get { return N != 0 && MajorAxis != 0; }
        }
    }
}
