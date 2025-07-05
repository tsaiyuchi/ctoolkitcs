using System;
using System.Collections;
using System.Collections.Generic;

namespace CToolkitCs.v1_2.Modbus
{
    /// <summary>
    /// Modbus TCP common driver class. This class implements a modbus TCP master driver.
    /// It supports the following commands:
    /// 
    /// Read coils
    /// Read discrete inputs
    /// Write single coil
    /// Write multiple cooils
    /// Read holding register
    /// Read input register
    /// Write single register
    /// Write multiple register
    /// 
    /// All commands can be sent in synchronous or asynchronous mode. If a value is accessed
    /// in synchronous mode the program will stop and wait for slave to response. If the 
    /// slave didn't answer within a specified time a timeout exception is called.
    /// The class uses multi threading for both synchronous and asynchronous access. For
    /// the communication two lines are created. This is necessary because the synchronous
    /// thread has to wait for a previous command to finish.
    /// 
    /// </summary>
    public class CtkModbusMessage : IDisposable
    {

        #region Const

        /// <summary> 0x86=134, error code=1~4 </summary>
        public const byte SlaveDeviceFailure = 0x86;

        /// <summary>Constant for exception acknowledge.</summary>
        public const byte excAck = 5;

        /// <summary>Constant for exception connection lost.</summary>
        public const byte excExceptionConnectionLost = 254;

        /// <summary>Constant for exception not connected.</summary>
        public const byte excExceptionNotConnected = 253;

        /// <summary>Constant for exception wrong offset.</summary>
        public const byte excExceptionOffset = 128;

        /// <summary>Constant for exception response timeout.</summary>
        public const byte excExceptionTimeout = 255;

        /// <summary>Constant for exception gate path unavailable.</summary>
        public const byte excGatePathUnavailable = 10;

        /// <summary>Constant for exception illegal data address.</summary>
        public const byte excIllegalDataAdr = 2;

        /// <summary>Constant for exception illegal data value.</summary>
        public const byte excIllegalDataVal = 3;

        /// <summary>Constant for exception illegal function.</summary>
        public const byte excIllegalFunction = 1;

        /// <summary>Constant for exception send failt.</summary>
        public const byte excSendFailt = 100;

        /// <summary>Constant for exception slave device failure.</summary>
        public const byte excSlaveDeviceFailure = 4;

        /// <summary>Constant for exception slave is busy/booting up.</summary>
        public const byte excSlaveIsBusy = 6;

        public const byte fcReadCoil = 1;
        public const byte fcReadDiscreteInputs = 2;
        public const byte fcReadHoldingRegister = 3;
        public const byte fcReadInputRegister = 4;
        public const byte fctReadWriteMultipleRegister = 23;
        public const byte fcWriteMultipleCoils = 15;
        public const byte fcWriteMultipleRegister = 16;
        public const byte fcWriteSingleCoil = 5;
        public const byte fcWriteSingleRegister = 6;
        

        #endregion




        public byte[] dataBytes = new byte[0];

        public byte funcCode;

        public bool isResponse = false;

        public ushort msgLength;

        /// <summary> usually=0, high byte 1st ; low byte 2nd </summary>
        public ushort protocolId;

        /// <summary> Start Address/Reference Number </summary>
        public ushort readAddress;

        /// <summary> Length of read bytes </summary>
        public ushort readLength;

        /// <summary> 流水號: high byte 1st ; low byte 2nd </summary>
        public ushort transactionId;

        public byte unitId;

        //Slave Address
        /// <summary> Start Address/Reference Number </summary>
        public ushort writeAddress;

        //include unit id and function code
        /// <summary> Length of write bytes </summary>
        public ushort writeLength;

        // ------------------------------------------------------------------------
        /// <summary>Create master instance without parameters.</summary>
        public CtkModbusMessage()
        {
        }

        // ------------------------------------------------------------------------
        /// <summary>Create master instance with parameters.</summary>
        /// <param name="ip">IP adress of modbus slave.</param>
        /// <param name="port">Port number of modbus slave. Usually port 502 is used.</param>
        ~CtkModbusMessage()
        {
            this.Dispose(false);
        }



        #region IDisposable
        // Flag: Has Dispose already been called?
        bool disposed = false;

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                //
            }

            // Free any unmanaged objects here.
            //

            this.DisposeSelf();

            disposed = true;
        }





        void DisposeSelf()
        {

        }

        #endregion
        public static CtkModbusMessage FromRequestBytes(byte[] buffer, int offset = 0, int length = -1)
        {
            var msg = new CtkModbusMessage();
            msg.LoadRequestBytes(buffer, offset, length);
            return msg;
        }

        public static CtkModbusMessage FromResponseBytes(byte[] buffer, int offset = 0, int length = -1)
        {
            var msg = new CtkModbusMessage();
            msg.LoadResponseBytes(buffer, offset, length);
            return msg;
        }

        public static bool GetMessageLength(List<byte> buffer, out ushort length)
        {
            length = 0;
            if (buffer.Count < 6) return false;

            var lens = new byte[2];
            lens[0] = buffer[5];
            lens[1] = buffer[4];
            length = BitConverter.ToUInt16(lens, 0);
            return true;
        }


        public BitArray DataToBitArray()
        {
            var rtn = new BitArray(this.dataBytes);
            return rtn;
        }
        public List<Int16> DataToListOfInt16()
        {
            var list = new List<Int16>();
            for (int idx = 0; idx < this.dataBytes.Length; idx += 2)
            {
                if (idx + 1 >= this.dataBytes.Length) break;
                list.Add(NetworkToHostOrder(BitConverter.ToInt16(this.dataBytes, idx)));
            }
            return list;
        }
        public List<UInt16> DataToListOfUInt16()
        {
            var list = new List<UInt16>();
            for (int idx = 0; idx < this.dataBytes.Length; idx += 2)
            {
                if (idx + 1 >= this.dataBytes.Length) break;
                list.Add(NetworkToHostOrder(BitConverter.ToUInt16(this.dataBytes, idx)));
            }
            return list;
        }


        public void LoadRequestBytes(byte[] buffer, int offset = 0, int length = -1)
        {
            this.isResponse = false;

            if (length < 0) length = buffer.Length - offset;
            var myBuffer = new byte[length];
            Array.Copy(buffer, offset, myBuffer, 0, length);



            this.transactionId = NetworkToHostOrder(BitConverter.ToUInt16(myBuffer, 0));
            this.protocolId = NetworkToHostOrder(BitConverter.ToUInt16(myBuffer, 2));
            this.msgLength = NetworkToHostOrder(BitConverter.ToUInt16(myBuffer, 4));
            this.unitId = myBuffer[6];
            this.funcCode = myBuffer[7];


            byte valLen = 0;

            switch (this.funcCode)
            {
                case CtkModbusMessage.fcReadCoil:
                case CtkModbusMessage.fcReadDiscreteInputs:
                case CtkModbusMessage.fcReadHoldingRegister:
                case CtkModbusMessage.fcReadInputRegister:
                    this.readAddress = NetworkToHostOrder(BitConverter.ToUInt16(myBuffer, 8));
                    this.readLength = NetworkToHostOrder(BitConverter.ToUInt16(myBuffer, 10));
                    break;

                case CtkModbusMessage.fcWriteMultipleCoils:
                case CtkModbusMessage.fcWriteMultipleRegister:
                    this.writeAddress = NetworkToHostOrder(BitConverter.ToUInt16(myBuffer, 8));
                    this.writeLength = NetworkToHostOrder(BitConverter.ToUInt16(myBuffer, 10));

                    valLen = myBuffer[12];
                    System.Diagnostics.Debug.Assert(myBuffer.Length == valLen + 12);
                    this.dataBytes = new byte[valLen];
                    Array.Copy(myBuffer, 13, this.dataBytes, 0, valLen);


                    break;

                case CtkModbusMessage.fctReadWriteMultipleRegister:
                    this.readAddress = NetworkToHostOrder(BitConverter.ToUInt16(myBuffer, 8));
                    this.readLength = NetworkToHostOrder(BitConverter.ToUInt16(myBuffer, 10));
                    this.writeAddress = NetworkToHostOrder(BitConverter.ToUInt16(myBuffer, 12));
                    this.writeLength = NetworkToHostOrder(BitConverter.ToUInt16(myBuffer, 14));

                    valLen = myBuffer[16];
                    System.Diagnostics.Debug.Assert(myBuffer.Length == valLen + 16);
                    this.dataBytes = new byte[valLen];
                    Array.Copy(myBuffer, 17, this.dataBytes, 0, valLen);

                    break;
            }



        }

        public void LoadResponseBytes(byte[] buffer, int offset = 0, int length = -1)
        {
            this.isResponse = true;

            if (length < 0) length = buffer.Length - offset;
            var myBuffer = new byte[length];
            Array.Copy(buffer, offset, myBuffer, 0, length);



            this.transactionId = NetworkToHostOrder(BitConverter.ToUInt16(myBuffer, 0));
            this.protocolId = NetworkToHostOrder(BitConverter.ToUInt16(myBuffer, 2));
            this.msgLength = NetworkToHostOrder(BitConverter.ToUInt16(myBuffer, 4));
            this.unitId = myBuffer[6];
            this.funcCode = myBuffer[7];



            switch (this.funcCode)
            {
                case CtkModbusMessage.fcReadCoil:
                case CtkModbusMessage.fcReadDiscreteInputs:
                case CtkModbusMessage.fcReadHoldingRegister:
                case CtkModbusMessage.fcReadInputRegister:
                    this.readLength = myBuffer[8];//若讀取Coil, 無法得知當初要求的Lenght, 其回傳 8bit=1byte
                    this.dataBytes = new byte[this.readLength];
                    Array.Copy(myBuffer, 9, this.dataBytes, 0, this.readLength);
                    break;
                case CtkModbusMessage.fcWriteSingleCoil:
                case CtkModbusMessage.fcWriteSingleRegister:
                case CtkModbusMessage.fcWriteMultipleCoils:
                case CtkModbusMessage.fcWriteMultipleRegister:
                case CtkModbusMessage.fctReadWriteMultipleRegister:


                    break;
            }



        }

        public byte[] ToRequestBytes()
        {
            //Big-Endian (same like SECS)
            var buffer = new List<byte>();


            //buffer[0] + [1]
            buffer.AddRange(HostToNetworkOrderBytes(this.transactionId));
            //buffer[2] + [3]
            buffer.AddRange(HostToNetworkOrderBytes(this.protocolId));

            var preDataBytes = new List<byte>();
            preDataBytes.Add(this.unitId);//buffer[6]
            preDataBytes.Add(this.funcCode);//buffer[7]

            var datas = new byte[0];

            System.Diagnostics.Debug.Assert(this.funcCode > 0);//function code需要是有定義的值

            switch (funcCode)
            {
                case CtkModbusMessage.fcReadCoil:
                case CtkModbusMessage.fcReadDiscreteInputs:
                case CtkModbusMessage.fcReadHoldingRegister:
                case CtkModbusMessage.fcReadInputRegister:
                    System.Diagnostics.Debug.Assert(this.dataBytes.Length == 0);//不應該有值要寫入
                    preDataBytes.AddRange(HostToNetworkOrderBytes(this.readAddress));//buffer[8] [9]
                    preDataBytes.AddRange(HostToNetworkOrderBytes(this.readLength));// buffer[10] [11]
                    break;
                case CtkModbusMessage.fcWriteSingleCoil:
                    preDataBytes.AddRange(HostToNetworkOrderBytes(this.writeAddress));//buffer[8] [9]
                                                                                      //Single不帶Length

                    // Example: UID=1, Coil=3
                    // 00 03 00 00 00 06 01 05 00 03 FF 00 = On (0xFF, 0x00)
                    // 00 03 00 00 00 06 01 05 00 03 00 00 = Off (0x00, 0x00)

                    break;
                case CtkModbusMessage.fcWriteSingleRegister:
                    preDataBytes.AddRange(HostToNetworkOrderBytes(this.writeAddress));//buffer[8] [9]
                    //Single不帶Length

                    // Example: UID=1, Register=3
                    // 00 05 00 00 00 06 01 06 00 03 00 0C = 寫入12

                    break;
                case CtkModbusMessage.fcWriteMultipleCoils:
                case CtkModbusMessage.fcWriteMultipleRegister:
                    this.writeLength = (ushort)Math.Ceiling(this.dataBytes.Length / 2.0);
                    //System.Diagnostics.Debug.Assert(this.writeLength * 2 == this.values.Length);//在寫入資料時, Word Count(address length) * 2 要等同於 values bytes 量
                    preDataBytes.AddRange(HostToNetworkOrderBytes(this.writeAddress));//buffer[8] [9]
                    preDataBytes.AddRange(HostToNetworkOrderBytes(this.writeLength));// buffer[10] [11]
                    preDataBytes.Add((byte)this.dataBytes.Length);//buffer[12]
                    break;

                case CtkModbusMessage.fctReadWriteMultipleRegister:
                    this.writeLength = (ushort)Math.Ceiling(this.dataBytes.Length / 2.0);

                    preDataBytes.AddRange(HostToNetworkOrderBytes(this.readAddress));//buffer[8] [9]
                    preDataBytes.AddRange(HostToNetworkOrderBytes(this.readLength));// buffer[10] [11]
                    preDataBytes.AddRange(HostToNetworkOrderBytes(this.writeAddress));//buffer[12] [13]
                    preDataBytes.AddRange(HostToNetworkOrderBytes(this.writeLength));// buffer[14] [15]
                    preDataBytes.Add((byte)this.dataBytes.Length);//buffer[16]

                    break;
            }


            preDataBytes.AddRange(this.dataBytes);//先加資料再計算長度
            this.msgLength = (ushort)(preDataBytes.Count);//include unit id and function code 
            //buffer[4] + [5]
            buffer.AddRange(HostToNetworkOrderBytes(this.msgLength));
            buffer.AddRange(preDataBytes);
            return buffer.ToArray();
            //T:00 01   00 00   00 06   01 06 05 DC 00 12 
        }



        #region Byte Transfer Method

        internal static UInt16 HostToNetworkOrder(UInt16 value)
        {
            if (BitConverter.IsLittleEndian)
                return SwapUInt16(value);
            return value;
        }
        internal static Int16 HostToNetworkOrder(Int16 value)
        {
            //Equal: IPAddress.HostToNetworkOrder(value);
            if (BitConverter.IsLittleEndian)
                return SwapInt16(value);
            return value;
        }
        internal static byte[] HostToNetworkOrderBytes(UInt16 value) { return BitConverter.GetBytes(HostToNetworkOrder(value)); }
        internal static byte[] HostToNetworkOrderBytes(Int16 value) { return BitConverter.GetBytes(HostToNetworkOrder(value)); }
        internal static UInt16 NetworkToHostOrder(UInt16 value)
        {
            if (BitConverter.IsLittleEndian)
                return SwapUInt16(value);
            return value;
        }
        internal static Int16 NetworkToHostOrder(Int16 value)
        {
            //Equal: IPAddress.NetworkToHostOrder(value);
            if (BitConverter.IsLittleEndian)
                return SwapInt16(value);
            return value;
        }
        internal static Int16 SwapInt16(Int16 inValue)
        {
            return (Int16)(((inValue & 0xff00) >> 8) |
                     ((inValue & 0x00ff) << 8));
        }
        internal static UInt16 SwapUInt16(UInt16 inValue)
        {
            return (UInt16)(((inValue & 0xff00) >> 8) |
                     ((inValue & 0x00ff) << 8));
        }

        #endregion





        #region Static

        protected static ushort _incrementTransactionId = 1;
        public static ushort IncrementTransactionId { get { return _incrementTransactionId++; } }

        public static CtkModbusMessage CreateReadCoil(ushort address, ushort length, byte unitId = 1, ushort? tid = null)
        {
            var msg = new CtkModbusMessage()
            {
                transactionId = tid.HasValue ? tid.Value : IncrementTransactionId,
                unitId = unitId,
                funcCode = fcReadCoil,
                readAddress = address,
                readLength = length,
            };

            return msg;
        }
        public static CtkModbusMessage CreateReadHoldingRegister(ushort address, ushort length, byte unitId = 1, ushort? tid = null)
        {
            var msg = new CtkModbusMessage()
            {
                transactionId = tid.HasValue ? tid.Value : IncrementTransactionId,
                unitId = unitId,
                funcCode = fcReadHoldingRegister,
                readAddress = address,
                readLength = length,
            };

            return msg;
        }

        public static CtkModbusMessage CreateWriteSingleCoil(ushort address, bool isOn, byte unitId = 1, ushort? tid = null)
        {
            var msg = new CtkModbusMessage()
            {
                transactionId = tid.HasValue ? tid.Value : IncrementTransactionId,
                unitId = unitId,
                funcCode = fcWriteSingleCoil,

                writeAddress = address,
            };

            if (isOn)
                msg.dataBytes = new byte[] { 0xff, 0x00 };
            else
                msg.dataBytes = new byte[] { 0x00, 0x00 };

            return msg;
        }
        public static CtkModbusMessage CreateWriteSingleRegister(ushort address, ushort value, byte unitId = 1, ushort? tid = null)
        {
            var msg = new CtkModbusMessage()
            {
                transactionId = tid.HasValue ? tid.Value : IncrementTransactionId,
                unitId = unitId,
                funcCode = fcWriteSingleRegister,
                writeAddress = address,
                dataBytes = HostToNetworkOrderBytes(value),
            };

            return msg;
        }
        public static CtkModbusMessage CreateWriteSingleRegister(ushort address, short value, byte unitId = 1, ushort? tid = null)
        {
            var msg = new CtkModbusMessage()
            {
                transactionId = tid.HasValue ? tid.Value : IncrementTransactionId,
                unitId = unitId,
                funcCode = fcWriteSingleRegister,
                writeAddress = address,
                dataBytes = HostToNetworkOrderBytes(value),
            };

            return msg;
        }

        #endregion



    }
}
