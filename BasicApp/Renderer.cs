using LibGL;
using LibGL.Buffers;
using LibGL.Shaders;
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
        private readonly List<(VertexBufferObject vbo, VertexArrayObject vao)> mModels = [];

        // To-Do: Move these somewhere non-nullable.
        private Window? mWindow;
        private ShaderProgram? mProgram;

        public bool Init(Window window)
        {
            mWindow = window;

            using (var vert = new Shader(ShaderType.VertexShader, "./Shaders/vert.glsl"))
            using (var frag = new Shader(ShaderType.FragmentShader, "./Shaders/frag.glsl"))
            {
                mProgram = new ShaderProgram(vert, frag);
            }

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

            // The VBO stores the triangle vertices.
            var vbo = new VertexBufferObject();
            using (vbo.Bind())
            {
                GL.BufferData(BufferTarget.ArrayBuffer, Mesh.VERTICES.Length * sizeof(float), Mesh.VERTICES, BufferUsageHint.StaticDraw);
            }

            // The VAO stores how to render those vertices.
            var vao = new VertexArrayObject();
            using (vao.Bind())
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo.Id);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
                GL.EnableVertexAttribArray(0);
            }

            // Add it to the list of models to be rendered.
            mModels.Add((vbo, vao));

            return true;
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

            // Create empty render space.
            GL.Viewport(0, 0, w, h);
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Set up standard rendering parameters.
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            using (mProgram!.Bind())
            {
                // Set static view and projection matrices.
                var view = View;
                var projection = ProjectionEye.First();
                GL.UniformMatrix4(mProgram.GetUniformLocation("View"), false, ref view);
                GL.UniformMatrix4(mProgram.GetUniformLocation("Projection"), false, ref projection);

                // For every model.
                foreach (var (_, vao) in mModels)
                {
                    // Set model matrix. Currently hardcoded at (0, 0, -2) translation.
                    var model = Matrix4.CreateTranslation(new(0f, 0f, -2f));
                    var modelNormal = model;
                    modelNormal.Row3.Xyz = Vector3.Zero;
                    modelNormal = modelNormal.Inverted();
                    modelNormal.Transpose(); // https://paroj.github.io/gltut/Illumination/Tut09%20Normal%20Transformation.html

                    GL.UniformMatrix4(mProgram.GetUniformLocation("Model"), false, ref model);
                    GL.UniformMatrix4(mProgram.GetUniformLocation("ModelNormal"), false, ref modelNormal);

                    // Bind mesh.
                    using (vao.Bind())
                    {
                        // Draw triangle.
                        GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
                    }
                }
            }

            // Clean up rendering parameters.
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);

            // Show the output to the screen.
            mWindow!.SwapBuffers();
            return true;
        }

        public void Dispose()
        {
            mModels.ForEach(_ =>
            {
                _.vbo.Dispose();
                _.vao.Dispose();
            });
        }
    }
}
