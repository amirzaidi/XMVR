using BasicApp;
using LibGL;
using LibMesh;
using LibMesh.Data;
using LibUtil;
using Nito.AsyncEx;

AsyncContext.Run(async () =>
{
    Log.Write("Hello, World!");

    var models = new List<RenderReadyModel>();
    using (var renderer = new Renderer(r => models.ForEach(_ => r.AddModel(_))))
    using (var window = new Window(renderer))
    {
        // Load models in async.
        // Can add more later.
        models.Add(await TriangleModelLoader.Create("Models", "cube.obj"));

        // Start rendering.
        window.SetVSync(true);
        window.Run();
    }

    Log.Write("Goodbye, World!");
});
