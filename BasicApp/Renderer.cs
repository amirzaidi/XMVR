using LibGL;
using LibGL.Shaders;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Window = LibGL.Window;

namespace BasicApp
{
    internal class Renderer : IRenderer, IDisposable
    {
        private const int BASE_EYE_WIDTH = 1024;
        private const int BASE_EYE_HEIGHT = 1024;
        private const float SIMULATE_EYE_DIST = 0.025f;

        // Perspective constants.
        private const float VERTICAL_FOV = 80f;
        private const float FRUSTUM_NEAR = 0.01f;
        private const float FRUSTUM_FAR = 100f;

        private Matrix4 V = Matrix4.Identity;
        private readonly Matrix4[] VEye = new Matrix4[2];
        private readonly Matrix4[] PEye = new Matrix4[2];

        private Vector3 P, R;

        private Window? mWindow;
        private ShaderProgram? mProgram;
        private int VBO, VAO;

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
                VEye[eye] = Matrix4.CreateTranslation(new Vector3(SIMULATE_EYE_DIST * (eye - 0.5f), 0, 0));
                PEye[eye] = Matrix4.CreatePerspectiveFieldOfView(
                    MathHelper.DegreesToRadians(VERTICAL_FOV),
                    (float)BASE_EYE_WIDTH / BASE_EYE_WIDTH,
                    FRUSTUM_NEAR,
                    FRUSTUM_FAR
                );
            }

            // To-Do: Clean this up.
            VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, Mesh.VERTICES.Length * sizeof(float), Mesh.VERTICES, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0); // Unbind.

            // To-Do: Clean this up.
            VAO = GL.GenVertexArray();
            GL.BindVertexArray(VAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.BindVertexArray(0); // Unbind.

            return true;
        }

        public bool Resize(int w, int h)
        {
            // Instead of resizing nicely, we force an aspect ratio.
            mWindow!.ForceResizeToAspect(BASE_EYE_WIDTH, BASE_EYE_HEIGHT);

            // For now ignore a request to resize in the renderer.
            return true;
        }

        public bool Update(double dt, KeyboardState ks)
        {
            // Set the view matrix for the current frame.
            V = Matrix4.Identity;
            V *= Matrix4.CreateTranslation(P); // Translate camera in absolute space.
            V *= RotationMatrix(R); // Rotate camera in absolute space.

            return true;
        }

        // Camera.
        private static Matrix4 RotationMatrix(Vector3 rotation) =>
            Matrix4.CreateRotationY(MathHelper.DegreesToRadians(rotation.Y))
                * Matrix4.CreateRotationX(MathHelper.DegreesToRadians(rotation.X))
                * Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(rotation.Z));

        public bool Render()
        {
            int w = BASE_EYE_WIDTH;
            int h = BASE_EYE_HEIGHT;

            // Create empty render space.
            GL.Viewport(0, 0, w, h);
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Set up rendering parameters.
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            using (mProgram!.Bind())
            {
                // Bind VAO.
                GL.BindVertexArray(VAO);

                // Draw triangle.
                GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

                // Unbind VAO.
                GL.BindVertexArray(0);
            }

            // Clean up rendering parameters.
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);

            mWindow!.SwapBuffers();

            return true;
        }

        public void Dispose()
        {
        }
    }
}
