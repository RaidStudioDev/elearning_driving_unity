using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TireSelectSection : MonoBehaviour
{
    private SelectSequenceScreen root;
    private RectTransform tireSelect;
    private int selectedIndex = -1;

    private RectTransform sectionLoaderLabel;
    private RectTransform billboardContainer;
    private Image billboardImage;

    private RectTransform customerPrefBtn;
    private RectTransform tireQuestionBar;
    private RectTransform tireQuestionText;

    private RectTransform[] optionList;
    private TireOptionData[] optionDataList;

    private bool hasInitialized = false;
    public bool HasInitialized { get { return hasInitialized; } set { hasInitialized = value; } }

    [HideInInspector]
    public int currentIndex = 0;        // Tire Option Index

    public void Initialize()
    {
        root = this.transform.parent.parent.GetComponent<SelectSequenceScreen>();

        tireSelect = root.sections.Find("TireSelect").GetComponent<RectTransform>();

        // main header image
        billboardContainer = tireSelect.Find("BillboardContainer").GetComponent<RectTransform>();

        // hide cust pref button
        customerPrefBtn = tireSelect.Find("CustomerPrefBtnContainer").Find("CustomerPrefButton").GetComponent<RectTransform>();
        customerPrefBtn.GetComponent<Image>().alphaHitTestMinimumThreshold = 1f;    // hit area, ignores transparent pixels
        customerPrefBtn.GetComponent<Image>().raycastTarget = false;

        // question bar and text
        tireQuestionBar = tireSelect.Find("QuestionContainer").GetComponent<RectTransform>();
        tireQuestionText = tireSelect.Find("QuestionText").GetComponent<RectTransform>();

        // hide loader
        sectionLoaderLabel = tireSelect.Find("LoadingTiresLabel").GetComponent<RectTransform>();

        // get and hide options
        RectTransform options = tireSelect.Find("Options").GetComponent<RectTransform>();
        optionList = new RectTransform[4];
        optionDataList = new TireOptionData[4];

        // tire option indexes
        var optionIndexes = new List<int>(Enumerable.Range(0, 4));

        // generate random array
        if (PersistentModel.Instance.RandomizeTireOptions)
            optionIndexes.ShuffleCrypto();

        // Debug.Log("LENGTH: " + optionIndexes.Count);

        // iterate options and cache
        for (int i = 0; i < optionList.Length; i++)
        {
            // Debug.Log("Random Index: " + optionIndexes[i]);

            TireOptionData item = new TireOptionData()
            {
                index = optionIndexes[i],
                correct = PersistentModel.Instance.IsChallengeOptionCorrect(optionIndexes[i]),
                transform = options.Find("Option_" + (i + 1)).GetComponent<RectTransform>()
            };

            optionDataList[i] = item;
            optionList[i] = options.Find("Option_" + (i + 1)).GetComponent<RectTransform>();
        }

        PrepareDraw(false);
    }

    public void PrepareDraw(bool forceFade)
    {
        billboardContainer.GetComponent<Image>().color = root.elementHideColor;
        customerPrefBtn.GetComponent<Image>().color = root.elementHideColor;
        sectionLoaderLabel.GetComponent<Text>().color = root.elementHideColor;
        
        LeanTween.delayedCall(0.1f, () => { UIManager.Instance.soundManager.PlaySound("PlaySawLowHighTone"); });

        if (!forceFade)
        {
            // hide red strip bar scale
            // tireQuestionBar.localScale = new Vector3(0f, 1f, 1f);
            tireQuestionBar.GetComponent<Image>().color = root.elementHideColor;
        }
        else
        {
            tireQuestionBar.localScale = new Vector3(1f, 1f, 1f);
            tireQuestionBar.GetComponent<Image>().color = root.elementShowColor;
            LeanTween.alpha(tireQuestionBar, 0f, 0.5f).setFrom(1).setEase(LeanTweenType.easeOutCubic);
        }

        // hide question text
        tireQuestionText.GetComponent<Text>().color = root.elementHideColor;
        tireQuestionText.GetComponent<Text>().text = PersistentModel.Instance.ChallengeText;

        // iterate options and hide them
        for (int i = 0; i < optionList.Length; i++)
        {
            optionList[i].GetComponent<Image>().raycastTarget = false;
            optionList[i].Find("Image").GetComponent<Image>().color = root.elementHideColor;
        }
    }

    public void Draw()
    {
        // show sub loading message
        sectionLoaderLabel.GetComponent<Text>().color = root.elementLoadingColor;
        LeanTween.alphaText(sectionLoaderLabel, 0.2f, 0.35f).setEaseInOutCubic().setLoopPingPong();

        // start loader
        StartCoroutine(LoadBillboardHeader());
    }

    IEnumerator LoadBillboardHeader()
    {
        billboardImage = billboardContainer.GetComponent<Image>();
        Texture2D billboardTexture;

        string billboardName = PersistentModel.Instance.GetChallengeBillboardName();

        DebugLog.Trace("billboardName/" + billboardName);

        if (Application.platform != RuntimePlatform.WebGLPlayer)
        {
            billboardTexture = Instantiate(Resources.Load("billboards/" + billboardName)) as Texture2D;
            billboardImage.sprite = Sprite.Create(billboardTexture, new Rect(0, 0, billboardTexture.width, billboardTexture.height), new Vector2(.5f, .5f), 100.0f);
        }
        else
        {
            billboardTexture = new Texture2D(1801, 350, TextureFormat.RGB24, false);

            using (WWW www = new WWW(PersistentModel.Instance.DynamicAssetsURL + "billboards/" + billboardName + ".jpg"))
            {
                yield return www;
                www.LoadImageIntoTexture(billboardTexture);
                billboardImage.sprite = Sprite.Create(billboardTexture, new Rect(0, 0, billboardTexture.width, billboardTexture.height), new Vector2(.5f, .5f), 100.0f);
            }
        }

        billboardImage.color = root.elementHideColor;

        StartCoroutine(LoadTireOptionImages());
    }

    IEnumerator LoadTireOptionImages()
    {
        // start loading tire images

        Image image;
        Texture2D texture2D;
        string tireName;

        for (int i = 0; i < optionList.Length; i++)
        {
            image = optionList[i].Find("Image").GetComponent<Image>();

            tireName = PersistentModel.Instance.GetChallengeTireName(optionDataList[i].index);

            if (Application.platform != RuntimePlatform.WebGLPlayer)
            {
                DebugLog.Trace("tires/" + tireName);
                texture2D = Instantiate(Resources.Load("tires/" + tireName)) as Texture2D;
                image.sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(.5f, .5f));
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

            image.color = root.elementHideColor;
        }

        StartCoroutine(OnImagesLoaded());
    }

    IEnumerator OnImagesLoaded()
    {
        yield return null;

        // remove loader label
        LeanTween.cancel(sectionLoaderLabel.gameObject);
        LeanTween.alphaText(sectionLoaderLabel, 0f, 0.35f).setEaseOutCubic().setFrom(1f)
            .setOnComplete(() => {
                StartCoroutine(StartTransitionIn());
            });
    }

    IEnumerator StartTransitionIn()
    {
        yield return null;

        LeanTween.delayedCall(0.1f, () => { UIManager.Instance.soundManager.PlaySound("PlayNoiseMidHighTone"); });
        
        if (!hasInitialized)
        {
            ScaleTransitionIn();
        }
        else
        {
            FadeTransitionIn();
        }

        customerPrefBtn.GetComponent<Image>().raycastTarget = true;
    }

    private void FadeTransitionIn()
    {
        // start showing options
        LeanTween.alpha(tireQuestionBar, 1f, 0.5f).setFrom(0).setDelay(0f).setEase(LeanTweenType.easeOutCubic)
            .setOnComplete(() => {

                // show question
                LeanTween.alphaText(tireQuestionText, 1f, 0.65f).setEase(LeanTweenType.easeOutCubic).setDelay(0.01f);

                // show vehicle image and cust preferences btn
                LeanTween.alpha(billboardImage.GetComponent<RectTransform>(), 1f, 0.75f)
                    .setEase(LeanTweenType.easeOutCubic)
                    .setDelay(0.25f)
                    .setOnComplete(() =>
                    {
                        // check if we have selected a tire, if not show tire options
                        if (root.TireSelectedIndex == -1 && !root.HasTireBeenSubmitted)
                        {
                            // show options
                            LeanTween.delayedCall(0.01f, ShowListOptions);
                        }
                     
                        LeanTween.alpha(customerPrefBtn, 1f, 0.5f).setEase(LeanTweenType.easeOutCubic).setDelay(0f);

                        LeanTween.alpha(root.pauseButton, 1f, 0.65f).setEase(LeanTweenType.easeOutCubic).setDelay(1.20f);
                    });

                hasInitialized = true;

                TransitionInCompleted();
            });
    }

    private void ScaleTransitionIn()
    {
        // start showing options
        LeanTween.scale(tireQuestionBar, new Vector3(1f, 1f, 1f), 0.5f).setDelay(0f).setEase(LeanTweenType.easeOutCubic)
            .setOnComplete(() => {

                // show question
                LeanTween.alphaText(tireQuestionText, 1f, 0.65f).setEase(LeanTweenType.easeOutCubic).setDelay(0.01f);

                // show vehicle image and cust preferences btn
                LeanTween.alpha(billboardImage.GetComponent<RectTransform>(), 1f, 0.75f)
                    .setEase(LeanTweenType.easeOutCubic)
                    .setDelay(0.25f)
                    .setOnComplete(() =>
                    {
                        // check if we have selected a tire, if not show tire options
                        if (root.TireSelectedIndex == -1 && !root.HasTireBeenSubmitted)
                        {
                            // show options
                            LeanTween.delayedCall(0.01f, ShowListOptions);
                        }
                    
                        LeanTween.alpha(customerPrefBtn, 1f, 0.5f).setEase(LeanTweenType.easeOutCubic).setDelay(0f);

                        LeanTween.alpha(root.pauseButton, 1f, 0.65f).setEase(LeanTweenType.easeOutCubic).setDelay(1.20f);
                    });

                TransitionInCompleted();
            });
    }

    private void TransitionInCompleted()
    {
        // add cust preferences click
        customerPrefBtn.GetComponent<Image>().raycastTarget = true;
        customerPrefBtn.GetComponent<Button>().onClick.AddListener(OnCustPrefClick);

        // start the game time
        if (!hasInitialized) root.InitializeTimeClock();
        else {
            root.HideSubmitButton();
            root.ShowClockIsRunningSign();
            root.StartClock();
            root.HideClockIsStoppedSign();
        }

        hasInitialized = true;
    }

    private void OnCustPrefClick()
    {
        // Stop the clock
        root.HideClockIsRunningSign();
        root.StopClock();
        root.ShowClockIsStoppedSign();

        customerPrefBtn.GetComponent<Image>().raycastTarget = false;
        customerPrefBtn.GetComponent<Button>().onClick.RemoveListener(OnCustPrefClick);

        root.HideSubmitButton();

        root.ButtonClickAnimation(customerPrefBtn);

        UIManager.Instance.soundManager.PlaySound("PlaySineWaveHighPitch");

        StartTransitionOut();

        // show next section
        root.CurrentState = SelectSequenceScreen.Section.CUST_PREF;
    }

    public void StartTransitionOut()
    {
        HideListOptions();
    }

    public void OnLeftButton()
    {
        if (currentIndex > 0) currentIndex--;

        EventSystem.current.SetSelectedGameObject(null);

        optionList[currentIndex].GetComponent<Button>().Select();
    }

    public void OnRightButton()
    {
        if (currentIndex < optionList.Length - 1) currentIndex++;

        EventSystem.current.SetSelectedGameObject(null);

        optionList[currentIndex].GetComponent<Button>().Select();
    }

    private void ShowListOptions()
    {
        selectedIndex = -1;

        // iterate options, cache and hide them
        float[] delay = { 0.25f, 0.35f, 0.45f, 0.55f };
        for (int i = 0; i < optionList.Length; i++)
        {
            optionDataList[i].transform.GetComponent<Image>().raycastTarget = true;
            optionDataList[i].transform.GetComponent<Button>().interactable = true;
            optionDataList[i].transform.localScale = new Vector3(0f, 0f, 0f);
            optionDataList[i].transform.Find("Image").GetComponent<Image>().color = root.elementShowColor;

            int index = i;
            int selIndex = optionDataList[i].index;

            DebugLog.Trace("optionDataList[i].index: " + optionDataList[i].index);

            optionDataList[i].transform.GetComponent<Button>().onClick.AddListener(delegate { ListOptionClick(index, selIndex); });
            
            LeanTween.delayedCall(delay[i], () => { UIManager.Instance.soundManager.PlaySound("PlaySawLowHighTone"); });

            LeanTween.scale(optionDataList[i].transform, new Vector3(1f, 1f, 1f), 0.6f)
                .setOvershoot(0.95f)
                .setEase(LeanTweenType.easeOutBack)
                .setDelay(delay[i]);
        }

        if (root.isGamePadEnabled)
        {
            optionDataList[currentIndex].transform.GetComponent<Button>().Select();
        }
    }

    private void ListOptionClick(int index , int selIndex)
    {
        UIManager.Instance.soundManager.PlaySound("PlayLowToneButton");

        if (selectedIndex == -1)
        {
            // When user selects a tire, show submit button (first time only)
            root.UpdateSubmitButton("SUBMIT");
            root.submitButton.GetComponent<Button>().onClick.AddListener(delegate { OnListOptionSubmitButtonClick(); });

            if (root.isGamePadEnabled) root.submitButton.GetComponent<Button>().Select();
        }

        selectedIndex = selIndex;

        root.FlickerAnimation(optionDataList[index].transform.GetComponent<RectTransform>(), 0.15f, 4, false);

        // Show ring around selected tire (by using its disabledSprite)
        for (int i = 0; i < optionList.Length; i++)
        {
            optionList[i].GetComponent<Button>().interactable = (i != index);
        }
    }

    private void OnListOptionSubmitButtonClick()
    {
        // stop game time
        PersistentModel.Instance.ClockIsStopped = true;

        root.submitButton.GetComponent<Button>().onClick.RemoveListener(OnListOptionSubmitButtonClick);

        root.HideSubmitButton();

        UIManager.Instance.soundManager.PlaySound("PlaySineWaveHighPitch");

        // set submission flag
        root.HasTireBeenSubmitted = true;

        // save tire selected
        root.TireSelectedIndex = selectedIndex;

        PersistentModel.Instance.TireOptionSelectedCorrect = PersistentModel.Instance.IsChallengeOptionCorrect(selectedIndex);

        PersistentModel.Instance.TireOptionSelectedData = new TireOptionData()
        {
            index = selectedIndex,
            correct = PersistentModel.Instance.IsChallengeOptionCorrect(selectedIndex),
            feedback = PersistentModel.Instance.GetChallengeOptionWhyText(selectedIndex)
        };

        // bool isCorrect = PersistentModel.Instance.IsChallengeOptionCorrect(selectedIndex);
        if (PersistentModel.Instance.TireOptionSelectedData.correct)
        {
            UIManager.Instance.soundManager.PlaySound("PlayCountdownHighPitchEndVibrato");
        }
        else
        {
            UIManager.Instance.soundManager.PlaySound("PlayCountdownHighPitchEndVibrato");
        }

        // simulate continue go to race 
        OnListOptionContinueButtonClick();
    }

    private void HideListOptions()
    {
        // iterate options, cache and hide them
        float[] delay = { 0.25f, 0.35f, 0.45f, 0.55f };
        for (int i = 0; i < optionList.Length; i++)
        {
            optionList[i].GetComponent<Image>().raycastTarget = false;
            optionList[i].localScale = new Vector3(0f, 0f, 0f);

            optionList[i].GetComponent<Button>().onClick.RemoveAllListeners();
            optionList[i].GetComponent<Button>().interactable = false;

            LeanTween.scale(optionList[i], new Vector3(0f, 0f, 0f), 0.6f)
                .setOvershoot(0.95f)
                .setEase(LeanTweenType.easeOutBack)
                .setDelay(delay[i]);
        }
    }

    public Sprite SelectedBillboardSprite
    {
        get { return billboardImage.sprite; }
    }

    public Sprite SelectedTireSprite
    {
        get { return optionList[selectedIndex].Find("Image").GetComponent<Image>().sprite; }
    }

    private void OnListOptionContinueButtonClick()
    {
        root.submitButton.GetComponent<Button>().onClick.RemoveListener(OnListOptionContinueButtonClick);

        root.HideSubmitButton();

        StartTransitionOut();

        // BYPASS LETS ROLL
        root.StopClock();
        root.HideClockIsRunningSign();
        root.HideClockIsStoppedSign();
        root.StartTransitionOut();
    }

    public void Remove()
    {
        tireQuestionBar.gameObject.SetActive(false);
        customerPrefBtn.gameObject.SetActive(false);

        // iterate options and de-activate
        for (int i = 0; i < optionList.Length; i++)
        {
            optionDataList[i].transform.GetComponent<Button>().onClick.RemoveAllListeners();
            optionDataList[i].transform = null;
            optionList[i].gameObject.SetActive(false);
        }
    }
}
