using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FishNet;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    
    [SerializeField] private GameObject lobbyUI, mainUI, endScreenUI;

    [SerializeField] private TMP_Text useText, endScreenText;

    void Awake()
    {
        if (UIManager.Instance != null)  Destroy(this);
        else    Instance = this;

        
        useText.SetText("");
    }

    public void CloseLobbyScreen()
    {
        lobbyUI.SetActive(false);
        mainUI.SetActive(true);
    }

    public TMP_Text GetUseText()
    {
        return useText;
    }

    public void OpenEndScreen()
    {
        endScreenUI.SetActive(true);
    }

    public void ExitToMenu()
    {
        InstanceFinder.ClientManager.StopConnection();
    }
}
