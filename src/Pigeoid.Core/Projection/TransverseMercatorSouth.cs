// TODO: source header

using System.Diagnostics;
using Pigeoid.Interop;
using Pigeoid.Transformation;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Projection
{
	public class TransverseMercatorSouth :
		TransverseMercator
	{

		private class Inverted : InvertedTransformationBase<TransverseMercatorSouth,Point2,GeographicCoord>
		{

			private readonly InvertedTransformationBase<TransverseMercator,Point2,GeographicCoord> _baseInv;

			public Inverted(TransverseMercatorSouth core)
				: base(core)
			{
				_baseInv = core.BaseInverse;
			}

			public override GeographicCoord TransformValue(Point2 source) {
				return _baseInv.TransformValue(new Point2(-source.X, -source.Y));
			}
		}

		public TransverseMercatorSouth(
			GeographicCoord naturalOrigin,
			Vector2 falseProjectedOffset,
			double scaleFactor,
			ISpheroid<double> spheroid
		)
			: base(
				naturalOrigin,
				new Vector2(-falseProjectedOffset.X, -falseProjectedOffset.Y),
				scaleFactor,
				spheroid
			) { }

		public override Point2 TransformValue(GeographicCoord coord) {
			var p = base.TransformValue(coord);
			return new Point2(-p.X, -p.Y);
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private InvertedTransformationBase<TransverseMercator,Point2,GeographicCoord> BaseInverse {
			get { return base.GetInverse() as InvertedTransformationBase<TransverseMercator,Point2,GeographicCoord>; }
		}

		public override ITransformation<Point2, GeographicCoord> GetInverse() {
			return new Inverted(this);
		}

		public override string Name {
			get { return CoordinateOperationStandardNames.TransverseMercatorSouthOriented; }
		}

	}
}
