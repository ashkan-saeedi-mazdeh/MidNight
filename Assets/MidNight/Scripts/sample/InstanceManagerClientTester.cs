using UnityEngine;
using System.Collections;

public class InstanceManagerClientTester : MonoBehaviour
{
    public string ip = "localhost";
    public int port = 6000;


    void Start()
    {
        InstanceManagerClient.instance.OnConnected += () =>
          {
              InstanceManagerClient.instance.StartInstance("milad-first");
          };
        InstanceManagerClient.instance.Connect(ip, port);
    }
}
