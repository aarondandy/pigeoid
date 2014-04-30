using System;
using System.Diagnostics.Contracts;
using Pigeoid.CoordinateOperation.Transformation;
using Vertesaur;
using Vertesaur.Transformation;

namespace Pigeoid.CoordinateOperation.Projection
{
    public class LambertCylindricalEqualArea : SpheroidProjectionBase
    {

        private class Inverse : InvertedTransformationBase<LambertCylindricalEqualArea, Point2, GeographicCoordinate>
        {

            private double Azimuth;
            private GeographicCoordinate P;
            private double BConstant;
            private double AConstant2;
            private double AConstant4;
            private double AConstant6;

            public Inverse(LambertCylindricalEqualArea core) : base(core) {
                Contract.Requires(core != null);
                // TODO: this is probably all wrong
                Azimuth = 0;
                P = new GeographicCoordinate(
                    Math.Asin(Math.Cos(core.GeographicOrigin.Latitude) * Math.Sin(Azimuth)),
                    Math.Atan(-Math.Cos(Azimuth) / (-Math.Sin(core.GeographicOrigin.Latitude) * Math.Sin(Azimuth))) + core.GeographicOrigin.Longitude);
                BConstant = 0.9991507126
                    + (-0.0008471537 * Math.Cos(2 * P.Latitude))
                    + (0.0000021283 * Math.Cos(4 * P.Latitude))
                    + (-0.0000000054 * Math.Cos(6 * P.Latitude));
                AConstant2 = -0.0001412090
                    + (-0.0001411258 * Math.Cos(2 * P.Latitude))
                    + (0.0000000839 * Math.Cos(4 * P.Latitude))
                    + (0.0000000006 * Math.Cos(6 * P.Latitude));
                AConstant4 = -0.0000000435
                    + (-0.0000000579 * Math.Cos(2 * P.Latitude))
                    + (-0.0000000144 * Math.Cos(4 * P.Latitude))
                    + (0 * Math.Cos(6 * P.Latitude));
                AConstant6 = 0;
            }

            public override GeographicCoordinate TransformValue(Point2 value) {
                throw new NotImplementedException("I have no idea what is going on.");
                var lon = value.X/(Core.MajorAxis*Core.Scale*BConstant);
                for (int i = 0; i < 8; i++) {
                    var newLon = (
                        (value.X/(Core.MajorAxis*Core.Scale))
                        - (AConstant2 * Math.Sin(lon * 2.0))
                        - (AConstant4 * Math.Sin(lon * 4.0))
                        - (AConstant6 * Math.Sin(lon * 6.0))
                    ) / BConstant;
                    if (newLon == lon)
                        break;
                    lon = newLon;
                }
            }
        }

        protected readonly GeographicCoordinate GeographicOrigin;
        protected readonly double Scale;

        public LambertCylindricalEqualArea(GeographicCoordinate geographicOrigin, Vector2 falseProjectedOffset, ISpheroid<double> spheroid)
            : base(falseProjectedOffset, spheroid) {
            Contract.Requires(spheroid != null);
            GeographicOrigin = geographicOrigin;
            var cosLatOrigin = Math.Cos(geographicOrigin.Latitude);
            var sinLatOrigin = Math.Sin(geographicOrigin.Latitude);
            Scale = cosLatOrigin/Math.Sqrt(1 - (ESq*sinLatOrigin*sinLatOrigin));
        }

        public override Point2 TransformValue(GeographicCoordinate source) {
            throw new NotImplementedException("I have no idea what is going on.");
            var sinLat = Math.Sin(source.Latitude);
            var q = (1 - ESq)*(
                (sinLat / (1 - (ESq * sinLat * sinLat)))
                - ((1.0 / (2.0 * E)) * Math.Log((1-(E * sinLat))/(1 + (E * sinLat))))
            );
            return new Point2(
                MajorAxis*Scale*(source.Longitude - GeographicOrigin.Longitude),
                MajorAxis * q / (2.0 * Scale));
        }

        public override ITransformation<Point2, GeographicCoordinate> GetInverse() {
            if (!HasInverse) throw new NoInverseException();
            Contract.Ensures(Contract.Result<ITransformation<Point2, GeographicCoordinate>>() != null);
            return new Inverse(this);
        }

        public override bool HasInverse {
            get {
                throw new NotImplementedException();
            }
        }

    }
}
