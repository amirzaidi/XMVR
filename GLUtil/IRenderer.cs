using OpenTK.Windowing.GraphicsLibraryFramework;

namespace LibGL
{
    public interface IRenderer
    {
        bool Init(Window window);

        bool Resize(int w, int h);

        bool Update(double dt, KeyboardState ks);

        bool Render();
    }
}
