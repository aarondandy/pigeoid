// TODO: source header

using System;
using System.Collections.Generic;
using System.Linq;
using Pigeoid.Contracts;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Transformation
{
	public class GeographicGeocentricTransformation :
		ITransformation<GeographicCoord, Point3>,
		ITransformation<GeographicHeightCoord, Point3>
	{

		public class GeocentricGeographicTransformation :
			ITransformation<Point3, GeographicCoord>,
			ITransformation<Point3, GeographicHeightCoord>
		{

			protected readonly GeographicGeocentricTransformation Core;
			private readonly double _eSqMajAxis;
			private readonly double _eSecSqMinAxis;

			internal GeocentricGeographicTransformation(GeographicGeocentricTransformation core) {
				Core = core;
				_eSqMajAxis = Core.ESq * Core.MajorAxis;
				_eSecSqMinAxis = Core.ESecSq * Core.MinorAxis;
				if (0 == Core.MinorAxis)
					throw new ArgumentException("Core cannot be inverted.");
			}

			GeographicCoord ITransformation<Point3, GeographicCoord>.TransformValue(Point3 geocentric) {
				double p = Math.Sqrt((geocentric.X * geocentric.X) + (geocentric.Y * geocentric.Y));
				double cosQ = Math.Atan((geocentric.Z * Core.MajorAxis) / (p * Core.MinorAxis));
				double sinQ = Math.Sin(cosQ);
				cosQ = Math.Cos(cosQ);
				return new GeographicCoord(
					Math.Atan(
						(geocentric.Z + (_eSecSqMinAxis * sinQ * sinQ * sinQ))
						/
						(p - (_eSqMajAxis * cosQ * cosQ * cosQ))
					),
					Math.Atan2(geocentric.Y, geocentric.X)
				);
			}

			public GeographicHeightCoord TransformValue(Point3 geocentric) {
				double p = Math.Sqrt((geocentric.X * geocentric.X) + (geocentric.Y * geocentric.Y));
				double cosQ = Math.Atan((geocentric.Z * Core.MajorAxis) / (p * Core.MinorAxis));
				double sinQ = Math.Sin(cosQ);
				cosQ = Math.Cos(cosQ);
				double lat = Math.Atan(
					(geocentric.Z + (_eSecSqMinAxis * sinQ * sinQ * sinQ))
					/
					(p - (_eSqMajAxis * cosQ * cosQ * cosQ))
				);
				sinQ = Math.Sin(lat);
				return new GeographicHeightCoord(
					lat,
					Math.Atan2(geocentric.Y, geocentric.X),
					(p / Math.Cos(lat))
					- (
						Core.MajorAxis / Math.Sqrt(
							1.0 - (Core.ESq * sinQ * sinQ)
						)
					)
				);
			}

			IEnumerable<GeographicCoord> ITransformation<Point3, GeographicCoord>.TransformValues(IEnumerable<Point3> values) {
				return values.Select(((ITransformation<Point3, GeographicCoord>)this).TransformValue);
			}

			public IEnumerable<GeographicHeightCoord> TransformValues(IEnumerable<Point3> values) {
				return values.Select(TransformValue);
			}

			public bool HasInverse {
				get { return true; }
			}

			public ITransformation GetInverse() {
				return Core;
			}

			ITransformation<GeographicCoord, Point3> ITransformation<Point3, GeographicCoord>.GetInverse() {
				return Core;
			}

			ITransformation<GeographicHeightCoord, Point3> ITransformation<Point3, GeographicHeightCoord>.GetInverse() {
				return Core;
			}

			public string Name {
				get { return "Geocentric To Ellipsoid"; }
			}

			public IEnumerable<INamedParameter> GetParameters() {
				return Core.GetParameters();
			}
		}

		protected readonly double MajorAxis;
		protected readonly double MinorAxis;
		protected readonly double ESq;
		protected readonly double ESecSq;
		protected readonly double OneMinusESq;
		/// <summary>
		/// The spheroid.
		/// </summary>
		public readonly ISpheroid<double> Spheroid;

		public GeographicGeocentricTransformation(
			ISpheroid<double> spheroid
		) {
			if (null == spheroid) {
				throw new ArgumentNullException("spheroid");
			}
			MajorAxis = spheroid.A;
			MinorAxis = spheroid.B;
			ESq = spheroid.ESquared;
			ESecSq = spheroid.ESecondSquared;
			OneMinusESq = 1.0 - ESq;
			Spheroid = spheroid;
		}

		public Point3 TransformValue(GeographicCoord geographic) {
			double sinLat = Math.Sin(geographic.Latitude);
			double v = MajorAxis / Math.Sqrt(
				1.0 - (ESq * sinLat * sinLat)
			);
			double vcl = v * Math.Cos(geographic.Latitude);
			return new Point3(
				vcl * Math.Cos(geographic.Longitude),
				vcl * Math.Sin(geographic.Longitude),
				OneMinusESq * v * sinLat
			);
		}

		public Point3 TransformValue(GeographicHeightCoord geographic) {
			double sinLat = Math.Sin(geographic.Latitude);
			double v = MajorAxis / Math.Sqrt(
				1.0 - (ESq * sinLat * sinLat)
			);
			double vhcl = (v + geographic.Height) * Math.Cos(geographic.Latitude);
			return new Point3(
				vhcl * Math.Cos(geographic.Longitude),
				vhcl * Math.Sin(geographic.Longitude),
				((OneMinusESq * v) + geographic.Height) * sinLat
			);
		}

		public IEnumerable<Point3> TransformValues(IEnumerable<GeographicCoord> values) {
			return values.Select(TransformValue);
		}


		public IEnumerable<Point3> TransformValues(IEnumerable<GeographicHeightCoord> values) {
			return values.Select(TransformValue);
		}

		public bool HasInverse {
			get { return 0 != MinorAxis; }
		}

		/// <summary>
		/// Gets the inverse transformation if one exists.
		/// </summary>
		/// <returns>A transformation.</returns>
		public GeocentricGeographicTransformation GetInverse() {
			if (!HasInverse) {
				throw new InvalidOperationException("no inverse");
			}
			return new GeocentricGeographicTransformation(this);
		}

		ITransformation ITransformation.GetInverse() {
			return GetInverse();
		}

		ITransformation<Point3, GeographicCoord> ITransformation<GeographicCoord, Point3>.GetInverse() {
			return GetInverse();
		}

		ITransformation<Point3, GeographicHeightCoord> ITransformation<GeographicHeightCoord, Point3>.GetInverse() {
			return GetInverse();
		}

		public IEnumerable<INamedParameter> GetParameters() {
			return new INamedParameter[]
                {
                    new NamedParameter<double>("semi major", MajorAxis),
                    new NamedParameter<double>("semi minor", MinorAxis)
                };
		}

		public string Name {
			get { return "Ellipsoid To Geocentric"; }
		}
	}
}
