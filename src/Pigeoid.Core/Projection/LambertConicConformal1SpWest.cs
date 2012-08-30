// TODO: source header

using System.Diagnostics;
using Pigeoid.Transformation;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Projection
{
	public class LambertConicConformal1SpWest :
		LambertConicConformal1Sp
	{

		private class Inverted : InvertedTransformationBase<LambertConicConformal1SpWest,Point2,GeographicCoordinate>
		{

			private readonly InvertedTransformationBase<LambertConicConformal,Point2,GeographicCoordinate> _baseInv;

			public Inverted(LambertConicConformal1SpWest core) : base(core) {
				_baseInv = core.BaseInverse;
			}

			public override GeographicCoordinate TransformValue(Point2 source) {
				return _baseInv.TransformValue(new Point2(-source.X, source.Y));
			}
		}

		public LambertConicConformal1SpWest(
			GeographicCoordinate geographicOrigin,
			double originScaleFactor,
			Vector2 falseProjectedOffset,
			ISpheroid<double> spheroid
		)
			: base(
			  geographicOrigin,
			  originScaleFactor,
			  new Vector2(-falseProjectedOffset.X, falseProjectedOffset.Y),
			  spheroid
			) { }

		public override Point2 TransformValue(GeographicCoordinate coordinate) {
			Point2 p = base.TransformValue(coordinate);
			return new Point2(-p.X, p.Y);
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private InvertedTransformationBase<LambertConicConformal,Point2,GeographicCoordinate> BaseInverse {
			get { return base.GetInverse() as InvertedTransformationBase<LambertConicConformal,Point2,GeographicCoordinate>; }
		}

		public override ITransformation<Point2, GeographicCoordinate> GetInverse() {
			return new Inverted(this);
		}

		public override string Name {
			get { return "Lambert Conic Conformal (West Orientated)"; }
		}

	}
}
