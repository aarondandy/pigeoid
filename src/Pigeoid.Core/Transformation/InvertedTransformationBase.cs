using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Vertesaur.Contracts;

namespace Pigeoid.Transformation
{
    internal abstract class InvertedTransformationBase<TCore, TSource, TTarget> :
        ITransformation<TSource, TTarget>
        where TCore : ITransformation<TTarget, TSource>
    {

        private readonly TCore _core;

        protected InvertedTransformationBase(TCore core) {
            if(core == null) throw new ArgumentNullException("core");
            Contract.EndContractBlock();
            _core = core;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(_core != null);
        }

        public TCore Core {
            get {
                Contract.Ensures(Contract.Result<TCore>() != null);
                return _core;
            }
        }

        public ITransformation<TTarget, TSource> GetInverse() {
            Contract.Ensures(Contract.Result<ITransformation<TTarget, TSource>>() != null);
            return _core;
        }

        public bool HasInverse {
            [Pure] get { return true; }
        }

        ITransformation ITransformation.GetInverse() {
            return GetInverse();
        }

        public abstract TTarget TransformValue(TSource value);

        public IEnumerable<TTarget> TransformValues(IEnumerable<TSource> values) {
            Contract.Ensures(Contract.Result<IEnumerable<TTarget>>() != null);
            return values.Select(TransformValue);
        }

    }

    internal abstract class InvertedTransformationBase<TCore, TCoordinate> :
        InvertedTransformationBase<TCore, TCoordinate, TCoordinate>,
        ITransformation<TCoordinate>
        where TCore : ITransformation<TCoordinate>
    {

        protected InvertedTransformationBase(TCore core)
            : base(core) {
            Contract.Requires(core != null);
        }

        public override abstract TCoordinate TransformValue(TCoordinate value);

        public new ITransformation<TCoordinate> GetInverse() {
            return (ITransformation<TCoordinate>)(base.GetInverse());
        }

        public void TransformValues(TCoordinate[] values) {
            for (int i = 0; i < values.Length; i++) {
                TransformValue(ref values[i]);
            }
        }

        public void TransformValue(ref TCoordinate value) {
            value = TransformValue(value);
        }
    }
}
