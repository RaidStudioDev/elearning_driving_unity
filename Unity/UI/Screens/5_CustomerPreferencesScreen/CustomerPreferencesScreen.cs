using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomerPreferencesScreen : BaseScreen
{
    private RectTransform popupBg;
    private RectTransform customerPrefsTitle;
	private RectTransform optionsContainer;
    private RectTransform optionItemPanel_01;
    private RectTransform optionItemPanel_02;
    private RectTransform optionItemPanel_03;
    private RectTransform gotItButton;
    private RectTransform superTiteSmallLogo;

	Color elementStartColor = new Color(1f, 1f, 1f, 1f);

	public override void Initialize(string id)
    {
        base.Initialize(id);
        
        popupBg = _screenElements["PopupBg"];
        customerPrefsTitle = _screenElements["CustomerPrefsTitle"];
		optionsContainer = _screenElements ["OptionsContainer"];
        optionItemPanel_01 = _screenElements["OptionItemPanel_01"];
        optionItemPanel_02 = _screenElements["OptionItemPanel_02"];
        optionItemPanel_03 = _screenElements["OptionItemPanel_03"];
        gotItButton = _screenElements["GotItButton"];
        superTiteSmallLogo = _screenElements["SuperTireSmallLogo"];

    }

    public override void Draw()
    {
		Vector3 scaleTo = new Vector3 (1f, 1f, 1f);
 
         // Setup Preference Boxes
        RectTransform optionText01 = optionItemPanel_01.Find("Text_01").GetComponent<RectTransform>();
        RectTransform optionText02 = optionItemPanel_02.Find("Text_02").GetComponent<RectTransform>();
        RectTransform optionText03 = optionItemPanel_03.Find("Text_03").GetComponent<RectTransform>();

		optionText01.GetComponent<Text> ().text = PersistentModel.Instance.GetCustomerPreference (0);
		optionText02.GetComponent<Text> ().text = PersistentModel.Instance.GetCustomerPreference (1);
		optionText03.GetComponent<Text> ().text = PersistentModel.Instance.GetCustomerPreference (2);

        Color optionTextFromColor = new Color(0.5f, 0.5f, 0.5f, 0f);
        Color optionTextToColor = new Color(50f / 255f, 50f / 255f, 50f / 255f, 1f);

        // Set option sizes to zero so can scale up in a bit
		optionItemPanel_01.localScale = Vector3.zero;
        optionItemPanel_01.GetComponent<Image>().color = elementStartColor;
        optionText01.GetComponent<Text>().color = optionTextFromColor;

        optionItemPanel_02.localScale = Vector3.zero;
        optionItemPanel_02.GetComponent<Image>().color = elementStartColor;
        optionText02.GetComponent<Text>().color = optionTextFromColor;

        optionItemPanel_03.localScale = Vector3.zero;
        optionItemPanel_03.GetComponent<Image>().color = elementStartColor;
        optionText03.GetComponent<Text>().color = optionTextFromColor;

        // Start Animations
        LeanTween.alpha(superTiteSmallLogo, 1f, 0.75f)
        .setEase(LeanTweenType.easeInOutCubic)
        .setDelay(0.75f);

        customerPrefsTitle.localScale = new Vector3(0f, 1f, 1f);
        customerPrefsTitle.GetComponent<Image>().color = elementStartColor;

        LeanTween.scale(customerPrefsTitle, new Vector3(1f, 1f, 1f), 0.65f)
        .setDelay(0.35f)
        .setEase(LeanTweenType.easeOutBack);

        popupBg.localScale = new Vector3(0f, 1f, 1f);
        popupBg.GetComponent<Image>().color = elementStartColor;

        LeanTween.scale(popupBg, new Vector3(1f, 1f, 1f), 0.65f)
	        .setDelay(0.25f)
	        .setEase(LeanTweenType.easeOutBack)
	        .setOnComplete(() => {



	        });

		LeanTween.delayedCall (0.6f, () => {

			LeanTween.scale(optionsContainer, scaleTo, 0.75f)
				.setEase(LeanTweenType.easeOutBack)
				.setDelay(0.1f);

			// Scale In panels
			LeanTween.scale(optionItemPanel_01, scaleTo, 0.75f)
				.setEase(LeanTweenType.easeOutBack)
				.setDelay(0.2f);
			LeanTween.scale(optionItemPanel_02, scaleTo, 0.75f)
				.setEase(LeanTweenType.easeOutBack)
				.setDelay(0.3f);
			LeanTween.scale(optionItemPanel_03, scaleTo, 0.75f)
				.setEase(LeanTweenType.easeOutBack)
				.setDelay(0.4f);
			LeanTween.colorText(optionText01, optionTextToColor, 0.75f)
				.setEase(LeanTweenType.easeOutQuad)
				.setDelay(0.5f);
			LeanTween.colorText(optionText02, optionTextToColor, 0.75f)
				.setEase(LeanTweenType.easeOutQuad)
				.setDelay(0.55f);
			LeanTween.colorText(optionText03, optionTextToColor, 0.75f)
				.setEase(LeanTweenType.easeOutQuad)
				.setDelay(0.6f)
				.setOnComplete(() => {
					// Display got it button
					LeanTween.alphaCanvas(gotItButton.GetComponent<CanvasGroup>(), 1f, 0.75f);

                    TransitionInCompleted();    // End of Transition
                });
		});
    }

    protected override void TransitionInCompleted()
    {
        // screen draw has been finished, invokes Base.TransitionInCompleteEvent if available
        base.TransitionInCompleted();

        // Set up let's go button
        InitButtonEvents();
    }

    private void InitButtonEvents()
    {
        gotItButton.gameObject.GetComponent<Button>().onClick.AddListener(OnGoButtonClick);

    }

    private void OnGoButtonClick()
    {
	    UIManager.Instance.soundManager.PlaySound("PlaySineWaveHighPitch");

        if (!_isTransitionComplete) return;

        gotItButton.GetComponent<Image>().raycastTarget = false;
        gotItButton.gameObject.GetComponent<Button>().onClick.RemoveListener(OnGoButtonClick);

		selectedScreen = UIManager.Screen.QUIZ_SCREEN;

        OnClickComplete += new OnClickAnimationCompleteEventHandler(StartTransitionOut);
        ButtonClickAnimation(gotItButton);
    }

    private void StartTransitionOut()
    {
 		Text optionText01 = optionItemPanel_01.Find("Text_01").GetComponent<Text>();
        Text optionText02 = optionItemPanel_02.Find("Text_02").GetComponent<Text>();
        Text optionText03 = optionItemPanel_03.Find("Text_03").GetComponent<Text>();
        
		LeanTween.alpha (optionText01.gameObject, 0f, 0.4f)
	        .setEase (LeanTweenType.easeOutQuad)
			.setDelay (0.01f);

        LeanTween.alpha(optionText02.gameObject, 0f, 0.4f)
	        .setEase(LeanTweenType.easeOutQuad)
	        .setDelay(0.03f);

        LeanTween.alpha(optionText03.gameObject, 0f, 0.4f)
	        .setEase(LeanTweenType.easeOutQuad)
	        .setDelay(0.05f);

        LeanTween.alpha(superTiteSmallLogo, 0f, 0.5f)
	        .setEase(LeanTweenType.easeInSine);

        LeanTween.scale(optionItemPanel_01, Vector3.zero, 0.95f)
	        .setEase(LeanTweenType.easeInOutBack)
	        .setOvershoot(0.95f)
	        .setDelay(0.1f);

        LeanTween.scale(optionItemPanel_02, Vector3.zero, 0.95f)
	        .setEase(LeanTweenType.easeInOutBack)
	        .setOvershoot(0.95f)
	        .setDelay(0.15f);

        LeanTween.scale(optionItemPanel_03, Vector3.zero, 0.95f)
	        .setEase(LeanTweenType.easeInOutBack)
	        .setOvershoot(0.95f)
	        .setDelay(0.25f);

		LeanTween.scale(optionsContainer, Vector3.zero, 0.95f)
			.setEase(LeanTweenType.easeInOutBack)
			.setOvershoot(0.95f)
			.setDelay(0.1f);

        LeanTween.scale(customerPrefsTitle, new Vector3(0f, 1f, 1f), 0.85f)
	        .setDelay(0.35f)
	        .setEase(LeanTweenType.easeOutBack);

        LeanTween.scale(popupBg, new Vector3(0f, 1f, 1f), 0.95f)
	        .setDelay(0.55f)
	        .setOvershoot(0.95f)
	        .setEase(LeanTweenType.easeOutBack);

        // Set Progress Complete Event 
        OnProgressLoadingTransitionInComplete += ProgressLoadingTransitionInComplete;

        base.OpenLoadingPanel();
    }

    public override void Remove()
    {
        LeanTween.cancel(popupBg);
        LeanTween.cancel(customerPrefsTitle);
		LeanTween.cancel(optionsContainer);
		LeanTween.cancel(optionItemPanel_01);
        LeanTween.cancel(optionItemPanel_02);
        LeanTween.cancel(optionItemPanel_03);
        LeanTween.cancel(gotItButton);

        Text optionText01 = optionItemPanel_01.Find("Text_01").GetComponent<Text>();
        Text optionText02 = optionItemPanel_02.Find("Text_02").GetComponent<Text>();
        Text optionText03 = optionItemPanel_03.Find("Text_03").GetComponent<Text>();

        LeanTween.cancel(optionText01.gameObject);
        LeanTween.cancel(optionText02.gameObject);
        LeanTween.cancel(optionText03.gameObject);

        base.Remove();
    }
}
