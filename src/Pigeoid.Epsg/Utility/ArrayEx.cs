using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace Pigeoid.Epsg.Utility
{
    internal static class ArrayEx
    {

        [Pure]
        public static ReadOnlyCollection<T> AsReadOnly<T>(this T[] items) {
            Contract.Requires(items != null);
            Contract.Ensures(Contract.Result<ReadOnlyCollection<T>>().Count == items.Length);
            Contract.Ensures(Contract.ForAll(items.GetLowerBound(0), items.GetLowerBound(0) + items.Length, i => Contract.Result<ReadOnlyCollection<T>>()[i].Equals(items[i])));
            return new ReadOnlyCollection<T>(items);
        }

    }
}
