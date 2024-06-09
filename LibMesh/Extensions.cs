using LibMesh.Data;
using System.Globalization;

namespace LibMesh
{
    internal static class Extensions
    {
        internal static int AsInt(this string input) =>
            int.Parse(input);

        internal static int AsInt(this string input, int fallbackWhenEmpty) =>
            string.IsNullOrEmpty(input) ? fallbackWhenEmpty : int.Parse(input);

        internal static float AsFloat(this string input) =>
            float.Parse(input, CultureInfo.InvariantCulture.NumberFormat);

        internal static (T, T, T) AsThreeTuple<T>(this T[] input) =>
            (input[0], input[1], input[2]);

        internal static V3 Sum<T>(this IEnumerable<T> array, Func<T, V3> f) =>
            array.Select(_ => f(_)).Aggregate((a, b) => a + b);

        internal static void Append(this List<float> list, V2 v)
        {
            list.Add(v.X);
            list.Add(v.Y);
        }

        internal static void Append(this List<float> list, V3 v)
        {
            list.Add(v.X);
            list.Add(v.Y);
            list.Add(v.Z);
        }

        internal static IEnumerable<int> IndicesWhereTrue(this bool[] array) =>
            IndicesWhere(array, _ => _);

        internal static IEnumerable<int> IndicesWhere<T>(this T[] array, Func<T, bool> selector) =>
            Enumerable.Range(0, array.Length)
                .Where(_ => selector(array[_]))
                .ToArray();

        internal static IEnumerable<int> IndicesWhere<T>(this List<T> list, Func<T, bool> selector) =>
            Enumerable.Range(0, list.Count)
                .Where(_ => selector(list[_]))
                .ToArray();
    }
}
