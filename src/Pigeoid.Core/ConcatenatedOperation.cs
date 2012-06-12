using System;
using System.Collections.Generic;
using System.Linq;
using Pigeoid.Contracts;
using Vertesaur.Contracts;

namespace Pigeoid
{

	public class ConcatenatedOperation : IConcatenatedOperation
	{

		

		protected readonly ITransformation[] _transformations;

		public ConcatenatedOperation(ITransformation[] transformations) {
			if(null == transformations)
				throw new ArgumentNullException("transformations");
			if (transformations.Length != 0 && transformations.Any(x => x == null))
				throw new ArgumentException("Null transformations are not valid.");

			_transformations = (ITransformation[])transformations.Clone();
		}

		protected ITransformation[] CreateInverseOperations() {
			var inverseTransformations = new ITransformation[_transformations.Length];
			for (int i = 0; i < inverseTransformations.Length; i++) {
				var tx = _transformations[_transformations.Length - 1 - i];
				if(!tx.HasInverse)
					throw new InvalidOperationException("Operation has no inverse.");
				var ix = tx.GetInverse();
				if(null == ix)
					throw new InvalidOperationException("Operation has invalid inverse.");
				inverseTransformations[i] = ix;
			}
			return inverseTransformations;
		}

		protected virtual ConcatenatedOperation CreateInverseConcatenatedOperation() {
			return new ConcatenatedOperation(CreateInverseOperations());
		}

		public IEnumerable<ITransformation> Transformations {
			get { return Array.AsReadOnly(_transformations); }
		}

		public ConcatenatedOperation GetInverse() {
			return CreateInverseConcatenatedOperation();
		}

		ITransformation ITransformation.GetInverse() {
			return GetInverse();
		}

		public bool HasInverse {
			get { return 0 == _transformations.Length || _transformations.All(x => x.HasInverse); }
		}
	}

	public class ConcatenatedOperation<TFrom,TTo> : ConcatenatedOperation, IConcatenatedOperation<TFrom,TTo>
	{

		private static bool HasValidJoins(ITransformation[] transformations) {
			if (transformations.Length > 1) {
				var prevGenerics = ExtractTypedTransformInterfaces(transformations[0]).Select(x => x.GetGenericArguments()).ToArray();
				for (int i = 1; i < transformations.Length; i++) {
					var currGenerics = ExtractTypedTransformInterfaces(transformations[i]).Select(x => x.GetGenericArguments()).ToArray();
					if (!IsValidJoin(prevGenerics, currGenerics))
						return false;

					prevGenerics = currGenerics;
				}
			}
			return true;
		}

		private static bool IsValidJoin(Type[][] a, Type[][] b) {
			if (null == a || null == b || a.Length == 0 || b.Length == 0)
				return false;

			foreach (var aType in a.Select(x => x[1]))
				foreach (var bType in b.Select(x => x[0]))
					if (aType == bType)
						return true;

			return false;
		}

		protected static IEnumerable<Type> ExtractTypedTransformInterfaces(ITransformation transformation) {
			return transformation
				.GetType()
				.GetInterfaces()
				.Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ITransformation<,>));
		}

		

		public ConcatenatedOperation(ITransformation[] transformations)
			: base(transformations)
		{
			if(!HasValidJoins(transformations))
				throw new ArgumentException("Transformation value types do not join.");
		}

		protected override ConcatenatedOperation CreateInverseConcatenatedOperation() {
			return GetInverse();
		}

		public ConcatenatedOperation<TTo,TFrom> GetInverse() {
			return new ConcatenatedOperation<TTo, TFrom>(CreateInverseOperations());
		}

		ITransformation ITransformation.GetInverse() {
			return GetInverse();
		}

		ITransformation<TTo, TFrom> ITransformation<TFrom, TTo>.GetInverse() {
			return GetInverse();
		}

		public TTo TransformValue(TFrom value) {
			if (_transformations.Length == 1)
				return ((ITransformation<TFrom, TTo>)(_transformations[0])).TransformValue(value);
			
			throw new NotImplementedException();
		}

		public IEnumerable<TTo> TransformValues(IEnumerable<TFrom> values) {
			if (_transformations.Length == 1)
				return ((ITransformation<TFrom, TTo>)(_transformations[0])).TransformValues(values);

			throw new NotImplementedException();
		}
	}

	public class ConcatenatedOperation<TValue> : ConcatenatedOperation<TValue, TValue>, IConcatenatedOperation<TValue>
	{

		private static bool CheckFirstAndLastTypesMatch(ITransformation[] transformations) {
			if (transformations.Length > 0) {
				var firstOk = ExtractTypedTransformInterfaces(transformations[0])
					.Any(t => t.GetGenericArguments()[0] == typeof(TValue));
				if (!firstOk)
					return false;

				var lastOk = ExtractTypedTransformInterfaces(transformations[transformations.Length - 1])
					.Any(t => t.GetGenericArguments()[1] == typeof(TValue));
				if (!lastOk)
					return false;
			}
			return true;
		}

		public ConcatenatedOperation(ITransformation[] transformations)
			: base(transformations)
		{
			if(!CheckFirstAndLastTypesMatch(transformations))
				throw new ArgumentException("First and last transformation value types must match.");
		}

		protected override ConcatenatedOperation CreateInverseConcatenatedOperation() {
			return GetInverse();
		}

		public ConcatenatedOperation<TValue> GetInverse() {
			return new ConcatenatedOperation<TValue>(CreateInverseOperations());
		}

		ITransformation<TValue, TValue> ITransformation<TValue, TValue>.GetInverse() {
			return GetInverse();
		}

		ITransformation<TValue> ITransformation<TValue>.GetInverse() {
			return GetInverse();
		}

		public void TransformValues(TValue[] values) {
			for (int i = 0; i < values.Length; i++)
				values[i] = TransformValue(values[i]);
		}
	}

}
