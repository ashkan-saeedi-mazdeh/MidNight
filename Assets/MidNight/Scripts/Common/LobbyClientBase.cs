using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.NetworkSystem;

public class LobbyClientBase : MonoBehaviour
{

    public static LobbyClientBase instance
    {
        get
        {
            if (_instance == null)
            {
                CreateInstance();
            }
            return _instance;
        }
    }
    private static LobbyClientBase _instance;

    public event Action OnFailedToConnect;
    public event Action OnConnected;
    public event Action OnDisconnected;

    public NetworkClient networkClient;
    private bool isConnected;

    static private void CreateInstance()
    {
        GameObject lobbyClient = new GameObject("lobby client");
        lobbyClient.hideFlags = HideFlags.HideAndDontSave;
        DontDestroyOnLoad(lobbyClient);
        _instance = lobbyClient.AddComponent<LobbyClientBase>();
    }

    public void InitializeAsGameClient()
    {
        this.gameObject.AddComponent<PlayerManagerClient>();
        this.gameObject.AddComponent<AccountManagerClientBase>();
    }

    public void InitializeAsGameServer()
    {
        this.gameObject.AddComponent<PlayerManagerServer>();
        this.gameObject.AddComponent<ServerListClient>();
        this.gameObject.AddComponent<AccountManagerClientBase>();
    }

    public void Connect(string ip, int port)
    {
        ConnectionConfig cc = new ConnectionConfig();
        cc.AddChannel(QosType.Reliable);
        cc.AddChannel(QosType.ReliableFragmented);
        networkClient = new NetworkClient();
        networkClient.Configure(cc, 1);
        networkClient.RegisterHandler(MsgType.Connect, x =>
        {
            isConnected = true;
            if (OnConnected != null)
                OnConnected();
        });
        networkClient.RegisterHandler(MsgType.Disconnect, x =>
        {
            if (isConnected)
            {
                isConnected = false;
                if (OnDisconnected != null)
                    OnDisconnected();
            }
            else //connection failure
            {
                if (OnFailedToConnect != null)
                    OnFailedToConnect();

            }
        });
        networkClient.Connect(ip, port);
    }
}
