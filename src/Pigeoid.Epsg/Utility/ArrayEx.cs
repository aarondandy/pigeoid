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
            Contract.Ensures(Contract.ForAll(0, items.Length, i => Contract.Result<ReadOnlyCollection<T>>()[i].Equals(items[i])));
#if DEBUG
            var result = new ReadOnlyCollection<T>(items);
            Contract.Assume(result.Count == items.Length);
            Contract.Assume(Contract.ForAll(0, items.Length, i => result[i].Equals(items[i])));
            return result;
#else
            return new ReadOnlyCollection<T>(items);
#endif
        }

    }
}
