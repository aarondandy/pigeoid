using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Vertesaur;
using Vertesaur.Contracts;

namespace Pigeoid.Projection
{
	public class KrovakNorth : ITransformation<GeographicCoordinate, Point2>
	{

		protected readonly Krovak Core;

		public KrovakNorth(
			GeographicCoordinate geographicOrigin,
			double latitudeOfPseudoStandardParallel,
			double azimuthOfInitialLine,
			double scaleFactor,
			Vector2 falseProjectedOffset,
			[NotNull] ISpheroid<double> spheroid
		) : this(new Krovak(
			geographicOrigin,
			latitudeOfPseudoStandardParallel,
			azimuthOfInitialLine,
			scaleFactor,
			falseProjectedOffset,
			spheroid
		)) { }

		public KrovakNorth([NotNull] Krovak core)
		{
			if(null == core) throw new ArgumentNullException("core");
			Core = core;
		}

		internal class Inverse : Krovak.Inverse
		{
			public Inverse([NotNull] Krovak core) : base(core) { }

			public override GeographicCoordinate TransformValue(Point2 value)
			{
				return base.TransformValue(new Point2(-value.Y,-value.X));
			}
		}

		public ITransformation<Point2, GeographicCoordinate> GetInverse()
		{
			return new Inverse(Core);
		}

		public Point2 TransformValue(GeographicCoordinate source)
		{
			var p = Core.TransformValue(source);
			return new Point2(-p.Y, -p.X);
		}

		public string Name { get { return "Krovak North"; } }

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
