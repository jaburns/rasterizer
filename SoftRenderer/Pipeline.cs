using GlmSharp;

namespace SoftRenderer
{
    public interface Interpolatable<T>
    {
        T Combine(T b, T c, float u, float v, float w);
    }

    public interface VertexShader<T, U> where U : Interpolatable<U>
    {
        U ShadeVertex(T vertexInfo);
    }

    public interface FragmentShader<U> where U : Interpolatable<U>
    {
        vec4 ShadeFragment(U interpolators);
    }

    public class Pipeline<T, U> where U : Interpolatable<U>
    {
        private readonly VertexShader<T, U> vertexShader;
        private readonly FragmentShader<U> fragmentShader;

        public Pipeline(VertexShader<T, U> vertexShader, FragmentShader<U> fragmentShader)
        {
            this.vertexShader = vertexShader;
            this.fragmentShader = fragmentShader;
        }

        public vec4[] Rasterize(T[] vertices, int[] triangles)
        {
            var va = vertexShader.ShadeVertex(vertices[0]);
            var vb = vertexShader.ShadeVertex(vertices[1]);
            var vc = vertexShader.ShadeVertex(vertices[2]);

            var middlePixel = fragmentShader.ShadeFragment(va.Combine(vb, vc, 0.5f, 0.5f, 0.5f));

            return new vec4[] { middlePixel };
        }
    }

    public class StandardShaderPipeline
    {
        struct AppData
        {
            public vec3 position;
            public vec3 normal;
        }

        struct VertData : Interpolatable<VertData>
        {
            public vec3 position;
            public vec3 normal;

            public VertData Combine(VertData b, VertData c, float u, float v, float w)
            {
                return new VertData {
                    position = u*position + v*b.position + w*c.position,
                    normal = u*normal + v*b.normal + w*c.normal
                };
            }
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

        public StandardShaderPipeline()
        {
        }
    }
}
