using OpenTK.Mathematics;

namespace LibGL
{
    public class MatUtil
    {
        public static Matrix2 CameraRotation(float rot) =>
            Matrix2.CreateRotation(MathHelper.DegreesToRadians(rot));

        public static Matrix4 CameraRotation(Vector3 rot) =>
            Matrix4.CreateRotationY(MathHelper.DegreesToRadians(rot.Y))
                * Matrix4.CreateRotationX(MathHelper.DegreesToRadians(rot.X))
                * Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(rot.Z));
    }
}
