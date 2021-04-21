using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerManager : MonoBehaviour
{
    private void Start()
    {
        Server.Start(8, 30303);
    }
}