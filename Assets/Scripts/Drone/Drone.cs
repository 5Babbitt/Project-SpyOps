using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class Drone : NetworkBehaviour, IDamageable
{
    [SyncVar] public Operator controllingOperator;
    [SyncVar] public float health = 5;

    [Header("Drone Componenets")]
    [SerializeField] private DroneMovement droneMovement;
    [SerializeField] private GameObject _dronePointer;
    
    public Transform droneCamTransform;

    public void TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0)
        {
            controllingOperator.controlledDrone = null;
            Despawn();
        }
    }

    private void Awake()
    {
        droneMovement = GetComponent<DroneMovement>();
    }

    private void Start() 
    {
        LevelManager.Instance.SetDroneParent(droneCamTransform);
    }

    void OnDestroy()
    {
        LevelManager.Instance.SetDroneCamOffline();
        controllingOperator.DespawnDrone();
    }
}
