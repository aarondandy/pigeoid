using System.Collections.Generic;

namespace Pigeoid.Contracts
{
	public interface INameNormalizedComparer : IComparer<string>, IEqualityComparer<string>
	{
		string Normalize(string text);
	}
}
