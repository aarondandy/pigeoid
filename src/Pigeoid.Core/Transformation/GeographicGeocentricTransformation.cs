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
		ITransformation<GeographicCoordinate, Point3>,
		ITransformation<GeographicHeightCoordinate, Point3>
	{

		public class GeocentricGeographicTransformation :
			ITransformation<Point3, GeographicCoordinate>,
			ITransformation<Point3, GeographicHeightCoordinate>
		{

			protected readonly GeographicGeocentricTransformation Core;
			private readonly double _eSqMajAxis;
			private readonly double _eSecSqMinAxis;

			internal GeocentricGeographicTransformation(GeographicGeocentricTransformation core) {
				Core = core;
				_eSqMajAxis = Core.ESq * Core.MajorAxis;
				_eSecSqMinAxis = Core.ESecSq * Core.MinorAxis;
				
// ReSharper disable CompareOfFloatsByEqualityOperator
				if (0 == Core.MinorAxis) throw new ArgumentException("Core cannot be inverted.");
// ReSharper restore CompareOfFloatsByEqualityOperator
			}

			GeographicCoordinate ITransformation<Point3, GeographicCoordinate>.TransformValue(Point3 geocentric) {
				double p = Math.Sqrt((geocentric.X * geocentric.X) + (geocentric.Y * geocentric.Y));
				double cosQ = Math.Atan((geocentric.Z * Core.MajorAxis) / (p * Core.MinorAxis));
				double sinQ = Math.Sin(cosQ);
				cosQ = Math.Cos(cosQ);
				return new GeographicCoordinate(
					Math.Atan(
						(geocentric.Z + (_eSecSqMinAxis * sinQ * sinQ * sinQ))
						/
						(p - (_eSqMajAxis * cosQ * cosQ * cosQ))
					),
					Math.Atan2(geocentric.Y, geocentric.X)
				);
			}

			public GeographicHeightCoordinate TransformValue(Point3 geocentric) {
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
				return new GeographicHeightCoordinate(
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

			IEnumerable<GeographicCoordinate> ITransformation<Point3, GeographicCoordinate>.TransformValues(IEnumerable<Point3> values) {
				return values.Select(((ITransformation<Point3, GeographicCoordinate>)this).TransformValue);
			}

			public IEnumerable<GeographicHeightCoordinate> TransformValues(IEnumerable<Point3> values) {
				return values.Select(TransformValue);
			}

			public bool HasInverse {
				get { return true; }
			}

			public ITransformation GetInverse() {
				return Core;
			}

			ITransformation<GeographicCoordinate, Point3> ITransformation<Point3, GeographicCoordinate>.GetInverse() {
				return Core;
			}

			ITransformation<GeographicHeightCoordinate, Point3> ITransformation<Point3, GeographicHeightCoordinate>.GetInverse() {
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

		public Point3 TransformValue(GeographicCoordinate geographic) {
			var sinLatitude = Math.Sin(geographic.Latitude);
			var v = MajorAxis / Math.Sqrt(
				1.0 - (ESq * sinLatitude * sinLatitude)
			);
			var vCosLatitude = v * Math.Cos(geographic.Latitude);
			return new Point3(
				vCosLatitude * Math.Cos(geographic.Longitude),
				vCosLatitude * Math.Sin(geographic.Longitude),
				OneMinusESq * v * sinLatitude
			);
		}

		public Point3 TransformValue(GeographicHeightCoordinate geographic) {
			var sinLatitude = Math.Sin(geographic.Latitude);
			var v = MajorAxis / Math.Sqrt(
				1.0 - (ESq * sinLatitude * sinLatitude)
			);
			var vHeightCostLatitude = (v + geographic.Height) * Math.Cos(geographic.Latitude);
			return new Point3(
				vHeightCostLatitude * Math.Cos(geographic.Longitude),
				vHeightCostLatitude * Math.Sin(geographic.Longitude),
				((OneMinusESq * v) + geographic.Height) * sinLatitude
			);
		}

		public IEnumerable<Point3> TransformValues(IEnumerable<GeographicCoordinate> values) {
			return values.Select(TransformValue);
		}


		public IEnumerable<Point3> TransformValues(IEnumerable<GeographicHeightCoordinate> values) {
			return values.Select(TransformValue);
		}

		public bool HasInverse {
// ReSharper disable CompareOfFloatsByEqualityOperator
			get { return 0 != MinorAxis; }
// ReSharper restore CompareOfFloatsByEqualityOperator
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

		ITransformation<Point3, GeographicCoordinate> ITransformation<GeographicCoordinate, Point3>.GetInverse() {
			return GetInverse();
		}

		ITransformation<Point3, GeographicHeightCoordinate> ITransformation<GeographicHeightCoordinate, Point3>.GetInverse() {
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
