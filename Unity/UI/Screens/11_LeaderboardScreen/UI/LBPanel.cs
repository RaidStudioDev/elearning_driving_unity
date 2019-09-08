using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LBPanel : MonoBehaviour
{
    private RectTransform _thisPanel;

    public Color color
    {
        set { _thisPanel.GetComponent<Image>().color = value; }
    }

    void Awake()
    {
        _thisPanel = this.transform.GetComponent<RectTransform>();
    }
}
