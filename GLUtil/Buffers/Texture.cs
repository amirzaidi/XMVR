using LibUtil;
using OpenTK.Graphics.OpenGL4;
using static LibGL.Buffers.TextureFormats;

namespace LibGL.Buffers
{
    public class Texture : Bindable
    {
        private const TextureUnit TEX_DEFAULT = TextureUnit.Texture0;
        private const int TEMP_TEX_USE_ID = 16;

        public readonly int Id = GL.GenTexture();
        public readonly int Width, Height, Channels, Count;
        public readonly Format Format;
        public readonly TextureTarget Target;

        public IntPtr NativePtr =>
            new(Id);

        public Texture(int w, int h, Format format = Format.Half, int ch = 1, int n = 1)
        {
            Width = w;
            Height = h;
            Format = format;
            Channels = ch;
            Count = n;

            if (n < 1)
            {
                throw new ArgumentException("Count must be larger than 0.");
            }

            Target = n == 1
                ? TextureTarget.Texture2D
                : TextureTarget.Texture2DArray;

            var (pif, pf, pt) = FormatToPixel(format, ch);

            using (Bind())
            {
                if (Count > 1)
                {
                    // Create multiple 2D textures in array.
                    GL.TexImage3D(
                        Target,
                        0,
                        pif,
                        w, h, n,
                        0,
                        pf,
                        pt,
                        IntPtr.Zero
                    );
                }
                else
                {
                    // Crete single 2D texture.
                    GL.TexImage2D(
                        Target,
                        0,
                        pif,
                        w, h,
                        0,
                        pf,
                        pt,
                        IntPtr.Zero
                    );
                }

                // Disable any texture filtering from sampling by default.
                GL.TexParameter(Target, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                GL.TexParameter(Target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

                // Use edge pixel when going over edge.
                GL.TexParameter(Target, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(Target, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            }
        }

        public AutoUnbind Bind(int tex = TEMP_TEX_USE_ID) =>
            Bind(TEX_DEFAULT + tex);

        public AutoUnbind Bind(TextureUnit unit)
        {
            // Bind this texture to texture unit.
            GL.ActiveTexture(unit);
            GL.BindTexture(Target, Id);

            return new AutoUnbind(() =>
            {
                // Unbind this texture from texture unit.
                GL.ActiveTexture(unit);
                GL.BindTexture(Target, 0);

                // Reset to default texture unit.
                GL.ActiveTexture(TEX_DEFAULT);
            });
        }

        public override void Dispose() =>
            GL.DeleteTexture(Id);
    }
}
