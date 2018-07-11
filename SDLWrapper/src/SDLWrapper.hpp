#pragma once

#include <SDL.h>
#include <cstdint>

struct InputState
{
    int mouse_x = 0;
    int mouse_y = 0;
    bool left_mouse_button_down = false;
};

class SDLWrapper
{
    SDL_Window *window;
    SDL_Renderer *renderer;
    SDL_Texture *texture;

    int width, height, scale;

    InputState input_state;

    bool handle_sdl_event(const SDL_Event& event);

public:
    SDLWrapper(const char *title, int width, int height, int scale);
    ~SDLWrapper();

    SDLWrapper(SDLWrapper const&) = delete;
    SDLWrapper& operator=(SDLWrapper const&) = delete;

    bool flip_frame(const uint32_t *pixels);
    InputState get_input_state();
};