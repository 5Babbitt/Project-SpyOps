using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;

public class AgentAction : NetworkBehaviour
{
    [SerializeField] private Transform cam;
    [SerializeField] private float maxUseDistance = 5f;
    [SerializeField] private LayerMask interactionLayers;
    [SerializeField] private bool canInteract = true;

    private Interactable currentInteractable;
    
    private InputHandler _input;

    private void Awake() 
    {
        _input = InputHandler.Instance;
        cam = Camera.main.transform;
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (canInteract)
        {
            HandleInteractionInput();
            HandleInteractionCheck();
        }
    }

    private void HandleInteractionCheck()
    {
        if (Physics.Raycast(cam.position, cam.forward, out RaycastHit hit, maxUseDistance))
        {
            if (hit.collider.gameObject.layer == 6 && (currentInteractable == null || hit.collider.gameObject.GetInstanceID() != currentInteractable.GetInstanceID()))
            {
                hit.collider.TryGetComponent(out currentInteractable);

                if (currentInteractable) currentInteractable.OnFocus();
            }
        }
        else if (currentInteractable)
        {
            currentInteractable.OnLoseFocus();
            currentInteractable = null;
        }
    }

    private void HandleInteractionInput()
    {
        if (_input.interact && currentInteractable != null && Physics.Raycast(cam.position, cam.forward, out RaycastHit hit, maxUseDistance, interactionLayers))
        {
            currentInteractable.OnInteract(GetComponent<Agent>().controllingPlayer);
            _input.interact = false;
        }
        else if(_input.interact && currentInteractable == null)
        {
            _input.interact = false;
        }
    }

    public void SetCanInteract(bool value)
    {
        canInteract = value;
    }
}
