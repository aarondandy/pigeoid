
using System.Collections.Generic;

namespace Pigeoid.Contracts
{
	public interface ICoordinateOperationInfo
	{

		string Name { get; }

		IEnumerable<INamedParameter> Parameters { get; }

	}
}
