using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour 
{
	public static GameManager Instance { get; private set; }

    public delegate void LoadedEventHandler();
    public event LoadedEventHandler OnLoaded;
    public delegate void CompletedEventHandler(float time);
    public event CompletedEventHandler OnCompleted;

    private bool loading = false;
    private Race race;
    
    private void Awake()
	{
        Application.runInBackground = true;
        // Caching.compressionEnabled = false;
		// Application.targetFrameRate = 60;

		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else Destroy(gameObject);
	}

	void Start () 
	{
	}

	public void Load()
	{
		if (loading) 
		{
			Debug.Log ("Error: Loaded or already loading");
			return;
		}
		loading = true;

        SceneManager.sceneLoaded += OnSceneLoaded;

        if (PersistentModel.Instance.GameScene != "") SceneManager.LoadScene(PersistentModel.Instance.GameScene, LoadSceneMode.Single);
        else SceneManager.LoadScene("Generic", LoadSceneMode.Single);
	}

    // triggered when URLScheme is launched and Race is running
    // if the user is different than the current user, we will force Race.complete
    // basically stopping the Update loop, which was causing problems
    // JOHN: Maybe a better way to do this? please check
    public void ForceCompleted()
    {
        if (race != null) race.ForceCompleted();
    }

    public void Unload()
	{
        if (loading) 
		{
			Debug.Log ("Error: Unloaded or already loading");
			return;
		}
		loading = true;

        race.OnCompleted -= OnRaceCompleted;
        race = null;

        SceneManager.sceneLoaded += OnSceneUnloaded;
        SceneManager.LoadScene("Main", LoadSceneMode.Single);
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    public void GameReady()
    {
        loading = false;
    
        race = GameObject.Find("Race").GetComponent<Race>();
        race.OnCompleted += OnRaceCompleted;

        OnLoaded();

        Application.runInBackground = false;
	}

    private void OnRaceCompleted(float time)
    {
        race.OnCompleted -= OnRaceCompleted;

        Application.runInBackground = true;

        if (OnCompleted != null) OnCompleted(time);
    }

	private void OnSceneUnloaded(Scene scene, LoadSceneMode mode)
	{
        SceneManager.sceneLoaded -= OnSceneUnloaded;

        loading = false;
    }

    void Update () 
	{		
	}
}
