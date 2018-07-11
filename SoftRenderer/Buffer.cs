using System;

namespace SoftRenderer
{
    public class Buffer<T>
    {
        public readonly int Width, Height;

        private readonly T defaultValue;

        public T[] RawPixels { get; private set; }

        public Buffer(int width, int height, T defaultValue)
        {
            Width = width;
            Height = height;
            this.defaultValue = defaultValue;

            RawPixels = new T[width * height];
            Clear();
        }

        public Buffer(int width, int height)
            : this(width, height, default(T))
        { } 

        public T this[int x, int y]
        {
            get {
                if (x < 0 || y < 0 || x >= Width || y >= Height) return defaultValue;
                return RawPixels[x + y * Width];
            }
            set {
                if (x < 0 || y < 0 || x >= Width || y >= Height) return;
                RawPixels[x + y * Width] = value;
            }
        }

        public T SampleUV(float u, float v, Func<T, T, float, T> lerp)
        {
            float fix(float z) {
                while (z < 0) z += 1f;
                z %= 1f;
                return z;
            }

            u = fix(u);
            v = fix(v);

            var x = u * (Width - 1);
            var y = v * (Height - 1);

            var x_floor = (int)x;
            var y_floor = (int)y;

            var x_fract = x - x_floor;
            var y_fract = y - y_floor;

            var x_next = x_floor ==  Width - 1 ? x_floor : x_floor + 1;
            var y_next = y_floor == Height - 1 ? y_floor : y_floor + 1;

            var a = this[x_floor, y_floor];
            var b = this[x_next, y_floor];
            var c = this[x_floor, y_next];
            var d = this[x_next, y_next];

            var ab = lerp(a, b, x_fract);
            var cd = lerp(c, d, x_fract);

            return lerp(ab, cd, y_fract);
        }

        public void Clear()
        {
            for (int i = 0; i < RawPixels.Length; ++i) {
                RawPixels[i] = defaultValue;
            }
        }
    }
}