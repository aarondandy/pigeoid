using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pigeoid.Transformation;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Projection
{
	public class TunisiaMiningGrid : ITransformation<GeographicCoordinate, Point2>
	{

		private class Inverse : InvertedTransformationBase<TunisiaMiningGrid,Point2,GeographicCoordinate>
		{
			public Inverse(TunisiaMiningGrid core) : base(core) { }

			public override GeographicCoordinate TransformValue(Point2 value) {
				var lat = 36.5964 + ((value.Y - 360.0) * (value.Y > 360.0 ? 0.010015 : 0.01002));
				var lon = 7.83445 + ((value.X - 270.0) * 0.012185);
				return new GeographicCoordinate(lat, lon);
			}
		}

		public static Point2 GridReferenceToLocation(int gridReference){
			var easting = gridReference / 1000;
			var northing = gridReference - (easting * 1000);
			return new Point2(easting, northing);
		}

		public Point2 TransformValue(GeographicCoordinate source) {
			var x = 270.0 + ((source.Longitude - 7.83445)/0.012185);
			var y = 360.0 + ((source.Latitude - 36.5964) / (source.Latitude > 36.5964 ? 0.010015 : 0.01002));
			return new Point2(x,y);
		}

		public bool HasInverse {
			get { return true; }
		}

		[NotNull] public IEnumerable<Point2> TransformValues([NotNull] IEnumerable<GeographicCoordinate> values) {
			return values.Select(TransformValue);
		}

		public ITransformation<Point2, GeographicCoordinate> GetInverse() {
			return new Inverse(this);
		}

		ITransformation ITransformation.GetInverse() {
			return GetInverse();
		}

	}
}
