using LibMesh.Data;
using LibUtil;

namespace LibMesh.Parser
{
    // To-Do: Remove static instances and make it able to load/unload on demand.
    internal class WavefrontMaterialParser
    {
        private static readonly Parser<WavefrontMaterialParser> sLineParser = new();

        internal readonly Dictionary<string, WavefrontMaterial> Mats = [];

        private string? mName;
        private WavefrontMaterial mMat;

        private string? mTexDir;

        static WavefrontMaterialParser()
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

        internal WavefrontMaterialParser()
        {
        }

        internal async Task<WavefrontMaterialParser> ParseFile(string dir, string filename)
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
