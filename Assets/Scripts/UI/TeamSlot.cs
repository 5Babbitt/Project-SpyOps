using UnityEngine;
using UnityEngine.UI;
using FishNet.Object;
using FishNet.Connection;
using FishNet.Object.Synchronizing;
using TMPro;

public class TeamSlot : NetworkBehaviour
{   
    [Header("Player References")]
    [SyncVar] public string operatorName;
    [SyncVar] public string agentName;
    [SyncObject] public readonly SyncList<Player> teamPlayers = new SyncList<Player>();
    [SyncVar] public Player _operator;
    [SyncVar] public Player _agent;

    [Header("Team References")]
    [SerializeField] private int _teamNumber;
    [SerializeField] private int playersOnTeam;

    [Header("Object References")]
    [SerializeField] private TMP_Text title;
    [SerializeField] private GameObject agentButton, operatorButton;


    void Awake()
    {
        playersOnTeam = teamPlayers.Count;
        title.text = "Team " + _teamNumber;
    }

    private void SetTeam(Player player)
    {
        teamPlayers.Add(player);
        playersOnTeam = teamPlayers.Count;        
        player.SetTeam(_teamNumber);
    }

    private Player GetPlayer()
    {
        Player joinedPlayer = null;
        
        foreach (Player player in GameManager.Instance.players)
        {
            if(Player.Instance == player) joinedPlayer = player;
        }

        return joinedPlayer;
    }

    public void JoinAsOperator()
    {
        if (_operator != null || Player.Instance.playerType != PlayerType.None) return;

        ServerSetOperator(GetPlayer());
    }

    public void JoinAsAgent()
    {
        if (_agent != null || Player.Instance.playerType != PlayerType.None) return;

        ServerSetAgent(GetPlayer());
    }

    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    private void ServerSetAgent(Player player)
    {
        SetAgent(player);
        SetTeam(player);
        ObserverSetAgent(player);
    }

    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    private void ServerSetOperator(Player player)
    {
        SetOperator(player);
        SetTeam(player);
        ObeserverSetOperator(player);
    }

    [ObserversRpc(BufferLast = true)]
    private void ObeserverSetOperator(Player player)
    {
        SetOperator(player);
    }

    [ObserversRpc(BufferLast = true)]
    private void ObserverSetAgent(Player player)
    {
        SetAgent(player);
    }

    private void SetOperator(Player player)
    {
        _operator = player;
        
        _operator.ServerSetPlayerType(PlayerType.Operator);
        InputHandler.Instance.SetOperatorControlScheme();
        operatorName = _operator.username;

        GameManager.Instance.operators.Add(player);

        operatorButton.GetComponent<RoleButton>().SetActiveButton(operatorName);
    }

    private void SetAgent(Player player)
    {
        _agent = player;
        
        _agent.ServerSetPlayerType(PlayerType.Agent);
        InputHandler.Instance.SetAgentControlScheme();
        agentName = _agent.username;

        GameManager.Instance.agents.Add(player);

        agentButton.GetComponent<RoleButton>().SetActiveButton(agentName);
    }
}
