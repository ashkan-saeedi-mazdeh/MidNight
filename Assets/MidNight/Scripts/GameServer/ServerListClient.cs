#if SERVER
using System;
using UnityEngine;
using System.Collections;

public class ServerListClient : MonoBehaviour
{
    private GameServerInfo myInfo;

    void Start()
    {
        LobbyClientBase.instance.networkClient.RegisterHandler((short)CustomMessageTypes.SetServerInfoOnGameServer, netMsg =>
         {
             myInfo = netMsg.ReadMessage<GameServerInfo>();
         });
    }

    public void AddSelfToLobby(string gameName, int port, MatchType matchType, int maxPlayers)
    {
        AddServerData data = new AddServerData();
        data.port = port;
        data.gameName = gameName;
        data.matchType = matchType;
        data.maxPlayers = maxPlayers;
        LobbyClientBase.instance.networkClient.Send((short)CustomMessageTypes.AddServer, data);
    }

    public void RemoveSelfFromLobby()
    {
        if (myInfo != null)
        {
            RemoveServerData data = new RemoveServerData();
            data.serverID = myInfo.id;
            LobbyClientBase.instance.networkClient.Send((short)CustomMessageTypes.RemoveServer, data);
        }
    }

    public void UpdatePlayerCount(int playerCount)
    {
        if (myInfo != null)
        {
            UpdatePlayerCountData data = new UpdatePlayerCountData();
            data.serverID = myInfo.id;
            data.currentPlayerCount = playerCount;
            LobbyClientBase.instance.networkClient.Send((short)CustomMessageTypes.UpdateServerPlayerCount, data);
        }
    }
}

#endif