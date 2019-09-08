using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LeaderboardScreen : BaseScreen
{
	public enum LEADERBOARDS {Overall, Passenger, LightTruck, Winter}

    private RectTransform superTireSmallLogo;
    private RectTransform leaderboardBg;
    private RectTransform leaderboardBanner;
    private RectTransform buttonPanel;

    private RectTransform overallButton;
    private RectTransform passengerButton;
    private RectTransform lightTruckButton;
    private RectTransform winterButton;

    private RectTransform tableHeaderPanel;
    private RectTransform title_RankText;
	private RectTransform title_NameText;
	private RectTransform title_TimeText;

    private RectTransform viewport;
    private RectTransform returnButton;

	private GameObject[] _listLeaderboardItems;
	private DataColumnsPanel dataColumn;
	private LeaderboardItem _baseLeaderboardItem;

    private Color elementShowColor = new Color(1f, 1f, 1f, 1f);
    private Color elementHideColor = new Color(1f, 1f, 1f, 0f);

    private bool isSectionTransitioning = false;

    public override void Initialize(string id)
    {
		base.Initialize(id);

        superTireSmallLogo = _screenElements["SuperTireSmallLogo"];
        leaderboardBg = _screenElements["LeaderboardBg"];
        leaderboardBanner = _screenElements["LeaderboardBanner"];
        buttonPanel = _screenElements["ButtonPanel"];
        tableHeaderPanel = _screenElements["TableHeaderPanel"];

        overallButton = _screenElements["OverallButton"];
        passengerButton = _screenElements["PassengerButton"];
        lightTruckButton = _screenElements["LightTruckButton"];
        winterButton = _screenElements["WinterButton"];
        
		title_RankText = _screenElements ["RankText"];
		title_RankText.GetComponent<Text> ().color = elementHideColor;
		title_NameText = _screenElements ["NameText"];
		title_NameText.GetComponent<Text> ().color = elementHideColor;
		title_TimeText = _screenElements ["TimesText"];
		title_TimeText.GetComponent<Text> ().color = elementHideColor;

		viewport = _screenElements["ScrollView"];
		viewport.GetComponent<Image> ().color = new Color (1, 1, 1, 1);
		viewport.Find("Viewport").GetComponent<Image>().color = new Color (1, 1, 1, 1);

		dataColumn = viewport.GetComponent<ScrollRect>().content.GetComponent<DataColumnsPanel>();
		dataColumn.GetComponent<CanvasGroup>().alpha = 1;

		_baseLeaderboardItem = dataColumn.transform.GetComponentInChildren<LeaderboardItem>();

        returnButton = _screenElements["ReturnButton"];
    }

	public override void Draw()
	{
		leaderboardBg.localScale = new Vector3(0f, 1f, 1f);
		leaderboardBg.GetComponent<Image>().color = elementShowColor;

		leaderboardBanner.localScale = new Vector3(0f, 1f, 1f);
		leaderboardBanner.GetComponent<Image>().color = elementShowColor;

		buttonPanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);

        overallButton.GetComponent<Image>().color = elementShowColor;
        overallButton.GetComponentInChildren<Text>().color = elementShowColor;
        overallButton.GetComponent<CanvasGroup>().alpha = 0;
        overallButton.GetComponent<Button>().interactable = false;

        passengerButton.GetComponent<Image>().color = elementShowColor;
        passengerButton.GetComponentInChildren<Text>().color = elementShowColor;
        passengerButton.GetComponent<CanvasGroup>().alpha = 0;

        lightTruckButton.GetComponent<Image>().color = elementShowColor;
        lightTruckButton.GetComponentInChildren<Text>().color = elementShowColor;
        lightTruckButton.GetComponent<CanvasGroup>().alpha = 0;

        winterButton.GetComponent<Image>().color = elementShowColor;
        winterButton.GetComponentInChildren<Text>().color = elementShowColor;
        winterButton.GetComponent<CanvasGroup>().alpha = 0;

		tableHeaderPanel.GetComponent<Image>().color = new Color(1, 1, 1, 0f);
		title_RankText.GetComponent<Text> ().color = elementHideColor;
		title_NameText.GetComponent<Text> ().color = elementHideColor;
		title_TimeText.GetComponent<Text> ().color = elementHideColor;

        TransitionIn();
    }

    private void TransitionIn()
    {
        // Start Animation In

        // Title Banner
        LeanTween.scale(leaderboardBanner, new Vector3(1f, 1f, 1f), 0.65f)
        .setDelay(0.35f)
        .setEase(LeanTweenType.easeOutBack);

        // Background Panel
        LeanTween.scale(leaderboardBg, new Vector3(1f, 1f, 1f), 0.65f)
            .setDelay(0.25f)
            .setEase(LeanTweenType.easeOutBack);

        // start showing top header and buttons
        LeanTween.delayedCall(0.85f, () => {

            float curTime = 0f;

            // TOP BUTTON PANEL and BUTTONS

            // OVERALL BUTTON

            LeanTween.alphaCanvas(overallButton.GetComponent<CanvasGroup>(), 1f, 0.75f)
                .setEase(LeanTweenType.easeOutCubic)
                .setDelay(curTime);

            // PASSENGER BUTTON
            LeanTween.alphaCanvas(passengerButton.GetComponent<CanvasGroup>(), 1f, 0.75f)
                .setEase(LeanTweenType.easeOutCubic)
                .setDelay(curTime);

            // LIGHT TRUCK BUTTON
            LeanTween.alphaCanvas(lightTruckButton.GetComponent<CanvasGroup>(), 1f, 0.75f)
                .setEase(LeanTweenType.easeOutCubic)
                .setDelay(curTime);

            // WINTER BUTTON
            LeanTween.alphaCanvas(winterButton.GetComponent<CanvasGroup>(), 1f, 0.75f)
                .setEase(LeanTweenType.easeOutCubic)
                .setDelay(curTime);

            // HEADER PANEL
            curTime += 0.25f;
            Color headerPanelClr = new Color(1, 1, 1, 1);
            LeanTween.color(tableHeaderPanel, headerPanelClr, 0.5f)
                .setEase(LeanTweenType.easeOutCubic)
                .setDelay(curTime)
                .setOnComplete(() =>
                {
                    TransitionInCompleted();    // End of Transition
                });

            //curTime += 0.01f;
            LeanTween.alphaText(title_RankText, 1f, 0.5f)
                .setEase(LeanTweenType.easeOutCubic)
                .setDelay(curTime);

            //curTime += 0.03f;
            LeanTween.alphaText(title_NameText, 1f, 0.5f)
                .setEase(LeanTweenType.easeOutCubic)
                .setDelay(curTime);

            //curTime += 0.05f;
            LeanTween.alphaText(title_TimeText, 1f, 0.5f)
                .setEase(LeanTweenType.easeOutCubic)
                .setDelay(curTime);
        });
    }

    protected override void TransitionInCompleted()
    {
        // screen draw has been finished, invokes Base.TransitionInCompleteEvent if available
        base.TransitionInCompleted();

        InitButtonEvents();

        LeanTween.delayedCall(0.1f, LoadOverallList);

        // Super Tire Logo
        LeanTween.alpha(superTireSmallLogo, 1f, 0.75f)
            .setEase(LeanTweenType.easeInOutCubic)
            .setDelay(1.5f);

        LeanTween.alphaCanvas(returnButton.GetComponent<CanvasGroup>(), 1f, 0.75f)
            .setEase(LeanTweenType.easeInOutCubic)
            .setDelay(2.0f);
    }

    private void LoadOverallList()
	{
		UIManager.Instance.soundManager.PlaySound("PlaySineWaveHighPitch");

        if (!_isTransitionComplete || isSectionTransitioning) return;

        UpdateTopButtons(LEADERBOARDS.Overall);

		// load top 10 times
		PersistentModel.Instance.Server.OnGetTop10FromServerComplete += OnGetOverallFromServerComplete;
		PersistentModel.Instance.Server.GrabTop10FromServer();
	}

	private void OnGetOverallFromServerComplete(bool success, ServerData data)
	{
        //LeanTween.cancelAll();

        PersistentModel.Instance.Server.OnGetTop10FromServerComplete -= OnGetOverallFromServerComplete;

        DebugLog.Trace("data.success:" + data.success);
        DebugLog.Trace("data.success:" + data.leaderboard_data.Length);

        _listLeaderboardItems = new GameObject[data.leaderboard_data.Length];

        float delay = 0.25f;

        LeaderboardItem leaderboardGameObj;
        for (int i = 0; i < data.leaderboard_data.Length; i++)
        {
            if (i == 0) leaderboardGameObj = _baseLeaderboardItem;
            else leaderboardGameObj = (Instantiate(_baseLeaderboardItem.gameObject) as GameObject).GetComponent<LeaderboardItem>();

            ResetBaseLeaderboardItem(leaderboardGameObj);

            leaderboardGameObj.GetComponent<RectTransform>().localScale = new Vector3(1f, 0f, 1f);

            leaderboardGameObj.gameObject.FindGameObjectChildWithName("Panel").GetComponent<Image>().color = elementShowColor;
            leaderboardGameObj.GetComponent<CanvasGroup>().alpha = 0;
            leaderboardGameObj.rankText = (i + 1).ToString();
            leaderboardGameObj.nameText = data.leaderboard_data[i].fullname.ToUpper();
            leaderboardGameObj.timeText = PersistentModel.Instance.ConvertTime(data.leaderboard_data[i].total_time.ToString());
            leaderboardGameObj.transform.SetParent(dataColumn.transform, false);

            if (i > 0) _listLeaderboardItems[i] = leaderboardGameObj.gameObject;

            // Set player item if avalaible
            if (data.leaderboard_data[i].fullname == PersistentModel.Instance.Name)
            {
                // check if NOT ranked top 10, if so set RANK number
                if (data.leaderboard_data[i].rank > 10)
                {
                    DebugLog.Trace("NOT_RANKED #: " + data.leaderboard_data[i].rank);
                    leaderboardGameObj.rankText = data.leaderboard_data[i].rank.ToString();
                }

                SetUserBaseLeaderboardItem(leaderboardGameObj);
            }

            LeanTween.alphaCanvas(leaderboardGameObj.GetComponent<CanvasGroup>(), 1f, 0.85f).setDelay(delay);

            LeanTween.scale(leaderboardGameObj.GetComponent<RectTransform>(), new Vector3(1f, 1f, 1f), 0.5f)
                .setDelay(delay)
                .setEase(LeanTweenType.easeInQuad);

            delay += 0.15f;
        }

        if (data.leaderboard_data.Length == 0)
        {
            leaderboardGameObj = _baseLeaderboardItem;

            leaderboardGameObj.GetComponent<RectTransform>().localScale = new Vector3(1f, 0f, 1f);
            leaderboardGameObj.GetComponent<CanvasGroup>().alpha = 0;
            leaderboardGameObj.rankText = "";
            leaderboardGameObj.nameText = "NO PLAYERS FOUND";
            leaderboardGameObj.timeText = "";
            leaderboardGameObj.transform.SetParent(dataColumn.transform, false);

            LeanTween.alphaCanvas(leaderboardGameObj.GetComponent<CanvasGroup>(), 1f, 0.85f).setDelay(delay);

            LeanTween.scale(leaderboardGameObj.GetComponent<RectTransform>(), new Vector3(1f, 1f, 1f), 0.75f)
                .setDelay(delay)
                .setEase(LeanTweenType.linear);
        }
    }

	private void LoadPassengerList()
	{
		UIManager.Instance.soundManager.PlaySound("PlaySineWaveHighPitch");

        if (!_isTransitionComplete || isSectionTransitioning) return;

        UpdateTopButtons(LEADERBOARDS.Passenger);

		// load top 10 times
		PersistentModel.Instance.Server.OnGetTop10ByGameModeFromServerComplete += OnGetTop10ByGameModeFromServerComplete;
		PersistentModel.Instance.Server.GrabTop10ByGameModeFromServer("passenger");
	}

    private void LoadLightTruckList()
    {
        UIManager.Instance.soundManager.PlaySound("PlaySineWaveHighPitch");

        if (!_isTransitionComplete || isSectionTransitioning) return;

        UpdateTopButtons(LEADERBOARDS.LightTruck);

        // load top 10 times
        PersistentModel.Instance.Server.OnGetTop10ByGameModeFromServerComplete += OnGetTop10ByGameModeFromServerComplete;
        PersistentModel.Instance.Server.GrabTop10ByGameModeFromServer("trucks");
    }

    private void LoadWinterList()
    {
        UIManager.Instance.soundManager.PlaySound("PlaySineWaveHighPitch");

        if (!_isTransitionComplete || isSectionTransitioning) return;

        UpdateTopButtons(LEADERBOARDS.Winter);

        // load top 10 times
        PersistentModel.Instance.Server.OnGetTop10ByGameModeFromServerComplete += OnGetTop10ByGameModeFromServerComplete;
        PersistentModel.Instance.Server.GrabTop10ByGameModeFromServer("winter");
    }

    void OnGetTop10ByGameModeFromServerComplete(bool success, ServerData data)
	{
       // LeanTween.cancelAll();

        PersistentModel.Instance.Server.OnGetTop10ByGameModeFromServerComplete -= OnGetTop10ByGameModeFromServerComplete;

        // Populate(LEADERBOARDS.National);

        DebugLog.Trace("data.success:" + data.success);
        DebugLog.Trace("data.success:" + data.leaderboard_data.Length);

        _listLeaderboardItems = new GameObject[data.leaderboard_data.Length];

        float delay = 0.25f;

        LeaderboardItem leaderboardGameObj;
        for (int i = 0; i < data.leaderboard_data.Length; i++)
        {
            if (i == 0) leaderboardGameObj = _baseLeaderboardItem;
            else leaderboardGameObj = (Instantiate(_baseLeaderboardItem.gameObject) as GameObject).GetComponent<LeaderboardItem>();

            ResetBaseLeaderboardItem(leaderboardGameObj);

            leaderboardGameObj.GetComponent<RectTransform>().localScale = new Vector3(1f, 0f, 1f);
            leaderboardGameObj.gameObject.FindGameObjectChildWithName("Panel").GetComponent<Image>().color = elementShowColor;

            leaderboardGameObj.GetComponent<CanvasGroup>().alpha = 0;
            leaderboardGameObj.rankText = (i + 1).ToString();
            leaderboardGameObj.nameText = data.leaderboard_data[i].fullname.ToUpper();
            leaderboardGameObj.timeText = PersistentModel.Instance.ConvertTime(data.leaderboard_data[i].total_time.ToString());
            leaderboardGameObj.transform.SetParent(dataColumn.transform, false);

            if (i > 0) _listLeaderboardItems[i] = leaderboardGameObj.gameObject;

            // Set player item if avalaible
            if (data.leaderboard_data[i].fullname == PersistentModel.Instance.Name)
            {
                // check if NOT ranked top 10, if so set RANK number
                if (data.leaderboard_data[i].rank > 10)
                {
                    DebugLog.Trace("NOT_RANKED #: " + data.leaderboard_data[i].rank);
                    leaderboardGameObj.rankText = data.leaderboard_data[i].rank.ToString();
                }

                SetUserBaseLeaderboardItem(leaderboardGameObj);
            }

            LeanTween.alphaCanvas(leaderboardGameObj.GetComponent<CanvasGroup>(), 1f, 0.85f).setDelay(delay);

            LeanTween.scale(leaderboardGameObj.GetComponent<RectTransform>(), new Vector3(1f, 1f, 1f), 0.5f)
                .setDelay(delay)
                .setEase(LeanTweenType.linear);

            delay += 0.15f;
        }

        if (data.leaderboard_data.Length == 0)
        {
            leaderboardGameObj = _baseLeaderboardItem;

            leaderboardGameObj.GetComponent<CanvasGroup>().alpha = 0;
            leaderboardGameObj.rankText = "";
            leaderboardGameObj.nameText = "NO PLAYERS FOUND";
            leaderboardGameObj.timeText = "";
            leaderboardGameObj.transform.SetParent(dataColumn.transform, false);

            LeanTween.alphaCanvas(leaderboardGameObj.GetComponent<CanvasGroup>(), 1f, 0.85f).setDelay(delay);

            LeanTween.scale(leaderboardGameObj.GetComponent<RectTransform>(), new Vector3(1f, 1f, 1f), 0.75f)
                .setDelay(delay)
                .setEase(LeanTweenType.linear);
        }
    }

    private void UpdateTopButtons(LEADERBOARDS leaderboards)
    {
        isSectionTransitioning = true;

        RemoveLeaderboardItems();

        EventSystem.current.SetSelectedGameObject(null);

        switch (leaderboards)
        {
            case LEADERBOARDS.Overall:
                overallButton.GetComponent<Button>().Select();
                overallButton.GetComponent<Button>().interactable = false;
                passengerButton.GetComponent<Button>().interactable = true;
                lightTruckButton.GetComponent<Button>().interactable = true;
                winterButton.GetComponent<Button>().interactable = true;

                break;
            case LEADERBOARDS.Passenger:
                passengerButton.GetComponent<Button>().Select();
                passengerButton.GetComponent<Button>().interactable = false;
                overallButton.GetComponent<Button>().interactable = true;
                lightTruckButton.GetComponent<Button>().interactable = true;
                winterButton.GetComponent<Button>().interactable = true;
                break;
            case LEADERBOARDS.LightTruck:
                lightTruckButton.GetComponent<Button>().Select();
                lightTruckButton.GetComponent<Button>().interactable = false;
                overallButton.GetComponent<Button>().interactable = true;
                passengerButton.GetComponent<Button>().interactable = true;
                winterButton.GetComponent<Button>().interactable = true;
                break;
            case LEADERBOARDS.Winter:
                winterButton.GetComponent<Button>().Select();
                winterButton.GetComponent<Button>().interactable = false;
                overallButton.GetComponent<Button>().interactable = true;
                passengerButton.GetComponent<Button>().interactable = true;
                lightTruckButton.GetComponent<Button>().interactable = true;
                break;
        }

        LeanTween.delayedCall(1.5f, () =>
        {
            isSectionTransitioning = false;
        });
    }

	private void ResetBaseLeaderboardItem(LeaderboardItem item)
	{
        item.LBRank.fontSize = 50;
		item.LBRank.color = Color.black;

		item.LBName.fontSize = 50;
		item.LBName.color = Color.white;

		item.LBTime.fontSize = 50;
		item.LBTime.color = Color.black;
	}

	private void SetUserBaseLeaderboardItem(LeaderboardItem item)
	{
        item.LBPanel.color = new Color(1, 0.7015066f, 0.259434f);

        item.LBRank.fontSize = 50;
		item.LBRank.color = Color.white;

		item.LBName.fontSize = 50;
		item.LBName.color = Color.black;

		item.LBTime.fontSize = 50;
		item.LBTime.color = Color.white;
	}

	private void InitButtonEvents()
    {
        overallButton.GetComponent<Button>().onClick.AddListener(LoadOverallList);
        passengerButton.GetComponent<Button>().onClick.AddListener(LoadPassengerList);
        lightTruckButton.GetComponent<Button>().onClick.AddListener(LoadLightTruckList);
        winterButton.GetComponent<Button>().onClick.AddListener(LoadWinterList);
       
        returnButton.gameObject.GetComponent<Button>().onClick.AddListener(OnReturnButtonClick);
    }

    private void OnReturnButtonClick()
    {
        UIManager.Instance.soundManager.PlaySound("PlaySineWaveHighPitch");

        if (!_isTransitionComplete || isSectionTransitioning) return;

        returnButton.gameObject.GetComponent<Button>().onClick.RemoveListener(OnReturnButtonClick);

        // check if coming from Welcome Back
        if (UIManager.Instance.PreviousScreenID == UIManager.Instance.GetScreenID(UIManager.Screen.WELCOME_BACK))
        {
            selectedScreen = UIManager.Screen.WELCOME_BACK;
        }
        else if (UIManager.Instance.PreviousScreenID == UIManager.Instance.GetScreenID(UIManager.Screen.CIRCUIT_COMPLETED_SCREEN))
        {
            selectedScreen = UIManager.Screen.CIRCUIT_COMPLETED_SCREEN;
        }
        else
        {
            selectedScreen = UIManager.Screen.CONGRATULATIONS_FINAL;
        }
        
        OnClickComplete += StartTransitionOut;
        ButtonClickAnimation(returnButton);
    }

	private void RemoveLeaderboardItems()
	{
		if (_listLeaderboardItems == null) return;

		for (int i = 0; i < _listLeaderboardItems.Length; i++) 
		{
			Destroy(_listLeaderboardItems[i]);
			_listLeaderboardItems[i] = null;
		}

		_baseLeaderboardItem.GetComponent<CanvasGroup>().alpha = 0;
	}

    private void StartTransitionOut()
    {
        OnClickComplete -= StartTransitionOut;

        RemoveLeaderboardItems();

		buttonPanel.GetComponent<Image>().color = new Color(0,0,0,0f);

        overallButton.GetComponent<CanvasGroup>().alpha = 0;
		passengerButton.GetComponent<CanvasGroup>().alpha = 0;
		lightTruckButton.GetComponent<CanvasGroup>().alpha = 0;
		winterButton.GetComponent<CanvasGroup>().alpha = 0;

		tableHeaderPanel.GetComponent<Image>().color = new Color(70f / 255f, 70f / 255f, 70f / 255f, 0f);
		title_RankText.GetComponent<Text>().color = elementHideColor;
		title_NameText.GetComponent<Text>().color = elementHideColor;
		title_TimeText.GetComponent<Text>().color = elementHideColor;

		LeanTween.alpha (superTireSmallLogo, 0f, 0.4f).setEase (LeanTweenType.easeOutQuad);

		LeanTween.scale(leaderboardBanner, new Vector3(0f, 1f, 1f), 0.85f)
			.setDelay(0.35f)
			.setEase(LeanTweenType.easeOutBack);

		LeanTween.scale(leaderboardBg, new Vector3(0f, 1f, 1f), 0.95f)
			.setDelay(0.55f)
			.setOvershoot(0.95f)
			.setEase(LeanTweenType.easeOutBack);

        // Set Progress Complete Event 
        OnProgressLoadingTransitionInComplete += ProgressLoadingTransitionInComplete;

        base.OpenLoadingPanel();
    }

    public override void Remove()
    {
        base.Remove();


    }
}
