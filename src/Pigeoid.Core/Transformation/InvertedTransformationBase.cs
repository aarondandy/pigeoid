
using System.Collections.Generic;
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
			_core = core;
		}

		public TCore Core {
			get { return _core; }
		}

		public ITransformation<TTarget, TSource> GetInverse() {
			return _core;
		}

		public bool HasInverse
		{
			get { return true; }
		}

		ITransformation ITransformation.GetInverse() {
			return GetInverse();
		}


		public abstract TTarget TransformValue(TSource value);

		public IEnumerable<TTarget> TransformValues(IEnumerable<TSource> values) {
			return values.Select(TransformValue);
		}

	}

	internal abstract class InvertedTransformationBase<TCore, TCoord> :
		InvertedTransformationBase<TCore, TCoord, TCoord>,
		ITransformation<TCoord>
		where TCore : ITransformation<TCoord>
	{

		protected InvertedTransformationBase(TCore core)
			: base(core) { }

		public override abstract TCoord TransformValue(TCoord value);

		public new ITransformation<TCoord> GetInverse() {
			return base.GetInverse() as ITransformation<TCoord>;
		}

		public void TransformValues(TCoord[] values) {
			for (int i = 0; i < values.Length; i++) {
				TransformValue(ref values[i]);
			}
		}

		public void TransformValue(ref TCoord value) {
			value = TransformValue(value);
		}
	}
}
