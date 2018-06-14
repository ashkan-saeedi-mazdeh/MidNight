using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.NetworkSystem;

public enum CustomMessageTypes : short
{
    #region ServerListMessages
    AddServer = MsgType.Highest + 1,
    RemoveServer,
    UpdateServerPlayerCount,
    SetServerInfoOnGameServer,
    #endregion

    #region PlayerMatchMessages
    FindMatchRequest,
    FindMatchResponse,
    SendAllowedPlayerToServer,
    SetPlayerAtClient,
    RequestPlayerApproval,
    ApprovalResponse,
    #endregion

    #region Account
    LoginRequest,
    LoginResponse,
    LogoutRequest,
    LogoutResponse,
    RegisterAccountRequest,
    RegisterAccountResponse,
    GetFriendslistRequest,
    GetFriendslistResponse,
    InviteFriendRequest,
    InviteFriendResponse,
    GetInvitationsRequest,
    GetInvitationsResponse,
    AcceptInvitationRequest,
    AcceptInvitationResponse,
    DenyInvitationRequest,
    DenyInvitationResponse,
    #endregion

    #region SaveData
    SetDataRequest,
    SetDataResponse,
    GetDataRequest,
    GetDataResponse,
    #endregion

    #region InstanceManager
    //InstanceManager client
    StartInstanceRequest,
    StartInstanceResponse,
    InstanceStopped,
    InstanceCrashed,
    
    //Slave
    InstanceSlaveConnectRequest,
    InstanceSlaveConnectResponse,
    InstanceSlaveShutdown,
    InstanceSlaveStartInstanceRequest,
    InstanceSlaveStartInstanceResponse,
    

    //Instance
    InstanceInitialize,
    InstanceShutdown
    #endregion
}