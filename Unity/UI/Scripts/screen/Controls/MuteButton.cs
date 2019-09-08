using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MuteButton : MonoBehaviour {

    public Color elementShowColor = new Color(1f, 1f, 1f, 1f);

    public RectTransform muteButtonOff;
    public RectTransform muteButtonOn;

    // Use this for initialization
    private void Awake ()
    {
        // check if music has initialized and loaded first
        bool hasInitialized = UIManager.Instance.soundManager.mPlayer.HasInitialized;
        bool isPlaying = UIManager.Instance.soundManager.mPlayer.isPlaying;

        if (hasInitialized)
        {
            if (isPlaying) SetMuteOnButton();
            else SetMuteOffButton();
        }
        else
        {
            // music not loaded yet, default to show mute on
            SetMuteOnButton();
        }

        muteButtonOff.GetComponent<Image>().color = elementShowColor;
        muteButtonOn.GetComponent<Image>().color = elementShowColor;
    }
	
    // mute sound
	void OnMuteOnButtonClick()
    {
        // mute all sounds
        AudioListener.volume = 0;

        // lets us control music once it loads track
        UIManager.Instance.soundManager.mPlayer.queueIsPlaying = false;

        //  DebugLog.Trace("mPlayer.isPlaying:" + UIManager.Instance.soundManager.mPlayer.isPlaying);
        if (UIManager.Instance.soundManager.mPlayer.isPlaying)
        {
            UIManager.Instance.soundManager.mPlayer.StopTrack();
        }

        SetMuteOffButton();
    }

    // turn sound back on
    void OnMuteOffButtonClick()
    {
        // turn on all sounds
        AudioListener.volume = 1;

        DebugLog.Trace("OnMuteOffButtonClick.isPlaying:" + UIManager.Instance.soundManager.mPlayer.isPlaying);

        // lets us control music once it loads track
        UIManager.Instance.soundManager.mPlayer.queueIsPlaying = true;

        // play random track
        int[] trackIndexes = new int[] { 1, 5 };
        trackIndexes.ShuffleCrypto();
        UIManager.Instance.soundManager.mPlayer.PlayTrack(trackIndexes[0], true);

        // show on mute
        SetMuteOnButton();
    }

    void SetMuteOnButton()
    {
        muteButtonOff.GetComponent<Button>().onClick.RemoveListener(OnMuteOffButtonClick);
        muteButtonOff.gameObject.SetActive(false);
        muteButtonOn.gameObject.SetActive(true);
        muteButtonOn.GetComponent<Button>().onClick.AddListener(OnMuteOnButtonClick);
    }

    void SetMuteOffButton()
    {
        muteButtonOn.GetComponent<Button>().onClick.RemoveListener(OnMuteOnButtonClick);
        muteButtonOn.gameObject.SetActive(false);
        muteButtonOff.gameObject.SetActive(true);
        muteButtonOff.GetComponent<Button>().onClick.AddListener(OnMuteOffButtonClick);
    }

    public void Remove()
    {
        muteButtonOff.GetComponent<Button>().onClick.RemoveListener(OnMuteOffButtonClick);
        muteButtonOn.GetComponent<Button>().onClick.RemoveListener(OnMuteOnButtonClick);
    }
}
