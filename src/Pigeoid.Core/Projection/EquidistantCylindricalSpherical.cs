using System;
using Pigeoid.Transformation;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Projection
{
	public class EquidistantCylindricalSpherical : ProjectionBase
	{
		private class Inverse : InvertedTransformationBase<EquidistantCylindricalSpherical, Point2, GeographicCoordinate>
		{

			public Inverse(EquidistantCylindricalSpherical core) : base(core) { }

			public override GeographicCoordinate TransformValue(Point2 value) {
				return new GeographicCoordinate(
					(value.Y - Core.FalseProjectedOffset.Y) / Core.MajorAxis,
					Core._origin.Longitude + ((value.X - Core.FalseProjectedOffset.X)/Core._majorAxisCosLatitude)
				);
			}
		}

		private readonly GeographicCoordinate _origin;
		private readonly double _majorAxisCosLatitude;

		public EquidistantCylindricalSpherical(GeographicCoordinate origin, Vector2 falseProjectedOffset, ISpheroid<double> spheroid)
			: base(falseProjectedOffset, spheroid)
		{
			_origin = origin;
			_majorAxisCosLatitude = MajorAxis * Math.Cos(_origin.Latitude);
		}

		public override Point2 TransformValue(GeographicCoordinate source) {
			return new Point2(
				FalseProjectedOffset.X + (_majorAxisCosLatitude * (source.Longitude - _origin.Longitude)),
				FalseProjectedOffset.Y + (MajorAxis * source.Latitude)
			);
		}

		public override ITransformation<Point2, GeographicCoordinate> GetInverse() {
			if (!HasInverse)
				throw new InvalidOperationException("No inverse.");
			return new Inverse(this);
		}

		public override bool HasInverse {
// ReSharper disable CompareOfFloatsByEqualityOperator
			get { return 0 != MajorAxis && 0 != _majorAxisCosLatitude; }
// ReSharper restore CompareOfFloatsByEqualityOperator
		}
	}
}
