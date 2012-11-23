using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pigeoid.Transformation;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Projection
{
	public class TunisiaMiningGrid : ProjectionBase
	{

		private class Inverse : InvertedTransformationBase<TunisiaMiningGrid,Point2,GeographicCoordinate>
		{
			public Inverse(TunisiaMiningGrid core) : base(core) { }

			public override GeographicCoordinate TransformValue(Point2 value) {
				throw new NotImplementedException();
			}
		}

		public static Vector2 GridReferenceToOffset(int gridReference){
			var easting = gridReference / 1000;
			var northing = gridReference - (easting * 1000);
			return new Vector2(easting, northing);
		}

		public TunisiaMiningGrid(int gridReference, ISpheroid<double> spheroid) : this(GridReferenceToOffset(gridReference), spheroid) { }

		public TunisiaMiningGrid(Vector2 offset, ISpheroid<double> spheroid) : base(offset,spheroid){
			
		}

		public override Point2 TransformValue(GeographicCoordinate source) {
			throw new NotImplementedException();
		}

		public override ITransformation<Point2, GeographicCoordinate> GetInverse() {
			throw new NotImplementedException();
		}

		public override bool HasInverse {
			get { throw new NotImplementedException(); }
		}
	}
}
