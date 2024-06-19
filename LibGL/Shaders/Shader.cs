using LibUtil;
using OpenTK.Graphics.OpenGL4;

namespace LibGL.Shaders
{
    public class Shader : IDisposable
    {
        public readonly int Id;
        public readonly string Name;

        public Shader(ShaderType type, string path)
        {
            // Create shader object.
            Id = GL.CreateShader(type);
            Name = path.Split(['/', '\\']).Last();

            // Load source code into shader object.
            GL.ShaderSource(Id, File.ReadAllText(path));

            // Compile into binary code.
            GL.CompileShader(Id);

            // Check if compilation successful.
            GL.GetShader(Id, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                var m = $"{type} {path}:\r\n{GL.GetShaderInfoLog(Id)}";
                Log.Write(m);
                throw new Exception(m);
            }
        }

        public void Dispose() =>
            GL.DeleteShader(Id);
    }
}
