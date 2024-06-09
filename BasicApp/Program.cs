using BasicApp;
using LibGL;
using LibMesh;
using LibMesh.Data;
using LibUtil;
using Nito.AsyncEx;

AsyncContext.Run(async () =>
{
    Log.Write("Hello, World!");

    var models = new List<StandardizedModel>();
    using (var renderer = new Renderer())
    using (var window = new Window(renderer))
    {
        // Load models in async.
        // Can add more later.
        renderer.AddModel(await TriangleModelLoader.Create("Models", "cube.obj"));

        // Start rendering.
        window.SetVSync(true);
        window.Run();
    }

    Log.Write("Goodbye, World!");
});
