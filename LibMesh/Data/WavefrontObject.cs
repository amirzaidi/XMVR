using LibUtil;

namespace LibMesh.Data
{
    internal readonly struct WavefrontObject
    {
        internal readonly List<V3> V = [];
        internal readonly List<V2> Vt = [];
        internal readonly List<V3> Vn = [];
        internal readonly List<V2> KH = [];
        internal readonly List<FaceEntry[]> F = [];
        internal readonly List<(string?, int)> MatUses = [];

        public WavefrontObject()
        {
        }
    }
}
