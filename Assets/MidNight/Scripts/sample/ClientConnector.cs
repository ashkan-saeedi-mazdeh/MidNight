using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.NetworkSystem;
public class ClientConnector : MonoBehaviour
{
    public string lobbyAddress = "localhost";
    public int lobbyPort = 7777;
    private PlayerInfo myPlayer;

    void Start()
    {
        LobbyClientBase.instance.OnConnected += () =>
        {
            LobbyClientBase.instance.InitializeAsGameClient();
            OnConnectedToLobby();
        };
        LobbyClientBase.instance.OnFailedToConnect += () =>
        {
            StartCoroutine(ConnectAfterDelay(3)); //circuit pattern
        };
        LobbyClientBase.instance.Connect(lobbyAddress, lobbyPort);
    }

    private static void OnConnectedToLobby()
    {
        print("connected to lobby from client");

        var pm = LobbyClientBase.instance.GetComponent<PlayerManagerClient>();
        pm.OnPlayerDataReceived += player =>
        {
            pm.FindMatch();
        };
        pm.OnMatchFound += match =>
        {
            OnMatchFound(match, pm);
        };
        pm.OnFailedToFindMatch += () =>
        {
            print("could not find a match");
        };
        var accm = AccountManagerClientBase.instance;
        accm.RegisterAccount("user " + Random.value.ToString(), "pass", "");
        accm.Login("Ashkan", "pass");
        accm.OnLoggedOut += () => Debug.Log("LoggedOut");
        accm.OnAccountRegistered += () => Debug.Log("created");
        accm.OnFriendInvited += () => print("invited");
        accm.OnFailedToInvite += () => Debug.LogError("failed to invite");
        accm.OnInvitationAccepted += () => print("accepted");
        accm.OnInvitationDenied += () => print("denied");
        accm.OnInvitationsReceived += x => { accm.DenyInvitation(x[0].invitationID); };
        accm.OnLoggedIn += x =>
        {
            print(x.ToString());
            
            accm.GetFriendsList();
            accm.InviteFriend("Ramin");
            accm.GetInvitations();
            //accm.Logout();
        };
        accm.OnFriendsListReceived += x =>
        {
            print(x.accounts.Count);
            foreach (var m in x.accounts)
            {
                print(m);
            }
        };
        accm.OnFailedToLogin += () => Debug.LogError("failed to login");
    }

    private static void OnMatchFound(GameServerInfo match, PlayerManagerClient pm)
    {
        print("match found " + match.ip + " " + match.port);
        NetworkClient gameClient = new NetworkClient();
        gameClient.RegisterHandler(MsgType.Connect, x =>
        {
            print("connected to gameServer");
            gameClient.RegisterHandler((short)CustomMessageTypes.ApprovalResponse, netMsg =>
            {
                IntegerMessage msg = netMsg.ReadMessage<IntegerMessage>();
                bool isApproved = msg.value == 1;
                print("approval status " + isApproved);
                if (!isApproved)
                {
                    gameClient.Disconnect();
                }
                else
                {
                    //Start playing , 
                    //Teel server i'm ready and addPlayerRequest if needed

                    //ClientScene.AddPlayer()
                    // or
                    //ClientScene.Ready();
                }
            });
            gameClient.Send((short)CustomMessageTypes.RequestPlayerApproval, new IntegerMessage(pm.myInfo.gameServerID));
        });
        gameClient.Connect(match.ip, match.port);
    }

    IEnumerator ConnectAfterDelay(float delay)
    {
        print("retrying to connect");
        yield return new WaitForSeconds(delay);
        LobbyClientBase.instance.Connect(lobbyAddress, lobbyPort);
    }

}
