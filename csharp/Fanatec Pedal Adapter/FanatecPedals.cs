using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Fanatec_Pedal_Adapter
{
    class FanatecPedals
    {
        private const DeviceType DEFAULT_DEVICE_TYPE = DeviceType.Joystick;
        private const string DEVICE_REGEX = "Fanatec ClubSport Pedals";

        private const double GAS_MAX = 4095.0;
        private const double BRAKE_MAX = 4095.0;
        private const double CLUTCH_MAX = 4095.0;
        private const double HANDBRAKE_MAX = 4095.0;
        // Offset to apply to the bottom values and range
        private const double TRIM_PADDING = 95.0;

        private const int UPDATE_MS = 1;
        private const int RETRY_MS = 500;

        private DirectInput directInput = new DirectInput();
        private Guid joystickGuid = Guid.Empty;
        private Joystick joystick = null;
        private JoystickState state = null;

        public bool EnableHandbrake { get; set; }
        public bool InvertValues
        {
            get; set;
        }

        private bool processThread = false;

        public FanatecPedals()
        {
            EnableHandbrake = false;
            InvertValues = false;
        }

        public void StartProcessingThread()
        {
            processThread = true;
            FindAndConnect();
            Thread thread = new Thread(() =>
            {
                byte[] gas;
                byte[] brake;
                byte[] clutch;

                while (processThread)
                {
                    if (IsConnected())
                    {
                        try
                        {
                            joystick.GetCurrentState(ref state);
                            gas = mapInt(state.X, GAS_MAX);
                            brake = mapInt(state.Y, BRAKE_MAX);
                            if (EnableHandbrake)
                            {
                                clutch = mapInt(state.RotationX, HANDBRAKE_MAX);
                            }
                            else
                            {
                                clutch = mapInt(state.Z, CLUTCH_MAX);
                            }
                            byte[] data = { gas[0], gas[1], brake[0], brake[1], clutch[0], clutch[1] };
                            Program.Arduino.Send(data);
                            Thread.Sleep(UPDATE_MS);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Error: " + e.Message);
                            handlePedalError();
                        }
                    }
                    else
                    {
                        handlePedalError();
                    }
                }
            });
            thread.IsBackground = false;
            thread.Start();
        }

        private void handlePedalError()
        {
            byte[] data = new byte[6];
            // Clear the inputs
            Program.Arduino.Send(data);
            if (!FindAndConnect())
            {
                Console.WriteLine("Waiting {0} ms to try again.", RETRY_MS);
                Thread.Sleep(RETRY_MS);
            }
        }

        public void StopProcessing()
        {
            processThread = false;
        }

        public bool IsConnected()
        {
            return joystick != null;
        }

        public bool FindAndConnect()
        {
            if (joystick != null)
            {
                try { joystick.Unacquire(); } catch { }
                joystick = null;
            }
            try {
                joystickGuid = Guid.Empty;
                foreach (var deviceInstance in directInput.GetDevices(DEFAULT_DEVICE_TYPE, DeviceEnumerationFlags.AllDevices))
                {
                    string name = deviceInstance.ProductName;
                    //Console.WriteLine(name);
                    if (Regex.IsMatch(name, DEVICE_REGEX))
                    {
                        Console.WriteLine("Found Pedals: " + name);
                        joystickGuid = deviceInstance.InstanceGuid;
                        break;
                    }
                }

                if (joystickGuid == Guid.Empty)
                {
                    return UnableToFindPedals();
                }

                joystick = new Joystick(directInput, joystickGuid);
                state = new JoystickState();
                joystick.Properties.AxisMode = DeviceAxisMode.Absolute;
                joystick.Acquire();

                Console.WriteLine("Connected to pedals!");
                return true;
            }
            catch
            {
                return UnableToFindPedals();
            }
        }

        private bool UnableToFindPedals()
        {
            Console.WriteLine("Unable to find pedals!");
            joystickGuid = Guid.Empty;
            joystick = null;
            state = null;
            return false;
        }

        private byte[] mapByte(int value, double newMax)
        {
            if (InvertValues)
            {
                return new byte[] { (byte)(newMax - Math.Round((value / 65535.0) * newMax)) };
            }
            else
            {
                return new byte[] { (byte)(Math.Round((value / 65535.0) * newMax)) };
            }
        }

        private byte[] mapInt(int value, double newMax)
        {
            int val = (int)Math.Round((value / 65535.0) * (newMax - TRIM_PADDING) + TRIM_PADDING);
            if (InvertValues)
            {
                val = (int)newMax - val;
            }
            return new byte[] { (byte)((val >> 8) & 0xff), (byte)(val & 0xff) };
        }

    }
}
