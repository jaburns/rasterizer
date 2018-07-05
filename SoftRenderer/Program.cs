namespace SoftRenderer
{
    static class Program
    {
        const int WIDTH = 400;
        const int HEIGHT = 300;

        static void Main(string[] args)
        {
            var pixels = new uint[WIDTH * HEIGHT];

            using (var sdl = new SDLWrapper("Scope Software Renderer", WIDTH, HEIGHT, 2))
            {
                do
                {
                    var inputs = sdl.ReadInputState();

                    if (inputs.leftMouseButtonDown) 
                    {
                        pixels[inputs.mouseX + inputs.mouseY * WIDTH] = 0xFFFFFF;
                    }
                }
                while (sdl.FlipFrame(pixels));
            }
        }
    }
}