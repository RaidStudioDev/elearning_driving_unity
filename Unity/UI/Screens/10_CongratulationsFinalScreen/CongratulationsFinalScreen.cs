using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CongratulationsFinalScreen : BaseScreen
{
	private RectTransform popupBg;
	private RectTransform superTireSmallLogo;
    private RectTransform congratsBg;
    private RectTransform flareBehindTime;
    private RectTransform timeCircle;
	private RectTransform timeCircleBright;
    private RectTransform badge;
	private RectTransform badgeWhiteBox;
    private RectTransform yourTimeText;
    private RectTransform leaderboardButton;
    private RectTransform playAgainButton;
	private RectTransform titleSubText;
	private RectTransform totalTimeCompletedLabel;

    private Color elementShowColor = new Color(1f, 1f, 1f, 1f);
    private Color elementHideColor = new Color(1f, 1f, 1f, 0f);

    public override void Initialize(string id)
    {
        base.Initialize(id);

		popupBg = _screenElements["PopupBg"];
		superTireSmallLogo = _screenElements["SuperTireSmallLogo"];
        congratsBg = _screenElements["CongratsBg"];
        flareBehindTime = _screenElements["FlareBehindTime"];
        timeCircle = _screenElements["TimeCircle"];
		// timeCircleBright = _screenElements["TimeCircleBright"];
        badge = _screenElements["Badge"];
		badgeWhiteBox = _screenElements["BadgeWhiteBox"];
		yourTimeText = _screenElements["YourTimeText"];
		playAgainButton = _screenElements["PlayAgainButton"];
        leaderboardButton = _screenElements["LeaderboardButton"];

		titleSubText = _screenElements["CompletedTitleSubText"];
		totalTimeCompletedLabel = _screenElements["TotalTimeCompletedText"];

        // Define your time text
        PersistentModel.Instance.TotalChallengeTime = PersistentModel.Instance.ResultData.totalAllCircuitsTime;
        PersistentModel.Instance.CurrentCircuitTime = PersistentModel.Instance.ResultData.totalAllCircuitsTime;
        yourTimeText.GetComponent<Text>().text = PersistentModel.Instance.FormatTime(PersistentModel.Instance.TotalChallengeTime);
        // yourTimeText.GetComponent<Text>().text = PersistentModel.Instance.FormatTime(PersistentModel.Instance.ResultData.totalAllCircuitsTime);

        titleSubText.GetComponent<Text>().color = elementHideColor;
		totalTimeCompletedLabel.GetComponent<Text>().color = elementHideColor;
	}

	void OnResetDrawButtonClick() // testing
	{
		Draw();
	}

    public override void Draw()
    {
		yourTimeText.GetComponent<Text>().color = elementHideColor;
       
        // Setup leaderboard && play buttons
        leaderboardButton.GetComponent<CanvasGroup>().alpha = 0f;
        playAgainButton.GetComponent<CanvasGroup>().alpha = 0f;

		// setup
		popupBg.localScale = new Vector3(0f, 1f, 1f);
		popupBg.GetComponent<Image>().color = elementShowColor;

		congratsBg.localScale = new Vector3(0f, 1f, 1f);
		congratsBg.GetComponent<Image>().color = elementShowColor;

		flareBehindTime.GetComponent<Image> ().color = elementHideColor;
		flareBehindTime.localScale = new Vector3(1.0f, 1.0f, 1f);

		badgeWhiteBox.GetComponent<Image>().color = new Color(1, 1, 1, 0);
		badge.localScale = new Vector3(0f, 0f, 1f);
		badge.GetComponent<Image>().color = elementShowColor;

		yourTimeText.GetComponent<Text>().color = elementShowColor;
		timeCircle.GetComponent<CanvasGroup>().alpha = 0f;
	
		StartCoroutine (StartTransitionIn());
    }

	private WaitForSeconds _waitSecondsDelay = new WaitForSeconds (0.2f);

	IEnumerator StartTransitionIn()
	{
		// Setup badge

		Image image = badge.GetComponent<Image>();

		Sprite badgeSpr = PersistentModel.Instance.GetTrophy(PersistentModel.Instance.GetTrophyIndex());

		yield return badgeSpr;

		image.sprite = badgeSpr;

		yield return image;

		yield return _waitSecondsDelay;

		yield return new WaitForEndOfFrame();

		// start animation sequence
		AnimateElementsSeq01();
	}

	void AnimateElementsSeq01()
	{
		bool estimatedTime = false;

		// fade in small logo
		LeanTween.alpha(superTireSmallLogo, 1f, 0.75f)
			.setEase(LeanTweenType.easeInOutCubic)
			.setDelay(0f)
			.setUseEstimatedTime(estimatedTime);

		// scale in bacgkround panel
		LeanTween.scale(popupBg, new Vector3(1f, 1f, 1f), 0.5f)
			.setDelay(0.15f)
			.setOvershoot (1.05f)
			.setEase(LeanTweenType.easeOutBack)
			.setUseEstimatedTime(estimatedTime);

		LeanTween.scale (badge, new Vector3 (1f, 1f, 1f), 0.95f)
			.setDelay (0.45f)
			.setOvershoot (1.5f)
			.setEase(LeanTweenType.easeOutBack);

		// show time and effect
		LeanTween.delayedCall (1.1f, () => {

			LeanTween.alphaText(titleSubText, 1f, 0.75f)
				.setEase(LeanTweenType.easeOutCubic)
				.setDelay(0f)
				.setUseEstimatedTime(estimatedTime);

			LeanTween.alphaText(totalTimeCompletedLabel, 1f, 0.75f)
				.setEase(LeanTweenType.easeOutCubic)
				.setDelay(0.5f)
				.setUseEstimatedTime(estimatedTime);


			// badge shot and slow fade out
			LeanTween.delayedCall (0.1f, () => {
				badgeWhiteBox.GetComponent<Image>().color = new Color(1, 1, 1, 1);
				LeanTween.alpha(badgeWhiteBox, 0.25f, 0.95f)
					.setDelay(0.1f)
					.setEase(LeanTweenType.linear)
					.setUseEstimatedTime(estimatedTime);
				LeanTween.alpha(badgeWhiteBox, 0.95f, 2.50f)
					.setDelay(1.50f)
					.setLoopPingPong()
					.setEase(LeanTweenType.linear)
					.setUseEstimatedTime(estimatedTime);
			});

			// fade in time
			LeanTween.alphaCanvas(timeCircle.GetComponent<CanvasGroup>(), 1f,  0.45f)
				.setEase(LeanTweenType.easeOutSine)
				.setDelay (0.35f);		

			// slowly scale and fade in flare
			LeanTween.alpha(flareBehindTime, 1.0f, 0.95f)
				.setDelay(0.33f).setUseEstimatedTime(estimatedTime)
                .setUseEstimatedTime(estimatedTime)
                .setOnComplete(() => {

                    // show bottom buttons and init events
                    LeanTween.alphaCanvas(leaderboardButton.GetComponent<CanvasGroup>(), 1f, 0.85f).setEase(LeanTweenType.easeOutCubic);
                    LeanTween.alphaCanvas(playAgainButton.GetComponent<CanvasGroup>(), 1f, 0.85f).setEase(LeanTweenType.easeOutCubic);

                    LeanTween.alpha(flareBehindTime, 0.25f, 2.50f)
                    .setDelay(0.5f)
                    .setLoopPingPong()
                    .setEase(LeanTweenType.linear)
                    .setUseEstimatedTime(estimatedTime);

                    TransitionInCompleted();    // End of Transition
                });
            
            Vector3 badgeTo = badge.anchoredPosition3D;
            Vector3 badgeLoop = badgeTo + Vector3.up * 6f;
            LeanTween.move(badge, badgeLoop, 3.5f)
            .setEase(LeanTweenType.linear)
            .setLoopPingPong();


            LeanTween.scale(badge, new Vector3(1.10f, 1.01f, 0.90f), 4.5f)
           .setEase(LeanTweenType.easeInOutBack)
           .setLoopPingPong();


        });

		// scale in congrats title
		LeanTween.scale (congratsBg, new Vector3 (1f, 1f, 1f), 0.95f)
			.setDelay (0.35f)
			.setOvershoot (0.95f)
			.setEase (LeanTweenType.easeOutBack)
			.setUseEstimatedTime(estimatedTime);
	}

    protected override void TransitionInCompleted()
    {
        // screen draw has been finished, invokes Base.TransitionInCompleteEvent if available
        base.TransitionInCompleted();

        InitButtonEvents();

        if (isGamePadEnabled)
        {
            playAgainButton.GetComponent<Button>().Select();
        }

    }

    private void StopAnimations()
	{
		// cancel and remove badge white box
		LeanTween.cancel(badgeWhiteBox.gameObject);
		badgeWhiteBox.gameObject.SetActive (false);

        LeanTween.cancel(badge.gameObject);
        LeanTween.cancel(flareBehindTime.gameObject);       
    }

    private void InitButtonEvents()
    {
        leaderboardButton.gameObject.GetComponent<Button>().onClick.AddListener(OnLeaderboardButtonClick);
        playAgainButton.gameObject.GetComponent<Button>().onClick.AddListener(OnPlayAgainButtonClick);
    }

	private void RemoveButtonEvents()
	{
		leaderboardButton.gameObject.GetComponent<Button>().onClick.RemoveListener(OnLeaderboardButtonClick);
		playAgainButton.gameObject.GetComponent<Button>().onClick.RemoveListener(OnPlayAgainButtonClick);
	}

    private void OnLeaderboardButtonClick()
    {
        UIManager.Instance.soundManager.PlaySound("PlaySineWaveHighPitch");

        if (!_isTransitionComplete) return;

        leaderboardButton.GetComponent<Image>().raycastTarget = false;

        RemoveButtonEvents();

        selectedScreen = UIManager.Screen.LEADERBOARD;

        OnClickComplete += StartTransitionOut;
        ButtonClickAnimation(leaderboardButton);
    }

    private void OnPlayAgainButtonClick()
    {
		//Debug.Log("CongratulationsFinalScreen: OnPlayAgainButtonClick");
		
		UIManager.Instance.soundManager.PlaySound("PlaySineWaveHighPitch");

        if (!_isTransitionComplete) return;

        playAgainButton.GetComponent<Image>().raycastTarget = false;

        RemoveButtonEvents();

        PersistentModel.Instance.Reset();
        selectedScreen = UIManager.Screen.WELCOME_BACK;
        
        OnClickComplete += StartTransitionOut;
        ButtonClickAnimation(playAgainButton);
    }

    private void StartTransitionOut()
    {
        OnClickComplete -= StartTransitionOut;

		StopAnimations();

		LeanTween.alphaText(titleSubText, 0f, 0.5f).setEase(LeanTweenType.easeOutSine);
		LeanTween.alphaText(totalTimeCompletedLabel, 0f, 0.5f).setEase(LeanTweenType.easeOutSine);

		// fade in small logo
		LeanTween.alpha(superTireSmallLogo, 0f, 0.5f)
			.setEase(LeanTweenType.easeOutSine);

		yourTimeText.GetComponent<Text>().color = elementHideColor;

		LeanTween.scale (flareBehindTime, new Vector3 (0f, 0f, 0.1f), 0.5f)
			.setEase (LeanTweenType.easeInOutBack);
		LeanTween.scale (badge, new Vector3 (0f, 0f, 0.1f), 0.5f)
			.setEase (LeanTweenType.easeInOutBack);
		LeanTween.scale (timeCircle, new Vector3 (0f, 0f, 0.1f), 0.5f)
			.setEase (LeanTweenType.easeInOutBack);

		LeanTween.alphaCanvas(leaderboardButton.GetComponent<CanvasGroup>(), 0f, 0.85f).setEase(LeanTweenType.easeOutCubic);
		LeanTween.alphaCanvas(playAgainButton.GetComponent<CanvasGroup>(), 0f, 0.85f).setEase(LeanTweenType.easeOutCubic);


		LeanTween.scale(congratsBg, new Vector3(0f, 1f, 1f), 0.85f)
			.setDelay(0f)
			.setEase(LeanTweenType.easeOutBack);

		LeanTween.scale(popupBg, new Vector3(0f, 1f, 1f), 0.95f)
			.setDelay(0.25f)
			.setOvershoot(0.95f)
			.setEase(LeanTweenType.easeOutBack);

        // Set Progress Complete Event 
        OnProgressLoadingTransitionInComplete += ProgressLoadingTransitionInComplete;

        base.OpenLoadingPanel();
    }

    public override void Remove()
    {
        base.Remove();
    }

	
}
