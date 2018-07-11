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
        private readonly Buffer<uint> screenBuffer;

        private mat4 rotation = mat4.Identity;

        public ProgramState()
        {
            teapot = new WavefrontObj("Resources/teapot.obj");
            texture = TextureReader.LoadPNG("Resources/texture.png");
            screenBuffer = new Buffer<uint>(WIDTH, HEIGHT);
            depthBuffer = new Buffer<float>(WIDTH, HEIGHT, float.NegativeInfinity);
        }

        public void Update()
        {
            screenBuffer.Clear();
            depthBuffer.Clear();

            rotation *= mat4.RotateZ(0.03f) * mat4.RotateY(-0.005f);

            for (int i = 0; i < teapot.triangles.Length; i += 3) 
            {
                var a = TransformVert(teapot.vertices[teapot.triangles[i  ].vertex]);
                var b = TransformVert(teapot.vertices[teapot.triangles[i+1].vertex]);
                var c = TransformVert(teapot.vertices[teapot.triangles[i+2].vertex]);

                var uv_a = teapot.uvs[teapot.triangles[i  ].uv];
                var uv_b = teapot.uvs[teapot.triangles[i+1].uv];
                var uv_c = teapot.uvs[teapot.triangles[i+2].uv];

                Rasterization.FillTriangle(Project(a), Project(b), Project(c), WIDTH, HEIGHT, (p, bcc) => 
                {
                    var newDepth = ((bcc.x * a) + (bcc.y * b) + (bcc.z * c)).y;
                    var oldDepth = depthBuffer[p.x, p.y];

                    if (newDepth < oldDepth) return;

                    var uv = (bcc.x * uv_a) + (bcc.y * uv_b) + (bcc.z * uv_c);

                    depthBuffer[p.x, p.y] = newDepth;
                    screenBuffer[p.x, p.y] = ColorToUint(texture.SampleUV(uv.x, uv.y, vec4.Lerp) * 0.003f*(newDepth));
                });
            }
        }

        static uint ColorToUint(vec4 color)
        {
            return (uint)(0xFF*color.r) |
                ((uint)(0xFF*color.g) << 8) |
                ((uint)(0xFF*color.b) << 16);
        }

        private vec3 TransformVert(vec3 v) 
        {
            v *= 10;
            v.z = -v.z;
            v = (rotation * (new vec4(v, 1))).xyz;
            v += new vec3(200, 150, 230);
            return v;
        }

        private ivec2 Project(vec3 v)
        {
            return new ivec2((int)v.x, (int)v.z);
        }

        public uint[] GetRawPixels()
        {
            return screenBuffer.RawPixels;
        }
    }
}