using UnityEngine;
using System.Runtime.InteropServices;	// for external Functions
using System.Collections.Generic;

/*
	Utility to handle apps from being launched from an url link
	Handles apps being launched from the background
	Works for both iOS and Android
	iOS and Android Plugins are required 

	Usage: ( Must be added to a gameobject )

	URLSchemeHandler urlSchemeHandler = GetComponent<URLSchemeHandler>();

	urlSchemeHandler.OnLaunchUrlEvent += OnLaunchUrlEvent;

	void OnLaunchUrlEvent(Dictionary<string, string> parameters) 
	{
		foreach(KeyValuePair<string,string> name in parameters)
		{
			Debug.Log(name.Key + " " + name.Value);
			Debug.Log(name.Value);
		}
	}
*/

public delegate void OnLaunchUrlEventHandler(Dictionary<string, string> parameters);

public class URLSchemeHandler : MonoBehaviour {

    public static URLSchemeHandler Instance { get; private set; }
    public event OnLaunchUrlEventHandler OnLaunchUrlEvent;

	// custom scheme
	public string schemeProtocol = "switchback://";

	private Dictionary<string, string> parameters;

    #if UNITY_IOS
        [DllImport ("__Internal")]
		private static extern void _startOpenURL();
    #endif

    // keep it alive while racing...
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    void Start () 
	{
		#if UNITY_EDITOR
		    // test url query
		    // string url = schemeProtocol + "fullname=Rafael Emmanuelli&email=rafael.emmanuelli@sweetrush.com&profile_field_region=SweetRush&profile_field_stateprovince=SR";
		    // ParseLaunchURL(url);
		#endif

		#if UNITY_IOS
			StartOpenURL();
		#endif
	}

	// Once iOS or Android handles the launch url, it will send string to this function
	private void ParseLaunchURL(string url)
	{
		Dictionary<string, string> result = new Dictionary<string, string> ();
		
		ParseQueryString (url, result);

		if (OnLaunchUrlEvent != null) OnLaunchUrlEvent( result );

		#if UNITY_EDITOR
		foreach(KeyValuePair<string,string> name in result)
		{
			Debug.Log("ParseLaunchURL -> " + name.Key + ": " + name.Value);
		}
		#endif
	}

	/* IOS ************************************************* */

	// This is called from the iOS AppController.mm
	void OnOpenWithUrl(string parameters)
	{
		ParseLaunchURL(parameters);
	}

	/* 
	 * Because of a slight delay when loading the app for the first time
	 * We don't get the URL Scheme parameters. We force the iOS external call at Unity Start
	 * 
	 */
	public void StartOpenURL()
	{
		#if UNITY_IOS
		if (Application.platform == RuntimePlatform.IPhonePlayer) 
		{
			_startOpenURL();
		}
		#endif
	}

	/* ANDROID ************************************************* */

	#if UNITY_ANDROID
	/* 
	* Android Only: Triggers when app is resuming from the background
	* If app was launched via url scheme, we will call AndroidGetLaunchURL() 
	* to get the url query
	* 
	*/
	void OnApplicationPause (bool pause) 
	{
		if (!pause) AndroidGetLaunchURL();
	}

	/* 
	 * Connects to the Android Java Plugin Class 
	 * Calls getLauncherURL from the Android Java Plugin Class 
	 * And gets the Launch URL Query
	 * 
	 */
	void AndroidGetLaunchURL()
	{
		if (Application.platform == RuntimePlatform.Android) 
		{
			AndroidJavaClass pluginClass = new AndroidJavaClass("com.sweetrush.unityurlschemeplugin.URLScheme");

			string parameters = pluginClass.CallStatic<string>("getLauncherURL");

			ParseLaunchURL(parameters);
		}
	}
	#endif

	/* 
	 * ref: https://gist.github.com/Ran-QUAN/d966423305ce70cbc320f319d9485fa2
	 * Returns a dict/keys from query 
	 * 
	 */
	void ParseQueryString(string query, Dictionary<string, string> result)
	{
		if (query.Length == 0) return;

		// remove protocol
		query = query.Substring(schemeProtocol.Length, query.Length - (schemeProtocol.Length));

		var decodedLength = query.Length;
		var namePos = 0;
		var first = true;

		while (namePos <= decodedLength)
		{
			int valuePos = -1, valueEnd = -1;

			for (var q = namePos; q < decodedLength; q++)
			{
				if ((valuePos == -1) && (query[q] == '='))
				{
					valuePos = q + 1;
				}
				else if (query[q] == '&')
				{
					valueEnd = q;
					break;
				}
			}

			if (first)
			{
				first = false;

				if (query[namePos] == '?') 
				{
					namePos++;
				}
			}

			string name;

			if (valuePos == -1)
			{
				name = null;
				valuePos = namePos;
			}
			else
			{
				name = WWW.UnEscapeURL(query.Substring(namePos, valuePos - namePos - 1));
			}

			if (valueEnd < 0)
			{
				namePos = -1;
				valueEnd = query.Length;
			}
			else
			{
				namePos = valueEnd + 1;
			}

			var value = WWW.UnEscapeURL(query.Substring(valuePos, valueEnd - valuePos));

			result.Add(name, value);

			if (namePos == -1) break;
		}
	}
}

 