using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WelcomeBackScreen : BaseScreen {

    private RectTransform title;
    private RectTransform subtitle;
    private RectTransform popupBg;
    private RectTransform resumeBtn;
    private RectTransform startOverBtn;
    private RectTransform tutorialBtn;
    private RectTransform leaderboardBtn;
	private RectTransform superTiteSmallLogo;
	private RectTransform resumeTxt;
	private RectTransform startoverTxt;
	private RectTransform tutorialTxt;

    private RectTransform currentCircuitValue;
    private RectTransform currentTimeValue;

    private Vector3 titleFrom;
    private Vector3 titleTo;

    // Title, Subtitle, PopupBg, ResumeBtn, StartOverBtn, TutorialBtn, SuperTireSmallLogo

    public override void Initialize(string id)
    {
        base.Initialize(id);

        title = _screenElements["Title"];
        subtitle = _screenElements["Subtitle"];
        popupBg = _screenElements["PopupBg"];
        resumeBtn = _screenElements["ResumeBtn"];
        startOverBtn = _screenElements["StartOverBtn"];
        tutorialBtn = _screenElements["TutorialBtn"];
        leaderboardBtn = _screenElements["LeaderboardBtn"];
		resumeTxt = _screenElements["ResumeText"];
		startoverTxt = _screenElements["StartOverText"];
		tutorialTxt = _screenElements["TutorialText"];
        superTiteSmallLogo = _screenElements["SuperTireSmallLogo"];

        resumeBtn.localScale = Vector3.zero;
        resumeBtn.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);

        startOverBtn.localScale = Vector3.zero;
        startOverBtn.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);

        tutorialBtn.localScale = Vector3.zero;
        tutorialBtn.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);

        leaderboardBtn.localScale = Vector3.zero;
        leaderboardBtn.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);

        // JBC: Set alpha to zero -- not sure why I have to do it here, but oh well
        title.GetComponent<Text>().color = new Color(1f, 1f, 1f, 0f);
        subtitle.GetComponent<Text>().color = new Color(1f, 1f, 1f, 0f);
		resumeTxt.GetComponent<Text>().color = new Color(1f, 1f, 1f, 0f);
		startoverTxt.GetComponent<Text>().color = new Color(1f, 1f, 1f, 0f);
		tutorialTxt.GetComponent<Text>().color = new Color(1f, 1f, 1f, 0f);

        Color saveColor = new Color(1f, 0.7686275f, 0f, 1f);

        _screenElements["CurrentCircuitText"].GetComponent<Text>().color = new Color(1f, 1f, 1f, 0f);
        _screenElements["CurrentCircuitValue"].GetComponent<Text>().color = new Color(saveColor.r, saveColor.g, saveColor.b, 0f);
        _screenElements["CurrentTotalTimeText"].GetComponent<Text>().color = new Color(1f, 1f, 1f, 0f);
        _screenElements["CurrentTotalTimeValue"].GetComponent<Text>().color = new Color(saveColor.r, saveColor.g, saveColor.b, 0f);

        string currentCircuitLabel = PersistentModel.Instance.GameModeID.ToUpper() + " ";
        //int cIndex = PersistentModel.Instance.ChallengeIndex;
        int cIndex = PersistentModel.Instance.ChallengeCounter;
        currentCircuitLabel += cIndex + "/" + PersistentModel.Instance.ChallengeCount;
        _screenElements["CurrentCircuitValue"].GetComponent<Text>().text = currentCircuitLabel;

        string timeInSeconds = PersistentModel.Instance.TotalChallengeTime.ToString();
        _screenElements["CurrentTotalTimeValue"].GetComponent<Text>().text = PersistentModel.Instance.ConvertTime(timeInSeconds);

        // check if all tracks are completed
        if (PersistentModel.Instance.IsAllTracksComplete())
        {
            resumeBtn.GetComponent<Image>().raycastTarget = false;
            resumeBtn.GetComponent<Button>().interactable = false;
            resumeBtn.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.5f);
          
            _screenElements["CurrentCircuitValue"].GetComponent<Text>().text = "ALL COMPLETED";
            _screenElements["CurrentTotalTimeText"].GetComponent<Text>().text = "OVERALL TIME:";
            _screenElements["CurrentTotalTimeValue"].GetComponent<Text>().text = PersistentModel.Instance.FormatTime(PersistentModel.Instance.CurrentCircuitTime);
        }

        // Get Random Challenge Index
        if (PersistentModel.Instance.RandomizeTracks)
            PersistentModel.Instance.ChallengeIndex = PersistentModel.Instance.GetRandomChallengeIndex();
    }

    public override void Draw()
    {
        
        LeanTween.delayedCall(0.0f, () => { UIManager.Instance.soundManager.PlaySound("PlaySawLowHighTone", 0.5f); });
        
        titleFrom = title.anchoredPosition3D + Vector3.right * _screenElements["RightSideBg"].rect.width;
        titleTo = title.anchoredPosition3D;
        title.anchoredPosition3D = titleFrom;
        title.GetComponent<Text>().color = new Color(1f, 1f, 1f, 1f);
        
        LeanTween.delayedCall(0.25f, () => { UIManager.Instance.soundManager.PlaySound("PlayNoiseMidHighTone", 0.5f); });
        LeanTween.move(title, titleTo, 0.85f)
            .setEase(LeanTweenType.easeInOutBack)
            .setDelay(0.25f)
            .setOvershoot(0.95f)
            .setOnComplete(()=> {

                LeanTween.alphaText(subtitle, 1f, 0.85f)
                .setEase(LeanTweenType.easeOutQuad);

                popupBg.localScale = new Vector3(0f, 1f, 1f);
                
                LeanTween.delayedCall(0.25f, () => { UIManager.Instance.soundManager.PlaySound("PlayNoiseMidHighTone", 0.5f); });

                popupBg.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
                LeanTween.scale(popupBg, new Vector3(1f, 1f, 1f), 0.65f)
                .setDelay(0.25f)
                .setEase(LeanTweenType.easeOutBack)
                .setOnComplete(()=> {
                    UIManager.Instance.soundManager.PlaySound("PlayLongAmbientLowMidWaveVibrato", 0.25f);
                  
                    LeanTween.alpha(superTiteSmallLogo, 1f, 0.5f).setEase(LeanTweenType.easeOutQuad);

                    TransitionInCompleted();    // End of Transition
                });

                // Scale In Buttons
                LeanTween.scale(resumeBtn, new Vector3(1f, 1f, 1f), 0.75f)
                .setEase(LeanTweenType.easeOutBack)
                .setDelay(0.55f);

                LeanTween.scale(startOverBtn, new Vector3(1f, 1f, 1f), 0.75f)
                .setEase(LeanTweenType.easeOutBack)
                .setDelay(0.65f);

                LeanTween.scale(tutorialBtn, new Vector3(1f, 1f, 1f), 0.75f)
                .setFrom(Vector3.zero)
                .setEase(LeanTweenType.easeOutBack)
                .setDelay(0.75f);

                // Make text under buttons fade in
                float resumeTextAlpha = (PersistentModel.Instance.IsAllTracksComplete()) ? 0.5f : 1f;
             
                LeanTween.alphaText(resumeTxt, resumeTextAlpha, 1f)
					.setDelay(0.8f)
					.setEase(LeanTweenType.easeOutQuad)
					.setFrom(0f);
				LeanTween.alphaText(startoverTxt, 1f, 1f)
					.setDelay(0.8f)
					.setEase(LeanTweenType.easeOutQuad)
					.setFrom(0f);
				LeanTween.alphaText(tutorialTxt, 1f, 1f)
					.setDelay(0.8f)
					.setEase(LeanTweenType.easeOutQuad)
					.setFrom(0f);

                LeanTween.scale(leaderboardBtn, new Vector3(1f, 1f, 1f), 0.75f)
                .setFrom(Vector3.zero)
                .setEase(LeanTweenType.easeOutBack)
                .setDelay(0.95f);

            });
    }

    protected override void TransitionInCompleted()
    {
        // screen draw has been finished, invokes Base.TransitionInCompleteEvent if available
        base.TransitionInCompleted();

        UIManager.Instance.soundManager.mPlayer.PlayTrack(0);

        InitButtonEvents();

        ShowCurrentStats();

        DebugLog.Trace("GameModeChallengeCount: " + PersistentModel.Instance.GameModeChallengeCount);
    }

    private void ShowCurrentStats()
    {
        LeanTween.alphaText(_screenElements["CurrentCircuitText"], 1f, 0.75f)
            .setDelay(0.5f)
            .setEase(LeanTweenType.easeOutQuad)
            .setFrom(0f);

        LeanTween.alphaText(_screenElements["CurrentCircuitValue"], 1f, 0.75f)
            .setDelay(0.85f)
            .setEase(LeanTweenType.easeOutQuad)
            .setFrom(0f);

        LeanTween.alphaText(_screenElements["CurrentTotalTimeText"], 1f, 0.75f)
            .setDelay(0.5f)
            .setEase(LeanTweenType.easeOutQuad)
            .setFrom(0f);

        LeanTween.alphaText(_screenElements["CurrentTotalTimeValue"], 1f, 0.75f)
            .setDelay(0.85f)
            .setEase(LeanTweenType.easeOutQuad)
            .setFrom(0f);

    }

    private void HideCurrentStats()
    {
        LeanTween.alphaText(_screenElements["CurrentCircuitText"], 0f, 0.5f)
            .setEase(LeanTweenType.easeOutQuad)
            .setFrom(1f);

        LeanTween.alphaText(_screenElements["CurrentCircuitValue"], 0f, 0.5f)
            .setEase(LeanTweenType.easeOutQuad)
            .setFrom(1f);

        LeanTween.alphaText(_screenElements["CurrentTotalTimeText"], 0f, 0.5f)
            .setEase(LeanTweenType.easeOutQuad)
            .setFrom(1f);

        LeanTween.alphaText(_screenElements["CurrentTotalTimeValue"], 0f, 0.5f)
            .setEase(LeanTweenType.easeOutQuad)
            .setFrom(1f);

    }

    private void InitButtonEvents()
    {
        startOverBtn.gameObject.GetComponent<Button>().onClick.AddListener(OnStartButtonClick);
        resumeBtn.gameObject.GetComponent<Button>().onClick.AddListener(OnResumeButtonClick);
        tutorialBtn.gameObject.GetComponent<Button>().onClick.AddListener(OnTutorialButtonClick);
        leaderboardBtn.gameObject.GetComponent<Button>().onClick.AddListener(OnLeaderboardButtonClick);
    }

    private void RemoveButtonEvents()
    {
        startOverBtn.gameObject.GetComponent<Image>().raycastTarget = false;
        resumeBtn.gameObject.GetComponent<Image>().raycastTarget = false;
        tutorialBtn.gameObject.GetComponent<Image>().raycastTarget = false;
        leaderboardBtn.gameObject.GetComponent<Image>().raycastTarget = false;

        startOverBtn.gameObject.GetComponent<Button>().onClick.RemoveListener(OnStartButtonClick);
        resumeBtn.gameObject.GetComponent<Button>().onClick.RemoveListener(OnResumeButtonClick);
        tutorialBtn.gameObject.GetComponent<Button>().onClick.RemoveListener(OnTutorialButtonClick);
        leaderboardBtn.gameObject.GetComponent<Button>().onClick.RemoveListener(OnLeaderboardButtonClick);
    }

    private void OnResumeButtonClick()
    {
        UIManager.Instance.soundManager.PlaySound("PlaySineWaveHighPitch");

        if (!_isTransitionComplete) return;

        RemoveButtonEvents();

        // check if we have completed current circuit
        if (PersistentModel.Instance.GameTrackData.Count == PersistentModel.Instance.ChallengeCount)
        {
            PersistentModel.Instance.Reset();
            selectedScreen = UIManager.Screen.GAMEMODE_SELECTION;
        }
        else selectedScreen = UIManager.Screen.QUIZ_SCREEN;

        OnClickComplete += StartTransitionOut;
        ButtonClickAnimation(resumeBtn);
    }

    private void OnStartButtonClick()
    {
        UIManager.Instance.soundManager.PlaySound("PlaySineWaveHighPitch");

        if (!_isTransitionComplete) return;

        RemoveButtonEvents();

        // RESET GAME
        PersistentModel.Instance.Reset();
        PersistentModel.Instance.ResetTrackCompletion();

        // just save reset silently
        PersistentModel.Instance.Server.StartNewGameUpdate();        

        selectedScreen = UIManager.Screen.GAMEMODE_SELECTION;

        OnClickComplete += StartTransitionOut;
        ButtonClickAnimation(startOverBtn);
    }

    private void OnUpdateUserChallengeIndexComplete(bool success)
    {


    }

    private void OnTutorialButtonClick()
    {
        UIManager.Instance.soundManager.PlaySound("PlaySineWaveHighPitch");

        if (!_isTransitionComplete) return;

        RemoveButtonEvents();

        selectedScreen = UIManager.Screen.INSTRUCTIONS;

        OnClickComplete += StartTransitionOut;
        ButtonClickAnimation(tutorialBtn);
    }

    private void OnLeaderboardButtonClick()
    {
        UIManager.Instance.soundManager.PlaySound("PlaySineWaveHighPitch");
        if (!_isTransitionComplete) return;

        RemoveButtonEvents();

        tutorialBtn.gameObject.GetComponent<Button>().onClick.RemoveListener(OnLeaderboardButtonClick);

        selectedScreen = UIManager.Screen.LEADERBOARD;

        OnClickComplete += StartTransitionOut;
        ButtonClickAnimation(leaderboardBtn);
    }

    private void StartTransitionOut()
    {
        LeanTween.delayedCall(0.25f, () => { UIManager.Instance.soundManager.PlaySound("PlaySawLowHighTone"); });
        
        OnClickComplete -= StartTransitionOut;

        HideCurrentStats();

        LeanTween.alphaText(resumeTxt, 0f, 0.5f)
            .setEase(LeanTweenType.easeOutBack)
            .setDelay(0.1f);

        LeanTween.alphaText(startoverTxt, 0f, 0.5f)
            .setEase(LeanTweenType.easeOutBack)
            .setDelay(0.2f);

        LeanTween.alphaText(tutorialTxt, 0f, 0.5f)
            .setEase(LeanTweenType.easeOutBack)
            .setDelay(0.3f);

        LeanTween.alpha(superTiteSmallLogo, 0f, 0.5f)
        .setEase(LeanTweenType.easeInSine);

        LeanTween.scale(title, new Vector3(0f, 0f, 0.1f), 0.95f)
        .setEase(LeanTweenType.easeInOutBack)
        .setOvershoot(0.95f);

        LeanTween.alphaText(subtitle, 0f, 0.85f)
        .setEase(LeanTweenType.easeOutQuad)
        .setDelay(0.65f);

        LeanTween.scale(tutorialBtn, Vector3.zero, 0.85f)
        .setEase(LeanTweenType.easeInOutBack)
        .setOvershoot(0.5f)
        .setDelay(0.55f);

        LeanTween.scale(startOverBtn, Vector3.zero, 0.85f)
        .setEase(LeanTweenType.easeInOutBack)
        .setOvershoot(0.75f)
        .setDelay(0.4f);

        LeanTween.scale(resumeBtn, Vector3.zero, 0.85f)
        .setEase(LeanTweenType.easeInOutBack)
        .setOvershoot(0.75f)
        .setDelay(0.65f);

		LeanTween.scale(popupBg, new Vector3(0f, 1f, 1f), 0.65f)
        .setDelay(0.7f)
        .setOvershoot(0.35f)
        .setEase(LeanTweenType.easeOutBack);

        LeanTween.scale(leaderboardBtn, new Vector3(0f, 0f, 0f), 0.5f)
        .setEase(LeanTweenType.easeOutBack)
        .setDelay(0.45f);

        // Set Progress Complete Event 
        OnProgressLoadingTransitionInComplete += ProgressLoadingTransitionInComplete;

        base.OpenLoadingPanel();
    }

    public override void Remove()
    {
        LeanTween.cancel(title);
        LeanTween.cancel(subtitle);
        LeanTween.cancel(popupBg);
        LeanTween.cancel(resumeBtn);
        LeanTween.cancel(startOverBtn);
        LeanTween.cancel(tutorialBtn);
        LeanTween.cancel(superTiteSmallLogo);

        base.Remove();
    }
}
