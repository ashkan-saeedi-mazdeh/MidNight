using UnityEngine;
using System;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;

[System.Serializable]
public class StorageAccountInfo
{
    public string userID;
    public string userName;
    public int passwordHash;
    public string data;

    public override string ToString()
    {
        return userID + " " + userName + " " + passwordHash + " " + data;
    }
}

public class AccountInfo : MessageBase, IEquatable<AccountInfo>
{
    public string userID;
    public string userName;
    public string data;


    public AccountInfo()
    {
    }

    public AccountInfo(string id, string name, string data)
    {
        this.userID = id;
        this.userName = name;
        this.data = data;
    }

    public override string ToString()
    {
        return string.Format("{0} {1} {2}", userID, userName, data);
    }

    public bool Equals(AccountInfo other)
    {
        return other.userName == this.userName && other.userID == this.userID && other.data == this.data;
    }
}
