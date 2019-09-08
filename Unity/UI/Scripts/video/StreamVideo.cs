using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class StreamVideo : MonoBehaviour 
{
	public RawImage rawImage;	// we render video to this
	public Button videoButton;
	public Button videoIcons;
	public Text loadingMessage;
	public bool IsAutoPlay = true;
	public string Url = "";

	private RectTransform _rawImageRect;
	private VideoPlayer _videoPlayer;
	private VideoSource _videoSource;
	private AudioSource _audioSource;
	private string _url;

	private Color elementShowing = new Color (1f, 1f, 1f, 1f); 
	private Color elementHidden = new Color (1f, 1f, 1f, 0f); 
	private Color elementPaused = new Color (0.25f, 0.25f, 0.25f, 1f); 

	private string[] _videoUrls = { "https://www.lms.mybridgestoneeducation.com/Switchback/AssetVideos/big_buck_bunny.mp4"
		, "https://www.lms.mybridgestoneeducation.com/Switchback/AssetVideos/blizzak.mp4" };

	void Awake()
	{
		if (rawImage != null) rawImage.gameObject.SetActive (false);

		ColorBlock btnColors = new ColorBlock ();
		btnColors.normalColor = elementHidden;
		btnColors.pressedColor = elementHidden;
		btnColors.highlightedColor = elementHidden;
		btnColors.disabledColor = elementHidden;
		if (videoButton != null)  videoButton.colors = btnColors;
	}

	void Start () 
	{
		// initialize
		if (rawImage != null) 
		{
			rawImage.color = elementHidden;
			_rawImageRect = rawImage.GetComponent<RectTransform>();
		}

		if (videoIcons != null && IsAutoPlay) videoIcons.gameObject.SetActive(false);
		if (loadingMessage != null) loadingMessage.color = elementHidden;

		if (videoButton != null) 
		{
			videoButton.onClick.AddListener (OnVideoButtonClick);
		}

		int urlIndex = Random.Range(0, (_videoUrls.Length - 1));

		if (string.IsNullOrEmpty(Url)) Url = _videoUrls[urlIndex];

		if (IsAutoPlay) StartVideo (Url);
	}

	private void OnVideoButtonClick()
	{
		// return;

		DisableVideoButton();

		ResetPlayIcon();

		// if videoPlayer exists we have already preloaded
		if (_videoPlayer != null) 
		{
			if (_videoPlayer.isPlaying) 
			{
				Pause();

				LeanTween.color(_rawImageRect, elementPaused, 0.75f).setEaseOutCubic();
				EnablePlayIcon();
				videoIcons.GetComponent<RectTransform>().localScale = new Vector3(0f, 0f, 1f);
				LeanTween.scale (videoIcons.gameObject, new Vector3 (1f, 1f, 1f), 0.5f)
					.setEaseOutBack()
					.setOvershoot (2.50f)
					.setDelay(0.25f)
					.setOnComplete(EnableVideoButton);
			} 
			else 
			{
				LeanTween.color(_rawImageRect, elementShowing, 0.75f);
				DisablePlayIcon();
				Play();
				LeanTween.delayedCall (0.5f, EnableVideoButton);
			}
		}
		else // prepare video and play 
		{
			DisablePlayIcon();

			StartVideo(Url);
		}
	}

	public void StartVideo(string url)
	{
		_url = url;

		DisablePlayIcon();

		StartCoroutine (prepareVideo());
	}

	IEnumerator prepareVideo()
    {
		LeanTween.alphaText(loadingMessage.GetComponent<RectTransform> (), 1f, 0.75f).setLoopPingPong ().setEaseInBack();

		// wait a sec before warming up video
		yield return new WaitForSeconds (1f);

		// hide video to prevent flickering
		rawImage.color = elementHidden;

        // add VideoPlayer to the GameObject
        _videoPlayer = gameObject.AddComponent<VideoPlayer>();

        // add AudioSource
		_audioSource = gameObject.AddComponent<AudioSource>();

        // disable Play on Awake for both Video and Audio
		_videoPlayer.playOnAwake = false;
		_audioSource.playOnAwake = false;
		_audioSource.Pause();

		// video clip from Url
		_videoPlayer.source = VideoSource.Url;
		_videoPlayer.url = _url; // "http://www.quirksmode.org/html5/videos/big_buck_bunny.mp4";

        // set Audio Output to AudioSource
		_videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;

        // assign the Audio from Video to AudioSource to be played
		_videoPlayer.controlledAudioTrackCount = 1;
		_videoPlayer.EnableAudioTrack(0, true);
		_videoPlayer.SetTargetAudioSource(0, _audioSource);

		_videoPlayer.prepareCompleted += Prepared;
		_videoPlayer.Prepare();

		yield return null;
    }

	private void Prepared(VideoPlayer vPlayer) 
	{
		LeanTween.cancel(loadingMessage.gameObject);
		LeanTween.alphaText(loadingMessage.GetComponent<RectTransform> (), 0f, 0.95f).setEaseOutCubic();

		if (rawImage != null) rawImage.gameObject.SetActive (true);

		// assign the Texture from Video to RawImage to be displayed
		rawImage.texture = _videoPlayer.texture;

		// play audio and video
		_videoPlayer.Play();
		_audioSource.Play();

		// show video
		LeanTween.color(_rawImageRect, elementShowing, 0.75f)
			.setDelay(0.5f)
			.setOnComplete(EnableVideoButton);

		// add on finish video event
		_videoPlayer.loopPointReached += LoopPointReached;

		// StartCoroutine(TrackProgress ());
	}

	private void LoopPointReached (VideoPlayer source)
	{
		Debug.Log("LoopPointReached()");

		DisableVideoButton();

		LeanTween.color(_rawImageRect, elementPaused, 0.75f).setEaseOutCubic();
		EnableRePlayIcon();
		videoIcons.GetComponent<RectTransform>().localScale = new Vector3(0f, 0f, 1f);
		LeanTween.scale (videoIcons.gameObject, new Vector3 (1f, 1f, 1f), 0.5f)
			.setEaseOutBack()
			.setOvershoot (2.50f)
			.setDelay(0.25f)
			.setOnComplete(EnableVideoButton);
	}

	private IEnumerator TrackProgress()
	{
		while (_videoPlayer.isPlaying)
		{
			// Debug.LogWarning("Video Time: " + Mathf.FloorToInt((float)_videoPlayer.time));

			yield return null;
		}
	}

	public void EnablePlayIcon()
	{
		if (videoIcons != null) 
		{
			videoIcons.interactable = true;
			videoIcons.gameObject.SetActive(true);
		}
	}

	public void EnableRePlayIcon()
	{
		if (videoIcons != null) 
		{
			videoIcons.interactable = false;
			videoIcons.gameObject.SetActive(true);
		}
	}

	public void ResetPlayIcon()
	{
		if (videoIcons != null) 
		{
			videoIcons.interactable = true;
		}
	}

	public void DisablePlayIcon()
	{
		if (videoIcons != null) 
		{
			videoIcons.gameObject.SetActive(false);
		}
	}

	public void EnableVideoButton()
	{
		if (videoButton != null) videoButton.interactable = true;
	}

	public void DisableVideoButton()
	{
		if (videoButton != null) videoButton.interactable = false;
	}

	public void Play()
	{
		if (_videoPlayer == null) return;
		
		_videoPlayer.Play();
	}

	public void Pause()
	{
		if (_videoPlayer == null) return;

		_videoPlayer.Pause();
	}

	public void Remove()
	{
		if (videoButton != null) 
		{
			videoButton.onClick.RemoveListener(OnVideoButtonClick);

			rawImage.color = elementHidden;
			rawImage.texture = null;
		}

		if (_videoPlayer != null) 
		{
			_videoPlayer.prepareCompleted -= Prepared;
			_videoPlayer.loopPointReached -= LoopPointReached;
			_videoPlayer.enabled = false;
		}

	}

}
