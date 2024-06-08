namespace LibUtil
{
    public readonly struct V3
    {
        public static readonly V3 ZERO = new(0f, 0f, 0f);

        public readonly float X, Y, Z;

        public V2 XY => new(X, Y);
        public V2 XZ => new(X, Z);
        public V2 YZ => new(Y, Z);

        public V3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public V3((float x, float y, float z) a)
        {
            X = a.x;
            Y = a.y;
            Z = a.z;
        }

        public V3(V2 v2, float z)
        {
            X = v2.X;
            Y = v2.Y;
            Z = z;
        }

        public V3(float[] input, int index)
        {
            X = input[index + 0];
            Y = input[index + 1];
            Z = input[index + 2];
        }

        public void Write(float[] output, int index)
        {
            output[index + 0] = X;
            output[index + 1] = Y;
            output[index + 2] = Z;
        }

        public void Write(List<float> output, int index)
        {
            output[index + 0] = X;
            output[index + 1] = Y;
            output[index + 2] = Z;
        }

        public float LengthSquared =>
            X * X + Y * Y + Z * Z;

        public float Length =>
            (float)Math.Sqrt(LengthSquared);

        public V3 Normalized =>
            this == ZERO
                ? ZERO
                : this / Length;

        public (float x, float y, float z) AsTuple =>
            (X, Y, Z);

        public override bool Equals(object? obj) =>
            obj is V3 v &&
                X == v.X &&
                Y == v.Y &&
                Z == v.Z;

        public override int GetHashCode() =>
            HashCode.Combine(X, Y, Z);

        public static V3 operator +(V3 a, V3 b) =>
            new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        public static V3 operator *(float a, V3 b) =>
            new(a * b.X, a * b.Y, a * b.Z);

        public static V3 operator -(V3 a) =>
            -1f * a;

        public static V3 operator -(V3 a, V3 b) =>
            a + -b;

        public static V3 operator /(V3 a, float b) =>
            1f / b * a;

        public static bool operator ==(V3 a, V3 b) =>
            a.X == b.X && a.Y == b.Y && a.Z == b.Z;

        public static bool operator !=(V3 a, V3 b) =>
            !(a == b);

        public static float Dot(V3 a, V3 b) =>
            a.X * b.X
            + a.Y * b.Y
            + a.Z * b.Z;

        public static V3 Cross(V3 a, V3 b) =>
            new(
                V2.Det(a.YZ, b.YZ),
                -V2.Det(a.XZ, b.XZ),
                V2.Det(a.XY, b.XY)
            );

        public static int Compare(V3 a, V3 b)
        {
            if (a.X == b.X)
            {
                if (a.Y == b.Y)
                {
                    if (a.Z == b.Z)
                    {
                        return 0;
                    }

                    return a.Z > b.Z ? 1 : -1;
                }

                return a.Y > b.Y ? 1 : -1;
            }

            return a.X > b.X ? 1 : -1;
        }

        public override string ToString()
        {
            return $"V3({X};{Y};{Z})";
        }
    }
}
