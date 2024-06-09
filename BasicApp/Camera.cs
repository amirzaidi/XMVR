using LibGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace BasicApp
{
    internal class Camera
    {
        // Speed of movement and rotation
        private const double MS = 0.25d;
        private const double RSY = 0.25d;
        private const double RSX = 0.25d;

        internal Vector3 ViewPos, ViewRot;
        
        internal void UpdateMovement(double dt, KeyboardState ks)
        {
            // First update position in XZ plane.
            var move = new Vector2();
            if (ks.IsKeyDown(Keys.A)) move.X -= (float)(MS * dt);
            if (ks.IsKeyDown(Keys.D)) move.X += (float)(MS * dt);
            if (ks.IsKeyDown(Keys.W)) move.Y -= (float)(MS * dt);
            if (ks.IsKeyDown(Keys.S)) move.Y += (float)(MS * dt);
            ViewPos.Xz += MatUtil.CameraRotation(ViewRot.Y) * move;

            // Then update height.
            if (ks.IsKeyDown(Keys.Q)) ViewPos.Y -= (float)(MS * dt);
            if (ks.IsKeyDown(Keys.E)) ViewPos.Y += (float)(MS * dt);

            // Then update rotation.
            if (ks.IsKeyDown(Keys.Left))
            {
                ViewRot.Y += (float)(RSY * 360d * dt);
                if (ViewRot.Y < -180f) ViewRot.Y -= 360f;
            }

            if (ks.IsKeyDown(Keys.Right))
            {
                ViewRot.Y -= (float)(RSY * 360d * dt);
                if (ViewRot.Y > 180f) ViewRot.Y += 360f;
            }

            if (ks.IsKeyDown(Keys.Up))
            {
                ViewRot.X += (float)(RSX * 360d * dt);
                if (ViewRot.X > 90f) ViewRot.X = 90f;
            }

            if (ks.IsKeyDown(Keys.Down))
            {
                ViewRot.X -= (float)(RSX * 360d * dt);
                if (ViewRot.X < -90f) ViewRot.X = -90f;
            }
        }
    }
}
