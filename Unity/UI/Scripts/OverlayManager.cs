using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityStandardAssets.CrossPlatformInput;

public class OverlayManager {

    public const string ALERT = "AlertOverlay";
    public const string LOGIN = "LoginPanel";
    public const string PAUSE = "PauseScreenOverlay";
    public const string GAMECOUNT = "GameScreenCountPanel";
    public const string INGAMEALERT = "InGameAlertPanel";

    public bool IsOverlayShowing = false;
    public bool IsPauseOverlayShowing = false;

    private BaseScreenOverlay _baseOverlay;
    private UnityAction _onCloseCallback;

    private UIManager ui;
    public void Initialize()
    {
        ui = UIManager.Instance;
    }

    public void ShowOverlay(string name, UnityAction OnCloseCallback = null)
    {
        switch (name)
        {
            case LOGIN:
                ShowLoginOverlay(OnCloseCallback);
                break;

            case PAUSE:
                ShowPauseOverlay(OnCloseCallback);
                break;

            case GAMECOUNT:
                ShowGameCountPanel();
                break;

        }
    }

    private void ShowLoginOverlay(UnityAction OnCloseCallback = null)
    {
        _onCloseCallback = OnCloseCallback;

        ui.StartCoroutine(LoadLoginOverlay());
    }

    IEnumerator LoadLoginOverlay()
    {
        GameObject loginPanel = (GameObject)GameObject.Instantiate(Resources.Load("LoginPanel"));

        _baseOverlay = loginPanel.GetComponent<LoginScreenOverlay>();
        _baseOverlay.OnScreenOverlayClose += OnLoginClose;

        // Wait for the load
        yield return _baseOverlay;

        // Add to Canvas
        _baseOverlay.transform.SetParent(ui.transform, false);
        _baseOverlay.Initialize();
    }

    private void OnLoginClose()
    {
        _baseOverlay.OnScreenOverlayClose -= OnLoginClose;

        _onCloseCallback?.Invoke();

        _onCloseCallback = null;
    }



    // PAUSE OVERLAY ////////////////////////////////////////////////////////////////
    private PauseScreenOverlay _pauseScreenOverlay;
    //public bool IsPauseScreenShowing = false;
    private UnityAction _onPauseCloseCallback;
    private void ShowPauseOverlay(UnityAction OnPauseCloseCallback = null)
    {
        DebugLog.Trace("ShowPauseOverlay.IsOverlayShowing" + IsOverlayShowing);

        _onCloseCallback = OnPauseCloseCallback;

        if (!IsPauseOverlayShowing) ui.StartCoroutine(LoadPauseOverlay());
    }

    IEnumerator LoadPauseOverlay()
    {
        DebugLog.Trace("LoadPauseOverlay");

        IsPauseOverlayShowing = true;

        GameObject pauseObject = (GameObject)GameObject.Instantiate(Resources.Load("PauseScreenOverlay"));

        _pauseScreenOverlay = pauseObject.GetComponent<PauseScreenOverlay>();
        _pauseScreenOverlay.OnScreenOverlayClose += OnPauseClose;

        // Wait for the load
        yield return _pauseScreenOverlay;

        // if game screen, hide drive controls *mobile only
        if (ui.CurrentScreenID == UIManager.GAME_SCREEN && MobileTools.IsMobile) HideDriveControls();

        // Add to Canvas
        _pauseScreenOverlay.transform.SetParent(ui.transform, false);
        _pauseScreenOverlay.Initialize();
    }

    private void OnPauseClose()
    {
        // if game screen, show drive controls *mobile only
        if (ui.CurrentScreenID == UIManager.GAME_SCREEN && MobileTools.IsMobile) ShowDriveControls();

        _pauseScreenOverlay.OnScreenOverlayClose -= OnPauseClose;

        PersistentModel.Instance.ClockIsStopped = IsPauseOverlayShowing = false;

        _onCloseCallback?.Invoke();

        _onCloseCallback = null;
    }

    // MOBILE CONTROLS OVERLAY ////////////////////////////////////////////////////////////////
    private void ShowDriveControls()
    {
        if (GameObject.Find("Accelerator_a") != null)
        {
            GameObject.Find("Accelerator_a").GetComponent<AxisTouchButton>().Show();
            GameObject.Find("Brake_b").GetComponent<AxisTouchButton>().Show();
        }
    }

    private void HideDriveControls()
    {
        if (GameObject.Find("Accelerator_a") != null)
        {
            GameObject.Find("Accelerator_a").GetComponent<AxisTouchButton>().Hide();
            GameObject.Find("Brake_b").GetComponent<AxisTouchButton>().Hide();
        }
    }


    // GAME COUNT FUNCTIONS ///////////////////////////////////////////////////////////////////
    /**
     * OnGameCountComplete is added on Race.OnSceneLoaded
     * It is triggered when game count has completed
     *  
     * */
    public event OnGameCountCompleteEventHandler OnGameCountComplete;

    // game screen countdown
    private GameCountPanel gameCountPanel;
    private void ShowGameCountPanel()
    {
        IsOverlayShowing = true;

        ui.StartCoroutine(LoadGameCountPanel());
    }

    IEnumerator LoadGameCountPanel()
    {
        GameObject gameCountObject = (GameObject)GameObject.Instantiate(Resources.Load("GameScreenCountPanel"));

        gameCountPanel = gameCountObject.gameObject.GetComponent<GameCountPanel>();
        gameCountPanel.OnGameCountComplete += OnGameCountCompletePrivate;
        gameCountPanel.OnGameCountFinished += OnGameCountFinishedPrivate;

        yield return gameCountPanel;

        gameCountPanel.transform.SetParent(ui.transform, false);
        gameCountPanel.Initialize();
    }

    // complete triggers when showing GO
    private void OnGameCountCompletePrivate()
    {
        gameCountPanel.OnGameCountComplete -= OnGameCountCompletePrivate;

        OnGameCountComplete();
    }

    // finished triggers when GO's animation is finished
    private void OnGameCountFinishedPrivate()
    {
        gameCountPanel.OnGameCountFinished -= OnGameCountFinishedPrivate;
        gameCountPanel.Remove();
        GameObject.Destroy(gameCountPanel.gameObject);
    }


    // IN GAME ALERT OVERLAY ///////////////////////////////////////////////////////////////////////
    
    public InGameAlertOverlay InGameAlert { get; private set; }

    public void ShowGameAlert(OverlaySettings settings, UnityAction OnCloseCallback)
    {
        GameObject panel = (GameObject)GameObject.Instantiate(Resources.Load("InGameAlertPanel"));

        _onCloseCallback = OnCloseCallback;

        InGameAlert = panel.GetComponent<InGameAlertOverlay>();
        InGameAlert.BodyText = settings.body;
        InGameAlert.OnScreenOverlayClose += OnInGameAlertClose;
        InGameAlert.transform.SetParent(ui.transform, false);
        InGameAlert.Initialize();

    }

    private void OnInGameAlertClose()
    {
        InGameAlert.OnScreenOverlayClose -= OnInGameAlertClose;

        _onCloseCallback?.Invoke();

        _onCloseCallback = null;
    }
}
