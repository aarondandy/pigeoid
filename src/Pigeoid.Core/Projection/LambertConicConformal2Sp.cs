// TODO: source header

using System;
using System.Collections.Generic;
using System.Linq;
using Pigeoid.Contracts;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Projection
{
	/// <summary>
	/// A lambert conic conformal projection with 2 standard parallels.
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

		/// <summary>
		/// Constructs a new lambert conic conformal projection from 2 standard parallels.
		/// </summary>
		/// <param name="geographiOrigin">The geographic origin.</param>
		/// <param name="firstParallel">The first parallel.</param>
		/// <param name="secondParallel">The second parallel.</param>
		/// <param name="falseProjectedOffset">The false projected offset.</param>
		/// <param name="spheroid">The spheroid.</param>
		public LambertConicConformal2Sp(
			GeographicCoord geographiOrigin,
			double firstParallel,
			double secondParallel,
			Vector2 falseProjectedOffset,
			ISpheroid<double> spheroid
		)
			: base(geographiOrigin, falseProjectedOffset, spheroid)
		{
			FirstParallel = firstParallel;
			SecondParallel = secondParallel;
			double firstParallelSin = Math.Sin(firstParallel);
			double eFirstParallelSin = E * firstParallelSin;
			double secondParallelSin = Math.Sin(secondParallel);
			double eSecondParallelSin = E * secondParallelSin;
			double eOriginParallelSin = E * Math.Sin(geographiOrigin.Latitude);
			double mFirst =
				Math.Cos(firstParallel)
				/ Math.Sqrt(1.0 - (ESq * firstParallelSin * firstParallelSin));
			double mSecond =
				Math.Cos(secondParallel)
				/ Math.Sqrt(1.0 - (ESq * secondParallelSin * secondParallelSin));
			double tFirst =
				Math.Tan(QuarterPi - (firstParallel / 2.0))
				/ Math.Pow((1.0 - eFirstParallelSin) / (1.0 + eFirstParallelSin),EHalf);
			double tSecond =
				Math.Tan(QuarterPi - (secondParallel / 2.0))
				/ Math.Pow((1.0 - eSecondParallelSin) / (1.0 + eSecondParallelSin), EHalf);
			N =
				(Math.Log(mFirst) - Math.Log(mSecond))
				/ (Math.Log(tFirst) - Math.Log(tSecond));

			if (0 == N || Double.IsNaN(N))
				throw new ArgumentException("Invalid N value.");

			F = mFirst / (N * Math.Pow(tFirst, N));
			Af = MajorAxis * F;
			double tOrigin =
				Math.Tan(QuarterPi - (geographiOrigin.Latitude / 2.0))
				/ Math.Pow((1.0 - eOriginParallelSin) / (1.0 + eOriginParallelSin),EHalf);
			ROrigin = MajorAxis * F * Math.Pow(tOrigin, N);

			/*ROrigin = Af * System.Math.Pow(
					System.Math.Tan(System.Math.Max(0, QuarterPi - (geographiOrigin.Lat / 2.0))) / System.Math.Pow(
						(1.0 - eOriginParallelSin) / (1.0 + eOriginParallelSin),
						EHalf
					)
				,
				N
			);*/

			Invn = 1.0 / N;
			NorthingOffset = falseProjectedOffset.Y + ROrigin;
		}

		public override string Name {
			get { return DefaultName; }
		}

		public override IEnumerable<INamedParameter> GetParameters() {
			return base.GetParameters()
				.Concat(new INamedParameter[]
                    {
                        new NamedParameter<double>(NamedParameter.NameLatitudeOfFirstStandardParallel,FirstParallel), 
                        new NamedParameter<double>(NamedParameter.NameLatitudeOfSecondStandardParallel,SecondParallel),
                    }
				)
			;
		}


		public bool Equals(LambertConicConformal2Sp other) {
			return !ReferenceEquals(other, null)
				&& (
					GeographiOrigin.Equals(other.GeographiOrigin)
					&& FirstParallel == other.FirstParallel
					&& SecondParallel == other.SecondParallel
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
