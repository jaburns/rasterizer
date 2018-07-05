#include "core.hpp"

#define DLL_EXPORT extern "C" __declspec(dllexport)

DLL_EXPORT void run_it()
{
    Core core("Scope Soft Renderer", 400, 300, 2);

    do {
        auto screen = core.get_screen_buffer_view();
        //write_random_pixel(*screen);
    }
    while (core.flip_frame_and_poll_events());
}
