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
	/// A base class for Lambert Conic Conformal projections.
	/// </summary>
	public abstract class LambertConicConformal :
		ProjectionBase
	{

		internal const string DefaultName = "Lambert Conformal Conic";

		/// <summary>
		/// The geographic origin of the projection.
		/// </summary>
		public readonly GeographicCoordinate GeographicOrigin;
		protected double Af;
		protected double N;
		protected double F;
		protected double ROrigin;
		protected double InvN;
		protected double NorthingOffset;

		private class Inverted : InvertedTransformationBase<LambertConicConformal,Point2,GeographicCoordinate>
		{

			public Inverted(LambertConicConformal core)
				: base(core)
			{
				if (!core.HasInverse)
					throw new ArgumentException("Core cannot be inverted.");
			}

			public override GeographicCoordinate TransformValue(Point2 coordinate) {
				var eastingComponent = coordinate.X - Core.FalseProjectedOffset.X;
				var northingComponent = Core.NorthingOffset - coordinate.Y;
				var t = Math.Sqrt((eastingComponent * eastingComponent) + (northingComponent * northingComponent));
				t = Math.Pow(((Core.N < 0) ? -t : t) / Core.Af, Core.InvN);
				var lat = HalfPi;
				for (int i = 0; i < 8; i++) {
					var temp = Core.E * Math.Sin(lat);
					temp = HalfPi - (
						2.0 * Math.Atan(
							t
							* Math.Pow(
								(1 - temp) / (1 + temp),
								Core.EHalf
							)
						)
					);

// ReSharper disable CompareOfFloatsByEqualityOperator
					if (temp == lat) break;
// ReSharper restore CompareOfFloatsByEqualityOperator
						
					lat = temp;
				}
				return new GeographicCoordinate(
					lat,
					(Math.Atan(eastingComponent / northingComponent) / Core.N)
						+ Core.GeographicOrigin.Longitude
				);
			}

		}

		protected LambertConicConformal(
			GeographicCoordinate geographicOrigin,
			Vector2 falseProjectedOffset,
			ISpheroid<double> spheroid
		)
			: base(falseProjectedOffset, spheroid)
		{
			GeographicOrigin = geographicOrigin;
		}

		public override Point2 TransformValue(GeographicCoordinate coordinate) {
			double r = E * Math.Sin(coordinate.Latitude);
			r = Af * Math.Pow(
				(
					Math.Tan(
						QuarterPi
						-
						(coordinate.Latitude / 2.0)
					)
					/
					Math.Pow(
						(1.0 - r) / (1.0 + r),
						EHalf
					)
				),
				N
			);
			double theta = N * (coordinate.Longitude - GeographicOrigin.Longitude);
			double x = FalseProjectedOffset.X + (r * Math.Sin(theta));
			double y = NorthingOffset - (r * Math.Cos(theta));
			return new Point2(x, y);
		}

		public override ITransformation<Point2, GeographicCoordinate> GetInverse() {
			return new Inverted(this);
		}

		public override bool HasInverse {
// ReSharper disable CompareOfFloatsByEqualityOperator
			get { return 0 != Af && 0 != N; }
// ReSharper restore CompareOfFloatsByEqualityOperator
		}

		public override string Name {
			get { return DefaultName; }
		}

		public override IEnumerable<INamedParameter> GetParameters() {
			return base.GetParameters()
				.Concat(new INamedParameter[]
                    {
                        new NamedParameter<double>(NamedParameter.NameLatitudeOfNaturalOrigin,GeographicOrigin.Latitude), 
                        new NamedParameter<double>(NamedParameter.NameLongitudeOfNaturalOrigin,GeographicOrigin.Longitude)
                    }
				)
			;
		}

	}
}
