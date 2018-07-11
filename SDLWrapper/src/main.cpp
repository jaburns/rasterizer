#include "SDLWrapper.hpp"

#define DLL_EXPORT extern "C" __declspec(dllexport)

DLL_EXPORT SDLWrapper *sdlw_create(const char *title, int width, int height, int scale)
{
    return new SDLWrapper(title, width, height, scale);
}

DLL_EXPORT bool sdlw_flip_frame(SDLWrapper *instance, const uint32_t *pixels)
{
    if (instance == nullptr) return false;

    return instance->flip_frame(pixels);
}

DLL_EXPORT void sdlw_read_input_state(SDLWrapper *instance, InputState *out)
{
    if (instance == nullptr) return;

    *out = instance->get_input_state();
}

DLL_EXPORT void sdlw_delete(SDLWrapper *instance)
{
    if (instance == nullptr) return;

    delete instance;
}