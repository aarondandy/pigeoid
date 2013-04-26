using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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

        private class Inverted : InvertedTransformationBase<MolodenskyBadekasTransformation, Point3>
        {

            private readonly Matrix3 _invRot;

            public Inverted(MolodenskyBadekasTransformation core) : base(core) {
                Contract.Requires(core != null);
                _invRot = new Matrix3(
                    1, -Core.R.Z, Core.R.Y,
                    Core.R.Z, 1, -Core.R.X,
                    -Core.R.Y, Core.R.X, 1
                );
                _invRot.Invert();
            }

            private void CodeContractInvariants() {
                Contract.Invariant(_invRot != null);
            }

            public override Point3 TransformValue(Point3 coordinate) {
                coordinate = new Point3(
                    (coordinate.X - Core.D.X - Core.O.X) / Core.M,
                    (coordinate.Y - Core.D.Y - Core.O.Y) / Core.M,
                    (coordinate.Z - Core.D.Z - Core.O.Z) / Core.M
                );
                return new Point3(
                    ((coordinate.X * _invRot.E00) + (coordinate.Y * _invRot.E10) + (coordinate.Z * _invRot.E20)) + Core.O.X,
                    ((coordinate.X * _invRot.E01) + (coordinate.Y * _invRot.E11) + (coordinate.Z * _invRot.E21)) + Core.O.Y,
                    ((coordinate.X * _invRot.E02) + (coordinate.Y * _invRot.E12) + (coordinate.Z * _invRot.E22)) + Core.O.Z
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

        public readonly Vector3 D;
        public readonly Vector3 R;
        public readonly Point3 O;
        public readonly double Mppm;
        public readonly double M;

        public Point3 TransformValue(Point3 coordinate) {
            coordinate = new Point3(coordinate.X - O.X, coordinate.Y - O.Y, coordinate.Z - O.Z);
            return new Point3(
                ((coordinate.X - (coordinate.Z * R.Y) + (coordinate.Y * R.Z)) * M) + O.X + D.X,
                ((coordinate.Y - (coordinate.X * R.Z) + (coordinate.Z * R.X)) * M) + O.Y + D.Y,
                ((coordinate.Z - (coordinate.Y * R.X) + (coordinate.X * R.Y)) * M) + O.Z + D.Z
            );
        }

        public bool HasInverse {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            get { return 0 != M && !Double.IsNaN(M); }
            // ReSharper restore CompareOfFloatsByEqualityOperator
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
            Contract.Requires(values != null);
            Contract.Ensures(Contract.Result<IEnumerable<Point3>>() != values);
            return values.Select(TransformValue);
        }

        public void TransformValues(Point3[] values) {
            Contract.Requires(values != null);
            for (int i = 0; i < values.Length; i++) {
                TransformValue(ref values[i]);
            }
        }

        private void TransformValue(ref Point3 value) {
            value = TransformValue(value);
        }

    }
}
