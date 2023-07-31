using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputHandler : MonoBehaviour
{
    public static InputHandler Instance { get; private set; }
    
    //public static PlayerInputControls inputActions = new PlayerInputControls();
    //public static event Action<InputActionMap> actionMapChanged; 

    [SerializeField] private PlayerInput playerInput;
    
    [Header("Input Values")]
    //Player Inputs
	public Vector2 move;
	public Vector2 look;
    public bool escape;

    //Agent Inputs
    public bool run;
    public bool crouch;
    public bool interact;
    public bool aim;
    public bool shoot;

    //Operator Inputs
    public float elevate;
    public float rotate;

    //Task Inputs


    [Header("Mouse Cursor Settings")]
    public bool cursorLocked = true;

    private void Awake() 
    {
        Instance = this;
    }

    /*public static void ToggleActionMap(InputActionMap actionMap)
    {
        if (actionMap.enabled) return;

        inputActions.Disable();
        actionMapChanged?.Invoke(actionMap);
        actionMap.Enable();
    }*/

    #region General Input
    public void MoveInput(InputAction.CallbackContext value)
    {
        move = value.ReadValue<Vector2>();
    }

    public void LookInput(InputAction.CallbackContext value)
    {
        look = value.ReadValue<Vector2>();
    }

    public void EscapeInput(InputAction.CallbackContext value)
    {
        if (!value.started)
        {
            return;
        }

        escape = true;
    }
    #endregion

    #region Agent Input
    public void RunInput(InputAction.CallbackContext value)
    {
        bool newRunState = value.performed;
        run = newRunState;
    }

    public void CrouchInput(InputAction.CallbackContext value)
    {
        if (value.started)
        {
            crouch = !crouch;
        }
    }

    public void InteractInput(InputAction.CallbackContext value)
    {
        if (!value.started)
        {
            return;
        }
        
        interact = true;
    }

    public void AimInput(InputAction.CallbackContext value)
    {
        if (value.started)
        {
            aim = !aim;
        }
    }

    public void ShootInput(InputAction.CallbackContext value)
    {
        if (!value.started)
        {
            return;
        }

        shoot = true;
    }
    #endregion

    #region Operator Input
    public void ElevationInput(InputAction.CallbackContext value)
    {
        elevate = value.ReadValue<float>();
    }

    public void Rotate(InputAction.CallbackContext value)
    {
        rotate = value.ReadValue<float>();
    }
    #endregion

    private void OnApplicationFocus(bool hasFocus)
	{
		SetCursorState(cursorLocked);
	}

	private void SetCursorState(bool newState)
	{
		Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.Confined;
	}

    [ContextMenu("Switch To Agent Controls")]
    public void SetAgentControlScheme()
    {
        playerInput.SwitchCurrentActionMap("Agent");
        Debug.Log(playerInput.currentActionMap.ToString());
    }

    [ContextMenu("Switch To Operator Controls")]
    public void SetOperatorControlScheme()
    {
        playerInput.SwitchCurrentActionMap("Operator"); 
        Debug.Log(playerInput.currentActionMap.ToString());
    }
}
