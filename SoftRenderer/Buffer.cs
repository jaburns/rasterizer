namespace SoftRenderer
{
    public class Buffer<T>
    {
        private readonly int width, height;
        private readonly T[] pixels;
        private readonly T defaultValue;

        public Buffer(int width, int height, T defaultValue)
        {
            this.width = width;
            this.height = height;
            this.defaultValue = defaultValue;

            pixels = new T[width * height];
            Clear();
        }

        public T this[int x, int y]
        {
            get {
                if (x < 0 || y < 0 || x >= width || y >= height) return defaultValue;
                return pixels[x + y * width];
            }
            set {
                if (x < 0 || y < 0 || x >= width || y >= height) return;
                pixels[x + y * width] = value;
            }
        }

        public void Clear()
        {
            for (int i = 0; i < pixels.Length; ++i) {
                pixels[i] = defaultValue;
            }
        }
    }
}