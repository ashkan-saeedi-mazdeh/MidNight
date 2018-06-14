using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.NetworkSystem;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Debug = UnityEngine.Debug;
public class InstanceManagerMaster : MonoBehaviour
{
    public static InstanceManagerMaster instance;

    private MasterConfig config;
    private NetworkServerSimple server;
    private bool isListenning;
    private Dictionary<int, SlaveInfo> slaves;
    private List<NetworkConnection> shuttingDownSlaves;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            UnityEngine.Debug.LogError("Only one instance of instance manager should exist");
            Destroy(this.gameObject);
            return;
        }
        server = new NetworkServerSimple();
        slaves = new Dictionary<int, SlaveInfo>();
        shuttingDownSlaves = new List<NetworkConnection>();
        Initialize(Application.dataPath + "\\..\\config.json");
    }

    private void Initialize(string configPath)
    {
        LoadConfig(Application.dataPath + "\\..\\master-config.json");
        isListenning = server.Listen(config.port);
        Debug.Log("master started listenning for nodes");
        server.RegisterHandler((short)CustomMessageTypes.StartInstanceRequest, netMsg => { HandleStartInstanceRequest(netMsg); });
        server.RegisterHandler((short)CustomMessageTypes.InstanceSlaveConnectRequest, netMsg => { HandleSlaveConnectRequest(netMsg); });
        server.RegisterHandler((short)CustomMessageTypes.InstanceSlaveShutdown, x => { HandleSlaveShutDown(x); });
        server.RegisterHandler(MsgType.Disconnect, x => { HandleDisconnect(x); });
        server.RegisterHandler((short)CustomMessageTypes.InstanceSlaveStartInstanceResponse, x =>
         {
             InstanceResponseData resp = x.ReadMessage<InstanceResponseData>();
             print(resp);
         });
    }

    private void HandleStartInstanceRequest(NetworkMessage netMsg)
    {
        StringMessage msg = netMsg.ReadMessage<StringMessage>();
        InstanceRequestData req = new InstanceRequestData();
        req.gameName = msg.value;
        req.instanceID = Guid.NewGuid().GetHashCode();
        bool result = StartInstance(req);
        IntegerMessage resultMessage = new IntegerMessage();
        resultMessage.value = (result) ? 1 : 0;
        netMsg.conn.Send((short)CustomMessageTypes.StartInstanceResponse, resultMessage);
    }

    private void HandleDisconnect(NetworkMessage x)
    {
        if (shuttingDownSlaves.Any(s => s.connectionId == x.conn.connectionId))
        {
            //normal DC with shutdown
            shuttingDownSlaves.Remove(x.conn);
        }
        else
        {
            //crash
        }
    }

    private void HandleSlaveShutDown(NetworkMessage x)
    {
        shuttingDownSlaves.Add(x.conn);
    }

    private void HandleSlaveConnectRequest(NetworkMessage netMsg)
    {
        var slaveInfo = netMsg.ReadMessage<SlaveInfo>();
        slaveInfo.networkConnection = netMsg.conn;
        int nodeID = Guid.NewGuid().GetHashCode();
        slaves.Add(nodeID, slaveInfo);
        netMsg.conn.Send((short)CustomMessageTypes.InstanceSlaveConnectResponse, new IntegerMessage(nodeID));
    }


    public bool StartInstance(InstanceRequestData request)
    {
        var gameConfigs = slaves.Where(x => x.Value.gameNames.Any(u => u == request.gameName)).ToList();
        if (gameConfigs.Count > 0)
        {
            //select a node
            var gameConfig = gameConfigs[UnityEngine.Random.Range(0, gameConfigs.Count)];
            //request the instance.
            gameConfig.Value.networkConnection.Send((short)CustomMessageTypes.InstanceSlaveStartInstanceRequest, request);
        }
        return false;
    }
    
    void Update()
    {
        if (server != null && isListenning)
        {
            server.Update();
        }
    }

    public void LoadConfig(string path)
    {
        config = JsonUtility.FromJson<MasterConfig>(System.IO.File.ReadAllText(path));
    }

    public void SaveConfig(string path)
    {
        var json = JsonUtility.ToJson(config, true);
        print(json);
        System.IO.File.WriteAllText(path, json);
    }
}
