//-----------------------------------------------------------------------
// <copyright file="TangoFloorFindingUIController.cs" company="Google">
//
// Copyright 2016 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using Assets.TheTimeAgency.Scripts;
using Tango;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

/// <summary>
/// Tango floor finding user interface controller. 
/// 
/// Place a marker at the y position of the found floor and allow user to recalculate.
/// </summary>
public class CrimeScene : MonoBehaviour
{
    /// <summary>
    /// The scene's Tango application.
    /// </summary>
    [HideInInspector]
    public TangoApplication m_tangoApplication;

    /// <summary>
    /// Reference to the TangoPointCloud in the scene. 
    /// 
    /// FindFloor is called in TangoPointCloud, and the TangoPointCloudFloor automatically reflects 
    /// changes in the found floor.
    /// </summary>
    [HideInInspector]
    public TangoPointCloud m_pointCloud;

    /// <summary>
    /// Reference to the TangoPointCloudFloor in the scene.
    /// </summary>
    [HideInInspector]
    public TangoPointCloudFloor m_pointCloudFloor;


    /**
     * States  Declaration
     */
    [HideInInspector]
    public ICrimeSceneState currentState;
    [HideInInspector]
    public FindFloorState findFloorState;
    [HideInInspector]
    public MarkCrimeSceneState markCrimeSceneState;
    [HideInInspector]
    public PingState pingState;
    [HideInInspector]
    public DefaultState defaultState;


    /**
     * Untiy Values
     */
    public GameObject m_markerCanvas;

    public GameObject m_marker;
    public int m_numberMarkers = 4;
    public float m_distanceMarkers = 1.0f;
    public GameObject m_AdvicePlaceHolder;
    public GameObject m_advice;
    public float m_distanceAdvices = 0.1f;

    /**
     * Global Variables
     */ 
    [HideInInspector]
    public List<Triangle2D> triangleList = new List<Triangle2D>();

    [HideInInspector]
    public Vector3 m_floorPoint;

    private void Awake()
    {
        findFloorState = new FindFloorState(this);
        markCrimeSceneState = new MarkCrimeSceneState(this);
        pingState = new PingState(this);
        defaultState = new DefaultState(this);
    }

    /// <summary>
    /// Use this for initialization.
    /// </summary>
    public void Start()
    {

        m_pointCloud = FindObjectOfType<TangoPointCloud>();
        m_pointCloudFloor = FindObjectOfType<TangoPointCloudFloor>();
        m_tangoApplication = FindObjectOfType<TangoApplication>();

        m_marker.SetActive(false);
        m_AdvicePlaceHolder.SetActive(false);
        m_advice.SetActive(false);

        currentState = findFloorState;
        currentState.StartState();
    }

    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    public void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }

        currentState.UpdateState();
    }

    /// <summary>
    /// OnGUI is called for rendering and handling GUI events.
    /// </summary>
    public void OnGUI()
    {
        currentState.OnGUIState();
    }

    /// <summary>
    /// Application onPause / onResume callback.
    /// </summary>
    /// <param name="pauseStatus"><c>true</c> if the application about to pause, otherwise <c>false</c>.</param>
    public void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            // When application is backgrounded, we reload the level because the Tango Service is disconected. All
            // learned area and placed marker should be discarded as they are not saved.
#pragma warning disable 618
            Application.LoadLevel(Application.loadedLevel);
#pragma warning restore 618
        }
    }

    public void Button_SetMarker()
    {
        Debug.Log("Button_SetMarker");
        markCrimeSceneState.m_setMarker = true;
    }

    public void Button_DefaultMarker()
    {
        Debug.Log("Button_DefaultMarker");
        markCrimeSceneState.m_defaultMarkers = true;
    }

    public void Button_ResetMarker()
    {
        Debug.Log("Button_ResetMarker");
        markCrimeSceneState.m_resetMarkers = true;
    }

    public void Button_PingAdvices()
    {
        Debug.Log("Button_PingAdvices");
        pingState.Ping = true;
    }

   
}