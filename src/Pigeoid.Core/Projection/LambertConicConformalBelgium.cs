// TODO: source header

using System;
using Pigeoid.Transformation;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Projection
{
	/// <summary>
	/// A Lambert Conic Conformal Belgium projection.
	/// </summary>
	public class LambertConicConformalBelgium : LambertConicConformal2Sp
	{

		private const double ThetaOffset = 0.00014204313635987739;

		private class Inverted : InvertedTransformationBase<LambertConicConformalBelgium,Point2,GeographicCoord>
		{

			public Inverted(LambertConicConformalBelgium core)
				: base(core) {
				if (0 == Core.Af || 0 == Core.N)
					throw new ArgumentException("Core cannot be inverted.");
			}

			public override GeographicCoord TransformValue(Point2 coord) {
				double eastingComponent = coord.X - Core.FalseProjectedOffset.X;
				double northingComponent = Core.NorthingOffset - coord.Y;
				double t = Math.Sqrt((eastingComponent * eastingComponent) + (northingComponent * northingComponent));
				t = Math.Pow(((Core.N < 0) ? -t : t) / Core.Af, Core.Invn);
				double lat = HalfPi;
				for (int i = 0; i < 8; i++) {
					double temp = Core.E * Math.Sin(lat);
					temp = HalfPi - (2.0 * Math.Atan(t * Math.Pow((1 - temp) / (1 + temp), Core.EHalf)));
					if (temp == lat)
						break;
					lat = temp;
				}
				return new GeographicCoord(
					lat,
					((Math.Atan(eastingComponent / northingComponent) + ThetaOffset) / Core.N) + Core.GeographiOrigin.Longitude
				);
			}

		}

		/// <summary>
		/// Constructs a new Lambert Conic Conformal Belgium projection.
		/// </summary>
		/// <param name="geographicOrigin">The geographic origin.</param>
		/// <param name="firstParallel">The first parallel.</param>
		/// <param name="secondParallel">The second parallel.</param>
		/// <param name="falseProjectedOffset">The false projected offset.</param>
		/// <param name="spheroid">The spheroid.</param>
		public LambertConicConformalBelgium(
			GeographicCoord geographicOrigin,
			double firstParallel,
			double secondParallel,
			Vector2 falseProjectedOffset,
			ISpheroid<double> spheroid
		)
			: base(geographicOrigin, firstParallel, secondParallel, falseProjectedOffset, spheroid) { }

		public override ITransformation<Point2, GeographicCoord> GetInverse() {
			return new Inverted(this);
		}

		public override bool HasInverse {
			get { return 0 != Af && 0 != N; }
		}

		public override Point2 TransformValue(GeographicCoord coord) {
			double r = E * Math.Sin(coord.Latitude);
			r = Af * Math.Pow(
				Math.Tan(QuarterPi - (coord.Latitude / 2.0)) / Math.Pow((1.0 - r) / (1.0 + r), EHalf),
				N
			);
			double theta = (N * (coord.Longitude - GeographiOrigin.Longitude)) - ThetaOffset;
			return new Point2(
				FalseProjectedOffset.X + (r * Math.Sin(theta)),
				NorthingOffset - (r * Math.Cos(theta))
			);
		}

	}
}
