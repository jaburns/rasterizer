namespace SoftRenderer
{
    public struct Vector2I
    {
        static public readonly Vector2I Zero = new Vector2I(0, 0);

        public int x, y;

        public Vector2I(int x, int y) 
        {
            this.x = x;
            this.y = y;
        }

        public Vector2 AsVector2()
        {
            return new Vector2(x, y);
        }
    }

    public struct Vector2
    {
        public float x, y;

        public Vector2(float x, float y) 
        {
            this.x = x;
            this.y = y;
        }

        static public float Dot(Vector2 a, Vector2 b)
        {
            return a.x * b.x + a.y * b.y;
        }

        static public Vector2 operator+(Vector2 lhs, Vector2 rhs)
        {
            return new Vector2(lhs.x + rhs.x, lhs.y + rhs.y);
        }

        static public Vector2 operator-(Vector2 lhs, Vector2 rhs)
        {
            return new Vector2(lhs.x - rhs.x, lhs.y - rhs.y);
        }
    }

    public struct Vector3
    {
        public float x, y, z;

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
}