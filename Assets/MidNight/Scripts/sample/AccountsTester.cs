using System.IO;
using UnityEngine;
using System.Collections;

public class AccountsTester : MonoBehaviour
{
    private AccountManager manager;

    IEnumerator Start()
    {
        LobbyServer.instance.Initialize(7000, 1);
        //Delete old saved data.
        var files = Directory.GetFiles(Application.dataPath + "\\..", "*.json");
        foreach (var f in files)
        {
            if(!f.Contains("config.json"))
            File.Delete(f);
        }

        manager = AccountManager.instance;
        manager.Initialize<JSONAccountStorage>();

        var reg = manager.RegisterAccount("Ashkan", "pass".GetHashCode(), "1");
        yield return reg.WaitUntilDone();
        reg = manager.RegisterAccount("Ramin", "pass".GetHashCode(), "1");
        yield return reg.WaitUntilDone();
        reg = manager.RegisterAccount("Sina", "pass".GetHashCode(), "1");
        yield return reg.WaitUntilDone();

        var loginRes = manager.Login("Ashkan", "pass".GetHashCode());
        yield return loginRes.WaitUntilDone();
        loginRes = manager.Login("Ramin", "pass".GetHashCode());
        yield return loginRes.WaitUntilDone();
        loginRes =  manager.Login("Sina", "pass".GetHashCode());
        yield return loginRes.WaitUntilDone();
        
        var getAccRes = manager.GetAccountByUserName("Ashkan");
        yield return getAccRes.WaitUntilDone();
        var a1 = getAccRes.Value;

        getAccRes = manager.GetAccountByUserName("Ramin");
        yield return getAccRes.WaitUntilDone();
        var a2 = getAccRes.Value;
        var addingReq = manager.InviteFriend(a2.userID, a1.userID);
        yield return addingReq.WaitUntilDone();
        var invsRes = manager.GetInvitations(a1.userID);
        yield return invsRes.WaitUntilDone();
        var invs = invsRes.Value;
        manager.AcceptInviation(a1.userID, invs.invites[0].invitationID);
        LobbyServer.instance.ShutDown();
    }

    
}
