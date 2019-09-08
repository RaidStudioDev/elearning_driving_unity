using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InGameAlertOverlay : BaseScreenOverlay
{
    private Text _bodyMessageTxt;
    public string BodyText { get;  set; }
    
    protected override void Awake()
    {
        base.Awake();

        _bodyMessageTxt = _elements["BodyMessage"].GetComponent<Text>();
        _bodyMessageTxt.color = elementHideColor;

        //_button = _elements["Button"].GetComponent<Button>();

        transform.localScale = new Vector3(0f, 1f, 1f);

        _elements["Panel"].localScale = new Vector3(0f, 1f, 1f);
        _elements["Button"].localScale = new Vector3(0f, 0f, 1f);
    }

    // Use this for initialization
    public override void Initialize()
    {
        _bodyMessageTxt.text = BodyText;

        LeanTween.scale(transform.GetComponent<RectTransform>(), new Vector3(1f, 1f, 1f), 0.5f)
            .setDelay(0.0f)
            .setOvershoot(0.5f)
            .setEase(LeanTweenType.easeOutBack).setIgnoreTimeScale(true);

        LeanTween.scale(_elements["Panel"], new Vector3(1f, 1f, 1f), 0.5f)
            .setDelay(0.25f)
            .setOvershoot(0.25f)
            .setEase(LeanTweenType.easeOutBack);// .setIgnoreTimeScale(true);

        LeanTween.alphaText(_elements["BodyMessage"], 1f, 0.55f)
            .setEase(LeanTweenType.easeOutCubic)
            .setDelay(0.35f);//.setIgnoreTimeScale(true);

        LeanTween.scale(_elements["Button"], new Vector3(1f, 1f, 1f), 0.55f).setDelay(0.35f)
            .setOvershoot(0.45f)
            .setIgnoreTimeScale(true)
            .setEase(LeanTweenType.easeOutBack)
            .setOnComplete(() => {
                _elements["Button"].GetComponent<Button>().onClick.AddListener(OnButtonClick);
            });
    }

    void OnButtonClick()
    {
        UIManager.Instance.soundManager.PlaySound("PlaySineWaveHighPitch");

        _elements["Button"].GetComponent<Button>().onClick.RemoveListener(OnButtonClick);

        base.OnCloseOverlay();
    }

    public override void Remove()
    {
        base.Remove();
    }
}
