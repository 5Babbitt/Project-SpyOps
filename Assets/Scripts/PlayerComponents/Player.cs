using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public class Player : NetworkBehaviour
{
    public static Player Instance {get; private set;}
    
	[Header("Player Settings")]
    [SyncVar] public string username;
    [SyncVar] public PlayerType playerType = PlayerType.None;
    [SyncVar] public bool isReady;
	[SyncVar] public int team = 0;
    [SyncVar] public Player teammate;

	[Header("Agent Settings")]
	public GameObject _agentPrefab;
	[SyncVar] public Agent controlledAgent;

	private float timeToRespawn;
	private float nextTimeToRespawn;
	private float respawnTime;
	
	[Header("Operator Settings")]
	public GameObject _operatorPrefab;
	[SyncVar] public Operator controlledOperator;

	void Awake()
	{
		
	}

	#region Network Methods
    public override void OnStartServer()
	{
		base.OnStartServer();

		GameManager.Instance.players.Add(this);

		if (playerType == PlayerType.Agent)	{	GameManager.Instance.agents.Add(this);	}
		else if(playerType == PlayerType.Operator)	{	GameManager.Instance.operators.Add(this);	}
	}

	public override void OnStopServer()
	{
		base.OnStopServer();

		if (playerType == PlayerType.Agent)	{	GameManager.Instance.agents.Remove(this);	}
		else if(playerType == PlayerType.Operator)	{	GameManager.Instance.operators.Remove(this);	}

		GameManager.Instance.players.Remove(this);
	}

    public override void OnStartClient()
	{
		base.OnStartClient();

		SetUsername();

		if (!IsOwner) return;

		Instance = this;
	}
	#endregion

	#region Player Methods
	private void SetUsername()
	{
		if (!IsOwner) return;
		
		string playerName = "Player" + Random.Range(0, 9999).ToString();

		ServerSetUsername(playerName);
		Lobby.Instance.SetPlayerUsernameText(playerName);
	}

	[ServerRpc(RequireOwnership = false)]
	public void ServerSetUsername(string name)
	{
		username = name;
	}

	public void StartGame()
	{
		UIManager.Instance.CloseLobbyScreen();
		GameManager.Instance.SetGameStarted();

		GetTeammate();
		
		if (playerType == PlayerType.Agent)
		{
			SpawnAgent();
			//controlledAgent.SetTeammate(teammate);
		} 
		else if (playerType == PlayerType.Operator)
		{
			SpawnOperator();
		}
	}

	public void StopGame()
	{
		if (playerType == PlayerType.Agent)
		{
			DespawnAgent();
		} 
		else if (playerType == PlayerType.Operator)
		{
			DespawnOperator();
		}
	}

    [ServerRpc(RequireOwnership = false)]
	public void ServerSetIsReady(bool value)
	{
		Debug.Log($"Set player is ready: {value}");
		isReady = value;
	}

    [ServerRpc(RequireOwnership = false)]
	public void ServerSetPlayerType(PlayerType value)
    {
		playerType = value;
    }

	public void OnReady()
	{
		Debug.Log("Player is Ready");
		
		if (playerType == PlayerType.Agent || playerType == PlayerType.Operator)
			ServerSetIsReady(!isReady);

		if (playerType == PlayerType.Agent) InputHandler.Instance.SetAgentControlScheme();

		if (playerType == PlayerType.Operator) InputHandler.Instance.SetOperatorControlScheme();
	}

	public void SetTeam(int value)
	{
		if (!IsOwner) return;
		
		ServerSetTeam(value);
	}

	[ServerRpc(RequireOwnership = false)]
	public void ServerSetTeam(int value)
	{
		Debug.Log($"Player joined team {value}");
		team = value;
	}

	private void GetTeammate()
	{
		if (teammate != null) return;
		
		SyncList<Player> playerList = GameManager.Instance.players;

		foreach (Player player in playerList)
		{
			if (player.team == team && player != this) teammate = player;
		}
	}
	#endregion

	#region Agent Methods
	
	[ContextMenu("Spawn Agent")]
	public void SpawnAgent()
	{
		GameObject agentInstance = Instantiate(_agentPrefab, LevelManager.Instance.agentSpawnpoints[team].position, Quaternion.identity);

        Spawn(agentInstance, Owner);

        controlledAgent = agentInstance.GetComponent<Agent>();
        controlledAgent.controllingPlayer = this;
		//controlledAgent.SetTeammate(teammate);
	}

	[ContextMenu("Despawn Agent")]
	public void DespawnAgent()
	{
		if (controlledAgent != null && controlledAgent.IsSpawned) controlledAgent.Despawn();
	}

	[TargetRpc]
	public void TargetAgentKilled(NetworkConnection connection)
	{
		StartCoroutine(RespawnCountdown());
		SpawnAgent();
	}

	private IEnumerator RespawnCountdown()
	{
		yield return new WaitForSeconds(timeToRespawn);
	}
	#endregion

	#region Operator Methods
	[ContextMenu("Spawn Operator")]
	public void SpawnOperator()
	{
		GameObject operatorInstance = Instantiate(_operatorPrefab);

        Spawn(operatorInstance, Owner);

        controlledOperator = operatorInstance.GetComponent<Operator>();
        controlledOperator.controllingPlayer = this;
		//controlledOperator.SetTeammate(teammate);
	}

	[ContextMenu("Despawn Operator")]
	public void DespawnOperator()
	{
		if (controlledOperator != null && controlledOperator.IsSpawned) controlledOperator.Despawn();
	}
	#endregion

	#region Spectator Methods
	public void SpawnSpectator()
	{

	}

	public void DespawnSpectator()
	{
		
	}
	#endregion
}

public enum PlayerType
{
    None,
	Agent,
    Operator,
	Spectator
}