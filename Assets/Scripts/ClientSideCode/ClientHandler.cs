using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Use the functions in this class to parse data from packet
/// </summary>
public static class ClientHandler
{
    public static void HandleInitialize(Packet p)
    {
        var msg = p.ReadString();
        var ID = p.ReadInt();

        ClientManager.ID = ID;
        Debug.Log(msg);

        ClientSend.SendInitializeReceived();
    }
}