using LibGL.Buffers;

namespace LibGL
{
    internal class ViewBuffers : IDisposable
    {
        private readonly Texture mColor;
        private readonly RenderBuffer mDepth;

        public ViewBuffers(int w, int h)
        {
            mColor = new Texture(w, h, TextureFormats.Format.Half, 4);
            mDepth = new RenderBuffer(w, h);
        }

        public void Dispose()
        {
        }
    }
}
