using LibUtil;
using OpenTK.Graphics.OpenGL4;

namespace LibGL.Buffers
{
    public class VertexBufferObject : Bindable, IDisposable
    {
        public readonly int Id = GL.GenBuffer();

        protected override Action BindInternal()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, Id);
            return () => GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public void Dispose() =>
            GL.DeleteBuffer(Id);
    }
}
