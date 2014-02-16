using System;
using System.Diagnostics.Contracts;
using Pigeoid.CoordinateOperation.Transformation;
using Vertesaur;
using Vertesaur.Transformation;

namespace Pigeoid.CoordinateOperation.Projection
{
    public class EquidistantCylindrical : SpheroidProjectionBase
    {

        private class Inverse : InvertedTransformationBase<EquidistantCylindrical, Point2, GeographicCoordinate>
        {

            private readonly double _uNumerator;
            private readonly double _coefLine1;
            private readonly double _coefLine2;
            private readonly double _coefLine3;
            private readonly double _coefLine4;
            private readonly double _coefLine5;
            private readonly double _coefLine6;
            private readonly double _coefLine7;

            public Inverse(EquidistantCylindrical core) : base(core) {
                Contract.Requires(core != null);
                var e2 = Core.ESq;
                var e4 = e2 * e2;
                var e6 = e4 * Core.ESq;
                var e8 = e6 * Core.ESq;
                var e10 = e8 * Core.ESq;
                var e12 = e10 * Core.ESq;
                var e14 = e12 * Core.ESq;
                _uNumerator = (
                    1.0
                    - (e2 / 4.0)
                    - (3.0 / 64.0 * e4)
                    - (5.0 / 256.0 * e6)
                    - (175.0 / 16384.0 * e8)
                    - (441.0 / 65536.0 * e10)
                    - (4851.0 / 1048576.0 * e12)
                    - (14157.0 / 4194304.0 * e14)
                ) * core.MajorAxis;
                var sqrtOneMinusESq = Math.Sqrt(1.0 - Core.ESq);
                var n = (1.0 - sqrtOneMinusESq) / (1.0 + sqrtOneMinusESq);
                var n2 = n * n;
                var n3 = n2 * n;
                var n4 = n3 * n;
                var n5 = n4 * n;
                var n6 = n5 * n;
                var n7 = n6 * n;
                _coefLine1 = (3.0 / 2.0 * n) - (27.0 / 32.0 * n3) + (269.0 / 512.0 * n5) - (6607.0 / 24576.0 * n7);
                _coefLine2 = (21.0 / 16.0 * n2) - (5.0 / 32.0 * n4) + (6759.0 / 4096.0 * n6);
                _coefLine3 = (151.0 / 96.0 * n3) - (417.0 / 128.0 * n5) + (87963.0 / 20480.0 * n7);
                _coefLine4 = (1097.0 / 512.0 * n4) - (15543.0 / 2560.0 * n6);
                _coefLine5 = (8011.0 / 2560.0 * n5) - (69119.0 / 6144.0 * n7);
                _coefLine6 = (293393.0 / 61440.0 * n6);
                _coefLine7 = (6845701.0 / 860160.0 * n7);
            }

            public override GeographicCoordinate TransformValue(Point2 value) {
                var x = value.X - Core.FalseProjectedOffset.X;
                var y = value.Y - Core.FalseProjectedOffset.Y;
                var u = y / _uNumerator;
                var lon = (x / Core._vCosLatOrigin) + Core.Origin.Longitude;
                var lat = (
                    (Math.Sin(2 * u) * _coefLine1)
                    + (Math.Sin(4 * u) * _coefLine2)
                    + (Math.Sin(6 * u) * _coefLine3)
                    + (Math.Sin(8 * u) * _coefLine4)
                    + (Math.Sin(10 * u) * _coefLine5)
                    + (Math.Sin(12 * u) * _coefLine6)
                    + (Math.Sin(14 * u) * _coefLine7)
                ) + u;
                return new GeographicCoordinate(lat, lon);
            }
        }

        private readonly GeographicCoordinate _origin;
        private readonly double _coefLine1;
        private readonly double _coefLine2;
        private readonly double _coefLine3;
        private readonly double _coefLine4;
        private readonly double _coefLine5;
        private readonly double _coefLine6;
        private readonly double _coefLine7;
        private readonly double _coefLine8;
        private readonly double _vCosLatOrigin;

        public EquidistantCylindrical(GeographicCoordinate origin, Vector2 falseProjectedOffset, ISpheroid<double> spheroid)
            : base(falseProjectedOffset, spheroid)
        {
            Contract.Requires(spheroid != null);
            _origin = origin;
            var e4 = ESq * ESq;
            var e6 = e4 * ESq;
            var e8 = e6 * ESq;
            var e10 = e8 * ESq;
            var e12 = e10 * ESq;
            var e14 = e12 * ESq;
            _coefLine1 = 1.0 - (ESq / 4.0)
                - (3.0 / 64.0 * e4)
                - (5.0 / 256.0 * e6)
                - (175.0 / 16384.0 * e8)
                - (441.0 / 65536.0 * e10)
                - (4851.0 / 1048576.0 * e12)
                - (14157.0 / 4194304.0 * e14);
            _coefLine2 = (-3.0 / 8.0 * ESq)
                - (3.0 / 32.0 * e4)
                - (45.0 / 1024.0 * e6)
                - (105.0 / 4096.0 * e8)
                - (2205.0 / 131072.0 * e10)
                - (6237.0 / 524288.0 * e12)
                - (297297.0 / 33554432.0 * e14);
            _coefLine3 = (15.0 / 256.0 * e4)
                + (45.0 / 1024.0 * e6)
                + (525.0 / 16384.0 * e8)
                + (1575.0 / 65536.0 * e10)
                + (155925.0 / 8388608.0 * e12)
                + (495495.0 / 33554432.0 * e14);
            _coefLine4 = (-35.0 / 3072.0 * e6)
                - (175.0 / 12288.0 * e8)
                - (3675.0 / 262144.0 * e10)
                - (13475.0 / 1048576.0 * e12)
                - (385385.0 / 33554432.0 * e14);
            _coefLine5 = (315.0 / 131072.0 * e8)
                + (2205.0 / 524288.0 * e10)
                + (43659.0 / 8388608.0 * e12)
                + (189189.0 / 33554432.0 * e14);
            _coefLine6 = (-693.0 / 1310720.0 * e10)
                - (6237.0 / 5242880.0 * e12)
                - (297297.0 / 167772160.0 * e14);
            _coefLine7 = (1001.0 / 8388608.0 * e12)
                + (11011.0 / 33554432.0 * e14);
            _coefLine8 = (-6435.0 / 234881024.0 * e14);

            var sinLatOrigin = Math.Sin(_origin.Latitude);
            var v = MajorAxis / Math.Sqrt(1.0 - (ESq * sinLatOrigin * sinLatOrigin));
            _vCosLatOrigin = v * Math.Cos(_origin.Latitude);
        }

        public GeographicCoordinate Origin { get { return _origin; } }

        public override Point2 TransformValue(GeographicCoordinate source) {
            var m = (
                (_coefLine1 * source.Latitude)
                + (_coefLine2 * Math.Sin(2 * source.Latitude))
                + (_coefLine3 * Math.Sin(4 * source.Latitude))
                + (_coefLine4 * Math.Sin(6 * source.Latitude))
                + (_coefLine5 * Math.Sin(8 * source.Latitude))
                + (_coefLine6 * Math.Sin(10 * source.Latitude))
                + (_coefLine7 * Math.Sin(12 * source.Latitude))
                + (_coefLine8 * Math.Sin(14 * source.Latitude))
            ) * MajorAxis;
            return new Point2(
                ((source.Longitude - _origin.Longitude) * _vCosLatOrigin) + FalseProjectedOffset.X,
                m + FalseProjectedOffset.Y
            );
        }

        public override ITransformation<Point2, GeographicCoordinate> GetInverse() {
            if (!HasInverse) throw new NoInverseException();
            Contract.Ensures(Contract.Result<ITransformation<Point2, GeographicCoordinate>>() != null);
            return new Inverse(this);
        }

        public override bool HasInverse {
            [Pure] get {
                // ReSharper disable CompareOfFloatsByEqualityOperator
                return 0 != _vCosLatOrigin;
                // ReSharper restore CompareOfFloatsByEqualityOperator
            }
        }
    }
}
