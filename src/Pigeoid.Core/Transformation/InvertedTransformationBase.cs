// TODO: source header

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Vertesaur.Contracts;

namespace Pigeoid.Transformation
{
	internal abstract class InvertedTransformationBase<TCore, TSource, TTarget> :
		ITransformation<TSource, TTarget>
		where TCore : ITransformation<TTarget, TSource>
	{

		private readonly TCore _core;

		protected InvertedTransformationBase([NotNull] TCore core) {
			if (!core.HasInverse)
				throw new ArgumentException("Core cannot be inverted.");
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

		[NotNull] public IEnumerable<TTarget> TransformValues([NotNull] IEnumerable<TSource> values) {
			return values.Select(TransformValue);
		}

	}

	internal abstract class InvertedTransformationBase<TCore, TCoordinate> :
		InvertedTransformationBase<TCore, TCoordinate, TCoordinate>,
		ITransformation<TCoordinate>
		where TCore : ITransformation<TCoordinate>
	{

		protected InvertedTransformationBase([NotNull] TCore core)
			: base(core) { }

		public override abstract TCoordinate TransformValue(TCoordinate value);

		public new ITransformation<TCoordinate> GetInverse() {
			return base.GetInverse() as ITransformation<TCoordinate>;
		}

		public void TransformValues([NotNull] TCoordinate[] values) {
			for (int i = 0; i < values.Length; i++) {
				TransformValue(ref values[i]);
			}
		}

		public void TransformValue(ref TCoordinate value) {
			value = TransformValue(value);
		}
	}
}
