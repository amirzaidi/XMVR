using LibMesh.Data;
using LibMesh.Discrete;
using LibUtil;

namespace LibMesh
{
    internal class WavefrontObjProcessor
    {
        private readonly WavefrontObject mInput, mOutput;
        private Topology? mTopology;
        private Geometry? mGeometry;
        private bool mCanonized;

        internal WavefrontObject Output => mOutput;

        internal float TotalAngleDefect => mGeometry!.TotalAngleDefect();

        internal WavefrontObjProcessor(WavefrontObject input)
        {
            mInput = input;
            mOutput = new();
        }

        // Compresses V, then triangulates F.
        internal async Task Canonize()
        {
            // Compress the set of vertices and get an index map from pre-compression to post-compression.
            var (setV, mapV) = await DuplicateCompressor.Compress(mInput.V.ToArray(), V3.Compare);
            Log.Write($"Reduced V from {mInput.V.Count} to {setV.Length}");

            // The new continous indices for each vertex are used to populate new V list.
            // Copy Vt, Vn and MatUses.
            mOutput.V.AddRange(setV);
            mOutput.Vt.AddRange(mInput.Vt);
            mOutput.Vn.AddRange(mInput.Vn);
            mOutput.MatUses.AddRange(mInput.MatUses);

            // Update all references in the vertices of faces using the map of replacements.
            mOutput.F.AddRange(mInput.F.Select(_ =>
                _.Select(_ => new FaceEntry(mapV[_.VId], _.VtId, _.VnId, _.KHId)).ToArray()
            ));

            // Then, triangulate all faces.
            // Check if the triangulation results in a simplicial complex.
            // Create traversal structure.
            var fOld = mOutput.F.ToArray();
            mOutput.F.Clear();
            mOutput.F.AddRange(fOld.Select(Triangulate).SelectMany(_ => _));
            Log.Write($"Triangulated F from {mInput.F.Count} to {mOutput.F.Count}");

            mCanonized = true;
        }

        private static IEnumerable<T[]> Triangulate<T>(T[] f) =>
            Enumerable.Range(0, f.Length - 2).Select(_ => new[] { f[0], f[_ + 1], f[_ + 2] });

        // Assumes already triangulated.
        internal async Task<bool> CheckEdges()
        {
            if (!mCanonized)
            {
                throw new Exception();
            }

            var halfEdges = new Dictionary<(int v1, int v2), int>();

            // The geometry is entirely encoded in F[_].VId.
            // We can do a check using only data from F to check if the mesh is a simplicial complex.
            for (var f = 0; f < mOutput.F.Count; f += 1)
            {
                // Each face also has an orientation, but each edge has to have one clockwise and one counterclockwise use in a face.
                // The reason for this is because of graphics pipelines rendering faces only in one direction.
                var fv = mOutput.F[f];

                var v1 = fv[0].VId;
                var v2 = fv[1].VId;
                var v3 = fv[2].VId;

                foreach (var e in Topology.HalfEdgesInFace(v1, v2, v3))
                {
                    if (!halfEdges.TryAdd(e, f))
                    {
                        Log.Write($"double halfedge {e.Item1},{e.Item2}");
                        return false;
                    }
                }
            }

            foreach (var (e, _) in halfEdges)
            {
                if (!halfEdges.ContainsKey((e.v2, e.v1)))
                {
                    Log.Write($"misses twin edge {e.v2},{e.v1}");
                    return false;
                }
            }

            return true;
        }

        internal async Task CreateHigherLevel()
        {
            mTopology = new Topology(mOutput);
            mGeometry = new Geometry(mOutput, mTopology);
        }

        internal async Task<int> CalculateNormals()
        {
            // Find which face-vertices need a vertex normal.
            var vertexNeedsNormal = new bool[mOutput.V.Count];
            mOutput.F.SelectMany(_ => _).ForEach(fv =>
            {
                // Manually implement this instead of using Any() for performance.
                vertexNeedsNormal[fv.VId] |= (fv.VnId == -1);
            });

            // Calculate normals for each face that has vertices with undefined normal.
            var faceNormalIds = mTopology!.FIds
                .Where(_ => mOutput.F[_].Any(_ => vertexNeedsNormal[_.VId]))
                .ToArray();
            var faceNormalIdsInv = DenseIntMap.CreateFromInverse(mOutput.F.Count, faceNormalIds);
            var faceNormals = faceNormalIds.Select(mGeometry!.FaceNormal).ToArray();

            // Map vertices to normals.
            var vertexNormalIds = vertexNeedsNormal.IndicesWhereTrue().ToArray();
            var vertexNormalIdsInv = DenseIntMap.CreateFromInverse(mOutput.V.Count, vertexNormalIds);
            var vertexNormals = vertexNormalIds.Select(vId =>
                mTopology!.AdjacentFaces(vId).Sum(fId =>
                    mGeometry!.AngleCorner(fId, vId) * faceNormals[faceNormalIdsInv[fId]]
                ).Normalized
            ).ToArray();

            // Add new set of normals to list.
            var offset = mOutput.Vn.Count;
            mOutput.Vn.AddRange(vertexNormals);

            // Replace missing normals with vertex normals.
            for (var fId = 0; fId < mOutput.F.Count; fId += 1)
            {
                for (var fvId = 0; fvId < mOutput.F[fId].Length; fvId += 1)
                {
                    var (vId, _, vnId, _) = mOutput.F[fId][fvId];
                    if (vnId == -1)
                    {
                        mOutput.F[fId][fvId].VnId = offset + vertexNormalIdsInv[vId];
                    }
                }
            }

            return vertexNormalIds.Length;
        }

        internal async Task CalculateCurvature()
        {
            mOutput.KH.AddRange(mTopology!.VIds.Select(vId =>
            {
                var angleDefect = mGeometry!.AngleDefect(vId);
                var meanCurvature = mGeometry!.ScalarMeanCurvature(vId);
                var dualArea = mGeometry!.CircumcentricDualArea(vId);

                float[] check = [angleDefect, meanCurvature, dualArea];
                if (check.Any(_ => float.IsNaN(_)))
                {
                    throw new Exception();
                }

                return new V2(angleDefect / dualArea, meanCurvature / dualArea);
            }));

            // Set khId = vId.
            for (var fId = 0; fId < mOutput.F.Count; fId += 1)
            {
                for (var fvId = 0; fvId < mOutput.F[fId].Length; fvId += 1)
                {
                    mOutput.F[fId][fvId].KHId = mOutput.F[fId][fvId].VId;
                }
            }
        }
    }
}
