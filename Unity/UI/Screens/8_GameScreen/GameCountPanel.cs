using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// complete triggers when showing GO
public delegate void OnGameCountCompleteEventHandler();

// finished triggers when GO's animation is finished
public delegate void OnGameCountFinishedEventHandler();

public class GameCountPanel : MonoBehaviour {

    private Dictionary<string, RectTransform> _countElements;
    private int _currentCountIndex = 0;

    public event OnGameCountCompleteEventHandler OnGameCountComplete;
    public event OnGameCountFinishedEventHandler OnGameCountFinished;
    
    void Awake()
    {
        _countElements = new Dictionary<string, RectTransform>();

        RectTransform[] countList = this.GetComponentsInChildren<RectTransform>();
        foreach (RectTransform element in countList)
        {
            _countElements.Add(element.gameObject.name, element);
        }
    }

    public void Initialize()
    {
        StartCoroutine(StartCount());
    }

    IEnumerator StartCount()
    {
        float countDelay = 1f;
        float hideDelay = 0.15f;

        // Count 3
        _currentCountIndex++;

		UIManager.Instance.soundManager.PlaySound("PlayCountdown");

        ShowCount(_currentCountIndex);

        yield return new WaitForSeconds(countDelay);

        HideCount(_currentCountIndex);

        yield return new WaitForSeconds(hideDelay);

        // Count 2
        _currentCountIndex++;

        UIManager.Instance.soundManager.PlaySound("PlayCountdown");

        ShowCount(_currentCountIndex);

        yield return new WaitForSeconds(countDelay);

        HideCount(_currentCountIndex);

        yield return new WaitForSeconds(hideDelay);

        // Count 1
        _currentCountIndex++;

        UIManager.Instance.soundManager.PlaySound("PlayCountdown");

        ShowCount(_currentCountIndex);

        yield return new WaitForSeconds(countDelay);

        HideCount(_currentCountIndex);

        yield return new WaitForSeconds(hideDelay);

        UIManager.Instance.soundManager.PlaySound("PlayCountdownGo", 0.75f);
        UIManager.Instance.soundManager.PlaySound("PlayCountdownHighPitchEndVibrato");
   
        // Count Complete - Show GO!
        ShowGo();
    }

    void ShowCount(int index)
    {
        LeanTween.scale(_countElements["Count_" + index], new Vector3(1f, 1f, 1f), 0.65f)
        .setDelay(0f)
        .setEase(LeanTweenType.easeOutBack);
    }

    void HideCount(int index)
    {
        LeanTween.rotateLocal(_countElements["Count_" + index].gameObject, new Vector3(1f, 1f, -45f), 0.95f)
        .setEase(LeanTweenType.easeInBack)
        .setDelay(0.5f);

        LeanTween.scale(_countElements["Count_" + index], Vector3.zero, 0.85f)
        .setDelay(0f)
        .setEase(LeanTweenType.easeInBack);
    }

    void ShowGo()
    {
        // Trigger Game Count Completed
        OnGameCountComplete();

        LeanTween.scale(_countElements["Go"], new Vector3(1f, 1f, 1f), 0.65f)
        .setDelay(0f)
        .setOvershoot(1.25f)
        .setEase(LeanTweenType.easeOutBack)
        .setOnComplete(()=>
        {
            LeanTween.rotateLocal(_countElements["Go"].gameObject, new Vector3(1f, 1f, -45f), 0.65f)
            .setEase(LeanTweenType.easeInBack)
            .setDelay(0.25f);

            LeanTween.scale(_countElements["Go"], Vector3.zero, 0.75f)
            .setDelay(0.25f)
            .setEase(LeanTweenType.easeInBack)
            .setOvershoot(1.25f)
            .setOnComplete(() =>
            {
                // Trigger Game Count Finished
                OnGameCountFinished();
                
            });
        });
    }

    public void Remove()
    {
        LeanTween.cancel(_countElements["Count_1"].gameObject);
        LeanTween.cancel(_countElements["Count_2"].gameObject);
        LeanTween.cancel(_countElements["Count_3"].gameObject);
        LeanTween.cancel(_countElements["Go"].gameObject);

        _countElements.Clear();
    }
}
