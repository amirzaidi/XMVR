using LibUtil;
using OpenTK.Compute.OpenCL;
using OpenTK.Graphics.OpenGL4;

namespace LibGL.Shaders
{
    public class ShaderProgram : Bindable, IDisposable
    {
        public readonly int Id;
        public readonly string Name;

        private readonly Dictionary<string, int> mLocations = [];

        public ShaderProgram(params Shader[] shaders)
        {
            // Create clean program.
            Id = GL.CreateProgram();
            Name = string.Join(" -> ", shaders.Select(_ => _.Name).ToArray());

            // Compile into full rasterization pipeline.
            shaders.ForEach(_ => GL.AttachShader(Id, _.Id));
            GL.LinkProgram(Id);

            // Check if compilation successful.
            GL.GetProgram(Id, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                var m = $"Program:\r\n{GL.GetProgramInfoLog(Id)}";
                Log.Write(m);
                throw new Exception(m);
            }

            // Clean up partial shaders.
            shaders.ForEach(_ => GL.DetachShader(Id, _.Id));
        }

        public int GetUniformLocation(string var) =>
            mLocations.GetOrAdd(var, () =>
            {
                var loc = GL.GetUniformLocation(Id, var);
                if (loc == -1)
                {
                    Log.Write($"{Name} GetUniformLoc: {var} {loc}");
                }
                return loc;
            });

        protected override Action BindInternal()
        {
            GL.UseProgram(Id);
            return () => GL.UseProgram(0);
        }

        public void Dispose() =>
            GL.DeleteProgram(Id);
    }
}
