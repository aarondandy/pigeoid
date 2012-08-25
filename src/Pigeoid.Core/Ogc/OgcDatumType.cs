// TODO: source header

namespace Pigeoid.Ogc
{
	/// <summary>
	/// Defines categories of datums. Integral values should match OGC values.
	/// </summary>
	public enum OgcDatumType : short
	{
		/// <summary>
		/// No datum type.
		/// </summary>
		None = 0,
		/// <summary>
		/// Generic horizontal datum.
		/// </summary>
		HorizontalOther = 1000,
		/// <summary>
		/// Classic 2D horizontal datum.
		/// </summary>
		HorizontalClassic = 1001,
		/// <summary>
		/// Satellite age datum.
		/// </summary>
		HorizontalGeocentric = 1002,
		/// <summary>
		/// Generic vertical datum.
		/// </summary>
		VerticalOther = 2000,
		/// <summary>
		/// Heights measured along a plumb line.
		/// </summary>
		VerticalOrthometric = 2001,
		/// <summary>
		/// Heights measured along a surface normal.
		/// </summary>
		VerticalEllipsoidal = 2002,
		/// <summary>
		/// Heights in the atmosphere.
		/// </summary>
		VerticalAltitudeBarometric = 2003,
		/// <summary>
		/// A normal height system.
		/// </summary>
		VerticalNormal = 2004,
		/// <summary>
		/// GPS height datum.
		/// </summary>
		VerticalGeoidModelDerived = 2005,
		/// <summary>
		/// Hydro-graphic depth datum.
		/// </summary>
		VerticalDepth = 2006,
		/// <summary>
		/// A local datum.
		/// </summary>
		LocalOther = 10000
	}
}
