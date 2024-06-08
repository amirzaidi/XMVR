namespace LibUtil
{
    public class DuplicateCompressor
    {
        public static async Task<(T[], int[])> Compress<T>(T[] array, Comparison<T> comparison)
        {
            // For each index in the array, which index contains the first occurence of the corresponding value in the sorted array.
            var mapArrayElementsToPivot = await MapDuplicates(array, comparison);

            // Now we want to move these pivots closer together in memory rather than leaving gaps.
            // The first variable is the list of pivot indices to retain.
            // The second variable is the map from the original indices in array to the new list of pivots only.
            var (pivotsInArray, mapCompressVPivots) = await CompressMap(array.Length, mapArrayElementsToPivot);

            // We extend the pivot compression list to the full list of all vertices.
            var mapCompressV = mapArrayElementsToPivot.Select(_ => mapCompressVPivots[_]).ToArray();

            return (pivotsInArray.Select(_ => array[_]).ToArray(), mapCompressV);
        }

        // First, merge v/vt/vn where possible, and keep a map of replacements.
        // For now we only map v to attempt creating a simplicial complex.
        public static async Task<int[]> MapDuplicates<T>(T[] array, Comparison<T> comparison)
        {
            var arrayOrdered = Enumerable.Range(0, array.Length).ToArray();
            // Array.Sort supplies the value and not the index, otherwise we need orderV[i|j] instead of i|j.
            Array.Sort(arrayOrdered, (i, j) => comparison(array[i], array[j]));

            var arrayMap = Enumerable.Range(0, array.Length).ToArray();
            for (var k = 1; k < arrayOrdered.Length; k += 1)
            {
                var i = arrayOrdered[k - 1];
                var j = arrayOrdered[k];

                if (Equals(array[i], array[j]))
                {
                    // By default, map[i] = i, but it may already reference another entry.
                    // We ensure no circular dependencies by going through orderV once from top to bottom.
                    // The top entry of a list of matching Vs becomes the canonical entry.
                    arrayMap[j] = arrayMap[i];
                }
            }

            return arrayMap;
        }

        // The indices of the previous map may have gaps, so we need to shift everything to fix this.
        // In other words, remove duplicate indices by putting everything in a set.
        public static async Task<(int[], int[])> CompressMap(int mapSize, int[] map)
        {
            var uniqueListOrdered = new SortedSet<int>(map).ToArray(); // Continuous list of all vertices to retain.
            Array.Sort(uniqueListOrdered); // Sort input vertices so we do not swap order, only filter out unused.
            var lookupTable = DenseIntMap.CreateFromInverse(mapSize, uniqueListOrdered); // How to map "vertices to retain" to continuous list.
            return (uniqueListOrdered, lookupTable);
        }
    }
}
