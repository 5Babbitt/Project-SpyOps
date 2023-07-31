using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;

[RequireComponent(typeof(CharacterController))]
public class AgentMovement : NetworkBehaviour
{
    [Header("Player Move Settings")]
    [SerializeField] private float moveSpeed = 4.0f;
    [SerializeField] private float crouchSpeed = 2.0f;
    [SerializeField] private float runSpeed = 7.0f;
    [SerializeField] private float characterHeight = 1.8f;
    [SerializeField] private float crouchHeight = 1.25f;

    [Tooltip("How fast the character turns to face movement direction")]
    [Range(0f, 1f)]
    [SerializeField] private float turnSpeed;

    [Tooltip("How fast the character turns to face movement direction")]
    [Range(0.0f, 0.3f)] 
    public float RotationSmoothTime = 0.12f;

    [Tooltip("Acceleration and deceleration")]
    public float SpeedChangeRate = 35.0f;

    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    public float gravity = -15.0f;
    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    public float FallTimeout = 0.15f;

    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool Grounded = true;
    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.14f;
    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.28f;
    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;

    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject CinemachineCameraTarget;
    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 70.0f;
    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -30.0f;
    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    public float CameraAngleOverride = 0.0f;
    [Range(1.0f, 100.0f)] 
    public float cameraSensitivity = 40f;
    [SerializeField] private GameObject playerCam;

    // cinemachine
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;
    private bool _rotateOnMove = true;

    [Header("Debug Values")]
    [SerializeField] private float _speed;
    [SerializeField] private float _animationBlend;
    [SerializeField] private float _targetRotation;
    [SerializeField] private float _verticalVelocity;
    [SerializeField] private float _rotationVelocity;

    private float _fallTimeoutDelta;
    private float _terminalVelocity = 53.0f;

    // animation IDs
    private int _animSpeed;
    private int _animCrouch;
    private int _animDance;

    private Animator _animator;
    private InputHandler _input;
    private CharacterController _controller;
    private GameObject _mainCamera;

    [SerializeField] private bool canMove = true;
    [SerializeField] private bool canLook = true;

    private const float _threshold = 0.01f;

    #region Network Methods
    public override void OnStartClient()
    {
        base.OnStartClient();
        if(!base.IsOwner)
        {
            playerCam.SetActive(false);
        }
    }
    #endregion

    #region Unity Methods
    private void Awake() 
    {
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
        
        _animator = GetComponent<Animator>();
        _input = InputHandler.Instance;
        _controller = GetComponent<CharacterController>();
        
        AssignAnimationIDs();
    }
    
    private void Start() 
    {
        _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
        _controller.center = new Vector3(0, _controller.height/2, 0);
        
        _fallTimeoutDelta = FallTimeout;
    }

    private void Update() 
    {
        if (!IsOwner) return;
        
        GroundedCheck();
        Gravity();
        if (canMove) Move();
    }

    private void LateUpdate() 
    {
        if (!IsOwner) return;

        if (canLook) CameraRotation();
    }
    #endregion

    #region Movement Methods
    private void Move()
    {
        //Set Target speed based on whether crouching or not
        float targetSpeed = _input.crouch ? crouchSpeed : moveSpeed;

        //Set height of character controller based on whether player is crouching
        Crouch();
        
        if (!_input.crouch && !_input.aim)
        {
            targetSpeed = _input.run ? runSpeed : moveSpeed;
        }

        if (_input.move == Vector2.zero) targetSpeed = 0f;

        // a reference to the players current horizontal velocity
        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

        float speedOffset = 0.05f;
        float inputMagnitude = 1f;

        // accelerate or decelerate to target speed
        if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            // creates curved result rather than a linear one giving a more organic speed change
            // note T in Lerp is clamped, so we don't need to clamp our speed
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

            // round speed to 3 decimal places
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else 
        {
            _speed = targetSpeed;
        }

        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
        if (_animationBlend < 0.01f) _animationBlend = 0f;

        // normalise input direction
        Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

        // if there is a move input rotate player when the player is moving
        if (_input.move != Vector2.zero)
        {
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);

            // rotate to face input direction relative to camera position
            
            if (_rotateOnMove)  {   transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);    }
        }

        Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

        // move the player
        _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

        _animator.SetFloat("Speed", _animationBlend);
        _animator.SetBool(_animCrouch, _input.crouch);
        //_animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
    }

    private void Crouch()
    {
        float height = _input.crouch ? crouchHeight : characterHeight;
        ServerChangeHeight(height);
    }

    [ServerRpc(RunLocally = true)]
    private void ServerChangeHeight(float newHeight)
    {
        SetCharacterHeight(newHeight);
        ObserverChangeHeight(newHeight);
    }

    [ObserversRpc(BufferLast = true, IncludeOwner = false)]
    private void ObserverChangeHeight(float newHeight)
    {
        SetCharacterHeight(newHeight);
    }

    private void SetCharacterHeight(float value)
    {
        _controller.height = value;
        _controller.center = new Vector3(0, _controller.height/2, 0);
    }

    private void CameraRotation()
    {
        if (_input.look.sqrMagnitude >= _threshold)
        {
            _cinemachineTargetYaw += _input.look.x * cameraSensitivity/100f;
            _cinemachineTargetPitch += _input.look.y * cameraSensitivity/100f;
        }

        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0.0f);
    }
    #endregion

    #region Gravity Methods
    private void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
    }

    private void Gravity()
    {
        if (Grounded)
        {
            // reset the fall timeout timer
            _fallTimeoutDelta = FallTimeout;
            
            // stop our velocity dropping infinitely when grounded
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }
        }
        else 
        {
            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
        }

        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += gravity * Time.deltaTime;
        }
    }
    #endregion

    #region Misc Methods
    private void AssignAnimationIDs()
    {
        _animSpeed = Animator.StringToHash("Speed");
        _animCrouch = Animator.StringToHash("Crouch");
    }
    
    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    public void SetSensitivity(float sensitivity)
    {
        cameraSensitivity = sensitivity;
    }

    public void SetRotationOnMove(bool newRotateOnMove)
    {
        _rotateOnMove = newRotateOnMove;
    }

    public void SetCanMove(bool value)
    {
        canMove = value;
    }

    public void SetCanLook(bool value)
    {
        canLook = value;
    }
    #endregion
}
