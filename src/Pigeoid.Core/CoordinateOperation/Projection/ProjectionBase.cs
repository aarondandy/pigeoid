using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
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

        protected ProjectionBase(
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

        public abstract Point2 TransformValue(GeographicCoordinate source);

        public virtual IEnumerable<Point2> TransformValues(IEnumerable<GeographicCoordinate> values) {
            if(values == null) throw new ArgumentNullException("values");
            Contract.Ensures(Contract.Result<IEnumerable<Point2>>() != null);
            return values.Select(TransformValue);
        }

        public abstract ITransformation<Point2, GeographicCoordinate> GetInverse();

        public abstract bool HasInverse { [Pure] get; }

        ITransformation ITransformation.GetInverse() {
            Contract.Ensures(Contract.Result<ITransformation>() != null);
            return GetInverse();
        }

    }

    [ContractClassFor(typeof(ProjectionBase))]
    internal abstract class ProjectionBaseCodeContracts : ProjectionBase
    {

        internal ProjectionBaseCodeContracts() : base(default(Vector2), null) { }

        public override Point2 TransformValue(GeographicCoordinate source) {
            throw new NotImplementedException();
        }

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
