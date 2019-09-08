using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LetsRollSection : MonoBehaviour
{
    private SelectSequenceScreen root;
    private RectTransform section;

    private Vector3 barPanelTo;

    private RectTransform titleContainer;
    private RectTransform subTitleText;

    private Image selectedTireImage;
    private Image billboardImage;

    private RectTransform raceTrackPanel;
    private RectTransform raceTrackBanner;
    private RectTransform raceTrackBannerGlow;
    private RectTransform trackRecordCircle;
    private RectTransform trackRecordText;

    private bool hasInitialized = false;
    private readonly float titleTopPosition = 0.0f;
    private readonly float titleMidPosition = -365.0f;

    public void Initialize()
    {
        root = this.transform.parent.parent.GetComponent<SelectSequenceScreen>();

        section = root.sections.Find("LetsRoll").GetComponent<RectTransform>();

        // title red bar
        titleContainer = section.Find("TitleContainer").GetComponent<RectTransform>();

        // subtitle
        subTitleText = section.Find("SubTitleText").GetComponent<RectTransform>();

        // race track banner
        raceTrackPanel = section.Find("RaceTrackPanel").GetComponent<RectTransform>();
        raceTrackBanner = section.Find("RaceTrackPanel").Find("RaceTrackBanner").GetComponent<RectTransform>();
        raceTrackBannerGlow = section.Find("RaceTrackPanel").Find("RaceTrackBannerGlow").GetComponent<RectTransform>();
        trackRecordCircle = section.Find("RaceTrackPanel").Find("TrackRecordCircle").GetComponent<RectTransform>();
        trackRecordText = trackRecordCircle.Find("TrackRecordText").GetComponent<RectTransform>();

        // main header image
        billboardImage = section.Find("BillboardContainer").GetComponent<Image>();
        selectedTireImage = section.Find("SelectedTire").GetComponent<Image>();

        PrepareDraw();
    }

    public void PrepareDraw()
    {
        // reset scale if first time running
        if (hasInitialized)
        {
            // set bar to mid position 
            titleContainer.anchoredPosition3D = new Vector3(titleContainer.anchoredPosition3D.x, titleMidPosition, titleContainer.anchoredPosition3D.z);
            LeanTween.alpha(titleContainer, 0f, 0.5f).setFrom(1).setEase(LeanTweenType.easeOutCubic);
        }
        else
        {
            // hide title
            titleContainer.GetComponent<Image>().color = root.elementHideColor;
            hasInitialized = true;
        }
        
        // hide 
        subTitleText.GetComponent<Text>().color = root.elementHideColor;

        billboardImage.color = root.elementHideColor;
        selectedTireImage.color = root.elementHideColor;

        // raceTrackBanner.localScale = new Vector3(0f, 1f, 1f);
        raceTrackBanner.GetComponent<Image>().color = root.elementHideColor;
        raceTrackBannerGlow.GetComponent<Image>().color = root.elementHideColor;

        trackRecordCircle.GetComponent<Image>().color = root.elementHideColor;
        trackRecordText.GetComponent<Text>().color = root.elementHideColor;
    }

    public void UpdateBillboard(Sprite billboard, Sprite selectedTire)
    {
        billboardImage.sprite = billboard;
        selectedTireImage.sprite = selectedTire;
    }

    public void Draw()
    {
        if (hasInitialized)
        {
            // Setup track record text
            trackRecordText.GetComponent<Text>().text = PersistentModel.Instance.ChallengeRecordTime;
            TitleFadeInTransition();
        }
    }

    private void TitleFadeInTransition()
    {
        // set titleContainer from and to positions
        barPanelTo = new Vector3(titleContainer.anchoredPosition3D.x, titleTopPosition, titleContainer.anchoredPosition3D.z);

        // fade in bar 
        LeanTween.alpha(titleContainer, 1f, 0.5f)
            .setDelay(0f)
            .setEase(LeanTweenType.easeOutCubic)
            .setOnComplete(() =>
            {
                // move to top
                LeanTween.move(titleContainer, barPanelTo, 0.5f)
                    .setEase(LeanTweenType.easeOutQuad)
                    .setOnComplete(() =>
                    {
                        hasInitialized = true;
                        TransitionInCompleted();
                    });

                LeanTween.delayedCall(0.45f, () =>
                {
                    LeanTween.alphaText(subTitleText, 1f, 0.5f)
                        .setFrom(0)
                        .setEase(LeanTweenType.easeOutQuad)
                        .setDelay(0.0f);

                });
            });
    }


    private void TransitionInCompleted()
    {
        LeanTween.alpha(billboardImage.GetComponent<RectTransform>(), 1f, 0.5f)
            .setEase(LeanTweenType.easeOutQuad)
            .setFrom(0)
            .setDelay(0f);

        LeanTween.alpha(selectedTireImage.GetComponent<RectTransform>(), 1f, 0.5f)
            .setEase(LeanTweenType.easeOutQuad)
            .setFrom(0)
            .setDelay(0.25f)
            .setOnComplete(() => {
                // raceTrackBanner.GetComponent<Image>().color = root.elementShowColor;
                // raceTrackBannerGlow.GetComponent<Image>().color = root.elementShowColor;
                // trackRecordCircle.GetComponent<Image>().color = root.elementShowColor;
                // trackRecordText.GetComponent<Text>().color = root.elementShowColor;

                LeanTween.alpha(raceTrackBannerGlow, 1f, 0.5f)
                   .setEase(LeanTweenType.easeInQuad)
                   .setFrom(0)
                   .setDelay(0.0f);

                LeanTween.alpha(raceTrackBannerGlow, 0f, 0.75f)
                   .setEase(LeanTweenType.easeOutSine)
                   .setFrom(1)
                   .setDelay(0.55f);

                LeanTween.alpha(raceTrackBanner, 1f, 0.75f)
                   .setEase(LeanTweenType.easeOutSine)
                   .setFrom(0)
                   .setDelay(0.55f);

                LeanTween.alpha(trackRecordCircle, 1f, 0.75f)
                   .setEase(LeanTweenType.easeOutSine)
                   .setFrom(0)
                   .setDelay(0.5f);

                LeanTween.alpha(trackRecordText, 1f, 0.75f)
                   .setEase(LeanTweenType.easeOutSine)
                   .setFrom(0)
                   .setDelay(0.5f);

                root.UpdateSubmitButton("LET'S ROLL");
                root.submitButton.GetComponent<Button>().onClick.AddListener(OnLetsRollButtonClick);
            });
    }

    private void OnLetsRollButtonClick()
    {
        root.submitButton.GetComponent<Button>().onClick.RemoveListener(OnLetsRollButtonClick);

        root.HideSubmitButton();

        StartTransitionOut();
    }

    public void StartTransitionOut()
    {
        raceTrackBannerGlow.GetComponent<Image>().color = root.elementHideColor;
     
        LeanTween.scale(titleContainer, new Vector3(0f, 1f, 1f), 0.5f)
            .setEase(LeanTweenType.easeOutBack)
            .setDelay(0.0f);

        LeanTween.alphaText(subTitleText, 0f, 0.5f)
            .setEase(LeanTweenType.easeOutQuad)
            .setDelay(0.0f);

        LeanTween.scale(billboardImage.GetComponent<RectTransform>(), new Vector3(0f, 1f, 1f), 0.5f)
            .setEase(LeanTweenType.easeOutBack)
            .setDelay(0.15f)
            .setOnComplete(() => {

                TransitionOutCompleted();

            }); 

        LeanTween.scale(selectedTireImage.GetComponent<RectTransform>(), new Vector3(0f, 1f, 1f), 0.5f)
            .setEase(LeanTweenType.easeOutBack)
            .setDelay(0.15f);

        LeanTween.scale(raceTrackPanel, new Vector3(0f, 1f, 1f), 0.5f)
            .setEase(LeanTweenType.easeOutBack)
            .setDelay(0.25f);
            
    }

    private void TransitionOutCompleted()
    {
        root.StartTransitionOut();
    }

    public void Remove()
    {
        billboardImage.sprite = null;
        selectedTireImage.sprite = null;
    }
}
