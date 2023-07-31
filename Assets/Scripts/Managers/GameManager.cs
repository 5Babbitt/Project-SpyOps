using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;


public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

	[SyncObject] public readonly SyncList<Player> players = new();
    [SyncObject] public readonly SyncList<Player> agents = new();
    [SyncObject] public readonly SyncList<Player> operators = new();

	[SyncVar] public bool canStart;
	[SyncVar] public bool gameStarted;

	private void Awake()
	{
		Instance = this;
	}

    private void Update()
	{
		if (!IsServer) return;

        if(agents.Count == operators.Count)
        {
		    canStart = players.All(player => player.isReady);
        }

	}

	public void SetGameStarted()
	{
		gameStarted = true;
	}

    [Server]
	public void StartGame()
	{
		if (!canStart) return;

		for (int i = 0; i < players.Count; i++)
		{
			players[i].StartGame();
		}
	}

	[Server]
	public void StopGame()
	{
		for (int i = 0; i < players.Count; i++)
		{
			players[i].StopGame();
		}

		gameStarted = false;
	}

	public void GameOver()
	{
		Debug.Log("TIMES UP!");
		StopGame();
	}
}
