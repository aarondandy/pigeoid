using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pigeoid.Contracts
{
	public interface ICoordinateOperationInfo
	{

		string Name { get; }

		IEnumerable<INamedParameter> Parameters { get; }

	}
}
