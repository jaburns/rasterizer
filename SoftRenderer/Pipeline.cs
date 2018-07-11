using GlmSharp;
using System.Linq;

namespace SoftRenderer
{
    public interface Interpolatable<T>
    {
        T Combine(T b, T c, float u, float v, float w);
    }

    public interface IVertexData
    {
        vec3 ClipPos { get; }
    }

    public interface VertexShader<T, U> where U : Interpolatable<U>, IVertexData
    {
        U ShadeVertex(T vertexInfo);
    }

    public interface FragmentShader<U> where U : Interpolatable<U>, IVertexData
    {
        vec4 ShadeFragment(U interpolators);
    }

    public class Pipeline<T, U> where U : Interpolatable<U>, IVertexData
    {
        private readonly VertexShader<T, U> vertexShader;
        private readonly FragmentShader<U> fragmentShader;

        public Pipeline(VertexShader<T, U> vertexShader, FragmentShader<U> fragmentShader)
        {
            this.vertexShader = vertexShader;
            this.fragmentShader = fragmentShader;
        }

        static ivec2 ClipSpaceToScreenSpace(int width, int height, vec3 clipPos)
        {
            return new ivec2(
                (int)(width * (0.5f + 0.5f*clipPos.x)),
                (int)(width * (0.5f + 0.5f*clipPos.y))
            );
        }

        public void Rasterize(Buffer<vec4> buffer, T[] vertices, int[] triangles)
        {
            var shaded = vertices.Select(x => vertexShader.ShadeVertex(x)).ToArray();

            for (int i = 0; i < triangles.Length; i += 3) 
            {
                var a = shaded[triangles[i  ]];
                var b = shaded[triangles[i+1]];
                var c = shaded[triangles[i+2]];

                var screenA = ClipSpaceToScreenSpace(buffer.Width, buffer.Height, a.ClipPos);
                var screenB = ClipSpaceToScreenSpace(buffer.Width, buffer.Height, b.ClipPos);
                var screenC = ClipSpaceToScreenSpace(buffer.Width, buffer.Height, c.ClipPos);

                Rasterization.FillTriangle(screenA, screenB, screenC, buffer.Width, buffer.Height, (p, bcc) => 
                {
                    var vertexData = a.Combine(b, c, bcc.x, bcc.y, bcc.z);
                    // TODO depth buffer
                    var color = fragmentShader.ShadeFragment(vertexData);
                    buffer[p.x, p.y] = color;

                //  var newDepth = ((bcc.x * a.ClipPos) + (bcc.y * b.ClipPos) + (bcc.z * c.ClipPos)).y;
                //  var oldDepth = depthBuffer[p.x, p.y];
                //  if (newDepth < oldDepth) return;

                //  fragmentShader.ShadeFragment(

                //  var uv = (bcc.x * uv_a) + (bcc.y * uv_b) + (bcc.z * uv_c);

                //  depthBuffer[p.x, p.y] = newDepth;
                //  screenBuffer[p.x, p.y] = ColorToUint(texture.SampleUV(uv.x, uv.y, vec4.Lerp) * 0.003f*(newDepth));
                });
            }


            var va = vertexShader.ShadeVertex(vertices[0]);
            var vb = vertexShader.ShadeVertex(vertices[1]);
            var vc = vertexShader.ShadeVertex(vertices[2]);

            var middlePixel = fragmentShader.ShadeFragment(va.Combine(vb, vc, 0.5f, 0.5f, 0.5f));

            buffer[0,0] = middlePixel;
        }
    }

    public class StandardShaderPipeline
    {
        struct AppData
        {
            public vec3 position;
            public vec3 normal;
            public vec2 uv;
        }

        struct VertData : Interpolatable<VertData>, IVertexData
        {
            public vec3 position;
            public vec3 normal;
            public vec2 uv;

            public VertData Combine(VertData b, VertData c, float u, float v, float w)
            {
                return new VertData {
                    position = u*position + v*b.position + w*c.position,
                    normal = u*normal + v*b.normal + w*c.normal,
                    uv = u*uv + v*b.uv + w*c.uv
                };
            }

            public vec3 ClipPos { get { return position; } }
        }

        class VertShader : VertexShader<AppData, VertData>
        {
            public VertData ShadeVertex(AppData d)
            {
                return new VertData {
                    position = d.position,
                    normal = d.normal
                };
            }
        }

        class FragShader : FragmentShader<VertData>
        {
            public vec4 ShadeFragment(VertData v)
            {
                return new vec4(v.normal, 1f);
            }
        }

        private Pipeline<AppData, VertData> pipeline
            = new Pipeline<AppData, VertData>(new VertShader(), new FragShader());

        private readonly WavefrontObj teapot;
        private readonly Buffer<vec4> texture;
        private mat4 rotation = mat4.Identity;

        public StandardShaderPipeline()
        {
            teapot = new WavefrontObj("Resources/teapot.obj");
            texture = TextureReader.LoadPNG("Resources/texture.png");

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

        public void DrawMesh()
        {
            /*
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

            pipeline.Rasterize(new Buffer<vec4>(0,0), obj.vertices.Select(x => new AppData()).ToArray(), obj.triangles.Select(x => x.vertex).ToArray());
            */
        }
    }
}
