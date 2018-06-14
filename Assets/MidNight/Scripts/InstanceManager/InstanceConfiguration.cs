using UnityEngine.Networking;

/// <summary>
/// Holds configuration for all startable instances
/// </summary>
public class SlaveConfiguration : MessageBase
{
    /// <summary>
    /// IP of the master.
    /// </summary>
    public string masterIP;

    /// <summary>
    /// The port that master is listenning to.
    /// </summary>
    public int masterPort;

    /// <summary>
    /// We use this IP address for clients to connect to the instance.
    /// </summary>
    /// <remarks>If the machine running the instance has multiple NICs, You can use this to choose which IP to use for communication.</remarks>
    public string IPAddress;
    
    /// <summary>
    /// All startable instance types
    /// </summary>
    public GameServerInstance[] instances;
}

[System.Serializable]
public class GameServerInstance
{
    public string gameName;
    public string path;
    public string commandLineArguments;
}

/// <summary>
/// Configuration for the instance manager master.
/// </summary>
[System.Serializable]
public class MasterConfig
{
    /// <summary>
    /// The port that master listens to.
    /// </summary>
    public int port;
}