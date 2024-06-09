using LibUtil;
using OpenTK.Graphics.OpenGL4;

namespace LibGL.Buffers
{
    public class VertexArrayObject : Bindable, IDisposable
    {
        private static readonly VertexAttribPointerType[] sAttribTypeMap =
        [
            VertexAttribPointerType.Float,
            VertexAttribPointerType.Int,
            VertexAttribPointerType.UnsignedInt,
        ];

        public readonly int Id = GL.GenVertexArray();

        public void SetAttributes(VertexAttribute[] attrs, Func<string, int> map)
        {
            using (Bind())
            {
                var totalElementSize = attrs.Sum(x => x.ElementCount * GetElementSize(x.ElementType));
                var currElementSize = 0;

                foreach (var attr in attrs)
                {
                    var pos = map(attr.ElementName);
                    if (pos == -1)
                    {
                        // Skip binding if not found.
                        Log.Write($"SetAttributes {attr.ElementName} not found in map.");
                    }
                    else
                    {
                        // Enable and bind.
                        GL.EnableVertexAttribArray(map(attr.ElementName));
                        GL.VertexAttribPointer(
                            map(attr.ElementName),
                            attr.ElementCount,
                            sAttribTypeMap[(int)attr.ElementType],
                            false,
                            totalElementSize,
                            currElementSize
                        );
                    }
                    currElementSize += attr.ElementCount * GetElementSize(attr.ElementType);
                }
            }
        }

        private static int GetElementSize(VertexAttribute.Type type) =>
            type switch
            {
                VertexAttribute.Type.Float => sizeof(float),
                VertexAttribute.Type.UInt => sizeof(uint),
                VertexAttribute.Type.Int => sizeof(int),
                _ => throw new Exception(),
            };

        protected override Action BindInternal()
        {
            GL.BindVertexArray(Id);
            return () => GL.BindVertexArray(0);
        }

        public void Dispose() =>
            GL.DeleteVertexArray(Id);
    }
}
