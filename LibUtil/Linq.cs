using System.Globalization;

namespace LibUtil
{
    public static class Linq
    {
        public static TOut Apply<TIn, TOut>(this TIn input, Func<TIn, TOut> output) =>
            output(input);

        public static void ForEach<TIn>(this IEnumerable<TIn> input, Action<TIn> output)
        {
            foreach (var inputVal in input)
            {
                output(inputVal);
            }
        }

        public static int AsInt(this string input) =>
            int.Parse(input);

        public static int AsInt(this string input, int fallbackWhenEmpty) =>
            string.IsNullOrEmpty(input) ? fallbackWhenEmpty : int.Parse(input);

        public static float AsFloat(this string input) =>
            float.Parse(input, CultureInfo.InvariantCulture.NumberFormat);

        public static (T, T, T) AsThreeTuple<T>(this T[] input) =>
            (input[0], input[1], input[2]);

        public static string[] SplitNotEmpty(this string input, params char[] separator) =>
            input.Split(separator, StringSplitOptions.RemoveEmptyEntries);

        /// <summary>
        /// Select elements from an array based on the first index and length.
        /// </summary>
        public static IEnumerable<T> SubArray<T>(this IEnumerable<T> input, int start, int length) =>
            input.Skip(start).Take(length);

        /// <summary>
        /// Select elements from an array from a list of indices.
        /// </summary>
        public static IEnumerable<T> SubArray<T>(this List<T> input, IEnumerable<uint> indices) =>
            indices.Select(i => input[(int)i]);

        public static void Add<T>(this List<T> input, params T[] values) =>
            input.AddRange(values);

        /// <summary>
        /// Not true LINQ, as it stores Tasks in an array.
        /// </summary>
        public static async Task<TOut[]> SelectAsync<TIn, TOut>(this IEnumerable<TIn> input, Func<TIn, Task<TOut>> f)
        {
            var awaiters = input.Select(x => f(x)).ToArray(); // ToArray to enforce all to start.
            var results = new TOut[awaiters.Length];

            // Await all tasks.
            for (var i = 0; i < awaiters.Length; i++)
            {
                results[i] = await awaiters[i];
            }

            return [.. results];
        }

        /// <summary>
        /// Not true LINQ, as it stores results in an array.
        /// </summary>
        public static async Task<List<TOut>> SelectSeqAsync<TIn, TOut>(this IEnumerable<TIn> input, Func<TIn, Task<TOut>> f)
        {
            var results = new List<TOut>();
            foreach (var inputVal in input)
            {
                results.Add(await f(inputVal));
            }
            return results;
        }

        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TValue> creator) where TKey : notnull =>
            dict.GetOrAdd(key, _ => creator());

        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TKey, TValue> creator) where TKey : notnull
        {
            if (dict.TryGetValue(key, out TValue? v))
            {
                return v;
            }

            var value = creator(key);
            dict.Add(key, value);
            return value;
        }

        public static int FindIndexOrAdd<T>(this List<T> list, T key)
        {
            if (!list.Contains(key))
            {
                list.Add(key);
            }

            return list.FindIndex(_ => Equals(_, key));
        }

        public static void IfTrue(this bool condition, Action action)
        {
            if (condition)
            {
                action();
            }
        }

        public static void IfNull(this object? obj, Action action) =>
            (obj == null).IfTrue(action);

        public static void ThrowIfNull(this object? obj)
            => obj.IfNull(() => throw new NullReferenceException());

        public static T[] Fill<T>(this T[] array, Action<int, T[]> fill)
        {
            fill(array.Length, array);
            return array;
        }

        public static T[] ExecuteAll<T>(this IEnumerable<Func<T>> array) =>
            array.Select(_ => _()).ToArray();

        public static IEnumerable<int> IndicesWhereTrue(this bool[] array) =>
            IndicesWhere(array, _ => _);

        public static IEnumerable<int> IndicesWhere<T>(this T[] array, Func<T, bool> selector) =>
            Enumerable.Range(0, array.Length)
                .Where(_ => selector(array[_]))
                .ToArray();

        public static IEnumerable<int> IndicesWhere<T>(this List<T> list, Func<T, bool> selector) =>
            Enumerable.Range(0, list.Count)
                .Where(_ => selector(list[_]))
                .ToArray();
    }
}
