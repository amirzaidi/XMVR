using LibUtil;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;

namespace LibGL
{
    public class Window(IRenderer renderer) : GameWindow(sGameSettings, sNativeSettings)
    {
        private static readonly GameWindowSettings sGameSettings = new()
        {
        };

        private static readonly NativeWindowSettings sNativeSettings = new()
        {
            ClientSize = new Vector2i(1024, 768),
        };

        private readonly IRenderer mRenderer = renderer;

        private const int RESIZE_WIDTH = 0x1;
        private const int RESIZE_HEIGHT = 0x2;

        // Multiples of two.
        private int RenderWidth => (Size.X / 2) * 2;
        private int RenderHeight => (Size.Y / 2) * 2;

        private Vector2i mPrevSize, mIntendSize;
        private int mLastResize;
        private bool mShouldRecreateBuffers;

        protected override void OnLoad()
        {
            base.OnLoad();
            Debug.Enable();
            GL.Disable(EnableCap.Dither);
            mRenderer.Init(this);
        }

        public void SetVSync(bool vsync)
        {
            GLFW.SwapInterval(vsync ? 1 : 0);
            VSync = vsync
                ? VSyncMode.On
                : VSyncMode.Off;
        }

        public void SetFullScreen(bool fullscreen) =>
            WindowState = fullscreen
                ? WindowState.Fullscreen
                : WindowState.Normal;

        // Needed to enforce VR aspect ratio.
        public void ForceResizeToAspect(int width, int height)
        {
            if (mLastResize == 0)
            {
                // Do Nothing: Automatic Resize.
            }
            else if (mLastResize == RESIZE_WIDTH)
            {
                // Change Height.
                mIntendSize = new Vector2i(Size.X, (int)Math.Ceiling((double)Size.X * width / height));
            }
            else if (mLastResize == RESIZE_HEIGHT)
            {
                // Change Width.
                mIntendSize = new Vector2i((int)Math.Ceiling((double)Size.Y * height / width), Size.Y);
            }
            else // Diagonal resize?
            {
                // For now, Change Width.
                mIntendSize = new Vector2i((int)Math.Ceiling((double)Size.Y * height / width), Size.Y);
            }

            if (Size != mIntendSize)
            {
                // Force Resize.
                Size = mIntendSize;
            }
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            // Not initialized yet, ignore this.
            // If the window was resized without actually moving, then ignore too.
            if (e.Width == 0 || e.Height == 0 || Size == mPrevSize)
            {
                return;
            }

            mPrevSize = Size;
            mLastResize = 0;
            if (Size != mIntendSize)
            {
                if (Size.X != mIntendSize.X)
                {
                    mLastResize |= RESIZE_WIDTH;
                }

                if (Size.Y != mIntendSize.Y)
                {
                    mLastResize |= RESIZE_HEIGHT;
                }
            }

            mShouldRecreateBuffers = true;
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (!mRenderer.Update(e.Time, KeyboardState))
            {
                Close();
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            if (mShouldRecreateBuffers)
            {
                mShouldRecreateBuffers = false;
                if (!mRenderer.Resize(RenderWidth / 2, RenderHeight / 2))
                {
                    Close();
                }
            }

            if (!mRenderer.Render())
            {
                Close();
            }
        }
    }
}
