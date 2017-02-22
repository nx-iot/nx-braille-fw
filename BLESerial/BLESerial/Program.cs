using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Runtime.Serialization;

namespace BLESerial
{
    class Program : IDisposable
    {

        class RestartException : Exception
        {
            public RestartException()
                : base()
            {
            }
            public RestartException(string message)
                : base(message)
            {
            }
            public RestartException(string message, Exception innerException)
                : base(message, innerException)
            {
            }
            protected RestartException(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {
            }
        }

        static bool _continue;
        static SerialPort _serialPort;

        public class DataSerialBuffer {
            public byte[] values;
            public int length;
            public int idx_read;
            public int idx_write;
            public DataSerialBuffer()
            { 
                values = new byte[1024];
                length = 0;
                idx_read = 0;
                idx_write = 0;
            }
        }

        public class BLEComportConfig { 
            public String PortName;
            public int BaudRate;
            public Parity Parity;
            public int DataBits;
            public StopBits StopBits;
            public Handshake Handshake;

            // Set the read/write timeouts
            public int ReadTimeout;
            public int WriteTimeout;

            public BLEComportConfig()
            { 
            
            }
        }

        

        public class BLEInfo
        {

            public String factoryID;
            public String iBeaconID;
            public String majorValue;
            public String minorValue;
            public String measuredPwr;
            public String macAdr;
            public String rssi;

            

            public BLEInfo()
            {
            }

            public override string ToString()
            {
                return "BLEInfo";
            }
        }

        static DataSerialBuffer dsbuff;

        static BLEComportConfig BLEComportConfigCurrent;
        static List<BLEInfo> BLEPeripheralTable = new List<BLEInfo>();

        static public int rssiRangeConnect = -50;
        static Thread readThread;
        static Network netServer;
        static Thread netThread;

        static void Main(string[] args)
        {
            
            BLEComportConfigCurrent = new BLEComportConfig();
            dsbuff = new DataSerialBuffer();
            

            StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
            //Thread readThread = new Thread(Read);

            // Create a new SerialPort object with default settings.
            //_serialPort = new SerialPort();


            
            //netServer = new Network();

            //netThread = new Thread(new ThreadStart(netServer.ListenForClients));
            //netThread.Start();

            //byte[] buffer = new byte[] { (byte)'A', (byte)'B', (byte)'C', (byte)'D' };
            //Buffer.BlockCopy(buffer, 0, netServer.writeBuff.values, netServer.writeBuff.idx_write, buffer.Length);
            //netServer.writeBuff.idx_write += buffer.Length;
            //netServer.writeBuff.length += buffer.Length;

            while (true)
            {

                netServer = new Network();

                try
                {
                    netThread = new Thread(new ThreadStart(netServer.ListenForClients));
                    netThread.Start();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Main Network Thread: {0}" ,e);
                }
          
                if (scanBLESerialPort())
                {
                    Console.WriteLine("Found have has central ble device({0}).", BLEComportConfigCurrent.PortName);
       
                    //iBeacon Deploy mode
                    if (setiBeaconDeployMode())
                    {
                        //Scan Peripheral
                        if (getPeripheral(ref BLEPeripheralTable))
                        {
                            for (int i = 0; i < BLEPeripheralTable.Count; i++)
                            {
                                int rssiInt = Int16.Parse(BLEPeripheralTable[i].rssi);
                                if (rssiInt >= rssiRangeConnect)
                                {
                                    if (connectPeripheral(BLEPeripheralTable[i].macAdr))
                                    {

                                    }
                                }
                            }


                        }
                    }
                }
                
                if (readThread.IsAlive)
                {
                    readThread.Abort();
                }

                if (_serialPort.IsOpen)
                {
                    _serialPort.Close();
                }

                if (netThread.IsAlive)
                {
                    netThread.Abort();
                }
            }

            if (readThread.IsAlive)
            {
                readThread.Abort();
            }

            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }

        public void Execute()
        {
            // start new process
            //System.Diagnostics.Process.Start(
            //     Environment.GetCommandLineArgs()[0],
            //     Environment.GetCommandLineArgs()[1]);

            var fileName = System.Reflection.Assembly.GetExecutingAssembly().Location;
            System.Diagnostics.Process.Start(fileName);
        }

        public void Dispose()
        {
            // dispose of any resources used by this instance
            // close current process
            Environment.Exit(0);
        }

        public static Boolean scanBLESerialPort() {
            //string portName;

            //Thread readThread = new Thread(Read);

            Console.WriteLine("Available Ports:");
            if (SerialPort.GetPortNames()==null) {
                return false;
            }
            foreach (string s in SerialPort.GetPortNames())
            {
                Console.WriteLine("   {0}", s);


                try
                {
                    _serialPort = new SerialPort();

                    _serialPort.PortName = s;
                    //_serialPort.BaudRate = 9600;
                    _serialPort.BaudRate = 115200;
                    _serialPort.Parity = Parity.None;
                    _serialPort.DataBits = 8;
                    _serialPort.StopBits = StopBits.One;
                    _serialPort.Handshake = Handshake.None;


                    _serialPort.ReadTimeout = 500;
                    _serialPort.WriteTimeout = 500;


                    _serialPort.Open();

                    dsbuff.idx_read = 0;
                    dsbuff.idx_write = 0;
                    dsbuff.length = 0;

                    try
                    {
                        readThread = new Thread(Read);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error Comport at thread{0} ", e);
                        throw;
                    }

                    _continue = true;
                    readThread.Start();

                    Thread.Sleep(200);

                    Boolean flagAT = false;
                    flagAT = getAT();
                    if (flagAT)
                    {
                        Console.WriteLine("{0} : Find BLE.", s);
                        if (getMacAddress())
                        {
                            if (getModeOperate())
                            {
                                //Update BLE Serial Port Configuration
                                BLEComportConfigCurrent.PortName = s;
                                //BLEComportConfigCurrent.BaudRate = 9600;
                                BLEComportConfigCurrent.BaudRate = 115200;
                                BLEComportConfigCurrent.Parity = Parity.None;
                                BLEComportConfigCurrent.DataBits = 8;
                                BLEComportConfigCurrent.StopBits = StopBits.One;
                                BLEComportConfigCurrent.Handshake = Handshake.None;

                                BLEComportConfigCurrent.ReadTimeout = 500;
                                BLEComportConfigCurrent.WriteTimeout = 500;


                                return true;
                            }
                            else
                            {
                                //return false;
                            }
                        }
                        else
                        {
                            //return false;
                        }
                    }
                    else
                    {

                        Console.WriteLine("{0} : Not Find BLE.", s);
                    }
                 
                    flagAT = getAT();
                    if (flagAT)
                    {
                        Console.WriteLine("{0} : Find BLE.", s);
                        if (getMacAddress())
                        {
                            if (getModeOperate())
                            {
                                //Update BLE Serial Port Configuration
                                BLEComportConfigCurrent.PortName = s;
                                //BLEComportConfigCurrent.BaudRate = 9600;
                                BLEComportConfigCurrent.BaudRate = 115200;
                                BLEComportConfigCurrent.Parity = Parity.None;
                                BLEComportConfigCurrent.DataBits = 8;
                                BLEComportConfigCurrent.StopBits = StopBits.One;
                                BLEComportConfigCurrent.Handshake = Handshake.None;

                                BLEComportConfigCurrent.ReadTimeout = 500;
                                BLEComportConfigCurrent.WriteTimeout = 500;


                                return true;
                            }
                            else
                            {
                                //return false;
                            }
                        }
                        else
                        {
                            //return false;
                        }
                    }
                    else
                    {

                        Console.WriteLine("{0} : Not Find BLE.", s);
                    }

                    if (_continue)
                    {
                        _continue = false;
                    }


                    if (readThread.IsAlive)
                    {
                        readThread.Abort();
                    }

                    if (_serialPort.IsOpen)
                    {
                        _serialPort.Close();
                    }
                }
                catch (System.IO.IOException e1)
                {
                    if (readThread.IsAlive)
                    {
                        readThread.Abort();
                    }

                    if (_serialPort.IsOpen)
                    {
                        _serialPort.Close();
                    }
                    Console.WriteLine("Error Comport {0} ", e1);

                    Environment.Exit(0);
                    var fileName = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    System.Diagnostics.Process.Start(fileName);

                    //System.Diagnostics.Process.Start(
                    // Environment.GetCommandLineArgs()[0],
                    // Environment.GetCommandLineArgs()[1]);

                    

                    return false;
                    //throw;
                }
                catch (System.ArgumentOutOfRangeException e2)
                {
                    if (readThread.IsAlive)
                    {
                        readThread.Abort();
                    }

                    if (_serialPort.IsOpen)
                    {
                        _serialPort.Close();
                    }
                    Console.WriteLine("Error Comport {0} ", e2);

                    Environment.Exit(0);
                    var fileName = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    System.Diagnostics.Process.Start(fileName);
                    

                    //System.Diagnostics.Process.Start(
                    // Environment.GetCommandLineArgs()[0],
                    // Environment.GetCommandLineArgs()[1]);

                    //Environment.Exit(0);

                    return false;
                    //throw;
                }
                catch (Exception ex)
                {
                    if (readThread.IsAlive)
                    {
                        readThread.Abort();
                    }

                    if (_serialPort.IsOpen)
                    {
                        _serialPort.Close();
                    }
                    Console.WriteLine("Error Comport {0} ", ex);
                    //throw;

                    //AssemblyLoadEventArgs.
                    Environment.Exit(0);
                    var fileName = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    System.Diagnostics.Process.Start(fileName);
                    //Environment.Exit(0);
                    //System.Diagnostics.Process.Start(
                    // Environment.GetCommandLineArgs()[0],
                    // Environment.GetCommandLineArgs()[1]);

                    //Environment.Exit(0);
                    
                    return false;
                    //throw;
                }
            }
            return false;
        } 

        public static Boolean getAT() {
            String str_at_cmd = "AT";
            str_at_cmd.Trim();
            Boolean dataAvai = false;

            Boolean reGetAT = false;

            byte[] at_cmd = Encoding.ASCII.GetBytes(str_at_cmd);

            if (_serialPort.IsOpen)
            {
                try
                {
                    _serialPort.Write(at_cmd, 0, at_cmd.Length);
                    Thread.Sleep(200);

                    DataSerialBuffer dsbuffParser = new DataSerialBuffer();

                    dataAvai = bleSerialRead(ref dsbuffParser);

                    byte[] byteBuff = new byte[dsbuffParser.length];
                    Buffer.BlockCopy(dsbuffParser.values, 0, byteBuff, 0, dsbuffParser.length);
                    String strBuff = ToString(byteBuff);

                    if (dataAvai)
                    {
                        if (strBuff.Length == 2)
	                    {
                            if (strBuff.Equals("OK") == true)
                            {
                                Console.WriteLine("{0} : BLE ack AT.", dataAvai);
                                return true;
                            }
                            else
                            {
                                Console.WriteLine("{0} : BLE no ack AT.", dataAvai);
                                return false;
                            }
	                    }
                        else
                        {
                            Console.WriteLine("{0} : Length Over AT at 2Byte : {1}.", dataAvai, strBuff.Length);
                            reGetAT = true;

                            return false;
                        }

                        //if (strBuff.Length  > 2)
                        //{
                        //    if (strBuff.Equals("OK+LOSS") == true)
                        //    {
                        //        Console.WriteLine("{0} : BLE ack AT is loss.", dataAvai);
                        //        return false;
                        //    }
                        //    return false;
                        //}
                        //else
                        //{
                             
                        //    Console.WriteLine("{0} : Length Over AT at 2Byte : {1}.", dataAvai, strBuff.Length);
                        //    return false;
                        //}

                    }
                    else
                    {
                        Console.WriteLine("{0} : BLE no ack AT.", dataAvai);
                        return false;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("{0} : BLE no ack AT.", e);
                    return false;
                }
            }

            //if (_serialPort.IsOpen && reGetAT)
            //{
            //    try
            //    {
            //        _serialPort.Write(at_cmd, 0, at_cmd.Length);
            //        Thread.Sleep(200);

            //        DataSerialBuffer dsbuffParser = new DataSerialBuffer();

            //        dataAvai = bleSerialRead(ref dsbuffParser);

            //        byte[] byteBuff = new byte[dsbuffParser.length];
            //        Buffer.BlockCopy(dsbuffParser.values, 0, byteBuff, 0, dsbuffParser.length);
            //        String strBuff = ToString(byteBuff);

            //        if (dataAvai)
            //        {
            //            if (strBuff.Length == 2)
            //            {
            //                if (strBuff.Equals("OK") == true)
            //                {
            //                    Console.WriteLine("{0} : BLE ack AT.", dataAvai);
            //                    return true;
            //                }
            //                else
            //                {
            //                    Console.WriteLine("{0} : BLE no ack AT.", dataAvai);
            //                    return false;
            //                }
            //            }
            //            else
            //            {
            //                Console.WriteLine("{0} : Length Over AT at 2Byte : {1}.", dataAvai, strBuff.Length);
            //                reGetAT = true;

            //                return false;
            //            }

            //            if (strBuff.Length > 2)
            //            {
            //                if (strBuff.Equals("OK+LOSS") == true)
            //                {
            //                    Console.WriteLine("{0} : BLE ack AT is loss.", dataAvai);
            //                    return false;
            //                }
            //                return false;
            //            }
            //            else
            //            {

            //                Console.WriteLine("{0} : Length Over AT at 2Byte : {1}.", dataAvai, strBuff.Length);
            //                return false;
            //            }

            //        }
            //        else
            //        {
            //            Console.WriteLine("{0} : BLE no ack AT.", dataAvai);
            //            return false;
            //        }
            //    }
            //    catch (Exception e)
            //    {
            //        Console.WriteLine("{0} : BLE no ack AT.", e);
            //        return false;
            //    }
            //}

            
            Console.WriteLine("{0} : BLE no ack AT.", dataAvai);
            return false;
        }

        public static Boolean getMacAddress()
        {
            String str_at_cmd = "AT+ADDR?";
            str_at_cmd.Trim();

            byte[] at_cmd = Encoding.ASCII.GetBytes(str_at_cmd);

            _serialPort.Write(at_cmd, 0, at_cmd.Length);
            Thread.Sleep(200);

            DataSerialBuffer dsbuffParser = new DataSerialBuffer();

            Boolean dataAvai = bleSerialRead(ref dsbuffParser);
            if (dataAvai)
            {
                Console.WriteLine("\nPayload");
                for (int i = 0; i < dsbuffParser.length; i++)
                {
                    Console.Write((char)dsbuffParser.values[i]);
                }

                Console.WriteLine("");


                byte[] byteBuff = new byte[dsbuffParser.length];
                Buffer.BlockCopy(dsbuffParser.values, 0, byteBuff, 0, dsbuffParser.length);
                String strBuff = ToString(byteBuff);

                String[] strArrBuff = strBuff.Split('+');
                if (strArrBuff[0].Equals("OK"))
                {
                    String[] strArrAckMAC = strArrBuff[1].Split(':');
                    if (strArrAckMAC[0].Equals("ADDR"))
                    {
                        byte[] toBytes = Encoding.ASCII.GetBytes(strArrAckMAC[1]);
                        Console.WriteLine("MAC Parser: {0}", strArrAckMAC[1]);
                        return true;
                    }
                }
            }
            Console.WriteLine("BLE no ack {0}", str_at_cmd);
            return false;
        }

        public static Boolean getModeOperate()
        {
            String str_at_cmd = "AT+ROLE?";
            str_at_cmd.Trim();

            byte[] at_cmd = Encoding.ASCII.GetBytes(str_at_cmd);

            _serialPort.Write(at_cmd, 0, at_cmd.Length);
            Thread.Sleep(200);

            DataSerialBuffer dsbuffParser = new DataSerialBuffer();

            Boolean dataAvai = bleSerialRead(ref dsbuffParser);
            if (dataAvai)
            {
                Console.WriteLine("\nPayload");
                for (int i = 0; i < dsbuffParser.length; i++)
                {
                    Console.Write((char)dsbuffParser.values[i]);
                }

                Console.WriteLine("");

                byte[] byteBuff = new byte[dsbuffParser.length];
                Buffer.BlockCopy(dsbuffParser.values, 0, byteBuff, 0, dsbuffParser.length);
                String strBuff = ToString(byteBuff);

                String[] strArrBuff = strBuff.Split('+');
                if (strArrBuff[0].Equals("OK"))
                {
                    String[] strArrKeyValue = strArrBuff[1].Split(':');
                    if (strArrKeyValue[0].Equals("Get"))
                    {
                        byte[] toBytes = Encoding.ASCII.GetBytes(strArrKeyValue[1]);
                        Console.WriteLine("Mode Operation Parser: {0}", strArrKeyValue[1]);
                        if (toBytes[0] == (byte)'1')
                        {
                            Console.WriteLine("Mode : Central");
                            return true;
                        }
                        else if (toBytes[0] == (byte)'0')
                        {
                            Console.WriteLine("Mode : Peripheral");
                            return false;
                        }
                    }
                    else
                    {
                        Console.WriteLine("BLE no ack {0}", str_at_cmd);
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine("BLE no ack {0}", str_at_cmd);
                    return false;
                }
            }
            Console.WriteLine("BLE no ack {0}", str_at_cmd);
            return false;
        }

        public static Boolean getPeripheral(ref List<BLEInfo> BLEInfoArg)
        {
            String str_at_cmd = "AT+DISI?";
            str_at_cmd.Trim();
            BLEInfo BLEInfoBuff = new BLEInfo();
            byte[] at_cmd = Encoding.ASCII.GetBytes(str_at_cmd);

            _serialPort.Write(at_cmd, 0, at_cmd.Length);
            Thread.Sleep(4000);

            DataSerialBuffer dsbuffParser = new DataSerialBuffer();

            Boolean dataAvai = bleSerialRead(ref dsbuffParser);
            if (dataAvai)
            {
                Console.WriteLine("\nPayload");
                for (int i = 0; i < dsbuffParser.length; i++)
                {
                    Console.Write((char)dsbuffParser.values[i]);
                }

                Console.WriteLine("");


                byte[] byteBuff = new byte[dsbuffParser.length];
                Buffer.BlockCopy(dsbuffParser.values, 0, byteBuff, 0, dsbuffParser.length);
                String strBuff = ToString(byteBuff);

                //string[] stringSeparators = new string[] { "OK" };
                //String[] strArrBuff = strBuff.Split(stringSeparators, StringSplitOptions.None);

                
                String[] strArrBuff = strBuff.Split('+');
                if (strArrBuff[0].Equals("OK"))
                {

                    string[] stringSeparators = new string[] { "OK" };
                    strArrBuff = strBuff.Split(stringSeparators, StringSplitOptions.None);
                    List<String[]> arrKeyValue = new List<string[]>();
                    for (int i = 0; i < strArrBuff.Length; i++)
                    {
                        int count = strArrBuff[i].Split(':').Length;
                        if (count == 6)
                        {
                            arrKeyValue.Add(strArrBuff[i].Split(':'));
                        }
                        
                    }

                    foreach (String[] key in arrKeyValue)
	                {
                        Console.WriteLine("{0},{1},{2},{3},{4}", key[1],key[2],key[3],key[4],key[5]);
                        BLEInfoBuff = new BLEInfo();
                        BLEInfoBuff.factoryID = key[1];
                        BLEInfoBuff.iBeaconID = key[2];
                        //BLEInfoBuff.majorValue = key[3].Substring(0, 4);
                        //BLEInfoBuff.minorValue = key[3].Substring(4, 4);
                        //BLEInfoBuff.measuredPwr = key[3].Substring(8, 2);
                        BLEInfoBuff.macAdr = key[4];
                        BLEInfoBuff.rssi = key[5];

                        BLEInfoArg.Add(BLEInfoBuff);
                        //
	                }

                    if (arrKeyValue.Count > 0)
                    {
                        return true;
                    }
                }
            }
            Console.WriteLine("BLE no ack {0}", str_at_cmd);
            return false;
        }

        static int countTime;

        public static Boolean connectPeripheral(String peripheralMaxAdr)
        {
            String str_at_cmd = "AT+CON" + peripheralMaxAdr;

            str_at_cmd = str_at_cmd.Trim();

            byte[] at_cmd = Encoding.ASCII.GetBytes(str_at_cmd);

            _serialPort.Write(at_cmd, 0, at_cmd.Length);
            Thread.Sleep(400);

            DataSerialBuffer dsbuffParser = new DataSerialBuffer();

            Boolean dataAvai = bleSerialRead(ref dsbuffParser);
            if (dataAvai)
            {
                Console.WriteLine("\nPayload");
                for (int i = 0; i < dsbuffParser.length; i++)
                {
                    Console.Write((char)dsbuffParser.values[i]);
                }

                Console.WriteLine("");


                byte[] byteBuff = new byte[dsbuffParser.length];
                Buffer.BlockCopy(dsbuffParser.values, 0, byteBuff, 0, dsbuffParser.length);
                String strBuff = ToString(byteBuff);

                String[] strArrBuff = strBuff.Split('+');
                if (strArrBuff[0].Equals("OK"))
                {
                    //String[] strArrKeyValue = strArrBuff[1].Split(':');
                    if (strArrBuff[1].Equals("CONNA") || strArrBuff[1].Equals("CONNAOK"))//CONNAOK
                    {
                        Console.WriteLine("\nConnected Peripheral\n");
                        Thread.Sleep(200);
                        countTime = 7;
                        int intCount = 0;
                        while (true)
                        {
                            Thread.Sleep(150);
                            if (dataAvai = bleSerialRead(ref dsbuffParser))
                            {
                                byte[] byteData= new byte[dsbuffParser.length];
                                Buffer.BlockCopy(dsbuffParser.values, 0, byteData, 0, dsbuffParser.length);
                                if (!checkBLEConnectLost(ref byteData))
                                {
                                    return false;
                                }
                                else
                                {
                                    if ((netServer.writeBuff.length >= netServer.writeBuff.values.Length) || (netServer.writeBuff.idx_write >= netServer.writeBuff.values.Length))
                                    {
                                        netServer.writeBuff.idx_write = 0;
                                        netServer.writeBuff.length = 0;
                                        Console.WriteLine("Buffer Overflow(length or idx_write)");
                                    }
                                    else
                                    {
                                        Buffer.BlockCopy(byteData, 0, netServer.writeBuff.values, netServer.writeBuff.idx_write, dsbuffParser.length);
                                        netServer.writeBuff.idx_write += dsbuffParser.length;
                                        netServer.writeBuff.length += dsbuffParser.length;
                                    }
                                    
                                }
                                
                            }

                            //if ((countTime--) == 0)
                            //{
                                
                            //    //Connected
                            //    //Thread.Sleep(1000);
                            //    intCount += 1;
                            //    String strSend = "BLEtest" + string.Format("{0}", intCount);
                            //    strSend = strSend.Trim();
                            //    byte[] b_data = Encoding.ASCII.GetBytes(strSend);
                            //    sendBLEData(ref b_data, b_data.Length);
                            //    countTime = 10;
                            //}


                            if (netServer.readBuff.length > 0)
                            {
                                sendBLEData(ref netServer.readBuff.values, netServer.readBuff.length);
                                netServer.readBuff.idx_read += netServer.readBuff.length;
                                netServer.readBuff.length -= netServer.readBuff.length;



                                if ((netServer.readBuff.idx_read < 0) || (netServer.readBuff.length < 0))
                                {
                                    netServer.readBuff.idx_read = 0;
                                    netServer.readBuff.length = 0;
                                }

                                if (netServer.readBuff.idx_read == netServer.readBuff.idx_write)
                                {
                                    netServer.readBuff.idx_write = 0;
                                    netServer.readBuff.idx_read = 0;
                                    netServer.readBuff.length = 0;
                                }


                                
                            }
                            
                        }


                    }
                }
            }
            Console.WriteLine("BLE no ack {0}", str_at_cmd);
            return false;
        }

        public static void sendBLEData(ref byte[] data , int length) {
            Console.Write("\n");
            for (int i = 0; i < length; i++)
            {
                Console.Write("{0}", (char)data[i]);
            }
            _serialPort.Write(data, 0, length);
        }

        public static Boolean checkBLEConnectLost(ref byte[] data)
        {

            String strBuff = ToString(data);
            strBuff = strBuff.Trim();
            String cmp1 = "OK";
            String cmp2 = "LOST";

            String[] strArrBuff = strBuff.Split('+');
            if (strArrBuff[0].Equals(cmp1))
            {
                if ((strArrBuff[1].Equals(cmp2)))
                {
                    Console.WriteLine("\nConnection Lost Peripheral\n");
                    return false;
                }
               
            }

            // Test with IndexOf method.
            if (strBuff.IndexOf("LOSS") != -1)
            {
                Console.WriteLine("\n\nLOSS");
                return false;
            }

            return true;
        }

        public static Boolean setiBeaconDeployMode()
        {
            String str_at_cmd = "AT+DISC?";
            str_at_cmd.Trim();

            byte[] at_cmd = Encoding.ASCII.GetBytes(str_at_cmd);
            
            if (_serialPort.IsOpen)
            {
                try
                {
                    _serialPort.Write(at_cmd, 0, at_cmd.Length);
                    Thread.Sleep(1000);

                    DataSerialBuffer dsbuffParser = new DataSerialBuffer();

                    Boolean dataAvai = bleSerialRead(ref dsbuffParser);
                    if (dataAvai)
                    {
                        Console.WriteLine("\nPayload");
                        for (int i = 0; i < dsbuffParser.length; i++)
                        {
                            Console.Write((char)dsbuffParser.values[i]);
                        }

                        Console.WriteLine("");


                        byte[] byteBuff = new byte[dsbuffParser.length];
                        Buffer.BlockCopy(dsbuffParser.values, 0, byteBuff, 0, dsbuffParser.length);
                        String strBuff = ToString(byteBuff);

                        String[] strArrBuff = strBuff.Split('+');
                        if (strArrBuff[0].Equals("OK"))
                        {
                            Console.WriteLine("iBeacon Deploy mode : OK");
                            return true;
                        }
                        else
                        {
                            Console.WriteLine("iBeacon Deploy mode : Failed");
                            return false;
                        }
                    }
                    Console.WriteLine("iBeacon Deploy mode : Failed");
                    return false;
                }
                catch (Exception e)
                {
                    Console.WriteLine("{0}",e);
                    throw;
                }
                
            }
            
            return false;
        }

        private static string ToString(byte[] bytes)
        {
            string response = string.Empty;

            foreach (byte b in bytes)
                response += (Char)b;

            return response;
        }

        //public static Boolean bleSerialParser(ref DataSerialBuffer dsbuffParser, int code)
        //{
        //    String at_cmd = "AT+DISC?";
        //    at_cmd.Trim();

        //    if (code == 1)//ScanPeripheral
        //    {
        //        _serialPort.Write(at_cmd, 0, at_cmd.Length);
        //        Thread.Sleep(1000);

        //        DataSerialBuffer dsbuffParser = new DataSerialBuffer();

        //        Boolean dataAvai = bleSerialRead(ref dsbuffParser);
        //        if (dataAvai)
        //        {
        //            Console.WriteLine("\nPayload");
        //            for (int i = 0; i < dsbuffParser.length; i++)
        //            {
        //                Console.Write((char)dsbuffParser.values[i]);
        //            }

        //            Console.WriteLine("");


        //            byte[] byteBuff = new byte[dsbuffParser.length];
        //            Buffer.BlockCopy(dsbuffParser.values, 0, byteBuff, 0, dsbuffParser.length);
        //            String strBuff = ToString(byteBuff);

        //            String[] strArrBuff = strBuff.Split('+');
        //            if (strArrBuff[0].Equals("OK"))
        //            {
        //                Console.WriteLine("iBeacon Deploy mode : OK");
        //                return true;
        //            }
        //        }
        //        Console.WriteLine("iBeacon Deploy mode : Failed");
        //    }
        //    return true;
        //}


        public static void Read()
        {
            //while (_continue)
            //{
            //    try
            //    {
            //        string message = _serialPort.ReadLine();
            //        Console.WriteLine(message);
            //    }
            //    catch (TimeoutException) { }
            //}

            while (_continue)
            {
                try
                {
                    int c = _serialPort.ReadByte();
                    dsbuff.values[dsbuff.idx_write++] = (byte)c;
                    dsbuff.length++;
                    //Console.WriteLine(ByteArrayToString(new byte[]{(byte)c}));
                    //Console.Write((char)c);
                }
                catch (TimeoutException) { }
            }
        }

        public static Boolean bleSerialRead(){
            if (dsbuff.length > 0)
            {
                DataSerialBuffer dsbuffParser = new DataSerialBuffer();
                Buffer.BlockCopy(dsbuff.values, dsbuff.idx_read, dsbuffParser.values, 0, dsbuff.length);
                dsbuffParser.length = dsbuff.length;
                dsbuff.idx_read += dsbuff.length;
                dsbuff.length -= dsbuff.length;

                if (dsbuff.idx_read == dsbuff.idx_write)
                {
                    dsbuff.idx_read = 0;
                    dsbuff.idx_write = 0;
                    dsbuff.length = 0;
                }

                

                for (int i = 0; i < dsbuffParser.length; i++)
                {
                    Console.Write((char)dsbuffParser.values[i]);
                }
                return true;
            }
            return false;
        }

        public static Boolean bleSerialRead(ref DataSerialBuffer dsbuffParser)
        {
            if (dsbuff.length > 0)
            {
                //DataSerialBuffer dsbuffParser = new DataSerialBuffer();
                Buffer.BlockCopy(dsbuff.values, dsbuff.idx_read, dsbuffParser.values, 0, dsbuff.length);
                dsbuffParser.length = dsbuff.length;
                dsbuff.idx_read += dsbuff.length;
                dsbuff.length -= dsbuff.length;

                if (dsbuff.idx_read == dsbuff.idx_write)
                {
                    dsbuff.idx_read = 0;
                    dsbuff.idx_write = 0;
                    dsbuff.length = 0;
                }



                for (int i = 0; i < dsbuffParser.length; i++)
                {
                    Console.Write((char)dsbuffParser.values[i]);
                }
                return true;
            }
            return false;
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        // Display Port values and prompt user to enter a port.
        public static string SetPortName(string defaultPortName)
        {
            string portName;

            Console.WriteLine("Available Ports:");
            foreach (string s in SerialPort.GetPortNames())
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Enter COM port value (Default: {0}): ", defaultPortName);
            portName = Console.ReadLine();

            if (portName == "" || !(portName.ToLower()).StartsWith("com"))
            {
                portName = defaultPortName;
            }
            return portName;
        }
        // Display BaudRate values and prompt user to enter a value.
        public static int SetPortBaudRate(int defaultPortBaudRate)
        {
            string baudRate;

            Console.Write("Baud Rate(default:{0}): ", defaultPortBaudRate);
            baudRate = Console.ReadLine();

            if (baudRate == "")
            {
                baudRate = defaultPortBaudRate.ToString();
            }

            return int.Parse(baudRate);
        }

        // Display PortParity values and prompt user to enter a value.
        public static Parity SetPortParity(Parity defaultPortParity)
        {
            string parity;

            Console.WriteLine("Available Parity options:");
            foreach (string s in Enum.GetNames(typeof(Parity)))
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Enter Parity value (Default: {0}):", defaultPortParity.ToString(), true);
            parity = Console.ReadLine();

            if (parity == "")
            {
                parity = defaultPortParity.ToString();
            }

            return (Parity)Enum.Parse(typeof(Parity), parity, true);
        }
        // Display DataBits values and prompt user to enter a value.
        public static int SetPortDataBits(int defaultPortDataBits)
        {
            string dataBits;

            Console.Write("Enter DataBits value (Default: {0}): ", defaultPortDataBits);
            dataBits = Console.ReadLine();

            if (dataBits == "")
            {
                dataBits = defaultPortDataBits.ToString();
            }

            return int.Parse(dataBits.ToUpperInvariant());
        }

        // Display StopBits values and prompt user to enter a value.
        public static StopBits SetPortStopBits(StopBits defaultPortStopBits)
        {
            string stopBits;

            Console.WriteLine("Available StopBits options:");
            foreach (string s in Enum.GetNames(typeof(StopBits)))
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Enter StopBits value (None is not supported and \n" +
             "raises an ArgumentOutOfRangeException. \n (Default: {0}):", defaultPortStopBits.ToString());
            stopBits = Console.ReadLine();

            if (stopBits == "")
            {
                stopBits = defaultPortStopBits.ToString();
            }

            return (StopBits)Enum.Parse(typeof(StopBits), stopBits, true);
        }
        public static Handshake SetPortHandshake(Handshake defaultPortHandshake)
        {
            string handshake;

            Console.WriteLine("Available Handshake options:");
            foreach (string s in Enum.GetNames(typeof(Handshake)))
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Enter Handshake value (Default: {0}):", defaultPortHandshake.ToString());
            handshake = Console.ReadLine();

            if (handshake == "")
            {
                handshake = defaultPortHandshake.ToString();
            }

            return (Handshake)Enum.Parse(typeof(Handshake), handshake, true);
        }
    }
}
