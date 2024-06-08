using LibUtil;
using OpenTK.Graphics.OpenGL4;

namespace LibGL.Buffers
{
    public class FrameBuffer : Bindable<FramebufferTarget>
    {
        internal readonly int Id;

        protected override FramebufferTarget BindDefault => FramebufferTarget.Framebuffer;

        public FrameBuffer()
        {
            var ids = new int[1];
            GL.CreateFramebuffers(ids.Length, ids);
            Id = ids[0];
        }

        internal void LinkTexture(Texture? texture, FramebufferAttachment attachment = FramebufferAttachment.ColorAttachment0, int layer = 0)
        {
            if (texture == null)
            {
                GL.NamedFramebufferTexture(Id, attachment, 0, 0);
            }
            else if (texture.Count == 1)
            {
                GL.NamedFramebufferTexture(Id, attachment, texture.Id, 0);
            }
            else
            {
                GL.NamedFramebufferTextureLayer(Id, attachment, texture.Id, 0, layer);
            }
        }

        internal void LinkRenderBuffer(RenderBuffer buffer, FramebufferAttachment attachment = FramebufferAttachment.DepthAttachment)
        {
            using (buffer.Bind())
            {
                GL.NamedFramebufferRenderbuffer(Id, attachment, RenderbufferTarget.Renderbuffer, buffer.Id);
            }
        }

        protected override Action BindInternal(FramebufferTarget target)
        {
            GL.BindFramebuffer(target, Id);
            return () => GL.BindFramebuffer(target, 0);
        }

        public void Dispose() =>
            GL.DeleteFramebuffer(Id);
    }
}
