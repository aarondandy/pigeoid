using System;
using System.Collections.Generic;
using System.Linq;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Projection
{
	public class KrovakModifiedNorth : ITransformation<GeographicCoordinate, Point2>
	{

		protected readonly KrovakModified Core;

		public KrovakModifiedNorth(
			GeographicCoordinate geographicOrigin,
			double latitudeOfPseudoStandardParallel,
			double azimuthOfInitialLine,
			double scaleFactor,
			Vector2 falseProjectedOffset,
			ISpheroid<double> spheroid,
			Point2 evaluationPoint,
			double[] constants
		)
			: this(new KrovakModified(
			geographicOrigin,
			latitudeOfPseudoStandardParallel,
			azimuthOfInitialLine,
			scaleFactor,
			falseProjectedOffset,
			spheroid,
			evaluationPoint,
			constants
		)) { }

		public KrovakModifiedNorth(KrovakModified core)
		{
			if(null == core) throw new ArgumentNullException("core");
			Core = core;
		}

		internal class Inverse : KrovakModified.Inverse
		{
			public Inverse(KrovakModified core) : base(core) { }

			public override GeographicCoordinate TransformValue(Point2 value)
			{
				return base.TransformValue(new Point2(-value.Y, -value.X));
			}
		}

		public ITransformation<Point2, GeographicCoordinate> GetInverse()
		{
			return new KrovakModifiedNorth.Inverse(Core);
		}

		public Point2 TransformValue(GeographicCoordinate source)
		{
			var p = Core.TransformValue(source);
			return new Point2(-p.Y, -p.X);
		}

		public string Name { get { return "Krovak Modified North"; } }

		public IEnumerable<Point2> TransformValues(IEnumerable<GeographicCoordinate> values)
		{
			return values.Select(TransformValue);
		}

		ITransformation ITransformation.GetInverse()
		{
			return GetInverse();
		}

		public bool HasInverse
		{
			get { return Core.HasInverse; }
		}

	}
}
