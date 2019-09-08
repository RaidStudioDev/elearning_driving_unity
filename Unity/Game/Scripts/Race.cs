using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityStandardAssets.Cameras;

public class Race : MonoBehaviour 
{
    public GameScreen GameScreen { get; private set; }

    public Avatar dontCodeStrip1 = null;
    public AnimationClip dontCodeStrip2 = null;
    public Track Track;
	public Vehicle Vehicle;

    public float Time { get; set; }
    public bool Started { get; private set; }

    private RaceTimeEventManager raceTimeEventManager;
    private GameObject trackGameObject;
    private GameObject vehicleGameObject;

    private int laps;
    private bool ready = false;
	private int readySteps = 0;
    private bool started = false;
    private string weather = "";
    private float targetFogLevel = FOG_LEVEL_NONE;
    
    public const string WEATHER_DEFAULT = "Default";
    public const string WEATHER_NONE = "None";
    public const string WEATHER_HAIL = "Hail";
    public const string WEATHER_SNOW = "Snow";
    public const string WEATHER_RAIN = "Rain";

    private const float FOG_LEVEL_NONE = .009f;
    private const float FOG_LEVEL_HAIL = .015f;
    private const float FOG_LEVEL_SNOW = .015f;
    private const float FOG_LEVEL_RAIN = .015f;

    // warning CS0414: The private field `Race.CarnivalLoop' is assigned but its value is never used
#pragma warning disable 414 
    private GameObject CarnivalLoop;
#pragma warning restore 414 

    private GameObject CarnivalLoopObject;
    private LTBezierPath CarnivalLoopLeanTweenBezierPath;
    private float CarnivalLoopLeanTweenIterator;

    public delegate void CompletedEventHandler(float time);
    public event CompletedEventHandler OnCompleted;

    private long time = DateTime.UtcNow.ToFileTimeUtc();
    private readonly WaitForSeconds waitSecAssetLoad = new WaitForSeconds(0.01f);
    private float downloadProgress = 0.0f;
    private bool completed;

    #if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern bool IsSafari();
    #else
        private static bool IsSafari() { return false; }
    #endif

    private void Awake()
	{
        if (GameManager.Instance == null) //so we don't have to test directly out of "Main" every time
        {
            PersistentModel.GameSceneOverride = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene("Main", LoadSceneMode.Single);
            return;
        }

        #if UNITY_WEBGL
            UnityEngine.PostProcessing.PostProcessingBehaviour postProcessingBehavior = Camera.main.GetComponent<UnityEngine.PostProcessing.PostProcessingBehaviour>();
            if (Application.platform == RuntimePlatform.WebGLPlayer && IsSafari()) 
            {
                var profile = postProcessingBehavior.profile;
                profile.colorGrading.enabled = false;
            }
        #endif

        Time = PersistentModel.Instance.ChallengeTime;

        SceneManager.sceneLoaded += OnSceneLoaded;

        laps = PersistentModel.Instance.GameLaps;

        if (!PersistentModel.Instance.DEBUG) GameObject.Find("Debug").SetActive(false);
        
        SetWeather(WEATHER_DEFAULT);

        GameScreen = UIManager.Instance.GetComponentInChildren<GameScreen>();

        //////////////////////////////////////////////////////////////////////////////////////////
        // init race time event manager
        //////////////////////////////////////////////////////////////////////////////////////////
        raceTimeEventManager = new RaceTimeEventManager();
        raceTimeEventManager.Initialize(this);

        // its null if user has not selected a tire option
        if (PersistentModel.Instance.TireOptionSelectedData != null)
        {
            if (PersistentModel.Instance.TireOptionSelectedData.correct)
            {
                PersistentModel.Instance.TireOptionSelectedData = null;
            }
            else
            {
                // stop race and show alert
                raceTimeEventManager.AddTimeEvent(RaceTimeEventManager.Event.FORCE_STOP, 1f);
            }
        }
        //////////////////////////////////////////////////////////////////////////////////////////
    }

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
        SceneManager.sceneLoaded -= OnSceneLoaded;

        UIManager.Instance.Overlay.OnGameCountComplete += OnGameCountCompleted;

        if (PersistentModel.Instance.GameSkybox != "")
        {
            if (true) //Application.platform != RuntimePlatform.WebGLPlayer)
            {
                RenderSettings.skybox = Instantiate(Resources.Load(PersistentModel.Instance.GameSkybox)) as Material;

                // if night time adjust environmental effects
                if (PersistentModel.Instance.GameNight)
                {
                    RenderSettings.ambientSkyColor = new Color(0.2924528f, 0.1613454f, 0.133811f, 1);
                    RenderSettings.fogColor = new Color(0.4056604f, 0.225291f, 0.132031f, 1);
                }
                
                DynamicGI.UpdateEnvironment();
            }
            // else StartCoroutine(LoadSkybox());
        }

        var gameScene = PersistentModel.Instance.GameScene;
        if (PersistentModel.GameSceneOverride != null)
        {
            if (gameScene != "") gameScene = PersistentModel.GameSceneOverride;
            PersistentModel.GameSceneOverride = null;
        }
        
        trackGameObject = GameObject.Find(gameScene);
		if (trackGameObject == null) 
		{       
            if (Application.platform != RuntimePlatform.WebGLPlayer) 
			{
                DebugLog.Trace("ChallengeIndex: " + PersistentModel.Instance.ChallengeIndex);
                DebugLog.Trace("GameTrack: " + PersistentModel.Instance.GameTrack);
                DebugLog.Trace("GameVehicle: " + PersistentModel.Instance.GameVehicle);

				trackGameObject = Instantiate(Resources.Load(PersistentModel.Instance.GameTrack)) as GameObject;
                vehicleGameObject = Instantiate(Resources.Load(PersistentModel.Instance.GameVehicle)) as GameObject;
			}
			else
                StartCoroutine(LoadTrackAndVehicle());
		} 
		else
		{
            if (Application.platform != RuntimePlatform.WebGLPlayer)
                vehicleGameObject = Instantiate(Resources.Load(PersistentModel.Instance.GameVehicle)) as GameObject;
			else
                StartCoroutine(LoadVehicle());
		}

		OnAllLoaded();    
    }

	IEnumerator LoadSkybox()
	{
        // materials cannot be loaded directly via asset bundle, 
        // you would need to put the material into a game object, 
        // instantiate it, and then get the material from it.
        // for now, let's just use resources

		readySteps++;

        WWW www = WWW.LoadFromCacheOrDownload(PersistentModel.Instance.AssetBundlesURL + PersistentModel.Instance.GameSkybox.ToLower() + "?t=" + time, 1);
		yield return www;

		AssetBundle bundle = www.assetBundle;
		AssetBundleRequest request = bundle.LoadAssetAsync(PersistentModel.Instance.GameSkybox);

		yield return request;

		var material = Instantiate<Material>(request.asset as Material);
        bundle.Unload(false);
		RenderSettings.skybox = material;
		DynamicGI.UpdateEnvironment();

		readySteps--;
		OnAllLoaded();
	}

    IEnumerator LoadTrackAndVehicle()
    {
        readySteps++;

        // load track
        string url = PersistentModel.Instance.AssetBundlesURL + PersistentModel.Instance.GameTrack.ToLower() + "?t=" + time;

        UnityWebRequest request = new UnityWebRequest(url)
        {
            downloadHandler = new DownloadHandlerAssetBundle(url, 1, 0),
            certificateHandler = new SSLAuth()
        };
        request.SendWebRequest();

        while (!request.isDone)
        {
            downloadProgress = request.downloadProgress * 50;

            UIManager.Instance.ProgressLoadingPanel.GetComponent<ProgressLoadingPanel>().LoadingPercent.text = Mathf.CeilToInt(downloadProgress).ToString() + "%";

            yield return waitSecAssetLoad;
        }

        AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);
        AssetBundleRequest bundleRequest = bundle.LoadAssetAsync(PersistentModel.Instance.GameTrack);

        yield return bundleRequest;

        GameObject track = bundleRequest.asset as GameObject;
        trackGameObject = Instantiate<GameObject>(track);
        bundle.Unload(false);

        StartCoroutine(LoadVehicle());

        readySteps--;
        OnAllLoaded();
    }

	IEnumerator LoadVehicle()
	{
		readySteps++;

        string url = PersistentModel.Instance.AssetBundlesURL + PersistentModel.Instance.GameVehicle.ToLower() + "?t=" + time;

        UnityWebRequest request = new UnityWebRequest(url)
        {
            downloadHandler = new DownloadHandlerAssetBundle(url, 1, 0),
            certificateHandler = new SSLAuth()
        };
        request.SendWebRequest();

        string downloadLabel = "";
        while (!request.isDone)
        {
            downloadLabel = Mathf.CeilToInt(downloadProgress + (request.downloadProgress * 50)).ToString() + "%";
            UIManager.Instance.ProgressLoadingPanel.GetComponent<ProgressLoadingPanel>().LoadingPercent.text = downloadLabel;

            yield return waitSecAssetLoad;
        }

        UIManager.Instance.ProgressLoadingPanel.GetComponent<ProgressLoadingPanel>().LoadingPercent.text = "100%";

        AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);
        AssetBundleRequest bundleRequest = bundle.LoadAssetAsync(PersistentModel.Instance.GameVehicle);

        yield return bundleRequest;

        GameObject vehicle = bundleRequest.asset as GameObject;
        vehicleGameObject = Instantiate<GameObject>(vehicle);
        bundle.Unload(false);

        readySteps--;
        OnAllLoaded();
    }

    private void OnAllLoaded()
    {
		if (readySteps > 0) return;

        Track = trackGameObject.AddComponent<Track>();
        Track.OnLapCompleted += OnLapCompleted;

		Vehicle = vehicleGameObject.AddComponent<Vehicle>();

        ready = true;

        GameManager.Instance.GameReady();
        
        if (started) DoStart();
    }
    
    private void OnLapCompleted()
    {
        laps--;

        if (laps != 0) return;

        DebugLog.Trace("Race.OnLapCompleted");

        PersistentModel.Instance.ClockIsStopped = true;

        GameScreen.transform.GetComponent<GameScreen>().ShowComplete();

        completed = true;

        Vehicle.Stop();

        UIManager.Instance.soundManager.mPlayer.PlayTrack(2, false);

        GameObject.Find("Cameras").GetComponent<AutoCam>().ReduceCamSpeed(0.09f, 0.01f);

        PersistentModel.Instance.ChallengeTime = Time;

        Invoke("DoComplete", 3);
     }

    private void DoComplete()
    {
        if (OnCompleted != null) OnCompleted(PersistentModel.Instance.ChallengeTime);    
    }

    private void OnGameCountCompleted()
    {
        UIManager.Instance.Overlay.OnGameCountComplete -= OnGameCountCompleted;

        Started = true;

		Vehicle.CanMove = true;
        
        raceTimeEventManager.Start();
    }

	void Start() 
	{
        started = true;
        
        if (ready) DoStart();
    }
    
    private void DoStart()
    {
        int[] trackIndexes = new int[] { 1, 5 };
        trackIndexes.ShuffleCrypto();

        UIManager.Instance.soundManager.mPlayer.PlayTrack(trackIndexes[0]);

        Track.PositionVehicleAtStartPosition(Vehicle);

        if (PersistentModel.Instance.GameCarnivalLoopObject.Length > 0)
        {
            CarnivalLoop = GameObject.Find("CarnivalLoop");
            CarnivalLoopObject = Instantiate(Resources.Load(PersistentModel.Instance.GameCarnivalLoopObject)) as GameObject;
            //LTBezierPath path = new LTBezierPath(CarnivalLoop.GetComponent<LeanTweenPath>().path);
            LTBezierPath path = new LTBezierPath(
new Vector3[] { new Vector3(134.65f, 16.7f, -27.5f), new Vector3(133.8322f, 18.95677f, -27.5f), new Vector3(134.6568f, 18.13219f, -27.5f), new Vector3(132.4f, 18.95f, -27.5f), new Vector3(132.4f, 18.95f, -27.5f), new Vector3(130.1432f, 18.13219f, -27.5f), new Vector3(130.9678f, 18.95677f, -27.5f), new Vector3(130.15f, 16.7f, -27.5f), new Vector3(130.15f, 16.7f, -27.5f), new Vector3(130.9678f, 14.44323f, -27.5f), new Vector3(130.1432f, 15.26781f, -27.5f), new Vector3(132.4f, 14.45f, -27.5f), new Vector3(132.4f, 14.45f, -27.5f), new Vector3(134.6568f, 15.26781f, -27.5f), new Vector3(133.8322f, 14.44323f, -27.5f), new Vector3(134.65f, 16.7f, -27.5f) });
            CarnivalLoopLeanTweenBezierPath = path;
            CarnivalLoopObject.transform.position = CarnivalLoopLeanTweenBezierPath.point(CarnivalLoopLeanTweenIterator);
        }
    }

    public void SetWeather(string name)
    {
        // Debug.Log("Set Weather: " + name);
        if (name == WEATHER_DEFAULT) name = PersistentModel.Instance.GameWeather;

        if (name == weather) return;

        targetFogLevel = FOG_LEVEL_NONE;

        //disable
        switch (weather)
        {
            case WEATHER_NONE:
                break;
            case WEATHER_HAIL:
                Camera.main.transform.Find("Hail").GetComponent<ParticleSystem>().Stop();
                break;
            case WEATHER_SNOW:
                Camera.main.transform.Find("Snow").GetComponent<ParticleSystem>().Stop();
			    break;
            case WEATHER_RAIN:
                Camera.main.transform.Find("Rain").GetComponent<ParticleSystem>().Stop();
			    break;
        }

        //enable
        switch (name)
        {
            case WEATHER_NONE:
                break;
            case WEATHER_HAIL:
                targetFogLevel = FOG_LEVEL_HAIL;
                Camera.main.transform.Find("Hail").GetComponent<ParticleSystem>().Play();
                break;
            case WEATHER_SNOW:
                targetFogLevel = FOG_LEVEL_SNOW;
                Camera.main.transform.Find("Snow").GetComponent<ParticleSystem>().Play();
		        break;
            case WEATHER_RAIN:
                targetFogLevel = FOG_LEVEL_RAIN;

                AddCameraDropEffects();
                Camera.main.transform.Find("Rain").GetComponent<ParticleSystem>().Play();
		        break;
        }

        if (!Started) RenderSettings.fogDensity = targetFogLevel;
        
        weather = name;
    }

    void Update () 
	{
        // time
        if (!PersistentModel.Instance.ClockIsStopped)
        {
            Time += UnityEngine.Time.deltaTime;
            GameScreen.SetTime(Time);
        }

        if (completed || !ready || !started) return;

        if (CarnivalLoopObject)
        {
            CarnivalLoopObject.transform.position = CarnivalLoopLeanTweenBezierPath.point(CarnivalLoopLeanTweenIterator);
            CarnivalLoopLeanTweenIterator += UnityEngine.Time.deltaTime * .01f;
            if (CarnivalLoopLeanTweenIterator > 1.0f) CarnivalLoopLeanTweenIterator = 0.0f;
        }

        if (PersistentModel.Instance.DEBUG)
        {
            GameObject speed = GameObject.Find("Speed");
            Text text = speed.GetComponent<Text>();
            text.text = Vehicle.Speed + " MPH";
        }

        // fog
        float current = RenderSettings.fogDensity;
        float target = targetFogLevel;
        float diff = current - target;
        if (diff > 0) current -= .0001f;
        else if (diff < 0) current += .0001f;

        diff = current > target ? current - target : target - current;

        if (diff != 0 && diff < .00001f) current = target;
        if (diff != 0) RenderSettings.fogDensity = current;
        if (!Started || completed) return;

        // time
		if (!PersistentModel.Instance.ClockIsStopped)
		{
            // Race Events
            raceTimeEventManager.Update(UnityEngine.Time.deltaTime);
        }
	}

    public void ForceCompleted()
    {
        completed = true;
    }

    public void StopVehicle()
    {
        Vehicle.Stop();
    }

    private void AddCameraDropEffects()
    {
        bool enableEffect = true;

        if (enableEffect)
        {
            Camera.main.gameObject.AddComponent<CameraDrops>();
        }
    }
}
