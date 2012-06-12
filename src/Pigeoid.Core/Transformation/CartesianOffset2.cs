// TODO: source header

using System.Collections.Generic;
using System.Linq;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Transformation
{
	public class CartesianOffset2 : ITransformation<Point2>
	{

		public readonly Vector2 Offset;

		public CartesianOffset2(Vector2 offset) {
			Offset = offset;
		}

		public void TransformValues(Point2[] values) {
			for (int i = 0; i < values.Length; i++) {
				TransformValue(ref values[i]);
			}
		}

		private void TransformValue(ref Point2 value) {
			value = value.Add(Offset);
		}

		public ITransformation<Point2> GetInverse() {
			return new CartesianOffset2(Offset.GetNegative());
		}

		public Point2 TransformValue(Point2 value) {
			return value.Add(Offset);
		}

		public IEnumerable<Point2> TransformValues(IEnumerable<Point2> values) {
			return values.Select(Offset.Add);
		}

		public bool HasInverse {
			get { return true; }
		}

		ITransformation ITransformation.GetInverse() {
			return GetInverse();
		}

		ITransformation<Point2, Point2> ITransformation<Point2, Point2>.GetInverse() {
			return GetInverse();
		}

	}
}
