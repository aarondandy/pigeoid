using System.Collections.Generic;

namespace Pigeoid.Contracts
{
	public interface IParameterizedCoordinateOperationInfo : ICoordinateOperationInfo
	{

		/// <summary>
		/// The operation parameters.
		/// </summary>
		IEnumerable<INamedParameter> Parameters { get; }

		ICoordinateOperationMethodInfo Method { get; }

	}
}
