using System;
using System.Collections.Generic;
using System.Linq;
using Pigeoid.Transformation;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Projection
{
	/// <summary>
	/// A spherical Mercator projection.
	/// </summary>
	public class MercatorSpherical :
		ITransformation<GeographicCoordinate, Point2>,
		IEquatable<MercatorSpherical>
	{
		/// <summary>
		/// False projected offset.
		/// </summary>
		public readonly Vector2 FalseProjectedOffset;
		/// <summary>
		/// Origin of longitude.
		/// </summary>
		public readonly double OriginLongitude;
		/// <summary>
		/// Radius.
		/// </summary>
		public readonly double Radius;

		private class Inverted : InvertedTransformationBase<MercatorSpherical,Point2,GeographicCoordinate>
		{

			public Inverted(MercatorSpherical core) : base(core) { }

			public override GeographicCoordinate TransformValue(Point2 coordinate) {
				return new GeographicCoordinate(
					(ProjectionBase.HalfPi - (2.0 * Math.Atan(Math.Pow(Math.E, (Core.FalseProjectedOffset.Y - coordinate.Y) / Core.Radius)))),
					((coordinate.X - Core.FalseProjectedOffset.X) / Core.Radius) + Core.OriginLongitude
				);
			}

		}

		/// <summary>
		/// Creates a Spherical Mercator projection.
		/// </summary>
		/// <param name="falseProjectedOffset">The false projected offset.</param>
		/// <param name="originLongitude">The origin of longitude.</param>
		/// <param name="radius">The radius.</param>
		public MercatorSpherical(
			Vector2 falseProjectedOffset,
			double originLongitude,
			double radius
		) {
			FalseProjectedOffset = falseProjectedOffset;
			OriginLongitude = originLongitude;
			Radius = radius;
		}

		/// <summary>
		/// Creates a spherical Mercator projection.
		/// </summary>
		/// <param name="radius">The radius.</param>
		public MercatorSpherical(double radius) : this(Vector2.Zero, 0, radius) { }

		public Point2 TransformValue(GeographicCoordinate coordinate) {
			return new Point2(
				FalseProjectedOffset.X + (Radius * (coordinate.Longitude - OriginLongitude)),
				FalseProjectedOffset.Y + (Radius * Math.Log(Math.Tan(ProjectionBase.QuarterPi + (coordinate.Latitude / 2.0))))
			);
		}

		public ITransformation<Point2, GeographicCoordinate> GetInverse() {
			return new Inverted(this);
		}

		public bool HasInverse {
// ReSharper disable CompareOfFloatsByEqualityOperator
			get { return 0 != Radius; }
// ReSharper restore CompareOfFloatsByEqualityOperator
		}

		ITransformation ITransformation.GetInverse() {
			return GetInverse();
		}

		public IEnumerable<Point2> TransformValues(IEnumerable<GeographicCoordinate> values) {
			return values.Select(TransformValue);
		}

		public bool Equals(MercatorSpherical other) {
			return !ReferenceEquals(other, null)
				&& (
// ReSharper disable CompareOfFloatsByEqualityOperator
					OriginLongitude == other.OriginLongitude
					&& Radius == other.Radius
// ReSharper restore CompareOfFloatsByEqualityOperator
					&& FalseProjectedOffset.Equals(other.FalseProjectedOffset)
				)
			;
		}

		public override bool Equals(object obj) {
			return null != obj
				&& (ReferenceEquals(this, obj) || Equals(obj as MercatorSpherical));
		}

		public override int GetHashCode() {
			return Radius.GetHashCode() ^ FalseProjectedOffset.GetHashCode();
		}

	}
}
