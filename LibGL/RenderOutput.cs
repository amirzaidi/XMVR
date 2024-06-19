using LibGL.Buffers;
using LibUtil;
using OpenTK.Graphics.OpenGL4;

namespace LibGL
{
    public class RenderOutput : IDisposable
    {
        public readonly FrameBuffer FB;
        public readonly Texture[] Texs;
        public readonly RenderBuffer? Depth;

        public RenderOutput(RenderBuffer? depth, params Texture[] tex)
        {
            FB = new FrameBuffer();
            Texs = tex;
            Depth = depth;

            var offset = 0;
            for (var i = 0; i < tex.Length; i += 1)
            {
                offset += AttachAll(tex[i], offset);
                CheckFrameBuffer($"Bind Tex {i}");
            }

            if (depth != null)
            {
                FB.LinkRenderBuffer(depth);
                CheckFrameBuffer("Bind Depth");
            }
        }
        private int AttachAll(Texture tex, int offset = 0)
        {
            for (var i = 0; i < tex.Count; i += 1)
            {
                FB.LinkTexture(tex, FramebufferAttachment.ColorAttachment0 + offset + i, i);
                tex.BoundFB = (this, offset + i);
                //Log.Write($"{FB.Id} attach at {offset + i}");
            }

            return tex.Count;
        }

        private static void CheckFrameBuffer(string msg)
        {
            var err = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (err != FramebufferErrorCode.FramebufferComplete)
            {
                throw new Exception($"{msg}: {err}");
            }
        }

        public void Dispose()
        {
            FB.Dispose();
            Texs.ForEach(_ => _.Dispose());
            Depth?.Dispose();
        }
    }
}
