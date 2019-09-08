using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public delegate void OnLoadingClosedEventHandler();
public delegate void OnProgressLoadingTransitionCompleteEventHandler();
public delegate void OnClickAnimationCompleteEventHandler();
public delegate void OnTransitionInCompleteEventHandler();

public class BaseScreen : UIScreen {

    public event OnLoadingClosedEventHandler OnCloseLoadingPanelComplete;
    public event OnProgressLoadingTransitionCompleteEventHandler OnProgressLoadingTransitionInComplete;
    public event OnClickAnimationCompleteEventHandler OnClickComplete;
    public event OnTransitionInCompleteEventHandler OnTransitionInComplete;

    protected UIManager _ui;

    public string screenId { get; private set; }
    protected Dictionary<string, RectTransform> _screenElements;

    public bool isLoadingRequiredBeforeDraw { get; set; }

    protected bool _isTransitionComplete = false;
    protected bool _isTransitioning = false;
    protected bool _isTransitioningSlidePanels = true;
    protected bool _fadeInPanels = true;
    protected bool _isGameScreenOverlay = false;
    protected UIManager.Screen selectedScreen;

    public delegate void LoadedEventHandler(BaseScreen baseScreen);
    public event LoadedEventHandler OnLoaded;
    protected void DispatchOnLoaded() { OnLoaded(this);  }

    [HideInInspector]
	public bool showProgressLoadingPanel = false;
    [HideInInspector]
    public bool showSmallProgressLoadingPanel = true;
    [HideInInspector]
    public bool isGamePadEnabled = false;
    public bool hideProgressPanel = false;
    protected bool isJoysticksFound = false;
    protected bool isJoysticksButtonPressed = false;

    protected Vector3 titleScale = new Vector3(1.3f, 1.3f, 1f);
    protected Color elementYellowColor = new Color(1f, 0.9118239f, 0f, 1f);

    protected RectTransform muteButton;

    private Button[] _buttons;
  
    void Awake()
    {
        isJoysticksFound = (Input.GetJoystickNames().Length > 0);

        _ui = UIManager.Instance;

        PreInitialize();
    }

    public override void PreInitialize()
    {
        _screenElements = new Dictionary<string, RectTransform>();

        // get all buttons in screen
        _buttons = this.GetComponentsInChildren<Button>();
        
        // Did this because get error "null not set to reference" otherwise during debugging
        bool automaticScreenAlpha = false;
		if (UIManager.Instance) {
			automaticScreenAlpha = UIManager.Instance.AutomaticScreenAlpha;
			// if (!PersistentModel.Instance.DEBUG) automaticScreenAlpha = true;
		}

        // Grab all the screen elements and set the opacity to clear, we will clear when Remove() is called.
        // The actual screens will update them accordingly
        RectTransform[] elementList = this.GetComponentsInChildren<RectTransform>();
        foreach (RectTransform element in elementList)
        {
 			bool doNotAdd = false;

            if (element.GetComponent<BaseScreen>() == null && !_isGameScreenOverlay)
            {
                // raw panels ignore alpha change
                if (element.GetComponent<RawPanel>()) 
				{
                    // do nothing
                }
                else if (element.GetComponent<CanvasGroup> ()) 
				{
					if (automaticScreenAlpha) element.GetComponent<CanvasGroup> ().alpha = 0f;
				} 
				else if (element.GetComponent<Image> ()) 
				{
					if (automaticScreenAlpha) element.GetComponent<Image> ().color = new Color (1f, 1f, 1f, 0f);
				} 
				else if (element.GetComponent<GameText> ()) 
				{
					if (automaticScreenAlpha) element.GetComponent<Text> ().color = new Color (1f, 1f, 1f, 0f);
				} 
				else if (element.GetComponent<Text>())
                {
                    // do nothing
                }
            }
            else
            {
	              doNotAdd = (_isGameScreenOverlay) ? false : true;
            }

			if (!doNotAdd && !_screenElements.ContainsKey (element.gameObject.name)) _screenElements.Add (element.gameObject.name, element);
		}

        // set event button triggers
        for (int i = 0; i < _buttons.Length; i++)
        {
            AddButtonEventTrigger(_buttons[i]);
        }
    }

    public void AddButtonEventTrigger(Button button)
    {
        EventTrigger buttonTrigger = button.gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry pointerEnter = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerEnter
        };
        pointerEnter.callback.AddListener((e) => OnPointerEnter());
        buttonTrigger.triggers.Add(pointerEnter);

        EventTrigger.Entry pointerExit = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerExit
        };
        pointerExit.callback.AddListener((e) => OnPointerExit());
        buttonTrigger.triggers.Add(pointerExit);
    }

    private void RemoveButtonEventTrigger(Button button)
    {
        EventTrigger buttonTrigger = button.gameObject.GetComponent<EventTrigger>();
       
        for (int i = 0; i < buttonTrigger.triggers.Count; i++)
        {
            EventTrigger.Entry pointerEntry = buttonTrigger.triggers[i];
            pointerEntry.callback.RemoveAllListeners();
            buttonTrigger.triggers.Remove(pointerEntry);
        }
    }

    protected void OnPointerEnter()
    {
        _ui.UpdateCursor(true);
    }

    protected void OnPointerExit()
    {
        _ui.UpdateCursor(false);
    }

    public override void Initialize(string id)
    {
        this.screenId = id;

        // Setup Side Panels
        if (!_isGameScreenOverlay) ResetSidePanelSize();

        // Fade In Side Panels
        if (_isTransitioningSlidePanels && !_isGameScreenOverlay) TransitionInSidePanels();

        // check for mute button
        if (_screenElements.ContainsKey("MuteButton"))
        {
            muteButton = _screenElements["MuteButton"];
            muteButton.localScale = new Vector3(0f, 0f, 1f);
            muteButton.GetComponent<CanvasGroup>().alpha = 1f;
            Button muteOffBtn = muteButton.GetComponent<MuteButton>().muteButtonOff.GetComponent<Button>();
            Button muteOnBtn = muteButton.GetComponent<MuteButton>().muteButtonOn.GetComponent<Button>();
            AddButtonEventTrigger(muteOffBtn);
            AddButtonEventTrigger(muteOnBtn);
        }
    }

    public override void Load()
    {

    }

    public override void Draw()
    {
        
    }

    virtual protected void TransitionInCompleted()
    {
        EnableGamePad();

        _isTransitionComplete = true;

        OnTransitionInComplete?.Invoke();

        // check for mute button
        if (_screenElements.ContainsKey("MuteButton"))
        {
            // LeanTween.alphaCanvas(muteButton.GetComponent<CanvasGroup>(), 1f, 0.75f);
            LeanTween.scale(muteButton, new Vector3(1f, 1f, 1f), 0.55f).setDelay(0.65f)
            .setOvershoot(0.75f)
            .setIgnoreTimeScale(true)
            .setEase(LeanTweenType.easeOutBack)
            .setOnComplete(() => {

            });
        }
    }

    public override void Remove()
    {
        // remove button listeners
        if (_buttons != null)
        {
            for (int i = 0; i < _buttons.Length; i++)
            {
                RemoveButtonEventTrigger(_buttons[i]);
            }
            _buttons = null;
        }
        
        // check for mute button
        if (_screenElements.ContainsKey("MuteButton"))
        {
            Button muteOffBtn = muteButton.GetComponent<MuteButton>().muteButtonOff.GetComponent<Button>();
            Button muteOnBtn = muteButton.GetComponent<MuteButton>().muteButtonOn.GetComponent<Button>();
            RemoveButtonEventTrigger(muteOffBtn);
            RemoveButtonEventTrigger(muteOnBtn);

            muteButton.GetComponent<MuteButton>().Remove();
        }

        _screenElements.Clear();

        _ui = null;
    }

    public void ResetSidePanelSize()
    {
        float updatedScale;

        // When we are running on bigger screens
        if (UIManager.ScaleFactor < 1f)
        {
            // Reduce ScaleFactor to about 60% more than the original 
            updatedScale = 1 + (UIManager.ScaleFactor * .6f);
        }
        else // Running on iPad 1x or Retina, scale just enough to prevent image edge exposure
        {
            // Reduce ScaleFactor to about 10% more than of the original 
            updatedScale = 1 + (UIManager.ScaleFactor * .1f);
        }

        // Resize Side Panels
        _screenElements["RightSideBg"].SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _screenElements["RightSideBg"].rect.width * updatedScale);
        _screenElements["RightSideBg"].SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _screenElements["RightSideBg"].rect.height * updatedScale);

        _screenElements["LeftSideBg"].SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _screenElements["LeftSideBg"].rect.width * updatedScale);
        _screenElements["LeftSideBg"].SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _screenElements["LeftSideBg"].rect.height * updatedScale);

        // save original scale
        slidePanelScaleFrom = _screenElements["LeftSideBg"].localScale * slidePanelScalePercent;
        slidePanelScaleTo = _screenElements["LeftSideBg"].localScale;
    }

    public void ScaleInPanels()
    {
        // float delay = 0.5f; && last 2 is at 0.3f

        RectTransform leftSidePanel = _screenElements["LeftSideBg"];
        RectTransform rightSidePanel = _screenElements["RightSideBg"];

        LeanTween.color(rightSidePanel, new Color(1, 1, 1, 1), 2f)
            .setFromColor(new Color(0.1294118f, 0.1294118f, 0.1294118f, 0))
            .setEase(LeanTweenType.easeInOutQuad)
            .setDelay(0.2f);

        LeanTween.color(leftSidePanel, new Color(1, 1, 1, 1), 2f)
            .setFromColor(new Color(0.1294118f, 0.1294118f, 0.1294118f, 0))
            .setEase(LeanTweenType.easeInOutQuad)
            .setDelay(0.2f);

        LeanTween.scale(rightSidePanel.gameObject, slidePanelScaleTo, 1f)
            .setFrom(slidePanelScaleFrom)
            .setEase(LeanTweenType.easeOutQuad)
            .setDelay(0.0f);
        LeanTween.scale(leftSidePanel.gameObject, slidePanelScaleTo, 1f)
            .setFrom(slidePanelScaleFrom)
            .setEase(LeanTweenType.easeOutQuad)
            .setDelay(0.0f);
    }

    private Vector3 slidePanelScaleFrom;
    private Vector3 slidePanelScaleTo;
    private readonly float slidePanelScalePercent = 0.90f;
    private readonly Color slidePanelColorFrom = new Color(0.1294118f, 0.1294118f, 0.1294118f, 1);
    public void TransitionInSidePanels()
    {
        /* Set Start Positions */
        RectTransform leftSidePanel = _screenElements["LeftSideBg"];
        RectTransform rightSidePanel = _screenElements["RightSideBg"];

        if (_fadeInPanels)
        {
            rightSidePanel.GetComponent<Image>().color = slidePanelColorFrom;
            leftSidePanel.GetComponent<Image>().color = slidePanelColorFrom;

            rightSidePanel.localScale = slidePanelScaleFrom;
            leftSidePanel.localScale = slidePanelScaleFrom;
        }
        else // Slide In Panels
        {
            LeanTween.alpha(leftSidePanel, 1f, 0f).setEase(LeanTweenType.easeOutQuad).setDelay(0f);
            LeanTween.alpha(rightSidePanel, 1f, 0f).setEase(LeanTweenType.easeOutQuad).setDelay(0f);
            
            LeanTween.delayedCall(0.25f, () => { UIManager.Instance.soundManager.PlaySound("PlaySawLowHighTone"); });

            // Set Start Positions
            Vector3 rightSideFrom = rightSidePanel.anchoredPosition3D + Vector3.right * rightSidePanel.rect.width;
            Vector3 rightSideTo = rightSidePanel.anchoredPosition3D + Vector3.left * 0;
            rightSidePanel.anchoredPosition3D = rightSideFrom;

            Vector3 leftSideFrom = leftSidePanel.anchoredPosition3D + Vector3.left * leftSidePanel.rect.width;
            Vector3 leftSideTo = leftSidePanel.anchoredPosition3D + Vector3.right * 0;
            leftSidePanel.anchoredPosition3D = leftSideFrom;

            // Animate
            LeanTween.move(rightSidePanel, rightSideTo, 0.5f)
            .setEase(LeanTweenType.easeInOutCubic)
            .setDelay(0.25f);

            LeanTween.move(leftSidePanel, leftSideTo, 0.5f)
            .setEase(LeanTweenType.easeInOutCubic)
            .setDelay(0.25f);
        }
    }

    public override void OpenLoadingPanel()
    {
       
        LeanTween.delayedCall(0.15f, () => { UIManager.Instance.soundManager.PlaySound("PlayNoiseMidHighTone"); });

        // check for mute button
        if (_screenElements.ContainsKey("MuteButton"))
        {
            LeanTween.alphaCanvas(muteButton.GetComponent<CanvasGroup>(), 0f, 0.5f).setEase(LeanTweenType.easeOutQuad);
        }

        if (isLoadingRequiredBeforeDraw) return;
     
        if (showSmallProgressLoadingPanel)
        {
            ShowSmallLoader();
            return;
        }

        if (showProgressLoadingPanel)
        {
            ShowProgressLoaderPanel();
            return;
        }
    }

    private void ShowSmallLoader()
    {
        GameObject smallLoader = UIManager.Instance.SmallProgressLoader;
        RectTransform spinningTire = smallLoader.transform.Find("SpinningTire").GetComponent<RectTransform>();
        spinningTire.localScale = Vector3.zero;
        spinningTire.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);

        smallLoader.transform.SetParent(transform, false);
        smallLoader.transform.SetAsLastSibling();

        LeanTween.rotateAroundLocal(spinningTire, Vector3.back, 360f, 1f).setRepeat(-1);
        LeanTween.scale(spinningTire, new Vector3(1f, 1f, 1f), 0.85f).setEase(LeanTweenType.easeInOutQuad);

        OnProgressLoadingTransitionInComplete?.Invoke();
    }

    public void ShowProgressLoaderPanel(bool showProgressLabel = true)
    {
        bool showLoadingBar = true;

        GameObject progressLoadingPanel = UIManager.Instance.ProgressLoadingPanel;

        //progressLoadingPanel.GetComponent<ProgressLoadingPanel>().LoadingPercent.text = (showProgressLabel) ? "0%" : "TRACK";
        progressLoadingPanel.GetComponent<ProgressLoadingPanel>().LoadingPercent.text = "0%";

        RectTransform loadingBar = progressLoadingPanel.transform.Find("LoadingBar").GetComponent<RectTransform>();
        RectTransform spinningTire = progressLoadingPanel.transform.Find("SpinningTire").GetComponent<RectTransform>();

        spinningTire.localScale = Vector3.zero;
        spinningTire.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);

        loadingBar.localScale = Vector3.zero;
        Vector3 loadingStartRotation = loadingBar.transform.eulerAngles;
        loadingStartRotation.z = 143f;
        loadingBar.transform.eulerAngles = loadingStartRotation;

        if (showLoadingBar) loadingBar.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
        else loadingBar.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);

        progressLoadingPanel.transform.SetParent(transform, false);
        progressLoadingPanel.transform.SetAsLastSibling();

        // Animate In
        LeanTween.rotateLocal(loadingBar.gameObject, new Vector3(1f, 1f, 0f), 0.95f).setEase(LeanTweenType.easeOutBack);
        LeanTween.scale(loadingBar, new Vector3(1f, 1f, 1f), 0.85f).setEase(LeanTweenType.easeOutBack).setOvershoot(1.25f);

        LeanTween.rotateAroundLocal(spinningTire, Vector3.back, 360f, 1f).setRepeat(-1);
        LeanTween.scale(spinningTire, new Vector3(1f, 1f, 1f), 0.85f).setEase(LeanTweenType.easeInOutQuad);

        // Trigger Event ProgressLoading Complete
        OnProgressLoadingTransitionInComplete?.Invoke();
    }

    virtual protected void ProgressLoadingTransitionInComplete()
    {
        OnProgressLoadingTransitionInComplete -= ProgressLoadingTransitionInComplete;

        UIManager.Instance.ShowScreen(selectedScreen);
    }

    // Reset Progress Panel's Parent Transform
    protected void ResetProgressLoadingPanel()
    {
        GameObject progressLoadingPanel = null;

        if (showSmallProgressLoadingPanel) progressLoadingPanel = UIManager.Instance.SmallProgressLoader;
        if (showProgressLoadingPanel) progressLoadingPanel = UIManager.Instance.ProgressLoadingPanel;

        if (progressLoadingPanel)
        {
            progressLoadingPanel.transform.SetParent(null, false);
            progressLoadingPanel.transform.SetAsLastSibling();

            // clear tween on spinner
            LeanTween.cancel(progressLoadingPanel.transform.Find("SpinningTire").GetComponent<RectTransform>());
        }
    }  

    public override void CloseLoadingPanel()
    {
        HideProgressLoaderPanel();
    }

    private void HideProgressLoaderPanel()
    {
        GameObject progressLoadingPanel = null;

        if (showSmallProgressLoadingPanel)
        {
            progressLoadingPanel = UIManager.Instance.SmallProgressLoader;
        }

        if (showProgressLoadingPanel)
        {
            progressLoadingPanel = UIManager.Instance.ProgressLoadingPanel;
        }

        RectTransform spinningTire = progressLoadingPanel.transform.Find("SpinningTire").GetComponent<RectTransform>();

        // check if we have a loading bar and transition out - ProgressLoadingPanel
        if (progressLoadingPanel.transform.Find("LoadingBar"))
        {
            RectTransform loadingBar = progressLoadingPanel.transform.Find("LoadingBar").GetComponent<RectTransform>();

            LeanTween.rotateLocal(loadingBar.gameObject, new Vector3(1f, 1f, 146f), 0.95f)
            .setEase(LeanTweenType.easeInBack)
            .setDelay(0.1f);

            LeanTween.scale(loadingBar, new Vector3(0f, 0f, 0f), 0.85f)
                .setEase(LeanTweenType.easeInBack)
                .setOvershoot(1.25f)
                .setDelay(0.1f);
        }
        

        // play transition sounds
        LeanTween.delayedCall(0.2f, () => { UIManager.Instance.soundManager.PlaySound("PlaySawLowMidHighTone"); });
        LeanTween.delayedCall(0.25f, () => { UIManager.Instance.soundManager.PlaySound("PlaySawLowHighTone"); });

        // slide in panels
        if (_screenElements["RightSideBg"] != null)
        {
            // Set Side Panel To Positions
            Vector3 rightSideTo = _screenElements["RightSideBg"].anchoredPosition3D + Vector3.right * _screenElements["RightSideBg"].rect.width;
            Vector3 leftSideTo = _screenElements["LeftSideBg"].anchoredPosition3D + Vector3.left * _screenElements["LeftSideBg"].rect.width;

            // Animate Side Panels
            LeanTween.move(_screenElements["RightSideBg"], rightSideTo, 1.5f)
                .setEase(LeanTweenType.easeInOutCubic)
                .setDelay(0.25f);

            LeanTween.move(_screenElements["LeftSideBg"], leftSideTo, 1.5f)
                .setEase(LeanTweenType.easeInOutCubic)
                .setDelay(0.25f);

        }

        // Scale Out Spinning Tire and Close Up This Screen on Completion
        LeanTween.scale(spinningTire, Vector3.zero, 0.95f)
            .setDelay(0.75f)
            .setEase(LeanTweenType.easeInBack)
            .setOvershoot(1.5f)
            .setOnComplete(() => {

                ResetProgressLoadingPanel();

                // triggers closed event from base class
                // the event is declared in the UIScreenManager.LoadScreen()
                CloseLoadingPanelComplete();
            }); 
    }

    public override void CloseLoadingPanelComplete()
    {
        OnCloseLoadingPanelComplete();
    }

    public void ClearSidePanelTweens()
    {
        LeanTween.cancel(_screenElements["RightSideBg"].gameObject);
        LeanTween.cancel(_screenElements["LeftSideBg"].gameObject);
    }

    public void ButtonClickAnimation(RectTransform uiButton)
    {
        LeanTween.cancel(uiButton.gameObject);
        uiButton.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);

        LeanTween.color(uiButton, Color.gray, 0.15f)
        .setEaseInOutCubic()
        .setLoopCount(4)
        .setLoopPingPong()
        .setOnComplete(() => {

            LeanTween.alpha(uiButton, 0f, 0.75f)
            .setEase(LeanTweenType.easeOutSine);

            if (OnClickComplete != null) OnClickComplete();
        });
    }

    public void FlickerAnimation(RectTransform uiButton,
        float speed = 0.15f,
        int loopCount = 4,
        bool fadeOut = true)
    {
        LeanTween.cancel(uiButton.gameObject);
        uiButton.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);

        LeanTween.color(uiButton, Color.gray, speed)
        .setEaseInOutCubic()
        .setLoopCount(loopCount)
        .setLoopPingPong()
        .setOnComplete(() => {

            if (fadeOut) LeanTween.alpha(uiButton, 0f, 0.75f).setEase(LeanTweenType.easeOutSine);
        
            if (OnClickComplete != null) OnClickComplete();
        });
    }

    protected void ClearButtonFocus()
    {
        EventSystem.current.SetSelectedGameObject(null);
    }

    protected delegate void OnShakePanelComplete();
    protected void ShakePanel(object parameters)
    {
        // Parameters: RectTransform panel, Vector3 direction, float amount
        object[] parameterList = parameters as object[];

        RectTransform panel = (RectTransform)parameterList[0];
        Vector3 direction = (Vector3)parameterList[1];
        float amount = (float)parameterList[2];

        // Check if we have a callback, sometimes this will be needed
        OnShakePanelComplete shakePanelComplete = null;
        if (parameterList.Length > 3) shakePanelComplete = (OnShakePanelComplete)parameterList[3];

        // Setup Shake Properties
        float forceJump = 9.5f;
        float height = Mathf.PerlinNoise(forceJump, 0f) * amount;
        height = height * height * 0.3f;
        float shakeAmt = height * 0.2f; // the degrees to shake the camera
        float shakePeriodTime = 0.32f; // The period of each shake
        float dropOffTime = 1.25f; // How long it takes the shaking to settle down to nothing
        LTDescr shakeTween = LeanTween.moveLocal(panel.gameObject, direction * 1.5f, shakePeriodTime)
        .setEase(LeanTweenType.easeShake) // this is a special ease that is good for shaking
        .setLoopClamp()
        .setRepeat(-1);

        // Slow the camera shake down to zero
        LeanTween.value(panel.gameObject, shakeAmt, 0f, dropOffTime).setOnUpdate(
            (float val) => {
                shakeTween.setTo(direction * val);
            }
        ).setEase(LeanTweenType.easeOutQuad).setOnComplete(() => {

            if (shakePanelComplete != null) shakePanelComplete();

        });
    }


    // GAMEPAD //////////////////////////////////////////////////////////////////////////////////////

    protected void EnableGamePad()
    {
        //Debug.Log("EnableGamePad.isJoysticksFound: " + isJoysticksFound);

        isGamePadEnabled = (isJoysticksFound);
    }

    protected void DisableGamePad()
    {
        //Debug.Log("DisableGamePad.isJoysticksFound: " + isJoysticksFound);

        isGamePadEnabled = false;
    }

    private bool isGamePadButtonPressed = false;

    virtual protected void Update()
    {
        float now = Time.realtimeSinceStartup;

        if (isGamePadEnabled && isJoysticksFound)
        {
            // get gamepad button 0 and d-pad
            bool joyBtn0 = Input.GetKey(KeyCode.Joystick1Button0);
            float dPadHoriz = Input.GetAxis("DPadHorizontal");
            float dPadVert = Input.GetAxis("DPadVertical");

            // reset if user not moving
            if (Mathf.Abs(dPadVert) < 0.1f && Mathf.Abs(dPadHoriz) < 0.1f)
                isGamePadButtonPressed = false;

            if (!isGamePadButtonPressed)
            {
                if (dPadHoriz >= 1)
                {
                    OnGamePadDPadRightButton();
                }
                else if (dPadHoriz <= -1)
                {
                    OnGamePadDPadLeftButton();
                }
                else if (dPadVert >= 1)
                {
                    OnGamePadDPadUpButton();
                }
                else if (dPadVert <= -1)
                {
                    OnGamePadDPadDownButton();
                }

                if (joyBtn0)
                {
                    OnGamePadButton01();
                }
            }
        }

        #if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }
        #endif

    }

    virtual protected void OnGamePadButton01()
    {
        isGamePadButtonPressed = true;
    }

    virtual protected void OnGamePadDPadLeftButton()
    {
        isGamePadButtonPressed = true;
    }

    virtual protected void OnGamePadDPadRightButton()
    {
        isGamePadButtonPressed = true;
    }

    virtual protected void OnGamePadDPadUpButton()
    {
        isGamePadButtonPressed = true;
    }

    virtual protected void OnGamePadDPadDownButton()
    {
        isGamePadButtonPressed = true;
    }

    // GAMEPAD //////////////////////////////////////////////////////////////////////////////////////
}
