using LibUtil;
using OpenTK.Graphics.OpenGL4;

namespace LibGL.Buffers
{
    public class FrameBuffer : Bindable<FramebufferTarget>
    {
        public readonly struct Bounds(int x0, int y0, int x1, int y1)
        {
            public readonly int X0 = x0,
                Y0 = y0,
                X1 = x1,
                Y1 = y1;
        }

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


        public static void BlitIndex(FrameBuffer fb, int readBufferIndex, Bounds bounds) =>
            BlitIndex(fb, readBufferIndex, bounds, bounds);

        private static void BlitIndex(FrameBuffer fb, int readBufferIndex, Bounds inBounds, Bounds outBounds)
        {
            using (fb.Bind(FramebufferTarget.ReadFramebuffer))
            {
                SetReadBuffer(readBufferIndex);
                GL.BlitFramebuffer(
                    inBounds.X0, inBounds.Y0, inBounds.X1, inBounds.Y1,
                    outBounds.X0, outBounds.Y0, outBounds.X1, outBounds.Y1,
                    ClearBufferMask.ColorBufferBit,
                    BlitFramebufferFilter.Nearest
                );
                SetReadBuffer(0); // Reset to 0.
            }
        }

        private static void SetReadBuffer(int bufId) =>
            GL.ReadBuffer(ReadBufferMode.ColorAttachment0 + bufId);

        private static void SetDrawBuffers(params int[] bufIds) =>
            GL.DrawBuffers(bufIds.Length, bufIds.Select(i => DrawBuffersEnum.ColorAttachment0 + i).ToArray());
    }
}
