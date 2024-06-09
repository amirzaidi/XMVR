using LibMesh.Data;
using LibUtil;

namespace LibMesh.Parser
{
    // To-Do: Remove static instances and make it able to load/unload on demand.
    internal class WavefrontObjectParser
    {
        private static readonly Parser<WavefrontObjectParser> sLineParser = new();

        static WavefrontObjectParser()
        {
            sLineParser.AddHandler("mtllib", async (obj, str) => await obj.MTLParser.ParseFile(obj.mDir, str[1]));
            sLineParser.AddHandler("usemtl", (obj, str) =>
            {
                obj.FinalizeMtlUsage();
                obj.CurrMatName = str[1];
            });

            sLineParser.AddHandler("o", (obj, str) => obj.SetObjectName(str[1]));
            sLineParser.AddHandler("g", (obj, str) => obj.SetObjectPartName(str[1]));

            sLineParser.AddHandler("v", (obj, str) => obj.Data.V.Add(new V3(str[1].AsFloat(), str[2].AsFloat(), str[3].AsFloat())));
            sLineParser.AddHandler("vp", (obj, str) => throw new Exception("vp not supported in obj file"));
            sLineParser.AddHandler("vt", (obj, str) => obj.Data.Vt.Add(new V2(str[1].AsFloat(), str[2].AsFloat())));
            sLineParser.AddHandler("vn", (obj, str) => obj.Data.Vn.Add(new V3(str[1].AsFloat(), str[2].AsFloat(), str[3].AsFloat())));

            sLineParser.AddHandler("f", (obj, str) =>
            {
                var faceIndices = str.Skip(1)
                    .Select(x => SplitFaceEntry(x))
                    .ToArray();
                obj.Data.F.Add(faceIndices);

                // Number of vertices, after triangulation of faces.
                // Three vertices per triangle, one additional triangle per vertex in the face.
                obj.CurrMatUses += 3 * (faceIndices.Length - 2);
            });
        }

        private static FaceEntry SplitFaceEntry(string entry)
        {
            var split = entry.Split('/');
            int v = 0, vt = 0, vn = 0;
            switch (split.Length)
            {
                case 3:
                    vn = split[2].AsInt(vn);
                    goto case 2;
                case 2:
                    vt = split[1].AsInt(vt);
                    goto case 1;
                case 1:
                    v = split[0].AsInt(v);
                    break;
                default:
                    throw new Exception();
            }

            // 1-indexed with 0 as "not found". Shift everything to 0-indexed with -1 as "not found".
            // Currently no way to add curvature from the file itself.
            return new()
            {
                VId = v - 1,
                VtId = vt - 1,
                VnId = vn - 1,
                KHId = -1,
            };
        }

        public static async Task<WavefrontObjectParser> ParseFile(string dir, string filename) =>
            await new WavefrontObjectParser(dir, filename).Load();

        public readonly WavefrontMaterialParser MTLParser = new();
        private readonly string mDir, mFilename;

        internal readonly WavefrontObject Data = new();

        // Counting mtl usage.
        private string? CurrMatName;
        private int CurrMatUses = 0;

        // To-Do: Remove or use (partial) polygon names?
        private string? mO, mG;

        private WavefrontObjectParser(string dir, string filename)
        {
            mDir = dir;
            mFilename = filename;
        }

        internal async Task<WavefrontObjectParser> Load()
        {
            await sLineParser.ParseFile(this, mDir, mFilename);
            FinalizeMtlUsage();
            return this;
        }

        private void SetObjectName(string name)
        {
            if (mO != null)
            {
                Log.Write($"Object name already set ({mO}, {name})");
            }

            mO = name;
        }

        private void SetObjectPartName(string name)
        {
            if (mG != null)
            {
                Log.Write($"Object part name already set ({mG}, {name})");
            }

            mG = name;
        }

        private void FinalizeMtlUsage()
        {
            if (CurrMatUses > 0)
            {
                Data.MatUses.Add((CurrMatName, CurrMatUses));
            }

            CurrMatName = string.Empty;
            CurrMatUses = 0;
        }
    }
}
