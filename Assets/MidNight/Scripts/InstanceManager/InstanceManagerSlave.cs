using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using System.Diagnostics;
using System.Linq;
using Debug = UnityEngine.Debug;

/// <summary>
/// Each slave manages instances on one machine.
/// It connects to the master automatically based on the config file and then can accept requests to start and stop instances.
/// Also it will inform master about instance state changes like disconnection, crashing and starting.
/// </summary>
/// <remarks>
/// Having the config file is mandatory. A config.json should be present beside the slave application, otherwise the app can not start. also correct
/// permissions should be available for reading the config file and accessing processes mentioned in the config as instances.
/// </remarks>
public class InstanceManagerSlave : MonoBehaviour
{
    /// <summary>
    /// We use this client to connect to master.
    /// </summary>
    private NetworkClient nc;

    /// <summary>
    /// Configuration is loaded to this variable.
    /// </summary>
    private SlaveConfiguration config;

    /// <summary>
    /// ID of the node which is unique in the node collection of master.
    /// </summary>
    /// <remarks>When disconnected, this is not valid.</remarks>
    private int slaveID;

    /// <summary>
    /// Holds all of the running processes which are under the command of this slave.
    /// </summary>
    private Dictionary<int, NetworkConnection> runningInstances;
    private Dictionary<int, InstanceData> InitializedInstances;

    void Awake()
    {
        //Try to load the config and initialize the node , otherwise simply crash.
        try
        {
            runningInstances = new Dictionary<int, NetworkConnection>();
            InitializedInstances = new Dictionary<int, InstanceData>();
            LoadConfig(Application.dataPath + "\\..\\config.json");
            if (config != null)
            {
                nc = new NetworkClient();
                nc.RegisterHandler((short)CustomMessageTypes.InstanceSlaveStartInstanceRequest, x => { HandleStartInstanceRequest(x); });
                nc.RegisterHandler(MsgType.Connect, x => { HandleOnConnectedToMaster(nc); });
                nc.RegisterHandler((short)CustomMessageTypes.InstanceSlaveConnectResponse, x => { HandleConnectResponseFromMaster(x); });
                nc.RegisterHandler(MsgType.Disconnect, x => { HandleDisconnectFromMaster(); });

                nc.Connect(config.masterIP, config.masterPort);
            }
            else //Could not load config so crash.
            {
                UnityEngine.Debug.LogError("Could not load the config");
                Application.Quit();
            }
        }
        catch (System.Exception x) //some operation caused the node to not start, things like not being able to read file or ...
        {
            UnityEngine.Debug.LogError(x.Message);
            Application.Quit();
        }
    }

    private void HandleDisconnectFromMaster()
    {
        slaveID = 0;

    }

    private void HandleConnectResponseFromMaster(NetworkMessage x)
    {
        IntegerMessage msg = x.ReadMessage<IntegerMessage>();
        slaveID = msg.value;

    }

    private void HandleOnConnectedToMaster(NetworkClient nc)
    {
        NetworkServer.Listen(40000);
        NetworkServer.RegisterHandler((short)CustomMessageTypes.InstanceInitialize, x => { HandleInstanceInitialization(x); });
        NetworkServer.RegisterHandler(MsgType.Disconnect, x =>
         {
             var p = runningInstances.FirstOrDefault(y => y.Value.connectionId == x.conn.connectionId);
             //Since FirstOrDefault doesn't throw, We should make sure if it found the value 
             if (p.Value.connectionId == x.conn.connectionId)
             {
                 runningInstances.Remove(p.Key);
                 InitializedInstances.Remove(p.Key);
             }
         });

        //Send slave info to master.
        SlaveInfo info = new SlaveInfo();
        info.ipAddress = config.IPAddress;
        info.gameNames = new string[config.instances.Length];
        for (int i = 0; i < info.gameNames.Length; ++i)
        {
            info.gameNames[i] = config.instances[i].gameName;
        }
        nc.Send((short)CustomMessageTypes.InstanceSlaveConnectRequest, info);
    }

    private void HandleInstanceInitialization(NetworkMessage x)
    {
        var msg = x.ReadMessage<InstanceInitializationData>();
        runningInstances[msg.processID] = x.conn;
        InitializedInstances[msg.processID].port = msg.port;
        var data = InitializedInstances[msg.processID];
        InstanceResponseData resp = new InstanceResponseData();
        resp.success = true;
        resp.data = data;
        x.conn.Send((short)CustomMessageTypes.InstanceSlaveStartInstanceResponse, resp);
    }

    private void HandleStartInstanceRequest(NetworkMessage x)
    {
        InstanceRequestData msg = x.ReadMessage<InstanceRequestData>();
        if (!string.IsNullOrEmpty(msg.gameName))
        {
            ProcessStartInfo info = new ProcessStartInfo();
            var instance = config.instances.FirstOrDefault(u => u.gameName == msg.gameName);
            info.FileName = instance.path;
            info.Arguments = instance.commandLineArguments;
            var p = Process.Start(info);
            InitializedInstances[p.Id] = new InstanceData();
            var data = InitializedInstances[p.Id];
            data = new InstanceData();
            data.gameName = msg.gameName;
            data.ipAddress = config.IPAddress;
            data.port = 0; //will be filled later on when instance responds.
            data.instanceID = msg.instanceID;
            runningInstances[p.Id] = null;
        }
    }

    private void ShutDownSlave()
    {
        if (nc != null && nc.isConnected)
            nc.Send((short)CustomMessageTypes.InstanceSlaveShutdown, new IntegerMessage(slaveID));

        Application.Quit();
    }

    void OnApplicationQuit()
    {
        if (nc != null && nc.isConnected)
            nc.Disconnect();
    }

    public void LoadConfig(string path)
    {
        config = JsonUtility.FromJson<SlaveConfiguration>(System.IO.File.ReadAllText(path));

    }

    public void SaveConfig(string path)
    {
        var json = JsonUtility.ToJson(config, true);
        print(json);
        System.IO.File.WriteAllText(path, json);
    }

}
