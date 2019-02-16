﻿using System;
using System.Collections.Generic;

using UnityEngine;

// Class for managing the Game functions
public class GameManager : MonoBehaviour
{
    // GameManager variables/objects
    public bool IsDebug = false;
    private bool isPlayerLoaded = false;
    public GameObject StartCamera;
    public GameObject UITEXT;
    public GameObject Player;
    public Vector3 PlayerPos { get; private set; }
    public static GameManager Instance;
    private MainLoopable main;
    private readonly List<Delegate> delegates = new List<Delegate>();
    public static string WorldName = "DevWorld";
    public static int WorldSeed = 0;
    public static string time;
    // Noise Variables
    public static float YMultiplier = 0.04f;
    public static float Land2NDLayerCutoff = 0.6f;
    public static float LandTopLayerCutoff = 0.9f;
    public static float AirAndLandIntersectionCutoff = 1.4f;
    public static float PerlinFrequency = 0.015f;
    public static float PerlinLacunarity = 2f;
    public static int PerlinOctaveCount = 4;
    public static float PerlinPersistence = 0.25f;
    public static int PerlinSeed = WorldSeed;
    // Cave Noise Variables
    public static float CaveYMultiplier = 0.3f;
    public static float CaveCutoff = 0.6f;
    public static float RidgedFrequency = 0.03f;
    public static float RidgedLacunarity = 2f;
    public static int RidgedOctaveCount = 4;
    public static int RidgedSeed = WorldSeed;

    // Start is called before the first frame update
    // GameManager Start: Register Files, Create Texture Atlas
    private void Start()
    {
        this.GetTime();
        Instance = this;
        FileManager.RegisterFiles();
        TextureAtlas.Instance.CreateAtlas();
        MainLoopable.Instantiate();
        this.main = MainLoopable.Instance;
        this.main.Start();
    }

    // Update is called once per frame
    // GameManager Update: Set Player position if Player exists, Update MainLoopable Instance, Invoke Delegates and Remove them
    private void Update()
    {
        this.GetTime();
        if(this.Player.activeSelf)
        {
            this.PlayerPos = this.Player.transform.position;
            Instance.isPlayerLoaded = true;
        }
        try
        {
            this.main.Update();
        }
        catch(System.Exception e)
        {
            Debug.Log($@"{time}: Can't update MainLoopable due to Exception: {e.ToString()}");
            Logger.Log($@"{time}: Can't update MainLoopable due to Exception: ");
            Logger.Log(e);
        }
        for(int i = 0; i < this.delegates.Count; i++)
        {
            try
            {
                this.delegates[i].DynamicInvoke();
                this.delegates.Remove(this.delegates[i]);
            }
            catch(Exception e)
            {
                Debug.Log($@"{time}: Can't Invoke Delegate due to Exception: {e.ToString()}");
                Logger.Log($@"{time}: Can't Invoke Delegate due to Exception: ");
                Logger.Log(e);
            }
        }
    }

    // Fixed Update called on timer, more than one per Update on slow FPS, less than one per Update on fast FPS
    private void FixedUpdate()
    {
        this.main.FixedUpdate();
    }

    // GameManager On Application Quit
    private void OnApplicationQuit()
    {
        this.main.OnApplicationQuit();
    }

    // Exit Game
    public static void ExitGame()
    {
        Instance.ExitGameInstance();
    }

    // Exit Game
    private void ExitGameInstance()
    {
        this.OnApplicationQuit();
    }

    // Register Delegates
    public void RegisterDelegate(Delegate d)
    {
        this.delegates.Add(d);
    }

    // Check if Player is loaded into world
    public static bool PlayerLoaded()
    {
        return Instance.isPlayerLoaded;
    }

    // Create player in world and destroy starting UI
    public void StartPlayer(Vector3 Pos, GameObject go)
    {
        Instance.RegisterDelegate(new Action(() =>
        {
            if(go != null && go.GetComponent<MeshCollider>() != null)
            {
                Debug.Log($@"{time}: Placing player in world...");
                Logger.Log($@"{time}: Placing player in world...");
                Destroy(this.StartCamera);
                Destroy(this.UITEXT);
                this.Player.transform.position = new Vector3(Pos.x, Pos.y, Pos.z);
                this.PlayerPos = this.Player.transform.position;
                this.Player.SetActive(true);
            }
            else
            {
                Debug.Log($@"{time}: Can't place player...");
                Logger.Log($@"{time}: Can't place player...");
            }
        }));
    }

    // Get time for logging
    private void GetTime()
    {
        int minutes = (int)(Time.time / 60);
        string Sminutes = minutes.ToString();
        if(minutes < 10)
        {
            Sminutes = $@"0{minutes}";
        }
        int seconds = (int)(Time.time % 60);
        string Sseconds = seconds.ToString();
        if(seconds < 10)
        {
            Sseconds = $@"0{seconds}";
        }
        int milliseconds = (int)(Time.time * 1000) % 1000;
        string Smilliseconds = milliseconds.ToString();
        if(milliseconds < 10)
        {
            Smilliseconds = $@"00{milliseconds}";
        }
        else if(milliseconds < 100)
        {
            Smilliseconds = $@"0{milliseconds}";
        }
        time = $@"{Sminutes}:{Sseconds}:{Smilliseconds}";
    }
}
