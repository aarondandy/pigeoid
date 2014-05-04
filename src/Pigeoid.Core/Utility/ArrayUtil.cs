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

        [Pure]
        public static T[] CreateSingleElementArray<T>(T item) {
            Contract.Ensures(Contract.Result<T[]>() != null);
            Contract.Ensures(Contract.Result<T[]>().Length == 1);
            Contract.Ensures(Contract.Result<T[]>()[0].Equals(item));
#if DEBUG
            var result = new[] {item};
            Contract.Assume(result.Length == 1);
            Contract.Assume(result[0].Equals(item));
            return result;
#else
            return new[] {item};
#endif
        }

    }

    internal static class ArrayUtil<T>
    {

        private static readonly T[] _empty = new T[0];

        public static T[] Empty {
            get {
                Contract.Ensures(Contract.Result<T[]>() != null);
                Contract.Ensures(Contract.Result<T[]>().Length == 0);
                Contract.Assume(_empty.Length == 0);
                return _empty;
            }
        }

    }

}
