using GlmSharp;
using System.Drawing;

namespace SoftRenderer
{
    static public class TextureReader
    {
        static public Buffer<vec4> LoadPNG(string path)
        {
            var image = Image.FromFile(path, true);
            var bitmap = new Bitmap(image);
            var result = new Buffer<vec4>(image.Width, image.Height);

            for (int ix = 0; ix < image.Width; ++ix) {
                for (int iy = 0; iy < image.Height; ++iy) {
                    var px = bitmap.GetPixel(ix, iy);
                    result[ix, iy] = new vec4(px.R, px.G, px.B, px.A) / 255f;
                }
            }

            return result;
        }
    }
}
