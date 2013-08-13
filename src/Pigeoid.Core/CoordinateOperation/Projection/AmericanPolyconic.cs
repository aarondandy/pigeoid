using System;
using System.Diagnostics.Contracts;
using Pigeoid.CoordinateOperation.Transformation;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.CoordinateOperation.Projection
{
    public class AmericanPolyconic : ProjectionBase
    {

        private class Inverted : InvertedTransformationBase<AmericanPolyconic, Point2, GeographicCoordinate>
        {
            public Inverted(AmericanPolyconic core) : base(core) {
                Contract.Requires(core != null);
            }

            public override GeographicCoordinate TransformValue(Point2 value) {
                var delta = value - Core.FalseProjectedOffset;
                if (Core.MOrigin == delta.Y)
                    return new GeographicCoordinate(0, Core.GeographicOrigin.Longitude + (delta.X / Core.MajorAxis));

                var a = (Core.MOrigin + delta.Y) / Core.MajorAxis;
                var b = (a * a) + ((delta.X * delta.X) / (Core.MajorAxis * Core.MajorAxis));
                var lat = a;
                double c = 0;
                for (int i = 0; i < 8; i++) {
                    var sinLat = Math.Sin(lat);
                    c = Math.Sqrt(1.0 - (Core.ESq * sinLat * sinLat)) * Math.Tan(lat);
                    var h = Core.CalculateMValue(lat);
                    var j = h / Core.MajorAxis;
                    var j2 = j * j;
                    var j2Pb = j2 + b;
                    var sinLat2 = Math.Sin(2.0 * lat);
                    if (sinLat2 == 0)
                        break;

                    var newLat = lat
                        - ((a * ((c * j) + 1.0)) - j - (c * j2Pb / 2.0))
                        / (
                            (Core.ESq * sinLat2 * (j2Pb - (2.0 * a * j)) / (4.0 * c))
                            + ((a - j) * ((c * h) - (2.0 / sinLat2)))
                            - h
                        );
                    if (newLat == lat)
                        break;

                    lat = newLat;
                }

                return new GeographicCoordinate(
                    lat,
                    Core.GeographicOrigin.Longitude
                        + ((Math.Asin(delta.X * c / Core.MajorAxis)) / Math.Sin(lat))
                );
            }
        }

        protected readonly GeographicCoordinate GeographicOrigin;
        protected readonly double MOrigin;
        protected readonly double E4;
        protected readonly double E6;

        public AmericanPolyconic(
            GeographicCoordinate geographicOrigin,
            Vector2 falseProjectedOffset,
            ISpheroid<double> spheroid
        ) : base(falseProjectedOffset, spheroid) {
            E4 = ESq * ESq;
            E6 = E4 * ESq;
            GeographicOrigin = geographicOrigin;
            MOrigin = CalculateMValue(geographicOrigin.Latitude);
        }

        private double CalculateMValue(double lat) {
            return (
                ((1.0 - (ESq / 4.0) - (E4 * (3.0 / 64.0)) - (E6 * (5.0 / 256.0))) * lat)
                - (((ESq * (3.0 / 8.0)) + (E4 * (3.0 / 32.0)) + (E6 * (45.0 / 1024.0))) * Math.Sin(lat * 2.0))
                + (((E4 * (15.0 / 256.0)) + (E6 * (45.0 / 1024.0))) * Math.Sin(lat * 4.0))
                - ((E6 * (35.0 / 3072.0)) * Math.Sin(lat * 6.0))
            ) * MajorAxis;
        }

        private static double Cot(double value) {
            return 1.0 / Math.Tan(value);
        }

        public override Point2 TransformValue(GeographicCoordinate source) {
            if (source.Latitude == 0) {
                return new Point2(
                    FalseProjectedOffset.X + (MajorAxis * (source.Longitude - GeographicOrigin.Longitude)),
                    FalseProjectedOffset.Y - MOrigin
                );
            }

            var m = CalculateMValue(source.Latitude);
            var sinLat = Math.Sin(source.Latitude);
            var v = MajorAxis / Math.Sqrt(1 - (ESq * sinLat * sinLat));
            var l = (source.Longitude - GeographicOrigin.Longitude) * sinLat;
            var cotLat = Cot(source.Latitude);
            return new Point2(
                FalseProjectedOffset.X + (v * cotLat * Math.Sin(l)),
                FalseProjectedOffset.Y + m - MOrigin + (v * cotLat * (1.0 - Math.Cos(l)))
            );
        }

        public override ITransformation<Point2, GeographicCoordinate> GetInverse() {
            if (!HasInverse) throw new NoInverseException();
            Contract.Ensures(Contract.Result<ITransformation<Point2, GeographicCoordinate>>() != null);
            return new Inverted(this);
        }

        public override bool HasInverse {
            [Pure] get {
                return MajorAxis != 0;
            }
        }
    }
}
