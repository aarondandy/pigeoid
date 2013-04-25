using System;
using System.Diagnostics.Contracts;
using Pigeoid.Transformation;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Projection
{
    public class LambertCylindricalEqualAreaSpherical : ProjectionBase
    {

        private class Inverse : InvertedTransformationBase<LambertCylindricalEqualAreaSpherical, Point2, GeographicCoordinate>
        {

            public Inverse(LambertCylindricalEqualAreaSpherical core) : base(core) {
                Contract.Requires(core != null);
            }

            public override GeographicCoordinate TransformValue(Point2 value) {
                return new GeographicCoordinate(
                    Math.Asin(((value.Y - Core.FalseProjectedOffset.Y) / Core.R) * Core.CosLatOrigin),
                    Core.GeographicOrigin.Longitude + ((value.X - Core.FalseProjectedOffset.X) / (Core.R * Core.CosLatOrigin))
                );
            }
        }

        protected readonly GeographicCoordinate GeographicOrigin;
        protected readonly double R;
        protected readonly double CosLatOrigin;

        public LambertCylindricalEqualAreaSpherical(GeographicCoordinate origin, Vector2 offset, ISpheroid<double> spheroid)
            : base(offset, spheroid)
        {
            Contract.Requires(spheroid != null);
            // ReSharper disable CompareOfFloatsByEqualityOperator
            GeographicOrigin = origin;
            CosLatOrigin = Math.Cos(GeographicOrigin.Latitude);
            R = MajorAxis;
            if (E != 0) {
                R *= Math.Sqrt((1 - (((1.0 - ESq) / (2.0 * E)) * Math.Log((1 - E) / (1 + E)))) / 2.0);
            }
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        public override Point2 TransformValue(GeographicCoordinate source) {
            return new Point2(
                FalseProjectedOffset.X + (R * (source.Longitude - GeographicOrigin.Longitude) * CosLatOrigin),
                FalseProjectedOffset.Y + (R * Math.Sin(source.Latitude) / CosLatOrigin)
            );
        }

        public override ITransformation<Point2, GeographicCoordinate> GetInverse() {
            if (!HasInverse) throw new InvalidOperationException("No inverse.");
            Contract.Ensures(Contract.Result<ITransformation<Point2, GeographicCoordinate>>() != null);
            return new Inverse(this);
        }

        public override bool HasInverse {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            [Pure] get {
                return 0 != R && !Double.IsNaN(R)
                    && 0 != CosLatOrigin && !Double.IsNaN(CosLatOrigin);
            }
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }
    }
}
