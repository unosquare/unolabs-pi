namespace Unosquare.Labs.WiringPi
{
    using System;
    using System.Runtime.InteropServices;

    public static class Constants
    {
        public const string LibWiringPi = "libwiringPi.so.2.29";

        public enum PinMode
        {
            Input = 0,
            Output = 1,
            PWM = 2,
            Clock = 3
        }

        public enum InternalResistor
        {
            Off = 0,
            PullDown = 1,
            PullUp = 2
        }

        public enum PwmMode
        {
            MarkSign = 0,
            Balanced = 1,
        }
    }

    /// <summary>
    /// http://wiringpi.com/reference/setup/
    /// One of the setup functions must be called at the start of your program or your program will fail to work correctly. 
    /// You may experience symptoms from it simply not working to segfaults and timing issues.
    /// </summary>
    public static class Setup
    {
        /// <summary>
        /// This initialises wiringPi and assumes that the calling program is going to be using the wiringPi pin numbering scheme. 
        /// This is a simplified numbering scheme which provides a mapping from virtual pin numbers 0 through 16 to the real underlying Broadcom GPIO pin numbers. 
        /// See the pins page for a table which maps the wiringPi pin number to the Broadcom GPIO pin number to the physical location on the edge connector.
        /// This function needs to be called with root privileges.
        /// </summary>
        /// <returns></returns>
        [DllImport(Constants.LibWiringPi, EntryPoint = "wiringPiSetup")]
        public static extern int WiringPiSetup();

        /// <summary>
        /// This is identical to above, however it allows the calling programs to use the Broadcom GPIO pin numbers directly with no re-mapping.
        /// As above, this function needs to be called with root privileges, and note that some pins are different from revision 1 to revision 2 boards.
        /// </summary>
        /// <returns></returns>
        [DllImport(Constants.LibWiringPi, EntryPoint = "wiringPiSetupGpio")]
        public static extern int WiringPiSetupGpio();

        /// <summary>
        /// This initialises wiringPi but uses the /sys/class/gpio interface rather than accessing the hardware directly. 
        /// This can be called as a non-root user provided the GPIO pins have been exported before-hand using the gpio program. 
        /// Pin numbering in this mode is the native Broadcom GPIO numbers – the same as wiringPiSetupGpio() above, 
        /// so be aware of the differences between Rev 1 and Rev 2 boards.
        /// </summary>
        /// <returns></returns>
        [DllImport(Constants.LibWiringPi, EntryPoint = "wiringPiSetupSys")]
        public static extern int WiringPiSetupSys();
    }


    /// <summary>
    /// http://wiringpi.com/reference/core-functions/
    /// These functions work directly on the Raspberry Pi and also with external GPIO modules such as GPIO expanders and so on, 
    /// although not all modules support all functions – e.g. the PiFace is pre-configured for its fixed inputs and outputs, 
    /// and the Raspberry Pi has no on-board analog hardware.
    /// </summary>
    public static class CoreFunctions
    {
        [DllImport(Constants.LibWiringPi, EntryPoint = "pinMode")]
        private static extern void SetPinMode(int pin, int mode);

        [DllImport(Constants.LibWiringPi, EntryPoint = "pullUpDnControl")]
        private static extern void SetPinResistorMode(int pin, int mode);

        [DllImport(Constants.LibWiringPi, EntryPoint = "digitalWrite")]
        private static extern void DigitalWrite(int pin, int value);

        [DllImport(Constants.LibWiringPi, EntryPoint = "digitalRead")]
        private static extern int DigitalRead(int pin);

        [DllImport(Constants.LibWiringPi, EntryPoint = "digitalWriteByte")]
        private static extern void DigitalWriteByte(int value);

        [DllImport(Constants.LibWiringPi, EntryPoint = "pwmWrite")]
        private static extern void PwmWrite(int pin, int value);

        [DllImport(Constants.LibWiringPi, EntryPoint = "pwmSetMode")]
        private static extern void SetPwmMode(int mode);

        /// <summary>
        /// This sets the mode of a pin to either INPUT, OUTPUT, PWM_OUTPUT or GPIO_CLOCK. Note that only wiringPi pin 1 (BCM_GPIO 18) supports PWM output and only wiringPi pin 7 (BCM_GPIO 4) supports CLOCK output modes.
        /// This function has no effect when in Sys mode. If you need to change the pin mode, then you can do it with the gpio program in a script before you start your program.
        /// </summary>
        public static void SetPinMode(int pin, Constants.PinMode mode)
        {
            SetPinMode(pin, (int)mode);
        }

        /// <summary>
        /// This sets the pull-up or pull-down resistor mode on the given pin, which should be set as an input. 
        /// Unlike the Arduino, the BCM2835 has both pull-up an down internal resistors. 
        /// The parameter pud should be; PUD_OFF, (no pull up/down), PUD_DOWN (pull to ground) or PUD_UP (pull to 3.3v) 
        /// The internal pull up/down resistors have a value of approximately 50KΩ on the Raspberry Pi.
        /// </summary>
        public static void SetPinResistorMode(int pin, Constants.InternalResistor mode)
        {
            SetPinResistorMode(pin, (int)mode);
        }

        /// <summary>
        /// Writes the value to the PWM register for the given pin. The Raspberry Pi has one on-board PWM pin, pin 1 (BMC_GPIO 18, Phys 12) 
        /// and the range is 0-1024. Other PWM devices may have other PWM ranges.
        /// This function is not able to control the Pi’s on-board PWM when in Sys mode.
        /// </summary>
        public static void SetPwmRegister(int value)
        {
            if (value <= 0) value = 0;
            if (value >= 1024) value = 1024;

            PwmWrite(1, value);
        }

        /// <summary>
        /// Writes the value HIGH or LOW (1 or 0) to the given pin which must have been previously set as an output.
        /// WiringPi treats any non-zero number as HIGH, however 0 is the only representation of LOW.
        /// </summary>
        public static void WriteBit(int pin, bool value)
        {
            DigitalWrite(pin, value ? 1 : 0);
        }

        /// <summary>
        /// This function returns the value read at the given pin. 
        /// It will be HIGH or LOW (1 or 0) depending on the logic level at the pin.
        /// </summary>
        public static bool ReadBit(int pin)
        {
            return DigitalRead(pin) != 0 ? true : false;
        }

        /// <summary>
        /// This writes the 8-bit byte supplied to the first 8 GPIO pins. It’s the fastest way to set all 8 bits at once to a particular value, 
        /// although it still takes two write operations to the Pi’s GPIO hardware.
        /// </summary>
        public static void WriteByte(byte value)
        {
            DigitalWriteByte(Convert.ToInt32(value));
        }

        /// <summary>
        /// This writes the given value to the supplied analog pin. You will need to register additional analog modules to enable this function for devices such as the Gertboard.
        /// </summary>
        [DllImport(Constants.LibWiringPi, EntryPoint = "analogWrite")]
        private static extern void AnalogWrite(int pin, int value);

        /// <summary>
        /// This returns the value read on the supplied analog input pin. You will need to register additional analog modules to enable this 
        /// function for devices such as the Gertboard, quick2Wire analog board, etc.
        /// </summary>
        [DllImport(Constants.LibWiringPi, EntryPoint = "analogRead")]
        private static extern int AnalogRead(int pin);

        /// <summary>
        /// The PWM generator can run in 2 modes – “balanced” and “mark:space”. The mark:space mode is traditional, however the default mode in the Pi is “balanced”. 
        /// You can switch modes by supplying the parameter: PWM_MODE_BAL or PWM_MODE_MS.
        /// </summary>
        public static void SetPwmMode(Constants.PwmMode mode)
        {
            SetPwmMode((int)mode);
        }

        /// <summary>
        /// This sets the range register in the PWM generator. The default is 1024.
        /// </summary>
        [DllImport(Constants.LibWiringPi, EntryPoint = "pwmSetRange")]
        public static extern void SetPwmRange(uint range);

        /// <summary>
        /// This sets the divisor for the PWM clock.
        /// Note: The PWM control functions can not be used when in Sys mode. To understand more about the PWM system, 
        /// you’ll need to read the Broadcom ARM peripherals manual.
        /// </summary>
        [DllImport(Constants.LibWiringPi, EntryPoint = "pwmSetClock")]
        public static extern void SetPwmClock(int divisor);


    }
}