using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Pigeoid.Utility;
using Vertesaur;
using Vertesaur.Transformation;

namespace Pigeoid.CoordinateOperation.Projection
{

    /// <summary>
    /// Projection base class.
    /// </summary>
    [ContractClass(typeof(ProjectionBaseCodeContracts))]
    public abstract class ProjectionBase : ITransformation<GeographicCoordinate, Point2>
    {

        public abstract Point2 TransformValue(GeographicCoordinate source);

        public virtual IEnumerable<Point2> TransformValues(IEnumerable<GeographicCoordinate> values) {
            if (values == null) throw new ArgumentNullException("values");
            Contract.Ensures(Contract.Result<IEnumerable<Point2>>() != null);
            return values.Select(TransformValue);
        }

        public abstract ITransformation<Point2, GeographicCoordinate> GetInverse();

        public abstract bool HasInverse { [Pure] get; }

        ITransformation ITransformation.GetInverse() {
            Contract.Ensures(Contract.Result<ITransformation>() != null);
            return GetInverse();
        }

        public Type[] GetInputTypes() {
            return new[] { typeof(Point2) };
        }

        public Type[] GetOutputTypes(Type inputType) {
            return inputType == typeof(Point2)
                ? new[] {typeof(GeographicCoordinate), typeof(GeographicHeightCoordinate)}
                : ArrayUtil<Type>.Empty;
        }

        public virtual object TransformValue(object value) {
            if (value is GeographicCoordinate) {
                return TransformValue((GeographicCoordinate)value);
            }
            if (value is GeographicHeightCoordinate) {
                return TransformValue((GeographicCoordinate)((GeographicHeightCoordinate)value));
            }
            throw new InvalidOperationException();
        }

        public IEnumerable<object> TransformValues(IEnumerable<object> values) {
            Contract.Ensures(Contract.Result<IEnumerable<object>>() != null);
            return values.Select(TransformValue);
        }

    }

    /// <summary>
    /// Projection base class with spheroid values.
    /// </summary>
    public abstract class SpheroidProjectionBase : ProjectionBase
    {
        internal const double QuarterPi = Math.PI / 4.0;
        internal const double HalfPi = Math.PI / 2.0;

        protected readonly double MajorAxis;
        protected readonly double ESq;
        protected readonly double E;
        protected readonly double EHalf;
        /// <summary>
        /// The false projected offset.
        /// </summary>
        public readonly Vector2 FalseProjectedOffset;
        /// <summary>
        /// The spheroid.
        /// </summary>
        public ISpheroid<double> Spheroid { get; private set; }

        protected SpheroidProjectionBase(
            Vector2 falseProjectedOffset,
            ISpheroid<double> spheroid
        ) {
            if (null == spheroid) throw new ArgumentNullException("spheroid");
            Contract.EndContractBlock();

            Spheroid = spheroid;
            FalseProjectedOffset = falseProjectedOffset;
            MajorAxis = spheroid.A;
            ESq = spheroid.ESquared;
            E = spheroid.E;
            EHalf = E / 2.0;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(Spheroid != null);
        }

    }

    [ContractClassFor(typeof(ProjectionBase))]
    internal abstract class ProjectionBaseCodeContracts : ProjectionBase
    {

        public abstract override Point2 TransformValue(GeographicCoordinate source);

        public override ITransformation<Point2, GeographicCoordinate> GetInverse() {
            Contract.Requires(HasInverse);
            Contract.Ensures(Contract.Result<ITransformation<Point2, GeographicCoordinate>>() != null);
            Contract.EndContractBlock();
            return default(ITransformation<Point2, GeographicCoordinate>);
        }

        public override bool HasInverse {
            [Pure] get { return false; }
        }
    }

}
