using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;

public abstract class Interactable : NetworkBehaviour
{
    [SyncVar] public Player lastInteractedPlayer;
    public TMP_Text useText;

    public virtual void OnInteract(Player interactingPlayer)
    {
        Debug.Log(interactingPlayer + " interacted with" + this.name);
        lastInteractedPlayer = interactingPlayer;
    }
    
    public abstract void OnFocus();
    public abstract void OnLoseFocus();

    public virtual void Start()
    {
        gameObject.layer = 6;
        
        useText = UIManager.Instance.GetUseText();
    }
}
