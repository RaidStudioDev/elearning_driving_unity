using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LBNameText : MonoBehaviour 
{
	private Text _thisText;
	private string _text;
	public string text 
	{
		get {
			return _text;
		}

		set {

			_text = value;
			_thisText.text = _text;
		}
	}

	public Color color
	{
		set { _thisText.color = value; }
	}

	public int fontSize
	{
		set { _thisText.fontSize = value; }
	}

	void Awake()
	{
		_thisText = this.transform.GetComponent<Text>();
	}
}
