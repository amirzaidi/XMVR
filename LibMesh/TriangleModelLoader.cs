using LibMesh.Data;
using LibMesh.Parser;
using LibUtil;

namespace LibMesh
{
    public class TriangleModelLoader
    {
        public static async Task<StandardizedModel> Create(string dir, string filename)
        {
            var obj = await WavefrontObjectParser.ParseFile(dir, filename);
            var p = new WavefrontObjProcessor(obj.Data);
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

            var objData = p.Output;
            var (vertices, indices) = await ObjToRenderMemory(objData);
            var t = new StandardizedModel(obj.MTLParser.Mats, [.. objData.MatUses], vertices, indices);
            return t;
        }

        internal static async Task<(float[], uint[])> ObjToRenderMemory(WavefrontObject objData)
        {
            var vertices = new List<float>();
            var indices = new List<uint>();

            int faceCurrVertexId;
            int faceNextVertexId = 0;
            foreach (var face in objData.F)
            {
                faceCurrVertexId = faceNextVertexId;
                faceNextVertexId += face.Length;

                // Calculate TBN.
                V3 tangent, bitangent, normal;

                V3 v1, v2, v3, e1, e2;
                v1 = objData.V[face[0].VId];
                v2 = objData.V[face[1].VId];
                v3 = objData.V[face[2].VId];
                e1 = v2 - v1;
                e2 = v3 - v1;

                // To-Do: Check if all of this is correct?
                if (face[0].VtId != -1 && face[1].VtId != -1 && face[2].VtId != -1)
                {
                    V2 uv1, uv2, uv3, duv1, duv2;
                    uv1 = objData.Vt[face[0].VtId];
                    uv2 = objData.Vt[face[1].VtId];
                    uv3 = objData.Vt[face[2].VtId];
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
                    vertices.Append(objData.V[vId]); // Position
                    vertices.Append(vtId == -1 ? V2.ZERO : objData.Vt[vtId]); // TexCoords

                    vertices.Append(tangent);
                    vertices.Append(bitangent);
                    vertices.Append(vnId == -1 ? normal : objData.Vn[vnId]); // Geometry-based or manually specified.

                    // Gaussian and mean curvature.
                    vertices.Append(khId == -1 ? V2.ZERO : objData.KH[khId]);
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

            return ([.. vertices], [.. indices]);
        }
    }
}
