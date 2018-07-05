#include <SDL.h>
#include <memory>

#define DLL_EXPORT extern "C" __declspec(dllexport)

struct WrapperState
{
    SDL_Window *window;
    SDL_Renderer *renderer;
    SDL_Texture *texture;

    int width, height, scale;
};

static WrapperState s_state;

DLL_EXPORT void sdlw_init(const char *title, int width, int height, int scale)
{
    SDL_Init(SDL_INIT_VIDEO);

    s_state.width = width;
    s_state.height = height;
    s_state.scale = scale;

    s_state.window = SDL_CreateWindow(title, SDL_WINDOWPOS_UNDEFINED, SDL_WINDOWPOS_UNDEFINED, width*scale, height*scale, 0);
    s_state.renderer = SDL_CreateRenderer(s_state.window, -1, 0);
    s_state.texture = SDL_CreateTexture(s_state.renderer, SDL_PIXELFORMAT_ARGB8888, SDL_TEXTUREACCESS_STREAMING, width, height);
}

DLL_EXPORT bool sdlw_flip_frame(const uint32_t *pixels)
{
    uint32_t *sdl_pixels;
    int pitch = s_state.width * sizeof(uint32_t);
    SDL_LockTexture(s_state.texture, nullptr, reinterpret_cast<void**>(&sdl_pixels), &pitch);
    std::memcpy(sdl_pixels, pixels, pitch * s_state.height);
    SDL_UnlockTexture(s_state.texture);

    bool running = true;
    SDL_Event event;
    while (SDL_PollEvent(&event)) {
        running &= event.type == SDL_QUIT;
    }

    SDL_RenderCopy(s_state.renderer, s_state.texture, nullptr, nullptr);
    SDL_RenderPresent(s_state.renderer);

    return running;
}

DLL_EXPORT void sdlw_quit()
{
    SDL_DestroyTexture(s_state.texture);
    SDL_DestroyRenderer(s_state.renderer);
    SDL_DestroyWindow(s_state.window);
    SDL_Quit();
}