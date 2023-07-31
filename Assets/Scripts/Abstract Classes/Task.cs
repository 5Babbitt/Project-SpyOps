using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public abstract class Task : NetworkBehaviour
{
    [SyncVar] public bool isComplete;
    [SyncVar] public Player lastInteractedPlayer;
    
    public virtual void OnTaskInitialize(Player currentPlayer)
    {
        lastInteractedPlayer = currentPlayer;

        if (lastInteractedPlayer != null) Debug.Log("true");

        if (lastInteractedPlayer.playerType == PlayerType.Agent)
        {
            lastInteractedPlayer.controlledAgent.SetAll(false);
        }
    }

    public virtual void OnTaskCancelled()
    {
        if (lastInteractedPlayer == null) return;
        
        if (lastInteractedPlayer.playerType == PlayerType.Agent)
        {
            lastInteractedPlayer.controlledAgent.SetAll(true);
        }
    }

    public virtual void OnTaskComplete()
    {
        isComplete = true;

        if (lastInteractedPlayer.playerType == PlayerType.Agent)
        {
            lastInteractedPlayer.controlledAgent.SetAll(true);
        }
    }

    public virtual void Update()
    {
        if (InputHandler.Instance.escape)
        {
            InputHandler.Instance.escape = false;
            OnTaskCancelled();
        }
    }
}
