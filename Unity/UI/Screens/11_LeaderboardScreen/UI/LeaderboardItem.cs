using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderboardItem : MonoBehaviour {

	private LBPanel _panel;
	private LBRankText _rankText;
	private LBNameText _nameText;
	private LBTimeText _timeText;

    public LBPanel LBPanel { get { return _panel; } }
    public LBRankText LBRank { get { return _rankText; } }
	public LBNameText LBName { get { return _nameText; } }
	public LBTimeText LBTime { get { return _timeText; } }

    public Color panel
    {
        set { _panel.color = value; }
    }

    public string rankText
	{
		set { _rankText.text = value; }
	}

	public string nameText
	{
		set { _nameText.text = value; }
	}

	public string timeText
	{
		set { _timeText.text = value; }
	}

	void Awake()
	{
        _panel = this.transform.GetComponentInChildren<LBPanel>();
		_rankText = this.transform.GetComponentInChildren<LBRankText>();
		_nameText = this.transform.GetComponentInChildren<LBNameText>();
		_timeText = this.transform.GetComponentInChildren<LBTimeText>();
	}
}
