using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Pigeoid.Utility;
using Vertesaur.Transformation;

namespace Pigeoid.CoordinateOperation.Transformation
{
    internal abstract class InvertedTransformationBase<TCore, TSource, TTarget> :
        ITransformation<TSource, TTarget>
        where TCore : ITransformation<TTarget, TSource>
    {

        protected InvertedTransformationBase(TCore core) {
            if(core == null) throw new ArgumentNullException("core");
            Contract.EndContractBlock();
            Core = core;
        }

        [ContractInvariantMethod]
        private void CodeContractInvariants() {
            Contract.Invariant(Core != null);
        }

        public TCore Core { get; private set; }

        public object TransformValue(object value) {
            return TransformValue((TSource)value);
        }

        public IEnumerable<object> TransformValues(IEnumerable<object> values) {
            return values.Select(TransformValue);
        }

        public Type[] GetInputTypes() {
            return new []{ typeof(TSource) };
        }

        public Type[] GetOutputTypes(Type inputType) {
            return inputType == typeof(TSource)
                ? new[] { typeof(TTarget) }
                : ArrayUtil<Type>.Empty;
        }

        public ITransformation<TTarget, TSource> GetInverse() {
            Contract.Ensures(Contract.Result<ITransformation<TTarget, TSource>>() != null);
            return Core;
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
