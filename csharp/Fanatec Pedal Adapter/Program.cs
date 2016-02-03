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
        public static Arduino Arduino;

        static void Main(string[] args)
        {
            Arduino = new Arduino("COM9", 115200);
            Arduino.Connect();

            FanatecPedals pedals = new FanatecPedals();
            pedals.StartProcessingThread();
        }
    }
}
