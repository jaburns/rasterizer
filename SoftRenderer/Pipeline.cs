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

	public interface IPipeline<T>
	{
		void Draw(Buffer<float> depth, Buffer<vec4> buffer, T[] vertices, int[] triangles);
	}

	public class Pipeline<T, U> : IPipeline<T> where U : Interpolatable<U>, IVertexData
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
				(int)(width * (0.5f + 0.5f * clipPos.x)),
				(int)(width * (0.5f + 0.5f * clipPos.y))
			);
		}

		public void Draw(Buffer<float> depth, Buffer<vec4> buffer, T[] vertices, int[] triangles)
		{
			var shaded = vertices.Select(x => vertexShader.ShadeVertex(x)).ToArray();

			for (int i = 0; i < triangles.Length; i += 3)
			{
				var a = shaded[triangles[i]];
				var b = shaded[triangles[i + 1]];
				var c = shaded[triangles[i + 2]];

				var screenA = ClipSpaceToScreenSpace(buffer.Width, buffer.Height, a.ClipPos);
				var screenB = ClipSpaceToScreenSpace(buffer.Width, buffer.Height, b.ClipPos);
				var screenC = ClipSpaceToScreenSpace(buffer.Width, buffer.Height, c.ClipPos);

				Rasterization.FillTriangle(screenA, screenB, screenC, buffer.Width, buffer.Height, (p, bcc) =>
				{
					var newDepth = ((bcc.x * a.ClipPos) + (bcc.y * b.ClipPos) + (bcc.z * c.ClipPos)).z;
					var oldDepth = depth[p.x, p.y];

					if (newDepth < oldDepth) return;

					depth[p.x, p.y] = newDepth;

					var vertexData = a.Combine(b, c, bcc.x, bcc.y, bcc.z);
					var color = fragmentShader.ShadeFragment(vertexData);

					buffer[p.x, p.y] = color;
				});
			}
		}
	}

	static public class StandardShader
	{
		public struct AppData
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
				return new VertData
				{
					position = u * position + v * b.position + w * c.position,
					normal = u * normal + v * b.normal + w * c.normal,
					uv = u * uv + v * b.uv + w * c.uv
				};
			}

			public vec3 ClipPos { get { return position; } }
		}

		class VertShader : VertexShader<AppData, VertData>
		{
			public VertData ShadeVertex(AppData d)
			{
				return new VertData
				{
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

		static public IPipeline<AppData> GetPipeline()
		{
			return new Pipeline<AppData, VertData>(new VertShader(), new FragShader());
		}
	}
}