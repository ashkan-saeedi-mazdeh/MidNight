using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.NetworkSystem;

public class FindMatchResponseData : MessageBase
{
    public bool isMatchFound;
    public GameServerInfo serverInfo;

    public override void Serialize(NetworkWriter writer)
    {
        writer.Write(isMatchFound);
        if (isMatchFound)
        {
            writer.Write(serverInfo);
        }
    }

    public override void Deserialize(NetworkReader reader)
    {
        isMatchFound = reader.ReadBoolean();
        if (isMatchFound)
        {
            serverInfo = reader.ReadMessage<GameServerInfo>();
        }
    }
}
