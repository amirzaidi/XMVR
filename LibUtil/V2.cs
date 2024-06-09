namespace LibUtil
{
    public readonly struct V2
    {
        public static readonly V2 ZERO = new(0f, 0f);

        public readonly float X, Y;

        public V2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public V2((float x, float y) a)
        {
            X = a.x;
            Y = a.y;
        }

        public V2(float[] input, int index)
        {
            X = input[index + 0];
            Y = input[index + 1];
        }

        public void Write(float[] output, int index)
        {
            output[index + 0] = X;
            output[index + 1] = Y;
        }

        public void Write(List<float> output, int index)
        {
            output[index + 0] = X;
            output[index + 1] = Y;
        }

        public float Length =>
            (float)Math.Sqrt(X * X + Y * Y);

        public V2 Normalized =>
            this / Length;

        public (float x, float y) ToTuple() =>
            (X, Y);

        public override bool Equals(object? obj) =>
            obj is V2 v &&
                X == v.X &&
                Y == v.Y;

        public override int GetHashCode() =>
            HashCode.Combine(X, Y);

        public static V2 operator +(V2 a, V2 b) =>
            new(a.X + b.X, a.Y + b.Y);

        public static V2 operator *(float a, V2 b) =>
            new(a * b.X, a * b.Y);

        public static V2 operator -(V2 a) =>
            -1f * a;

        public static V2 operator -(V2 a, V2 b) =>
            a + -b;

        public static V2 operator /(V2 a, float b) =>
            1f / b * a;

        public static bool operator ==(V2 a, V2 b) =>
            a.X == b.X && a.Y == b.Y;

        public static bool operator !=(V2 a, V2 b) =>
            !(a == b);

        public static float Dot(V2 a, V2 b) =>
            a.X * b.X + a.Y * b.Y;

        public static float Det(V2 aRow, V2 bRow) =>
            aRow.X * bRow.Y - aRow.Y * bRow.X;

        public override string ToString()
        {
            return $"V2({X};{Y})";
        }
    }
}
