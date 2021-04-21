using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;

public static class Server
{
    public static int MaxPlayers { get; private set; }   
    public static int Port { get; private set; }

    public static Dictionary<int, Client> clients = new Dictionary<int, Client>();

    // packet handler methods
    public static Dictionary<int, Action<int, Packet>> packetHandlers;

    private static TcpListener tcpListener;

    public static void Start(int maxPlayers, int port)
    {
        MaxPlayers = maxPlayers;
        Port = port;
        InitializeCLients();

        Debug.Log("Starting server...");

        tcpListener = new TcpListener(IPAddress.Any, port);
        tcpListener.Start();
        BeginAcceptingClients();

        Debug.Log($"Started server on port {Port}");
    }

    private static void BeginAcceptingClients() => tcpListener.BeginAcceptTcpClient(
        new System.AsyncCallback(TCPClientConnectedCallback), null);

    private static void TCPClientConnectedCallback(IAsyncResult result)
    {
        var client = tcpListener.EndAcceptTcpClient(result);

        ConnectClient(client);

        BeginAcceptingClients();
    }

    private static void ConnectClient(TcpClient client)
    {
        // find vacant client spot
        for (int i = 1; i <= MaxPlayers; i++)
            if (clients[i].tcp.socket == null)
            {
                clients[i].tcp.Connect(client);
                return;
            }

        Debug.Log("server is full!");
    }

    private static void InitializeCLients()
    {
        for (int i = 1; i <= MaxPlayers; i++)
            clients.Add(i, new Client(i));

        packetHandlers = new Dictionary<int, Action<int, Packet>>()
        {
            { (int)ClientPackets.welcomeReceived, ServerHandler.HandleInitializeConfirmation }
        };
    }
}