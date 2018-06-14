#if CLIENT
using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.NetworkSystem;

public class PlayerManagerClient : MonoBehaviour
{
    public event Action<GameServerInfo> OnMatchFound;
    public event Action OnFailedToFindMatch;
    public event Action<PlayerInfo> OnPlayerDataReceived;

    public PlayerInfo myInfo;

    void Start()
    {
        LobbyClientBase.instance.networkClient.RegisterHandler((short)CustomMessageTypes.SetPlayerAtClient, netMsg =>
         {
             myInfo = netMsg.ReadMessage<PlayerInfo>();
             if (OnPlayerDataReceived != null)
                 OnPlayerDataReceived(myInfo);
         });
        LobbyClientBase.instance.networkClient.RegisterHandler((short)CustomMessageTypes.FindMatchResponse, netMsg =>
         {
             var resp = netMsg.ReadMessage<FindMatchResponseData>();
             if (resp.isMatchFound)
             {
                 var server = resp.serverInfo;
                 if (OnMatchFound != null)
                     OnMatchFound(server);
             }
             else
             {
                 if (OnFailedToFindMatch != null)
                     OnFailedToFindMatch();
             }
         });
    }

    public void FindMatch()
    {
        LobbyClientBase.instance.networkClient.Send((short)CustomMessageTypes.FindMatchRequest, new EmptyMessage());
    }
}

#endif