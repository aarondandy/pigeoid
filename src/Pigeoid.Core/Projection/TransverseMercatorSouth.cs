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

		private class Inverted : InvertedTransformationBase<TransverseMercatorSouth,Point2,GeographicCoordinate>
		{

			private readonly InvertedTransformationBase<TransverseMercator,Point2,GeographicCoordinate> _baseInv;

			public Inverted(TransverseMercatorSouth core) : base(core) {
				_baseInv = core.BaseInverse;
			}

			public override GeographicCoordinate TransformValue(Point2 source) {
				return _baseInv.TransformValue(new Point2(-source.X, -source.Y));
			}
		}

		public TransverseMercatorSouth(
			GeographicCoordinate naturalOrigin,
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

		public override Point2 TransformValue(GeographicCoordinate coordinate) {
			var p = base.TransformValue(coordinate);
			return new Point2(-p.X, -p.Y);
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private InvertedTransformationBase<TransverseMercator,Point2,GeographicCoordinate> BaseInverse {
			get { return base.GetInverse() as InvertedTransformationBase<TransverseMercator,Point2,GeographicCoordinate>; }
		}

		public override ITransformation<Point2, GeographicCoordinate> GetInverse() {
			return new Inverted(this);
		}

		public override string Name {
			get { return CoordinateOperationStandardNames.TransverseMercatorSouthOriented; }
		}

	}
}
