using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FishNet.Object;
using FishNet.Connection;

public class OperatorUI : NetworkBehaviour
{
    [SerializeField] private Operator _operator;
    [SerializeField] private GameObject operatorUI;

    
    [Header("Header Settings")]
    [SerializeField] private TMP_Text time;
    [SerializeField] private TMP_Text headerTitle;

    [Header("Main Display Settings")]
    [SerializeField] private GameObject mapDisplay;
    [SerializeField] private GameObject camDisplay, droneDisplay;

    [SerializeField] private RawImage mapDisplayImage, camDisplayImage, droneDisplayImage;

    [Header("Agent Info Settings")]
    [SerializeField] private float agentHealth;
    [SerializeField] private TMP_Text agentNameText, agentHealthText;

    [Header("Operator Action Settings")]
    [SerializeField] private GameObject operatorInfo;
    [SerializeField] private GameObject operatorActions;
    [SerializeField] private TMP_Text infoBrief;
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        
        if (IsOwner) operatorUI.SetActive(true);
        else this.enabled = false; 
    }


    void OnEnable()
    {
        headerTitle.text = "Operation: " + LevelManager.Instance.levelInfo.levelName;
        infoBrief.text = LevelManager.Instance.levelInfo.levelObjective;

    }

    void Update()
    {
        if (!IsOwner) return;
        
        time.text = LevelManager.Instance.DisplayTime(LevelManager.Instance.GetTimeLeft());
    }

    public void AgentUpdate(Agent agent)
    {
        agentNameText.text = agent.controllingPlayer.username;
        agentHealthText.text = agent.health.ToString();
    }
}
