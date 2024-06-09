using OpenTK.Graphics.OpenGL4;

namespace LibGL.Buffers
{
    public class ElementBufferObject : BufferObject
    {
        protected override BufferTarget BindDefault =>
            BufferTarget.ElementArrayBuffer;
    }
}
