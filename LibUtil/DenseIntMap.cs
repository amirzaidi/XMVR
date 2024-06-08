namespace LibUtil
{
    public class DenseIntMap
    {
        public static int[] CreateFromInverse(int mapSize, int[] reverseMap)
        {
            var map = new int[mapSize];
            Array.Fill(map, -1);
            for (var i = 0; i < reverseMap.Length; i += 1)
            {
                map[reverseMap[i]] = i;
            }
            return map;
        }
    }
}
