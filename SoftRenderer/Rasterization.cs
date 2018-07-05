using System;
using GlmSharp;

namespace SoftRenderer
{
    static public class Rasterization
    {
        // Bresenham's line algorithm from: https://stackoverflow.com/a/11683720
        static public void FillLine(ivec2 a, ivec2 b, Action<ivec2> fill)
        {
            int x = a.x, y = a.y, x2 = b.x, y2 = b.y;
            int w = x2 - x ;
            int h = y2 - y ;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0 ;
            if (w<0) dx1 = -1 ; else if (w>0) dx1 = 1 ;
            if (h<0) dy1 = -1 ; else if (h>0) dy1 = 1 ;
            if (w<0) dx2 = -1 ; else if (w>0) dx2 = 1 ;
            int longest = Math.Abs(w) ;
            int shortest = Math.Abs(h) ;
            if (!(longest>shortest)) {
                longest = Math.Abs(h) ;
                shortest = Math.Abs(w) ;
                if (h<0) dy2 = -1 ; else if (h>0) dy2 = 1 ;
                dx2 = 0 ;            
            }
            int numerator = longest >> 1 ;
            for (int i=0;i<=longest;i++) {
                fill(new ivec2(x, y));
                numerator += shortest ;
                if (!(numerator<longest)) {
                    numerator -= longest ;
                    x += dx1 ;
                    y += dy1 ;
                } else {
                    x += dx2 ;
                    y += dy2 ;
                }
            }
        }

        // From: https://fgiesen.wordpress.com/2013/02/08/triangle-rasterization-in-practice/
        static public void FillTriangle(ivec2 v0, ivec2 v1, ivec2 v2, int width, int height, Action<ivec2, vec3> fill)
        {
            int Orient2D(ivec2 a, ivec2 b, ivec2 c) {
                return (b.x-a.x)*(c.y-a.y) - (b.y-a.y)*(c.x-a.x);
            }

            bool windCCW = Orient2D(v0, v1, v2) <= 0;

            if (windCCW) {
                var swap = v2;
                v2 = v1;
                v1 = swap;
            }

            int minX = Math.Min(Math.Min(v0.x, v1.x), v2.x);
            int minY = Math.Min(Math.Min(v0.y, v1.y), v2.y);
            int maxX = Math.Max(Math.Max(v0.x, v1.x), v2.x);
            int maxY = Math.Max(Math.Max(v0.y, v1.y), v2.y);

            minX = Math.Max(minX, 0);
            minY = Math.Max(minY, 0);
            maxX = Math.Min(maxX, width - 1);
            maxY = Math.Min(maxY, height - 1);

            ivec2 p;
            for (p.y = minY; p.y <= maxY; p.y++) {
                for (p.x = minX; p.x <= maxX; p.x++) {
                    int w0 = Orient2D(v1, v2, p);
                    int w1 = Orient2D(v2, v0, p);
                    int w2 = Orient2D(v0, v1, p);

                    if (w0 >= 0 && w1 >= 0 && w2 >= 0) {
                        var bcc = windCCW
                            ? GetBarycentricCoords(p, v0, v2, v1)
                            : GetBarycentricCoords(p, v0, v1, v2);

                        fill(p, bcc);
                    }
                }
            }       
        }

        // From: https://gamedev.stackexchange.com/a/23745
        static vec3 GetBarycentricCoords(vec2 p, vec2 a, vec2 b, vec2 c)
        {
            vec2 v0 = b - a, v1 = c - a, v2 = p - a;
            float d00 = vec2.Dot(v0, v0);
            float d01 = vec2.Dot(v0, v1);
            float d11 = vec2.Dot(v1, v1);
            float d20 = vec2.Dot(v2, v0);
            float d21 = vec2.Dot(v2, v1);
            float denom = d00 * d11 - d01 * d01;

            if (denom < 1e-15f && denom > -1e-15) {
                return new vec3(1, 0, 0);
            }

            float v = (d11 * d20 - d01 * d21) / denom;
            float w = (d00 * d21 - d01 * d20) / denom;
            float u = 1 - v - w;
            return new vec3(u, v, w);
        }
    }
}
