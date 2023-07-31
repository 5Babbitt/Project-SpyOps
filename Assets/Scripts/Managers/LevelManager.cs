using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    public LevelInfo levelInfo;
    public UIManager LevelUI;

    public Camera mapCamera;
    public GameObject[] mapCamTransforms;
    public GameObject[] securityCameras;
    public GameObject droneCamera;
    public Transform offlineCameraTransform;

    public Transform[] agentSpawnpoints;
    public Transform droneSpawn;

    public float timeLeft {get; private set;}

    private float missionTime;



    void Awake()
    {
        if (LevelManager.Instance != null)  Destroy(this);
        else    Instance = this;

        missionTime = levelInfo.missionTime;

        mapCamera = GameObject.FindGameObjectWithTag("MapCamera").GetComponent<Camera>();
        mapCamTransforms = GameObject.FindGameObjectsWithTag("MapCameraTransform");
        securityCameras = GameObject.FindGameObjectsWithTag("SecurityCamera");
    }

    private void Start() 
    {
        timeLeft = missionTime;
    }

    void Update()
    {
        if (GameManager.Instance.gameStarted)
            LevelTimer();
    }

    private void LevelTimer()
    {
        timeLeft -= Time.deltaTime;

        if (timeLeft <= 0) GameManager.Instance.GameOver();
    }

    public void SetMapCameraPosition(int transformIndex)
    {
        mapCamera.transform.position = mapCamTransforms[transformIndex].transform.position;

        if (transformIndex == 0) mapCamera.farClipPlane = 50f;
        else mapCamera.farClipPlane = 4.9f; 
    }

    //Converts time float to minutes and seconds
    public string DisplayTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60F);
        int seconds = Mathf.FloorToInt(time % 60F);
        
        string levelTime = minutes.ToString("00") + ":" + seconds.ToString("00");

        return levelTime;
    }

    public float GetTimeLeft()
    {
        return timeLeft;
    }

    public void SetDroneCamOffline()
    {
        droneCamera.transform.parent = offlineCameraTransform;
        droneCamera.transform.position = offlineCameraTransform.position;
        droneCamera.transform.rotation = offlineCameraTransform.rotation;
    }

    public void SetDroneParent(Transform newTransform)
    {
        droneCamera.transform.parent = newTransform;
        droneCamera.transform.position = newTransform.position;
        droneCamera.transform.rotation = newTransform.rotation;
    }
}