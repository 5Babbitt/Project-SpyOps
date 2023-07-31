using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;

public class Agent : NetworkBehaviour
{
    [SyncVar] public Player controllingPlayer;

    [SyncVar] public float health = 10;
    
    [SerializeField] private TMP_Text healthText;

    [Header("Agent Components")]
    [SerializeField] private AgentMovement agentMovement;
    [SerializeField] private AgentAction agentAction;
    [SerializeField] private AgentShoot agentShoot;
    [SerializeField] private GameObject _mapPointer;
    [SerializeField] private GameObject _agentUI;

    private void Awake() 
    {
        agentAction = GetComponent<AgentAction>();
        agentMovement = GetComponent<AgentMovement>();
        agentShoot = GetComponent<AgentShoot>();
    }
 
    private void Start() 
    {
        InputHandler.Instance.SetAgentControlScheme();
        
        //InputHandler.ToggleActionMap(InputHandler.inputActions.Agent);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!IsOwner) _agentUI.SetActive(false);
    }

    private void Update() 
    {
        if (!IsOwner) return;

        healthText.text = health.ToString();
    }

    public void TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0)
        {
            controllingPlayer.TargetAgentKilled(Owner);
            Despawn();
        }
    }

    public void SetAll(bool value)
    {
        SetCanMove(value);
        SetCanLook(value);
        SetCanShoot(value);
        SetCanInteract(value);
    }

    public void SetCanMove(bool value)
    {
        agentMovement.SetCanMove(value);
    }

    public void SetCanLook(bool value)
    {
        agentMovement.SetCanLook(value);
    }

    public void SetCanShoot(bool value)
    {
        agentShoot.SetCanShoot(value);
    }

    public void SetCanInteract(bool value)
    {
        agentAction.SetCanInteract(value);
    }

    public void SetMapIdentity(Color newColour, int layerID)
    {
        _mapPointer.layer = layerID;
        _mapPointer.GetComponent<MeshRenderer>().material.color = newColour;
    }
}
