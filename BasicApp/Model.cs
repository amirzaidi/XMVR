using LibGL.Buffers;
using LibGL.Shaders;
using LibMesh.Data;
using LibUtil;

namespace BasicApp
{
    internal class Model : Bindable, IDisposable
    {
        internal readonly VertexBufferObject VBO;
        internal readonly ElementBufferObject EBO;
        internal readonly VertexArrayObject VAO;

        internal Model(ShaderProgram program, StandardizedModel model)
        {
            // The VBO stores the triangle vertices.
            VBO = new VertexBufferObject();
            VBO.BindBufferData(model.Vertices);

            // The EBO stores the triangle indices.
            EBO = new ElementBufferObject();
            EBO.BindBufferData(model.Indices);

            // The VAO stores how to render those vertices/indices.
            VAO = new VertexArrayObject();

            // OpenGL needs the raw data to validate the indexing used in the next step.
            VAO.SetVertexBufferObject(VBO);

            // Set up the memory layout.
            // This will be saved for the next time the VAO is bound.
            VAO.SetAttributes(VertexAttribute.DEFAULT, program!.GetAttribLocation);
        }

        protected override Action BindInternal() =>
            Chain(VBO.Bind(), EBO.Bind());

        public void Dispose()
        {
            VBO.Dispose();
            EBO.Dispose();
            VAO.Dispose();
        }
    }
}
