#include "core.hpp"
#include <cstdint>
#include <chrono>

ScreenBufferView::ScreenBufferView(int width, int height, uint32_t *raw_texture)
    : m_width(width), m_height(height), m_raw_texture(raw_texture)
{ }

void ScreenBufferView::set_pixel(int x, int y, uint32_t color) const
{
    m_raw_texture[m_width*y + x] = color;
}

int ScreenBufferView::width() const
{
    return m_width;
}

int ScreenBufferView::height() const
{
    return m_height;
}

void ScreenBufferView::destroy()
{
    m_raw_texture = nullptr;
}

Core::Core(const std::string& title, int width, int height, int scale)
    : m_width(width), m_height(height), m_scale(scale), m_cur_screen_writer(nullptr)
{
    SDL_Init(SDL_INIT_VIDEO);

    m_window = SDL_CreateWindow(title.c_str(), SDL_WINDOWPOS_UNDEFINED, SDL_WINDOWPOS_UNDEFINED, width*scale, height*scale, 0);
    m_renderer = SDL_CreateRenderer(m_window, -1, 0);
    m_texture = SDL_CreateTexture(m_renderer, SDL_PIXELFORMAT_ARGB8888, SDL_TEXTUREACCESS_STREAMING, width, height);
}

std::shared_ptr<ScreenBufferView> Core::get_screen_buffer_view()
{
    uint32_t *pixels;
    int pitch = m_width * sizeof(uint32_t);

    if (m_cur_screen_writer != nullptr) {
        throw std::logic_error("Cannot call Core::get_screen_buffer_view() multiple times per frame.");
    }

    SDL_LockTexture(m_texture, nullptr, reinterpret_cast<void**>(&pixels), &pitch);

    m_cur_screen_writer = std::make_shared<ScreenBufferView>(m_width, m_height, pixels);

    return m_cur_screen_writer;
}

bool Core::flip_frame_and_poll_events()
{
    SDL_UnlockTexture(m_texture);

    m_cur_screen_writer->destroy();
    m_cur_screen_writer.reset();

    bool quit = false;
    SDL_Event event;
    while (SDL_PollEvent(&event)) {
        quit |= event.type == SDL_QUIT;
    }

    SDL_RenderCopy(m_renderer, m_texture, nullptr, nullptr);
    SDL_RenderPresent(m_renderer);

    return !quit;
}

Core::~Core()
{
    SDL_DestroyTexture(m_texture);
    SDL_DestroyRenderer(m_renderer);
    SDL_DestroyWindow(m_window);
    SDL_Quit();
}