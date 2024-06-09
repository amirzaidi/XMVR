using BasicApp;
using LibGL;
//using LibMesh;
using LibUtil;
using Nito.AsyncEx;

AsyncContext.Run(async () =>
{
    Log.Write("Hello, World!");

    //var model = await TriangleModelLoader.Create("Models", "boss.obj");

    using (var renderer = new Renderer())
    using (var window = new Window(renderer))
    {
        window.SetVSync(true);
        window.Run();
    }

    Log.Write("Goodbye, World!");
});
