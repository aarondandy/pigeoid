using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Projection
{
	public abstract class LambertConicBase : ProjectionBase
	{

		/// <summary>
		/// The geographic origin of the projection.
		/// </summary>
		protected readonly GeographicCoordinate GeographicOrigin;

		protected LambertConicBase(
			GeographicCoordinate geographicOrigin,
			Vector2 falseProjectedOffset,
			ISpheroid<double> spheroid
		)
			: base(falseProjectedOffset, spheroid)
		{
			GeographicOrigin = geographicOrigin;
		}

		public abstract override Point2 TransformValue(GeographicCoordinate source);

		public abstract override ITransformation<Point2, GeographicCoordinate> GetInverse();

		public abstract override bool HasInverse { get; }

		public abstract override string Name { get; }
	}
}
