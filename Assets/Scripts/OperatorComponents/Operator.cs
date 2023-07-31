using System;
using UnityEngine;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public class Operator : NetworkBehaviour
{
    [SyncVar] public Player controllingPlayer;
    [SyncVar] public Drone controlledDrone;

    [SerializeField] private int droneSpawns = 3;
    [SerializeField] private GameObject _dronePrefab;

 
    public OperatorUI _operatorUI {get; private set;}
    [SerializeField] private GameObject operatorUI;
    [SerializeField] private Color teamColour;
    [SerializeField] private Color enemyColour;

    private int teamLayer = 10;
    private int enemyLayer = 13;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!IsOwner) 
        {
            operatorUI.SetActive(false);
        }

        UIManager.Instance.CloseLobbyScreen();
        InputHandler.Instance.SetOperatorControlScheme();
    }
    
    void Awake()
    {   
        _operatorUI = GetComponent<OperatorUI>();
    }

    void Start()
    {
        InputHandler.Instance.SetOperatorControlScheme();
        //InputHandler.ToggleActionMap(InputHandler.inputActions.Operator);
    }

    void Update()
    {
        if (controllingPlayer.teammate.controlledAgent != null) _operatorUI.AgentUpdate(controllingPlayer.teammate.controlledAgent);
    }

    [ContextMenu("Spawn Drone")]
    public void SpawnDrone()
    {
        if (droneSpawns <= 0) return;
        
        droneSpawns--;
        
        GameObject droneInstance = Instantiate(_dronePrefab, LevelManager.Instance.droneSpawn.position, Quaternion.identity);

        Spawn(droneInstance, Owner);

        controlledDrone = droneInstance.GetComponent<Drone>();
        controlledDrone.controllingOperator = this;
        
        InputHandler.Instance.SetOperatorControlScheme();
    }

    public void DespawnDrone()
    {
        if (controlledDrone == null && controlledDrone.IsSpawned) controlledDrone.Despawn();
    }
}
