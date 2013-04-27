using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Transformation
{
    public class SimilarityTransformation : ITransformation<Point2>
    {

        private class Inverse : ITransformation<Point2>
        {
            private readonly SimilarityTransformation _core;

            public Inverse(SimilarityTransformation core) {
                Contract.Requires(core != null);
                _core = core;
            }

            [ContractInvariantMethod]
            private void CodeContractInvariants() {
                Contract.Invariant(_core != null);
            }

            public Point2 TransformValue(Point2 value) {
                var dx = value.X - _core._origin.X;
                var dy = value.Y - _core._origin.Y;
                return new Point2(
                    ((dx * _core._cosTheta) - (dy * _core._sinTheta)) / _core._unitScaleValue,
                    ((dx * _core._sinTheta) + (dy * _core._cosTheta)) / _core._unitScaleValue
                );
            }

            public void TransformValues(Point2[] values) {
                for (int i = 0; i < values.Length; i++)
                    values[i] = TransformValue(values[i]);
            }

            public IEnumerable<Point2> TransformValues(IEnumerable<Point2> values) {
                Contract.Ensures(Contract.Result<IEnumerable<Point2>>() != null);
                return values.Select(TransformValue);
            }

            public ITransformation<Point2> GetInverse() {
                Contract.Ensures(Contract.Result<ITransformation<Point2>>() != null);
                return _core;
            }

            ITransformation<Point2, Point2> ITransformation<Point2, Point2>.GetInverse() {
                return _core;
            }

            ITransformation ITransformation.GetInverse() {
                return _core;
            }

            public bool HasInverse {
                [Pure] get { return true; }
            }
        }

        private readonly Point2 _origin;
        private readonly double _unitScaleValue;
        private readonly double _rotationAngle;
        private readonly double _cosTheta;
        private readonly double _sinTheta;

        public SimilarityTransformation(Point2 origin, double unitScaleVale, double rotationAngle) {
            _origin = origin;
            _unitScaleValue = unitScaleVale;
            _rotationAngle = rotationAngle;
            _sinTheta = Math.Sin(rotationAngle);
            _cosTheta = Math.Cos(rotationAngle);
        }

        public Point2 Origin { get { return _origin; } }
        public double UnitScaleValue { get { return _unitScaleValue; } }
        public double RotationAngle { get { return _rotationAngle; } }

        public Point2 TransformValue(Point2 value) {
            return new Point2(
                _origin.X + (value.X * _unitScaleValue * _cosTheta) + (value.Y * _unitScaleValue * _sinTheta),
                _origin.Y - (value.X * _unitScaleValue * _sinTheta) + (value.Y * _unitScaleValue * _cosTheta)
            );
        }

        public IEnumerable<Point2> TransformValues(IEnumerable<Point2> values) {
            Contract.Ensures(Contract.Result<IEnumerable<Point2>>() != null);
            return values.Select(TransformValue);
        }

        public void TransformValues(Point2[] values) {
            for (int i = 0; i < values.Length; i++)
                values[i] = TransformValue(values[i]);
        }

        public ITransformation<Point2> GetInverse() {
            if (!HasInverse) throw new NoInverseException();
            Contract.Ensures(Contract.Result<ITransformation<Point2>>() != null);
            return new Inverse(this);
        }

        ITransformation ITransformation.GetInverse() {
            return GetInverse();
        }

        ITransformation<Point2, Point2> ITransformation<Point2, Point2>.GetInverse() {
            return GetInverse();
        }

        public bool HasInverse {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            [Pure] get { return 0 != _unitScaleValue && !Double.IsNaN(_unitScaleValue); }
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

    }
}
