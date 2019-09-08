using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseScreenOverlay : BaseScreenOverlay 
{
    protected override void Awake()
    {
        base.Awake();

        _elements["PausePopup"].localScale = new Vector3(0f, 1f, 1f);
        _elements["PauseButton"].localScale = new Vector3(0f, 0f, 1f);

        LeanTween.alphaText(_elements["PauseText"], 0f, 0f);
    }

    // Use this for initialization
    public override void Initialize() 
	{
        // Stop Game
        Time.timeScale = 0;

        // If on Android, show text regarding the back button exit
        #if UNITY_ANDROID
        if (Application.platform == RuntimePlatform.Android)
        {
            _elements["PauseText"].GetComponent<Text>().text = "PRESS PLAY TO CONTINUE\nPRESS BACK BUTTON AGAIN TO EXIT GAME";
        }
        #endif

        LeanTween.scale(_elements["PausePopup"], new Vector3(1f, 1f, 1f), 0.5f)
			.setDelay(0.1f)
			.setOvershoot (0.5f)
			.setEase(LeanTweenType.easeOutBack).setIgnoreTimeScale(true);

		LeanTween.alphaText(_elements["PauseText"], 1f, 0.55f).setEase(LeanTweenType.easeOutCubic).setDelay(0.25f).setIgnoreTimeScale(true);
	
		LeanTween.scale(_elements["PauseButton"], new Vector3(1f, 1f, 1f), 0.55f).setDelay(0.35f)
			.setOvershoot(0.75f)
			.setIgnoreTimeScale(true)
			.setEase(LeanTweenType.easeOutBack)
			.setOnComplete(() => {
				_elements["PauseButton"].GetComponent<Button>().onClick.AddListener(OnPauseButtonClick);
			});
    }

	void OnPauseButtonClick()
	{
		UIManager.Instance.soundManager.PlaySound("PlaySineWaveHighPitch");

        Time.timeScale = 1;

        _elements["PauseButton"].GetComponent<Button>().onClick.RemoveListener(OnPauseButtonClick);

        base.OnCloseOverlay();
    }

	public override void Remove()
	{
        base.Remove();
	}
}
