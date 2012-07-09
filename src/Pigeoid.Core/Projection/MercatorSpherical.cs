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
		ITransformation<GeographicCoord, Point2>,
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

		private class Inverted : InvertedTransformationBase<MercatorSpherical,Point2,GeographicCoord>
		{

			public Inverted(MercatorSpherical core)
				: base(core)
			{
				if (0 == Core.Radius)
					throw new ArgumentException("Core cannot be inverted.");
			}

			public override GeographicCoord TransformValue(Point2 coord) {
				return new GeographicCoord(
					(ProjectionBase.HalfPi - (2.0 * Math.Atan(Math.Pow(Math.E, (Core.FalseProjectedOffset.Y - coord.Y) / Core.Radius)))),
					((coord.X - Core.FalseProjectedOffset.X) / Core.Radius) + Core.OriginLongitude
				);
			}

		}

		/// <summary>
		/// Creates a spherical mercator projection.
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

		public Point2 TransformValue(GeographicCoord coord) {
			return new Point2(
				FalseProjectedOffset.X + (Radius * (coord.Longitude - OriginLongitude)),
				FalseProjectedOffset.Y + (Radius * Math.Log(Math.Tan(ProjectionBase.QuarterPi + (coord.Latitude / 2.0))))
			);
		}

		public ITransformation<Point2, GeographicCoord> GetInverse() {
			return new Inverted(this);
		}

		public bool HasInverse {
			get { return 0 != Radius; }
		}

		ITransformation ITransformation.GetInverse() {
			return GetInverse();
		}

		public IEnumerable<Point2> TransformValues(IEnumerable<GeographicCoord> values) {
			return values.Select(TransformValue);
		}

		public bool Equals(MercatorSpherical other) {
			return !ReferenceEquals(other, null)
				&& (
					OriginLongitude == other.OriginLongitude
					&& Radius == other.Radius
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
