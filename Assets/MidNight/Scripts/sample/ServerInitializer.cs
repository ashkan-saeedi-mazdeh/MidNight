using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.NetworkSystem;

public class ServerInitializer : MonoBehaviour
{
    public string lobbyAddres = "localhost";
    public int lobbyPort = 7777;

    void Start()
    {
        LobbyClientBase.instance.OnConnected += () =>
        {
            LobbyClientBase.instance.InitializeAsGameServer();
            print("connected to lobby from server");
            NetworkServer.RegisterHandler(MsgType.AddPlayer, x =>
             {
                 //NetworkServer.AddPlayerForConnection()
             });
            NetworkServer.Listen(5000);
            LobbyClientBase.instance.GetComponent<ServerListClient>().AddSelfToLobby("milad-first", 5000, MatchType.Normal, 8);
        };
        LobbyClientBase.instance.OnFailedToConnect += () =>
        {
            StartCoroutine(TryToConnectAfterDelay(3)); //implement circiut pattern
        };
        LobbyClientBase.instance.Connect(lobbyAddres, lobbyPort);
    }

    IEnumerator TryToConnectAfterDelay(float delay)
    {
        print("retrying to connect");
        yield return new WaitForSeconds(delay);
        LobbyClientBase.instance.Connect(lobbyAddres, lobbyPort);
    }
}
