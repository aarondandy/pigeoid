// TODO: source header

using System;
using System.Collections.Generic;
using System.Linq;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Transformation
{
	/// <summary>
	/// An abridged Molodensky transformation.
	/// </summary>
	public class AbridgedMolodenskyTransformation : ITransformation<GeographicHeightCoordinate>
	{

		private static readonly double SinOne = Math.Sin(1);

		public readonly Vector3 D;
		protected readonly double Da;
		protected readonly double SeSq;
		protected readonly double Sadfsfda;
		protected readonly double OneMinusESqsaSinOne;
		protected readonly double SaSinOne;

		public readonly ISpheroid<double> SourceSpheroid;
		public readonly ISpheroid<double> TargetSpheroid;

		/// <summary>
		/// Constructs an abridged molodensky transformation.
		/// </summary>
		/// <param name="translation">The amount to translate.</param>
		/// <param name="sourceSpheroid">The source CRS spheroid.</param>
		/// <param name="targetSpheroid">The destination CRS spheroid.</param>
		public AbridgedMolodenskyTransformation(
			Vector3 translation,
			ISpheroid<double> sourceSpheroid,
			ISpheroid<double> targetSpheroid
		) {
			SourceSpheroid = sourceSpheroid;
			TargetSpheroid = targetSpheroid;
			D = translation;
			double sf = sourceSpheroid.F;
			double tf = targetSpheroid.F;
			double df = tf - sf;
			double sa = sourceSpheroid.A;
			double ta = targetSpheroid.A;
			Da = ta - sa;
			SeSq = sourceSpheroid.ESquared;
			Sadfsfda = (sa * df) + (sf * Da);
			SaSinOne = sa * SinOne;
			OneMinusESqsaSinOne = SaSinOne * (1.0 - SeSq);
			
// ReSharper disable CompareOfFloatsByEqualityOperator
			if (0 == OneMinusESqsaSinOne || 0 == SaSinOne) throw new ArgumentException("Invalid spheroid.", "sourceSpheroid");
// ReSharper restore CompareOfFloatsByEqualityOperator
		}

		public GeographicHeightCoordinate TransformValue(GeographicHeightCoordinate coordinate) {
			var sinLatitude = Math.Sin(coordinate.Latitude);
			var sinLatitudeSquared = sinLatitude * sinLatitude;
			var cosLatitude = Math.Cos(coordinate.Latitude);
			var sinLongitude = Math.Sin(coordinate.Longitude);
			var cosLongitude = Math.Cos(coordinate.Longitude);
			var c = 1.0 - (SeSq * sinLatitudeSquared);
			var cSq = Math.Sqrt(c);
			var dxdy = (D.X * cosLongitude) + (D.Y * sinLongitude);
			return new GeographicHeightCoordinate(
				coordinate.Latitude + (
					(
						(
							(D.Z * cosLatitude)
							+ (Sadfsfda * Math.Sin(2.0 * coordinate.Latitude))
							- (sinLatitude * dxdy)
						)
						* c * cSq
					)
					/ OneMinusESqsaSinOne
				),
				coordinate.Longitude + (
					(
						((D.Y * cosLongitude) - (D.X * sinLongitude))
						* cSq
					)
					/ (cosLatitude * SaSinOne)
				),
				coordinate.Height + (
					+(cosLatitude * dxdy)
					+ (D.Z * sinLatitude)
					+ (Sadfsfda * sinLatitudeSquared)
					- Da
				)
			);
		}

		public void TransformValues(GeographicHeightCoordinate[] values) {
			for (int i = 0; i < values.Length; i++) {
				TransformValue(ref values[i]);
			}
		}

		private void TransformValue(ref GeographicHeightCoordinate value) {
			value = TransformValue(value);
		}

		public ITransformation<GeographicHeightCoordinate> GetInverse() {
			return new AbridgedMolodenskyTransformation(D.GetNegative(), TargetSpheroid, SourceSpheroid);
		}

		public bool HasInverse {
// ReSharper disable CompareOfFloatsByEqualityOperator
			get { return (0 != TargetSpheroid.A && 0 != (1.0 - TargetSpheroid.ESquared)); }
// ReSharper restore CompareOfFloatsByEqualityOperator
		}

		ITransformation ITransformation.GetInverse() {
			return GetInverse();
		}

		ITransformation<GeographicHeightCoordinate, GeographicHeightCoordinate> ITransformation<GeographicHeightCoordinate, GeographicHeightCoordinate>.GetInverse() {
			return GetInverse();
		}

		public IEnumerable<GeographicHeightCoordinate> TransformValues(IEnumerable<GeographicHeightCoordinate> values) {
			return values.Select(TransformValue);
		}


	}
}
