#include <cstdlib>

#include "core.hpp"

static float rand01()
{
    return static_cast<float>(rand()) / RAND_MAX;
}

static void write_random_pixel(const ScreenBufferView& screen)
{
    screen.set_pixel(
        rand01() * (screen.width() - 1),
        rand01() * (screen.height() - 1),
        rand01() * 0xFFFFFF
    );
}

int main(int argc, char ** argv)
{
    Core core("Scope Soft Renderer", 400, 300, 2);

    do {
        auto screen = core.get_screen_buffer_view();
        write_random_pixel(*screen);
    }
    while (core.flip_frame_and_poll_events());

    return 0;
}