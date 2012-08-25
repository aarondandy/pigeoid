// TODO: source header

using System.Collections.Generic;
using System.Linq;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Transformation
{
	/// <summary>
	/// A geocentric translation.
	/// </summary>
	public class GeocentricTranslation : ITransformation<Point3>
	{

		public readonly Vector3 D;

		/// <summary>
		/// Constructs a new geocentric translation operation.
		/// </summary>
		/// <param name="d"></param>
		public GeocentricTranslation(Vector3 d) {
			D = d;
		}

		public Point3 TransformValue(Point3 p) {
			return p.Add(D);
		}

		public IEnumerable<Point3> TransformValues(IEnumerable<Point3> values) {
			return values.Select(D.Add);
		}

		public void TransformValues(Point3[] values) {
			for (int i = 0; i < values.Length; i++) {
				TransformValue(ref values[i]);
			}
		}

		private void TransformValue(ref Point3 p) {
			p = p.Add(D);
		}

		public bool HasInverse {
			get { return true; }
		}

		ITransformation<Point3, Point3> ITransformation<Point3, Point3>.GetInverse() {
			return GetInverse();
		}

		ITransformation ITransformation.GetInverse() {
			return GetInverse();
		}

		public ITransformation<Point3> GetInverse() {
			return new GeocentricTranslation(D.GetNegative());
		}
	}
}
