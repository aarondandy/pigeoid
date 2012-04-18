// TODO: source header

using System;
using System.Collections.Generic;
using System.Linq;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Transformation
{
	/// <summary>
	/// A Molodensky-Badekas transformation.
	/// </summary>
	public class MolodenskyBadekasTransformation : ITransformation<Point3>
	{

		public readonly Vector3 D;
		public readonly Vector3 R;
		public readonly Point3 O;
		public readonly double Mppm;
		public readonly double M;

		private class Inverted : InvertedTransformationBase<MolodenskyBadekasTransformation, Point3>
		{

			private readonly Matrix3 _invRot;

			public Inverted(MolodenskyBadekasTransformation core) : base(core) {
				if (0 == Core.M)
					throw new ArgumentException("Core has no inverse.");

				_invRot = new Matrix3(
					1, -Core.R.Z, Core.R.Y,
					Core.R.Z, 1, -Core.R.X,
					-Core.R.Y, Core.R.X, 1
				);
				_invRot.Invert();
			}

			public override Point3 TransformValue(Point3 coord) {
				coord = new Point3(
					(coord.X - Core.D.X - Core.O.X) / Core.M,
					(coord.Y - Core.D.Y - Core.O.Y) / Core.M,
					(coord.Z - Core.D.Z - Core.O.Z) / Core.M
				);
				return new Point3(
					((coord.X * _invRot.E00) + (coord.Y * _invRot.E10) + (coord.Z * _invRot.E20)) + Core.O.X,
					((coord.X * _invRot.E01) + (coord.Y * _invRot.E11) + (coord.Z * _invRot.E21)) + Core.O.Y,
					((coord.X * _invRot.E02) + (coord.Y * _invRot.E12) + (coord.Z * _invRot.E22)) + Core.O.Z
				);
			}

		}

		/// <summary>
		/// Constructs a new Molodensky-Badekas transformation.
		/// </summary>
		/// <param name="translationVector">The translation vector.</param>
		/// <param name="rotationVector">The rotation vector.</param>
		/// <param name="rotationOrigin">The origin of rotation.</param>
		/// <param name="mppm">The scale factor offset in PPM.</param>
		public MolodenskyBadekasTransformation(
			Vector3 translationVector,
			Vector3 rotationVector,
			Point3 rotationOrigin,
			double mppm
		) {
			D = translationVector;
			R = rotationVector;
			O = rotationOrigin;
			Mppm = mppm;
			M = 1 + (mppm / 1000000.0);
		}

		public Point3 TransformValue(Point3 coord) {
			coord = new Point3(coord.X - O.X, coord.Y - O.Y, coord.Z - O.Z);
			return new Point3(
				((coord.X - (coord.Z * R.Y) + (coord.Y * R.Z)) * M) + O.X + D.X,
				((coord.Y - (coord.X * R.Z) + (coord.Z * R.X)) * M) + O.Y + D.Y,
				((coord.Z - (coord.Y * R.X) + (coord.X * R.Y)) * M) + O.Z + D.Z
			);
		}

		public bool HasInverse {
			get { return 0 != M; }
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

		public void TransformValues(Point3[] values) {
			for (int i = 0; i < values.Length; i++) {
				TransformValue(ref values[i]);
			}
		}

		private void TransformValue(ref Point3 value) {
			value = TransformValue(value);
		}

	}
}
