using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class ClientManager : MonoBehaviour
{
    public static ClientManager Instace;

    public TCP tcp = new TCP();

    public static int ID;

    // packet handler methods
    private static Dictionary<int, Action<Packet>> packetHandlers;

    private void Awake()
    {
        if (Instace != null)
        {
            Destroy(gameObject);
            return;
        }

        Instace = this;
    }

    private void Start()
    {
        //ConnectToServer();
    }

    // begin connecting to server
    public void ConnectToServer()
    {
        InitClientData();
        tcp.Connect();
    }

    public class TCP
    {
        public TcpClient socket = null;
        private NetworkStream stream;
        private byte[] receiveBuffer;
        private Packet receivedPacket;

        public void Connect()
        {
            socket = new TcpClient()
            {
                ReceiveBufferSize = ConnectionOptions.BufferSize,
                SendBufferSize = ConnectionOptions.BufferSize,
            };

            receiveBuffer = new byte[ConnectionOptions.BufferSize];

            socket.BeginConnect(ConnectionOptions.IP, ConnectionOptions.Port, ConnectedToServerCallback, socket);
        }

        private void ReadData() => stream.BeginRead(receiveBuffer, 0, ConnectionOptions.BufferSize, ReceivedStreamCallback, null);

        // connection callback
        private void ConnectedToServerCallback(IAsyncResult result)
        {
            socket.EndConnect(result);

            if (!socket.Connected) return;

            receivedPacket = new Packet();

            stream = socket.GetStream();
            ReadData();
        }

        private void ReceivedStreamCallback(IAsyncResult result)
        {
            try
            {
                var length = stream.EndRead(result);

                if (length <= 0) return;

                var data = new byte[length];
                Array.Copy(receiveBuffer, data, length);

                receivedPacket.Reset(HandleData(data));

                // continue reading
                ReadData();
            }
            catch
            {
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
                        packetHandlers[packetID](p);
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
    }

    private void InitClientData()
    {
        packetHandlers = new Dictionary<int, Action<Packet>>()
        {
            { (int)ServerPackets.welcome, ClientHandler.HandleInitialize }
        };
    }
}