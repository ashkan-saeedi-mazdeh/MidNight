#if SERVER
using System.Collections;
using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.NetworkSystem;
using System.Linq;
public class PlayerManagerServer : MonoBehaviour
{
    List<PlayerInfo> allowedPlayers;

    void Start()
    {
        allowedPlayers = new List<PlayerInfo>();
        LobbyClientBase.instance.networkClient.RegisterHandler((short)CustomMessageTypes.RequestPlayerApproval, x =>
         {
             IntegerMessage msg = x.ReadMessage<IntegerMessage>();
             var playerId = msg.value;
             IntegerMessage response = new IntegerMessage();
             response.value = (allowedPlayers.Any(item => item.gameServerID == playerId)) ? 1 : 0;
             x.conn.Send((short)CustomMessageTypes.ApprovalResponse, response);
             if(response.value == 0)
             {
                 StartCoroutine(DisconnectDeniedPlayer(x.conn));
             }
         });
        LobbyClientBase.instance.networkClient.RegisterHandler((short)CustomMessageTypes.SendAllowedPlayerToServer, netMsg =>
         {
             PlayerInfo p = netMsg.ReadMessage<PlayerInfo>();
             allowedPlayers.Add(p);
         });
    }

    IEnumerator DisconnectDeniedPlayer(NetworkConnection conn)
    {
        yield return new WaitForSeconds(5);
        conn.Disconnect();
    }
}
#endif