using LibUtil;

namespace LibMesh
{
    internal class DiscreteTopology
    {
        // Must be a triangulated and normalized mesh.
        private readonly OBJParser.OBJData mMesh;
        private readonly List<int>[] mVertexFaceMap;

        internal IEnumerable<int> VIds => Enumerable.Range(0, mMesh.V.Count);
        internal IEnumerable<int> FIds => Enumerable.Range(0, mMesh.F.Count);

        internal DiscreteTopology(OBJParser.OBJData mesh)
        {
            mMesh = mesh;
            mVertexFaceMap = mMesh.V.Select(_ => new List<int>()).ToArray();

            for (var fId = 0; fId < mMesh.F.Count; fId += 1)
            {
                foreach (var (vId, _, _, _) in mMesh.F[fId])
                {
                    mVertexFaceMap[vId].Add(fId);
                }
            }
        }

        internal List<int> AdjacentFaces(int vId) =>
            mVertexFaceMap[vId];

        internal (int v1Id, int v2Id, int v3Id) AdjacentVertices(int fId) =>
            mMesh.F[fId].Select(_ => _.VId).ToArray().AsThreeTuple();

        internal (int v1Id, int v2Id, int v3Id) AdjacentVertices(int fId, int v1Id) =>
            ReorderToPivot(AdjacentVertices(fId), v1Id);

        internal static (int A, int B, int C) ReorderToPivot((int A, int B, int C) triplet, int newA)
        {
            if (triplet.A == newA)
            {
                return triplet;
            }

            if (triplet.B == newA)
            {
                return (triplet.B, triplet.C, triplet.A);
            }

            if (triplet.C == newA)
            {
                return (triplet.C, triplet.A, triplet.B);
            }

            throw new Exception();
        }

        internal static IEnumerable<(int, int)> HalfEdgesInFace(params int[] v)
        {
            for (var i = 0; i < v.Length - 1; i += 1)
            {
                yield return (v[i], v[i + 1]);
            }

            if (v.Length > 1)
            {
                yield return (v[^1], v[0]);
            }
        }
    }
}
