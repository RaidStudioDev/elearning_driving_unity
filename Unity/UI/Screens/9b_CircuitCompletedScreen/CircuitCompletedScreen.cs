using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CircuitCompletedScreen : BaseScreen
{
	private RectTransform popupBg; 

	private RectTransform superTireSmallLogo;
	private RectTransform congratsBg;
	private RectTransform titleText;
	private RectTransform completedText;

	private RectTransform raceTrackBg;
	private RectTransform raceTrackBrightBg;
    private RectTransform yourTimeCircle;
	private RectTransform yourTimeCircleBright;
	private RectTransform yourTimeText;
	private RectTransform trackRecordCircle;
	private RectTransform trackRecordCircleBright;
	private RectTransform trackRecordText;
    private RectTransform trackRecordByText;

    private RectTransform leaderboardButton;
    private RectTransform continueButton;

    private Color elementShowColor = new Color(1f, 1f, 1f, 1f);
    private Color elementHideColor = new Color(1f, 1f, 1f, 0f);

    private IntUnityEvent circuitCompleteAction;

    public override void Initialize(string id)
    {
        _fadeInPanels = false;

        base.Initialize(id);

        gameObject.GetComponent<Image>().color = elementHideColor;

        superTireSmallLogo = _screenElements["SuperTireSmallLogo"];
		popupBg = _screenElements["PopupBg"];

		congratsBg = _screenElements["CongratsBg"];
		titleText = _screenElements["TitleText"];
		completedText = _screenElements["CompletedText"];
        titleText.GetComponent<Text>().color = elementHideColor;
		completedText.GetComponent<Text>().color = elementHideColor;
	
		raceTrackBg = _screenElements["RaceTrackBg"];
		raceTrackBrightBg = _screenElements["RaceTrackBrightBg"];

        yourTimeCircle = _screenElements["YourTimeCircle"];
		yourTimeCircleBright = _screenElements["YourTimeCircleBright"];
		yourTimeText = _screenElements["YourTimeText"];
		yourTimeText.GetComponent<Text> ().color = elementHideColor;
		trackRecordCircle = _screenElements["TrackRecordCircle"];
		trackRecordCircleBright = _screenElements["TrackRecordCircleBright"];
		trackRecordText = _screenElements["TrackRecordText"];
		trackRecordText.GetComponent<Text> ().color = elementHideColor;

        trackRecordByText = _screenElements["TrackRecordByText"];
        trackRecordByText.GetComponent<Text>().color = elementHideColor;

        continueButton = _screenElements["ContinueButton"];
		continueButton.Find("Text").GetComponent<Text>().color = elementHideColor;
		continueButton.GetComponent<Image>().color = elementHideColor;

        leaderboardButton = _screenElements["LeaderboardButton"];
        leaderboardButton.Find("LeaderboardText").GetComponent<Text>().color = elementHideColor;
        leaderboardButton.GetComponent<Image>().color = elementHideColor;


        // Setup completed text
        completedText.GetComponent<Text>().text = "YOU'VE COMPLETED ALL " + PersistentModel.Instance.GameModeID.ToUpper() + " TRACKS";
		
        // Setup your time text
        yourTimeText.GetComponent<Text>().text = PersistentModel.Instance.FormatTime(PersistentModel.Instance.CurrentCircuitTime);

        // Setup track record text
        int circuitRecordTime = PersistentModel.Instance.GetCircuitRecordTime(PersistentModel.Instance.GameModeID);
        trackRecordText.GetComponent<Text>().text = PersistentModel.Instance.FormatTime(circuitRecordTime);

        Color refColor = trackRecordByText.GetComponent<Text>().color;
        Color newColor = new Color(elementYellowColor.r, elementYellowColor.g, elementYellowColor.b, refColor.a);

        if (circuitRecordTime != 0)
        {
            // check if user circuit time is less/better then record circuit time
            if (PersistentModel.Instance.CurrentCircuitTime < circuitRecordTime)
            {
                // maybe show New Record flourish?
                trackRecordByText.GetComponent<Text>().color = newColor;

                // check if the user is the same as the user record
                if (PersistentModel.Instance.ResultData.fullname == PersistentModel.Instance.Name)
                    trackRecordByText.GetComponent<Text>().text = "You have beaten your own circuit record!";
                else
                    trackRecordByText.GetComponent<Text>().text = "You have beaten the circuit record set by " + PersistentModel.Instance.ResultData.fullname + "!!";

                yourTimeText.GetComponent<Text>().color = newColor;
            }
            else
            {
                trackRecordByText.GetComponent<Text>().text = "Circuit Record by: " + PersistentModel.Instance.ResultData.fullname;
            }
        }
        else
        {
            // set user circuit time record if nothing found from server
            trackRecordText.GetComponent<Text>().text = PersistentModel.Instance.FormatTime(circuitRecordTime);
        }

            
    }

	void OnResetDrawButtonClick() // testing
	{
		Draw();
	}

    public override void Draw()
    {
		// setup
		popupBg.localScale = new Vector3(0f, 1f, 1f);
		popupBg.GetComponent<Image>().color = elementShowColor;

		congratsBg.localScale = new Vector3(0f, 1f, 1f);
		congratsBg.GetComponent<Image>().color = elementShowColor;

        titleText.localScale = new Vector3(0f, 1f, 1f);
        titleText.GetComponent<Text>().color = elementShowColor;

        completedText.GetComponent<Text>().color = elementHideColor;

		raceTrackBrightBg.localScale = new Vector3(0f, 0f, 1f);
		raceTrackBrightBg.GetComponent<Image>().color = elementShowColor;
		raceTrackBg.GetComponent<Image>().color = elementHideColor;

		//yourTimeText.GetComponent<Text>().color = elementHideColor;
		//trackRecordText.GetComponent<Text> ().color = elementHideColor;

		trackRecordCircle.GetComponent<CanvasGroup>().alpha = 0f;
		trackRecordCircleBright.GetComponent<Image>().color = elementHideColor;

		yourTimeCircle.GetComponent<CanvasGroup>().alpha = 0f;
		yourTimeCircleBright.GetComponent<Image>().color = elementHideColor;

        StartCoroutine (StartTransitionIn());
    }

	IEnumerator StartTransitionIn()
	{
        yield return 0;

		// start animation sequence
		AnimateElementsSeq01();
	}

	void AnimateElementsSeq01()
	{
        // animation
        LeanTween.alpha(superTireSmallLogo, 1f, 0.75f)
			.setEase(LeanTweenType.easeInOutCubic)
			.setDelay(0f);
        
        LeanTween.delayedCall(0.15f, () => { UIManager.Instance.soundManager.PlaySound("PlaySawLowHighTone"); });

        LeanTween.scale(popupBg, new Vector3(1f, 1f, 1f), 0.65f)
			.setDelay(0.15f)
			.setOvershoot (0.5f)
			.setEase(LeanTweenType.easeOutBack);
        
        LeanTween.delayedCall(0.95f, () => { UIManager.Instance.soundManager.PlaySound("PlayNoiseMidHighTone"); });

        // Scale In Bright Effect
        LeanTween.scale (raceTrackBrightBg, new Vector3 (1f, 1f, 1f), 0.65f)
			.setDelay (0.75f)
			.setOvershoot (0.95f)
			.setEase(LeanTweenType.easeOutBack)
			.setFromColor(elementShowColor);

		LeanTween.alpha(raceTrackBg, 1f,  0.85f)
			.setEase(LeanTweenType.easeOutSine)
			.setDelay(0.95f);

        LeanTween.scale(titleText, new Vector3(titleScale.x, titleScale.y, 1f), 0.65f)
            .setDelay(0.35f)
            .setEase(LeanTweenType.easeOutBack);

        LeanTween.scale(congratsBg, new Vector3(1f, 1f, 1f), 0.65f)
			.setDelay(0.35f)
			.setEase(LeanTweenType.easeOutBack)
			.setOnComplete(() => {

				// Fade Out Bright Effect
				LeanTween.alpha(raceTrackBrightBg, 0f,  0.75f)
					.setEase(LeanTweenType.easeOutCubic)
					.setDelay(0f);
            });

		// show track times
		LeanTween.delayedCall(1.2f, () => {

			// all time circle
			LeanTween.scale(trackRecordCircleBright, new Vector3(1, 1, 1), 0.35f).setDelay(0f).setEase(LeanTweenType.easeOutCubic);

			LeanTween.alpha(trackRecordCircleBright, 1f, 0.35f)
				.setEase(LeanTweenType.easeOutCubic)
				.setDelay(0f);

			LeanTween.alphaCanvas(trackRecordCircle.GetComponent<CanvasGroup>(), 1f, 0.5f)
				.setEase(LeanTweenType.easeOutCubic)
				.setDelay(0.33f);

			LeanTween.alpha(trackRecordCircleBright, 0f, 0.45f)
				.setEase(LeanTweenType.easeOutSine)
				.setDelay(0.45f);

			// track time circle
			LeanTween.scale(yourTimeCircleBright, new Vector3(1, 1, 1), 0.35f).setDelay(0f).setEase(LeanTweenType.easeOutCubic);

			LeanTween.alpha(yourTimeCircleBright, 1f, 0.35f)
				.setEase(LeanTweenType.easeOutCubic)
				.setDelay(0f);

			LeanTween.alphaCanvas(yourTimeCircle.GetComponent<CanvasGroup>(), 1f, 0.5f)
				.setEase(LeanTweenType.easeOutCubic)
				.setDelay(0.33f);

			LeanTween.alpha(yourTimeCircleBright, 0f, 0.45f)
				.setEase(LeanTweenType.easeOutSine)
				.setDelay(0.45f)
				.setOnComplete(AnimateElementsSeq02); 
		});
	}

	// show times and badge
	void AnimateElementsSeq02()
	{
		LeanTween.alphaText(completedText, 1f, 1f).setDelay(0f)
			.setEase(LeanTweenType.easeOutQuad);
		
		LeanTween.alphaText(yourTimeText, 1f, 1f).setDelay(0f)
			.setEase(LeanTweenType.easeOutQuad);

        LeanTween.alphaText(trackRecordByText, 1f, 1f).setDelay(0f)
            .setEase(LeanTweenType.easeOutQuad);

        LeanTween.alphaText(trackRecordText, 1f, 1f).setDelay(0f)
			.setEase(LeanTweenType.easeOutQuad)
			.setOnComplete(() => {

                TransitionInCompleted();    // End of Transition
			});
	}

    protected override void TransitionInCompleted()
    {
        // screen draw has been finished, invokes Base.TransitionInCompleteEvent if available
        base.TransitionInCompleted();

        // This screen contains no loader panel, so...
        // the following code is for editor only, when force loading this screen at startup
#if UNITY_EDITOR
        GameObject progressLoadingPanel = UIManager.Instance.ProgressLoadingPanel;
        RectTransform spinningTire = progressLoadingPanel.transform.Find("SpinningTire").GetComponent<RectTransform>();
        LeanTween.cancel(spinningTire);
        progressLoadingPanel.transform.SetParent(null, false);
        progressLoadingPanel.transform.SetAsLastSibling();
#endif

        continueButton.gameObject.GetComponent<Button>().onClick.AddListener(OnContinueButtonClick);
        leaderboardButton.gameObject.GetComponent<Button>().onClick.AddListener(OnLeaderboardButtonClick);

        if (UIManager.Instance.PreviousScreenID == UIManager.Instance.GetScreenID(UIManager.Screen.LEADERBOARD))
        {
            // Debug.Log("Coming From Leaderboards, dont update anything");
            // enable continue until server is completed
            LeanTween.alpha(continueButton, 1f, 0.65f)
                        .setDelay(0f)
                        .setEase(LeanTweenType.easeOutQuad);

            LeanTween.alpha(leaderboardButton, 1f, 0.65f)
                .setDelay(0f)
                .setEase(LeanTweenType.easeOutQuad);
            return;
        }
       
        // update track completion
        PersistentModel.Instance.UpdateTrackCompletion();


        // mode is completed, save data
        leaderboardButton.gameObject.GetComponent<Image>().raycastTarget = false;
        continueButton.gameObject.GetComponent<Image>().raycastTarget = false;
        circuitCompleteAction = new IntUnityEvent();
        circuitCompleteAction.AddListener(OnCircuitCompletedUpdateServerComplete);
        
        PersistentModel.Instance.Server.CircuitCompleteUpdate(circuitCompleteAction);
    }

    private void OnCircuitCompletedUpdateServerComplete(ServerData data)
    {
        DebugLog.Trace("OnCircuitCompletedUpdateServerComplete: " + data);
        
        circuitCompleteAction.RemoveListener(OnCircuitCompletedUpdateServerComplete);
        circuitCompleteAction = null;

        // enable continue until server is completed
        LeanTween.alpha(continueButton, 1f, 0.65f)
                    .setDelay(0f)
                    .setEase(LeanTweenType.easeOutQuad);

        LeanTween.alpha(leaderboardButton, 1f, 0.65f)
            .setDelay(0f)
            .setEase(LeanTweenType.easeOutQuad);


        leaderboardButton.gameObject.GetComponent<Image>().raycastTarget = true;
        continueButton.gameObject.GetComponent<Image>().raycastTarget = true;

        Debug.Log("totalAllCircuitsTime: " + data.totalAllCircuitsTime);

        // update current data
        PersistentModel.Instance.ResultData.totalAllCircuitsTime = data.totalAllCircuitsTime;
    }

    private bool clickOnce = false;
    protected override void OnGamePadButton01()
    {
        base.OnGamePadButton01();

        if (clickOnce) return;

        clickOnce = true;

        OnContinueButtonClick();
    }


    private void OnContinueButtonClick()
    {
		UIManager.Instance.soundManager.PlaySound("PlaySineWaveHighPitch");

        if (!_isTransitionComplete) return;

        continueButton.gameObject.GetComponent<Image>().raycastTarget = false;
        continueButton.gameObject.GetComponent<Button>().onClick.RemoveListener(OnContinueButtonClick);

        // check if we all circuits completed
        if (PersistentModel.Instance.IsAllTracksComplete())
        {
            UIManager.Instance.soundManager.mPlayer.PlayTrack(4);

            selectedScreen = UIManager.Screen.CONGRATULATIONS_FINAL;
        }
        else    // circuit is complete, continue game and show game mode selection
        {
            UIManager.Instance.soundManager.mPlayer.PlayTrack(3);

            PersistentModel.Instance.Reset();
            selectedScreen = UIManager.Screen.GAMEMODE_SELECTION;
        }

        OnClickComplete += StartTransitionOut;
        ButtonClickAnimation(continueButton);
    }

    private void OnLeaderboardButtonClick()
    {
        UIManager.Instance.soundManager.PlaySound("PlaySineWaveHighPitch");

        if (!_isTransitionComplete) return;

        leaderboardButton.gameObject.GetComponent<Image>().raycastTarget = false;
        leaderboardButton.gameObject.GetComponent<Button>().onClick.RemoveListener(OnLeaderboardButtonClick);

        selectedScreen = UIManager.Screen.LEADERBOARD;

        OnClickComplete += StartTransitionOut;
        ButtonClickAnimation(leaderboardButton);
    }

    private void StartTransitionOut()
    {
        OnClickComplete -= StartTransitionOut;

    	completedText.GetComponent<Text>().color = elementHideColor;
		raceTrackBg.GetComponent<Image>().color = elementHideColor;
		yourTimeText.GetComponent<Text>().color = elementHideColor;
		trackRecordText.GetComponent<Text>().color = elementHideColor;
		trackRecordByText.GetComponent<Text>().color = elementHideColor;
		trackRecordCircle.GetComponent<CanvasGroup>().alpha = 0f;
		yourTimeCircle.GetComponent<CanvasGroup>().alpha = 0f;
		yourTimeCircleBright.GetComponent<Image>().color = elementHideColor;
	
		LeanTween.scale(congratsBg, new Vector3(0f, 1f, 1f), 0.85f)
			.setDelay(0f)
			.setEase(LeanTweenType.easeOutBack);

        LeanTween.scale(titleText, new Vector3(0f, 1f, 1f), 0.7f)
            .setDelay(0f)
            .setEase(LeanTweenType.easeOutBack);
        
        LeanTween.delayedCall(0.25f, () => { UIManager.Instance.soundManager.PlaySound("PlaySawLowHighTone"); });
        LeanTween.scale(popupBg, new Vector3(0f, 1f, 1f), 0.95f)
			.setDelay(0.25f)
			.setOvershoot(0.95f)
			.setEase(LeanTweenType.easeOutBack);


        LeanTween.alpha(continueButton, 0f, 0.65f)
                    .setDelay(0f)
                    .setEase(LeanTweenType.easeOutQuad);

        LeanTween.alpha(leaderboardButton, 0f, 0.65f)
            .setDelay(0f)
            .setEase(LeanTweenType.easeOutQuad);


        // Set Progress Complete Event 
        OnProgressLoadingTransitionInComplete += ProgressLoadingTransitionInComplete;

        base.OpenLoadingPanel();
    }

    public override void Remove()
    {
        base.Remove();
    }
}
