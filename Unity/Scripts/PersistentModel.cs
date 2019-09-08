using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public delegate void OnLeaderboardDataFromServer(bool success, ServerData data);
public delegate void OnUpdateDataFromServer(bool success);

public class PersistentModel : MonoBehaviour {

    public static PersistentModel Instance { get; private set; }

    public static string GameSceneOverride; //see "Race.cs"
    public bool DEBUG = true;

    [Tooltip("set to always load this challenge index")]
    public int ForceChallengeIndex = -1;

    [Tooltip("force game to resume at this index")]
    public int ResumeChallengeIndex = -1;

    public UIManager.Screen InitialScreen = UIManager.Screen.LOADING;

    [HideInInspector]
    public float CarSpeed = 0f;                     // convenience prop to get car speed

    public enum ModeEnum : int
    {
        WINTER = 1,
        LIGHTTRUCK = 2,
        PASSENGER = 3
    }

    public enum RUN_LOCATION {Local, Cocoa, Client, Raid}
    public enum SERVER_LOCATION {Client, Local, Raid}
	public RUN_LOCATION RunLocation = RUN_LOCATION.Client;
	public SERVER_LOCATION ServerLocation = SERVER_LOCATION.Client;

    public string RootURL
    {
        get
        {
            switch (PersistentModel.Instance.RunLocation)
            {
				case PersistentModel.RUN_LOCATION.Raid:
				case PersistentModel.RUN_LOCATION.Client:
			    case PersistentModel.RUN_LOCATION.Cocoa:
                    var str = Application.absoluteURL;
                    var index = str.LastIndexOf('/');
                    str = str.Substring(0, index + 1);
                    Console.WriteLine(str);
                    return str;
                case PersistentModel.RUN_LOCATION.Local:
                    return "http://localhost/";
            }

            throw new Exception("unhandled PersistentModel RootURL case");
        }
    }

	public string ServerURL
	{
		get
		{
			switch (PersistentModel.Instance.ServerLocation)
			{
		    	case PersistentModel.SERVER_LOCATION.Client:
				    return "https://CLIENTURL/api";
                case PersistentModel.SERVER_LOCATION.Raid:
                    return "https://RAIDURL/api";
                case PersistentModel.SERVER_LOCATION.Local:
                    return "https://localhost/Switchback/api/";
            }

			throw new Exception("unhandled PersistentModel ServerURL case");
		}
	}

	public string AssetBundlesURL
	{
		get
		{
			switch (PersistentModel.Instance.RunLocation)
			{
                case PersistentModel.RUN_LOCATION.Raid:
                    return "https://CLIENTURL/AssetBundles/";
                case PersistentModel.RUN_LOCATION.Client:
                case PersistentModel.RUN_LOCATION.Cocoa:
                    var str = Application.absoluteURL;
                    var index = str.LastIndexOf('/');
                    str = str.Substring(0, index + 1) + "AssetBundles/";
                    Console.WriteLine(str);
                    return str;
				case PersistentModel.RUN_LOCATION.Local:
                    return "http://localhost/AssetBundles/Compiled/";
            }

		    throw new Exception("unhandled PersistentModel AssetBundlesURL case");
		}
	}

    public string DynamicAssetsURL
    {
        get
        {
            switch (PersistentModel.Instance.RunLocation)
            {
                case PersistentModel.RUN_LOCATION.Client:
                case PersistentModel.RUN_LOCATION.Raid:
                case PersistentModel.RUN_LOCATION.Cocoa:
                    var str = Application.absoluteURL;
                    var index = str.LastIndexOf('/');
                    str = str.Substring(0, index + 1) + "DynamicAssets/";
                    Console.WriteLine(str);
                    return str;
                case PersistentModel.RUN_LOCATION.Local:
                    return "http://localhost/DynamicAssets/";
            }

            throw new Exception("unhandled PersistentModel AssetBundlesURL case");
        }
    }

    //[HideInInspector]
    // holds the current record time for a track
    public int CurrentTrackRecordTime;

    //[HideInInspector]
    // Debug purposes: holds current result data from server
    public ServerData ResultData;

    //[HideInInspector]
    // holds the user's current total time for the circuit
    public int CurrentCircuitTime;

    //[HideInInspector]
    // holds the record times
    public CircuitRecordTimes CircuitRecordTimes;
    public void SetCircuitTimes(CircuitRecordTimes times)
    {
        CircuitRecordTimes = times;
    }

    public int GetCircuitRecordTime(string mode)
    {
        DebugLog.Trace("GetCircuitRecordTime " + mode);

        int time = 0;

        switch (mode)
        {
            case "passenger":
                time = CircuitRecordTimes.passenger;
                break;
            case "trucks":
                time = CircuitRecordTimes.trucks;
                break;
            case "winter":
                time = CircuitRecordTimes.winter;
                break;
        }

        return time;
    }



    //[HideInInspector]
    // holds the tracks completed
    public TrackCompletion TracksComplete;
    public void SetTrackCompletion(TrackCompletion completion)
    {
        TracksComplete = completion;
    }

    public bool IsAllTracksComplete()
    {
        return (TracksComplete.passenger && TracksComplete.trucks && TracksComplete.winter);
    }

    public void UpdateTrackCompletion()
    {
        DebugLog.Trace("UpdateTrackCompletion " + GameModeID);

        switch (GameModeID)
        {
            case "passenger":
                TracksComplete.passenger = true;
                break;
            case "trucks":
                TracksComplete.trucks = true;
                break;
            case "winter":
                TracksComplete.winter = true;
                break;
        }
    }

    public void ResetTrackCompletion()
    {
        DebugLog.Trace("ResetTrackCompletion");

        TracksComplete.passenger = false;
        TracksComplete.trucks = false;
        TracksComplete.winter = false;
    }

    [Tooltip("Select circuit mode")]
    public ModeEnum Mode = ModeEnum.WINTER;

    //[HideInInspector]
    public string GameModeID = "";                      // unique name id from config.xml - ex: winter

    //[HideInInspector]
    public List<string> GameTrackData;                  // holds the tracks played so far - contains unique track ID

    public void SetGameMode(string name)
    {
        if (ForceChallengeIndex > -1) return;

        DebugLog.Trace("Persistent.SetGameMode:" + name);

        GameModeID = name;

        switch (name)
        {
            case "winter":
                Mode = ModeEnum.WINTER;
                break;
            case "trucks":
                Mode = ModeEnum.LIGHTTRUCK;
                break;
            case "passenger":
                Mode = ModeEnum.PASSENGER;
                break;
        }
    }

    [HideInInspector]
    public string GameCarnivalLoopObject = "";

    private int challengeIndex = 0;
    public int ChallengeIndex
    {
        get
        {
            if (ForceChallengeIndex != -1) challengeIndex = ForceChallengeIndex;

            return challengeIndex;
        }

        set
        {
            challengeIndex = value;
        }
    }
    public string ChallengeUID { get; set; }
    public int ChallengeCounter { get; set; }

    // game time props
    public float ChallengeTime { get; set; }                                    // elapsed game time
    public float CurrentCompletedChallengeTime { get; set; }                    // current completed elapsed game time
    public float TotalChallengeTime { get; set; }                               // final total game time
	public bool ClockIsStopped { get; set; }                                    // if true, don't advance clock (e.g., user clicked Pause)
	public bool HasReadInstructions { get; set; }

    // config xml props
    private XmlDocument config;
    private XmlNodeList vehicles;
    private XmlNodeList tracks;
    private XmlNodeList badges;
    private XmlNodeList trophies;
    private XmlNodeList modes;
	private XmlNodeList settings;

    // user profile
    private Dictionary<string, string> parameters = new Dictionary<string, string>();
    public string Name { get; set; }
    public string Email { get; set; }
    public string Region { get; set; }
    public string Org { get; set; }

    // user rankings
    public int UserRank { get; set; }
    public int UserRegionRank { get; set; }
    public int UserOrgRank { get; set; }

    // user interaction
    public bool TireOptionSelectedCorrect = false;                             // saved the user selected tire option
    public TireOptionData TireOptionSelectedData;                              // saved the user selected tire option

    // randomize settings
    public bool RandomizeTireOptions = false;
    public bool RandomizeTracks = false;

    // leaderboard props
    public List<string> PlayersRangeList { get; set; }
    public List<string> ScoresRangeList { get; set; }
    public List<string> PlayersRegionRangeList { get; set; }
    public List<string> ScoresRegionRangeList { get; set; }
    public List<string> PlayersOrgRangeList { get; set; }
    public List<string> ScoresOrgRangeList { get; set; }
    public List<string> Top10NamesList { get; set; }
    public List<string> Top10ScoreList { get; set; }

    // handles all server connections
    public ServerHandler Server { get; set; }

    private void Awake()
    {
        Caching.ClearCache();

        HasReadInstructions = false;

        if (Instance == null)
        {
            Instance = this;

            Initialize();

            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    private void Initialize()
    {
        DebugLog.Trace("PersistentModel.Initialize()");

        ClockIsStopped = true;
        ChallengeTime = 0.0f;
        CurrentCompletedChallengeTime = 0.0f;
        TotalChallengeTime = 0.0f;
        ChallengeCounter = 0;
        // force game to resume at this index
        if (ResumeChallengeIndex != -1) ChallengeIndex = ResumeChallengeIndex;

        // handles all server connections
        Server = transform.GetComponent<ServerHandler>();

        // reset user profile
        ClearUserDetails();

        // iOS and Android: At startup we need to wait for the AppURLSchemeLauncher to trigger
        // see UIManager.

    #if UNITY_WEBGL || UNITY_EDITOR

        char[] parameterDelimiters = new char[] { '?', '&' };
        string[] parameterStrings = Application.absoluteURL.Split(parameterDelimiters, System.StringSplitOptions.RemoveEmptyEntries);

        if (parameterStrings.Length > 1 && !DEBUG)
        {
            char[] keyValueDelimiters = new char[] { '=' };
            for (int i = 0; i < parameterStrings.Length; ++i)
            {
                string[] keyValue = parameterStrings[i].Split(keyValueDelimiters, System.StringSplitOptions.None);

                if (keyValue.Length >= 2) parameters.Add(WWW.UnEscapeURL(keyValue[0]), WWW.UnEscapeURL(keyValue[1]));
                else if (keyValue.Length == 1) parameters.Add(WWW.UnEscapeURL(keyValue[0]), "");
            }
        }
        else
        {
            parameters = GetTempUser();
        }

        UpdateUserParameters(parameters);
    #endif

        LoadConfigSettings();
    }

    // Tester user info
    public string UserName = "Guest04";
    public string UserEmail = "Guest04@sweetrush.com";
    public Dictionary<string, string> GetTempUser()
    {
        Dictionary<string, string> userParams = new Dictionary<string, string>
        {
            { "fullname", UserName },
            { "email", UserEmail },
            { "profile_field_region", "SweetRush" },
            { "profile_field_stateprovince", "SR" }
        };

        return userParams;
    }

    public void ClearUserDetails()
    {
        Name = "";
        Email = "";
        Region = "";
        Org = "";
    }

    public void UpdateUserParameters(Dictionary<string, string> parameters)
    {
        DebugLog.Trace("PersistentModel.UpdateUserParameters()");

        Name = parameters.ContainsKey("fullname") ? parameters["fullname"] : "";
        Email = parameters.ContainsKey("email") ? parameters["email"] : "";
        Region = parameters.ContainsKey("profile_field_region") ? parameters["profile_field_region"] : "";
        Org = parameters.ContainsKey("profile_field_stateprovince") ? parameters["profile_field_stateprovince"] : "";

        DebugLog.Trace("PersistentModel.Email:" + Email);

        bool addParams = false;
        if (addParams)
        {
            parameters.Add("d", "1");
            parameters.Add("mode", "trucks");
            parameters.Add("screen", "tire");
            parameters.Add("cid", "10");
            parameters.Add("uid", "20");
        }

        // check for tire random options
        if (parameters.ContainsKey("rndOpts"))
        {
            int randOpts = int.Parse(parameters["rndOpts"]);
            RandomizeTireOptions = (randOpts > 0) ? true : false;
        }

        // check if we have any special commands in url params
        // the following url param will force load the challenge
        // ?mode=winter || passenger || trucks &cid=4 &screen=game || tire
        if (parameters.ContainsKey("d")) DebugHandler.ParseUrlParameters(parameters);

        if (parameters.ContainsKey("fps"))
        {
            GameObject.Find("FrameRate").SetActive(true);
        }
    }

    private void LoadConfigSettings()
    {
        // load config xml
        TextAsset textAsset = (TextAsset)Resources.Load("config");
        config = new XmlDocument();
        config.LoadXml(textAsset.text);

        XmlNodeList list;

        list = config.GetElementsByTagName("vehicle");
        vehicles = list;

        list = config.GetElementsByTagName("track");
        tracks = list;

        list = config.GetElementsByTagName("badge");
        badges = list;

        list = config.GetElementsByTagName("trophy");
        trophies = list;

        list = config.GetElementsByTagName("mode");
        modes = list;

        list = config.GetElementsByTagName("setting");
        settings = list;

    }

    public void Reset()
    {
        GameModeID = "";
        ChallengeIndex = 0;
        ChallengeCounter = 0;
        TotalChallengeTime = 0;
        ChallengeTime = 0;
    }

    public string GameScene
    {
        get {
            return modes[((int)Mode) - 1].SelectNodes("challenges/challenge")[ChallengeIndex].SelectNodes("game").Item(0).Attributes.GetNamedItem("scene").Value;
        }
    }

    public string GameTrack
    {
        get {
            return modes[((int)Mode) - 1].SelectNodes("challenges/challenge")[ChallengeIndex].SelectNodes("game").Item(0).Attributes.GetNamedItem("track").Value;
        }
    }

    public string GameTrackType
	{
    	get {
            return modes[((int)Mode) - 1].SelectNodes("challenges/challenge")[ChallengeIndex].SelectNodes("game").Item(0).Attributes.GetNamedItem("tracktype").Value;
        }
	}

    public string GameVehicle
    {
        get {
            return modes[((int)Mode) - 1].SelectNodes("challenges/challenge")[ChallengeIndex].SelectNodes("game").Item(0).Attributes.GetNamedItem("vehicle").Value;
        }
    }

    public string GameSkybox
    {
        get {
            return modes[((int)Mode) - 1].SelectNodes("challenges/challenge")[ChallengeIndex].SelectNodes("game").Item(0).Attributes.GetNamedItem("skybox").Value; }
    }

    public bool GameNight
    {
        get {
            return modes[((int)Mode) - 1].SelectNodes("challenges/challenge")[ChallengeIndex].SelectNodes("game").Item(0).Attributes.GetNamedItem("night").Value == "true";
        }
    }

    public string GameWeather
    {
        get {
            return modes[((int)Mode) - 1].SelectNodes("challenges/challenge")[ChallengeIndex].SelectNodes("game").Item(0).Attributes.GetNamedItem("weather").Value;
        }
    }

    public int GameLaps
    {
        get {
            return Convert.ToInt32(modes[((int)Mode) - 1].SelectNodes("challenges/challenge")[ChallengeIndex].SelectNodes("game").Item(0).Attributes.GetNamedItem("laps").Value);
        }
    }

    public int ChallengeCount
    {
		// JBC: This is now a parameter in config (<setting>), so it can be adjusted and others added more easily
		get {
            return Convert.ToInt32(settings[0].Attributes.GetNamedItem("val").Value);
        }
    }

    public int GameModeChallengeCount
    {
        get {
            return modes[((int)Mode) - 1].ChildNodes.Count;
        }

    }

    public string GetGameModeID
    {
        get {
            return modes[((int)Mode) - 1].Attributes.GetNamedItem("name").Value;
        }
    }

    public string ChallengeText
    {
        get { return modes[((int)Mode) - 1].SelectNodes("challenges/challenge")[ChallengeIndex].SelectSingleNode("question/text()").Value.Trim(); }
    }

    public string ChallengeTrackUID
    {
        get { return modes[((int)Mode) - 1].SelectNodes("challenges/challenge")[ChallengeIndex].Attributes.GetNamedItem("uid").Value; }
    }

    public string ChallengeRecordTime
    {
        get { return modes[((int)Mode) - 1].SelectNodes("challenges/challenge")[ChallengeIndex].Attributes.GetNamedItem("record_time").Value; }
    }

    public string ChallengeBrandName
    {
        get { return modes[((int)Mode) - 1].SelectNodes("challenges/challenge")[ChallengeIndex].Attributes.GetNamedItem("brand").Value; }
    }

    public string GetChallengeOptionWhyText(int index)
    {
        return modes[((int)Mode) - 1].SelectNodes("challenges/challenge")[ChallengeIndex].SelectNodes("question/answers/answer").Item(index).InnerText.Trim();
    }

	public string GetChallengeTireName(int index)
	{
        return modes[((int)Mode) - 1].SelectNodes("challenges/challenge")[ChallengeIndex].SelectNodes ("question/answers/answer").Item (index).Attributes.GetNamedItem ("tire").Value;
	}

	public string GetChallengeBillboardName()
	{
        return modes[((int)Mode) - 1].SelectNodes("challenges/challenge")[ChallengeIndex].SelectSingleNode("question").Attributes.GetNamedItem("billboard").Value;
	}

    public IEnumerator GetChallengeOptionTire(int index, Image image)
    {
        string tireName  = modes[((int)Mode) - 1].SelectNodes("challenges/challenge")[ChallengeIndex].SelectNodes("question/answers/answer").Item(index).Attributes.GetNamedItem("tire").Value;
        Texture2D texture2D;

        if (Application.platform != RuntimePlatform.WebGLPlayer)
        {
            texture2D = Instantiate(Resources.Load("tires/" + tireName)) as Texture2D;
            image.sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(.5f, .5f));
            yield break;
        }

        WWW www = new WWW(DynamicAssetsURL + "tires/" + tireName + ".png" + "?t=" + DateTime.UtcNow.ToFileTimeUtc());
        yield return www;
        texture2D = www.texture;
        image.sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(.5f, .5f));
    }

    public int GetChallengeOptionCorrectIndex()
    {
        XmlNode xmlNode = modes[((int)Mode) - 1].SelectNodes("challenges/challenge")[ChallengeIndex].SelectSingleNode("question/answers/answer[@correct = 'true']");
        XmlNodeList childNodes = xmlNode.ParentNode.ChildNodes;

        int i = 0;
        for (; i < childNodes.Count; i++) if (childNodes[i] == xmlNode) break;

        return i;
    }

    public bool IsChallengeOptionCorrect(int index)
    {
        return modes[((int)Mode) - 1].SelectNodes("challenges/challenge")[ChallengeIndex].SelectNodes("question/answers/answer").Item(index).Attributes.GetNamedItem("correct").Value == "true";
    }

	public string GetCustomerPreference (int index)
	{
        return modes[((int)Mode) - 1].SelectNodes("challenges/challenge")[ChallengeIndex].SelectNodes ("preference/text").Item (index).InnerText.Trim ();
	}

    public int GetCustomerPreferenceCount()
    {
        return modes[((int)Mode) - 1].SelectNodes("challenges/challenge")[ChallengeIndex].SelectNodes("preference/text").Count;
    }

    public IEnumerator GetChallengeBillboard(Image image)
    {
        string billboardName = modes[((int)Mode) - 1].SelectNodes("challenges/challenge")[ChallengeIndex].SelectSingleNode("question").Attributes.GetNamedItem("billboard").Value;
        Texture2D texture2D;

        if (Application.platform != RuntimePlatform.WebGLPlayer)
        {
            texture2D = Instantiate(Resources.Load("billboards/" + billboardName)) as Texture2D;
            image.sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(.5f, .5f));
            yield break;
        }

        WWW www = new WWW(DynamicAssetsURL + "billboards/" + billboardName + ".jpg" + "?t=" + DateTime.UtcNow.ToFileTimeUtc());

        yield return www;

        texture2D = www.texture;
        image.sprite =  Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(.5f, .5f));
    }

    public string GetVehicleName(int index)
    {
        return vehicles[index].Attributes.GetNamedItem("name").Value;
    }

    public string GetTrackName(int index)
    {
        return tracks[index].Attributes.GetNamedItem("name").Value;
    }

    public Sprite GetBadge(int index) // 0, 1, or 2
    {
        string badgeName = badges[index].Attributes.GetNamedItem("name").Value;
        Texture2D texture2D = Instantiate(Resources.Load("badges/" + badgeName)) as Texture2D;
        return Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(.5f, .5f));
    }

    public Sprite GetTrophy(int index) // 0, 1, or 2
    {
        string trophyName = trophies[index].Attributes.GetNamedItem("name").Value;
        Texture2D texture2D = Instantiate(Resources.Load("trophies/" + trophyName)) as Texture2D;
        return Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(.5f, .5f));
    }

	public int GetTrophyIndex()
	{
		// badge_dummy = 0, badge_rookie = 1, badge_pro = 2
		int trophyIndex;

        int multiplier = 3;
        int proRankMin = 200 * multiplier;
        int rookieRankMin = 201 * multiplier;
        int rookieRankMax = 240 * multiplier;

        // base off 40
		if (TotalChallengeTime <= proRankMin) // PRO 3.33
		{
			trophyIndex = 2;
		}
		else if (TotalChallengeTime >= rookieRankMin && TotalChallengeTime <= rookieRankMax) // ROOKIE
		{
			trophyIndex = 1;
		}
		else // if (PersistentModel.Instance.ChallengeTime >= 241) // CRASH DUMMY
		{
			trophyIndex = 0;
		}

		return trophyIndex;
	}

    // formats float time to readable time format for display
    public string FormatTime(float timeInSeconds)
    {
        return System.TimeSpan.FromSeconds(timeInSeconds).ToString(@"mm\:ss"); ;
    }

    // string time coming from server
    public string ConvertTime (string timeInSeconds)
	{
		int val =  Convert.ToInt32(timeInSeconds);

		System.TimeSpan time = System.TimeSpan.FromSeconds (val);

        return time.ToString(@"mm\:ss");
    }

	public void SaveCurrentTotalTime()
	{
		TotalChallengeTime += (int)ChallengeTime;
    }

    public int GetRandomChallengeIndex()
    {
        //Debug.Log("GetRandomChallengeIndex.total: " + GameTrackData.Count);

        // we check if user has any tracks played in current circuit
        // we dont want them to do any previously played tracks
        for (int i = 0; i < GameTrackData.Count; i++)
        {
            //Debug.Log("COMPLETED TRACK UID:" + GameTrackData[i]);
        }

        // get all challenges and compare with completed ones
        List<string> currentChallengeList = new List<string>();
        XmlNodeList challengeList = modes[((int)Mode) - 1].SelectNodes("challenges/challenge");
        int challengesTotal = modes[((int)Mode) - 1].SelectNodes("challenges/challenge").Count;
        for (int j = 0; j < challengeList.Count; j++)
        {
            string cUID = challengeList[j].Attributes.GetNamedItem("uid").Value;
            //Debug.Log("challengeList:" + cUID);
            currentChallengeList.Add(cUID);
        }

        // no completed tracks
        if (GameTrackData.Count == 0)
        {
            // currentChallengeList.ShuffleCrypto();



        }
        else
        {
            //Debug.Log("LIST CURRENT CHALLENGE LIST");
            //Debug.Log("REMOVE ITEMS");
            bool removedChallenge = false;
            for (int k = currentChallengeList.Count - 1; k >= 0; k--)
            {
                //Debug.Log("currentChallengeList: " + currentChallengeList[k]);
                removedChallenge = false;

                for (int m = 0; m < GameTrackData.Count; m++)
                {
                    //Debug.Log("GameTrackData: " + GameTrackData[m]);

                    if (removedChallenge) continue;

                    if (currentChallengeList[k] == GameTrackData[m])
                    {
                       // Debug.Log("FOUND COMPLETED CHALLENGE: Removing->" + GameTrackData[m]);

                        removedChallenge = true;
                        currentChallengeList.RemoveAt(k);
                    }

                }
            }
        }

        currentChallengeList.ShuffleCrypto();

        //Debug.Log("LIST UPDATED CURRENT CHALLENGE LIST " + currentChallengeList.Count);
        //for (int n = 0; n < currentChallengeList.Count; n++)
       // {
        //    Debug.Log("currentChallengeList: " + currentChallengeList[n]);
        //}

        string selectedUID = currentChallengeList[0];
        int selectedIndex = -1;
        for (int index = 0; index < challengeList.Count; index++)
        {
            string cUID = challengeList[index].Attributes.GetNamedItem("uid").Value;

            if (selectedUID == cUID)
            {
                selectedIndex = index;
            }
        }

        //Debug.Log("SELECTED INDEX:" + selectedIndex);

        return selectedIndex;
    }

}
