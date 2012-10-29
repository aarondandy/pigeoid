// TODO: source header

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Projection
{
	/// <summary>
	/// Projection base class.
	/// </summary>
	public abstract class ProjectionBase : ITransformation<GeographicCoordinate, Point2>
	{
		internal const double QuarterPi = Math.PI / 4.0;
		internal const double HalfPi = Math.PI / 2.0;

		protected readonly double MajorAxis;
		protected readonly double ESq;
		protected readonly double E;
		protected readonly double EHalf;
		/// <summary>
		/// The false projected offset.
		/// </summary>
		public readonly Vector2 FalseProjectedOffset;
		/// <summary>
		/// The spheroid.
		/// </summary>
		public readonly ISpheroid<double> Spheroid;

		protected ProjectionBase(
			Vector2 falseProjectedOffset,
			[NotNull] ISpheroid<double> spheroid
		) {
			if (null == spheroid)
				throw new ArgumentNullException("spheroid");

			Spheroid = spheroid;
			FalseProjectedOffset = falseProjectedOffset;
			MajorAxis = spheroid.A;
			ESq = spheroid.ESquared;
			E = spheroid.E;
			EHalf = E / 2.0;
		}

		public abstract Point2 TransformValue(GeographicCoordinate source);

		[ContractAnnotation("notnull=>notnull")]
		public virtual IEnumerable<Point2> TransformValues([NotNull] IEnumerable<GeographicCoordinate> values) {
			return values.Select(TransformValue);
		}

		public abstract ITransformation<Point2, GeographicCoordinate> GetInverse();

		public abstract bool HasInverse { get; }

		ITransformation ITransformation.GetInverse() {
			return GetInverse();
		}

	}
}
