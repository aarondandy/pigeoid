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
	public class AbridgedMolodenskyTransformation : ITransformation<GeographicHeightCoord>
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
			ICoordinateTriple<double> translation,
			ISpheroid<double> sourceSpheroid,
			ISpheroid<double> targetSpheroid
		) {
			SourceSpheroid = sourceSpheroid;
			TargetSpheroid = targetSpheroid;
			D = new Vector3(translation);
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
			if (0 == OneMinusESqsaSinOne || 0 == SaSinOne)
				throw new ArgumentException("Invalid spheroid.", "sourceSpheroid");
		}

		public GeographicHeightCoord TransformValue(GeographicHeightCoord coord) {
			double sinLats = Math.Sin(coord.Latitude);
			double sinLatsSq = sinLats * sinLats;
			double cosLats = Math.Cos(coord.Latitude);
			double sinLons = Math.Sin(coord.Longitude);
			double cosLons = Math.Cos(coord.Longitude);
			double c = 1.0 - (SeSq * sinLatsSq);
			double cSq = Math.Sqrt(c);
			double dxdy = (D.X * cosLons) + (D.Y * sinLons);
			return new GeographicHeightCoord(
				coord.Latitude + (
					(
						(
							(D.Z * cosLats)
							+ (Sadfsfda * Math.Sin(2.0 * coord.Latitude))
							- (sinLats * dxdy)
						)
						* c * cSq
					)
					/ OneMinusESqsaSinOne
				),
				coord.Longitude + (
					(
						((D.Y * cosLons) - (D.X * sinLons))
						* cSq
					)
					/ (cosLats * SaSinOne)
				),
				coord.Height + (
					+(cosLats * dxdy)
					+ (D.Z * sinLats)
					+ (Sadfsfda * sinLatsSq)
					- Da
				)
			);
		}

		public void TransformValues(GeographicHeightCoord[] values) {
			for (int i = 0; i < values.Length; i++) {
				TransformValue(ref values[i]);
			}
		}

		private void TransformValue(ref GeographicHeightCoord value) {
			value = TransformValue(value);
		}

		public ITransformation<GeographicHeightCoord> GetInverse() {
			return new AbridgedMolodenskyTransformation(D.GetNegative(), TargetSpheroid, SourceSpheroid);
		}

		public bool HasInverse {
			get { return (0 != TargetSpheroid.A && 0 != (1.0 - TargetSpheroid.ESquared)); }
		}

		ITransformation ITransformation.GetInverse() {
			return GetInverse();
		}

		ITransformation<GeographicHeightCoord, GeographicHeightCoord> ITransformation<GeographicHeightCoord, GeographicHeightCoord>.GetInverse() {
			return GetInverse();
		}

		public IEnumerable<GeographicHeightCoord> TransformValues(IEnumerable<GeographicHeightCoord> values) {
			return values.Select(TransformValue);
		}


	}
}
