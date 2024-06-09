using LibMesh.Data;
using LibUtil;

namespace LibMesh.Discrete
{
    internal class Geometry
    {
        private readonly WavefrontObject mMesh;
        private readonly Topology mTopology;

        internal Geometry(WavefrontObject mesh, Topology topology)
        {
            mMesh = mesh;
            mTopology = topology;
        }

        internal V3 FaceNormal(int fId)
        {
            var f = mMesh.F[fId];
            var v1 = mMesh.V[f[0].VId];
            var v2 = mMesh.V[f[1].VId];
            var v3 = mMesh.V[f[2].VId];
            return V3.Cross(v2 - v1, v3 - v1).Normalized;
        }

        internal float CircumcentricDualArea(int vId)
        {
            float totalarea = 0f;
            foreach (var fId in mTopology.AdjacentFaces(vId))
            {
                var (_, v2Id, v3Id) = mTopology.AdjacentVertices(fId, vId);

                var v1 = mMesh.V[vId];
                var v2 = mMesh.V[v2Id];
                var v3 = mMesh.V[v3Id];

                totalarea +=
                    (v3 - v1).LengthSquared * Cotan(v1 - v2, v3 - v2) + // Angle at v2 * length of e[v1, v3]
                    (v2 - v1).LengthSquared * Cotan(v1 - v3, v2 - v3); // Angle at v3 * length of e[v1, v2]
            }
            return 1f / 8f * totalarea;
        }

        internal float ScalarMeanCurvature(int vId)
        {
            var totalangle = 0f;
            foreach (var fId in mTopology.AdjacentFaces(vId))
            {
                var (_, v2Id, _) = mTopology.AdjacentVertices(fId, vId);
                totalangle += DihedralAngle(vId, v2Id) * (mMesh.V[v2Id] - mMesh.V[vId]).Length;
            }
            return 0.5f * totalangle;
        }

        internal float DihedralAngle(int v1Id, int v2Id)
        {
            // No fancy LINQ for once, because it is difficult to combine two loops nicely.
            int f1Id = -1, f2Id = -1;
            foreach (var fId in mTopology.AdjacentFaces(v1Id))
            {
                var f = mTopology.AdjacentVertices(fId, v1Id);

                if (f.v2Id == v2Id)
                {
                    f1Id = fId;
                }

                if (f.v3Id == v2Id)
                {
                    f2Id = fId;
                }
            }

            if (f1Id == -1 || f2Id == -1)
            {
                throw new Exception();
            }

            var n1 = FaceNormal(f1Id);
            var n2 = FaceNormal(f2Id);

            var v1 = mMesh.V[v1Id];
            var v2 = mMesh.V[v2Id];

            return V3.Dot((v2 - v1).Normalized, V3.Cross(n1, n2).Normalized) * SafeAcos(V3.Dot(n1, n2));
        }

        private static float SafeAcos(float v) =>
            (float)Math.Acos(Math.Min(1.0, v));

        internal float AngleDefect(int vId) =>
            2f * (float)Math.PI - mTopology.AdjacentFaces(vId).Sum(fId => Angle(mTopology.AdjacentVertices(fId, vId)));

        internal float TotalAngleDefect() =>
            Enumerable.Range(0, mMesh.V.Count).Sum(AngleDefect);

        internal float AngleCorner(int fId, int vId) =>
            Angle(mTopology.AdjacentVertices(fId, vId));

        private float Angle((int v1Id, int v2Id, int v3Id) _) =>
            Angle(mMesh.V[_.v1Id], mMesh.V[_.v2Id], mMesh.V[_.v3Id]);

        private static float Angle(V3 v1, V3 v2, V3 v3)
        {
            var e1 = v2 - v1;
            var e2 = v3 - v1;
            return (float)Math.Acos(V3.Dot(e1.Normalized, e2.Normalized));
        }

        // v1 and v2 are the edge to calculate the dual for. v3 is opposite.
        //private static float Cotan(V3 v1, V3 v2, V3 v3) =>
        //    Cotan(v1 - v3, v2 - v3);

        private static float Cotan(V3 e1, V3 e2) =>
            V3.Dot(e1, e2) / V3.Cross(e1, e2).Length;
    }
}
