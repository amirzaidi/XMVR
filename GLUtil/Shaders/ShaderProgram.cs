using LibUtil;
using OpenTK.Graphics.OpenGL4;
using static LibGL.Bindable;

namespace LibGL.Shaders
{
    public class ShaderProgram : IDisposable
    {
        public readonly int Id;

        public ShaderProgram(Shader vert, Shader frag)
        {
            // Create clean program.
            Id = GL.CreateProgram();

            // Compile into full rasterization pipeline.
            GL.AttachShader(Id, vert.Id);
            GL.AttachShader(Id, frag.Id);
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
            GL.DetachShader(Id, vert.Id);
            GL.DetachShader(Id, frag.Id);
        }

        public AutoUnbind Bind()
        {
            GL.UseProgram(Id);
            return new AutoUnbind(() => GL.UseProgram(0));
        }

        public void Dispose() =>
            GL.DeleteProgram(Id);
    }
}
