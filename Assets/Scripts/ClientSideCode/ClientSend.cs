using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ClientSend
{
    private static void SendTcpData(Packet p)
    {
        p.WriteLength();
        ClientManager.Instace.tcp.SendData(p);
    }

    public static void SendInitializeReceived()
    {
        using (var p = new Packet((int)ClientPackets.welcomeReceived))
        {
            p.Write(ClientManager.ID);

            SendTcpData(p);
        }
    }
}