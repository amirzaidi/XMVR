using VRDUtil;

namespace LibMesh
{
    public class MTLParser
    {
        [Serializable]
        public struct Material
        {
            public (float, float, float) Ka, Kd, Ks, Ke;
            public float Ns, Ni, D;
            public int Illum;
            public string TexDir;
            public string KaTex, KdTex, KsTex, VnTex;
        }

        private static readonly LineParser<MTLParser> sLineParser = new();

        internal readonly Dictionary<string, Material> Mats = new();

        private string? mName;
        private Material mMat = new();

        private string? mTexDir;

        static MTLParser()
        {
            sLineParser.AddHandler("newmtl", (obj, str) =>
            {
                obj.FinalizeMat();
                obj.mName = str[1];
            });

            sLineParser.AddHandler("Ka", (obj, str) => obj.mMat.Ka = ParseFloat3(str));
            sLineParser.AddHandler("Kd", (obj, str) => obj.mMat.Kd = ParseFloat3(str));
            sLineParser.AddHandler("Ks", (obj, str) => obj.mMat.Ks = ParseFloat3(str));
            sLineParser.AddHandler("Ke", (obj, str) => obj.mMat.Ke = ParseFloat3(str));

            sLineParser.AddHandler("Ns", (obj, str) => obj.mMat.Ns = str[1].AsFloat());
            sLineParser.AddHandler("Ni", (obj, str) => obj.mMat.Ni = str[1].AsFloat());
            sLineParser.AddHandler("d", (obj, str) => obj.mMat.D = str[1].AsFloat());
            sLineParser.AddHandler("illum", (obj, str) => obj.mMat.Illum = str[1].AsInt());

            sLineParser.AddHandler("map_Ka", (obj, str) => obj.mMat.KaTex = str[1]);
            sLineParser.AddHandler("map_Kd", (obj, str) => obj.mMat.KdTex = str[1]);
            sLineParser.AddHandler("map_Ks", (obj, str) => obj.mMat.KsTex = str[1]);
            sLineParser.AddHandler("map_vn", (obj, str) => obj.mMat.VnTex = str[1]);
        }

        internal MTLParser()
        {
        }

        internal async Task<MTLParser> ParseFile(string dir, string filename)
        {
            mTexDir = dir;
            FinalizeMat(); // Won't add a mat because name is empty.

            await sLineParser.ParseFile(this, dir, filename);
            FinalizeMat(); // Add the final mat.

            return this;
        }

        private void FinalizeMat()
        {
            if (mName != null)
            {
                Mats.Add(mName!, mMat);
                mName = null;
            }

            mMat = new()
            {
                TexDir = mTexDir!
            };
        }

        private static (float, float, float) ParseFloat3(string[] str) =>
            (str[1].AsFloat(), str[2].AsFloat(), str[3].AsFloat());
    }
}
