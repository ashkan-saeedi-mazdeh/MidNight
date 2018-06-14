using UnityEngine;
using System;
using UnityEngine.Networking;

public class SlaveInfo : MessageBase
{
    public string ipAddress;
    public string[] gameNames;
    public NetworkConnection networkConnection;

    public override void Serialize(NetworkWriter writer)
    {
        writer.Write(gameNames.Length);
        for (int i = 0; i < gameNames.Length; ++i)
        {
            writer.Write(gameNames[i]);
        }
        writer.Write(ipAddress);
    }

    public override void Deserialize(NetworkReader reader)
    {
        int length = reader.ReadInt32();
        gameNames = new string[length];
        for (int i = 0; i < length; ++i)
        {
            gameNames[i] = reader.ReadString();
        }

        ipAddress = reader.ReadString();

    }

    public override string ToString()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine(ipAddress);
        for (int i = 0; i < gameNames.Length; ++i)
            sb.Append(gameNames[i] + " ");
        return sb.ToString();
    }
}

public class InstanceRequestData : MessageBase
{
    public string gameName;
    public int instanceID;
}

public class InstanceResponseData : MessageBase
{
    public bool success;
    public InstanceData data;

    public override string ToString()
    {
        return success + " " + data.ToString();
    }
}

public class InstanceData : MessageBase
{
    public int instanceID;
    public string gameName;
    public string ipAddress;
    public int port;

    public override string ToString()
    {
        return instanceID + " " + gameName + " " + ipAddress + ":" + port;
    }
}

/// <summary>
/// Instances use this to initialize themselves
/// </summary>
public class InstanceInitializationData : MessageBase
{
    public int processID;
    public int port;
}