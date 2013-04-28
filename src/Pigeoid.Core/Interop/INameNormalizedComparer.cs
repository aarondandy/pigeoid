using System.Collections.Generic;

namespace Pigeoid.Interop
{
    public interface INameNormalizedComparer : IComparer<string>, IEqualityComparer<string>
    {
        string Normalize(string text);
    }
}
