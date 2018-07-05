using System;
using GlmSharp;

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

                do { program.Update(); }
                while (sdl.FlipFrame(program.GetRawPixels()));
            }
        }
    }

    public class ProgramState
    {
        public const int WIDTH = 400;
        public const int HEIGHT = 300;

        private readonly uint[] pixels = new uint[WIDTH * HEIGHT];
        private readonly WavefrontObj teapot;
        private readonly Buffer<float> depthBuffer;

        private mat4 rotation = mat4.Identity;

        public ProgramState()
        {
            teapot = new WavefrontObj("Resources/teapot.obj");
            depthBuffer = new Buffer<float>(WIDTH, HEIGHT, float.NegativeInfinity);
        }

        public void Update()
        {
            Array.Clear(pixels, 0, pixels.Length);
            depthBuffer.Clear();

            rotation *= mat4.RotateZ(0.03f) * mat4.RotateY(-0.005f);

            for (int i = 0; i < teapot.triangles.Length; i += 3) 
            {
                var a = TransformVert(teapot.vertices[teapot.triangles[i  ].vertex]);
                var b = TransformVert(teapot.vertices[teapot.triangles[i+1].vertex]);
                var c = TransformVert(teapot.vertices[teapot.triangles[i+2].vertex]);

                Rasterization.FillTriangle(Project(a), Project(b), Project(c), WIDTH, HEIGHT, (p, bcc) => 
                {
                    var newDepth = ((bcc.x * a) + (bcc.y * b) + (bcc.z * c)).y;
                    var oldDepth = depthBuffer[p.x, p.y];

                    if (newDepth < oldDepth) return;

                    depthBuffer[p.x, p.y] = newDepth;

                    pixels[p.x + p.y * WIDTH] = (uint)(newDepth / 2);
                });
            }
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
            return pixels;
        }
    }
}