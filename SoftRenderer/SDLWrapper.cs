using System;
using System.Runtime.InteropServices;
using GlmSharp;

namespace SoftRenderer
{
    public class SDLWrapper : IDisposable
    {
        public struct InputState
        {
            public ivec2 mousePos;
            public bool leftMouseButtonDown;

            static public readonly InputState Zero = new InputState {
                mousePos = ivec2.Zero,
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
                    mousePos = new ivec2(mouseX, mouseY),
                    leftMouseButtonDown = leftMouseButtonDown
                };
            }
        }

        [DllImport("SDLWrapper.dll", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)]
        private static extern IntPtr sdlw_create(string title, int width, int height, int scale);

        [DllImport("SDLWrapper.dll", CallingConvention=CallingConvention.Cdecl)]
        private static extern bool sdlw_flip_frame(IntPtr instance, uint[] pixels);

        [DllImport("SDLWrapper.dll", CallingConvention=CallingConvention.Cdecl)]
        private static extern void sdlw_read_input_state(IntPtr instance, out InputStateInterop inputs);

        [DllImport("SDLWrapper.dll", CallingConvention=CallingConvention.Cdecl)]
        private static extern void sdlw_delete(IntPtr instance);

        private IntPtr instance = IntPtr.Zero;

        public SDLWrapper(string title, int width, int height, int scale)
        {
            instance = sdlw_create(title, width, height, scale);
        }

        public InputState ReadInputState()
        {
            InputStateInterop inputs;
            sdlw_read_input_state(instance, out inputs);
            return inputs.AsInputState();
        }

        public bool FlipFrame(uint[] pixels)
        {
            return sdlw_flip_frame(instance, pixels);
        }

    #region IDisposable Support

        private bool disposedValue = false; 

        protected virtual void Dispose()
        {
            if (disposedValue) return;
            disposedValue = true;

            sdlw_delete(instance);
            instance = IntPtr.Zero;
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