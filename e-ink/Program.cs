//using Iot.Device.EPaper.Drivers.Ssd168x.Ssd1681;
using Iot.Device.EPaper.Enums;
using Iot.Device.EPaper.Fonts;
using Iot.Device.EPaper;
using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using Iot.Device.EPaper.Drivers.Ssd168x;
using Iot.Device.EPaper.Drivers.Ssd168x.Ssd1680;
using nanoFramework.Hardware.Esp32;

namespace e_ink
{
    public class Program
    {
        public static void Main()
        {
            // Create an instance of the GPIO Controller.
            // The display driver uses this to open pins to the display device.
            // You could also pass null to Ssd1681 instead of a GpioController instance and it will make one for you.
            using var gpioController = new GpioController();

            // Setup SPI connection with the display
            var spiConnectionSettings = new SpiConnectionSettings(busId: 1, chipSelectLine: Gpio.IO33)
            {
                ClockFrequency = Ssd1680.SpiClockFrequency,
                Mode = Ssd1680.SpiMode,
                ChipSelectLineActiveState = false,
                Configuration = SpiBusConfiguration.HalfDuplex,
                DataFlow = DataFlow.MsbFirst
            };

            using var spiDevice = new SpiDevice(spiConnectionSettings);

            // Create an instance of the display driver
            using var display = new Ssd1680(
                spiDevice,
                resetPin: Gpio.IO34,
                busyPin: Gpio.IO35,
                dataCommandPin: Gpio.IO32,
                width: 250,
                height: 122,
                gpioController,
                enableFramePaging: true,
                shouldDispose: false);

            // Power on the display and initialize it
            display.PowerOn();
            display.Initialize();

            // clear the display
            display.Clear(triggerPageRefresh: true);

            // initialize the graphics library
            using var gfx = new Graphics(display)
            {
                DisplayRotation = Rotation.Default
            };

            // a simple font to use
            // you can make use your own font by implementing IFont interface
            var font = new Font8x12();

            // write text to the internal graphics buffer
            gfx.DrawText("Hello World", font, x: 0, y: 0, Color.Black);

            // flush the buffer to the display and then initiate the refresh sequence
            display.Flush();
            display.PerformFullRefresh();
            // Done! now put the display to sleep to reduce its power consumption

            display.PowerDown(SleepMode.DeepSleepModeTwo);
        }
    }
}
