using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pigeoid.Contracts
{
	public interface INamedParameter
	{
		string Name { get; }
		object Value { get; }
	}

}
