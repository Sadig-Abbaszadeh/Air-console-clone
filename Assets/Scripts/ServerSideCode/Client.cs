using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;

public class Client
{
    public static int dataBufferSize = 4096;

    // client id
    public int ID;
    public TCP tcp;

    public Client(int ID)
    {
        this.ID = ID;
        this.tcp = new TCP(ID);
    }

    public class TCP
    {
        public TcpClient socket;
        private NetworkStream stream;
        private byte[] receiveBuffer;
        private Packet receivedPacket;

        private int ID;

        public TCP(int ID)
        {
            this.ID = ID;
        }

        public void Connect(TcpClient socket)
        {
            this.socket = socket;

            socket.ReceiveBufferSize = dataBufferSize;
            socket.SendBufferSize = dataBufferSize;

            this.stream = socket.GetStream();
            this.receiveBuffer = new byte[dataBufferSize];
            this.receivedPacket = new Packet();

            ReadData();

            // welcome
            ServerSend.SendInitialize(ID);
        }

        public void SendData(Packet p)
        {
            try
            {
                stream?.BeginWrite(p.ToArray(), 0, p.Length(), null, null);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }

        private void ReadData() => stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceivedStreamCallback, null);

        private void ReceivedStreamCallback(IAsyncResult result)
        {
            try
            {
                var length = stream.EndRead(result);

                if (length <= 0) return;

                var data = new byte[length];
                Array.Copy(receiveBuffer, data, length);

                // TODO: handle data
                ReadData();
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }

        private bool HandleData(byte[] data)
        {
            int length = 0;

            receivedPacket.SetBytes(data);

            if (receivedPacket.UnreadLength() >= 4)
            {
                length = receivedPacket.ReadInt();

                if (length <= 0)
                    return true;
            }

            // read incomplete
            while (length > 0 && length <= receivedPacket.UnreadLength())
            {
                var packetBytes = receivedPacket.ReadBytes(length);

                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (var p = new Packet(packetBytes))
                    {
                        var packetID = p.ReadInt();
                        Debug.Log(packetID);
                        Server.packetHandlers[packetID](ID, p);
                    }
                });

                length = 0;
            }

            if (receivedPacket.UnreadLength() >= 4)
            {
                length = receivedPacket.ReadInt();

                if (length <= 0)
                    return true;
            }

            return length <= 1;
        }
    }
}