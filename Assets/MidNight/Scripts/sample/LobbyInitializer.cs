using UnityEngine;
using System.Collections;

public class LobbyInitializer : MonoBehaviour
{
    public int lobbyPort = 7777;

    void Start()
    {
        LobbyServer.instance.OnInitialized += () =>
        {
            print("lobby initialized");
            LobbyServer.instance.GetComponent<AccountManager>().Initialize<JSONAccountStorage>();
        };
        LobbyServer.instance.Initialize(lobbyPort,2000);

    }
}
