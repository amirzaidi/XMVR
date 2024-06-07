using LibUtil;
using OpenTK.Graphics.OpenGL4;

namespace LibGL.Buffers
{
    public class VertexArrayObject : Bindable, IDisposable
    {
        public readonly int Id = GL.GenVertexArray();

        protected override Action BindInternal()
        {
            GL.BindVertexArray(Id);
            return () => GL.BindVertexArray(0);
        }

        public void Dispose() =>
            GL.DeleteVertexArray(Id);
    }
}
