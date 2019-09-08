using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public delegate void OnClickEventHandler(int index);

public class SlideIndicator : MonoBehaviour {

	public event OnClickEventHandler OnClick;
	
	private Button _baseSlideItem;
	private GameObject[] _slideItemGameObjs;

	private int _currentSlideIndex = 0;
	private int _prevSlideIndex = 0;

	public List<int> DataProvider;

    private Color _alphaHideElement = new Color(1, 1, 1, 0);
    private Vector3 _scaleShowElement = new Vector3(1, 1, 1);
	private Vector3 _scaleHideElement = new Vector3(0, 0, 1);

	// Use this for initialization
	void Start () 
	{
		// first get a copy of the indicator button
		_baseSlideItem = this.transform.GetComponentInChildren<Button>();
		_baseSlideItem.GetComponent<Image>().color = _alphaHideElement;
	}

	public void Initialize(BaseScreen baseScreen)
	{
		if (DataProvider == null) return; 

		// RemoveTempItems();

		_slideItemGameObjs = new GameObject[DataProvider.Count];

		float delay = 0.2f;
		for (int i = 0; i < _slideItemGameObjs.Length; i++) 
		{
			Button slideItemBtn = (i == 0) ? _baseSlideItem : (Instantiate(_baseSlideItem.gameObject) as GameObject).GetComponent<Button>();

			slideItemBtn.transform.localScale = _scaleHideElement;
			slideItemBtn.transform.SetParent(this.transform, false);
			int index = DataProvider[i];
			slideItemBtn.onClick.AddListener(() => OnSlideItemButtonClick(index));

			_slideItemGameObjs[i] = slideItemBtn.gameObject;

			LeanTween.scale(slideItemBtn.GetComponent<RectTransform>(), _scaleShowElement, 0.75f).setEaseInOutBack().setDelay(delay).setOvershoot(0.75f);
			LeanTween.alpha(slideItemBtn.GetComponent<RectTransform>(), 1f, 0.25f).setEaseOutQuad().setDelay(delay);

            baseScreen.AddButtonEventTrigger(slideItemBtn);

            delay += i * 0.03f;
		}

		SetIndicatorByIndex(0);
	}

	void OnSlideItemButtonClick(int index)
	{
		UIManager.Instance.soundManager.PlaySound("PlaySineWaveHighPitch");

        _prevSlideIndex = _currentSlideIndex;
		_currentSlideIndex = index;

		UpdateIndicators();

		if (OnClick != null) OnClick(_currentSlideIndex);
	}

	public void PrevItem(int index)
	{
		if (_currentSlideIndex > 0) 
		{
			_prevSlideIndex = _currentSlideIndex;
			_currentSlideIndex--;
		}

		UpdateIndicators();
	}

	public void NextItem(int index)
	{
		_prevSlideIndex = _currentSlideIndex;

		if (_currentSlideIndex == DataProvider.Count - 1) 
		{
			_currentSlideIndex = 0;
		}
		else 
		{
			_currentSlideIndex++;
		}

		UpdateIndicators();
	}

	void UpdateIndicators()
	{
		_slideItemGameObjs[_prevSlideIndex].GetComponent<Button>().interactable = true;
		_slideItemGameObjs[_currentSlideIndex].GetComponent<Button>().interactable = false;
	}

	void SetIndicatorByIndex(int index)
	{
		_currentSlideIndex = index;

		_slideItemGameObjs[_currentSlideIndex].GetComponent<Button>().interactable = false;
	}

	void ResetItems()
	{
		for (int i = 0; i < DataProvider.Count; i++) 
		{
			_slideItemGameObjs[i].GetComponent<Button>().interactable = true;
		}
	}

	public void Remove()
	{
		for (int i = 0; i < DataProvider.Count; i++) 
		{
			_slideItemGameObjs[i].GetComponent<Button>().onClick.RemoveAllListeners();

			Destroy(_slideItemGameObjs[i]);

			_slideItemGameObjs[i] = null;
		}
	}

	void RemoveTempItems()
	{
		Button[] children = this.transform.GetComponentsInChildren<Button>();
		
		for (int i = 1; i < children.Length; i++) 
		{
			Destroy(children[i].gameObject);
		}
	}
}
