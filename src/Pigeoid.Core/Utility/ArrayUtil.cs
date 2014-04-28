using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace Pigeoid.Utility
{
    internal static class ArrayUtil
    {

        [Pure]
        public static bool Any<T>(this T[] source, Predicate<T> predicate) {
            Contract.Requires(source != null);
            Contract.Requires(predicate != null);
            Contract.Ensures(source.Length != 0 || !Contract.Result<bool>());
            for (int i = 0; i < source.Length; ++i) {
                if (predicate(source[i]))
                    return true;
            }
            return false;
        }

        [Pure]
        public static ReadOnlyCollection<T> AsReadOnly<T>(this T[] items) {
            Contract.Requires(items != null);
            Contract.Ensures(Contract.Result<ReadOnlyCollection<T>>().Count == items.Length);
            Contract.Ensures(Contract.ForAll(items.GetLowerBound(0), items.GetLowerBound(0) + items.Length, i => Contract.Result<ReadOnlyCollection<T>>()[i].Equals(items[i])));
            return new ReadOnlyCollection<T>(items);
        }

    }

    internal static class ArrayUtil<T>
    {

        public static readonly T[] Empty = new T[0];

    }

}
