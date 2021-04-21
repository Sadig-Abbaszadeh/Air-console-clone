using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerSend
{
    // initial packet sent to "welcome" client and send its id
    public static void SendInitialize(int clientID)
    {
        using (var p = new Packet((int)ServerPackets.welcome))
        {
            p.Write("Welcome");
            p.Write(clientID);

            SendTcpData(clientID, p);
        }
    }

    private static void SendTcpData(int clientID, Packet p)
    {
        p.WriteLength();
        Server.clients[clientID].tcp.SendData(p);
    }

    private static void SendTcpDataToAll(Packet p)
    {
        for (int i = 0; i < Server.MaxPlayers; i++)
            Server.clients[i].tcp.SendData(p);
    }

    private static void SentToAllExceptOne(int exceptionClient, Packet p)
    {
        for (int i = 0; i < Server.MaxPlayers; i++)
            if (i != exceptionClient)
                Server.clients[i].tcp.SendData(p);
    }
}