using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.NetworkSystem;


public class LoginRequestData : MessageBase
{
    public string userName;
    public int passwordHash;
}

public class LoginResponseData : MessageBase
{
    public AccountInfo account;
    public bool isSuccessful;

    public override void Serialize(NetworkWriter writer)
    {
        writer.Write(isSuccessful);
        if (isSuccessful)
        {
            writer.Write(account);
        }
    }

    public override void Deserialize(NetworkReader reader)
    {
        isSuccessful = reader.ReadBoolean();
        if (isSuccessful)
        {
            account = reader.ReadMessage<AccountInfo>();
        }
    }
}

public class BooleanMessage : MessageBase
{
    public bool value;
}

public class RegisterAccountRequestData : MessageBase
{
    public string userName;
    public int passwordHash;
    public string data;
}

public class GetFriendsListResponseData : MessageBase
{
    public AccountsCollection friends;
    public bool success;

    public override void Serialize(NetworkWriter writer)
    {
        writer.Write(success);
        if(success)
        {
            writer.Write(friends);
        }
    }

    public override void Deserialize(NetworkReader reader)
    {
        success = reader.ReadBoolean();
        if(success)
        {
            friends = reader.ReadMessage<AccountsCollection>();
        }
    }
}

public class AccountsCollection : MessageBase
{
    public List<AccountInfo> accounts;

    public override void Serialize(NetworkWriter writer)
    {
        writer.Write(accounts.Count);
        for (int i=0;i<accounts.Count;++i)
        {
            writer.Write(accounts[i]);
        }
    }

    public override void Deserialize(NetworkReader reader)
    {
        accounts = new List<AccountInfo>();
        int count = reader.ReadInt32();
        for (int i=0;i<count;++i)
        {
            accounts.Add(reader.ReadMessage<AccountInfo>());
        }
    }

    public AccountsCollection()
    {
        accounts = new List<AccountInfo>();
    }
}


public class Invitation : MessageBase
{
    public string inviterName;
    public string invitationID;

    public Invitation(string name,string id)
    {
        this.inviterName = name;
        this.invitationID = id;
    }

    public Invitation()
    {

    }
}

public class InvitationsCollection : MessageBase
{
    public bool success;
    public List<Invitation> invitations;

    public InvitationsCollection()
    {
        invitations = new List<Invitation>();
    }

    public override void Serialize(NetworkWriter writer)
    {
        writer.Write(success);
        if (success)
        {
            writer.Write(invitations.Count);
            for (int i = 0; i < invitations.Count; ++i)
                writer.Write(invitations[i]);
        }
    }

    public override void Deserialize(NetworkReader reader)
    {
        invitations = new List<Invitation>();
        success = reader.ReadBoolean();
        if (success)
        {
            var count = reader.ReadInt32();
            for (int i = 0; i < count; ++i)
                invitations.Add(reader.ReadMessage<Invitation>());
        }
    }
}