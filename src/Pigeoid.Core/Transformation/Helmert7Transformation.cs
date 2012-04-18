// TODO: source header

using System;
using System.Collections.Generic;
using System.Linq;
using Pigeoid.Contracts;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Transformation
{
	/// <summary>
	/// A 7 parameter Helmert Transformation.
	/// </summary>
	public class Helmert7Transformation :
		ITransformation<Point3>,
		IEquatable<Helmert7Transformation>
	{

		/// <summary>
		/// A transformation which does nothing.
		/// </summary>
		public static readonly Helmert7Transformation IdentityTransformation = new Helmert7Transformation(Vector3.ZeroVector);

		private class Inverted :
			InvertedTransformationBase<Helmert7Transformation, Point3>,
			IEquatable<Inverted>
		{

			private readonly Matrix3 _invRot;

			public Inverted(Helmert7Transformation core) : base(core) {
				if (0 == Core.M)
					throw new ArgumentException("Core cannot be inverted.");

				_invRot = new Matrix3(
					1, Core.R.Z, -Core.R.Y,
					-Core.R.Z, 1, Core.R.X,
					Core.R.Y, -Core.R.X, 1
				);
				_invRot.Invert();
			}

			public override Point3 TransformValue(Point3 coord) {
				coord = new Point3(
					(coord.X - Core.D.X) / Core.M,
					(coord.Y - Core.D.Y) / Core.M,
					(coord.Z - Core.D.Z) / Core.M
				);
				return new Point3(
					(coord.X * _invRot.E00) + (coord.Y * _invRot.E10) + (coord.Z * _invRot.E20),
					(coord.X * _invRot.E01) + (coord.Y * _invRot.E11) + (coord.Z * _invRot.E21),
					(coord.X * _invRot.E02) + (coord.Y * _invRot.E12) + (coord.Z * _invRot.E22)
				);
			}

			public override int GetHashCode() {
				return Core.GetHashCode();
			}

			public bool Equals(Inverted other) {
				return !ReferenceEquals(null, other)
					&& Core.D.Equals(other.Core.D)
					&& Core.R.Equals(other.Core.R)
					&& Core.M == other.Core.M
				;
			}

			public override bool Equals(object obj) {
				return null != obj
					&& (
						obj is Inverted && Equals(obj as Inverted)
					)
				;
			}

		}

		/// <summary>
		/// Translation vector.
		/// </summary>
		public readonly Vector3 D;
		/// <summary>
		/// Rotation vector.
		/// </summary>
		public readonly Vector3 R;
		/// <summary>
		/// Scale factor, offset in ppm from 1.
		/// </summary>
		public readonly double Mppm;
		/// <summary>
		/// Scale factor.
		/// </summary>
		public readonly double M;

		/// <summary>
		/// Constructs a new Helmert 7 parameter transform.
		/// </summary>
		/// <param name="translationVector">The vector used to translate.</param>
		public Helmert7Transformation(
			ICoordinateTriple<double> translationVector
		)
			: this(translationVector, Vector3.ZeroVector, 0) { }

		/// <summary>
		/// Constructs a new Helmert 7 parameter transform.
		/// </summary>
		/// <param name="translationVector">The vector used to translate.</param>
		/// <param name="rotationVector">The vector containing rotation parameters.</param>
		/// <param name="mppm">The scale factor offset in PPM.</param>
		public Helmert7Transformation(
			ICoordinateTriple<double> translationVector,
			ICoordinateTriple<double> rotationVector,
			double mppm
		) {
			D = new Vector3(translationVector);
			R = new Vector3(rotationVector);
			Mppm = mppm;
			M = 1 + (mppm / 1000000.0);
		}

		private void TransformValue(ref Point3 coord) {
			coord = new Point3(
				((coord.X + (coord.Z * R.Y) - (coord.Y * R.Z)) * M) + D.X,
				((coord.Y + (coord.X * R.Z) - (coord.Z * R.X)) * M) + D.Y,
				((coord.Z + (coord.Y * R.X) - (coord.X * R.Y)) * M) + D.Z
			);
		}


		public Point3 TransformValue(Point3 coord) {
			return new Point3(
				((coord.X + (coord.Z * R.Y) - (coord.Y * R.Z)) * M) + D.X,
				((coord.Y + (coord.X * R.Z) - (coord.Z * R.X)) * M) + D.Y,
				((coord.Z + (coord.Y * R.X) - (coord.X * R.Y)) * M) + D.Z
			);
		}

		public void TransformValues(Point3[] values) {
			for (int i = 0; i < values.Length; i++) {
				TransformValue(ref values[i]);
			}
		}

		public IEnumerable<Point3> TransformValues(IEnumerable<Point3> values) {
			return values.Select(TransformValue);
		}

		public ITransformation<Point3> GetInverse() {
			return new Inverted(this);
		}

		ITransformation<Point3, Point3> ITransformation<Point3, Point3>.GetInverse() {
			return GetInverse();
		}

		public bool HasInverse {
			get { return 0 != M; }
		}

		ITransformation ITransformation.GetInverse() {
			return GetInverse();
		}

		public bool Equals(Helmert7Transformation other) {
			return !ReferenceEquals(null, other)
				&& D.Equals(other.D)
				&& R.Equals(other.R)
				&& M == other.M
			;
		}

		public override bool Equals(object obj) {
			return null != obj
				&& (obj is Helmert7Transformation && Equals(obj as Helmert7Transformation));
		}

		public override int GetHashCode() {
			return D.GetHashCode() ^ R.GetHashCode();
		}

		public IEnumerable<INamedParameter> GetParameters() {
			return new INamedParameter[]
                {
                    new NamedParameter<double>("dx",D.X),
                    new NamedParameter<double>("dy",D.Y),
                    new NamedParameter<double>("dz",D.Z),
                    new NamedParameter<double>("rx",R.X),
                    new NamedParameter<double>("ry",R.Y),
                    new NamedParameter<double>("rz",R.Z),
                    new NamedParameter<double>("m",Mppm)
                }
			;
		}

		public string Name {
			get { return "Helmert 7 Parameter Transformation"; }
		}

	}
}
