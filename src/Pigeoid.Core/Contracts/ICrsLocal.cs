// TODO: source header

using System.Collections.Generic;

namespace Pigeoid.Contracts
{
	/// <summary>
	/// A local coordinate reference system.
	/// </summary>
	public interface ICrsLocal : ICrs
	{
		/// <summary>
		/// The datum the coordinate reference system is based on.
		/// </summary>
		IDatum Datum { get; }

		/// <summary>
		/// The unit of measure used by this CRS.
		/// </summary>
		IUnit Unit { get; }

		/// <summary>
		/// The axes for the projection.
		/// </summary>
		IList<IAxis> Axes { get; }

	}
}
