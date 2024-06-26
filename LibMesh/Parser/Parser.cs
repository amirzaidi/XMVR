﻿using LibUtil;

namespace LibMesh.Parser
{
    // To-Do: Remove static instances and make it able to load/unload on demand.
    internal class Parser<T>
    {
        private readonly Dictionary<string, Func<T, string[], Task>> mActions = [];

        internal void AddHandler(string cmd, Func<T, string[], Task> action) =>
            mActions.Add(cmd, action);

        internal void AddHandler(string cmd, Action<T, string[]> action) =>
            AddHandler(cmd, async (obj, str) => action(obj, str));

        internal async Task ParseFile(T obj, string dir, string filename)
        {
            var file = Path.Combine(dir, filename);
            Assert.FileExists(file);

            using var fileStream = File.OpenText(file);
            string? line;
            while ((line = await fileStream.ReadLineAsync()) != null)
            {
                var split = line.SplitNotEmpty(' ');
                if (split.Length == 0)
                {
                    continue;
                }
                await ParseLine(obj, split);
            }
        }

        internal async Task ParseLine(T obj, string[] split)
        {
            var cmd = split[0];
            if (mActions.TryGetValue(cmd, out Func<T, string[], Task>? v))
            {
                await v(obj, split);
            }
        }
    }
}
