using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Use the functions in this class to parse data from the packet
/// </summary>
public static class ServerHandler
{
    public static void HandleInitializeConfirmation(int clientID, Packet p)
    {
        var packetClientID = p.ReadInt();

        if (clientID != packetClientID)
            Debug.Log("Wrong client ID!");
    }
}