using LibUtil;
using OpenTK.Graphics.OpenGL4;

namespace LibGL.Shaders
{
    public class ShaderProgram : Bindable, IDisposable
    {
        public readonly int Id;
        public readonly string Name;

        private readonly Dictionary<string, int> mUniformCache = [];
        private readonly Dictionary<string, int> mAttribCache = [];

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
                var m = $"{Name}:\r\n{GL.GetProgramInfoLog(Id)}";
                Log.Write(m);
                throw new Exception(m);
            }

            // Clean up partial shaders.
            shaders.ForEach(_ => GL.DetachShader(Id, _.Id));
        }

        public void Validate()
        {
            GL.ValidateProgram(Id);
            GL.GetProgram(Id, GetProgramParameterName.ValidateStatus, out int success);
            if (success == 0)
            {
                var m = $"{Name} Invalid State:\r\n{GL.GetProgramInfoLog(Id)}";
                Log.Write(m);
                throw new Exception(m);
            }
        }

        public int GetAttribLocation(string var) =>
            GetCachedLocation(var, mAttribCache, GL.GetAttribLocation);

        public int GetUniformLocation(string var) =>
            GetCachedLocation(var, mUniformCache, GL.GetUniformLocation);

        private int GetCachedLocation(string var, Dictionary<string, int> map, Func<int, string, int> f) =>
            map.GetOrAdd(var, () =>
            {
                var location = f(Id, var);
                if (location == -1)
                {
                    Log.Write($"{Name}: Failed to find {var}");
                }

                return location;
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
