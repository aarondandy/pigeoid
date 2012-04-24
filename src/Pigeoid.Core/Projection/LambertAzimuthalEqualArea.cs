﻿// TODO: source header

using System;
using Pigeoid.Interop;
using Pigeoid.Transformation;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Projection
{
	public class LambertAzimuthalEqualArea : ProjectionBase
	{

		private class Inverted : InvertedTransformationBase<LambertAzimuthalEqualArea,Point2,GeographicCoord>
		{

			public Inverted(LambertAzimuthalEqualArea core)
				: base(core) { }

			public override GeographicCoord TransformValue(Point2 source) {
				throw new NotImplementedException();
			}
		}

		public LambertAzimuthalEqualArea(ISpheroid<double> spheroid)
			: base(Vector2.Zero, spheroid) { }

		public override Point2 TransformValue(GeographicCoord source) {
			throw new NotImplementedException();
		}

		public override ITransformation<Point2, GeographicCoord> GetInverse() {
			return new Inverted(this);
		}

		public override bool HasInverse {
			get { return true; }
		}

		public override string Name {
			get { return CoordinateOperationStandardNames.LambertAzimuthalEqualArea; }
		}
	}
}