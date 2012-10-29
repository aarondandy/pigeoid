// TODO: source header

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pigeoid.Contracts;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Projection
{
	/// <summary>
	/// A Lambert Conic Conformal projection with 2 standard parallels.
	/// </summary>
	public class LambertConicConformal2Sp :
		LambertConicConformal,
		IEquatable<LambertConicConformal2Sp>
	{

		internal new const string DefaultName = LambertConicConformal.DefaultName + " 2SP";

		/// <summary>
		/// The first parallel.
		/// </summary>
		public readonly double FirstParallel;
		/// <summary>
		/// The second parallel.
		/// </summary>
		public readonly double SecondParallel;

		private static double CalculateTValue(double parallel, double sinParallel, double power)
		{
			return Math.Tan(QuarterPi - (parallel / 2.0))
				/ Math.Pow((1.0 - sinParallel) / (1.0 + sinParallel), power);
		}

		private static double CalculateMValue(double parallel, double sinParallel, double eSquared)
		{
			return Math.Cos(parallel) / Math.Sqrt(1.0 - (eSquared * sinParallel * sinParallel));
		}

		/// <summary>
		/// Constructs a new Lambert Conic Conformal projection from 2 standard parallels.
		/// </summary>
		/// <param name="geographicOrigin">The geographic origin.</param>
		/// <param name="firstParallel">The first parallel.</param>
		/// <param name="secondParallel">The second parallel.</param>
		/// <param name="falseProjectedOffset">The false projected offset.</param>
		/// <param name="spheroid">The spheroid.</param>
		public LambertConicConformal2Sp(
			GeographicCoordinate geographicOrigin,
			double firstParallel,
			double secondParallel,
			Vector2 falseProjectedOffset,
			[NotNull] ISpheroid<double> spheroid
		)
			: base(geographicOrigin, falseProjectedOffset, spheroid)
		{
			FirstParallel = firstParallel;
			SecondParallel = secondParallel;
			var firstParallelSin = Math.Sin(firstParallel);
			var secondParallelSin = Math.Sin(secondParallel);
			var mFirst = CalculateMValue(firstParallel, firstParallelSin, ESq);
			var mSecond = CalculateMValue(secondParallel, secondParallelSin, ESq);
			var tFirst = CalculateTValue(firstParallel, E * firstParallelSin, EHalf);
			var tSecond = CalculateTValue(secondParallel, E * secondParallelSin, EHalf);
			N = (Math.Log(mFirst) - Math.Log(mSecond))
				/ (Math.Log(tFirst) - Math.Log(tSecond));

// ReSharper disable CompareOfFloatsByEqualityOperator
			if (0 == N || Double.IsNaN(N)) throw new ArgumentException("Invalid N value.");
// ReSharper restore CompareOfFloatsByEqualityOperator

			F = mFirst / (N * Math.Pow(tFirst, N));
			Af = MajorAxis * F;

			ROrigin = CalculateTValue(geographicOrigin.Latitude, Math.Sin(geographicOrigin.Latitude) * E, EHalf);

			// ReSharper disable CompareOfFloatsByEqualityOperator
			if (0 <= ROrigin)
				ROrigin = Math.Pow(ROrigin, N) * Af;
			// ReSharper restore CompareOfFloatsByEqualityOperator

			InvN = 1.0 / N;
			NorthingOffset = falseProjectedOffset.Y + ROrigin;
		}

		public bool Equals(LambertConicConformal2Sp other) {
			return !ReferenceEquals(other, null)
				&& (
					GeographicOrigin.Equals(other.GeographicOrigin)
// ReSharper disable CompareOfFloatsByEqualityOperator
					&& FirstParallel == other.FirstParallel
					&& SecondParallel == other.SecondParallel
// ReSharper restore CompareOfFloatsByEqualityOperator
					&& FalseProjectedOffset.Equals(other.FalseProjectedOffset)
					&& Spheroid.Equals(other.Spheroid)
				)
			;
		}

		public override bool Equals(object obj) {
			return null != obj
				&& (ReferenceEquals(this, obj) || Equals(obj as LambertConicConformal2Sp));
		}

		public override int GetHashCode() {
			return FirstParallel.GetHashCode() ^ -SecondParallel.GetHashCode();
		}

	}
}
