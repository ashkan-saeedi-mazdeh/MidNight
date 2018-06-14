#if LOBBY
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.NetworkSystem;
using System.Linq;

public class PlayerManager : MonoBehaviour
{
    private Dictionary<PlayerInfo, NetworkConnection> players;

    void Start()
    {
        LobbyServer.instance.OnClientDisconnected += conn =>
        {
            var p = GetPlayer(conn);
            if (p != null)
                RemovePlayer(p);
        };
        AccountManager.instance.OnLoggedIn += x =>
        {
            PlayerInfo p = new PlayerInfo();
            p.userID = x.userID;
            var c = AccountManager.instance.GetConnectionByLoggedinAccount(x);
            AddPlayer(p,c);
        };
        players = new Dictionary<PlayerInfo, NetworkConnection>();
        NetworkServer.RegisterHandler((short)CustomMessageTypes.FindMatchRequest, netMsg =>
         {
             var player = GetPlayer(netMsg.conn);
             FindGameForPlayer(player);
         });
    }

    public void AddPlayer(PlayerInfo player, NetworkConnection conn)
    {
        players.Add(player, conn);
        conn.Send((short)CustomMessageTypes.SetPlayerAtClient, player);
    }

    public void RemovePlayer(PlayerInfo player)
    {
        players.Remove(player);
    }

    public void PlayerMatchFinished(PlayerInfo player)
    {
        player.isPlaying = false;
        player.gameServerID = 0;
    }

    public void FindGameForPlayer(PlayerInfo player)
    {
        var serverList = GetComponent<ServerList>();
        var match = serverList.FindMatch(player);
        if (match != null)//server found
        {
            FindMatchResponseData data = new FindMatchResponseData();
            data.isMatchFound = true;
            data.serverInfo = match;
            player.gameServerID = match.id;
            player.isPlaying = true;
            players[player].Send((short)CustomMessageTypes.FindMatchResponse, data);

            var serverConn = GetComponent<ServerList>().GetServerConnection(match);
            serverConn.Send((short)CustomMessageTypes.SendAllowedPlayerToServer, player);
        }
        else //server not found
        {
            FindMatchResponseData data = new FindMatchResponseData();
            data.isMatchFound = false;
            players[player].Send((short)CustomMessageTypes.FindMatchResponse, data);
        }
    }

    public PlayerInfo GetPlayer(int playerID)
    {
        try
        {
            var p = players.First(x => x.Key.gameServerID == playerID);
            return p.Key;
        }
        catch
        {
            return null;
        }
    }

    public PlayerInfo GetPlayer(NetworkConnection conn)
    {
        try
        {
            var p = players.First(x => x.Value == conn);
            return p.Key;
        }
        catch
        {
            return null;
        }
    }

}
#endif