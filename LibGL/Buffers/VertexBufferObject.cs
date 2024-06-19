using OpenTK.Graphics.OpenGL4;

namespace LibGL.Buffers
{
    public class VertexBufferObject : BufferObject
    {
        protected override BufferTarget BindDefault =>
            BufferTarget.ArrayBuffer;
    }
}
