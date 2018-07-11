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

        private readonly Buffer<float> depthBuffer;
        private readonly Buffer<uint> screenBuffer;

        private readonly StandardShaderPipeline pipeline;

        public ProgramState()
        {
            pipeline = new StandardShaderPipeline();
            screenBuffer = new Buffer<uint>(WIDTH, HEIGHT);
            depthBuffer = new Buffer<float>(WIDTH, HEIGHT, float.NegativeInfinity);
        }

        public void Update()
        {
            screenBuffer.Clear();
            depthBuffer.Clear();
            pipeline.DrawMesh();
        }

        public uint[] GetRawPixels()
        {
            return screenBuffer.RawPixels;
        }
    }
}