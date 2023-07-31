using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Lockpick : Task
{
    public GameObject lockpick;
    public GameObject door;
    
    public Camera cam;
    public Transform innerLock;
    public Transform pickPosition;

    public float maxAngle = 90;
    public float lockSpeed = 10;

    [Range(1,25)] public float lockRange = 10;

    private float eulerAngle;
    private float unlockAngle;
    private Vector2 unlockRange;

    private float keyPressTime = 0;

    private bool movePick = true;
    
    public override void OnTaskCancelled()
    {
        base.OnTaskCancelled();
        
        gameObject.SetActive(false);
    }

    public override void OnTaskInitialize(Player currentPlayer)
    {
        base.OnTaskInitialize(currentPlayer);

        SetLockpickToPlayer();
        
        unlockAngle = Random.Range(-maxAngle + lockRange, maxAngle - lockRange);
        unlockRange = new Vector2(unlockAngle - lockRange, unlockAngle + lockRange);
    }

    public override void OnTaskComplete()
    {
        base.OnTaskComplete();

        door.GetComponentInParent<Door>().ServerUnlock();
        gameObject.SetActive(false);
    }

    private void Awake() 
    {
        cam = Camera.main;
        door = transform.parent.gameObject;
    }

    private void SetLockpickToPlayer()
    {
        transform.SetPositionAndRotation(cam.transform.position + (cam.transform.forward * 0.5f), cam.transform.rotation);
    }

    public override void Update() 
    {
        base.Update();
        
        lockpick.transform.position = pickPosition.position;

        if(movePick)
        {
            
            Vector3 mousePos = Mouse.current.position.ReadValue();
            //Debug.Log("mouse position:  x: " + mousePos.x + "    y: " + mousePos.y + "   z: " +mousePos.z);
            
            Vector3 dir = mousePos - Camera.main.WorldToScreenPoint(lockpick.transform.position);

            eulerAngle = Vector3.Angle(dir, Vector3.up);

            Vector3 cross = Vector3.Cross(Vector3.up, dir);
            if (cross.z < 0) { eulerAngle = -eulerAngle; }

            eulerAngle = Mathf.Clamp(eulerAngle, -maxAngle, maxAngle);

            Quaternion rotateTo = Quaternion.AngleAxis(eulerAngle, innerLock.transform.forward);
            lockpick.transform.rotation = rotateTo;
        }

        if (InputHandler.Instance.move.x != 0)
        {
            movePick = false;
            keyPressTime = 1;
        }

        if (InputHandler.Instance.move.x == 0)
        {
            movePick = true;
            keyPressTime = 0;
        }

        float percentage = Mathf.Round(100 - Mathf.Abs(((eulerAngle - unlockAngle) / 100) * 100));
        float lockRotation = ((percentage / 100) * maxAngle) * keyPressTime;
        float maxRotation = (percentage / 100) * maxAngle;

        float lockLerp = Mathf.LerpAngle(innerLock.eulerAngles.z, lockRotation, Time.deltaTime * lockSpeed);
        innerLock.eulerAngles = new Vector3(innerLock.eulerAngles.x, innerLock.eulerAngles.y, lockLerp);

        if(lockLerp >= maxRotation -1)
        {
            if (eulerAngle < unlockRange.y && eulerAngle > unlockRange.x)
            {
                Debug.Log("Unlocked!");
                OnTaskComplete();

                movePick = true;
                keyPressTime = 0;
            }
            else
            {
                float randomRotation = Random.insideUnitCircle.x;
                lockpick.transform.eulerAngles += new Vector3(0, Random.Range(-randomRotation, randomRotation), 0);
            }
        }
    }
}
