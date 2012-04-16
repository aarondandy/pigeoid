using System.Collections.Generic;

namespace Pigeoid.Contracts
{
	/// <summary>
	/// A geodetic coordinate reference system.
	/// </summary>
	public interface ICrsGeodetic : ICrs
	{
		/// <summary>
		/// The datum this coordinate reference system is based on.
		/// </summary>
		IDatumGeodetic Datum { get; }

		/// <summary>
		/// The angular unit of measure for this CRS.
		/// </summary>
		IUom Unit { get; }

		/// <summary>
		/// Gets a collection of axes for this CRS.
		/// </summary>
		IEnumerable<IAxis> Axes { get; }
	}
}
