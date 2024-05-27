using OpenTK.Mathematics;

namespace LibGL
{
    internal class View
    {
        internal Matrix4 V, P;

        private ViewBuffers mBuffers;

        internal void SetResolution(int width, int height)
        {
            mBuffers?.Dispose();
            mBuffers = new ViewBuffers(width, height);
        }
    }
}
