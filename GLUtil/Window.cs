using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace LibGL
{
    public class Window(IRendererCallbacks cb) : GameWindow(sGameSettings, sNativeSettings)
    {
        private static readonly GameWindowSettings sGameSettings = new()
        {
        };

        private static readonly NativeWindowSettings sNativeSettings = new()
        {
            ClientSize = new Vector2i(1024, 768),
        };

        private readonly IRendererCallbacks mCb = cb;

        private const int RESIZE_WIDTH = 0x1;
        private const int RESIZE_HEIGHT = 0x2;

        private int RenderWidth => Size.X;
        private int RenderHeight => Size.Y;

        private Vector2i mPrevSize, mIntendSize;
        private int mLastResize;
        private bool mShouldRecreateBuffers;

        protected override void OnLoad()
        {
            base.OnLoad();
            Debug.Enable();
            GL.Disable(EnableCap.Dither);
            mCb.Init(this);
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
                // Force Resize. This does not call OnResize.
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

        // Physics and everything that might be frame-independent.
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (!mCb.Update(e.Time, KeyboardState))
            {
                Close();
            }
        }

        // Everything rendering related, including rescaling resources.
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            if (mShouldRecreateBuffers)
            {
                mShouldRecreateBuffers = false;
                if (!mCb.Resize(RenderWidth, RenderHeight))
                {
                    Close();
                }
            }

            if (!mCb.Render())
            {
                Close();
            }
        }
    }
}
