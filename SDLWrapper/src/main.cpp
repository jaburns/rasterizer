#include <SDL.h>
#include <memory>

#define DLL_EXPORT extern "C" __declspec(dllexport)

struct InputState
{
    int mouse_x = 0;
    int mouse_y = 0;
    bool left_mouse_button_down = false;
};

struct WrapperState
{
    SDL_Window *window;
    SDL_Renderer *renderer;
    SDL_Texture *texture;

    int width, height, scale;

    InputState input_state;
};

static WrapperState s_state;

static bool handle_event(const SDL_Event& event);

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
        running &= handle_event(event);
    }

    SDL_RenderCopy(s_state.renderer, s_state.texture, nullptr, nullptr);
    SDL_RenderPresent(s_state.renderer);

    return running;
}

DLL_EXPORT void sdlw_read_input_state(InputState *out)
{
    *out = s_state.input_state;
}

DLL_EXPORT void sdlw_quit()
{
    SDL_DestroyTexture(s_state.texture);
    SDL_DestroyRenderer(s_state.renderer);
    SDL_DestroyWindow(s_state.window);
    SDL_Quit();
}

static bool handle_event(const SDL_Event& event)
{
    switch (event.type)
    {
        case SDL_QUIT:
            return false;

         case SDL_MOUSEBUTTONUP:
            if (event.button.button == SDL_BUTTON_LEFT) {
                s_state.input_state.left_mouse_button_down = false;
            }
            break;

        case SDL_MOUSEBUTTONDOWN:
            if (event.button.button == SDL_BUTTON_LEFT) {
                s_state.input_state.left_mouse_button_down = true;
            }
            break;

        case SDL_MOUSEMOTION:
            s_state.input_state.mouse_x = event.motion.x / s_state.scale;
            s_state.input_state.mouse_y = event.motion.y / s_state.scale;
            break;
    }

    return true;
}