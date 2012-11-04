using System;
using Pigeoid.Transformation;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Projection
{
	public class EquidistantCylindrical : ProjectionBase
	{

		private class Inverse : InvertedTransformationBase<EquidistantCylindrical,Point2, GeographicCoordinate>
		{

			public Inverse(EquidistantCylindrical core) : base(core) {
				throw new NotImplementedException();
			}

			public override GeographicCoordinate TransformValue(Point2 value) {
				throw new NotImplementedException();
			}
		}

		public EquidistantCylindrical(Vector2 falseProjectedOffset, ISpheroid<double> spheroid)
			: base(falseProjectedOffset, spheroid)
		{
			
		}

		public override Point2 TransformValue(GeographicCoordinate source) {
			throw new NotImplementedException();
		}

		public override ITransformation<Point2, GeographicCoordinate> GetInverse() {
			if(!HasInverse)
				throw new InvalidOperationException("No inverse.");
			return new Inverse(this);
		}

		public override bool HasInverse {
			get {
				throw new NotImplementedException();
			}
		}
	}
}
