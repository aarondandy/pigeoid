// TODO: source header

using System;
using System.Collections.Generic;
using System.Linq;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Transformation
{
	/// <summary>
	/// A geocentric to topocentric transformation.
	/// </summary>
	public class GeocentricTopocentricTransformation : ITransformation<Point3>
	{

		protected readonly GeographicGeocentricTransformation GeographicTransform;
		private Point3 _topocentricOrigin;
		private double _sinLat;
		private double _cosLat;
		private double _sinLon;
		private double _cosLon;
		private double _sinLatCosLon;
		private double _cosLatCosLon;
		private double _cosLatSinLon;
		private double _sinLatSinLon;

		private class Inverted : InvertedTransformationBase<GeocentricTopocentricTransformation, Point3>
		{

			public Inverted(GeocentricTopocentricTransformation core) : base(core) { }

			public override Point3 TransformValue(Point3 topocentric) {
				return new Point3(
					Core._topocentricOrigin.X - (topocentric.X * Core._sinLon) - (topocentric.Y * Core._sinLatCosLon) + (topocentric.Z * Core._cosLatCosLon),
					Core._topocentricOrigin.Y + (topocentric.X * Core._cosLon) - (topocentric.Y * Core._sinLatSinLon) + (topocentric.Z * Core._cosLatSinLon),
					Core._topocentricOrigin.Z + (topocentric.Y * Core._cosLat) + (topocentric.Z * Core._sinLat)
				);
			}

		}
		/// <summary>
		/// Creates a new geocentric to topocentric transformation.
		/// </summary>
		/// <param name="topocentricOrigin">The topocentric origin.</param>
		/// <param name="spheroid">The spheroid.</param>
		public GeocentricTopocentricTransformation(
			Point3 topocentricOrigin,
			ISpheroid<double> spheroid
		) {
			GeographicTransform = new GeographicGeocentricTransformation(spheroid);
			SetTopocentricOrigin(topocentricOrigin);
		}

		/// <summary>
		/// Sets the topocentric origin for this transformation.
		/// </summary>
		/// <param name="topocentricOrigin">The topocentric origin.</param>
		public void SetTopocentricOrigin(Point3 topocentricOrigin) {
			_topocentricOrigin = topocentricOrigin;
			GeographicCoord ellipsoidalOrigin =
				(GeographicTransform.GetInverse() as ITransformation<Point3, GeographicCoord>)
				.TransformValue(_topocentricOrigin);
			_sinLat = Math.Sin(ellipsoidalOrigin.Latitude);
			_cosLat = Math.Cos(ellipsoidalOrigin.Latitude);
			_sinLon = Math.Sin(ellipsoidalOrigin.Longitude);
			_cosLon = Math.Cos(ellipsoidalOrigin.Longitude);
			_sinLatCosLon = _sinLat * _cosLon;
			_cosLatCosLon = _cosLat * _cosLon;
			_cosLatSinLon = _cosLat * _sinLon;
			_sinLatSinLon = _sinLat * _sinLon;
		}

		public Point3 TransformValue(Point3 geocentric) {
			Vector3 d = geocentric.Difference(_topocentricOrigin);
			return new Point3(
				(d.Y * _cosLon) - (d.X * _sinLon),
				(d.Z * _cosLat) - (d.X * _sinLatCosLon) - (d.Y * _sinLatSinLon),
				(d.X * _cosLatCosLon) + (d.Y * _cosLatSinLon) + (d.Z * _sinLat)
			);
		}

		public void TransformValues(Point3[] values) {
			for (int i = 0; i < values.Length; i++) {
				TransformValue(ref values[i]);
			}
		}

		[CLSCompliant(false)]
		public void TransformValue(ref Point3 value) {
			Vector3 d = value.Difference(_topocentricOrigin);
			value = new Point3(
				(d.Y * _cosLon) - (d.X * _sinLon),
				(d.Z * _cosLat) - (d.X * _sinLatCosLon) - (d.Y * _sinLatSinLon),
				(d.X * _cosLatCosLon) + (d.Y * _cosLatSinLon) + (d.Z * _sinLat)
			);
		}

		public bool HasInverse {
			get { return true; }
		}

		public ITransformation<Point3> GetInverse() {
			return new Inverted(this);
		}

		ITransformation<Point3, Point3> ITransformation<Point3, Point3>.GetInverse() {
			return GetInverse();
		}

		ITransformation ITransformation.GetInverse() {
			return GetInverse();
		}

		public IEnumerable<Point3> TransformValues(IEnumerable<Point3> values) {
			return values.Select(TransformValue);
		}

	}
}
