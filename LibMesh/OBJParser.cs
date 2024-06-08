using LibUtil;

namespace LibMesh
{
    public class OBJParser
    {
        private static readonly LineParser<OBJParser> sLineParser = new();

        static OBJParser()
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
            sLineParser.AddHandler("vt", (obj, str) => obj.Data.Vt.Add((str[1].AsFloat(), str[2].AsFloat())));
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

        private static FV SplitFaceEntry(string entry)
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

        public static async Task<OBJParser> ParseFile(string dir, string filename) =>
            await new OBJParser(dir, filename).Load();

        public readonly MTLParser MTLParser = new();
        private readonly string mDir, mFilename;

        // Encapsulates all the relevant data to pass on.
        // This can be transformed into a canonical mesh representation.
        internal readonly struct OBJData
        {
            internal readonly List<V3> V = [];
            internal readonly List<(float U, float V)> Vt = [];
            internal readonly List<V3> Vn = [];
            internal readonly List<(float K, float H)> KH = [];
            internal readonly List<FV[]> F = [];
            internal readonly List<(string?, int)> MatUses = [];

            public OBJData()
            {
            }
        }

        internal struct FV
        {
            internal int VId, VtId, VnId, KHId;

            internal FV(int vId, int vtId, int vnId, int khId)
            {
                VId = vId;
                VtId = vtId;
                VnId = vnId;
                KHId = khId;
            }

            internal readonly void Deconstruct(out int vId, out int vtId, out int vnId, out int khId)
            {
                vId = VId;
                vtId = VtId;
                vnId = VnId;
                khId = KHId;
            }
        }

        internal readonly OBJData Data = new();

        // Counting mtl usage.
        private string? CurrMatName;
        private int CurrMatUses = 0;

        // To-Do: Remove or use (partial) polygon names?
        private string? mO, mG;

        private OBJParser(string dir, string filename)
        {
            mDir = dir;
            mFilename = filename;
        }

        internal async Task<OBJParser> Load()
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
