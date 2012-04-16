
namespace Pigeoid.Contracts
{
	/// <summary>
	/// A vertical coordinate reference system.
	/// </summary>
	public interface ICrsVertical : ICrs
	{
		/// <summary>
		/// The datum the CRS is based on.
		/// </summary>
		IDatum Datum { get; }

		/// <summary>
		/// The vertical unit of measure for this CRS.
		/// </summary>
		IUom Unit { get; }

		/// <summary>
		/// The axis for this CRS.
		/// </summary>
		IAxis Axis { get; }
	}
}
