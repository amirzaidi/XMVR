using OpenTK.Windowing.GraphicsLibraryFramework;

namespace LibGL
{
    public interface IRendererCallbacks
    {
        bool Init(Window window);

        bool Update(double dt, KeyboardState ks);

        bool Resize(int w, int h);

        bool Render();
    }
}
