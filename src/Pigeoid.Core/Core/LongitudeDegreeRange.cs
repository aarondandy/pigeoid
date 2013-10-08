using System;
using System.Diagnostics.Contracts;
using Pigeoid.Core;
using Vertesaur;
using Vertesaur.Periodic;

namespace Pigeoid
{
    public struct LongitudeDegreeRange :
        IPeriodicRange<double>,
        IEquatable<LongitudeDegreeRange>,
        IEquatable<double>,
        IHasDistance<double, double>,
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
        [Pure] public static bool operator ==(LongitudeDegreeRange a, LongitudeDegreeRange b) {
            return a.Equals(b);
        }

        /// <summary>
        /// Determines if two ranges are not equal.
        /// </summary>
        /// <param name="a">A range.</param>
        /// <param name="b">A range.</param>
        /// <returns>True when the ranges are not equal.</returns>
        [Pure] public static bool operator !=(LongitudeDegreeRange a, LongitudeDegreeRange b) {
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

        double IPeriodicRange<double>.Start { get { return Start; } }

        double IPeriodicRange<double>.End { get { return End; } }

        public double Mid {
            [Pure] get { return DefaultPeriodicOperations.CalculateMidpoint(Start, End); }
        }

        [Pure] public double GetMagnitude() {
            return DefaultPeriodicOperations.Magnitude(Start, End);
        }

        [Pure] public double GetMagnitudeSquared() {
            var m = GetMagnitude();
            return m * m;
        }

        [Pure] public double Distance(double value) {
            return DefaultPeriodicOperations.Distance(Start, End, value);
        }

        [Pure] public double DistanceSquared(double value) {
            var d = Distance(value);
            return d * d;
        }

        [Pure] public bool Intersects(double value) {
            return DefaultPeriodicOperations.Intersects(Start, End, value);
        }

        [Pure] public bool Intersects(LongitudeDegreeRange r) {
            return DefaultPeriodicOperations.Intersects(Start, End, r.Start, r.End);
        }

        [Pure] public bool Contains(double value) {
            return DefaultPeriodicOperations.Contains(Start, End, value);
        }

        [Pure] public bool Contains(LongitudeDegreeRange r) {
            return DefaultPeriodicOperations.Contains(Start, End, r.Start, r.End);
        }

        [Pure] public bool Within(LongitudeDegreeRange r) {
            return DefaultPeriodicOperations.Contains(r.Start, r.End, Start, End);
        }

        [Pure] public bool Equals(LongitudeDegreeRange r) {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            return Start == r.Start && End == r.End;
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        [Pure] public bool Equals(double v) {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            return Start == v && End == v;
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        [Pure] public override bool Equals(object obj) {
            return obj is LongitudeDegreeRange
                ? Equals((LongitudeDegreeRange)obj)
                : (obj is double && Equals((double)obj));
        }

        [Pure] public override int GetHashCode() {
            return Start.GetHashCode() ^ -End.GetHashCode();
        }

        [Pure] public override string ToString() {
            Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));
            return String.Concat(Start, ':', End);
        }

        [Pure] public bool TryIntersection(LongitudeDegreeRange other, out LongitudeDegreeRange result) {
            if (Intersects(other)) {
                result = new LongitudeDegreeRange(
                    Math.Max(Start, other.Start),
                    Math.Min(End, other.End));
                return true;
            }
            result = default(LongitudeDegreeRange);
            return false;
        }

    }
}
