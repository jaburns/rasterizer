using System;
using System.Runtime.InteropServices;

namespace Sharpy
{
    static class SDLWrapper
    {
        [DllImport("SDLWrapper.dll", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)]
        static public extern void sdlw_init(string title, int width, int height, int scale);

        [DllImport("SDLWrapper.dll", CallingConvention=CallingConvention.Cdecl)]
        static public extern bool sdlw_flip_frame(uint[] pixels);

        [DllImport("SDLWrapper.dll", CallingConvention=CallingConvention.Cdecl)]
        static public extern void sdlw_quit();
    }

    class Program
    {
        static void Main(string[] args)
        {
            var pixels = new uint[400 * 300];
            var rng = new Random();

            SDLWrapper.sdlw_init("Hello from C#", 400, 300, 2);

            while (SDLWrapper.sdlw_flip_frame(pixels))
            {
                ColorRandomPixel(rng, pixels);
            }

            SDLWrapper.sdlw_quit();
        }

        static void ColorRandomPixel(Random rng, uint[] pixels)
        {
            pixels[(int)Math.Floor(rng.NextDouble() * pixels.Length)] = (uint)rng.Next();
        }
    }
}
