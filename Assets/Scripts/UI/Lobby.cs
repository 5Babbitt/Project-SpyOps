using System.Collections;
using System.Collections.Generic;
using FishNet;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Lobby : MonoBehaviour
{
    public static Lobby Instance;
    
    [SerializeField] private TMP_Text playerName;
    [SerializeField] private Button readyButton, startButton;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }

    void Start()
    {
        if (!InstanceFinder.IsHost) startButton.gameObject.SetActive(false);
        //startButton.gameObject.SetActive(InstanceFinder.IsHost);
    }

    public void SetPlayerUsernameText(string name)
    {
        playerName.text = name;
    }

    public void IsReady()
    {
        Debug.Log("Pressed is Ready");
        Player.Instance.OnReady();

        if(InstanceFinder.IsHost) startButton.gameObject.SetActive(true);
    }

    public void OnStart()
    {
        GameManager.Instance.StartGame();
    }

    void Update()
    {
        if (Player.Instance != null)
            readyButton.image.color = Player.Instance.isReady ? Color.green : Color.red;
        

        startButton.interactable = GameManager.Instance.canStart;
    }
}
