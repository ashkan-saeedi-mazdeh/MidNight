using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;

[System.Serializable]
public class LobbyUserGroupInfo
{
    public string groupName;
    public bool isInvitationRequired;
    public List<string> members;

    public IEnumerator GetAccountsCollection(AccountsCollection col)
    {
        for (int i = 0; i < members.Count; ++i)
        {
            Result<AccountInfo> res = AccountManager.instance.GetAccountByUserID(members[i]);
            yield return res.WaitUntilDone();
            col.accounts.Add(res.Value);
        }
        yield break;
    }
}
