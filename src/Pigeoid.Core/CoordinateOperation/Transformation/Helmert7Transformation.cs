using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Pigeoid.Ogc;
using Vertesaur;
using Vertesaur.Transformation;
using Pigeoid.Utility;

namespace Pigeoid.CoordinateOperation.Transformation
{
    /// <summary>
    /// A 7 parameter Helmert Transformation.
    /// </summary>
    public class Helmert7Transformation :
        ITransformation<Point3>,
        IEquatable<Helmert7Transformation>,
        IParameterizedCoordinateOperationInfo
    {

        public static Helmert7Transformation CreatePositionVectorFormat(Vector3 translationVector, Vector3 rotationVector, double mppm) {
            Contract.Ensures(Contract.Result<Helmert7Transformation>() != null);
            return new Helmert7Transformation(translationVector, rotationVector, mppm);
        }

        public static Helmert7Transformation CreateCoordinateFrameRotationFormat(Vector3 translationVector, Vector3 rotationVector, double mppm) {
            Contract.Ensures(Contract.Result<Helmert7Transformation>() != null);
            return new Helmert7Transformation(translationVector, rotationVector.GetNegative(), mppm);
        }

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

            public Inverted(Helmert7Transformation core)
                : base(core) {
                Contract.Requires(core != null);
                _invRot = new Matrix3(
                    1, -Core.RotationRadians.Z, Core.RotationRadians.Y,
                    Core.RotationRadians.Z, 1, -Core.RotationRadians.X,
                    -Core.RotationRadians.Y, Core.RotationRadians.X, 1
                );
                _invRot.Invert();
            }

            public override Point3 TransformValue(Point3 coordinate) {
                coordinate = new Point3(
                    (coordinate.X - Core.Delta.X) / Core.ScaleFactor,
                    (coordinate.Y - Core.Delta.Y) / Core.ScaleFactor,
                    (coordinate.Z - Core.Delta.Z) / Core.ScaleFactor
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
                    && Core.Delta.Equals(other.Core.Delta)
                    && Core.RotationRadians.Equals(other.Core.RotationRadians)
                    // ReSharper disable CompareOfFloatsByEqualityOperator
                    && Core.ScaleFactor == other.Core.ScaleFactor
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

            public override string ToString() {
                return "Inverse " + Core;
            }

        }

        /// <summary>
        /// Translation vector.
        /// </summary>
        public readonly Vector3 Delta;
        /// <summary>
        /// Rotation vector in radians.
        /// </summary>
        public readonly Vector3 RotationRadians;
        /// <summary>
        /// Rotation vector in arc-seconds.
        /// </summary>
        public readonly Vector3 RotationArcSeconds;
        /// <summary>
        /// Scale factor, offset in ppm from 1.
        /// </summary>
        public readonly double ScaleDeltaPartsPerMillion;
        /// <summary>
        /// Scale factor.
        /// </summary>
        public readonly double ScaleFactor;

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
        /// <param name="rotationVectorArcSeconds">The vector containing rotation parameters.</param>
        /// <param name="scaleDeltaPartsPerMillion">The scale factor offset in PPM.</param>
        public Helmert7Transformation(
            Vector3 translationVector,
            Vector3 rotationVectorArcSeconds,
            double scaleDeltaPartsPerMillion
        ) {
            Delta = translationVector;
            RotationArcSeconds = rotationVectorArcSeconds;
            RotationRadians = RotationArcSeconds.GetScaled(Math.PI / 648000.0);
            ScaleDeltaPartsPerMillion = scaleDeltaPartsPerMillion;
            ScaleFactor = 1 + (scaleDeltaPartsPerMillion / 1000000.0);
        }

        private void TransformValue(ref Point3 coordinate) {
            coordinate = new Point3(
                ((coordinate.X - (coordinate.Z * RotationRadians.Y) + (coordinate.Y * RotationRadians.Z)) * ScaleFactor) + Delta.X,
                ((coordinate.Y - (coordinate.X * RotationRadians.Z) + (coordinate.Z * RotationRadians.X)) * ScaleFactor) + Delta.Y,
                ((coordinate.Z - (coordinate.Y * RotationRadians.X) + (coordinate.X * RotationRadians.Y)) * ScaleFactor) + Delta.Z
            );
        }


        public Point3 TransformValue(Point3 coordinate) {
            return new Point3(
                ((coordinate.X - (coordinate.Z * RotationRadians.Y) + (coordinate.Y * RotationRadians.Z)) * ScaleFactor) + Delta.X,
                ((coordinate.Y - (coordinate.X * RotationRadians.Z) + (coordinate.Z * RotationRadians.X)) * ScaleFactor) + Delta.Y,
                ((coordinate.Z - (coordinate.Y * RotationRadians.X) + (coordinate.X * RotationRadians.Y)) * ScaleFactor) + Delta.Z
            );
        }

        public void TransformValues(Point3[] values) {
            for (int i = 0; i < values.Length; i++) {
                TransformValue(ref values[i]);
            }
        }

        public IEnumerable<Point3> TransformValues(IEnumerable<Point3> values) {
            Contract.Ensures(Contract.Result<IEnumerable<Point3>>() != null);
            return values.Select(TransformValue);
        }

        public ITransformation<Point3> GetInverse() {
            if(!HasInverse) throw new NoInverseException();
            Contract.Ensures(Contract.Result<ITransformation<Point3>>() != null);
            return new Inverted(this);
        }

        ITransformation<Point3, Point3> ITransformation<Point3, Point3>.GetInverse() {
            return GetInverse();
        }

        // ReSharper disable CompareOfFloatsByEqualityOperator
        public bool HasInverse { [Pure] get { return 0 != ScaleFactor && !Double.IsNaN(ScaleFactor); } }
        // ReSharper restore CompareOfFloatsByEqualityOperator

        ITransformation ITransformation.GetInverse() {
            return GetInverse();
        }

        public bool Equals(Helmert7Transformation other) {
            return !ReferenceEquals(null, other)
                && Delta.Equals(other.Delta)
                && RotationRadians.Equals(other.RotationRadians)
                // ReSharper disable CompareOfFloatsByEqualityOperator
                && ScaleDeltaPartsPerMillion == other.ScaleDeltaPartsPerMillion
                // ReSharper restore CompareOfFloatsByEqualityOperator
            ;
        }

        public override bool Equals(object obj) {
            return Equals(obj as Helmert7Transformation);
        }

        public override int GetHashCode() {
            return Delta.GetHashCode() ^ RotationRadians.GetHashCode();
        }

        public string Name {
            get { return "Helmert 7 Parameter Transformation"; }
        }

        public IEnumerable<INamedParameter> Parameters {
            get {
                return new INamedParameter[] {
                    new NamedParameter<double>("dx",Delta.X),
                    new NamedParameter<double>("dy",Delta.Y),
                    new NamedParameter<double>("dz",Delta.Z),
                    new NamedParameter<double>("rx",RotationRadians.X),
                    new NamedParameter<double>("ry",RotationRadians.Y),
                    new NamedParameter<double>("rz",RotationRadians.Z),
                    new NamedParameter<double>("m",ScaleDeltaPartsPerMillion)
                };
            }
        }

        public ICoordinateOperationMethodInfo Method {
            get { return new OgcCoordinateOperationMethodInfo(Name); }
        }

        ICoordinateOperationInfo ICoordinateOperationInfo.GetInverse() {
            return (ICoordinateOperationInfo)GetInverse();
        }

        bool ICoordinateOperationInfo.IsInverseOfDefinition {
            get { return false; }
        }

        public override string ToString() {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            var result = Name + ' ' + Delta;
            var writeM = 0 != ScaleDeltaPartsPerMillion;

            if (writeM || !Vector3.Zero.Equals(RotationRadians)) {
                result += ' ' + RotationRadians.ToString();
                if (writeM) {
                    result += String.Concat(' ', ScaleFactor);
                }
            }

            return result;
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        public object TransformValue(object value) {
            return TransformValue((Point3) value);
        }

        public IEnumerable<object> TransformValues(IEnumerable<object> values) {
            Contract.Ensures(Contract.Result<IEnumerable<object>>() != null);
            return values.Select(TransformValue);
        }

        public Type[] GetInputTypes() {
            return new []{typeof(Point3)};
        }

        public Type[] GetOutputTypes(Type inputType) {
            return inputType == typeof(Point3)
                ? new[] { typeof(Point3) }
                : ArrayUtil<Type>.Empty;
        }

    }
}
