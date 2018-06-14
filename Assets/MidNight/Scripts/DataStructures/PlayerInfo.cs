using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.EventSystems;

/// <summary>
/// contains all information regarding a player in the lobby.
/// </summary>
[System.Serializable]
public class PlayerInfo : MessageBase
{
    public string userID;
    public int playerID;
    public bool isPlaying;
    public int gameServerID;

    public PlayerInfo()
    {
        playerID = System.Guid.NewGuid().GetHashCode();
    }
}