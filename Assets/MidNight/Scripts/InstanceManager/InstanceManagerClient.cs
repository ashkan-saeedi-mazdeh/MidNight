using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.NetworkSystem;

/// <summary>
/// This is the client of the instance manager which requests instances from the master. Most of the times this is from Lobby.
/// </summary>
public class InstanceManagerClient : MonoBehaviour
{
    public static InstanceManagerClient instance;
    public event Action OnConnected;
    public event Action OnDisconnected;
    public event Action OnFailedToConnect;
    public event Action OnInstanceStarted;
    public event Action OnFailedToStartInstance;

    private NetworkClient networkClient;
    private bool isConnected;

    // Use this for initialization
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

    public void StartInstance(string gameName)
    {
        StringMessage msg = new StringMessage();
        msg.value = gameName;
        networkClient.Send((short)CustomMessageTypes.StartInstanceRequest, msg);
    }

    public void Connect(string ip,int port)
    {
        networkClient = new NetworkClient();
        networkClient.RegisterHandler((short)CustomMessageTypes.StartInstanceResponse, x =>
         {
             IntegerMessage resp = x.ReadMessage<IntegerMessage>();
             if(resp.value == 1)
             {
                 if (OnInstanceStarted != null)
                     OnInstanceStarted();

             }
             else
             {
                 if (OnFailedToStartInstance != null)
                     OnFailedToStartInstance();

             }
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
        networkClient.RegisterHandler(MsgType.Connect, x =>
         {
             isConnected = true;
             if (OnConnected != null)
                 OnConnected();
         });
        networkClient.Connect(ip, port);
    }
}