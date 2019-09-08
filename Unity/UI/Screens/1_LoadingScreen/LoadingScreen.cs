using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DentedPixel;

public class LoadingScreen : BaseScreen {

    private RectTransform leftSidePanel;
    private RectTransform rightSidePanel;
    private RectTransform loadingBar;
    private RectTransform logo;
    private RectTransform spinningTire;
    private RectTransform bridgestoneLogo;

    public override void PreInitialize()
    {
        showProgressLoadingPanel = true;
        showSmallProgressLoadingPanel = false;

        // Prevent Side Panels to be faded in - called in base
        _isTransitioningSlidePanels = false;    

        _screenElements = new Dictionary<string, RectTransform>();

        // Grab all the screen elements, we will clear when Remove() is called.
        RectTransform[] elementList = this.GetComponentsInChildren<RectTransform>();
        foreach (RectTransform element in elementList)
        {
            _screenElements.Add(element.gameObject.name, element);

            if (element.GetComponent<BaseScreen>() == null)
            {
                if (element.gameObject.name != "LeftSideBg" && element.gameObject.name != "RightSideBg")
                {
                    element.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
                }
            }
        }
    }

    public override void Initialize(string id)
    {
        base.Initialize(id);

        leftSidePanel = _screenElements["LeftSideBg"];
        rightSidePanel = _screenElements["RightSideBg"];
        logo = _screenElements["SuperTireSmallLogo"];
        bridgestoneLogo = _screenElements["BridgestoneLogo"];
  
        // Slide In
        StartBackgroundSlideInTransition();
    }
    
    void StartBackgroundSlideInTransition()
    {
		LeanTween.delayedCall(0.25f, () => {
            UIManager.Instance.soundManager.PlaySound("PlaySawLowHighTone");
        });

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
        
        LeanTween.delayedCall(0.85f, BackgroundSlideInTransitionComplete);
    }

    void BackgroundSlideInTransitionComplete()
    {
        // Clear Tweens
        base.ClearSidePanelTweens();
        
        LeanTween.alpha(logo, 1f, 3f).setEase(LeanTweenType.easeOutQuad);
        LeanTween.scale(logo, new Vector3(1f, 1f, 1f), 0.85f).setEase(LeanTweenType.easeOutBack).setOvershoot(1.25f);

        // Clear BR Logo Tween
        LeanTween.cancel(bridgestoneLogo.gameObject);
        
        // Set Progress Complete Event 
        OnProgressLoadingTransitionInComplete += ProgressLoadingTransitionInComplete;

        // Fade in Logo and Open Load Panel segment
        LeanTween.alpha(bridgestoneLogo, 1f, 1f)
            .setDelay(0.5f)
            .setEase(LeanTweenType.easeOutQuad).setFrom(0f)
            .setOnComplete(()=> {

                base.OpenLoadingPanel();

            });
    }

    protected override void ProgressLoadingTransitionInComplete()
    {
        OnProgressLoadingTransitionInComplete -= ProgressLoadingTransitionInComplete;

        UIManager.Instance.StartupLoadComplete();
    }

    public override void CloseLoadingPanel()
    {
        LeanTween.alpha(bridgestoneLogo, 0f, 1f).setDelay(0.1f).setEase(LeanTweenType.easeOutQuad);
        LeanTween.alpha(logo, 0f, 1f)
            .setDelay(0.1f)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnComplete(()=>{

                LeanTween.cancel(logo);

                base.CloseLoadingPanel();
            });
    }

    public override void Draw()
    {

    }

    public override void Remove()
    {
        // make sure to clear tweens, just in case
        LeanTween.cancel(logo);
        LeanTween.cancel(bridgestoneLogo);

        // Remove elements from list
        base.Remove();
    }
}
