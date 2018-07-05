namespace SoftRenderer
{
    static internal class Program
    {
        const int VIEW_SCALE = 2;

        static void Main(string[] args)
        {
            using (var sdl = new SDLWrapper("Scope Software Renderer", ProgramState.WIDTH, ProgramState.HEIGHT, VIEW_SCALE))
            {
                var program = new ProgramState();

                do {
                    program.Update(sdl.ReadInputState());
                }

                while (sdl.FlipFrame(program.GetRawPixels()));
            }
        }
    }

    public class ProgramState
    {
        public const int WIDTH = 400;
        public const int HEIGHT = 300;

        private readonly Vector2I[] clickPoints = new Vector2I[] { Vector2I.Zero, Vector2I.Zero, Vector2I.Zero };
        private readonly uint[] pixels = new uint[WIDTH * HEIGHT];

        private int clickPointIndex = 0;
        private SDLWrapper.InputState previousInput;

        public void Update(SDLWrapper.InputState input)
        {
            if (input.leftMouseButtonDown && !previousInput.leftMouseButtonDown) {
                OnClick(input.mousePos);
            }

            previousInput = input;
        }

        private void OnClick(Vector2I pt)
        {
            clickPoints[clickPointIndex] = pt;
            clickPointIndex++;
            clickPointIndex %= clickPoints.Length;

            if (clickPointIndex != 0) return;

            Rasterization.FillTriangle(clickPoints[0], clickPoints[1], clickPoints[2], (p, bcc) => {
                pixels[p.x + p.y * WIDTH] =
                    ((uint)(bcc.x * 0xFF) << 16) |
                    ((uint)(bcc.y * 0xFF) <<  8) |
                    ((uint)(bcc.z * 0xFF));
            });

            Rasterization.FillLine(clickPoints[0], clickPoints[1], p => pixels[p.x + p.y * WIDTH] = 0xFFFFFF);
            Rasterization.FillLine(clickPoints[1], clickPoints[2], p => pixels[p.x + p.y * WIDTH] = 0xFFFFFF);
            Rasterization.FillLine(clickPoints[2], clickPoints[0], p => pixels[p.x + p.y * WIDTH] = 0xFFFFFF);
        }

        public uint[] GetRawPixels()
        {
            return pixels;
        }
    }
}