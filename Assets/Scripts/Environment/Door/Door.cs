using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class Door : Interactable
{
    [SyncVar] public bool isOpen = false;
    [SyncVar] public bool isLocked = true;

    public GameObject lockpickMinigame;

    [SerializeField] private bool isRotatingDoor = true;
    [SerializeField] private float speed = 5f;

    [Header("Rotation Configs")]
    [SerializeField] private float rotationAmount = 90f;
    [SerializeField] private float forwardDirection = 0;

    [Header("Sliding Configs")]
    [SerializeField] private Vector3 slideDirection = Vector3.back;
    [SerializeField] private float slideAmount = 1.9f;

    private Vector3 _startRotation;
    private Vector3 _startPosition;
    private Vector3 Forward;

    private Coroutine AnimationCoroutine;

    public override void Start() 
    {
        base.Start();
        
        _startRotation = transform.rotation.eulerAngles;
        Forward = transform.forward;
        _startPosition = transform.position;

        lockpickMinigame = GetComponentInChildren<Lockpick>(true).gameObject;
    }
    
    public override void OnFocus()
    {
        Debug.Log("Focused");
        
        if (!isLocked)
        {
            if (isOpen)
            {
                base.useText.SetText("Close \"E\"");
            }
            else
            {
                base.useText.SetText("Open \"E\"");
            }
        }
        else
        {
            base.useText.SetText("Unlock \"E\"");
        }
    }

    public override void OnInteract(Player interactingPlayer)
    {
        lastInteractedPlayer = interactingPlayer;
        
        Vector3 playerPosition = Vector3.zero;
        
        if (interactingPlayer.playerType == PlayerType.Agent)
        {
            playerPosition = interactingPlayer.controlledAgent.transform.position;
        }
        else
        {
            playerPosition = interactingPlayer.controlledOperator.transform.position;
        }
        
        if (!isLocked)
        {
            if (isOpen)
            {
                Close();
            }
            else
            {
                Open(playerPosition);
            }
        }
        else
        {
            Unlock();
        }
    }

    public override void OnLoseFocus()
    {
        useText.SetText("");
    }

    public void Unlock()
    {
        lockpickMinigame.SetActive(true);
        Debug.Log("Lockpick activated");
        lockpickMinigame.GetComponentInChildren<Lockpick>(true).OnTaskInitialize(lastInteractedPlayer);
    }

    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void ServerUnlock()
    {
        UnlockDoor();
    }

    private void UnlockDoor()
    {
        isLocked = false;
    }

    public void Open(Vector3 UserPosition)
    {
        ServerOpen(UserPosition);
    }

    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    public void ServerOpen(Vector3 UserPosition)
    {
        OpenDoor(UserPosition);
        ObserverOpen(UserPosition);
    }

    [ObserversRpc(BufferLast = true, IncludeOwner = false)]
    private void ObserverOpen(Vector3 UserPosition)
    {
        OpenDoor(UserPosition);
    }

    private void OpenDoor(Vector3 UserPosition)
    {
        if (!isOpen)
        {
            if (AnimationCoroutine != null)
            {
                StopCoroutine(AnimationCoroutine);
            }

            if (isRotatingDoor)
            {
                float dot = Vector3.Dot(Forward, (UserPosition - transform.position).normalized);
                Debug.Log($"Dot: {dot.ToString("N3")}");
                AnimationCoroutine = StartCoroutine(DoRotationOpen(dot));
            }
            else
            {
                AnimationCoroutine = StartCoroutine(DoSlidingOpen()); 
            }
        }
    }

    private IEnumerator DoRotationOpen(float ForwardAmount)
    {
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation;

        if (ForwardAmount >= forwardDirection)
        {
            endRotation = Quaternion.Euler(new Vector3(0, _startRotation.y - rotationAmount, 0));
        }
        else
        {
            endRotation = Quaternion.Euler(new Vector3(0, _startRotation.y + rotationAmount, 0));
        }

        isOpen = true;

        float time = 0;
        while (time < 1)
        {
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, time);
            yield return null;
            time += Time.deltaTime * speed;
        }
    }

    private IEnumerator DoSlidingOpen()
    {
        Vector3 endPosition = _startPosition + slideAmount * slideDirection;
        Vector3 startPosition = transform.position;

        float time = 0;
        isOpen = true;
        while (time < 1)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, time);
            yield return null;
            time += Time.deltaTime * speed;
        }
    }

    public void Close()
    {
        ServerClose();
    }

    [ServerRpc(RequireOwnership = false, RunLocally = true)]
    private void ServerClose()
    {
        CloseDoor();
        ObserverClose();
    }

    [ObserversRpc(BufferLast = true, IncludeOwner = false)]
    private void ObserverClose()
    {
        CloseDoor();
    }

    private void CloseDoor()
    {
        if (isOpen)
        {
            if (AnimationCoroutine != null)
            {
                StopCoroutine(AnimationCoroutine);
            }

            if (isRotatingDoor)
            {
                AnimationCoroutine = StartCoroutine(DoRotationClose());
            }
            else
            {
                AnimationCoroutine = StartCoroutine(DoSlidingClose());
            }
        }
    }

    private IEnumerator DoRotationClose()
    {
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.Euler(_startRotation);

        isOpen = false;

        float time = 0;
        while (time < 1)
        {
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, time);
            yield return null;
            time += Time.deltaTime * speed;
        }
    }

    private IEnumerator DoSlidingClose()
    {
        Vector3 endPosition = _startPosition;
        Vector3 startPosition = transform.position;
        float time = 0;

        isOpen = false;

        while (time < 1)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, time);
            yield return null;
            time += Time.deltaTime * speed;
        }
    }
}
