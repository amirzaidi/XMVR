using BasicApp;
using LibGL;
using LibUtil;

/**
 * Start of this amazing app.
 */

Log.Write("Hello, World!");

using (var renderer = new Renderer())
using (var window = new Window(renderer))
{
    window.Run();
}

Log.Write("Goodbye, World!");
