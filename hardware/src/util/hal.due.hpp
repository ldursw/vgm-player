#define SdFatCls SdFat
#define PSG_WE 29
#define PSG_DAC DAC0
#define PSG_ATTENUATION 5
#define FM_IC 28
#define FM_WR 27
#define FM_A0 26
#define FM_A1 25

#include <Arduino.h>

inline __attribute__((always_inline)) void digitalWriteFast(uint8_t pin, uint8_t val)
{
    if (val)
    {
        digitalPinToPort(pin)->PIO_SODR = digitalPinToBitMask(pin);
    }
    else
    {
        digitalPinToPort(pin)->PIO_CODR = digitalPinToBitMask(pin);
    }
}

inline __attribute__((always_inline)) void enableDAC0()
{
    /* Enable clock for DACC_INTERFACE */
    pmc_enable_periph_clk(DACC_INTERFACE_ID);

    /* Reset DACC registers */
    dacc_reset(DACC_INTERFACE);

    /* Half word transfer mode */
    dacc_set_transfer_mode(DACC_INTERFACE, 0);

    /* Power save:
        * sleep mode  - 0 (disabled)
        * fast wakeup - 0 (disabled)
        */
    dacc_set_power_save(DACC_INTERFACE, 0, 0);
    /* Timing:
        * refresh        - 0x08 (1024*8 dacc clocks)
        * max speed mode -    0 (disabled)
        * startup time   - 0x10 (1024 dacc clocks)
        */
    dacc_set_timing(DACC_INTERFACE, 0x08, 0, 0x10);

    /* Set up analog current */
    dacc_set_analog_control(
        DACC_INTERFACE,
        DACC_ACR_IBCTLCH0(0x02) | DACC_ACR_IBCTLCH1(0x02) | DACC_ACR_IBCTLDACCORE(0x01));

    // Enable channel
    if ((dacc_get_channel_status(DACC_INTERFACE) & (1 << 0)) == 0)
    {
        dacc_enable_channel(DACC_INTERFACE, 0);
    }

    /* Disable TAG and select output channel chDACC */
    dacc_set_channel_selection(DACC_INTERFACE, 0);
}

inline __attribute__((always_inline)) void analogWriteDAC0(uint32_t sample)
{
    dacc_write_conversion_data(DACC_INTERFACE, sample);
    // while ((dacc_get_interrupt_status(DACC_INTERFACE) & DACC_ISR_EOC) == 0);
}
