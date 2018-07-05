using System;
using System.Runtime.InteropServices;

namespace SoftRenderer
{
    public class SDLWrapper : IDisposable
    {
        public struct InputState
        {
            public Vector2I mousePos;
            public bool leftMouseButtonDown;

            static public readonly InputState Zero = new InputState {
                mousePos = Vector2I.Zero,
                leftMouseButtonDown = false
            };
        }

        [StructLayout(LayoutKind.Sequential)]
        struct InputStateInterop
        {
            public int mouseX;
            public int mouseY;

            [MarshalAs(UnmanagedType.I1)] 
            public bool leftMouseButtonDown;

            public InputState AsInputState()
            {
                return new InputState {
                    mousePos = new Vector2I(mouseX, mouseY),
                    leftMouseButtonDown = leftMouseButtonDown
                };
            }
        }

        [DllImport("SDLWrapper.dll", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)]
        private static extern void sdlw_init(string title, int width, int height, int scale);

        [DllImport("SDLWrapper.dll", CallingConvention=CallingConvention.Cdecl)]
        private static extern bool sdlw_flip_frame(uint[] pixels);

        [DllImport("SDLWrapper.dll", CallingConvention=CallingConvention.Cdecl)]
        private static extern void sdlw_read_input_state(out InputStateInterop inputs);

        [DllImport("SDLWrapper.dll", CallingConvention=CallingConvention.Cdecl)]
        private static extern void sdlw_quit();

        public SDLWrapper(string title, int width, int height, int scale)
        {
            sdlw_init(title, width, height, scale);
        }

        public InputState ReadInputState()
        {
            InputStateInterop inputs;
            sdlw_read_input_state(out inputs);
            return inputs.AsInputState();
        }

        public bool FlipFrame(uint[] pixels)
        {
            return sdlw_flip_frame(pixels);
        }

    #region IDisposable Support

        private bool disposedValue = false; 

        protected virtual void Dispose()
        {
            if (disposedValue) return;
            disposedValue = true;

            sdlw_quit();
        }

        ~SDLWrapper() 
        {
            Dispose();
        }

        void IDisposable.Dispose()
        {
            Dispose();
            GC.SuppressFinalize(this);
        }

    #endregion
    }
}