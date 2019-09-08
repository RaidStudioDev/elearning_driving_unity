using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LetsRollScreen : BaseScreen
{
    private RectTransform timeTopPanel;
    private RectTransform gameTimeText;
    private Vector3 timePanelFrom;
    private Vector3 timePanelTo;

	private RectTransform superTireSmallLogo;
  	private RectTransform popupBg;
	private RectTransform bannerBg;
    private RectTransform carHolder;
    private RectTransform selectedTire;
    private RectTransform trackRecordText;
	private RectTransform readyText;
	private RectTransform selectedTrack;
    private RectTransform trackRecordCircle;
    private RectTransform letsRollButton;

    private Color elementShowColor = new Color(1f, 1f, 1f, 1f);
	private Color elementHideColor = new Color(1f, 1f, 1f, 0f);

    public override void Initialize(string id)
    {
        base.Initialize(id);

        timeTopPanel = _screenElements["TimeTopPanel"];
        gameTimeText = _screenElements["GameTimeText"];

        popupBg = _screenElements["PopupBg"];
        superTireSmallLogo = _screenElements["SuperTireSmallLogo"];
        carHolder = _screenElements["CarHolder"];
        selectedTire = _screenElements["SelectedTire"];
		trackRecordText = _screenElements ["TrackRecordText"];
		readyText = _screenElements ["ReadyText"];
        bannerBg = _screenElements["BannerBg"];
        selectedTrack = _screenElements["SelectedTrack"];
        trackRecordCircle = _screenElements["TrackRecordCircle"];
        letsRollButton = _screenElements["LetsRollButton"];

		// Hide start text and track record text
		readyText.GetComponent<Text>().color = elementHideColor;
		trackRecordText.GetComponent<Text>().color = elementHideColor;
		gameTimeText.GetComponent<Text>().color = new Color(33f / 255f, 33f / 255f, 33f / 255f, 0f);

        timePanelFrom = timeTopPanel.anchoredPosition3D + Vector3.up * timeTopPanel.sizeDelta.y;
        timePanelTo = timeTopPanel.anchoredPosition3D;
    }

    public override void Draw()
    {
       	// Setup popupBg
 		popupBg.localScale = new Vector3(0f, 1f, 1f);
		popupBg.GetComponent<Image>().color = elementShowColor;

        // Setup track record text
        trackRecordText.GetComponent<Text>().text = PersistentModel.Instance.ChallengeRecordTime;

 		bannerBg.localScale = new Vector3(0f, 1f, 1f);
		bannerBg.GetComponent<Image>().color = elementShowColor;

 		selectedTrack.localScale = new Vector3(0f, 1f, 1f);
		selectedTrack.GetComponent<Image>().color = elementShowColor;
 
		StartCoroutine(LoadChallengeImages());
    }

	IEnumerator LoadChallengeImages()
	{
		Image billboardImage = carHolder.GetComponent<Image>();
		Texture2D billboardTexture;

		string billboardName = PersistentModel.Instance.GetChallengeBillboardName();

		if (Application.platform != RuntimePlatform.WebGLPlayer)
        {
			billboardTexture = Instantiate (Resources.Load ("billboards/" + billboardName)) as Texture2D;
			billboardImage.sprite = Sprite.Create (billboardTexture, new Rect (0, 0, billboardTexture.width, billboardTexture.height), new Vector2 (.5f, .5f));
		} 
		else 
		{
            billboardTexture = new Texture2D(1801, 350, TextureFormat.RGB24, false);

            using (WWW wwwBillboard = new WWW(PersistentModel.Instance.DynamicAssetsURL + "billboards/" + billboardName + ".jpg"))
            {
                yield return wwwBillboard;
                wwwBillboard.LoadImageIntoTexture(billboardTexture);
                billboardImage.sprite = Sprite.Create(billboardTexture, new Rect(0, 0, billboardTexture.width, billboardTexture.height), new Vector2(.5f, .5f), 100.0f);
            }
		}

		billboardImage.color = elementHideColor;

		// Load up tire options

		Image image = selectedTire.GetComponent<Image>();
		Texture2D texture2D;
        string tireName  = PersistentModel.Instance.GetChallengeTireName(PersistentModel.Instance.GetChallengeOptionCorrectIndex());

		if (Application.platform != RuntimePlatform.WebGLPlayer) 
		{
			texture2D = Instantiate (Resources.Load ("tires/" + tireName)) as Texture2D;
			image.sprite = Sprite.Create (texture2D, new Rect (0, 0, texture2D.width, texture2D.height), new Vector2 (.5f, .5f));
            // yield break;
		}
		else 
		{
            texture2D = new Texture2D(331, 331, TextureFormat.ARGB32, false);

            using (WWW www = new WWW(PersistentModel.Instance.DynamicAssetsURL + "tires/" + tireName + ".png"))
            {
                yield return www;
                www.LoadImageIntoTexture(texture2D);
                image.sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(.5f, .5f), 100.0f);
            }
		}

		LeanTween.alpha(superTireSmallLogo, 1f, 0.75f)
			.setEase(LeanTweenType.easeInOutCubic)
			.setDelay(0.75f);

		LeanTween.scale(popupBg, new Vector3(1f, 1f, 1f), 0.65f)
			.setDelay(0.15f)
			.setOvershoot (0.5f)
			.setEase(LeanTweenType.easeOutBack);

		LeanTween.scale(selectedTrack, new Vector3(1f, 1f, 1f), 0.65f)
			.setDelay(0.45f)
			.setOvershoot (0.5f)
			.setEase(LeanTweenType.easeOutBack);

		LeanTween.scale(bannerBg, new Vector3(1f, 1f, 1f), 0.65f)
			.setDelay(0.35f)
			.setEase(LeanTweenType.easeOutBack)
			.setOnComplete(() => {

				LeanTween.alphaText(readyText, 1f, 0.75f).setEase(LeanTweenType.easeOutCubic).setDelay(0f);
				LeanTween.alpha(billboardImage.GetComponent<RectTransform>(), 1f, 0.75f).setEase(LeanTweenType.easeOutCubic).setDelay(0.2f);
				LeanTween.alpha(selectedTire.GetComponent<RectTransform>(), 1f, 0.75f).setEase(LeanTweenType.easeOutCubic).setDelay(0.3f);
				LeanTween.alpha(trackRecordCircle, 1f, 0.75f).setEase(LeanTweenType.easeOutCubic).setDelay(0.4f);
				LeanTween.alpha(trackRecordText, 1f, 0.75f).setEase(LeanTweenType.easeOutCubic).setDelay(0.4f);

                // Show the Time Clock
                gameTimeText.GetComponent<Text>().text = PersistentModel.Instance.FormatTime(PersistentModel.Instance.ChallengeTime);

                timeTopPanel.anchoredPosition3D = timePanelFrom;
				timeTopPanel.GetComponent<Image>().color = elementShowColor;
				LeanTween.move (timeTopPanel, timePanelTo, 0.95f)
					.setEase (LeanTweenType.easeInOutCubic)
					.setDelay (0.5f)
					.setOvershoot (0.95f)
					.setOnComplete (() => {
						// gameTimeText.GetComponent<Text>().color = elementShowColor;
						LeanTween.alphaText(gameTimeText, 1f, 0.65f).setEase(LeanTweenType.easeOutCubic).setDelay(0.1f);
						LeanTween.alphaCanvas(letsRollButton.GetComponent<CanvasGroup>(), 1f, 0.65f).setEase(LeanTweenType.easeOutCubic).setDelay(0.25f);

                        TransitionInCompleted();    // End of Transition
					});
			});
	}

    protected override void TransitionInCompleted()
    {
        // screen draw has been finished, invokes Base.TransitionInCompleteEvent if available
        base.TransitionInCompleted();

        InitButtonEvents();
    }

    private void InitButtonEvents()
    {
        letsRollButton.GetComponent<Button>().onClick.AddListener(OnLetsRollButtonClick);
    }

    private void OnLetsRollButtonClick()
    {
		UIManager.Instance.soundManager.PlaySound("PlaySineWaveHighPitch");

        if (!_isTransitionComplete) return;

        letsRollButton.GetComponent<Button>().onClick.RemoveListener(OnLetsRollButtonClick);

        selectedScreen = UIManager.Screen.GAME;
        // selectedScreen = UIManager.Screen.CONGRATULATIONS_FINAL;

        OnClickComplete += StartTransitionOut;
        ButtonClickAnimation(letsRollButton);
    }

    private void StartTransitionOut()
    {
        OnClickComplete -= StartTransitionOut;

        gameTimeText.gameObject.SetActive(false);
		readyText.GetComponent<Text>().color = elementHideColor;
		trackRecordText.GetComponent<Text>().color = elementHideColor;

		LeanTween.alpha(letsRollButton, 0f, 0.4f).setEase(LeanTweenType.easeOutQuad).setDelay(0f);
		LeanTween.alpha(superTireSmallLogo, 0f, 0.4f).setEase(LeanTweenType.easeOutQuad).setDelay(0f);
		LeanTween.alpha(timeTopPanel, 0f, 0.4f).setEase(LeanTweenType.easeOutQuad).setDelay(0.1f);
		LeanTween.alpha(trackRecordCircle, 0f, 0.4f).setEase(LeanTweenType.easeOutQuad).setDelay(0.15f);

		Vector3 _scaleIn = new Vector3 (0f, 0f, 1f);
		LeanTween.scale(carHolder, _scaleIn, 0.3f).setEase(LeanTweenType.easeOutQuad).setDelay(0.15f);
		LeanTween.scale(selectedTire, _scaleIn, 0.35f).setEase(LeanTweenType.easeOutQuad).setDelay(0.15f);
		LeanTween.scale(selectedTrack, _scaleIn, 0.4f).setEase(LeanTweenType.easeOutQuad).setDelay(0.15f);

        // Set Progress Complete Event 
        OnProgressLoadingTransitionInComplete += ProgressLoadingTransitionInComplete;

        LeanTween.scale(bannerBg, new Vector3 (0f, 1f, 1f), 0.65f).setDelay(0.35f).setEase(LeanTweenType.easeOutBack);
		LeanTween.scale(popupBg, new Vector3(0f, 1f, 1f), 0.65f)
			.setDelay(0.2f)
			.setOvershoot(0.95f)
			.setEase(LeanTweenType.easeOutBack)
			.setOnComplete(() => {
				// make time invisible
				gameTimeText.GetComponent<Text> ().color = new Color(1f, 1f, 1f, 0f);
                LeanTween.move(timeTopPanel, timePanelFrom, 0.65f)
                    .setEase(LeanTweenType.easeOutSine)
                    .setDelay(0f)
                    .setOvershoot(0.95f)
                    .setOnComplete(() =>
                    {
                        base.OpenLoadingPanel();
                    });
            });
    }

    public override void Remove()
    {
        Debug.Log("Lets Roll Remove()");

        // if (selectedTire != null) Destroy(selectedTire.GetComponent<Image>().mainTexture);

        base.Remove();
    }
}
