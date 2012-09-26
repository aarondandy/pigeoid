// TODO: source header

using System;
using System.Collections.Generic;
using System.Linq;
using Pigeoid.Contracts;
using Pigeoid.Ogc;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Transformation
{
	/// <summary>
	/// A 7 parameter Helmert Transformation.
	/// </summary>
	public class Helmert7Transformation :
		ITransformation<Point3>,
		IEquatable<Helmert7Transformation>,
		IParameterizedCoordinateOperationInfo
	{

		/// <summary>
		/// A transformation which does nothing.
		/// </summary>
		public static readonly Helmert7Transformation IdentityTransformation = new Helmert7Transformation(Vector3.Zero);

		private class Inverted :
			InvertedTransformationBase<Helmert7Transformation, Point3>,
			IEquatable<Inverted>,
			ICoordinateOperationInfo
		{

			private readonly Matrix3 _invRot;

			public Inverted(Helmert7Transformation core) : base(core) {
				if (!core.HasInverse)
					throw new ArgumentException("Core cannot be inverted.");

				_invRot = new Matrix3(
					1, Core.R.Z, -Core.R.Y,
					-Core.R.Z, 1, Core.R.X,
					Core.R.Y, -Core.R.X, 1
				);
				_invRot.Invert();
			}

			public override Point3 TransformValue(Point3 coordinate) {
				coordinate = new Point3(
					(coordinate.X - Core.D.X) / Core.M,
					(coordinate.Y - Core.D.Y) / Core.M,
					(coordinate.Z - Core.D.Z) / Core.M
				);
				return new Point3(
					(coordinate.X * _invRot.E00) + (coordinate.Y * _invRot.E10) + (coordinate.Z * _invRot.E20),
					(coordinate.X * _invRot.E01) + (coordinate.Y * _invRot.E11) + (coordinate.Z * _invRot.E21),
					(coordinate.X * _invRot.E02) + (coordinate.Y * _invRot.E12) + (coordinate.Z * _invRot.E22)
				);
			}

			public override int GetHashCode() {
				return Core.GetHashCode();
			}

			public bool Equals(Inverted other) {
				return !ReferenceEquals(null, other)
					&& Core.D.Equals(other.Core.D)
					&& Core.R.Equals(other.Core.R)
// ReSharper disable CompareOfFloatsByEqualityOperator
					&& Core.M == other.Core.M
// ReSharper restore CompareOfFloatsByEqualityOperator
				;
			}

			public override bool Equals(object obj) {
				return Equals(obj as Inverted);
			}


			public string Name {
				get { return "Inverse " + Core.Name; }
			}

			ICoordinateOperationInfo ICoordinateOperationInfo.GetInverse() {
				return Core;
			}

			public bool IsInverseOfDefinition {
				get { return true; }
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
		public Helmert7Transformation(Vector3 translationVector)
			: this(translationVector, Vector3.Zero, 0) { }

		/// <summary>
		/// Constructs a new Helmert 7 parameter transform.
		/// </summary>
		/// <param name="translationVector">The vector used to translate.</param>
		/// <param name="rotationVector">The vector containing rotation parameters.</param>
		/// <param name="mppm">The scale factor offset in PPM.</param>
		public Helmert7Transformation(
			Vector3 translationVector,
			Vector3 rotationVector,
			double mppm
		) {
			D = translationVector;
			R = rotationVector;
			Mppm = mppm;
			M = 1 + (mppm / 1000000.0);
		}

		private void TransformValue(ref Point3 coordinate) {
			coordinate = new Point3(
				((coordinate.X + (coordinate.Z * R.Y) - (coordinate.Y * R.Z)) * M) + D.X,
				((coordinate.Y + (coordinate.X * R.Z) - (coordinate.Z * R.X)) * M) + D.Y,
				((coordinate.Z + (coordinate.Y * R.X) - (coordinate.X * R.Y)) * M) + D.Z
			);
		}


		public Point3 TransformValue(Point3 coordinate) {
			return new Point3(
				((coordinate.X + (coordinate.Z * R.Y) - (coordinate.Y * R.Z)) * M) + D.X,
				((coordinate.Y + (coordinate.X * R.Z) - (coordinate.Z * R.X)) * M) + D.Y,
				((coordinate.Z + (coordinate.Y * R.X) - (coordinate.X * R.Y)) * M) + D.Z
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

// ReSharper disable CompareOfFloatsByEqualityOperator
		public bool HasInverse { get { return 0 != M; } }
// ReSharper restore CompareOfFloatsByEqualityOperator

		ITransformation ITransformation.GetInverse() {
			return GetInverse();
		}

		public bool Equals(Helmert7Transformation other) {
			return !ReferenceEquals(null, other)
				&& D.Equals(other.D)
				&& R.Equals(other.R)
// ReSharper disable CompareOfFloatsByEqualityOperator
				&& Mppm == other.Mppm
// ReSharper restore CompareOfFloatsByEqualityOperator
			;
		}

		public override bool Equals(object obj) {
			return Equals(obj as Helmert7Transformation);
		}

		public override int GetHashCode() {
			return D.GetHashCode() ^ R.GetHashCode();
		}

		public string Name {
			get { return "Helmert 7 Parameter Transformation"; }
		}


		public IEnumerable<INamedParameter> Parameters {
			get {
				return new INamedParameter[] {
					new NamedParameter<double>("dx",D.X),
					new NamedParameter<double>("dy",D.Y),
					new NamedParameter<double>("dz",D.Z),
					new NamedParameter<double>("rx",R.X),
					new NamedParameter<double>("ry",R.Y),
					new NamedParameter<double>("rz",R.Z),
					new NamedParameter<double>("m",Mppm)
				};
			}
		}

		public ICoordinateOperationMethodInfo Method {
			get { return new OgcCoordinateOperationMethodInfo(Name); }
		}

		ICoordinateOperationInfo ICoordinateOperationInfo.GetInverse() {
			return GetInverse() as ICoordinateOperationInfo;
		}

		public bool IsInverseOfDefinition {
			get { return false; }
		}
	}
}
