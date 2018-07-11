#include "SDLWrapper.hpp"

#include <memory>

SDLWrapper::SDLWrapper(const char *title, int width, int height, int scale)
{
    SDL_Init(SDL_INIT_VIDEO);

    this->width = width;
    this->height = height;
    this->scale = scale;

    window = SDL_CreateWindow(title, SDL_WINDOWPOS_UNDEFINED, SDL_WINDOWPOS_UNDEFINED, width*scale, height*scale, 0);
    renderer = SDL_CreateRenderer(window, -1, 0);
    texture = SDL_CreateTexture(renderer, SDL_PIXELFORMAT_ARGB8888, SDL_TEXTUREACCESS_STREAMING, width, height);
}

SDLWrapper::~SDLWrapper()
{
    SDL_DestroyTexture(texture);
    SDL_DestroyRenderer(renderer);
    SDL_DestroyWindow(window);
    SDL_Quit();
}

bool SDLWrapper::flip_frame(const uint32_t *pixels)
{
    uint32_t *sdl_pixels;
    int pitch = width * sizeof(uint32_t);
    SDL_LockTexture(texture, nullptr, reinterpret_cast<void**>(&sdl_pixels), &pitch);
    std::memcpy(sdl_pixels, pixels, pitch * height);
    SDL_UnlockTexture(texture);

    bool running = true;
    SDL_Event event;
    while (SDL_PollEvent(&event)) {
        running &= handle_sdl_event(event);
    }

    SDL_RenderCopy(renderer, texture, nullptr, nullptr);
    SDL_RenderPresent(renderer);

    return running;
}

InputState SDLWrapper::get_input_state()
{
    return input_state;
}

bool SDLWrapper::handle_sdl_event(const SDL_Event& event)
{
    switch (event.type)
    {
        case SDL_QUIT:
            return false;

         case SDL_MOUSEBUTTONUP:
            if (event.button.button == SDL_BUTTON_LEFT) {
                input_state.left_mouse_button_down = false;
            }
            break;

        case SDL_MOUSEBUTTONDOWN:
            if (event.button.button == SDL_BUTTON_LEFT) {
                input_state.left_mouse_button_down = true;
            }
            break;

        case SDL_MOUSEMOTION:
            input_state.mouse_x = event.motion.x / scale;
            input_state.mouse_y = event.motion.y / scale;
            break;
    }

    return true;
}
