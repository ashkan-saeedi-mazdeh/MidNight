using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.NetworkSystem;


public class AddServerData : MessageBase
{
    public string gameName;
    public MatchType matchType;
    public int maxPlayers;
    public int port;
}

public class RemoveServerData : MessageBase
{
    public int serverID;
}

public class UpdatePlayerCountData : MessageBase
{
    public int serverID;
    public int currentPlayerCount;
}