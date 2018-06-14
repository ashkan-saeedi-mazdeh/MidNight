#if LOBBY
using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.NetworkSystem;
using System.Linq;

public class LobbyServer : MonoBehaviour
{
    public static LobbyServer instance
    {
        get
        {
            if(_instance == null)
            {
                CreateInstance();
            }
            return _instance;
        }
    }
    private static LobbyServer _instance;

    public event Action OnInitialized;
    public event Action<NetworkConnection> OnClientConnected;
    public event Action<NetworkConnection> OnClientDisconnected;


    static private void CreateInstance()
    {
        GameObject lobby = new GameObject("lobby");
        lobby.hideFlags = HideFlags.HideAndDontSave;
        DontDestroyOnLoad(lobby);
        _instance = lobby.AddComponent<LobbyServer>();
        lobby.AddComponent<ServerList>();
        lobby.AddComponent<PlayerManager>();
        lobby.AddComponent<AccountManager>();
    }

    /// <summary>
    /// Initializes the lobby
    /// </summary>
    /// <param name="port">The port to listen to</param>
    /// <param name="maxPlayers">Maximum number of allowed players</param>
    public void Initialize(int port,int maxPlayers)
    {
        ConnectionConfig cc = new ConnectionConfig();
        cc.AddChannel(QosType.Reliable);
        cc.AddChannel(QosType.ReliableFragmented);
        NetworkServer.Configure(cc, maxPlayers);
        NetworkServer.RegisterHandler(MsgType.Connect, netMsg =>
        { if (OnClientConnected != null) OnClientConnected(netMsg.conn); });

        NetworkServer.RegisterHandler(MsgType.Disconnect, netMsg =>
        { if (OnClientConnected != null) OnClientConnected(netMsg.conn); });

        if (NetworkServer.Listen(port))
        {
            if (OnInitialized != null)
                OnInitialized();
        }
    }

    public void ShutDown()
    {
        NetworkServer.Shutdown();
    }

    public void CloseConnection(int connectionID)
    {
        var con = NetworkServer.connections.FirstOrDefault(x => x.connectionId == connectionID);
        if (con != null)
            con.Disconnect();
    }
}
#endif