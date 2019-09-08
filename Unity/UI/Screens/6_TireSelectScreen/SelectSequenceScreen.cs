using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectSequenceScreen : BaseScreen
{
    public Color elementShowColor = new Color(1f, 1f, 1f, 1f);
    public Color elementHideColor = new Color(1f, 1f, 1f, 0f);
    public Color elementLoadingColor = new Color(1f, 1f, 1f, 120f / 255f);
  
    private float elapsedTime = 0.0f;                       // holds the time on the clock
    
    public enum Section
    {
        CUST_PREF,
        TIRESELECT
    }

    private CustomerPreferencesSection customerPreferencesSection;
    private TireSelectSection tireSelectSection;

    private RectTransform timeTopPanel;
    private Text gameTimeText;
    private Vector3 timePanelFrom;
    private Vector3 timePanelTo;

    private RectTransform clockRunningTopMidPanel;
    private Vector3 clockRMidPanelFrom;
    private Vector3 clockRMidPanelTo;

    private RectTransform clockStoppedTopMidPanel;
    private Vector3 clockSMidPanelFrom;
    private Vector3 clockSMidPanelTo;

    private RectTransform currentCircuitValue;

    private RectTransform superTireSmallLogo;
    
    [HideInInspector]
    public RectTransform pauseButton, submitButton, sections;

    public bool HasTireBeenSubmitted { get; set; }
    public int TireSelectedIndex { get; set; }

    private IntUnityEvent getRecordTimeFromServerAttempt;

    public override void Initialize(string id)
    {
        base.Initialize(id);

        DebugLog.Trace("SelectSequenceScreen");
        DebugLog.Trace("PersistentModel.Instance.ChallengeTime: " + PersistentModel.Instance.ChallengeTime);

        showProgressLoadingPanel = true;
        elapsedTime = PersistentModel.Instance.ChallengeTime;

        HasTireBeenSubmitted = false;
        TireSelectedIndex = -1;

        // grab sections and hide scale
        sections = _screenElements["Sections"];
        sections.localScale = new Vector3(0f, 1f, 1f);

        pauseButton = _screenElements["PauseButton"];
        superTireSmallLogo = _screenElements["SuperTireSmallLogo"];

        // set submit
        submitButton = _screenElements["SubmitButton"];
        submitButton.GetComponent<Image>().color = elementShowColor;
        submitButton.Find("Text").GetComponent<Text>().color = elementShowColor;
        submitButton.GetComponent<CanvasGroup>().alpha = 0;
        submitButton.GetComponent<CanvasGroup>().interactable = false;
       
        // set time panel
        timeTopPanel = _screenElements["TimeTopPanel"];
        timePanelFrom = timeTopPanel.anchoredPosition3D + Vector3.up * timeTopPanel.sizeDelta.y;
        timePanelTo = timeTopPanel.anchoredPosition3D;
        timeTopPanel.anchoredPosition3D = timePanelFrom;
        timeTopPanel.GetComponent<Image>().color = elementShowColor;

        // set game time
        gameTimeText = _screenElements["GameTimeText"].GetComponent<Text>();
        gameTimeText.color = new Color(33f / 255f, 33f / 255f, 33f / 255f, 0f);
        gameTimeText.text = PersistentModel.Instance.FormatTime(PersistentModel.Instance.ChallengeTime);

        // set clock notification panel
        clockRunningTopMidPanel = _screenElements["ClockRunningTopMidPanel"];
        clockStoppedTopMidPanel = _screenElements["ClockStoppedTopMidPanel"];

        Vector3 orgRunningPos = clockRunningTopMidPanel.anchoredPosition3D;
        clockRunningTopMidPanel.anchoredPosition3D = new Vector3(orgRunningPos.x, sections.anchoredPosition3D.y, orgRunningPos.z);

        Vector3 orgStoppedPos = clockStoppedTopMidPanel.anchoredPosition3D;
        clockRunningTopMidPanel.anchoredPosition3D = new Vector3(orgStoppedPos.x, sections.anchoredPosition3D.y, orgStoppedPos.z);

        clockRMidPanelFrom = clockRunningTopMidPanel.anchoredPosition3D + Vector3.down * clockRunningTopMidPanel.sizeDelta.y;
        clockRMidPanelTo = clockRunningTopMidPanel.anchoredPosition3D;
        clockSMidPanelFrom = clockStoppedTopMidPanel.anchoredPosition3D + Vector3.down * clockStoppedTopMidPanel.sizeDelta.y;
        clockSMidPanelTo = clockStoppedTopMidPanel.anchoredPosition3D;
        clockRunningTopMidPanel.anchoredPosition3D = clockRMidPanelFrom;
        clockStoppedTopMidPanel.anchoredPosition3D = clockSMidPanelFrom;

        // top stats info
        Color saveColor = new Color(1f, 0.7686275f, 0f, 1f);

        _screenElements["CurrentCircuitValue"].GetComponent<Text>().color = new Color(saveColor.r, saveColor.g, saveColor.b, 0f);

        // THIS CAN BEDONE BETTER ///////////////////////////////////////////////////////////////
        string currentCircuitLabel = PersistentModel.Instance.GameModeID.ToUpper() + " ";
        // int cIndex = PersistentModel.Instance.ChallengeIndex;
        int cIndex = PersistentModel.Instance.ChallengeCounter;
        if (cIndex == 0) cIndex = 1;
        else if (cIndex == PersistentModel.Instance.ChallengeCount) cIndex = 5;
        else cIndex++;
        /////////////////////////////////////////////////////////////////////////////////

        currentCircuitLabel += cIndex + "/" + PersistentModel.Instance.ChallengeCount;

        if (DebugHandler.isEnabled) currentCircuitLabel = "CHALLENGE #" + PersistentModel.Instance.ChallengeUID;
        _screenElements["CurrentCircuitValue"].GetComponent<Text>().text = currentCircuitLabel;

        // tire select section ////////
        customerPreferencesSection = sections.Find("CustomerPreferences").GetComponent<CustomerPreferencesSection>();
        customerPreferencesSection.Initialize();

        tireSelectSection = sections.Find("TireSelect").GetComponent<TireSelectSection>();
        tireSelectSection.Initialize();

        currState = (PersistentModel.Instance.TireOptionSelectedData == null) ? Section.CUST_PREF : Section.TIRESELECT;

        // GET RECORD TIME from SERVER
        getRecordTimeFromServerAttempt = new IntUnityEvent();
        getRecordTimeFromServerAttempt.AddListener(OnGetRecordAttempt);
        PersistentModel.Instance.Server.GetRecordTimeByTrackID(PersistentModel.Instance.ChallengeTrackUID, getRecordTimeFromServerAttempt);
    }

    private void OnGetRecordAttempt(ServerData data)
    {
        DebugLog.Trace("GetRecordTimeByTrackID.data.currentTrackRecordTime:" + data.currentTrackRecordTime);
        PersistentModel.Instance.CurrentTrackRecordTime = data.currentTrackRecordTime;
        PersistentModel.Instance.ResultData = data;
        
        getRecordTimeFromServerAttempt.RemoveListener(OnGetRecordAttempt);
        getRecordTimeFromServerAttempt = null;
    }

    private void ShowCurrentStats()
    {
        LeanTween.alphaText(_screenElements["CurrentCircuitValue"], 1f, 0.75f)
            .setDelay(0.85f)
            .setEase(LeanTweenType.easeOutQuad)
            .setFrom(0f);
    }

    private void HideCurrentStats()
    {
        LeanTween.alphaText(_screenElements["CurrentCircuitValue"], 0f, 0.75f)
            .setDelay(0.85f)
            .setEase(LeanTweenType.easeOutQuad)
            .setFrom(1f);
    }

    public void UpdateSubmitButton(string label)
    {
        submitButton.GetComponent<Image>().raycastTarget = true;
        submitButton.GetComponent<CanvasGroup>().blocksRaycasts = true;

        // check if its the same, if so do nothing
        if (submitButton.Find("Text").GetComponent<Text>().text == label) return;

        submitButton.GetComponent<Button>().onClick.RemoveAllListeners();
        submitButton.GetComponent<Button>().interactable = false;
        submitButton.GetComponent<CanvasGroup>().interactable = false;
        submitButton.GetComponent<CanvasGroup>().alpha = 0;

        LeanTween.alphaCanvas(submitButton.GetComponent<CanvasGroup>(), 0f, 0.5f)
            .setEase(LeanTweenType.easeInCubic)
            .setDelay(0.0f)
            .setOnComplete(() => {

                submitButton.Find("Text").GetComponent<Text>().text = label;

                LeanTween.alphaCanvas(submitButton.GetComponent<CanvasGroup>(), 1f, 0.5f)
                    .setEase(LeanTweenType.easeOutCubic);

                submitButton.GetComponent<Button>().interactable = true;
                submitButton.GetComponent<CanvasGroup>().interactable = true;
            });
    }

    public void HideSubmitButton()
    {
        submitButton.GetComponent<Image>().raycastTarget = false;
        submitButton.GetComponent<CanvasGroup>().blocksRaycasts = false;

        LeanTween.alphaCanvas(submitButton.GetComponent<CanvasGroup>(), 0f, 0.5f)
            .setEase(LeanTweenType.easeInCubic)
            .setDelay(0.0f)
            .setOnComplete(() => {

                submitButton.GetComponent<Button>().interactable = false;
                submitButton.GetComponent<CanvasGroup>().interactable = false;
                submitButton.Find("Text").GetComponent<Text>().text = "";
            });
    }

    public void ShowPauseButton()
    {
        pauseButton.GetComponent<Button>().interactable = true;
        pauseButton.GetComponent<Button>().onClick.AddListener(OnPauseButtonClick);

        // show pause button
        LeanTween.alpha(pauseButton, 1f, 0.65f).setEase(LeanTweenType.easeOutCubic).setDelay(0.0f);
    }

    public void HidePauseButton()
    {
        // hide pause button
        LeanTween.alpha(pauseButton, 0f, 0.65f).setEase(LeanTweenType.easeOutCubic).setDelay(0.0f)
            .setOnComplete(() => {

                pauseButton.GetComponent<Button>().interactable = false;
                pauseButton.GetComponent<Button>().onClick.RemoveListener(OnPauseButtonClick);
            });
    }

    private Section currState = Section.CUST_PREF;

    public Section CurrentState
    {
        set {
            currState = value;
            UpdateSectionState();
        }
    }

    private void UpdateSectionState()
    {
        switch (currState)
        {
            case Section.CUST_PREF:
                HidePauseButton();
                customerPreferencesSection.Draw();
                tireSelectSection.PrepareDraw(true);
                break;

            case Section.TIRESELECT:
                ShowPauseButton();
                tireSelectSection.Draw();
                customerPreferencesSection.PrepareDraw();
                break;
        }
    }
    
    public override void Draw()
    {
        ShowCurrentStats();

        LeanTween.alpha(superTireSmallLogo, 1f, 0.75f).setEase(LeanTweenType.easeOutCubic).setDelay(0f);

        sections.GetComponent<Image>().color = elementShowColor;

        LeanTween.scale(sections, new Vector3(1f, 1f, 1f), 0.65f).setDelay(0.15f).setOvershoot(0.5f).setEase(LeanTweenType.easeOutBack)
            .setOnComplete(() => {

                if (currState == Section.CUST_PREF) customerPreferencesSection.Draw();
                else if (currState == Section.TIRESELECT)
                {
                    InitializeTimeClock();
                    ShowPauseButton();

                    tireSelectSection.HasInitialized = true;
                    tireSelectSection.PrepareDraw(false);
                    tireSelectSection.Draw();
                }

                TransitionInCompleted();
            });
    }

    void OnPauseButtonClick()
    {
        UIManager.Instance.soundManager.PlaySound("PlaySineWaveHighPitch");

        StopClock();

        UIManager.Instance.Overlay.ShowOverlay(OverlayManager.PAUSE, OnPauseClose);
    }

    void OnPauseClose()
    {
        StartClock();
    }

    protected override void TransitionInCompleted()
    {
        // screen draw has been finished, invokes Base.TransitionInCompleteEvent if available
        base.TransitionInCompleted();

        // Debug.Log("Select.TransitionInCompleted: " + currState);
    }
    
    protected override void OnGamePadDPadLeftButton()
    {
        base.OnGamePadDPadLeftButton();

        if (currState == Section.TIRESELECT)
        {
            tireSelectSection.OnLeftButton();
        }
    }

    protected override void OnGamePadDPadRightButton()
    {
        base.OnGamePadDPadRightButton();

        Debug.Log("currState: " + currState);

        if (currState == Section.TIRESELECT)
        {
            tireSelectSection.OnRightButton();
        }
    }

    public void StartTransitionOut()
    {
        HideCurrentStats();

        LeanTween.alpha(superTireSmallLogo, 0f, 0.5f).setEase(LeanTweenType.easeOutCubic).setDelay(0.0f);

        LeanTween.alphaText(gameTimeText.GetComponent<RectTransform>(), 0f, 0.4f).setEase(LeanTweenType.easeOutQuad).setDelay(0.01f);
        LeanTween.alpha(timeTopPanel, 0f, 0.4f).setEase(LeanTweenType.easeOutQuad).setDelay(0.05f);
        LeanTween.alpha(pauseButton, 0f, 0.4f).setEase(LeanTweenType.easeOutQuad).setDelay(0.1f);

        LeanTween.cancel(clockStoppedTopMidPanel);
        LeanTween.cancel(clockRunningTopMidPanel);

        // hide the clock status labels
        clockStoppedTopMidPanel.GetComponent<Image>().color = elementHideColor;
        clockRunningTopMidPanel.GetComponent<Image>().color = elementHideColor;

         // set to game screen
         selectedScreen = UIManager.Screen.GAME;

        // Set Progress Complete Event 
        OnProgressLoadingTransitionInComplete += ProgressLoadingTransitionInComplete;

        LeanTween.scale(sections, new Vector3(0f, 1f, 1f), 0.65f).setDelay(0.0f).setOvershoot(0.5f).setEase(LeanTweenType.easeOutBack)
            .setOnComplete(() => {

                base.OpenLoadingPanel();
            });
    }

    public override void Remove()
    {
        if (pauseButton != null) pauseButton.GetComponent<Button>().onClick.RemoveListener(OnPauseButtonClick);

        base.Remove();
    }

    override protected void Update()
    {
        base.Update();

        // set the time if clock's running
        if (!PersistentModel.Instance.ClockIsStopped)
        {
            elapsedTime += Time.deltaTime;

            PersistentModel.Instance.ChallengeTime = elapsedTime;

            if (gameTimeText != null) gameTimeText.text = PersistentModel.Instance.FormatTime(elapsedTime);
        }
    }


    /* CLOCK FUNCTIONS */

    public void InitializeTimeClock()
    {
        // Show the Time Clock
        timeTopPanel.anchoredPosition3D = timePanelFrom;
        timeTopPanel.GetComponent<Image>().color = elementShowColor;
        LeanTween.move(timeTopPanel, timePanelTo, 0.95f).setEase(LeanTweenType.easeInOutCubic).setDelay(0.5f).setOvershoot(0.95f)
            .setOnComplete(() => {

                LeanTween.alphaText(gameTimeText.GetComponent<RectTransform>(), 1f, 0.65f).setEase(LeanTweenType.easeOutCubic);

                // if we have not submitted a tire, start clock and show options
                if (TireSelectedIndex == -1 && !HasTireBeenSubmitted)
                {
                    StartClock();
                    ShowClockIsRunningSign();
                }

                TransitionInCompleted();    // End of Transition
            });
    }

    public void StopClock()
    {
        PersistentModel.Instance.ClockIsStopped = true;
    }

    public void StartClock()
    {
        PersistentModel.Instance.ClockIsStopped = false;
    }

    public void ShowClockIsRunningSign()
    {
        // Show "clock is running" sign
        clockRunningTopMidPanel.GetComponent<Image>().color = elementShowColor;
        LeanTween.move(clockRunningTopMidPanel, clockRMidPanelTo, 0.75f).setEase(LeanTweenType.easeInOutCubic).setDelay(0.5f).setOvershoot(0.95f).setIgnoreTimeScale(true);

        // Pulse the 'clock is running' sign
        LeanTween.value(0f, 1f, 0.5f).setDelay(1.20f).setEase(LeanTweenType.easeOutCubic).setIgnoreTimeScale(true)
            .setOnComplete(() => {
                FlickerAnimation(clockRunningTopMidPanel, 0.25f, 16, false);
            });
    }

    public void HideClockIsRunningSign()
    {
        LeanTween.move(clockRunningTopMidPanel, clockRMidPanelFrom, 0.75f).setIgnoreTimeScale(true).setEase(LeanTweenType.easeInOutCubic);
    }

    public void HideClockStopSign()
    {
        LeanTween.move(clockStoppedTopMidPanel, clockSMidPanelFrom, 0.75f).setIgnoreTimeScale(true).setEase(LeanTweenType.easeInOutCubic);
    }

    public void ShowClockIsStoppedSign()
    {
        // Hide 'clock is running' if it is showing
        HideClockIsRunningSign();

        // Show "clock is stopped" message
        clockStoppedTopMidPanel.GetComponent<Image>().color = elementShowColor;
        LeanTween.move(clockStoppedTopMidPanel, clockSMidPanelTo, 0.75f).setEase(LeanTweenType.easeInOutCubic).setIgnoreTimeScale(true).setOvershoot(0.95f);
    }

    public void HideClockIsStoppedSign()
    {
        // Show "clock is running" sign
        clockStoppedTopMidPanel.GetComponent<Image>().color = elementShowColor;
        LeanTween.move(clockStoppedTopMidPanel, clockSMidPanelFrom, 0.5f).setEase(LeanTweenType.easeInOutCubic).setIgnoreTimeScale(true).setOvershoot(0.95f);
    }
}
