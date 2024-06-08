using LibUtil;
using OpenTK.Graphics.OpenGL4;
using System.Runtime.InteropServices;

namespace LibGL.Buffers
{
    public class VertexBufferObject : Bindable<BufferTarget>, IDisposable
    {
        public readonly int Id = GL.GenBuffer();

        protected override BufferTarget BindDefault => BufferTarget.ArrayBuffer;

        protected override Action BindInternal(BufferTarget target)
        {
            GL.BindBuffer(target, Id);
            return () => GL.BindBuffer(target, 0);
        }

        public void BindBufferData<T>(T[] data, BufferUsageHint hint = BufferUsageHint.StaticDraw)
            where T : struct
        {
            using (Bind())
            {
                GL.BufferData(BindDefault, data.Length * Marshal.SizeOf(data[0]), data, hint);
            }
        }

        public void Dispose() =>
            GL.DeleteBuffer(Id);
    }
}
