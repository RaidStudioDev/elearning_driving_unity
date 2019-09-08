using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public delegate void OnScreenOverlayCallBackHandler();
public delegate void OnScreenOverlayCloseEventHandler();

public class BaseScreenOverlay : MonoBehaviour
{
    public event OnScreenOverlayCloseEventHandler OnScreenOverlayClose;
    public OnScreenOverlayCallBackHandler OnCallBackMethod;

    protected Dictionary<string, RectTransform> _elements;
    protected RectTransform[] _elementList;

    protected Color elementShowColor = new Color(1f, 1f, 1f, 1f);
    protected Color elementHideColor = new Color(1f, 1f, 1f, 0f);

    protected UIManager _ui;

    virtual protected void Awake()
    {
        _ui = UIManager.Instance;

        _elements = new Dictionary<string, RectTransform>();

        _elementList = this.GetComponentsInChildren<RectTransform>();
        foreach (RectTransform element in _elementList)
        {
            _elements.Add(element.gameObject.name, element);

            if (element.GetComponent<Button>())
            {
                AddButtonEventTrigger(element.GetComponent<Button>());
            }
        }
    }

    virtual public void Initialize()
    {

    }

    protected void AddButtonEventTrigger(Button button)
    {
        EventTrigger buttonTrigger = button.gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry pointerEnter = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerEnter
        };
        pointerEnter.callback.AddListener((e) => OnPointerEnter());
        buttonTrigger.triggers.Add(pointerEnter);

        EventTrigger.Entry pointerExit = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerExit
        };
        pointerExit.callback.AddListener((e) => OnPointerExit());
        buttonTrigger.triggers.Add(pointerExit);
    }

    protected void RemoveButtonEventTrigger(Button button)
    {
        EventTrigger buttonTrigger = button.gameObject.GetComponent<EventTrigger>();

        for (int i = 0; i < buttonTrigger.triggers.Count; i++)
        {
            EventTrigger.Entry pointerEntry = buttonTrigger.triggers[i];
            pointerEntry.callback.RemoveAllListeners();
            buttonTrigger.triggers.Remove(pointerEntry);
        }
    }

    protected void OnPointerEnter()
    {
        _ui.UpdateCursor(true);
    }

    protected void OnPointerExit()
    {
        _ui.UpdateCursor(false);
    }

    virtual protected void OnCloseOverlay()
    {
        _ui.UpdateCursor(false);

        OnScreenOverlayClose?.Invoke();

        Remove();
    }

    virtual public void Remove()
    {
        foreach (RectTransform element in _elementList)
        {
            if (element.GetComponent<Button>())
                RemoveButtonEventTrigger(element.GetComponent<Button>());
        }
        _elementList = null;
        _elements.Clear();

        OnCallBackMethod = null;

        this.transform.SetParent(null);

        Destroy(gameObject);

        _ui = null;
    }
}