
using System.Collections.Generic;

namespace Pigeoid.Contracts
{
	public interface ICoordinateOperationInfo
	{

		/// <summary>
		/// The name of the operation.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// The operation parameters.
		/// </summary>
		IEnumerable<INamedParameter> Parameters { get; }

		/// <summary>
		/// Determines if this operation has an inverse.
		/// </summary>
		bool HasInverse { get; }

		/// <summary>
		/// Gets the inverse of this operation information if one exists.
		/// </summary>
		/// <returns>An operation.</returns>
		ICoordinateOperationInfo GetInverse();

	}
}
