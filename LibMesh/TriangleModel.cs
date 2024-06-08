using System.Diagnostics;
using LibUtil;

namespace LibMesh
{
    [Serializable]
    public class TriangleModel
    {
        public enum AttributeType
        {
            Float, // Default
            Int,
            UInt,
        }

        public struct Attribute
        {
            public string ElementName;
            public int ElementCount;
            public AttributeType ElementType;
        }

        public static readonly Attribute[] ATTRS =
        [
            new() { ElementName = "VertexPosition", ElementCount = 3 },
            new() { ElementName = "VertexTexCoords", ElementCount = 2 },

            new() { ElementName = "VertexTangent", ElementCount = 3 },
            new() { ElementName = "VertexBitangent", ElementCount = 3 },
            new() { ElementName = "VertexNormal", ElementCount = 3 },

            new() { ElementName = "NormalS1", ElementCount = 3 },
            new() { ElementName = "NormalS2", ElementCount = 3 },
            new() { ElementName = "NormalS3", ElementCount = 3 },
            new() { ElementName = "NormalS4", ElementCount = 3 },
            new() { ElementName = "NormalS5", ElementCount = 3 },

            new() { ElementName = "VertexCurvature", ElementCount = 2 },
        ];

        public static readonly int ATTRS_TOTAL_COUNT = ATTRS.Sum(x => x.ElementCount);

        public static async Task<TriangleModel> Create(string dir, string filename)
        {
            var obj = await OBJParser.ParseFile(dir, filename);
            var p = new OBJProcessor(obj.Data);
            await p.Canonize();
            var isConnectedSurface = await p.CheckEdges();
            Log.Write($"{filename} has connected surface: {isConnectedSurface}");

            if (isConnectedSurface)
            {
                await p.CreateHigherLevel();
                Log.Write($"{filename} total angle defect: {p.TotalAngleDefect / Math.PI}π");

                var normals = await p.CalculateNormals();
                Log.Write($"{filename} calculated {normals} new normals");

                await p.CalculateCurvature();
                Log.Write($"{filename} calculated curvature");
            }

            var t = new TriangleModel(obj.MTLParser.Mats);
            await t.Load(p.Output);
            return t;
        }

        public Dictionary<string, MTLParser.Material> Mats = [];
        public (string?, int)[] MatUses = [];

        public float[] Vertices = [];
        public uint[] Indices = [];

        // For logging.
        [NonSerialized]
        public string? Name;

        private TriangleModel(Dictionary<string, MTLParser.Material> mats)
        {
            Mats = mats;
        }

        private async Task Load(OBJParser.OBJData obj)
        {
            MatUses = [.. obj.MatUses];

            var vertices = new List<float>();
            var indices = new List<uint>();

            int faceCurrVertexId;
            int faceNextVertexId = 0;
            foreach (var face in obj.F)
            {
                faceCurrVertexId = faceNextVertexId;
                faceNextVertexId += face.Length;

                // Calculate TBN.
                V3 tangent, bitangent, normal;

                V3 v1, v2, v3, e1, e2;
                v1 = obj.V[face[0].VId];
                v2 = obj.V[face[1].VId];
                v3 = obj.V[face[2].VId];
                e1 = v2 - v1;
                e2 = v3 - v1;

                // To-Do: Check if all of this is correct?
                if (face[0].VtId != -1 && face[1].VtId != -1 && face[2].VtId != -1)
                {
                    V2 uv1, uv2, uv3, duv1, duv2;
                    uv1 = new V2(obj.Vt[face[0].VtId]);
                    uv2 = new V2(obj.Vt[face[1].VtId]);
                    uv3 = new V2(obj.Vt[face[2].VtId]);
                    duv1 = uv2 - uv1;
                    duv2 = uv3 - uv1;

                    var ff = 1f / (duv1.X * duv2.Y - duv2.X * duv1.Y);
                    tangent = ff * (duv2.Y * e1 - duv1.Y * e2);
                    bitangent = ff * (-duv2.X * e1 + duv1.X * e2);
                    normal = V3.Cross(tangent, bitangent);
                }
                else
                {
                    // Take cross product twice for orthonormal reference frame.
                    normal = V3.Cross(e1, e2).Normalized;
                    tangent = e1.Normalized;
                    bitangent = V3.Cross(tangent, normal); // Check orientation?
                }

                // Add each vertex in the face.
                foreach (var (vId, vtId, vnId, khId) in face)
                {
                    AppendTuple(vertices, obj.V[vId].AsTuple); // Position
                    AppendTuple(vertices, vtId == -1 ? (0f, 0f) : obj.Vt[vtId]); // TexCoords

                    AppendTuple(vertices, tangent.AsTuple);
                    AppendTuple(vertices, bitangent.AsTuple);
                    AppendTuple(vertices, vnId == -1 ? normal.AsTuple : obj.Vn[vnId].AsTuple); // Geometry-based or manually specified.

                    // Downsampled Normals. Calculate these afterwards.
                    AppendTuple(vertices, (0f, 0f, 0f));
                    AppendTuple(vertices, (0f, 0f, 0f));
                    AppendTuple(vertices, (0f, 0f, 0f));
                    AppendTuple(vertices, (0f, 0f, 0f));
                    AppendTuple(vertices, (0f, 0f, 0f));

                    // Gaussian and mean curvature.
                    AppendTuple(vertices, khId == -1 ? (0f, 0f) : obj.KH[khId]);
                }

                // For each triangle in the face, add vertex indices.
                // We already triangulate, so usually this is one face.
                foreach (int i in Enumerable.Range(0, face.Length - 2))
                {
                    indices.Add((uint)faceCurrVertexId);
                    indices.Add((uint)(faceCurrVertexId + i + 1));
                    indices.Add((uint)(faceCurrVertexId + i + 2));
                }
            }

            Vertices = [.. vertices];
            Indices = [.. indices];

            await DownscaleNormals();
        }

        private async Task DownscaleNormals()
        {
            int vCount = Vertices.Length / ATTRS_TOTAL_COUNT;

            var sw = new Stopwatch();
            sw.Start();

            // Find duplicate vertices.
            var verticesOrdered = new (int Id, float X, float Y, float Z)[vCount];
            //var verticesOrderedData = new Vertex[faceNextVertexId];
            for (var i = 0; i < vCount; i += 1)
            {
                var index = i * ATTRS_TOTAL_COUNT;
                verticesOrdered[i] = (i, Vertices[index], Vertices[index + 1], Vertices[index + 2]);
                //verticesOrderedData[i] = new Vertex(vertices, i * ATTRS_TOTAL_COUNT);
            }

            // Sort because comparing everything would take O(n^2) time instead of O(n log n).
            // Takes about 800ms for 1M vertices.
            Array.Sort(verticesOrdered, (a, b) =>
            {
                if (a.X == b.X)
                {
                    if (a.Y == b.Y)
                    {
                        if (a.Z == b.Z)
                        {
                            return 0;
                        }

                        return a.Z > b.Z ? 1 : -1;
                    }

                    return a.Y > b.Y ? 1 : -1;
                }

                return a.X > b.X ? 1 : -1;
            });

            //var verticesOrderedIds = verticesOrdered.Select(v => v.Id).ToArray();
            // Save intermediate: vertices sorted list.

            Log.Write($"Normal Downscaling Time 1 ({verticesOrdered.Length}): {sw.ElapsedMilliseconds}ms");
            sw.Restart();

            // Indices are the same as the normal vertex indices.
            var matchingMap = new int[vCount];
            for (var i = 0; i < vCount; i += 1)
            {
                // Initially link each vertex to itself.
                matchingMap[i] = i;
            }

            // Find which vertices are the same by iterating through the sorted list.
            for (var i = 0; i < vCount - 1; i += 1)
            {
                var (v1Id, v1X, v1Y, v1Z) = verticesOrdered[i];
                var (v2Id, v2X, v2Y, v2Z) = verticesOrdered[i + 1];

                // If we match the previous vertex, link it to the original vertex that the previous vertex is linked to.
                //if (Vertex.EqualPosNorm(verticesOrderedData[v1], verticesOrderedData[v2]))
                if (v1X == v2X && v1Y == v2Y && v1Z == v2Z)
                {
                    matchingMap[v2Id] = matchingMap[v1Id];
                }
            }

            Log.Write($"Normal Downscaling Time 2: {sw.ElapsedMilliseconds}ms");
            sw.Restart();

            // Calculate downsampled normals.
            var neighbourMap = new List<int>[vCount];
            for (var i = 0; i < vCount; i += 1)
            {
                if (matchingMap[i] == i) // For each non-mapped vertex:
                {
                    neighbourMap[i] = [];
                }
            }

            // For each triangle in the mesh.
            for (var i = 0; i < Indices.Length; i += 3)
            {
                var vid0 = matchingMap[Indices[i + 0]];
                var vid1 = matchingMap[Indices[i + 1]];
                var vid2 = matchingMap[Indices[i + 2]];

                // Adjacent vertices in this triangle are neighbours.
                neighbourMap[vid0].Add(vid1);
                neighbourMap[vid0].Add(vid2);

                neighbourMap[vid1].Add(vid0);
                neighbourMap[vid1].Add(vid2);

                neighbourMap[vid2].Add(vid0);
                neighbourMap[vid2].Add(vid1);
            }

            var neighbourMapA = neighbourMap.Select(x => x?.ToArray()).ToArray();

            Log.Write($"Normal Downscaling Time 3: {sw.ElapsedMilliseconds}ms");
            sw.Restart();

            for (int i = 0; i < 5; i += 1)
            {
                await BlurNormals(matchingMap, neighbourMapA, i);
            }

            Log.Write($"Normal Downscaling Time 4: {sw.ElapsedMilliseconds}ms");
        }

        private async Task BlurNormals(int[] matchingMap, int[]?[] neighbourMap, int level)
        {
            var attrOffset = 11;
            var levelOffset = level * 3;
            var attrLevelOffset = attrOffset + levelOffset;

            var vertCount = Vertices.Length / ATTRS_TOTAL_COUNT;

            await Loop.For(0, vertCount, i =>
            {
                var vertOffset = i * ATTRS_TOTAL_COUNT; // Potentially mapped vertex.
                var norm = new V3(Vertices, vertOffset + attrLevelOffset); // Read the normal from the potentially mapped vertex.

                foreach (var j in neighbourMap[matchingMap[i]]!) // Switch to pivot vertex for only neighbour map, because neighbours only care about position.
                {
                    var vertOffset2 = j * ATTRS_TOTAL_COUNT; // Merge another normal from a potentially mapped vertex.
                    norm += new V3(Vertices, vertOffset2 + attrLevelOffset);
                }

                norm.Normalized.Write(Vertices, vertOffset + attrOffset + levelOffset + 3);
            });
        }

        private static void AppendTuple(List<float> list, (float, float) tuple) =>
            list.AddRange(new float[] { tuple.Item1, tuple.Item2 });

        private static void AppendTuple(List<float> list, (float, float, float) tuple) =>
            list.AddRange(new float[] { tuple.Item1, tuple.Item2, tuple.Item3 });
    }
}
