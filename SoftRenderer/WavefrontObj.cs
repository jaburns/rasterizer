using System;
using System.IO;
using System.Collections.Generic;
using GlmSharp;
using System.Linq;

namespace SoftRenderer
{
    public class WavefrontObj
    {
        public struct IndexTriplet
        {
            public int vertex;
            public int normal;
            public int uv;
        }

        public readonly vec3[] vertices;
        public readonly vec3[] normals;
        public readonly vec2[] uvs;
        public readonly IndexTriplet[] triangles;

        public WavefrontObj(string path)
        {
            var vertexList = new List<vec3>();
            var normalList = new List<vec3>();
            var uvList = new List<vec2>();
            var triangleList = new List<IndexTriplet>();

            string line;
            var file = new StreamReader(path);
            while((line = file.ReadLine()) != null) 
            {  
                if (line.Length < 1) continue;

                switch (line[0]) {
                    case 'v':
                        if (line[1] == 'n') {
                            normalList.Add(ParseVec3(line));
                        }
                        else if (line[1] == 't') {
                            uvList.Add(ParseVec3(line).xy);
                        }
                        else {
                            vertexList.Add(ParseVec3(line));
                        }
                        break;

                    case 'f':
                        ParseFaceLine(line, triangleList);
                        break;
                }
            }

            file.Close();  

            vertices = vertexList.ToArray();
            normals = normalList.ToArray();
            uvs = uvList.ToArray();
            triangles = triangleList.ToArray();
        }

        static private vec3 ParseVec3(string line)
        {
            var splits = line.Trim().Split(' ');

            return new vec3(
                float.Parse(splits[splits.Length - 3]),
                float.Parse(splits[splits.Length - 2]),
                float.Parse(splits[splits.Length - 1])
            );
        }

        static private void ParseFaceLine(string line, List<IndexTriplet> outList)
        {
            var verts = line.Trim().Split(' ').Skip(1).ToArray();

            if (verts.Length == 3) {
                outList.Add(ParseTriplet(verts[0]));
                outList.Add(ParseTriplet(verts[1]));
                outList.Add(ParseTriplet(verts[2]));
            }
            else if (verts.Length == 4) {
                var v0 = ParseTriplet(verts[0]);
                var v1 = ParseTriplet(verts[1]);
                var v2 = ParseTriplet(verts[2]);
                var v3 = ParseTriplet(verts[3]);

                outList.Add(v0);
                outList.Add(v1);
                outList.Add(v2);
                outList.Add(v0);
                outList.Add(v2);
                outList.Add(v3);
            }
            else {
                throw new System.Exception("Unexpected number of indices in face");
            }
        }

        static private IndexTriplet ParseTriplet(string triplet)
        {
            var indices = triplet.Split('/');

            return new IndexTriplet {
                vertex = int.Parse(indices[0]) - 1,
                uv = int.Parse(indices[1]) - 1,
                normal = int.Parse(indices[2]) - 1,
            };
        }

		public StandardShader.AppData[] GetStandardAppDataForTriangles()
		{
            var result = new StandardShader.AppData[vertices.Length];

            for (int i = 0; i < vertices.Length; ++i) {
                result[i].position = vertices[i];
            }

            for (int i = 0; i < triangles.Length; ++i) {
                result[triangles[i].vertex].uv = uvs[triangles[i].uv];
                result[triangles[i].vertex].normal = normals[triangles[i].normal];
            }

            return result;
		}
    }
}