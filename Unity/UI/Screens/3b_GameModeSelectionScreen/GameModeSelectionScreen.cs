using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameModeSelectionScreen : BaseScreen {

    private RectTransform superTireSmallLogo;
    private RectTransform titleText;
    private RectTransform menuButton1;  // passenger
    private RectTransform menuButton2;  // light truck
    private RectTransform menuButton3;  // winter

    private Color elementShowColor = new Color(1f, 1f, 1f, 1f);
    private Color elementHideColor = new Color(1f, 1f, 1f, 0f);
    private Vector3 scaleOut = new Vector3(0f, 1f, 1f);
    private Vector3 scaleIn = new Vector3(1f, 1f, 1f);

    public override void Initialize(string id)
    {
        base.Initialize(id);

        superTireSmallLogo = _screenElements["SuperTireSmallLogo"];
        titleText = _screenElements["TitleText"];
        menuButton1 = _screenElements["MenuButton1"];
        menuButton2 = _screenElements["MenuButton2"];
        menuButton3 = _screenElements["MenuButton3"];

        titleText.GetComponent<Text>().color = elementHideColor;
        menuButton1.localScale = scaleOut;
        menuButton2.localScale = scaleOut;
        menuButton3.localScale = scaleOut;
        menuButton1.GetComponent<Image>().color = elementShowColor;
        menuButton2.GetComponent<Image>().color = elementShowColor;
        menuButton3.GetComponent<Image>().color = elementShowColor;

        // check if user has already completed circuits
        if (PersistentModel.Instance.TracksComplete.passenger)
        {
            menuButton1.GetComponent<Image>().sprite = menuButton1.GetComponent<Button>().spriteState.disabledSprite;
        }
        if (PersistentModel.Instance.TracksComplete.trucks)
        {
            menuButton2.GetComponent<Image>().sprite = menuButton2.GetComponent<Button>().spriteState.disabledSprite;
        }
        if (PersistentModel.Instance.TracksComplete.winter)
        {
            menuButton3.GetComponent<Image>().sprite = menuButton3.GetComponent<Button>().spriteState.disabledSprite;
        }
    }
    
    public override void Draw()
    {
        LeanTween.alpha(superTireSmallLogo, 1f, 0.75f).setEase(LeanTweenType.easeInOutCubic).setDelay(0.1f);
        
        LeanTween.delayedCall(0.5f, () => { UIManager.Instance.soundManager.PlaySound("PlaySawLowHighTone"); });
        LeanTween.alphaText(titleText, 1f, 1f).setDelay(0.5f).setEase(LeanTweenType.easeOutQuad).setFrom(0f);
        
        LeanTween.delayedCall(0.95f, () => { UIManager.Instance.soundManager.PlaySound("PlayNoiseMidHighTone"); });
        LeanTween.scale(menuButton1, scaleIn, 0.5f).setEase(LeanTweenType.easeOutBack).setDelay(0.95f).setOvershoot(0.5f);
        LeanTween.scale(menuButton2, scaleIn, 0.5f).setEase(LeanTweenType.easeOutBack).setDelay(1.05f).setOvershoot(0.5f);
        LeanTween.scale(menuButton3, scaleIn, 0.5f).setEase(LeanTweenType.easeOutBack).setDelay(1.15f).setOvershoot(0.5f)
            .setOnComplete(() => {
                TransitionInCompleted();    // End of Transition
            });
    }

    private Sprite visitedState;
    protected override void TransitionInCompleted()
    {
        // screen draw has been finished, invokes Base.TransitionInCompleteEvent if available
        base.TransitionInCompleted();

        if (isGamePadEnabled)
        {
            currentMenuIndex = 0;
            menuButton1.GetComponent<Button>().Select();
        }

        AddEvents();
    }

    private void AddEvents()
    {
        menuButton1.GetComponent<Button>().onClick.AddListener(OnPassengerButtonClick);
        menuButton2.GetComponent<Button>().onClick.AddListener(OnLightTrucksButtonClick);
        menuButton3.GetComponent<Button>().onClick.AddListener(OnWinterButtonClick);
    }

    private void RemoveEvents()
    {
        menuButton1.GetComponent<Button>().onClick.RemoveListener(OnPassengerButtonClick);
        menuButton2.GetComponent<Button>().onClick.RemoveListener(OnLightTrucksButtonClick);
        menuButton3.GetComponent<Button>().onClick.RemoveListener(OnWinterButtonClick);
    }

    private int currentMenuIndex = -1;
    protected override void OnGamePadDPadUpButton()
    {
        base.OnGamePadDPadUpButton();

        if (currentMenuIndex != -1 && currentMenuIndex > 0) currentMenuIndex--;

        UpdateMenuSelect();
    }

    protected override void OnGamePadDPadDownButton()
    {
        base.OnGamePadDPadDownButton();

        if (currentMenuIndex != -1 && currentMenuIndex < 2) currentMenuIndex++;

        UpdateMenuSelect();
    }

    private void UpdateMenuSelect()
    {
        EventSystem.current.SetSelectedGameObject(null);

        switch (currentMenuIndex)
        {
            case 0:
                menuButton1.GetComponent<Button>().Select();
                break;
            case 1:
                menuButton2.GetComponent<Button>().Select();
                break;
            case 2:
                menuButton3.GetComponent<Button>().Select();
                break;
        }
    }

    void DisableMenuButtons()
    {
        menuButton1.GetComponent<Image>().raycastTarget = false;
        menuButton2.GetComponent<Image>().raycastTarget = false;
        menuButton3.GetComponent<Image>().raycastTarget = false;
    }

    void OnLightTrucksButtonClick()
    {
        RemoveEvents();

        DisableMenuButtons();

        ButtonClickAnimation(menuButton2);

        PersistentModel.Instance.Mode = PersistentModel.ModeEnum.LIGHTTRUCK;
        PersistentModel.Instance.GameModeID = PersistentModel.Instance.GetGameModeID;

        OnClick();
    }

    void OnPassengerButtonClick()
    {
        RemoveEvents();

        DisableMenuButtons();

        ButtonClickAnimation(menuButton1);

        PersistentModel.Instance.Mode = PersistentModel.ModeEnum.PASSENGER;
        PersistentModel.Instance.GameModeID = PersistentModel.Instance.GetGameModeID;

        OnClick();
    }

    void OnWinterButtonClick()
    {
        RemoveEvents();

        DisableMenuButtons();

        ButtonClickAnimation(menuButton3);

        PersistentModel.Instance.Mode = PersistentModel.ModeEnum.WINTER;
        PersistentModel.Instance.GameModeID = PersistentModel.Instance.GetGameModeID;

        OnClick();
    }

    void OnClick()
    {
        UIManager.Instance.soundManager.PlaySound("PlaySineWaveHighPitch");

        RemoveEvents();

        selectedScreen = (!PersistentModel.Instance.HasReadInstructions) ? UIManager.Screen.INSTRUCTIONS : UIManager.Screen.QUIZ_SCREEN;

        OnClickComplete += StartTransitionOut;
    }

    void StartTransitionOut()
    {
        if (PersistentModel.Instance.RandomizeTracks)
            PersistentModel.Instance.ChallengeIndex = PersistentModel.Instance.GetRandomChallengeIndex();
      
        OnClickComplete -= StartTransitionOut;

        LeanTween.alpha(superTireSmallLogo, 0f, 0.75f).setEase(LeanTweenType.easeOutQuad).setDelay(0.1f);
        LeanTween.alphaText(titleText, 0f, 1f).setDelay(0.5f).setEase(LeanTweenType.easeOutQuad).setFrom(1f);
        
        LeanTween.delayedCall(0.35f, () => { UIManager.Instance.soundManager.PlaySound("PlaySawLowHighTone"); });
        LeanTween.scale(menuButton1, scaleOut, 0.5f).setEase(LeanTweenType.easeInOutBack).setDelay(0.35f).setOvershoot(0.5f);
        LeanTween.scale(menuButton2, scaleOut, 0.5f).setEase(LeanTweenType.easeInOutBack).setDelay(0.45f).setOvershoot(0.5f);
        LeanTween.scale(menuButton3, scaleOut, 0.5f).setEase(LeanTweenType.easeInOutBack).setDelay(0.55f).setOvershoot (0.5f)
            .setOnComplete(() => {
                // Set Progress Complete Event 
                OnProgressLoadingTransitionInComplete += ProgressLoadingTransitionInComplete;

                base.OpenLoadingPanel();
            });
    }
    
    public override void Remove()
    {
        RemoveEvents();
        
        base.Remove();
    }
}
