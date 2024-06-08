namespace LibUtil
{
    public class Loop
    {
        public const int PARALLELIZE = 16;

#pragma warning disable CS0162 // Unreachable code detected
        public static async Task For(int fromInclusive, int toExclusive, Action<int> action)
        {
            if (PARALLELIZE > 1)
            {
                var length = toExclusive - fromInclusive;
                var div = PARALLELIZE > length ? length : PARALLELIZE;

                var tasks = new Task[div];

                for (var i = 0; i < div; i += 1)
                {
                    var start = fromInclusive + length * i / div;
                    var end = fromInclusive + length * (i + 1) / div;

                    tasks[i] = Task.Run(() =>
                    {
                        for (var j = start; j < end; j += 1)
                        {
                            action(j);
                        }
                    });
                }

                foreach (var task in tasks)
                {
                    await task;
                }
            }
            else
            {
                for (var i = fromInclusive; i < toExclusive; i += 1)
                {
                    action(i);
                }
            }
        }
#pragma warning restore CS0162 // Unreachable code detected
    }
}
