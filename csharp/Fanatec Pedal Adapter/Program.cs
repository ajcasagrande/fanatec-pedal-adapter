using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Ports;

namespace Fanatec_Pedal_Adapter
{
    class Program
    {
        private const int BAUD_RATE = 115200;

        public static Arduino Arduino
        {
            get; set;
        }

        static void Main(string[] args)
        {
            Arduino = new Arduino("COM3", BAUD_RATE);
            Arduino.Connect();

            FanatecPedals pedals = new FanatecPedals();
            pedals.StartProcessingThread();

            Console.WriteLine();
            while (true)
            {
                try
                {
                    Console.Write("command> ");
                    string input = Console.ReadLine();
                    string[] tokens = input.Split();
                    string cmd = tokens[0].ToLower();
                    switch (cmd)
                    {
                        case "port":
                            string port = tokens[1];
                            Console.WriteLine(port);
                            Arduino.Disconnect();
                            Arduino = new Arduino(port, BAUD_RATE);
                            Arduino.Connect();
                            break;
                        case "hb":
                            string param = tokens[1].ToLower();
                            bool enable = param == "on" || param == "enable";
                            pedals.EnableHandbrake = enable;
                            Console.WriteLine("Success: Handbrake {0}", pedals.EnableHandbrake);
                            break;
                        case "save":
                            // TODO: Save the state for next time
                            break;
                        case "status":
                            Console.WriteLine("Arduino -- Connected: {0}, Port: {1}, Baud Rate: {2}", 
                                Arduino.IsConnected(), Arduino.serialPort, Arduino.baudRate);
                            Console.WriteLine("Pedals -- Connected: {0}, Handbrake: {1}", pedals.IsConnected(), pedals.EnableHandbrake);
                            break;
                        case "help":
                            PrintHelp();
                            break;
                        default:
                            Console.WriteLine("Error, unrecognized command!");
                            break;
                    }
                }
                catch { }
            }
        }

        static void PrintHelp()
        {
            Console.WriteLine("\nCommands\n");
            Console.WriteLine("port PORT_NAME \t\t Set the arduino port");
            Console.WriteLine("hb [on/off] \t\t Enable or disable the handbrake (over clutch pin)");
            Console.WriteLine("status \t\t\t Print the connected status of Arduino and Pedals");
            Console.WriteLine("help \t\t\t Print this help message");
            Console.WriteLine("");
        }
    }
}
