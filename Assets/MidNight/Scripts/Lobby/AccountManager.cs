#if LOBBY
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.NetworkSystem;
using System.Linq;

/// <summary>
/// fast code for testing the system.
/// </summary>
public class AccountManager : MonoBehaviour
{
    public static AccountManager instance;

    public Action<AccountInfo> OnLoggedIn;
    public Action OnLoginFailed;

    private IAccountStorage storage;
    private Dictionary<AccountInfo, NetworkConnection> loggedInAccounts = new Dictionary<AccountInfo, NetworkConnection>();

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Only one instance of AccountManager should exist");
            return;

        }
        instance = this;
    }

    public bool Initialize<T>() where T : IAccountStorage, new()
    {
        storage = new T();
        if (storage.Initialize())
        {
            NetworkServer.RegisterHandler((short)CustomMessageTypes.LoginRequest, x => { StartCoroutine(HandleLoginRequest(x)); });
            NetworkServer.RegisterHandler((short)CustomMessageTypes.LogoutRequest, x => StartCoroutine(HandleLogoutRequestLogout(x.conn)));
            NetworkServer.RegisterHandler((short)CustomMessageTypes.RegisterAccountRequest, x => { StartCoroutine(HandleRegisterAccountRequest(x)); });
            NetworkServer.RegisterHandler((short)CustomMessageTypes.GetFriendslistRequest, x => { StartCoroutine(HandleGetFriendsListRequest(x)); });
            NetworkServer.RegisterHandler((short)CustomMessageTypes.InviteFriendRequest, x => { StartCoroutine(HandleInviteFriendRequest(x)); });
            NetworkServer.RegisterHandler((short)CustomMessageTypes.GetInvitationsRequest, x => { StartCoroutine(HandleGetInvitationsRequest(x)); });
            NetworkServer.RegisterHandler((short)CustomMessageTypes.AcceptInvitationRequest, x => { StartCoroutine(HandleAcceptInvitationRequest(x)); });
            NetworkServer.RegisterHandler((short)CustomMessageTypes.DenyInvitationRequest, x => { StartCoroutine(HandleDenyInvitationRequest(x)); });
            return true;
        }
        return false;
    }

    private IEnumerator HandleDenyInvitationRequest(NetworkMessage x)
    {
        BooleanMessage resp = new BooleanMessage();
        var user = GetAccountByConnection(x.conn);
        var invID = x.ReadMessage<StringMessage>();
        if (user != null)
        {
            Result<bool> r = DenyInviation(user.userID, invID.value);
            yield return r.WaitUntilDone();
            resp.value = r.Value;

        }
        else
        {
            resp.value = false;
        }
        x.conn.Send((short)CustomMessageTypes.DenyInvitationResponse, resp);
        yield break;
    }

    private IEnumerator HandleAcceptInvitationRequest(NetworkMessage x)
    {
        BooleanMessage resp = new BooleanMessage();
        var user = GetAccountByConnection(x.conn);
        var invID = x.ReadMessage<StringMessage>();
        if (user != null)
        {
            Result<bool> r = AcceptInviation(user.userID, invID.value);
            yield return r.WaitUntilDone();
            resp.value = r.Value;

        }
        else
        {
            resp.value = false;
        }
        x.conn.Send((short)CustomMessageTypes.AcceptInvitationResponse, resp);
        yield break;
    }

    private IEnumerator HandleGetInvitationsRequest(NetworkMessage x)
    {
        InvitationsCollection response = new InvitationsCollection();
        var user = GetAccountByConnection(x.conn);
        if (user != null)
        {
            Result<StorageInvitationsCollection> r = GetInvitations(user.userID);
            yield return r.WaitUntilDone();

            var invs = r.Value;
            response.success = true;
            foreach (var inv in invs.invites)
            {
                Result<AccountInfo> r2 = GetAccountByUserID(inv.inviterID);
                yield return r2.WaitUntilDone();
                var usr = r2.Value;
                response.invitations.Add(new Invitation(usr.userName, inv.invitationID));
            }
        }
        else
        {
            response.success = false;
        }
        x.conn.Send((short)CustomMessageTypes.GetInvitationsResponse, response);
    }

    private IEnumerator HandleInviteFriendRequest(NetworkMessage x)
    {
        var user = GetAccountByConnection(x.conn);
        Result<AccountInfo> rr = GetAccountByUserName(x.ReadMessage<StringMessage>().value);
        yield return rr.WaitUntilDone();
        var invitee = rr.Value;
        BooleanMessage resp = new BooleanMessage();
        if (user != null && invitee != null)
        {
            Result<bool> r = InviteFriend(user.userID, invitee.userID);
            yield return r.WaitUntilDone();
            resp.value = r.Value;
        }
        else
        {
            resp.value = false;
        }
        x.conn.Send((short)CustomMessageTypes.InviteFriendResponse, resp);
        yield break;
    }

    private IEnumerator HandleGetFriendsListRequest(NetworkMessage x)
    {
        var user = GetAccountByConnection(x.conn);
        GetFriendsListResponseData resp = new GetFriendsListResponseData();
        if (user != null)
        {
            Result<LobbyUserGroupInfo> res = GetFriends(user.userID);
            yield return res.WaitUntilDone();
            var friends = res.Value;

            resp.success = true;
            AccountsCollection col = new AccountsCollection();
            yield return StartCoroutine(friends.GetAccountsCollection(col));
            resp.friends = col;

        }
        else
        {
            resp.success = false;
        }
        x.conn.Send((short)CustomMessageTypes.GetFriendslistResponse, resp);
    }

    private IEnumerator HandleRegisterAccountRequest(NetworkMessage x)
    {
        var msg = x.ReadMessage<RegisterAccountRequestData>();
        Result<bool> r = RegisterAccount(msg.userName, msg.passwordHash, msg.data);
        yield return r.WaitUntilDone();
        var success = r.Value;
        BooleanMessage resp = new BooleanMessage();
        resp.value = success;
        x.conn.Send((short)CustomMessageTypes.RegisterAccountResponse, resp);
        yield break;
    }

    private IEnumerator HandleLoginRequest(NetworkMessage x)
    {
        var msg = x.ReadMessage<LoginRequestData>();
        LoginResponseData resp = new LoginResponseData();
        Result<bool> r2 = Login(msg.userName, msg.passwordHash, x.conn);
        yield return r2.WaitUntilDone();
        if (r2.Value)
        {

            resp.isSuccessful = true;
            Result<AccountInfo> r = GetAccountByUserName(msg.userName);
            yield return r.WaitUntilDone();
            var user = r.Value;
            resp.account = new AccountInfo(user.userID, user.userName, user.data);
            if (OnLoggedIn != null)
                OnLoggedIn(new AccountInfo(user.userID, user.userName, user.data));
        }
        else
        {
            resp.isSuccessful = false;
            if (OnLoginFailed != null)
                OnLoginFailed();
        }
        x.conn.Send((short)CustomMessageTypes.LoginResponse, resp);
    }

    public IEnumerator HandleLogoutRequestLogout(NetworkConnection con)
    {
        BooleanMessage resp = new BooleanMessage();
        var user = GetAccountByConnection(con);
        if (user != null)
        {
            var success = Logout(user.userID);

            resp.value = success;
            con.Send((short)CustomMessageTypes.LogoutResponse, resp);
            yield break;
        }
        resp.value = false;
        con.Send((short)CustomMessageTypes.LogoutResponse, resp);
    }

    public Result<bool> RegisterAccount(string userName, int passwordHash, string data)
    {
        Result<bool> r = new Result<bool>(this);
        StartCoroutine(RegisterAccount(userName, passwordHash, data, r));
        return r;
    }

    private IEnumerator RegisterAccount(string userName, int passwordHash, string data, Result<bool> result)
    {
        Result<bool> r = new Result<bool>(this);
        yield return StartCoroutine(storage.AddAccount(userName, passwordHash, storage.GenerateAccountID(), data, r));
        result.SetValue(r.Value);
    }

    public Result<AccountInfo> GetAccountByUserID(string userID)
    {
        Result<AccountInfo> r = new Result<AccountInfo>(this);
        StartCoroutine(GetAccountByUserID(userID, r));
        return r;
    }

    private IEnumerator GetAccountByUserID(string userID, Result<AccountInfo> result)
    {
        Result<StorageAccountInfo> r = new Result<StorageAccountInfo>(this);
        yield return StartCoroutine(storage.GetAccountByUserID(userID, r));
        var storageUser = r.Value;
        result.SetValue(new AccountInfo(storageUser.userID, storageUser.userName, storageUser.data));
    }

    public Result<AccountInfo> GetAccountByUserName(string userName)
    {
        Result<AccountInfo> r = new Result<AccountInfo>(this);
        StartCoroutine(GetAccountByUserName(userName, r));
        return r;
    }

    public IEnumerator GetAccountByUserName(string userName, Result<AccountInfo> result)
    {
        Result<StorageAccountInfo> r = new Result<StorageAccountInfo>(this);
        yield return StartCoroutine(storage.GetAccountByUserName(userName, r));
        var user = r.Value;
        result.SetValue(new AccountInfo(user.userID, user.userName, user.data));
    }

    public Result<bool> Login(string userName, int passwordHash, NetworkConnection con = null)
    {
        Result<bool> r = new Result<bool>(this);
        StartCoroutine(Login(userName, passwordHash, r,con));
        return r;
    }
    private IEnumerator Login(string userName, int passwordHash, Result<bool> result, NetworkConnection con = null)
    {
        if (loggedInAccounts.Any(x => x.Key.userName == userName))
        {
            Debug.LogWarning("already loggedIn!");
            result.SetValue(false);
            yield break;
        }
        yield return StartCoroutine(storage.IsLoginDataCorrect(userName, passwordHash, result));
        if (result.Value)
        {
            Result<StorageAccountInfo> r = new Result<StorageAccountInfo>(this);
            yield return StartCoroutine(storage.GetAccountByUserName(userName, r));
            var user = r.Value;
            loggedInAccounts.Add(new AccountInfo(user.userID, user.userName, user.data), con);
        }
        yield break;
    }



    public AccountInfo GetAccountByConnection(NetworkConnection con)
    {
        if (con == null)
            return null;
        var kvp = loggedInAccounts.FirstOrDefault(x => x.Value != null && x.Value.connectionId == con.connectionId);
        return (kvp.Key != null) ? kvp.Key : null;
    }

    public NetworkConnection GetConnectionByLoggedinAccount(AccountInfo acc)
    {
        if (acc == null)
            return null;
        var conn = loggedInAccounts.FirstOrDefault(x=>x.Key.userName == acc.userName).Value;
        return conn;
    }

    public bool Logout(string accountID)
    {
        var user = loggedInAccounts.FirstOrDefault(x => x.Key.userID == accountID).Key;
        if (user != null)
        {
            loggedInAccounts.Remove(user);
            return true;
        }
        return false;
    }

    public Result<bool> AddFriend(string userID, string friendID)
    {
        Result<bool> r = new Result<bool>(this);
        StartCoroutine(AddFriend(userID, friendID, r));
        return r;
    }


    private IEnumerator AddFriend(string userID, string friendID, Result<bool> result)
    {
        Result<StorageAccountInfo> res = new Result<StorageAccountInfo>(this);
        yield return StartCoroutine(storage.GetAccountByUserID(userID, res));
        var user = res.Value;
        var friend = res.Value;
        if (user != null && friend != null)
        {
            Result<LobbyUserGroupInfo> r = new Result<LobbyUserGroupInfo>(this);
            yield return StartCoroutine(storage.GetFriends(user.userID, r));
            var friendsList = r.Value;
            friendsList.members.Add(friend.userID);
            Result<bool> rr = new Result<bool>(this);
            yield return StartCoroutine(storage.SetFriends(user.userID, friendsList, rr));
            result.SetValue(rr.Value);
            yield break;
        }
        result.SetValue(false);
        yield break;
    }

    public Result<bool> RemoveFriend(string userID, string friendID)
    {
        Result<bool> r = new Result<bool>(this);
        StartCoroutine(RemoveFriend(userID, friendID, r));
        return r;
    }

    private IEnumerator RemoveFriend(string userID, string friendID, Result<bool> result)
    {
        Result<StorageAccountInfo> res = new Result<StorageAccountInfo>(this);
        yield return StartCoroutine(storage.GetAccountByUserID(userID, res));
        var user = res.Value;
        if (user != null)
        {
            Result<LobbyUserGroupInfo> res2 = new Result<LobbyUserGroupInfo>(this);
            yield return StartCoroutine(storage.GetFriends(user.userID, res2));
            var friendsList = res2.Value;
            friendsList.members.Remove(friendID);
            Result<bool> res3 = new Result<bool>(this);
            yield return StartCoroutine(storage.SetFriends(user.userID, friendsList, res3));
            result.SetValue(res3.Value);
            yield break;
        }
        result.SetValue(false);
    }

    public Result<LobbyUserGroupInfo> GetFriends(string userID)
    {
        Result<LobbyUserGroupInfo> r = new Result<LobbyUserGroupInfo>(this);
        StartCoroutine(GetFriends(userID, r));
        return r;
    }

    private IEnumerator GetFriends(string userID, Result<LobbyUserGroupInfo> result)
    {
        Result<StorageAccountInfo> res = new Result<StorageAccountInfo>(this);
        yield return StartCoroutine(storage.GetAccountByUserID(userID, res));
        StorageAccountInfo user = res.Value;
        if (user != null)
        {
            Result<LobbyUserGroupInfo> res2 = new Result<LobbyUserGroupInfo>(this);
            yield return StartCoroutine(storage.GetFriends(user.userID, res2));
            var friendsList = res2.Value;
            result.SetValue(friendsList);
            yield break;
        }
        result.SetValue(null);
    }

    public Result<bool> InviteFriend(string userID, string friendID)
    {
        Result<bool> r = new Result<bool>(this);
        StartCoroutine(InviteFriend(userID, friendID, r));
        return r;
    }

    private IEnumerator InviteFriend(string userID, string friendID, Result<bool> result)
    {
        Result<StorageAccountInfo> r = new Result<StorageAccountInfo>(this);
        yield return StartCoroutine(storage.GetAccountByUserID(userID, r));
        var user = r.Value;
        yield return StartCoroutine(storage.GetAccountByUserID(friendID, r));
        var friend = r.Value;
        if (user != null && friend != null)
        {
            Result<StorageInvitationsCollection> res = new Result<StorageInvitationsCollection>(this);
            yield return StartCoroutine(storage.GetInvitations(friend.userID, res));
            var invitaitonList = res.Value;
            var invitation = new InvitationInfo();
            invitation.invitationID = System.Guid.NewGuid().ToString();
            invitation.inviterID = userID;
            invitation.InviteeID = friendID;
            invitation.groupName = "";
            invitaitonList.invites.Add(invitation);
            yield return StartCoroutine(storage.SetInvitations(friend.userID, invitaitonList, result));
            yield break;
        }
        result.SetValue(false);
    }

    public Result<StorageInvitationsCollection> GetInvitations(string userID)
    {
        Result<StorageInvitationsCollection> r = new Result<StorageInvitationsCollection>(this);
        StartCoroutine(GetInvitations(userID, r));
        return r;
    }

    private IEnumerator GetInvitations(string userID, Result<StorageInvitationsCollection> result)
    {
        yield return StartCoroutine(storage.GetInvitations(userID, result));
    }

    public Result<bool> AcceptInviation(string userID, string invitationID)
    {
        Result<bool> r = new Result<bool>(this);
        StartCoroutine(AcceptInviation(userID, invitationID, r));
        return r;
    }

    private IEnumerator AcceptInviation(string userID, string invitationID, Result<bool> result)
    {
        Result<StorageInvitationsCollection> res = new Result<StorageInvitationsCollection>(this);
        yield return StartCoroutine(storage.GetInvitations(userID, res));
        var invs = res.Value;
        var inv = invs.invites.FirstOrDefault(x => x.invitationID == invitationID);
        if (inv != null)
        {
            var r1 = AddFriend(inv.InviteeID, inv.inviterID);
            yield return r1.WaitUntilDone();
            r1 = AddFriend(inv.inviterID, inv.InviteeID);
            yield return r1.WaitUntilDone();
            invs.invites.Remove(inv);
            yield return StartCoroutine(storage.SetInvitations(userID, invs, new Result<bool>(this)));
            result.SetValue(true);
            yield break;
        }
        result.SetValue(false);
    }

    public Result<bool> DenyInviation(string userID, string invitationID)
    {
        Result<bool> r = new Result<bool>(this);
        StartCoroutine(DenyInviation(userID, invitationID, r));
        return r;
    }
    private IEnumerator DenyInviation(string userID, string invitationID, Result<bool> result)
    {
        Result<StorageInvitationsCollection> res = new Result<StorageInvitationsCollection>(this);
        yield return StartCoroutine(storage.GetInvitations(userID, res));
        var invs = res.Value;
        var inv = invs.invites.FirstOrDefault(x => x.invitationID == invitationID);
        if (inv != null)
        {
            invs.invites.Remove(inv);
            yield return StartCoroutine(storage.SetInvitations(userID, invs, new Result<bool>(this)));
            result.SetValue(true);
            yield break;
        }
        result.SetValue(false);
    }
}

#endif