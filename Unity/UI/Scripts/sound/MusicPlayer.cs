using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class MusicPlayer {

    private List<string> playlist;
    private GameObject trackGameObject;

    public bool HasInitialized = false;
    public bool isPlaying = false;
    public bool queueIsPlaying = true;
    private bool isEnabled = true;

    public MusicPlayer(bool isMusicEnabled)
    {
        isEnabled = isMusicEnabled;

        if (!isEnabled) DebugLog.Trace("MusicPlayer Disabled()");

        CreateMusicObject();

        playlist = new List<string>();
    }

    private GameObject mPlayerGO;
    private void CreateMusicObject()
    {
        mPlayerGO = new GameObject("MusicPlayer");
        mPlayerGO.AddComponent<AudioSource>();

        mPlayerGO.transform.SetParent(UIManager.Instance.transform);
        mPlayerGO.transform.SetAsLastSibling();
    }

    public void Update(float gT)
    {

    }

    public void AddSong(string songID)
    {
        playlist.Add(songID);
    }

    //string transitionTrackAssetID;
    string currentTrackAssetID;
    int currentSongIndex;

    public void PlayTrack(int songIndex, bool fade = true)
    {
        if (!isEnabled) return;

        currentSongIndex = songIndex;

        //transitionTrackAssetID = playlist[songIndex];

        if (mPlayerGO.GetComponent<AudioSource>().clip != null)
        {
            if (fade) UIManager.Instance.StartCoroutine(StartTransitionIn());
            else
            {
                StopTrack();
                UIManager.Instance.StartCoroutine(PrepareToPlay());
            }
        }
        else UIManager.Instance.StartCoroutine(PrepareToPlay()); 
    }

    private IEnumerator StartTransitionIn()
    {
        LeanTween.value(0.25f, 0f, 1.5f)
        .setDelay(0.0f)
        .setEase(LeanTweenType.easeOutQuad)
        .setOnUpdate((float val) =>
        {
            mPlayerGO.GetComponent<AudioSource>().volume = val;
        })
        .setOnComplete(() =>
        {
            StopTrack();
            UIManager.Instance.StartCoroutine(PrepareToPlay());
        });
       
        yield return 0;
    }

    public void StopTrack()
    {
        isPlaying = false;
        mPlayerGO.GetComponent<AudioSource>().Stop();
    }

    private IEnumerator PrepareToPlay()
    {
        if (Application.platform != RuntimePlatform.WebGLPlayer)
        {
            string assetId = playlist[currentSongIndex] + ".ogg";
            // string url = "https://raidpr.secure.omnis.com/clients/demo/br/sb/Music/" + assetId;
            string url = "https://www.lms.mybridgestoneeducation.com/Switchback/Music/" + assetId;

            DownloadHandlerAudioClip downloadHandler = new DownloadHandlerAudioClip(url, AudioType.OGGVORBIS);
            downloadHandler.streamAudio = true;
            UnityWebRequest request = new UnityWebRequest(url)
            {
                downloadHandler = downloadHandler,
                certificateHandler = new SSLAuth()
            };
            request.SendWebRequest();

            while (!request.isDone)
            {
                yield return null;
            }

            if (queueIsPlaying)
            {
                AudioClip clipa = DownloadHandlerAudioClip.GetContent(request);
                mPlayerGO.GetComponent<AudioSource>().clip = clipa;
                mPlayerGO.GetComponent<AudioSource>().volume = 0.25f;
                mPlayerGO.GetComponent<AudioSource>().Play();

                isPlaying = true;
            }

            HasInitialized = true;
        }
        else UIManager.Instance.StartCoroutine(LoadTrack(currentSongIndex));
    }

    private IEnumerator LoadTrack(int songIndex)
    {
        // load track
        string assetId = playlist[songIndex] + ".mp3";
        // string url = "https://raidpr.secure.omnis.com/clients/demo/br/sb/Music/" + assetId;
        // string url = "https://www.lms.mybridgestoneeducation.com/Switchback/Music/" + assetId;
        string url = GetURL() + assetId;
        // Debug.Log("LoadTrack.url: " + url);

        DownloadHandlerAudioClip downloadHandler = new DownloadHandlerAudioClip(url, AudioType.MPEG);
        downloadHandler.streamAudio = true;
        UnityWebRequest request = new UnityWebRequest(url)
        {
            downloadHandler = downloadHandler,
            certificateHandler = new SSLAuth()
        };
        request.SendWebRequest();

        while (!request.isDone)
        {
            // Debug.Log("request.downloadProgress: " + request.downloadProgress);
            yield return new WaitForSeconds(0.1f);

        }

        if (queueIsPlaying)
        {
            AudioClip clipa = DownloadHandlerAudioClip.GetContent(request);
            mPlayerGO.GetComponent<AudioSource>().clip = clipa;
            mPlayerGO.GetComponent<AudioSource>().volume = 0.25f;
            mPlayerGO.GetComponent<AudioSource>().Play();

            isPlaying = true;
        }

        HasInitialized = true;
    }

    public void PreloadTrack(int songIndex)
    {
        UIManager.Instance.StartCoroutine(PreloadPrepareTrack(songIndex));
    }

    private string GetURL()
    {
        var str = Application.absoluteURL;
        var index = str.LastIndexOf('/');
        str = str.Substring(0, index + 1) + "Music/";

        Console.WriteLine(str);
        return str;
    }

    private IEnumerator PreloadPrepareTrack(int songIndex)
    {
        if (Application.platform != RuntimePlatform.WebGLPlayer)
        {
            string assetId = playlist[songIndex] + ".ogg";
            // string url = "https://raidpr.secure.omnis.com/clients/demo/br/sb/Music/" + assetId;
            string url = GetURL() + assetId;
            Debug.Log("LoadTrack.url: " + url);

            WWW www = WWW.LoadFromCacheOrDownload(url, 0);// new WWW(url);
            AudioClip clip = www.GetAudioClip(false, true);

            while (clip.loadState != AudioDataLoadState.Loaded)
            // while (!clip.isReadyToPlay)
            {
                //Debug.Log("Waiting");
                yield return 0;
            }
        }
        else
        {
            string assetId = playlist[songIndex] + ".mp3";
            // string url = "https://raidpr.secure.omnis.com/clients/demo/br/sb/Music/" + assetId;
            string url = GetURL() + assetId;
            Debug.Log("LoadTrack.url: " + url);

            DownloadHandlerAudioClip downloadHandler = new DownloadHandlerAudioClip(url, AudioType.MPEG);
            downloadHandler.streamAudio = true;
            UnityWebRequest request = new UnityWebRequest(url)
            {
                downloadHandler = downloadHandler,
                certificateHandler = new SSLAuth()
            };
            request.SendWebRequest();

            while (!request.isDone)
            {
                //Debug.Log("request.downloadProgress: " + request.downloadProgress);
                yield return new WaitForSeconds(0.1f);

            }


        }
    }
}
