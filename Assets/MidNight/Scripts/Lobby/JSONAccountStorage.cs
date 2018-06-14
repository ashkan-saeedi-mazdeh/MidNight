using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using System;
using System.IO;
using System.Linq;

public class JSONAccountStorage : IAccountStorage
{
    

    public string storagePath;
    private List<StorageAccountInfo> accounts;
    public Dictionary<string, LobbyUserGroupInfo> friends;
    public Dictionary<string,StorageInvitationsCollection> invitations;

    public IEnumerator AddAccount(string userName, int passwordHash, string userID, string data,Result<bool> result)
    {
        accounts.Add(new StorageAccountInfo { userName = userName, passwordHash = passwordHash, userID = userID, data = data });
        SaveJSONFile("accounts.json", JsonUtility.ToJson(new StorageAccountCollection { accounts = this.accounts }, true));
        result.SetValue(true);
        yield break;
    }

    public string GenerateAccountID()
    {
        return System.Guid.NewGuid().ToString();
    }

    public IEnumerator GetAccountByUserID(string userID,Result<StorageAccountInfo> result)
    {
        var acc = accounts.FirstOrDefault(account => account.userID == userID);
        result.SetValue(acc);
        yield break;
    }

    public IEnumerator GetAccountByUserName(string userName,Result<StorageAccountInfo> result)
    {
        var acc = accounts.FirstOrDefault(account => account.userName == userName);
        result.SetValue(acc);
        yield break;
    }

    public IEnumerator GetFriends(string userID,Result<LobbyUserGroupInfo> result)
    {
        if (!friends.ContainsKey(userID))
        {
            var info = new LobbyUserGroupInfo();
            info.isInvitationRequired = true;
            info.members = new List<string>();
            friends[userID] = info;
        }
        result.SetValue(friends[userID]);
        yield break;

    }

    public IEnumerator GetInvitations(string userID,Result<StorageInvitationsCollection> result)
    {
        if (!invitations.ContainsKey(userID))
        {
            invitations[userID] = new StorageInvitationsCollection();
        }
        result.SetValue(invitations[userID]);
        yield break;
    }

    public bool Initialize()
    {
        try
        {
            accounts = new List<StorageAccountInfo>();
            friends = new Dictionary<string, LobbyUserGroupInfo>();
            invitations = new Dictionary<string, StorageInvitationsCollection>();
            if (File.Exists("accounts.json"))
                accounts = JsonUtility.FromJson<StorageAccountCollection>(LoadJSONFile("accounts.json")).accounts;
            foreach (var acc in accounts)
            {
                LoadFriendsAndInvites(acc.userID);
            }
            return true;
        }
        catch (Exception x)
        {
            Debug.LogError(x.Message);
            return false;
        }
    }

    private bool LoadFriendsAndInvites(string userID)
    {
        if (File.Exists("inv-" + userID + ".json"))
            invitations[userID] = JsonUtility.FromJson<StorageInvitationsCollection>(LoadJSONFile("inv-" + userID + ".json"));
        if (File.Exists("friends-" + userID + ".json"))
            friends[userID] = JsonUtility.FromJson<LobbyUserGroupInfo>(LoadJSONFile("friends-" + userID + ".json"));
        return true;
    }

    public IEnumerator IsLoginDataCorrect(string userName, int passwordHash,Result<bool> result)
    {
        result.SetValue(accounts.Any(x => x.userName == userName && x.passwordHash == passwordHash));
        yield break;
    }

    public IEnumerator SetFriends(string userID, LobbyUserGroupInfo friends,Result<bool> result)
    {
        this.friends[userID] = friends;
        SaveJSONFile("friends-" + userID + ".json", JsonUtility.ToJson(friends, true));
        result.SetValue(true);
        yield break;
    }

    public IEnumerator SetInvitations(string userID,StorageInvitationsCollection invitations,Result<bool> result)
    {
        this.invitations[userID] = invitations;
        SaveJSONFile("inv-" + userID + ".json", JsonUtility.ToJson(invitations, true));
        result.SetValue(true);
        yield break;
    }

    private void SaveJSONFile(string file, string json)
    {
        if(Debug.isDebugBuild)
            Debug.Log("JSON " + json);
        File.WriteAllText(storagePath + file, json);
    }

    private string LoadJSONFile(string file)
    {
        if (File.Exists(storagePath + file))
            return File.ReadAllText(storagePath + file);
        return "";
    }
}
