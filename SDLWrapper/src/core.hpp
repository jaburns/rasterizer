#pragma once

#include <SDL.h>
#include <string>
#include <memory>

class ScreenBufferView
{
    int m_width, m_height;
    uint32_t *m_raw_texture;

public:
    ScreenBufferView(int width, int height, uint32_t *raw_texture);

    void set_pixel(int x, int y, uint32_t color) const;
    int width() const;
    int height() const;

    void destroy();
};

class Core
{
    SDL_Window *m_window;
    SDL_Renderer *m_renderer;
    SDL_Texture *m_texture;

    int m_width, m_height, m_scale;
    std::shared_ptr<ScreenBufferView> m_cur_screen_writer;

public:
    Core(const std::string& title, int width, int height, int scale);
    ~Core();

    Core(const Core&) = delete;
    Core& operator=(const Core&) = delete;

    std::shared_ptr<ScreenBufferView> get_screen_buffer_view();
    bool flip_frame_and_poll_events();
};