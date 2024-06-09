namespace LibMesh.Data
{
    [Serializable]
    public readonly struct StandardizedModel
    {
        public readonly Dictionary<string, WavefrontMaterial> Mats = [];
        public readonly (string?, int)[] MatUses = [];
        public readonly float[] Vertices;
        public readonly uint[] Indices;

        internal StandardizedModel(Dictionary<string, WavefrontMaterial> mats, (string?, int)[] matUses, float[] vertices, uint[] indices)
        {
            Mats = mats;
            MatUses = matUses;
            Vertices = vertices;
            Indices = indices;
        }
    }
}
