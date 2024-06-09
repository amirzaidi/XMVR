namespace LibMesh.Data
{
    [Serializable]
    public struct WavefrontMaterial
    {
        public (float, float, float) Ka, Kd, Ks, Ke;
        public float Ns, Ni, D;
        public int Illum;
        public string TexDir;
        public string KaTex, KdTex, KsTex, VnTex;
    }
}
