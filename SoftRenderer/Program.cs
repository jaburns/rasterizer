using GlmSharp;
using System.Linq;

namespace SoftRenderer
{
    static internal class Program
    {
        const int VIEW_SCALE = 1;

        static void Main(string[] args)
        {
            using (var sdl = new SDLWrapper("Rasterizer", ProgramState.WIDTH, ProgramState.HEIGHT, VIEW_SCALE))
            {
                var program = new ProgramState();

                do { program.Update(); }
                while (sdl.FlipFrame(program.GetRawPixels()));
            }
        }
    }

    public class ProgramState
    {
        public const int WIDTH = 400;
        public const int HEIGHT = 300;

		private readonly WavefrontObj teapot;
		private readonly Buffer<vec4> texture;
        private readonly Buffer<float> depthBuffer;
        private readonly Buffer<vec4> screenBuffer;
		private readonly IPipeline<StandardShader.AppData> pipeline;

        public ProgramState()
        {
			teapot = new WavefrontObj("Resources/teapot.obj");
            texture = TextureReader.LoadPNG("Resources/texture.png");

			pipeline = StandardShader.GetPipeline();

            screenBuffer = new Buffer<vec4>(WIDTH, HEIGHT);
            depthBuffer = new Buffer<float>(WIDTH, HEIGHT, float.NegativeInfinity);
        }

        public void Update()
        {
            screenBuffer.Clear();
            depthBuffer.Clear();

			pipeline.Draw(depthBuffer, screenBuffer, teapot.GetStandardAppDataForTriangles(), teapot.triangles.Select(x => x.vertex).ToArray());
        }

        public uint[] GetRawPixels()
        {
			return screenBuffer.RawPixels.Select(ColorToUint).ToArray();
        }

		static uint ColorToUint(vec4 color)
        {
            return (uint)(0xFF * color.r) |
                ((uint)(0xFF * color.g) << 8) |
                ((uint)(0xFF * color.b) << 16);
        }
    }
}