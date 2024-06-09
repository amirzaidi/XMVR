using LibGL;
using LibGL.Buffers;
using LibGL.Shaders;
using LibMesh.Data;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Window = LibGL.Window;

namespace BasicApp
{
    internal class Renderer : IRendererCallbacks, IDisposable
    {
        // VR usually has a hardcoded resolution at program start.
        private const int BASE_EYE_WIDTH = 1024;
        private const int BASE_EYE_HEIGHT = 1024;
        private const float SIMULATE_EYE_DIST = 0.025f;

        // Perspective constants.
        private const float VERTICAL_FOV = 80f;
        private const float FRUSTUM_NEAR = 0.01f;
        private const float FRUSTUM_FAR = 100f;

        private readonly Camera mCamera = new();
        private Matrix4 View =>
            Matrix4.CreateTranslation(-mCamera.ViewPos) * MatUtil.CameraRotation(-mCamera.ViewRot);

        // These are read from the VR headset.
        private readonly Matrix4[] ViewEye = new Matrix4[2];
        private readonly Matrix4[] ProjectionEye = new Matrix4[2];

        // Add all objects to be rendered here.
        private readonly List<StandardizedModel> mInputModels = [];
        private readonly List<Model> mModels = [];

        // To-Do: Move these somewhere non-nullable.
        private Window? mWindow;
        private ShaderProgram? mProgram;
        private RenderOutput? mOutput;

        public bool Init(Window window)
        {
            mWindow = window;
            RecreateRenderOutput();

            // Create a rasterization pipeline using a vertex and fragment shader.
            using (var vert = new Shader(ShaderType.VertexShader, "./Shaders/vert.glsl"))
            using (var frag = new Shader(ShaderType.FragmentShader, "./Shaders/frag.glsl"))
            {
                mProgram = new ShaderProgram(vert, frag);
            }

            // Create different matrices for two different eyes in VR.
            for (var eye = 0; eye < 2; eye += 1)
            {
                ViewEye[eye] = Matrix4.CreateTranslation(new Vector3(SIMULATE_EYE_DIST * (eye - 0.5f), 0, 0));
                ProjectionEye[eye] = Matrix4.CreatePerspectiveFieldOfView(
                    MathHelper.DegreesToRadians(VERTICAL_FOV),
                    (float)BASE_EYE_WIDTH / BASE_EYE_WIDTH,
                    FRUSTUM_NEAR,
                    FRUSTUM_FAR
                );
            }

            // Process all queued models.
            mInputModels.ForEach(AddModel);
            mInputModels.Clear();

            return true;
        }

        private void RecreateRenderOutput()
        {
            mOutput?.Dispose();

            var w = mWindow!.Size.X;
            var h = mWindow!.Size.Y;
            mOutput = new RenderOutput(
                new RenderBuffer(w, h),
                new Texture(w, h, ch: 4)
            );
        }

        public void AddModel(StandardizedModel model)
        {
            if (mProgram == null)
            {
                mInputModels.Add(model);
            }
            else
            {
                mModels.Add(new Model(mProgram!, model));
            }
        }

        public bool Update(double dt, KeyboardState ks)
        {
            mCamera.UpdateMovement(dt, ks);
            return true;
        }

        public bool Resize(int w, int h)
        {
            // Instead of resizing nicely, we force an aspect ratio.
            mWindow!.ForceResizeToAspect(BASE_EYE_WIDTH, BASE_EYE_HEIGHT);
            RecreateRenderOutput();
            return true;
        }

        public bool Render()
        {
            // Use this if you want a forced render resolution for VR.
            //int w = BASE_EYE_WIDTH;
            //int h = BASE_EYE_HEIGHT;

            // Use this to draw for exactly the window size.
            int w = mWindow!.Size.X;
            int h = mWindow!.Size.Y;

            // Set up standard rendering parameters.
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            using (mProgram!.Bind())
            using (mOutput!.FB.Bind())
            {
                // Create empty render space.
                GL.Viewport(0, 0, w, h);
                GL.ClearColor(0.05f, 0.10f, 0.15f, 1f);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                // Set static view and projection matrices.
                var view = View;
                var projection = ProjectionEye.First();
                GL.UniformMatrix4(mProgram.GetUniformLocation("View"), false, ref view);
                GL.UniformMatrix4(mProgram.GetUniformLocation("Projection"), false, ref projection);

                // Set static uniform parameters.
                GL.Uniform3(mProgram.GetUniformLocation("CameraPosition"), mCamera.ViewPos);
                GL.Uniform3(mProgram.GetUniformLocation("LightDirection"), -Vector3.One.Normalized());
                GL.Uniform3(mProgram.GetUniformLocation("LightColor"), 0.5f * Vector3.One);

                // For every model.
                foreach (var model in mModels)
                {
                    // Set model matrix. Currently hardcoded at translation and scale.
                    var modelMatrix = Matrix4.CreateTranslation(new(0f, 0f, -2f))
                        * Matrix4.CreateScale(0.1f);

                    // Needed for normal vectors.
                    // Source: https://paroj.github.io/gltut/Illumination/Tut09%20Normal%20Transformation.html
                    var modelNormalMatrix = modelMatrix;
                    modelNormalMatrix.Row3.Xyz = Vector3.Zero;
                    modelNormalMatrix = modelNormalMatrix.Inverted();
                    modelNormalMatrix.Transpose();

                    GL.UniformMatrix4(mProgram.GetUniformLocation("Model"), false, ref modelMatrix);
                    GL.UniformMatrix4(mProgram.GetUniformLocation("ModelNormal"), false, ref modelNormalMatrix);

                    // Bind mesh.
                    using (model.VAO.Bind())
                    using (model.EBO.Bind())
                    {
                        // Draw indices for cube.
                        mProgram.Validate();
                        GL.DrawElements(PrimitiveType.Triangles, 36, DrawElementsType.UnsignedInt, 0 * sizeof(uint));
                    }
                }
            }

            // Clean up rendering parameters.
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);

            // Copy rendered output to window framebuffer.
            GL.Enable(EnableCap.FramebufferSrgb);
            using (mOutput!.FB.Bind(FramebufferTarget.ReadFramebuffer))
            {
                GL.BlitFramebuffer(
                    0, 0, w, h,
                    0, 0, w, h,
                    ClearBufferMask.ColorBufferBit,
                    BlitFramebufferFilter.Nearest
                );
            }
            GL.Disable(EnableCap.FramebufferSrgb);

            // Show the output to the screen.
            mWindow!.SwapBuffers();
            return true;
        }

        public void Dispose()
        {
            mProgram?.Dispose();
            mModels.ForEach(_ => _.Dispose());
        }
    }
}
