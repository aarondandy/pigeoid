using System.Diagnostics.Contracts;

namespace Pigeoid.Epsg.Utility
{
    internal static class ArrayUtil
    {

        [Pure]
        public static T[] ToArray<T>(this T[] array) {
            Contract.Requires(array != null);
            Contract.Ensures(Contract.Result<T[]>() != null);
            Contract.Ensures(Contract.Result<T[]>().Length == array.Length);
            return (T[])(array.Clone());
        }

        [Pure]
        public static T[] CreateSingleElementArray<T>(T item) {
            Contract.Ensures(Contract.Result<T[]>() != null);
            Contract.Ensures(Contract.Result<T[]>().Length == 1);
            Contract.Ensures(Contract.Result<T[]>()[0].Equals(item));
#if DEBUG
            var result = new[] { item };
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
