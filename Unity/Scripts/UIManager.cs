using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public delegate void OnCacheCompleteEvent();

public class UIManager : MonoBehaviour
{
	public static UIManager Instance { get; private set; }

    public static string LOADING_SCREEN = "LoadingScreen";
    public static string START_PLAY_SCREEN = "StartPlayScreen";
    public static string WELCOME_BACK_SCREEN = "WelcomeBackScreen";
    public static string GAMEMODE_SELECTION_SCREEN = "GameModeSelectionScreen";
    public static string INSTUCTIONS_SCREEN = "InstructionsScreen";
    readonly public static string QUIZ_SCREEN = "SelectSequenceScreen";
    readonly public static string GAME_SCREEN = "GameScreen";
    readonly public static string CONGRATULATIONS_SCREEN = "CongratulationsScreen";
    readonly public static string CIRCUIT_COMPLETED_SCREEN = "CircuitCompletedScreen";
    readonly public static string CONGRATULATIONS_FINAL_SCREEN = "CongratulationsFinalScreen";
    readonly public static string LEADERBOARD_SCREEN = "LeaderboardScreen";

    public static AudioSource AudioSrc { get; private set; }

    public enum Screen
	{
		LOADING,
        START_PLAY,
        WELCOME_BACK,
        GAMEMODE_SELECTION,
        INSTRUCTIONS,
        QUIZ_SCREEN,
        GAME,
        CONGRATULATIONS,
        CIRCUIT_COMPLETED_SCREEN,
        CONGRATULATIONS_FINAL,
        LEADERBOARD, DEFAULT
	}

    readonly private string[] screens = new string[] 
	{
		LOADING_SCREEN, 
		START_PLAY_SCREEN, 
		WELCOME_BACK_SCREEN, 
		GAMEMODE_SELECTION_SCREEN, 
		INSTUCTIONS_SCREEN, 
        QUIZ_SCREEN, 
		GAME_SCREEN, 
		CONGRATULATIONS_SCREEN,
        CIRCUIT_COMPLETED_SCREEN, 
		CONGRATULATIONS_FINAL_SCREEN, 
		LEADERBOARD_SCREEN
	};

    public bool LOGGER = false;
    public bool PlayMusic = true;
    public bool EnableCursor = true;

    public static float ScaleFactor { get; private set; }
    public GameObject ProgressLoadingPanel { get; private set; }
    public GameObject SmallProgressLoader { get; private set; }

    public string CurrentScreenID { get; private set; }
    public string PreviousScreenID { get; private set; }
    private string transitioningScreenId;

    private BaseScreen currentScreen;
    private BaseScreen transitioningScreen;

    private URLSchemeHandler urlSchemeHandler;
    [HideInInspector] public bool AutomaticScreenAlpha = true;
    [HideInInspector] public SoundManager soundManager;
    public OverlayManager Overlay { get; private set; }

    private Texture2D cursorHand;
    private Vector2 cursorHotspot = new Vector2(36, 10);
    
    private void Awake()
	{
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);

        DebugLog.isEnabled = LOGGER;
        
        InitializeAppURLLauncher(false);

        InitializeOverlayManager();

        InitializeSoundManager();
    }
    
    // Use this for initialization
    private void Start ()
	{
        if (EnableCursor) InitializeMouseCursor();

        // preload ProgressLoadingPanel
        ProgressLoadingPanel = (GameObject)Instantiate(Resources.Load("ProgressLoadingPanel"));
        ProgressLoadingPanel.transform.SetAsLastSibling();

        // small loader
        SmallProgressLoader = (GameObject)Instantiate(Resources.Load("SmallProgressLoader"));
        SmallProgressLoader.transform.SetAsLastSibling();

        // get canvas scale 
        ScaleFactor = this.GetComponent<Canvas>().scaleFactor;

        // show startup loader
        StartCoroutine(ShowStartUpLoadingScreen());
	}

    private void InitializeMouseCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        cursorHand = (Texture2D)Resources.Load("cursor_hand");
        cursorHotspot = new Vector2(31, 2);
        UpdateCursor(false);
    }

    public void UpdateCursor(bool isEntering)
    {
        if (!EnableCursor) return;

        if (isEntering)
            Cursor.SetCursor(cursorHand, cursorHotspot, CursorMode.ForceSoftware);
        else
            Cursor.SetCursor(null, cursorHotspot, CursorMode.ForceSoftware);
    }

    private void InitializeOverlayManager()
    {
        Overlay = new OverlayManager();
        Overlay.Initialize();
    }

    private void InitializeSoundManager()
    {
        AudioSrc = transform.GetComponent<AudioSource>();
        
        soundManager = new SoundManager(AudioSrc, PlayMusic);
    }

    private IEnumerator ShowStartUpLoadingScreen()
	{
        CurrentScreenID = UIManager.LOADING_SCREEN;

        ResourceRequest resourceRequest = Resources.LoadAsync<BaseScreen>(CurrentScreenID);
        while (!resourceRequest.isDone)
        {
            yield return 0;
        }

        currentScreen = Instantiate<BaseScreen>(resourceRequest.asset as BaseScreen, transform);
        currentScreen.transform.SetAsFirstSibling();
        currentScreen.Initialize(CurrentScreenID);
    }

    public void StartupLoadComplete()
    {
        DebugLog.Trace("UIManager.StartupLoadComplete()");

        // if we have no email, we are not launching app via url scheme so show login screen.
        PersistentModel.Instance.Server.isShowLogin = (PersistentModel.Instance.Email.Length == 0);

        if (!PersistentModel.Instance.Server.isShowLogin)
        {
            // get user data
            PersistentModel.Instance.Server.OnGetUserDataAttemptForRequestComplete += OnGetUserDataAttemptForRequestComplete;
            StartCoroutine(PersistentModel.Instance.Server.GetUserDataAttemptForRequest());
        }
        else
        {
            // we have not user data available, show start screen and show login
            UIManager.Instance.CheckInitialScreen();
        }

    }

    // triggered when game has first launched
    // requires user email
    private void OnGetUserDataAttemptForRequestComplete(bool success, ServerData resultData)
    {
        DebugLog.Trace("UIManager.OnGetUserDataAttemptForRequestComplete()");

        PersistentModel.Instance.Server.OnGetUserDataAttemptForRequestComplete -= OnGetUserDataAttemptForRequestComplete;

        CheckInitialScreen();
    }

    public void CheckInitialScreen()
    {
        DebugLog.Trace("UIManager.CheckInitialScreen()");

        StartCoroutine(ShowInitialScreen());
    }

    private IEnumerator ShowInitialScreen()
    {
        DebugLog.Trace("UIManager.ShowInitialScreen.isShowLogin:" + PersistentModel.Instance.Server.isShowLogin);
        DebugLog.Trace("UIManager.GameModeID:" + PersistentModel.Instance.GameModeID);

        WaitForSeconds waitSecAssetLoad = new WaitForSeconds(0.5f);

        yield return waitSecAssetLoad;

        Screen initialScreen = PersistentModel.Instance.GameModeID == "" ? Screen.START_PLAY : Screen.WELCOME_BACK;

        if (PersistentModel.Instance.InitialScreen != Screen.LOADING)
        {
            DebugLog.Trace("UIManager.Manual Loading Enabled: " + PersistentModel.Instance.InitialScreen);
            ShowScreen(PersistentModel.Instance.InitialScreen);
        }
        else
        {
            DebugLog.Trace("UIManager.Standard Load: " + PersistentModel.Instance.InitialScreen);
            if (PersistentModel.Instance.Server.isShowLogin) UIManager.Instance.ShowScreen(UIManager.Screen.START_PLAY);
            else ShowScreen(initialScreen);
        }
    }

    public void RemoveCurrentScreen()
    {
        currentScreen.Remove();
        Destroy(currentScreen.gameObject);
    }

    public void ShowScreen(Screen screenId)
	{
        StartCoroutine(LoadScreen(screens[(int)screenId], screens[(int)screenId]));
	}

    public string GetScreenID(Screen screenId)
    {
        return screens[(int)screenId];
    }

    private WaitForSeconds waitSecAssetLoad = new WaitForSeconds(0.01f);
    private IEnumerator LoadScreen(string screenId, string screenPath)
    {
        GameObject newScreen;

        PreviousScreenID = currentScreen.screenId;

        // when running on webgl; only load game screen from resource
        if ((Application.platform != RuntimePlatform.WebGLPlayer && PersistentModel.Instance.RunLocation != PersistentModel.RUN_LOCATION.Client) || screenId == GAME_SCREEN)
        {
            ResourceRequest resourceRequest = Resources.LoadAsync<BaseScreen>(screenPath);
            while (!resourceRequest.isDone)
            {
                yield return null;
            }

            transitioningScreenId = screenId;
            transitioningScreen = Instantiate<BaseScreen>(resourceRequest.asset as BaseScreen, transform);
            transitioningScreen.transform.SetAsFirstSibling();
            transitioningScreen.Initialize(screenId);

            // loading is complete
            if (ProgressLoadingPanel) ProgressLoadingPanel.GetComponent<ProgressLoadingPanel>().LoadingPercent.text = "100%";
        }
        else
        {
            string url = PersistentModel.Instance.AssetBundlesURL + screenId.ToLower();
            UnityWebRequest request = new UnityWebRequest(url)
            {
                downloadHandler = new DownloadHandlerAssetBundle(url, 1, 0),
                certificateHandler = new SSLAuth()
            };
            request.SendWebRequest();

            while (!request.isDone)
            {
                ProgressLoadingPanel.GetComponent<ProgressLoadingPanel>().LoadingPercent.text = Mathf.CeilToInt(request.downloadProgress * 100).ToString() + "%";
                yield return waitSecAssetLoad;
            }

            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);
            AssetBundleRequest bundleRequest = bundle.LoadAssetAsync(screenId);

            yield return bundleRequest;

            newScreen = Instantiate<GameObject>(bundleRequest.asset as GameObject);
            bundle.Unload(false);

            ProgressLoadingPanel.GetComponent<ProgressLoadingPanel>().LoadingPercent.text = "100%";

            // Wait for the load
            yield return newScreen;

            // add Parent, and Add it in the 1st index of Parent
            // so that we can have the new screen behind the current screen
            newScreen.transform.SetParent(transform, false);
            newScreen.transform.SetAsFirstSibling();
            newScreen.GetComponent<BaseScreen>().Initialize(screenId);  

            // add to list
            transitioningScreenId = screenId;
            transitioningScreen = newScreen.GetComponent<BaseScreen>();
        }

        // isLoadingRequiredBeforeDraw is when loading the game screen and track
        if (!transitioningScreen.isLoadingRequiredBeforeDraw)
        {
            if (currentScreen != null)
            {
                Color activeColor = currentScreen.gameObject.GetComponent<Image>().color;
                activeColor.a = 1f;

                // fadeout background image color from screen
                LeanTween.value(1f, 0f, 0.75f)
                    .setDelay(0.25f)
                    .setEase(LeanTweenType.easeOutQuad)
                    .setOnUpdate((float val) =>
                    {
                        activeColor.a = val;
                        currentScreen.gameObject.GetComponent<Image>().color = activeColor;
                    })
                    .setOnComplete(() =>
                    {
                        // ignore closing sequence, start transition in right away after race.
                        if (screenId == CONGRATULATIONS_SCREEN)
                        {
                            ShowTransitionedScreen();
                        }
                        else
                        {
                            // scales and fades in side panels
                            // transitioningScreen.ScaleInPanels();

                            LeanTween.delayedCall(0.5f, transitioningScreen.ScaleInPanels);

                            // add event when loading close sequence is complete
                            currentScreen.OnCloseLoadingPanelComplete += CurrentScreen_OnCloseLoadingPanelComplete;

                            // start Close Panel sequence
                            currentScreen.CloseLoadingPanel();
                        }
                    });
            }
            else
            {
                Color activeColor = transitioningScreen.gameObject.GetComponent<Image>().color;
                activeColor.a = 1f;

                // fadeout background image color from screen
                LeanTween.value(1f, 0f, 0.15f)
                    .setDelay(0.0f)
                    .setEase(LeanTweenType.easeOutQuad)
                    .setOnUpdate((float val) =>
                    {
                        activeColor.a = val;
                        transitioningScreen.gameObject.GetComponent<Image>().color = activeColor;
                    })
                    .setOnComplete(() =>
                    {
                        // scales and fades in side panels
                        transitioningScreen.ScaleInPanels();

                        LeanTween.delayedCall(0.65f, ShowTransitionedScreen);
                    });

                // ShowTransitionedScreen();
                yield return 0;
            }
        }
        else
        {
            ShowTransitionedScreen();
        }
    }

    // this event function is called from the current screen
    // after LoadingPanel is closed, 
    // this function gets called to start transition animation for a new screen
    private void CurrentScreen_OnCloseLoadingPanelComplete()
    {
        currentScreen.OnCloseLoadingPanelComplete -= CurrentScreen_OnCloseLoadingPanelComplete;

        ShowTransitionedScreen();
    }

	private void ShowTransitionedScreen()
	{
        // do we need to load this screen before we call draw?
        if (transitioningScreen.isLoadingRequiredBeforeDraw)
		{
            // when loading in GameScreen, we show the loader on the currentScreen
            currentScreen.ShowProgressLoaderPanel(false);

            LeanTween.delayedCall(1.5f, () => {

                transitioningScreen.OnLoaded += OnScreenLoaded;
                transitioningScreen.Load();

            });
            
			return;
		}

		OnScreenLoaded(transitioningScreen);
	}

	private void OnScreenLoaded(BaseScreen baseScreen)
	{
		if (baseScreen.isLoadingRequiredBeforeDraw)
		{
			baseScreen.OnLoaded -= OnScreenLoaded;

			// when loaded, there we seem to lose frames
			// wait about 1 1/4 update cycle then prepare for draw
			LeanTween.delayedCall(1.25f, PrepareForGameScreenDraw);

			return;
		}

		DrawScreen();
	}

	private void PrepareForGameScreenDraw()
	{
        Color activeColor = currentScreen.gameObject.GetComponent<Image>().color;
		LeanTween.value(1f, 0f, 0.75f)
			.setDelay(0f)
			.setEase(LeanTweenType.easeOutQuad)
			.setOnUpdate((float val) =>
			{
				activeColor.a = val;
                currentScreen.gameObject.GetComponent<Image>().color = activeColor;
			})
			.setOnComplete(() => 
			{
                // Start Close Panel sequence
                currentScreen.OnCloseLoadingPanelComplete += CurrentScreen_OnCloseLoadingPanelComplete1;
                currentScreen.CloseLoadingPanel();
			});
	}

    private void CurrentScreen_OnCloseLoadingPanelComplete1()
    {
        currentScreen.OnCloseLoadingPanelComplete -= CurrentScreen_OnCloseLoadingPanelComplete1;

        DrawScreen();
    }

    private void DrawScreen()
	{
        // Start Intro Transition In for the New Screen
        transitioningScreen.OnTransitionInComplete += OnTransitionInComplete;
        transitioningScreen.Draw();
    }

    private void OnTransitionInComplete()
    {
        if (transitioningScreen == null) return;

        transitioningScreen.OnTransitionInComplete -= OnTransitionInComplete;

        // clear tween
        LeanTween.cancel(transitioningScreen.gameObject, false);

        // remove the current screen object
        if (currentScreen != null)
        {
            currentScreen.Remove();
            Destroy(currentScreen.gameObject);
        }
     
        // update CurrentScreenId with new screen id
        currentScreen = transitioningScreen;
        CurrentScreenID = transitioningScreenId;
    }

    // Foreground/Background Handler for Mobile
#if UNITY_IOS || UNITY_ANDROID
    public void OnApplicationPause(bool pause)
    {
        if (Overlay.IsOverlayShowing) return;

        // if true, app is in background.  Show pause
        if (pause)
        {
            Overlay.ShowOverlay(OverlayManager.PAUSE);
        }
    }
#endif

// Android Device Back Button
#if UNITY_ANDROID
    void Update()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                //if (IsPauseScreenShowing) Application.Quit();
                //else ShowPauseOverlay();
            }
        }
    }
#endif

    private void InitializeAppURLLauncher(bool isEnabled)
    {
        if (!isEnabled) return;

        #if UNITY_IOS
            Application.targetFrameRate = 60;
        #endif

        #if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                Application.targetFrameRate = 60;
            }   
        #endif

        #if UNITY_IOS || UNITY_EDITOR
            urlSchemeHandler = GameObject.FindObjectOfType<URLSchemeHandler>();
            urlSchemeHandler.OnLaunchUrlEvent += OnLaunchUrlEvent;
        #endif
    }

    // Runs when user launches app via URL on iOS
    public void OnLaunchUrlEvent(Dictionary<string, string> parameters)
    {
        // Check if user info matches current user info if available
        if (parameters["fullname"] != PersistentModel.Instance.Name)
        {
            ResetGame(parameters);
        }
    }

    private void ResetGame(Dictionary<string, string> parameters)
    {
        // reset login properties
        PersistentModel.Instance.Server.isShowLogin = false;
        PersistentModel.Instance.Server.isPasscodeAuthorized = false;

        // update new user credentials
        PersistentModel.Instance.UpdateUserParameters(parameters);

        // reset time scale 
        Time.timeScale = 1;

        // stop update loop
        if (GameManager.Instance != null)
        {
            //Debug.Log(" GameManager.Instance NOT NULL");

            GameManager.Instance.ForceCompleted();
        }

        // show loading screen again to reload user
        currentScreen.Remove();
        Destroy(currentScreen.gameObject);
        StartCoroutine(ShowStartUpLoadingScreen());
    }

}
