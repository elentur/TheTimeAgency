﻿//-----------------------------------------------------------------------
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

/// <summary>
/// Tango floor finding user interface controller. 
/// 
/// Place a marker at the y position of the found floor and allow user to recalculate.
/// </summary>
public class CrimeScene : MonoBehaviour
{
    /// <summary>
    /// The marker for the found floor.
    /// </summary>
    public GameObject m_marker;

    /// <summary>
    /// The cube for the found floor.
    /// </summary>
    public GameObject m_cube;

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

    /// <summary>
    /// If <c>true</c>, floor finding is in progress.
    /// </summary>
    private bool m_findingFloor = false;

    [HideInInspector]
    public ICrimeSceneState currentState;
    [HideInInspector]
    public MarkCrimeSceneState markCrimeSceneState;
    [HideInInspector]
    public SpreadAdviceState spreadAdviceState;
    [HideInInspector]
    public PingState pingState;

    [HideInInspector]
    public List<GameObject> markerList = new List<GameObject>();

    [HideInInspector]
    public List<Triangle2D> triangleList = new List<Triangle2D>();

    [HideInInspector]
    public List<Vector3> m_pointList = new List<Vector3>();

    [HideInInspector]
    public List<GameObject> m_cubeList = new List<GameObject>();

    

    public int m_numberMarkers;
    public float m_distanceMarkers;

    private void Awake()
    {
        markCrimeSceneState = new MarkCrimeSceneState(this);
        spreadAdviceState = new SpreadAdviceState(this);
        pingState = new PingState(this);
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
        m_cube.SetActive(false);

        currentState = markCrimeSceneState;

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
}