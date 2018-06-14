using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.NetworkSystem;

/// <summary>
/// Contains all information about a game server instance in the lobby.
/// </summary>
public class GameServerInfo : MessageBase
{
    public int id;
    public string ip;
    public int port;
    public string gameName;
    public MatchType matchType;
    public int maxPlayers;
    public int playerCount;

    public int GenerateAndSetUniqueID()
    {
        id = Guid.NewGuid().GetHashCode();
        return id;
    }
}


public enum MatchType : byte
{
    Normal,
    TimeBased
}