// TODO: source header

using System;
using JetBrains.Annotations;
using Pigeoid.Transformation;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Projection
{
	public class LambertAzimuthalEqualArea : ProjectionBase
	{

		private class Inverted : InvertedTransformationBase<LambertAzimuthalEqualArea,Point2,GeographicCoordinate>
		{

			public Inverted([NotNull] LambertAzimuthalEqualArea core) : base(core) { }

			public override GeographicCoordinate TransformValue(Point2 source) {
				throw new NotImplementedException();
			}
		}

		public LambertAzimuthalEqualArea([NotNull] ISpheroid<double> spheroid)
			: base(Vector2.Zero, spheroid) { }

		public override Point2 TransformValue(GeographicCoordinate source) {
			throw new NotImplementedException();
		}

		public override ITransformation<Point2, GeographicCoordinate> GetInverse() {
			return new Inverted(this);
		}

		public override bool HasInverse {
			get { return true; }
		}

	}
}
