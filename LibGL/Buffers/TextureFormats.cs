using OpenTK.Graphics.OpenGL4;

namespace LibGL.Buffers
{
    public class TextureFormats
    {
        public enum Format
        {
            Half, // 16
            Float, // 32
            Depth, // 32

            Byte, // 8
            Short, // 16
            Int, // 32

            UByte, // 8
            UShort, // 16
            UInt, // 32
        }

        // Last two tuple entries are only used to load the data into TexImage2D, but we don't use that.
        // The important value is the PixelInternalFormat.
        internal static (PixelInternalFormat, PixelFormat, PixelType) FormatToPixel(Format format, int ch) =>
            (FormatToPixelPIF(format, ch), FormatToPixelPF(format, ch), FormatToPixelPT(format));

        private static PixelInternalFormat FormatToPixelPIF(Format format, int ch) =>
            (format, ch) switch
            {
                (Format.Half, 1) => PixelInternalFormat.R16f,
                (Format.Half, 2) => PixelInternalFormat.Rg16f,
                (Format.Half, 3) => PixelInternalFormat.Rgb16f,
                (Format.Half, 4) => PixelInternalFormat.Rgba16f,

                (Format.Float, 1) => PixelInternalFormat.R32f,
                (Format.Float, 2) => PixelInternalFormat.Rg32f,
                (Format.Float, 3) => PixelInternalFormat.Rgb32f,
                (Format.Float, 4) => PixelInternalFormat.Rgba32f,

                (Format.Depth, 1) => PixelInternalFormat.DepthComponent32f,
                (Format.Depth, _) => throw new NotSupportedException(),

                (Format.Short, 1) => PixelInternalFormat.R16i,
                (Format.Short, 2) => PixelInternalFormat.Rg16i,
                (Format.Short, 3) => PixelInternalFormat.Rgb16i,
                (Format.Short, 4) => PixelInternalFormat.Rgba16i,

                (Format.Int, 1) => PixelInternalFormat.R32i,
                (Format.Int, 2) => PixelInternalFormat.Rg32i,
                (Format.Int, 3) => PixelInternalFormat.Rgb32i,
                (Format.Int, 4) => PixelInternalFormat.Rgba32i,

                (Format.UByte, 1) => PixelInternalFormat.R8,
                (Format.UByte, 2) => PixelInternalFormat.Rg8,
                (Format.UByte, 3) => PixelInternalFormat.Rgb8,
                (Format.UByte, 4) => PixelInternalFormat.Rgba8,

                (Format.UShort, 1) => PixelInternalFormat.R16ui,
                (Format.UShort, 2) => PixelInternalFormat.Rg16ui,
                (Format.UShort, 3) => PixelInternalFormat.Rgb16ui,
                (Format.UShort, 4) => PixelInternalFormat.Rgba16ui,

                (Format.UInt, 1) => PixelInternalFormat.R32ui,
                (Format.UInt, 2) => PixelInternalFormat.Rg32ui,
                (Format.UInt, 3) => PixelInternalFormat.Rgb32ui,
                (Format.UInt, 4) => PixelInternalFormat.Rgba32ui,

                (_, _) => throw new NotSupportedException(),
            };

        private static PixelFormat FormatToPixelPF(Format format, int ch) =>
            format switch
            {
                Format.Half => FloatPF(ch),
                Format.Float => FloatPF(ch),
                Format.Depth => PixelFormat.DepthComponent,

                Format.Byte => BytePF(ch),
                Format.Short => IntPF(ch),
                Format.Int => IntPF(ch),

                Format.UByte => BytePF(ch),
                Format.UShort => IntPF(ch),
                Format.UInt => IntPF(ch),

                _ => throw new NotSupportedException(),
            };

        private static PixelType FormatToPixelPT(Format format) =>
            format switch
            {
                Format.Half => PixelType.Float,
                Format.Float => PixelType.Float,
                Format.Depth => PixelType.Float,

                Format.Byte => PixelType.Byte,
                Format.Short => PixelType.Short,
                Format.Int => PixelType.Int,

                Format.UByte => PixelType.UnsignedByte,
                Format.UShort => PixelType.UnsignedShort,
                Format.UInt => PixelType.UnsignedInt,

                _ => throw new NotSupportedException(),
            };

        private static PixelFormat FloatPF(int ch) =>
            ch switch
            {
                1 => PixelFormat.Red,
                2 => PixelFormat.Rg,
                3 => PixelFormat.Rgb,
                4 => PixelFormat.Rgba,

                _ => throw new NotSupportedException(),
            };

        private static PixelFormat BytePF(int ch) =>
            ch switch
            {
                1 => PixelFormat.Red,
                2 => PixelFormat.Rg,
                3 => PixelFormat.Rgb,
                4 => PixelFormat.Rgba,

                _ => throw new NotSupportedException(),
            };

        private static PixelFormat IntPF(int ch) =>
            ch switch
            {
                1 => PixelFormat.RedInteger,
                2 => PixelFormat.RgInteger,
                3 => PixelFormat.RgbInteger,
                4 => PixelFormat.RgbaInteger,

                _ => throw new NotSupportedException(),
            };
    }
}
