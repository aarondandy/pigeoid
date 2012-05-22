using System;
using Vertesaur.Contracts;
using Vertesaur.Periodic;

namespace Pigeoid
{
	public struct LongitudeDegreeRange :
		IEquatable<LongitudeDegreeRange>,
		IEquatable<double>,
		IHasDistance<double,double>,
		IHasMagnitude<double>,
		IRelatableIntersects<double>,
		IRelatableIntersects<LongitudeDegreeRange>,
		IRelatableContains<double>,
		IRelatableContains<LongitudeDegreeRange>,
		IRelatableWithin<LongitudeDegreeRange>
	{

		private static readonly PeriodicOperations PeriodicOperations;

		static LongitudeDegreeRange() {
			PeriodicOperations = new PeriodicOperations(-180, 360);
		}

		/// <summary>
		/// Determines if two ranges are equal.
		/// </summary>
		/// <param name="a">A range.</param>
		/// <param name="b">A range.</param>
		/// <returns>True when the ranges are equal.</returns>
		public static bool operator ==(LongitudeDegreeRange a, LongitudeDegreeRange b) {
			return a.Equals(b);
		}

		/// <summary>
		/// Determines if two ranges are not equal.
		/// </summary>
		/// <param name="a">A range.</param>
		/// <param name="b">A range.</param>
		/// <returns>True when the ranges are not equal.</returns>
		public static bool operator !=(LongitudeDegreeRange a, LongitudeDegreeRange b) {
			return !a.Equals(b);
		}

		/// <summary>
		/// The starting value of the range.
		/// </summary>
		public readonly double Start;

		/// <summary>
		/// The ending value of the range.
		/// </summary>
		public readonly double End;

		public LongitudeDegreeRange(double v) {
			Start = End = v;
		}

		public LongitudeDegreeRange(double start, double end) {
			Start = start;
			End = end;
		}

		public double Mid {
			get { return PeriodicOperations.CalculateMidpoint(Start,End); }
		}

		public double GetMagnitude() {
			return PeriodicOperations.Magnitude(Start, End);
		}

		public double GetMagnitudeSquared() {
			var m = GetMagnitude();
			return m * m;
		}

		public double Distance(double value) {
			return PeriodicOperations.Distance(Start, End, value);
		}

		public double DistanceSquared(double value) {
			var d = Distance(value);
			return d * d;
		}

		public bool Intersects(double value) {
			return PeriodicOperations.Intersects(Start, End, value);
		}

		public bool Intersects(LongitudeDegreeRange r) {
			return PeriodicOperations.Intersects(Start, End, r.Start, r.End);
		}

		public bool Contains(double value) {
			return PeriodicOperations.Contains(Start, End, value);
		}

		public bool Contains(LongitudeDegreeRange r) {
			return PeriodicOperations.Contains(Start, End, r.Start, r.End);
		}

		public bool Within(LongitudeDegreeRange r) {
			return PeriodicOperations.Contains(r.Start, r.End, Start, End);
		}

		public bool Equals(LongitudeDegreeRange r) {
			return Start == r.Start && End == r.End;
		}

		public bool Equals(double v) {
			return Start == v && End == v;
		}

		public override bool Equals(object obj) {
			return obj is LongitudeDegreeRange
				? Equals((LongitudeDegreeRange) obj)
				: obj is double
				&& Equals((double) obj)
			;
		}

		public override int GetHashCode() {
			return Start.GetHashCode() ^ -End.GetHashCode();
		}

		public override string ToString() {
			return String.Concat(Start, ':', End);
		}

	}
}
