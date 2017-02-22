using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;


namespace BLESerial
{
    public class NetworkBuffer {
        public byte[] values;
        public int length;
        public int idx_read;
        public int idx_write;

        public NetworkBuffer() {
            values = new byte[1024];
            length = 0;
            idx_read = 0;
            idx_write = 0;
        }
    }

    public class Network
    {
        public NetworkBuffer readBuff, writeBuff;


        TcpListener tcpServer;
        Thread listenThread;
        public Network() {
            readBuff = new NetworkBuffer();
            writeBuff = new NetworkBuffer();
            tcpServer = new TcpListener(IPAddress.Any, 40000); //in constructor (auto added if added as a component)
            //this.listenThread = new Thread(new ThreadStart(ListenForClients));
            //this.listenThread.Start();
        }



        public void ListenForClients()
        {
            try
            {
                this.tcpServer.Start();

                //while (true)
                //{
                    //blocks until a client has connected to the server
                    TcpClient client = this.tcpServer.AcceptTcpClient();


                    // here was first an message that send hello client
                    //
                    ///////////////////////////////////////////////////

                    //create a thread to handle communication
                    //with connected client
                    Thread clientThreadRead = new Thread(new ParameterizedThreadStart(HandleClientReadComm));

                    clientThreadRead.Start(client);

                    Thread clientThreadWrite = new Thread(new ParameterizedThreadStart(HandleClientWriteComm));
                    clientThreadWrite.Start(client);
                //}
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        private void HandleClientReadComm(object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();

            byte[] message = new byte[4096];
            int bytesRead;

            int countTimer = 10;

            if (!tcpClient.Connected)
            {
                return;
            }

            while (true)
            {
                Thread.Sleep(100);
                bytesRead = 0;

                try
                {
                    //blocks until a client sends a message
                    bytesRead = clientStream.Read(message, 0, 4096);
                    Console.Write("Network\n");
                    for (int i = 0; i < bytesRead; i++)
                    {
                        Console.Write("{0}", (char)message[i]);
                    }
                    Console.Write("\n");

                    if (bytesRead > 0)
                    {
                        if ((readBuff.length >= readBuff.values.Length) || (readBuff.idx_write >= readBuff.values.Length))
                        {
                            readBuff.idx_write = 0;
                            readBuff.length = 0;
                            Console.WriteLine("Buffer Overflow(length or idx_write)");
                        }
                        else
                        {
                            Buffer.BlockCopy(message, 0, readBuff.values, readBuff.idx_write, bytesRead);
                            readBuff.idx_write += bytesRead;
                            readBuff.length += bytesRead;
                        }
                    }

                    

                }
                catch
                {
                    //a socket error has occured
                    //break;
                    throw;
                }

                if (bytesRead == 0)
                {
                    //the client has disconnected from the server
                    break;
                }
            }
        }


        private void HandleClientWriteComm(object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();

            byte[] message = new byte[4096];

            int countTimer = 10;

            if (!tcpClient.Connected)
            {
                return;
            }

            while (true)
            {
                Thread.Sleep(100);

                //if ((countTimer--) == 0)
                //{
                //    byte[] buffer = new byte[] { (byte)'T', (byte)'E', (byte)'S', (byte)'T' };

                //    clientStream.Write(buffer, 0, buffer.Length);
                //    clientStream.Flush();

                //    countTimer = 10;
                //}

                if (writeBuff.length > 0)
                {
                    try
                    {
                        clientStream.Write(writeBuff.values, writeBuff.idx_read, writeBuff.length);
                        writeBuff.idx_read += writeBuff.length;
                        writeBuff.length -= writeBuff.length;



                        if ((writeBuff.idx_read < 0) || (writeBuff.length < 0))
                        {
                            writeBuff.idx_read = 0;
                            writeBuff.length = 0;
                        }

                        if (writeBuff.idx_read == writeBuff.idx_write)
                        {
                            writeBuff.idx_write = 0;
                            writeBuff.idx_read = 0;
                            writeBuff.length = 0;
                        }

                        clientStream.Flush();
                    }
                    catch (Exception)
                    {
                        var fileName = System.Reflection.Assembly.GetExecutingAssembly().Location;
                        System.Diagnostics.Process.Start(fileName);
                        Environment.Exit(0);
                        throw;
                    }
                }

            }
        }

    }
}
