#ifndef INC_PLAYER_TIMER
#define INC_PLAYER_TIMER

class Timer {
public:
    static void begin(void (*)());

private:
    // 44.1 kHz frequency.
    static constexpr double sampleRate = 44100.0;

    // Microseconds for each sample.
    // That gives us ~22.67us per interrupt or ~4081 cycles at 180 MHz.
    static constexpr double sampleTicks = (1.0 / sampleRate) * 1000000UL;
};

#endif
