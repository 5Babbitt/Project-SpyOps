using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FishNet.Object;


public class OperatorMap : NetworkBehaviour
{
    private Operator _operator;
    
    [SerializeField] private bool _canSeeMap = true;
    [SerializeField] private bool _canSeeIcons, _canSeeAI, _canSeeAgents, _canSeeAll;

    private int mapLayer = 9;
    private int iconLayer = 10;
    private int AILayer = 11;
    private int enemyLayer = 12;

    [SerializeField] private LayerMask allMapLayers;
    [SerializeField] private LayerMask currentLayers;

    private Camera mapCam;
    private int currentFloor;
    private int floorCount;

    [SerializeField] private TMP_Text floorText;
    [SerializeField] private float timeToReveal = 15f;

    private float nextTimeToReveal;
    private float revealTime;

    void Awake()
    {
        _operator = GetComponent<Operator>();
        
        mapCam = GameObject.FindGameObjectWithTag("MapCamera").GetComponent<Camera>();

        floorCount = LevelManager.Instance.mapCamTransforms.Length - 1;
        currentFloor = 0;

        SetCamPosition(currentFloor);

        nextTimeToReveal = timeToReveal;
    }

    void Update()
    {
        if (!IsOwner) return;
        
        revealTime += Time.deltaTime;

        if (revealTime >= nextTimeToReveal)
        {
            nextTimeToReveal = revealTime + timeToReveal;
        }
    }

    [ContextMenu("Upper Floor")]
    public void UpperFloor()
    {
        if (currentFloor == 0)  return;
        else currentFloor--;

        SetCamPosition(currentFloor);
    }

    [ContextMenu("Lower Floor")]
    public void LowerFloor()
    {
        if (currentFloor == floorCount) return;
        else currentFloor++;

        SetCamPosition(currentFloor);
    }

    private void SetCamPosition(int value)
    {
        LevelManager.Instance.SetMapCameraPosition(value);
        floorText.text = LevelManager.Instance.mapCamTransforms[value].name;
    }

    public void SetCanSeeMap(bool value)
    {
        _canSeeMap = value;

        EditCurrentLayers(_canSeeMap, mapLayer);
    }

    public void SetCanSeeIcons(bool value)
    {
        _canSeeIcons = value;

        EditCurrentLayers(_canSeeIcons, iconLayer);
    }

    public void SetCanSeeAI(bool value)
    {
        _canSeeAI = value;

        EditCurrentLayers(_canSeeAI, AILayer);
    }

    public void SetCanSeeEnemies(bool value)
    {
        _canSeeAgents = value;

        EditCurrentLayers(_canSeeAgents, enemyLayer);
    }

    [ContextMenu("Reveal Enemies")]
    private void RevealEnemies()
    {
        Debug.Log("reveal Enemies");
        SetCanSeeEnemies(true);
        StartCoroutine("RevealEnemyAgents");
    }

    private void EditCurrentLayers(bool value, int layer)
    {
        if (value) AddLayer(layer);
        else if (!value) RemoveLayer(layer);

        SetCameraLayers();
    }

    private void AddLayer(int layer)
    {
        currentLayers |= (1 << layer);
    }

    private void RemoveLayer(int layer)
    {
        currentLayers &= ~(1 << layer);
    }

    private void SetCameraLayers()
    {
        LevelManager.Instance.mapCamera.cullingMask = currentLayers;
    }

    private IEnumerator RevealEnemyAgents()
    {
        Debug.Log("Stop Enemy Reveal");
        
        yield return new WaitForSeconds(1.5f);
        SetCanSeeEnemies(false);
    }
}
