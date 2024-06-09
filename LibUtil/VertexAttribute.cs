namespace LibUtil
{
    // To-Do: Make this nicely bound in code, instead of hardcoded.
    public struct VertexAttribute
    {
        public static readonly VertexAttribute[] DEFAULT =
        [
            new() { ElementName = "VertexPosition", ElementCount = 3 },
            new() { ElementName = "VertexTexCoords", ElementCount = 2 },

            new() { ElementName = "VertexTangent", ElementCount = 3 },
            new() { ElementName = "VertexBitangent", ElementCount = 3 },
            new() { ElementName = "VertexNormal", ElementCount = 3 },
            new() { ElementName = "VertexCurvature", ElementCount = 2 },
        ];

        public static readonly int DEFAULT_TOTAL_COUNT = DEFAULT.Sum(x => x.ElementCount);

        public enum Type
        {
            Float, // Default
            Int,
            UInt,
        }

        public string ElementName;
        public int ElementCount;
        public Type ElementType;
    }
}
