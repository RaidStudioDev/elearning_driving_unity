using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class AlertPanelOverlay : BaseScreenOverlay
{
    [HideInInspector] public string BodyMessage = "BodyMessage";
    [HideInInspector] public string ErrorMessage = "ErrorMessage";
    [HideInInspector] public string ButtonLabel = "OK";
     
    private Text _bodyMessageTxt;
    private Text _errorMessageTxt;
    private Button _button;

    protected override void Awake()
    {
        base.Awake();

        _bodyMessageTxt = _elements["BodyMessage"].GetComponent<Text>();
        _errorMessageTxt = _elements["ErrorMessage"].GetComponent<Text>();
        _button = _elements["Button"].GetComponent<Button>();

        LeanTween.alphaText(_elements["BodyMessage"], 0f, 0f);
        LeanTween.alphaText(_elements["ErrorMessage"], 0f, 0f);

        _elements["Button"].localScale = new Vector3(0f, 0f, 1f);
    }

    public override void Initialize()
    {
        _bodyMessageTxt.text = BodyMessage;
        _errorMessageTxt.text = ErrorMessage;
        _button.GetComponentInChildren<Text>().text = ButtonLabel;

        LeanTween.alphaText(_elements["BodyMessage"], 1f, 0.55f).setEase(LeanTweenType.easeOutCubic).setDelay(0.25f).setIgnoreTimeScale(true);
        LeanTween.alphaText(_elements["ErrorMessage"], 1f, 0.55f).setEase(LeanTweenType.easeOutCubic).setDelay(0.55f).setIgnoreTimeScale(true);

        LeanTween.scale(_elements["Button"], new Vector3(1f, 1f, 1f), 0.55f).setDelay(0.85f)
            .setOvershoot(0.75f)
            .setIgnoreTimeScale(true)
            .setEase(LeanTweenType.easeOutBack)
            .setOnComplete(() => {

                AddEvents();

            });
    }

    private void AddEvents()
    {
        _button.onClick.AddListener(OnButtonClick);
    }

    private void RemoveEvents()
    {
        _button.onClick.RemoveListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        UIManager.Instance.soundManager.PlaySound("PlaySineWaveHighPitch");

        //OnCallBackMethod?.Invoke();

        base.OnCloseOverlay();
    }

    public override void Remove()
    {
        RemoveEvents();

        base.Remove();
    }
}