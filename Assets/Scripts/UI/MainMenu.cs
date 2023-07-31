using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;

public class MainMenu : MonoBehaviour
{
    
    
    public void HostLobby()
    {
        InstanceFinder.ServerManager.StartConnection();
        InstanceFinder.ClientManager.StartConnection();
    }

    public void JoinLobby()
    {
        InstanceFinder.ClientManager.StartConnection();
    }

    public void Exit()
    {
        Application.Quit();
    }
}
