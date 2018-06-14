using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Linq;

public interface IAccountStorage
{

    IEnumerator AddAccount(string userName, int passwordHash, string userID, string data,Result<bool> result);
    IEnumerator GetAccountByUserID(string userID,Result<StorageAccountInfo> result);
    IEnumerator GetAccountByUserName(string userName,Result<StorageAccountInfo> result);
    IEnumerator IsLoginDataCorrect(string userName, int passwordHash,Result<bool> result);
    bool Initialize();
    string GenerateAccountID();
    IEnumerator SetFriends(string accountID, LobbyUserGroupInfo friends,Result<bool> result);
    IEnumerator GetFriends(string userID,Result<LobbyUserGroupInfo> result);
    IEnumerator SetInvitations(string userID, StorageInvitationsCollection invitations,Result<bool> result);
    IEnumerator GetInvitations(string userID,Result<StorageInvitationsCollection> result);
}

[System.Serializable]
public class StorageAccountCollection
{
    public List<StorageAccountInfo> accounts;

    public StorageAccountCollection()
    {
        accounts = new List<StorageAccountInfo>();
    }
}

[System.Serializable]
public class StorageInvitationsCollection
{
    public List<InvitationInfo> invites;

    

    public StorageInvitationsCollection()
    {
        invites = new List<InvitationInfo>();
    }
}