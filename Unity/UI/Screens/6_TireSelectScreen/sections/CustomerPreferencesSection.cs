using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CustomerPreferencesSection : MonoBehaviour
{
    private SelectSequenceScreen root;
    private RectTransform custPref;
    
    private Vector3 barPanelTo;

    private RectTransform titleContainer;
    private RectTransform titleText;
    private RectTransform subTitleText;
    private RectTransform icon;
    private RectTransform optionsContainer;
    private RectTransform[] optionList;

    private bool hasInitialized = false;
    private readonly float titleTopPosition = 0.0f;
    private readonly float titleMidPosition = -365.0f;

    public void Initialize()
    {
        root = this.transform.parent.parent.GetComponent<SelectSequenceScreen>();

        custPref = root.sections.Find("CustomerPreferences").GetComponent<RectTransform>();

        // title red bar
        titleContainer = custPref.Find("TitleContainer").GetComponent<RectTransform>();
        
        // title, subtitle and icon
        titleText = custPref.Find("TitleText").GetComponent<RectTransform>();
        subTitleText = custPref.Find("SubTitleText").GetComponent<RectTransform>();
        icon = custPref.Find("Icon").GetComponent<RectTransform>();

        // set list options
        optionsContainer = custPref.Find("OptionsContainer").GetComponent<RectTransform>();

        // iterate options and cache
        optionList = new RectTransform[3];
        for (int i = 0; i < optionList.Length; i++)
        {
            optionList[i] = optionsContainer.Find("OptionItemPanel_0" + (i + 1)).GetComponent<RectTransform>();
        }

        PrepareDraw();
    }

    public void PrepareDraw()
    {
        // reset scale if first time running
        if (!hasInitialized)
        {
            // set bar to top position 
            titleContainer.anchoredPosition3D = new Vector3(titleContainer.anchoredPosition3D.x, titleTopPosition, titleContainer.anchoredPosition3D.z);

            // set scale in 
            titleContainer.localScale = new Vector3(0f, 1f, 1f);
        }
        else
        {
            // set bar to mid position 
            titleContainer.anchoredPosition3D = new Vector3(titleContainer.anchoredPosition3D.x, titleMidPosition, titleContainer.anchoredPosition3D.z);

            // hide title
            // titleContainer.GetComponent<Image>().color = root.elementHideColor;
        }

        int configCustPrefCount = PersistentModel.Instance.GetCustomerPreferenceCount();
        //Debug.Log("configCustPrefCount: " + configCustPrefCount);
        //Debug.Log("optionList.Length: " + optionList.Length);

        // hide 
        titleText.GetComponent<Text>().color = root.elementHideColor;
        subTitleText.GetComponent<Text>().color = root.elementHideColor;
        icon.GetComponent<Image>().color = root.elementHideColor;

        // iterate options and update
        for (int i = 0; i < optionList.Length; i++)
        {
            if (i == configCustPrefCount)
            {
                optionList[i].localScale = new Vector3(1, 0, 0);
                continue;
            }

            Text optionText = optionList[i].Find("Text_0" + (i + 1)).GetComponent<Text>();

            // update text
            optionText.text = PersistentModel.Instance.GetCustomerPreference(i);

            // Set option sizes to zero so can scale up in a bit
            optionList[i].localScale = new Vector3(1, 0, 0);
            optionList[i].GetComponent<Image>().color = root.elementShowColor;
        }
    }

    public void Draw()
    {
        if (hasInitialized)
        {
            TitleFadeInTransition();
        }
        else
        {
            TitleScaleInTransition();
        }

        
    }

    private void TitleFadeInTransition()
    {
        // set titleContainer from and to positions
        barPanelTo = new Vector3(titleContainer.anchoredPosition3D.x, titleTopPosition, titleContainer.anchoredPosition3D.z);
        
        LeanTween.delayedCall(0.00f, () => { UIManager.Instance.soundManager.PlaySound("PlayNoiseMidHighTone"); });

        // fade in bar 
        LeanTween.alpha(titleContainer, 1f, 0.5f)
            .setDelay(0f)
            .setEase(LeanTweenType.easeOutCubic)
            .setOnComplete(() =>
            {
                // move to top
                LeanTween.move(titleContainer, barPanelTo, 0.5f)
                    .setEase(LeanTweenType.easeInOutQuad)
                    .setOnComplete(() =>
                    {
                        OptionsTransitionIn();
                    });

                LeanTween.alphaText(titleText, 1f, 0.5f).setFrom(0).setEase(LeanTweenType.easeOutCubic).setDelay(0.25f);

                LeanTween.delayedCall(0.55f, () =>
                {
                    //LeanTween.alphaText(titleText, 1f, 0.5f).setFrom(0).setEase(LeanTweenType.easeOutCubic).setDelay(0f);
                    LeanTween.alpha(icon, 1f, 0.5f).setFrom(0).setEase(LeanTweenType.easeOutCubic).setDelay(0.25f);
                    LeanTween.alphaText(subTitleText, 1f, 0.5f)
                        .setFrom(0)
                        .setEase(LeanTweenType.easeOutCubic)
                        .setDelay(0.45f);

                });
            });
    }

    private void TitleScaleInTransition()
    {
        // scale in bar
        LeanTween.scale(titleContainer, new Vector3(1f, 1f, 1f), 0.65f)
            .setDelay(0.0f)
            .setEase(LeanTweenType.easeOutBack);

        LeanTween.alphaText(titleText, 1f, 0.5f)
                .setFrom(0)
                .setEase(LeanTweenType.easeOutCubic)
                .setDelay(0.25f);

        // scale in options
        LeanTween.delayedCall(0.35f, () =>
        {
            OptionsTransitionIn();

            /*LeanTween.alphaText(titleText, 1f, 0.5f)
                .setFrom(0)
                .setEase(LeanTweenType.easeOutCubic)
                .setDelay(0f);*/

            LeanTween.alpha(icon, 1f, 0.5f).setFrom(0)
                .setEase(LeanTweenType.easeOutCubic)
                .setDelay(0.25f)
                .setOnComplete(() =>
                {
                    hasInitialized = true;
                    // OptionsTransitionIn();
                });

            LeanTween.alphaText(subTitleText, 1f, 0.5f)
                .setFrom(0)
                .setEase(LeanTweenType.easeOutCubic)
                .setDelay(0.45f);

        });
    }

    private void OptionsTransitionIn()
    {
        Vector3 scaleTo = new Vector3(1f, 1f, 1f);
        float delay = 0.2f;

        LeanTween.delayedCall(0.0f, () => {

            LeanTween.scale(optionsContainer, scaleTo, 0.75f)
                .setEase(LeanTweenType.easeOutBack)
                .setDelay(0.1f);
           
            LeanTween.delayedCall(delay, () => { UIManager.Instance.soundManager.PlaySound("PlayNoiseMidHighTone"); });

            int configCustPrefCount = PersistentModel.Instance.GetCustomerPreferenceCount();

            // iterate options and update scale
            for (int i = 0; i < configCustPrefCount; i++)
            {
                LeanTween.scale(optionList[i], scaleTo, 0.75f)
                    .setEase(LeanTweenType.easeOutBack)
                    .setDelay(delay);

                delay += 0.1f;
            }

            TransitionInCompleted();

        });
    }

    private void TransitionInCompleted()
    {
        root.UpdateSubmitButton("GOT IT!");
        root.submitButton.GetComponent<Button>().onClick.AddListener(OnGotItButtonClick);

        if (root.isGamePadEnabled)
        {
            root.submitButton.GetComponent<Button>().Select();
        }
    }

    private void OnGotItButtonClick()
    {
        UIManager.Instance.soundManager.PlaySound("PlaySineWaveHighPitch");

        root.submitButton.GetComponent<Button>().onClick.RemoveListener(OnGotItButtonClick);

        root.HideSubmitButton();

        StartTransitionOut();
    }

    public void StartTransitionOut()
    {
        // set titleContainer to positions
        barPanelTo = new Vector3(titleContainer.anchoredPosition3D.x, titleMidPosition, titleContainer.anchoredPosition3D.z);

        // fade out title and sub contents
        LeanTween.delayedCall(0.0f, () =>
        {
            LeanTween.alphaText(titleText, 0f, 0.5f).setFrom(1).setEase(LeanTweenType.easeInCubic).setDelay(0f);
            LeanTween.alpha(icon, 0f, 0.5f).setFrom(1).setFrom(1).setEase(LeanTweenType.easeInCubic).setDelay(0.15f);
            LeanTween.alphaText(subTitleText, 0f, 0.5f).setFrom(1).setEase(LeanTweenType.easeInCubic).setDelay(0.25f)
                .setOnComplete(() =>
                {
                    
                });

            // scale out options and move title bar down
            LeanTween.delayedCall(0.5f, () => {

                float delay = 0.0f;

                // iterate options and update scale
                for (int i = 0; i < optionList.Length; i++)
                {
                    int index_item = i;

                    LeanTween.scale(optionList[i], new Vector3(1, 0, 0), 0.5f)
                        .setEase(LeanTweenType.easeInBack)
                        .setOvershoot(0.55f)
                        .setDelay(delay)
                        .setOnComplete(() =>
                        {
                            // end transition after last item
                            if (index_item == optionList.Length - 1)
                            {
                                // move to top
                                LeanTween.move(titleContainer, barPanelTo, 0.5f)
                                    .setEase(LeanTweenType.easeInOutQuad)
                                    .setOnComplete(() =>
                                    {
                                        TransitionOutCompleted();
                                    });
                            }
                        });

                    delay += 0.1f;
                }
            });
        });
    }

    private void TransitionOutCompleted()
    {
        // show next section
        root.CurrentState = SelectSequenceScreen.Section.TIRESELECT;
    }

    public void Remove()
    {
        titleContainer.gameObject.SetActive(false);

        for (int i = 0; i < optionList.Length; i++)
        {
            optionList[i].gameObject.SetActive(false);
        }
    }
}
