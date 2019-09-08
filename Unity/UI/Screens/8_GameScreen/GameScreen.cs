using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityStandardAssets.CrossPlatformInput;
using System.Text;

public class GameScreen : BaseScreen
{
    // ui hud elements
    private RectTransform topHeaderBg;
    private RectTransform superTireSmallLogo;
    private RectTransform timeTopPanel;
    private RectTransform gameTimeText;
    private RectTransform pauseButton;
    private Text gameTimeTextComp;

    // in game notifications
    private RectTransform gameNotification;
    private Vector3 notificationFrom;
    private Vector3 notificationTo;
    private Image gameNotificationImage;
    private Sprite boomSprite;
    private Sprite bonusSprite;
    private Sprite raceCompleteSprite;

    // ui positions
    private Vector3 timePanelFrom;
    private Vector3 timePanelTo;
    private Vector3 headerPanelFrom;
    private Vector3 headerPanelTo;
    private Vector3 pauseButtonFrom;
    private Vector3 pauseButtonTo;
    private Vector3 muteButtonFrom;
    private Vector3 muteButtonTo;

    private bool paused;
    private Color elementStartColor;
    private PersistentModel pModel;
    private StringBuilder builder;
    private GameObject acceleratorUI;
    private GameObject carCamera;
    private Race race;

    public override void PreInitialize()
    {
        pModel = PersistentModel.Instance;

        _isGameScreenOverlay = true;
        isLoadingRequiredBeforeDraw = true;
        showProgressLoadingPanel = true;
        showSmallProgressLoadingPanel = false;

        builder = new StringBuilder(10, 16);
        
        base.PreInitialize();
    }

    public override void Initialize(string id)
    {
        base.Initialize(id);

        _screenElements["RightSideBg"] = null;
        topHeaderBg = _screenElements["TopHeaderBg"];
        superTireSmallLogo = _screenElements["SuperTireSmallLogo"];
        timeTopPanel = _screenElements["TimeTopPanel"];
        gameTimeText = _screenElements["GameTimeText"];
        pauseButton = _screenElements["PauseButton"];
        gameTimeTextComp = gameTimeText.GetComponent<Text>();

        // grab the sprites from all the in game notifications 
        // save them and we will use 1 sprite to show all
        // we will just replace the sprite
        // cache notification sprites
        gameNotification = _screenElements["Boom"];
        gameNotificationImage = gameNotification.GetComponent<Image>();
        boomSprite = gameNotificationImage.sprite;
        bonusSprite = _screenElements["Bonus"].GetComponent<Image>().sprite;
        raceCompleteSprite = _screenElements["Complete"].GetComponent<Image>().sprite;

        Destroy(_screenElements["Bonus"].gameObject);
        Destroy(_screenElements["Complete"].gameObject);
       
        notificationFrom = gameNotification.anchoredPosition3D + Vector3.up * gameNotification.sizeDelta.y;
        notificationTo = gameNotification.anchoredPosition3D;
        gameNotification.anchoredPosition3D = notificationFrom;

        // update game time
        SetTime(pModel.ChallengeTime);

        // Setup Top Header Panel
        headerPanelFrom = topHeaderBg.anchoredPosition3D + Vector3.up * topHeaderBg.sizeDelta.y;
        headerPanelTo = topHeaderBg.anchoredPosition3D;
        topHeaderBg.anchoredPosition3D = headerPanelFrom;
       
        // Setup Time Clock
        timePanelFrom = timeTopPanel.anchoredPosition3D + Vector3.up * timeTopPanel.sizeDelta.y;
        timePanelTo = timeTopPanel.anchoredPosition3D;
        timeTopPanel.anchoredPosition3D = timePanelFrom;
    
        // Setup Top Left Logo
        superTireSmallLogo.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);

        // Setup Game Time Text
        gameTimeTextComp.color = new Color(0f, 0f, 0f, 0f);

        // Setup Pause
        pauseButtonFrom = pauseButton.anchoredPosition3D + Vector3.right * (pauseButton.sizeDelta.x * 2);
        pauseButtonTo = pauseButton.anchoredPosition3D;
        pauseButton.anchoredPosition = pauseButtonFrom;

        // Setup Mute
        muteButtonFrom = muteButton.anchoredPosition3D + Vector3.right * (muteButton.sizeDelta.x * 3);
        muteButtonTo = muteButton.anchoredPosition3D;
        muteButton.anchoredPosition = muteButtonFrom;
    }

    readonly string timeFormat = "{0:00}:{1:00}";
    public void SetTime(float value)
    {
        int minutes = Mathf.FloorToInt(value / 60f);
        int seconds = Mathf.FloorToInt(value - minutes * 60f);

        builder.Clear();
        builder.AppendFormat(timeFormat, minutes, seconds);
        gameTimeTextComp.text = builder.ToString();
        
        //string niceTime = string.Format("{0:00}:{1:00}", minutes, seconds);
        //gameTimeTextComp.text = niceTime;

        // TODO: If update of string property in update loop is necessary, use StringBuilder object instead of string.
        // gameTimeTextComp.text = pModel.FormatTime(value);

    }

    public override void Load()
    {
        GameManager.Instance.OnLoaded += OnGameLoaded;
        GameManager.Instance.Load();
    }

    private void OnGameLoaded()
    {
        GameManager.Instance.OnLoaded -= OnGameLoaded;
        GameManager.Instance.OnCompleted += OnGameCompleted;

        System.GC.Collect();

        //race = GameObject.Find("Race").GetComponent<Race>();
        acceleratorUI = GameObject.Find("Accelerator_a");
        carCamera = Camera.main.gameObject;

        DispatchOnLoaded();
    }

    public override void Draw()
    {
        LeanTween.move(topHeaderBg, headerPanelTo, 0.6f)
            .setEase(LeanTweenType.easeOutCubic);

        LeanTween.move(timeTopPanel, timePanelTo, 0.5f)
            .setEase(LeanTweenType.easeOutCubic)
            .setDelay(0.15f)
            .setOnComplete(() =>
            {
                LeanTween.textColor(gameTimeText, new Color(0f, 0f, 0f, 1f), 0.5f)
                    .setEase(LeanTweenType.easeOutCubic)
                    .setDelay(0.05f);

                LeanTween.move(pauseButton, pauseButtonTo, 0.5f)
                    .setEase(LeanTweenType.easeOutCubic)
                    .setDelay(0.15f);

                LeanTween.move(muteButton, muteButtonTo, 0.5f)
                    .setEase(LeanTweenType.easeOutCubic)
                    .setDelay(0.2f);

                LeanTween.alpha(superTireSmallLogo, 1f, 0.65f)
                    .setEase(LeanTweenType.easeOutCubic)
						.setDelay(0.25f).setOnComplete(TransitionInCompleted);
            });

        if (acceleratorUI != null)
        {
            acceleratorUI.GetComponent<AxisTouchButton>().Show();
            GameObject.Find("Brake_b").GetComponent<AxisTouchButton>().Show();
        }
    }

    // TODO: Use one UI panel/sprite and swap cached textures
    public void ShowBoom(float shakeAmount = 10f, float shakeOption = 0.0f)
    {
        ExtensionMethods.Shake(carCamera, shakeAmount, shakeOption);

        gameNotificationImage.sprite = boomSprite;

        LeanTween.move(gameNotification, notificationTo, 0.25f).setEase(LeanTweenType.easeOutCubic).setOnComplete(() =>
        {
            LeanTween.move(gameNotification, notificationFrom, 0.2f).setEase(LeanTweenType.easeOutCubic).setDelay(0.25f);
        });
    }

    public void ShowBonus()
    {
        gameNotificationImage.sprite = bonusSprite;

        LeanTween.move(gameNotification, notificationTo, 0.25f).setEase(LeanTweenType.easeOutCubic).setOnComplete(() =>
        {
            LeanTween.move(gameNotification, notificationFrom, 0.2f).setEase(LeanTweenType.easeOutCubic).setDelay(0.35f);
        });
    }

    public void ShowComplete()
    {
        gameNotificationImage.sprite = raceCompleteSprite;

        LeanTween.move(gameNotification, notificationTo, 0.25f).setEase(LeanTweenType.easeOutCubic).setOnComplete(() =>
        {
            LeanTween.move(gameNotification, notificationFrom, 0.75f).setEase(LeanTweenType.easeOutCubic).setDelay(1.5f);
        });
    }

    protected override void TransitionInCompleted()
    {
        // screen draw has been finished, invokes Base.TransitionInCompleteEvent if available
        base.TransitionInCompleted();

        // listen to pause
        pauseButton.gameObject.GetComponent<Button>().onClick.AddListener(OnPauseButtonClick);

        // Start Game Time once all transitions have been completed
        pModel.ClockIsStopped = false;

        ShowGameCountPanel();
    }

    void OnPauseButtonClick()
    {
		pModel.ClockIsStopped = paused = !paused;

		AudioListener.volume = paused ? 0 : 1;

		UIManager.Instance.Overlay.ShowOverlay(OverlayManager.PAUSE, OnPauseClose);

		base.ClearButtonFocus();
    }

	void OnPauseClose()
	{
		pModel.ClockIsStopped = paused = !paused;

		AudioListener.volume = paused ? 0 : 1;

	    UIManager.Instance.soundManager.PlaySound("PlaySineWaveHighPitch");
    }

    void OnBrakeButtonClick()
    {
        base.ClearButtonFocus();
    }

    void ShowGameCountPanel()
    {
        UIManager.Instance.Overlay.ShowOverlay(OverlayManager.GAMECOUNT);
    }

    private void OnGameCompleted(float time)
    {
        GameManager.Instance.OnCompleted -= OnGameCompleted;

        // here we are stopping the clock flag, adding to the total time and resetting the challenge time.

        // stop clock and reset
        pModel.ClockIsStopped = true;

        // save current time to total time 
        // The Challenge Time should get added up to the TotalChallengeTime
        // We should save the TotalChallengeTime and the current ChallengeIndex to the server.  
        // If the user leaves early, we can load up the TotalChallengeTime and ChallengeIndex from the server to resume play
        pModel.SaveCurrentTotalTime();

        // before we reset ChallengeTime, lets save it to CurrentCompletedChallengeTime
        // we will be using it to display the time in the Congratulations Screen
        pModel.CurrentCompletedChallengeTime = pModel.ChallengeTime;

        // now reset ChallengeTime to 0
        pModel.ChallengeTime = 0;

        // transition out
        LeanTween.move(topHeaderBg, headerPanelFrom, .5f).setEase(LeanTweenType.easeOutCubic);
        LeanTween.textColor(gameTimeText, new Color(0f, 0f, 0f, 0f), 0.5f).setEase(LeanTweenType.easeOutCubic);
        LeanTween.move(timeTopPanel, timePanelFrom, 0.5f).setEase(LeanTweenType.easeOutCubic);
        LeanTween.move(pauseButton, pauseButtonFrom, 0.5f).setEase(LeanTweenType.easeOutCubic);
        LeanTween.move(muteButton, muteButtonFrom, 0.5f).setEase(LeanTweenType.easeOutCubic);
        LeanTween.alpha(superTireSmallLogo, 0f, 0.5f).setEase(LeanTweenType.easeOutCubic).setOnComplete(GotoNextScreen);
    }

    private void GotoNextScreen()
    {
        if (acceleratorUI != null)
        {
            acceleratorUI.GetComponent<AxisTouchButton>().Hide();
            GameObject.Find("Brake_b").GetComponent<AxisTouchButton>().Hide();
        }

        selectedScreen = UIManager.Screen.CONGRATULATIONS;
        UIManager.Instance.ShowScreen(selectedScreen);
    }

    public override void Remove()
    {
        pauseButton.gameObject.GetComponent<Button>().onClick.RemoveListener(OnPauseButtonClick);

        GameManager.Instance.OnCompleted -= OnGameCompleted;

        GameManager.Instance.Unload();

        carCamera = null;

        base.Remove();
    }

}
