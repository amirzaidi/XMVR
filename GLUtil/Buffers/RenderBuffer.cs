using LibUtil;
using OpenTK.Graphics.OpenGL4;

namespace LibGL.Buffers
{
    public class RenderBuffer : Bindable
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

        public AutoUnbind Bind()
        {
            GL.BindRenderbuffer(TARGET, Id);
            return new AutoUnbind(() => GL.BindRenderbuffer(TARGET, 0));
        }

        public override void Dispose() =>
            GL.DeleteRenderbuffer(Id);
    }
}
