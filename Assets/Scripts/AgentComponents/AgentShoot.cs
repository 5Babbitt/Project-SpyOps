using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using FishNet.Connection;
using FishNet.Object;

public class AgentShoot : NetworkBehaviour
{
    [SerializeField] private CinemachineVirtualCamera aimVirtualCamera;
    [SerializeField] private GameObject crosshair;
    [SerializeField] private GameObject pistol;

    [Range(1.0f, 100.0f)]
    [SerializeField] private float normalSensitivity = 40;
    [Range(1.0f, 100.0f)]
    [SerializeField] private float aimSensitivity = 25;

	public int damage;
    public float timeBetweenFire;

    private float fireTimer;

    [SerializeField] private LayerMask aimColliderMask;
    [SerializeField] private Transform bulletSpawnTransform;
    [SerializeField] private Animator _animator;
    
    private InputHandler _input;
    private AgentMovement _playerMovement;
    private Vector3 mouseWorldPosition = Vector3.zero;

    [SerializeField] private bool canShoot = true;

    public override void OnStartClient()
    {
        base.OnStartClient();
    }
    
    void Awake()
    {
        _input = InputHandler.Instance;
        _playerMovement = GetComponent<AgentMovement>();
        _animator = GetComponent<Animator>();

        pistol.SetActive(false);
    }

    void Update()
    {
        if (!IsOwner) return;
        
        if (canShoot)
        {
            Aiming();
            ShootInput();
        }
    }

    void Aiming()
    {
        Vector2 screenCentre = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(screenCentre);

        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderMask))
        {
            mouseWorldPosition = raycastHit.point;
        }
        else 
        {
            mouseWorldPosition = Camera.main.transform.position + Camera.main.transform.forward * 50f;
        }

        if (_input.aim && !_input.crouch) 
        {   
            AimState(true);
            pistol.SetActive(true);

            Vector3 worldAimTarget = mouseWorldPosition;
            worldAimTarget.y = transform.position.y;
            Vector3 aimDirection = (worldAimTarget - transform.position).normalized;

            transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);
        }
        else 
        {  
            AimState(false);
            pistol.SetActive(false);
        }
    }

    void ShootInput()
    {
        if (_input.shoot)
        {
            if (fireTimer <= 0)
            {
                if (CanShoot()) ShootServer(1, bulletSpawnTransform.position, AimDirection());
                _input.shoot = false;
                fireTimer = timeBetweenFire;
            }            
        }

        if (fireTimer > 0) fireTimer -= Time.deltaTime;
    }

    [ServerRpc(RequireOwnership = false)]
    void ShootServer(int damage, Vector3 position, Vector3 direction)
    {
        Shoot(damage, position, direction);
    }

    void Shoot(int damage, Vector3 position, Vector3 direction)
    {
        if(Physics.Raycast(position, direction, out RaycastHit hit) && hit.transform.TryGetComponent<Agent>(out Agent enemyAgent))
        {
            Debug.DrawRay(position, direction * Vector3.Distance(position, hit.transform.position), Color.green, 5);
            enemyAgent.health -= damage;
        }

        if(Physics.Raycast(position, direction, out RaycastHit hitDrone) && hit.transform.TryGetComponent<Drone>(out Drone enemyDrone))
        {
            Debug.DrawRay(position, direction * Vector3.Distance(position, hitDrone.transform.position), Color.green, 5);
            enemyDrone.TakeDamage(damage);
        }
    }

    bool CanShoot()
    {
        if (!_input.aim || _input.crouch)
        {
            _input.shoot = false;
            return false;
        }
        
        return true;
    }

    void AimState(bool aiming)
    {
        float sensitivity = _playerMovement.cameraSensitivity;
        float targetLayerWeight;
        
        aimVirtualCamera.gameObject.SetActive(aiming);
        crosshair.SetActive(aiming);
        _playerMovement.SetRotationOnMove(!aiming);

        if(aiming == true)
        {
            sensitivity = aimSensitivity;
            targetLayerWeight = 1f;
        }
        else
        {
            sensitivity = normalSensitivity;
            targetLayerWeight = 0f;
        }

        _playerMovement.SetSensitivity(sensitivity);
        _animator.SetLayerWeight(1, Mathf.Lerp(_animator.GetLayerWeight(1), targetLayerWeight, Time.deltaTime * 10f));
    }

    Vector3 AimDirection()
    {
        Vector3 aimDirection = (mouseWorldPosition - bulletSpawnTransform.position).normalized;

        return aimDirection;
    }

    public void SetCanShoot(bool value)
    {
        canShoot = value;
    }
}
