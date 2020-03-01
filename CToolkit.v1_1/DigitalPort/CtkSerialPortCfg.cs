using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace CToolkit.v1_1.DigitalPort
{
    [Serializable]
    public class CtkSerialPortCfg
    {
        public string PortName = "COM1";
        public int BaudRate = 9600;
        public Parity Parity = Parity.None;
        public StopBits StopBits = StopBits.One;
        public int DataBits = 8;
        public Handshake Handshake = Handshake.None;
        public bool RtsEnable = true;
    }
}
