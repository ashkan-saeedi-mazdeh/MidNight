#if LOBBY
using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.NetworkSystem;
using System.Linq;

public class ServerList : MonoBehaviour
{
    public event Action<GameServerInfo> OnServerAdded;
    public event Action<GameServerInfo> OnServerRemoved;
    public event Action<GameServerInfo> OnServerUpdated;

    private Dictionary<GameServerInfo,NetworkConnection> servers;

    void Awake()
    {
        servers = new Dictionary<GameServerInfo, NetworkConnection>();
        NetworkServer.RegisterHandler((short)CustomMessageTypes.AddServer, netMsg =>
        {
            print("received add server request");
            AddServerData receivedData = netMsg.ReadMessage<AddServerData>();
            GameServerInfo serverInfo = new GameServerInfo();
            serverInfo.maxPlayers = receivedData.maxPlayers;
            serverInfo.gameName = receivedData.gameName;
            serverInfo.matchType = receivedData.matchType;
            serverInfo.ip = netMsg.conn.address;
            serverInfo.port = receivedData.port;
            serverInfo.GenerateAndSetUniqueID();
            AddServer(serverInfo,netMsg.conn);
            NetworkServer.SendToClient(netMsg.conn.connectionId, (short)CustomMessageTypes.SetServerInfoOnGameServer, serverInfo);
        });

        NetworkServer.RegisterHandler((short)CustomMessageTypes.RemoveServer, netMsg =>
         {
             RemoveServerData receiveData = netMsg.ReadMessage<RemoveServerData>();
             RemoveServer(receiveData.serverID);
         });

        NetworkServer.RegisterHandler((short)CustomMessageTypes.UpdateServerPlayerCount, netMsg =>
         {
             UpdatePlayerCountData receivedData = netMsg.ReadMessage<UpdatePlayerCountData>();
             UpdatePlayerCount(receivedData.serverID, receivedData.currentPlayerCount);
         });

    }

    void Start()
    {
        LobbyServer.instance.OnClientDisconnected += conn =>
        {
            var server = GetServer(conn);
            if (server != null)
                RemoveServer(server.id);
        };
    }

    public void AddServer(GameServerInfo server,NetworkConnection conn)
    {
        servers.Add(server,conn);
        if (OnServerAdded != null)
            OnServerAdded(server);
    }

    public void RemoveServer(int serverID)
    {
        var server = servers.FirstOrDefault(x => x.Key.id == serverID).Key;
        if (server != null)
        {
            servers.Remove(server);
            if (OnServerRemoved != null)
                OnServerRemoved(server);
        }
    }

    public bool UpdatePlayerCount(int serverID, int playerCount)
    {
        bool success = false;
        var server = GetServer(serverID);
        if (server != null)
        {
            if (server.maxPlayers >= playerCount)
            {
                server.playerCount = playerCount;
                success = true;
            }
            else
            {
                Debug.LogError("Player count is greater than max players!");
                throw new InvalidOperationException("player count is bigger than max players");
            }
        }
        else
        {
            Debug.LogError("The server no longer exists");
            success = false;

        }
        return success;
    }

    private GameServerInfo GetServer(int serverId)
    {
        return servers.FirstOrDefault(x => x.Key.id == serverId).Key;
    }

    private GameServerInfo GetServer(NetworkConnection serverCon)
    {
        return servers.FirstOrDefault(x => x.Value == serverCon).Key;
    }

    public GameServerInfo FindMatch(PlayerInfo player)
    {
        Dictionary<GameServerInfo, NetworkConnection> candidates = new Dictionary<GameServerInfo, NetworkConnection>();
        foreach(var kvp in servers)
        {
            
            if (kvp.Key.playerCount < kvp.Key.maxPlayers)
                candidates.Add(kvp.Key,kvp.Value);
        }
        if (candidates.Count > 0)
        {
            GameServerInfo match = candidates.Keys.ToArray()[0];
            AddPlayerToServer(match,player);
            return match;
        }
        else
        {
            //TODO we should instantiate new server.
            return null;
        }

    }

    private void AddPlayerToServer(GameServerInfo match,PlayerInfo player)
    {
        print(match == null);
        UpdatePlayerCount(match.id, match.playerCount + 1);
    }

    private void RemovePlayerFromServer(GameServerInfo server,PlayerInfo player)
    {
        //TODO We should have a dictioanry which tracks player lists for each server.
        UpdatePlayerCount(server.id, server.playerCount - 1);
    }

    public NetworkConnection GetServerConnection(GameServerInfo server)
    {
        return servers[server];
    }

}
#endif