using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fanatec_Pedal_Adapter
{
    class Arduino
    {
        private const int RETRY_MS = 500;
        private const int START_DELAY_MS = 250;
        private const string START_COMMAND = "^START$\n";

        private SerialPort serialPort1 = null;
        public string serialPort { get; private set; }
        public int baudRate { get; private set; }

        public Arduino(string serialPort, int baudRate)
        {
            this.serialPort = serialPort;
            this.baudRate = baudRate;
        }

        public bool Connect()
        {
            if (IsConnected())
            {
                try
                {
                    serialPort1.Close();
                }
                catch { }
            }
            try {
                serialPort1 = new SerialPort();
                serialPort1.PortName = serialPort;
                serialPort1.BaudRate = baudRate;
                serialPort1.Open();
                Thread.Sleep(START_DELAY_MS);
                serialPort1.Write(START_COMMAND);
                Console.WriteLine("Connected to Arduino: {0}, {1}", serialPort, baudRate);
                return IsConnected();
            } catch
            {
                Console.WriteLine("Error: Unable to connect to Arduino: {0}, {1}", serialPort, baudRate);
                serialPort1 = null;
                return false;
            }
        }

        public void Disconnect()
        {
            try
            {
                serialPort1.Close();
            }
            catch { }
        }

        public bool IsConnected()
        {
            return serialPort1 != null && serialPort1.IsOpen;
        }

        public bool Send(byte[] data)
        {
            try
            {
                serialPort1.Write(data, 0, data.Length);
                return true;
            }
            catch
            {
                if (!Connect())
                {
                    Console.WriteLine("Waiting {0} ms to try again.", RETRY_MS);
                    Thread.Sleep(RETRY_MS);
                }
                return false;
            }
        }
    }
}
