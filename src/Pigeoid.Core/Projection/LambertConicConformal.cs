// TODO: source header

using System;
using System.Collections.Generic;
using System.Linq;
using Pigeoid.Contracts;
using Pigeoid.Transformation;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Projection
{
	/// <summary>
	/// A base class for lamber conic conformal projections.
	/// </summary>
	public abstract class LambertConicConformal :
		ProjectionBase
	{

		internal const string DefaultName = "Lambert Conformal Conic";

		/// <summary>
		/// The geographic origin of the projection.
		/// </summary>
		public readonly GeographicCoord GeographiOrigin;
		protected double Af;
		protected double N;
		protected double F;
		protected double ROrigin;
		protected double Invn;
		protected double NorthingOffset;

		private class Inverted : InvertedTransformationBase<LambertConicConformal,Point2,GeographicCoord>
		{

			public Inverted(LambertConicConformal core)
				: base(core)
			{
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
					temp = HalfPi - (
						2.0 * Math.Atan(
							t
							* Math.Pow(
								(1 - temp) / (1 + temp),
								Core.EHalf
							)
						)
					);
					if (temp == lat) {
						break;
					}
					lat = temp;
				}
				return new GeographicCoord(
					lat,
					(Math.Atan(eastingComponent / northingComponent) / Core.N)
						+ Core.GeographiOrigin.Longitude
				);
			}

		}

		protected LambertConicConformal(
			GeographicCoord geographiOrigin,
			Vector2 falseProjectedOffset,
			ISpheroid<double> spheroid
		)
			: base(falseProjectedOffset, spheroid)
		{
			GeographiOrigin = geographiOrigin;
		}

		public override Point2 TransformValue(GeographicCoord coord) {
			double r = E * Math.Sin(coord.Latitude);
			r = Af * Math.Pow(
				(
					Math.Tan(
						QuarterPi
						-
						(coord.Latitude / 2.0)
					)
					/
					Math.Pow(
						(1.0 - r) / (1.0 + r),
						EHalf
					)
				),
				N
			);
			double theta = N * (coord.Longitude - GeographiOrigin.Longitude);
			double x = FalseProjectedOffset.X + (r * Math.Sin(theta));
			double y = NorthingOffset - (r * Math.Cos(theta));
			return new Point2(x, y);
		}

		public override ITransformation<Point2, GeographicCoord> GetInverse() {
			return new Inverted(this);
		}

		public override bool HasInverse {
			get { return 0 != Af && 0 != N; }
		}

		public override string Name {
			get { return DefaultName; }
		}

		public override IEnumerable<INamedParameter> GetParameters() {
			return base.GetParameters()
				.Concat(new INamedParameter[]
                    {
                        new NamedParameter<double>(NamedParameter.NameLatitudeOfNaturalOrigin,GeographiOrigin.Latitude), 
                        new NamedParameter<double>(NamedParameter.NameLongitudeOfNaturalOrigin,GeographiOrigin.Longitude)
                    }
				)
			;
		}

	}
}
