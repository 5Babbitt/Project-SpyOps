using UnityEngine;
using FishNet.Object;

public class DroneMovement : NetworkBehaviour
{
    private Drone _drone;
    private InputHandler _input;

    [SerializeField] private float horizontalSpeed;
    [SerializeField] private float verticalSpeed;
    [SerializeField] private float rotationSpeed;

    [SerializeField] Vector3 input3D;
    [SerializeField] float inputRotate;

    public bool canMove = true;

    private Rigidbody rb;

    void Awake()
    {
        _drone = GetComponent<Drone>();
        _input = InputHandler.Instance;
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (!IsOwner) return;
        
        if(canMove) Rotate();
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;
        
        if(canMove) Move();
    }

    private void Move()
    {
        Vector3 verticalDirection = transform.up * _input.elevate;
        Vector3 horizontalDriection = transform.right * _input.move.x + transform.forward * _input.move.y;

        Vector3 verticalMovement = verticalDirection * verticalSpeed;
        Vector3 horizontalMovment = horizontalDriection * horizontalSpeed;

        rb.AddForce(horizontalMovment + verticalMovement, ForceMode.Force);
    }

    private void Rotate()
    {
        transform.Rotate(new Vector3(0, _input.rotate * rotationSpeed * Time.deltaTime, 0), Space.Self);
    }

    public void SetCanMove(bool value)
    {
        canMove = value;
    }
}
