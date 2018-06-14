using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.Networking.Types;

public class AccountManagerClientBase : MonoBehaviour
{
    public static AccountManagerClientBase instance;

    public Action<AccountInfo> OnLoggedIn;
    public Action OnFailedToLogin;
    public Action OnLoggedOut;
    public Action OnFailedToLogout;
    public Action OnAccountRegistered;
    public Action OnFailedToRegisterAccount;
    public Action OnFailedToGetFriendsList;
    public Action<AccountsCollection> OnFriendsListReceived;
    public Action OnFailedToInvite;
    public Action OnFriendInvited;
    public Action<List<Invitation>> OnInvitationsReceived;
    public Action OnFailedToGetInvitations;
    public Action OnInvitationAccepted;
    public Action OnFailedToAcceptInvitation;
    public Action OnInvitationDenied;
    public Action OnFailedToDeny;

    private LobbyClientBase lobby;
    private bool isConnected;

    private AccountInfo loggedInAccount;
    public bool IsLoggedIn
    {
        get
        {
            return loggedInAccount != null;
        }
    }

    public AccountInfo GetLoggedInUser()
    {
        return loggedInAccount;
    }

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("One instance of AccountManagerClientBase already exists, only one version should be in a scene.");
            return;
        }
        instance = this;
        lobby = LobbyClientBase.instance;
        lobby.OnConnected += () =>
            {
                isConnected = true;
            };

        lobby.OnDisconnected += () =>
        {
            isConnected = false;
        };

        lobby.networkClient.RegisterHandler((short)CustomMessageTypes.LoginResponse, x =>
         {
             var msg = x.ReadMessage<LoginResponseData>();
             if (msg.isSuccessful)
             {
                 loggedInAccount = msg.account;
                 if (OnLoggedIn != null)
                     OnLoggedIn(msg.account);
             }
             else
             {
                 if (OnFailedToLogin != null)
                     OnFailedToLogin();
             }
         });

        lobby.networkClient.RegisterHandler((short)CustomMessageTypes.LogoutResponse, x =>
         {
             var msg = x.ReadMessage<BooleanMessage>();
             if(msg.value)
             {
                 loggedInAccount = null;
                 if (OnLoggedOut != null)
                     OnLoggedOut();
             }
             else
             {
                 if (OnFailedToLogout != null)
                     OnFailedToLogout();
             }
         });
        lobby.networkClient.RegisterHandler((short)CustomMessageTypes.RegisterAccountResponse, x =>
         {
             var msg = x.ReadMessage<BooleanMessage>();
             if(msg.value)
             {
                 if (OnAccountRegistered != null)
                     OnAccountRegistered();
             }
             else
             {
                 if (OnFailedToRegisterAccount != null)
                     OnFailedToRegisterAccount();
             }
         });

        lobby.networkClient.RegisterHandler((short)CustomMessageTypes.GetFriendslistResponse, x =>
         {
             var resp = x.ReadMessage<GetFriendsListResponseData>();
             if(resp.success)
             {
                 if (OnFriendsListReceived != null)
                     OnFriendsListReceived(resp.friends);
             }
             else
             {
                 if (OnFailedToGetFriendsList != null)
                     OnFailedToGetFriendsList();
             }
         });
        lobby.networkClient.RegisterHandler((short)CustomMessageTypes.InviteFriendResponse, x =>
         {
             var resp = x.ReadMessage<BooleanMessage>();
             if(resp.value)
             {
                 if (OnFriendInvited != null)
                     OnFriendInvited();
             }
             else
             {
                 if (OnFailedToInvite != null)
                     OnFailedToInvite();
             }
         });
        lobby.networkClient.RegisterHandler((short)CustomMessageTypes.GetInvitationsResponse, x =>
         {
             var msg = x.ReadMessage<InvitationsCollection>();
             if(msg.success)
             {
                 if (OnInvitationsReceived != null)
                     OnInvitationsReceived(msg.invitations);
             }
             else
             {
                 if (OnFailedToGetInvitations != null)
                     OnFailedToGetInvitations();
             }
         });
        lobby.networkClient.RegisterHandler((short)CustomMessageTypes.AcceptInvitationResponse, x =>
         {
             var msg = x.ReadMessage<BooleanMessage>();
             if(msg.value)
             {
                 if (OnInvitationAccepted != null)
                     OnInvitationAccepted();
             }
             else
             {
                 if (OnFailedToAcceptInvitation != null)
                     OnFailedToAcceptInvitation();
             }
         });
        lobby.networkClient.RegisterHandler((short)CustomMessageTypes.DenyInvitationResponse, x =>
        {
            var msg = x.ReadMessage<BooleanMessage>();
            if (msg.value)
            {
                if (OnInvitationDenied != null)
                    OnInvitationDenied();
            }
            else
            {
                if (OnFailedToDeny != null)
                    OnFailedToDeny();
            }
        });
    }

    public void Login(string userName, string password)
    {
        LoginRequestData data = new LoginRequestData { userName = userName, passwordHash = password.GetHashCode() };
        lobby.networkClient.Send((short)CustomMessageTypes.LoginRequest, data);
    }

    public bool Logout()
    {
        if(IsReadyForAccountWork())
        {
            lobby.networkClient.Send((short)CustomMessageTypes.LogoutRequest,new EmptyMessage());
            return true;
        }
        return false;
    }

    public void RegisterAccount(string userName,string password,string data)
    {
        RegisterAccountRequestData req = new RegisterAccountRequestData();
        req.userName = userName;
        req.passwordHash = password.GetHashCode();
        req.data = data;
        lobby.networkClient.Send((short)CustomMessageTypes.RegisterAccountRequest, req);
    }

    public void GetFriendsList()
    {
        if(IsReadyForAccountWork())
            lobby.networkClient.Send((short)CustomMessageTypes.GetFriendslistRequest, new EmptyMessage());
    }

    public void InviteFriend(string friendName)
    {
        if(IsReadyForAccountWork())
        {
            StringMessage msg = new StringMessage(friendName);
            lobby.networkClient.Send((short)CustomMessageTypes.InviteFriendRequest, msg);
        }
    }

    public void GetInvitations()
    {
        if(IsReadyForAccountWork())
            lobby.networkClient.Send((short)CustomMessageTypes.GetInvitationsRequest, new EmptyMessage());
    }

    public void AcceptInvitation(string invitationID)
    {
        if (IsReadyForAccountWork())
            lobby.networkClient.Send((short)CustomMessageTypes.AcceptInvitationRequest, new StringMessage(invitationID));
    }

    public void DenyInvitation(string invitationID)
    {
        if (IsReadyForAccountWork())
            lobby.networkClient.Send((short)CustomMessageTypes.DenyInvitationRequest, new StringMessage(invitationID));
    }

    private bool IsReadyForAccountWork()
    {
        return isConnected && IsLoggedIn;
    }
}
