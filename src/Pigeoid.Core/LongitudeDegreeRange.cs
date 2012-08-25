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

		private static readonly PeriodicOperations DefaultPeriodicOperations;

		static LongitudeDegreeRange() {
			DefaultPeriodicOperations = new PeriodicOperations(-180, 360);
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
			get { return DefaultPeriodicOperations.CalculateMidpoint(Start,End); }
		}

		public double GetMagnitude() {
			return DefaultPeriodicOperations.Magnitude(Start, End);
		}

		public double GetMagnitudeSquared() {
			var m = GetMagnitude();
			return m * m;
		}

		public double Distance(double value) {
			return DefaultPeriodicOperations.Distance(Start, End, value);
		}

		public double DistanceSquared(double value) {
			var d = Distance(value);
			return d * d;
		}

		public bool Intersects(double value) {
			return DefaultPeriodicOperations.Intersects(Start, End, value);
		}

		public bool Intersects(LongitudeDegreeRange r) {
			return DefaultPeriodicOperations.Intersects(Start, End, r.Start, r.End);
		}

		public bool Contains(double value) {
			return DefaultPeriodicOperations.Contains(Start, End, value);
		}

		public bool Contains(LongitudeDegreeRange r) {
			return DefaultPeriodicOperations.Contains(Start, End, r.Start, r.End);
		}

		public bool Within(LongitudeDegreeRange r) {
			return DefaultPeriodicOperations.Contains(r.Start, r.End, Start, End);
		}

		public bool Equals(LongitudeDegreeRange r) {
// ReSharper disable CompareOfFloatsByEqualityOperator
			return Start == r.Start && End == r.End;
// ReSharper restore CompareOfFloatsByEqualityOperator
		}

		public bool Equals(double v) {
// ReSharper disable CompareOfFloatsByEqualityOperator
			return Start == v && End == v;
// ReSharper restore CompareOfFloatsByEqualityOperator
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
