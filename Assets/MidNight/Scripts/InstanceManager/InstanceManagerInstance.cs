using System;
using System.Diagnostics;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.NetworkSystem;
using Debug = UnityEngine.Debug;

public class InstanceManagerInstance : MonoBehaviour
{
    public static InstanceManagerInstance instance;
    public event Action OnConnected;
    public event Action OnDisconnected;
    public event Action OnFailedToConnect;
    public event Action OnInstanceStarted;
    public event Action OnFailedToStartInstance;

    private NetworkClient networkClient;
    private bool isConnected;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("Only one instance of instance manager client exist");
            Destroy(this.gameObject);
            return;
        }

    }

    public void Initialize()
    {
        networkClient.RegisterHandler(MsgType.Connect, x =>
         {
             isConnected = true;
             if (OnConnected != null)
                 OnConnected();
         });

        networkClient.RegisterHandler(MsgType.Disconnect, x =>
        {
            if(isConnected)
            {
                isConnected = false;
                if (OnDisconnected != null)
                    OnDisconnected();
            }
            else
            {
                if (OnFailedToConnect != null)
                    OnFailedToConnect();
            }
            
        });
        networkClient.Connect("localhost", 40000);
    }

    public void ConnectInstanceToManager(int port)
    {
        InstanceInitializationData data = new InstanceInitializationData();
        data.port = port;
        data.processID = System.Diagnostics.Process.GetCurrentProcess().Id;
        if(networkClient != null && networkClient.isConnected)
            networkClient.Send((short)CustomMessageTypes.InstanceInitialize, data);
    }

    public void ShutDown()
    {
        if (networkClient != null && networkClient.isConnected)
            networkClient.Send((short)CustomMessageTypes.InstanceShutdown, new EmptyMessage());
    }
}