using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class InstructionsScreen : BaseScreen {

    private RectTransform title;
    private RectTransform popupBg;
    private RectTransform videoBg;
    private RectTransform slideImageBG;
    private RectTransform skipBtn;
    private RectTransform loadingText;

	private bool _hasSlideInitialized = false;
	private string[] _slides = { "instruction_01", "instruction_02", "instruction_03", "instruction_04", "instruction_05", "instruction_06", "instruction_07" };
	private SlideIndicator _slideIndicator;
	private Image _slideImage;
	private int _slideCurrentIndex = 0;
	private Button slideLeftBtn;
	private Button slideRightBtn;

	private Color _showElement = new Color(1, 1, 1, 1);
	private Color _hideElement = new Color(1, 1, 1, 0);
	private Vector3 _scaleShowElement = new Vector3(1, 1, 1);
	private Vector3 _scaleHideElement = new Vector3(0, 0, 1);
	private Vector3 _scaleShowReverseElement = new Vector3(-1f, 1f, 1f);

    // Title, PopupBg, VideoBg, SkipButton, SuperTireSmallLogo

    public override void Initialize(string id)
    {
        base.Initialize(id);

        title = _screenElements["Title"];
        title.GetComponent<Text>().color = _hideElement;

        popupBg = _screenElements["PopupBg"];
        skipBtn = _screenElements["SkipButton"];
		loadingText = _screenElements["LoadingText"];
		slideLeftBtn = _screenElements["SlideLeftBtn"].GetComponent<Button>();
		slideRightBtn = _screenElements["SlideRightBtn"].GetComponent<Button>();

        slideImageBG = _screenElements["SlideImageBG"];
		_slideImage = _screenElements["SlideImage"].GetComponent<Image>();
		_slideIndicator = _screenElements ["SlideIndicatorPanel"].GetComponent<SlideIndicator>();

        UpdateSkipButton();
    }

    public override void Draw()
    {
        DebugLog.Trace("PersistentModel.Draw.HasReadInstructions: " + PersistentModel.Instance.HasReadInstructions);

        slideImageBG.GetComponent<Image>().color = new Color(0, 0, 0, 0);

        popupBg.localScale = new Vector3(0f, 1f, 1f);
		popupBg.GetComponent<Image>().color = _showElement;

		_screenElements["SlideImage"].localScale = new Vector3(0f, 1f, 1f);
		_slideImage.color = _showElement;

		_screenElements["SlideLeftBtn"].localScale = new Vector3 (0, 0, 1);
		_screenElements["SlideRightBtn"].localScale = new Vector3 (0, 0, 1);

		slideLeftBtn.GetComponent<Image> ().color = _showElement;
		slideRightBtn.GetComponent<Image> ().color = _showElement;

        Vector3 titleFrom = title.anchoredPosition3D + Vector3.right * _screenElements["RightSideBg"].rect.width;
        Vector3 titleTo = title.anchoredPosition3D;
        title.anchoredPosition3D = titleFrom;
        title.GetComponent<Text>().color = new Color(1f, 1f, 1f, 1f);
        
        LeanTween.delayedCall(0.00f, () => { UIManager.Instance.soundManager.PlaySound("PlayNoiseMidHighTone"); });
        LeanTween.move(title, titleTo, 0.75f)
            .setEase(LeanTweenType.easeInOutBack)
            .setDelay(0f)
            .setOvershoot(0.95f)
            .setOnComplete(() => {

                LeanTween.alpha(slideImageBG, 1.0f, 0.95f)
                    .setDelay(0.1f).setUseEstimatedTime(slideImageBG);
            });

		LeanTween.scale(_screenElements["SlideImage"], _scaleShowElement, 0.75f).setDelay(0.55f).setEase(LeanTweenType.easeOutBack);
		_screenElements["SlideLeftBtn"].gameObject.SetActive(false);
		LeanTween.scale(_screenElements["SlideRightBtn"], _scaleShowElement, 0.8f).setDelay(0.95f).setEase(LeanTweenType.easeInOutBack);
        
        LeanTween.delayedCall(0.75f, () => { UIManager.Instance.soundManager.PlaySound("PlaySawLowHighTone"); });
        LeanTween.scale(popupBg, new Vector3(1f, 1f, 1f), 0.85f)
			.setDelay(0.75f)
			.setEase(LeanTweenType.easeOutBack)
			.setOnComplete(() => {

                InitializeSlideIndicator();

                LeanTween.alphaCanvas(skipBtn.GetComponent<CanvasGroup>(), 1f, 0.75f)
					.setEase(LeanTweenType.easeOutQuad)
					.setDelay(0.1f)
					.setOnComplete(() => {

                        TransitionInCompleted();    // End of Transition
                    });
			});

		LeanTween.alphaCanvas (_slideIndicator.GetComponent<CanvasGroup> (), 1, 0.5f).setEaseOutQuad();
    }

    protected override void TransitionInCompleted()
    {
        // screen draw has been finished, invokes Base.TransitionInCompleteEvent if available
        base.TransitionInCompleted();

        PersistentModel.Instance.HasReadInstructions = true;

        if (isGamePadEnabled)
        {
            skipBtn.GetComponent<Button>().Select();
        }

        InitButtonEvents();
    }

    void InitializeSlideIndicator()
	{
		List<int> slideIndexes = new List<int>();

		for (int i = 0; i < _slides.Length; i++) slideIndexes.Add(i);

		_slideIndicator.OnClick += OnSlideIndicatorClick;
		_slideIndicator.DataProvider = slideIndexes;
		_slideIndicator.Initialize(this);
	}

	void OnSlideIndicatorClick(int index)
	{
		_slideCurrentIndex = index;

		// Left Slider Button
		if (_slideCurrentIndex == 0) 
		{
			// hide left button
			LeanTween.scale(_screenElements["SlideLeftBtn"], _scaleHideElement, 0.65f)
				.setEaseInOutBack()
				.setOnComplete(() => {
					slideLeftBtn.gameObject.SetActive(false);
				});
		}

		if (_slideCurrentIndex > 0) 
		{
			if (!_screenElements["SlideLeftBtn"].gameObject.activeSelf) 
			{
				// show right button
				slideLeftBtn.gameObject.SetActive(true);
				LeanTween.scale(_screenElements["SlideLeftBtn"], _scaleShowReverseElement, 0.5f)
					.setEaseInOutBack()
					.setOnComplete(() => {

					});
			}
		}

		// Right Slider Button
		if (_slideCurrentIndex < _slides.Length - 1) 
		{
			if (!_screenElements["SlideRightBtn"].gameObject.activeSelf) 
			{
				// show right button
				slideRightBtn.gameObject.SetActive(true);
				LeanTween.scale(_screenElements["SlideRightBtn"], _scaleShowElement, 0.5f)
					.setEaseInOutBack()
					.setOnComplete(() => {
						
					});
			}
		}

		if (_slideCurrentIndex == _slides.Length - 1) 
		{
			// show left button
			LeanTween.scale(_screenElements["SlideRightBtn"], _scaleHideElement, 0.5f)
				.setEaseInOutBack()
				.setOnComplete(() => {
					slideRightBtn.gameObject.SetActive(false);
				});
		}

		// load and show slide image
		StartCoroutine(StartSlideImageLoad());
	}

	private void UpdateSkipButton()
	{
        // check if coming from Welcome Back
        if (UIManager.Instance.PreviousScreenID == UIManager.Instance.GetScreenID(UIManager.Screen.WELCOME_BACK))
        {
            skipBtn.GetComponentInChildren<Text>().text = "RETURN";

            return;
        }

        if (_slideCurrentIndex == _slides.Length - 1) 
		{
			skipBtn.GetComponentInChildren<Text> ().text = "START";
		}
		else skipBtn.GetComponentInChildren<Text> ().text = "SKIP";
	}

	IEnumerator StartSlideImageLoad()
	{
		UpdateSkipButton();

		LeanTween.cancel(_slideImage.GetComponent<RectTransform>());

		_slideImage.color = _hideElement;

		ShowLoader();

		string slideName = _slides[_slideCurrentIndex];
		Texture2D texture2D;

		_slideImage.color = _hideElement;

		if (Application.platform != RuntimePlatform.WebGLPlayer)
		{
			texture2D = Instantiate(Resources.Load("instructions/" + slideName)) as Texture2D;
			_slideImage.sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(.5f, .5f));

			StartCoroutine(ShowSlideImage());
			yield break;
		}

		WWW www = new WWW(PersistentModel.Instance.DynamicAssetsURL + "instructions/" + slideName + ".jpg");

		yield return www;

		texture2D = www.texture;
		_slideImage.sprite =  Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(.5f, .5f));

		StartCoroutine(ShowSlideImage());
	}

	IEnumerator ShowSlideImage()
	{
		yield return new WaitForEndOfFrame();

		HideLoader();

		LeanTween.alpha(_slideImage.GetComponent<RectTransform>(), 1f, 0.75f).setDelay(0.25f).setEaseOutCubic().setOnComplete(() => {

			ShowSlideNavigation();

			slideLeftBtn.interactable = slideRightBtn.interactable = true;
		});
	}

	void ShowSlideNavigation()
	{
		if (!_hasSlideInitialized) 
		{
			_hasSlideInitialized = true;

			LeanTween.scale (_screenElements["SlideRightBtn"], _scaleShowElement, 0.65f)
			.setEaseInOutBack ()
			.setOnComplete (() => {
				
			});

		}
	}

	// private StreamVideo _video;
	private void PrepareVideo()
	{
		// _video = videoBg.GetComponentInChildren<StreamVideo>();
		// _video.StartVideo("https://www.lms.mybridgestoneeducation.com/Switchback/AssetVideos/big_buck_bunny.mp4");
	}

    private void InitButtonEvents()
    {
        skipBtn.gameObject.GetComponent<Button>().onClick.AddListener(OnSkipButtonClick);

		slideLeftBtn.onClick.AddListener(OnSlideLeftClick);
		slideRightBtn.onClick.AddListener(OnSlideRightClick);
    }

	private void OnSlideLeftClick()
	{
		UIManager.Instance.soundManager.PlaySound("PlaySineWaveHighPitch");

        slideLeftBtn.interactable = false;

		if (_slideCurrentIndex > 0) 
		{
			_slideCurrentIndex--;
			if (_slideCurrentIndex == 0) 
			{
				// hide left button
				LeanTween.scale(_screenElements["SlideLeftBtn"], _scaleHideElement, 0.65f)
					.setEaseInOutBack()
					.setOnComplete(() => {
						slideLeftBtn.gameObject.SetActive(false);
					});
			}

			if (_slideCurrentIndex < _slides.Length && !slideRightBtn.gameObject.activeSelf) 
			{
				// show right button
				slideRightBtn.gameObject.SetActive(true);
				LeanTween.scale(_screenElements["SlideRightBtn"], _scaleShowElement, 0.5f)
					.setEaseInOutBack()
					.setOnComplete(() => {
						
					});
			}

			_slideIndicator.PrevItem(_slideCurrentIndex);

			// load and show slide image
			StartCoroutine(StartSlideImageLoad());
		}
	}

	private void OnSlideRightClick()
	{
		UIManager.Instance.soundManager.PlaySound("PlaySineWaveHighPitch");

        slideRightBtn.interactable = false;

		if (_slideCurrentIndex < _slides.Length - 1) 
		{
			_slideCurrentIndex++;
			if (_slideCurrentIndex == _slides.Length - 1) 
			{
				// hide left button
				LeanTween.scale(_screenElements["SlideRightBtn"], _scaleHideElement, 0.65f)
					.setEaseInOutBack()
					.setOnComplete(() => {
						slideRightBtn.gameObject.SetActive(false);
					});
			}

			if (_slideCurrentIndex > 0 && !slideLeftBtn.gameObject.activeSelf) 
			{
				// show left button
				slideLeftBtn.gameObject.SetActive(true);
				LeanTween.scale(_screenElements["SlideLeftBtn"], _scaleShowReverseElement, 0.5f)
					.setEaseInOutBack()
					.setOnComplete(() => {
						
					});

			}
		
			_slideIndicator.NextItem(_slideCurrentIndex);

			// load and show slide image
			StartCoroutine(StartSlideImageLoad());
		}
	}

    protected override void OnGamePadDPadLeftButton()
    {
        base.OnGamePadDPadLeftButton();

        OnSlideLeftClick();
    }

    protected override void OnGamePadDPadRightButton()
    {
        base.OnGamePadDPadRightButton();

        OnSlideRightClick();
    }

    private void OnSkipButtonClick()
    {
		UIManager.Instance.soundManager.PlaySound("PlaySineWaveHighPitch");
        if (!_isTransitionComplete) return;

        skipBtn.gameObject.GetComponent<CanvasGroup>().interactable = false;
        skipBtn.gameObject.GetComponent<CanvasGroup>().blocksRaycasts = false;
        skipBtn.gameObject.GetComponent<Image>().raycastTarget = false;
        skipBtn.gameObject.GetComponent<Button>().onClick.RemoveListener(OnSkipButtonClick);

        // check if coming from Welcome Back
        if (UIManager.Instance.PreviousScreenID == UIManager.Instance.GetScreenID(UIManager.Screen.WELCOME_BACK))
        {
            selectedScreen = UIManager.Screen.WELCOME_BACK;
        }
        else
        {
            selectedScreen = UIManager.Screen.QUIZ_SCREEN;
        }

        OnClickComplete += StartTransitionOut;
        ButtonClickAnimation(skipBtn);
    }

    private void StartTransitionOut()
    {
        OnClickComplete -= StartTransitionOut;

        LeanTween.alpha(slideImageBG, 0.0f, 0.5f)
                    .setDelay(0.0f).setUseEstimatedTime(slideImageBG);

        LeanTween.alphaCanvas (_slideIndicator.GetComponent<CanvasGroup> (), 0, 0.5f).setEaseOutQuad();

		if (_screenElements["SlideLeftBtn"].gameObject.activeSelf)  
			LeanTween.scale(_screenElements["SlideLeftBtn"], _scaleHideElement, 0.5f).setDelay(0.1f).setEase(LeanTweenType.easeInOutBack);

		if (_screenElements["SlideRightBtn"].gameObject.activeSelf)  
			LeanTween.scale(_screenElements["SlideRightBtn"], _scaleHideElement, 0.5f).setDelay(0.1f).setEase(LeanTweenType.easeInOutBack);
        
        LeanTween.delayedCall(0.0f, () => { UIManager.Instance.soundManager.PlaySound("PlaySawLowHighTone"); });

        LeanTween.scale(title, new Vector3(0f, 0f, 0.1f), 0.95f)
        .setEase(LeanTweenType.easeInOutBack)
        .setOvershoot(0.95f);

        LeanTween.value(1f, 0f, 0.95f)
        .setDelay(0.15f)
        .setEase(LeanTweenType.easeOutQuad)
        .setOnUpdate((float val) =>
        {
            CanvasGroup cg = skipBtn.GetComponent<CanvasGroup>();
            cg.alpha = val;
        });

		LeanTween.scale(_screenElements["SlideImage"], new Vector3(0f, 1f, 1f), 0.85f)
        .setEase(LeanTweenType.easeInOutBack)
        .setDelay(0.25f)
        .setOvershoot(0.95f);
        
        LeanTween.delayedCall(0.35f, () => { UIManager.Instance.soundManager.PlaySound("PlaySawLowHighTone"); });

        LeanTween.scale(popupBg, new Vector3(0f, 1f, 1f), 0.95f)
        .setDelay(0.35f)
        .setOvershoot(0.95f)
        .setEase(LeanTweenType.easeOutBack);

        // Set Progress Complete Event 
        OnProgressLoadingTransitionInComplete += ProgressLoadingTransitionInComplete;

        base.OpenLoadingPanel();
    }

    public override void Remove()
    {
        slideLeftBtn.onClick.RemoveListener(OnSlideLeftClick);
        slideRightBtn.onClick.RemoveListener(OnSlideRightClick);

        _slideIndicator.Remove();

		LeanTween.cancel(title);
        LeanTween.cancel(popupBg);
        LeanTween.cancel(skipBtn);
    
		base.Remove();
    }

	void ShowLoader()
	{
		loadingText.gameObject.SetActive(true);
		loadingText.GetComponent<Text>().color = _hideElement;

		LeanTween.alphaText(loadingText.GetComponent<RectTransform>(), 1f, 0.5f).setEaseInBack().setLoopPingPong();
	}

	void HideLoader()
	{
		LeanTween.cancel(loadingText.GetComponent<RectTransform>());

		loadingText.GetComponent<Text>().color = _hideElement;
		loadingText.gameObject.SetActive(false);
	}
}
