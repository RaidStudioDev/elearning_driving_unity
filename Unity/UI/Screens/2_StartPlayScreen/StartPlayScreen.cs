using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StartPlayScreen : BaseScreen {

    private RectTransform bridgestoneLogo;
    private RectTransform superTireLogo;
    private RectTransform switchPart1_Logo;
    private RectTransform switchPart2_Logo;
    private RectTransform playButton;
    private RectTransform copyrightTxt;

    // BridgestoneLogo, SuperTireLogo, SwitchPart1_Logo, SwitchPart2_Logo, PlayButton

    public override void Initialize(string id)
    {
        base.Initialize(id);

        bridgestoneLogo = _screenElements["BridgestoneLogo"];
        superTireLogo = _screenElements["SuperTireLogo"];
        switchPart1_Logo = _screenElements["SwitchPart1_Logo"];
        switchPart2_Logo = _screenElements["SwitchPart2_Logo"];
        playButton = _screenElements["PlayButton"];
    	copyrightTxt = _screenElements["CopyrightText"];

        // Hide copyright
        copyrightTxt.GetComponent<Text>().color = new Color(1f, 1f, 1f, 0f);
        copyrightTxt.GetComponent<Text>().text = "user: " + PersistentModel.Instance.Name + " email: " + PersistentModel.Instance.Email;
    }

    public override void Draw()
    {
        Vector3 superTireLogoFrom = superTireLogo.anchoredPosition3D + Vector3.left * _screenElements["LeftSideBg"].rect.width;
        Vector3 superTireLogoTo = superTireLogo.anchoredPosition3D;
        superTireLogo.anchoredPosition3D = superTireLogoFrom;
        superTireLogo.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
        
        LeanTween.delayedCall(0.25f, () => { UIManager.Instance.soundManager.PlaySound("PlaySawLowHighTone"); });

        LeanTween.move(superTireLogo, superTireLogoTo, 0.85f)
        .setEase(LeanTweenType.easeInOutBack)
        .setDelay(0.25f)
        .setOvershoot(0.95f)
        .setOnComplete(()=> {

            Vector3 superTireLogoLoop = superTireLogoTo + Vector3.up * 12f;
            LeanTween.move(superTireLogo, superTireLogoLoop, 3.5f)
            .setEase(LeanTweenType.linear)
            .setLoopPingPong();

			LeanTween.delayedCall(0.25f, () => {
                UIManager.Instance.soundManager.PlaySound("PlayLongAmbientSineWaveVibrato", 0.35f, -0.95f);
                UIManager.Instance.soundManager.PlaySound("PlayLongAmbientLowMidWaveVibrato", 0.25f, 0.75f);
            });

            LeanTween.scale(superTireLogo, new Vector3(1.05f, 0.99f, 0.90f), 4.5f)
            .setEase(LeanTweenType.easeInOutBack)
            .setLoopPingPong();

            LeanTween.delayedCall(0.25f, () => {

                LeanTween.alpha(playButton, 1f, 0.95f)
                .setEase(LeanTweenType.easeOutQuad)
                .setOnComplete(()=>
                {
                    LeanTween.alpha(playButton, 0.5f, 0.35f)
                    .setEaseInOutCubic()
                    .setLoopPingPong();

                    TransitionInCompleted();    // End of Transition
                });
            });

			// JBC: Show copyright last
			LeanTween.alphaText(copyrightTxt, 1f, 1f)
					.setDelay(0.8f)
					.setEase(LeanTweenType.easeOutQuad)
					.setFrom(0f);
        });
        
        Vector3 switchLogo1From = switchPart1_Logo.anchoredPosition3D + Vector3.left * _screenElements["LeftSideBg"].rect.width;
        Vector3 switchLogo1To = switchPart1_Logo.anchoredPosition3D;
        switchPart1_Logo.anchoredPosition3D = switchLogo1From;
        switchPart1_Logo.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);

        LeanTween.delayedCall(0.65f, () => { UIManager.Instance.soundManager.PlaySound("PlayNoiseMidHighTone", 0.65f, -0.95f); });
        LeanTween.delayedCall(0.6f, () => { UIManager.Instance.soundManager.PlaySound("PlayNoiseMidHighTone", 0.5f, 0.75f); });

        LeanTween.move(switchPart1_Logo, switchLogo1To, 0.85f)
        .setEase(LeanTweenType.easeInOutBack)
        .setDelay(0.65f)
        .setOvershoot(0.95f);

        Vector3 switchLogo2From = switchPart2_Logo.anchoredPosition3D + Vector3.right * _screenElements["LeftSideBg"].rect.width;
        Vector3 switchLogo2To = switchPart2_Logo.anchoredPosition3D;
        switchPart2_Logo.anchoredPosition3D = switchLogo2From;
        switchPart2_Logo.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);

        LeanTween.move(switchPart2_Logo, switchLogo2To, 0.85f)
        .setEase(LeanTweenType.easeInOutBack)
        .setDelay(0.65f)
        .setOvershoot(1.00f);

        LeanTween.alpha(bridgestoneLogo, 1f, 0.75f)
        .setEase(LeanTweenType.easeOutSine);
    }

    protected override void TransitionInCompleted()
    {
        DebugLog.Trace("StartPlay.TransitionInCompleted()");

        // screen draw has been finished, invokes Base.TransitionInCompleteEvent if available
        base.TransitionInCompleted();

        UIManager.Instance.soundManager.mPlayer.PlayTrack(0);

        DisableGamePad();

        if (PersistentModel.Instance.Server.isShowLogin && !PersistentModel.Instance.Server.isPasscodeAuthorized)
        {
            _ui.Overlay.ShowOverlay(OverlayManager.LOGIN, OnLoginClose);
        }
        else
        {
            DebugLog.Trace("StartPlay.ByPass_Login()");

            AddEvents();
            EnableGamePad();
            if (isGamePadEnabled) playButton.gameObject.GetComponent<Button>().Select();
        }
    }

    void OnPlayButtonClick()
    {
		UIManager.Instance.soundManager.PlaySound("PlaySineWaveHighPitch");

        if (!_isTransitionComplete) return;

        playButton.gameObject.GetComponent<Button>().interactable = false;
        playButton.GetComponent<Image>().raycastTarget = false;
        playButton.gameObject.GetComponent<Button>().onClick.RemoveListener(OnPlayButtonClick);

        selectedScreen = UIManager.Screen.GAMEMODE_SELECTION;

        OnClickComplete += StartTransitionOut;
        ButtonClickAnimation(playButton);
    }

    void StartTransitionOut()
    {
        OnClickComplete -= StartTransitionOut;

        // JBC: Show copyright last
        LeanTween.alphaText(copyrightTxt, 0f, 1f)
            .setDelay(0f)
            .setEase(LeanTweenType.easeOutQuad)
            .setFrom(1f);

        LeanTween.cancel(superTireLogo);

        LeanTween.alpha(bridgestoneLogo, 0f, 0.5f).setEase(LeanTweenType.easeInSine);
        
        LeanTween.delayedCall(0.15f, () => { UIManager.Instance.soundManager.PlaySound("PlaySawLowHighTone", 0.5f, -0.95f); });
        
        LeanTween.delayedCall(0.1f, () => { UIManager.Instance.soundManager.PlaySound("PlaySawLowHighTone", 0.5f, 0.75f); });

        // Super Tire Logo Sprite
        LeanTween.scale(superTireLogo, new Vector3(0f, 0f, 0.1f), 0.95f)
        .setEase(LeanTweenType.easeInOutBack)
        .setOvershoot(0.95f);

        LeanTween.alpha(superTireLogo.gameObject, 0f, 0.95f)
        .setDelay(0.25f)
        .setEase(LeanTweenType.easeInSine);

        // Switch Logo Part 1 Sprite
        LeanTween.scale(switchPart1_Logo, Vector3.zero, 0.95f)
        .setEase(LeanTweenType.easeInBack)
        .setDelay(0.25f)
        .setOvershoot(2.5f);

        LeanTween.rotateAroundLocal(switchPart1_Logo.gameObject, new Vector3(0f, 0f, 6.25f), 14.5f, 1.50f)
        .setEase(LeanTweenType.easeInOutSine)
        .setDelay(0.25f);

        // Switch Logo Part 2 Sprite
        LeanTween.scale(switchPart2_Logo, Vector3.zero, 0.95f)
        .setEase(LeanTweenType.easeInBack)
        .setDelay(0.60f)
        .setOvershoot(2.5f);

        LeanTween.rotateAroundLocal(switchPart2_Logo.gameObject, new Vector3(0f, 0f, -6.25f), 14.5f, 1.50f)
        .setEase(LeanTweenType.easeInOutSine)
        .setDelay(0.60f);

        // Set Progress Complete Event 
        OnProgressLoadingTransitionInComplete += ProgressLoadingTransitionInComplete;

        base.OpenLoadingPanel();
    }
    
    private void AddEvents()
    {
        playButton.gameObject.GetComponent<Button>().onClick.AddListener(OnPlayButtonClick);
    }

    private void RemoveEvents()
    {
        playButton.gameObject.GetComponent<Button>().onClick.RemoveListener(OnPlayButtonClick);
    }

    public override void Remove()
    {
        RemoveEvents();

        if (PersistentModel.Instance.Server.isShowLogin)
        {
            OnLoginClose();
        }

        // Clear Tweens, just in case
        LeanTween.cancel(superTireLogo);
        LeanTween.cancel(switchPart1_Logo);
        LeanTween.cancel(switchPart2_Logo);
        LeanTween.cancel(bridgestoneLogo);

        base.Remove();
    }

    private void OnLoginClose()
    {
        EnableGamePad();
        if (isGamePadEnabled) playButton.gameObject.GetComponent<Button>().Select();
        AddEvents();    // set play button event

        copyrightTxt.GetComponent<Text>().text = "user: " + PersistentModel.Instance.Name + " email: " + PersistentModel.Instance.Email;
    }
}
