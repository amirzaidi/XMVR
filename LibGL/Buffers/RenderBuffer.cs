using LibUtil;
using OpenTK.Graphics.OpenGL4;

namespace LibGL.Buffers
{
    public class RenderBuffer : Bindable, IDisposable
    {
        private static readonly RenderbufferTarget TARGET = RenderbufferTarget.Renderbuffer;

        public readonly int Id = GL.GenRenderbuffer();

        public RenderBuffer(int w, int h)
        {
            using (Bind())
            {
                GL.RenderbufferStorage(TARGET, RenderbufferStorage.DepthComponent32f, w, h);
            }
        }

        protected override Action BindInternal()
        {
            GL.BindRenderbuffer(TARGET, Id);
            return () => GL.BindRenderbuffer(TARGET, 0);
        }

        public void Dispose() =>
            GL.DeleteRenderbuffer(Id);
    }
}
