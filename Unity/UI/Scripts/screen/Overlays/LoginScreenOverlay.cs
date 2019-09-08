using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginScreenOverlay : BaseScreenOverlay 
{
	private InputField _emailInputField;
	private InputField _passwordInputField;
    private Button _loginButton;
    private RectTransform _spinningTire;
    private RectTransform _spinningTireText;
    private RectTransform _submitLoader;

    // warning CS0414: The private field `Race.CarnivalLoop' is assigned but its value is never used
#pragma warning disable 414 
    private bool _hasSubmitted = false;
#pragma warning restore 414

    protected override void Awake()
    {
        base.Awake();

        _elements["EmailInputField"].localScale = new Vector3(0f, 1f, 1f);
        _elements["EmailInputField"].localScale = new Vector3(0f, 1f, 1f);
        _elements["PasswordInputField"].localScale = new Vector3(0f, 1f, 1f);
        _elements["LoginButton"].localScale = new Vector3(0f, 0f, 1f);

        LeanTween.alphaText(_elements["SplashText"], 0f, 0f);
        LeanTween.alphaText(_elements["ErrorMessage"], 0f, 0f);

        _emailInputField = _elements["EmailInputField"].GetComponent<InputField>();
        _passwordInputField = _elements["PasswordInputField"].GetComponent<InputField>();
        _loginButton = _elements["LoginButton"].GetComponent<Button>();
        _spinningTire = _elements["SubmitLoader"].Find("SpinningTire").GetComponent<RectTransform>();
        _submitLoader = _elements["SubmitLoader"];
    }

    // Use this for initialization
    public override void Initialize() 
	{
        _emailInputField.inputType = InputField.InputType.Standard;
        _emailInputField.keyboardType = TouchScreenKeyboardType.EmailAddress;
        _passwordInputField.inputType = InputField.InputType.Password;

#if UNITY_EDITOR
        SubmitLogin();
#else
        LeanTween.scale(_elements["EmailInputField"], new Vector3(1f, 1f, 1f), 0.5f)
			.setDelay(0.1f)
			.setOvershoot (0.5f)
			.setEase(LeanTweenType.easeOutBack).setIgnoreTimeScale(true);

        LeanTween.scale(_elements["PasswordInputField"], new Vector3(1f, 1f, 1f), 0.5f)
            .setDelay(0.1f)
            .setOvershoot(0.5f)
            .setEase(LeanTweenType.easeOutBack).setIgnoreTimeScale(true);

        LeanTween.alphaText(_elements["SplashText"], 1f, 0.55f).setEase(LeanTweenType.easeOutCubic).setDelay(0.25f).setIgnoreTimeScale(true);
	
		LeanTween.scale(_elements["LoginButton"], new Vector3(1f, 1f, 1f), 0.55f).setDelay(0.35f)
			.setOvershoot(0.75f)
			.setIgnoreTimeScale(true)
			.setEase(LeanTweenType.easeOutBack)
			.setOnComplete(() => {

                AddEvents();

            });
#endif


    }

    private void AddEvents()
    {
        _loginButton.onClick.AddListener(OnLoginButtonClick);

        _emailInputField.onEndEdit.AddListener(delegate { OnEmailEndEdit(_passwordInputField); });
        _passwordInputField.onEndEdit.AddListener(delegate { OnPasswordEndEdit(_passwordInputField); });
    }

    private void RemoveEvents()
    {
        _loginButton.onClick.RemoveListener(OnLoginButtonClick);
        _emailInputField.onEndEdit.RemoveAllListeners();
        _passwordInputField.onEndEdit.RemoveAllListeners();
    }

    /* Triggered when user hits ENTER or FocusOUT */
    private void OnEmailEndEdit(InputField inputField)
    {
        _passwordInputField.Select();
    }

    /* Triggered when user hits ENTER or FocusOUT */
    private void OnPasswordEndEdit(InputField inputField)
    {
        SubmitLogin();
    }

    private void ShowErrorMessage(string msg)
    {
        _elements["ErrorMessage"].GetComponent<Text>().text = msg;

        LeanTween.alphaText(_elements["ErrorMessage"], 1f, 0.55f).setEase(LeanTweenType.easeOutCubic).setDelay(0.25f).setIgnoreTimeScale(true);
    }

    private void OnLoginButtonClick()
	{
        SubmitLogin();
	}

    private void SubmitLogin()
    {
        UIManager.Instance.soundManager.PlaySound("PlaySineWaveHighPitch");

        _hasSubmitted = true;
        _loginButton.enabled = false;

        // clear error msg
        LeanTween.alphaText(_elements["ErrorMessage"], 0f, 0f);

        // temporary login info
        Dictionary<string, string> parameters = PersistentModel.Instance.GetTempUser();
       
        StartCoroutine(ValidationSuccessfull(parameters));
    }

    WaitForSeconds waitSecAssetLoad = new WaitForSeconds(2.00f);
    WaitForSeconds waitHalfSecAssetLoad = new WaitForSeconds(0.25f);
    private IEnumerator ValidationSuccessfull(Dictionary<string, string> parameters)
    {
        DebugLog.Trace("LoginScreenOverlay.ValidationSuccessfull()");

        _elements["EmailInputField"].localScale = new Vector3(0f, 1f, 1f);
        _elements["PasswordInputField"].localScale = new Vector3(0f, 1f, 1f);
        _elements["LoginButton"].localScale = new Vector3(0f, 0f, 1f);
        _elements["SplashText"].localScale = new Vector3(0f, 0f, 1f);
        _elements["ErrorMessage"].localScale = new Vector3(0f, 0f, 1f);

        LeanTween.alpha(_elements["Panel"], 0.75f, 0.5f).setEase(LeanTweenType.easeOutCubic).setDelay(0f);

        yield return waitHalfSecAssetLoad;

        LeanTween.alphaCanvas(_submitLoader.GetComponent<CanvasGroup>(), 1f, 0.75f).setEase(LeanTweenType.easeOutQuad);

        LeanTween.rotateAroundLocal(_spinningTire, Vector3.back, 360f, 1f).setRepeat(-1);
        LeanTween.scale(_spinningTire, new Vector3(1f, 1f, 1f), 0.85f).setEase(LeanTweenType.easeInOutQuad);

        yield return waitSecAssetLoad;

        ServerConnectionStart(parameters);
    }

    private void ServerConnectionStart(Dictionary<string, string> parameters)
    {
        DebugLog.Trace("LoginScreenOverlay.ServerConnectionStart()");

        LeanTween.cancel(_spinningTire);

        PersistentModel.Instance.Server.isPasscodeAuthorized = true;
        PersistentModel.Instance.UpdateUserParameters(parameters);
        PersistentModel.Instance.Server.OnGetUserDataAttemptForRequestComplete += OnGetUserDataAttemptForRequestComplete;
        StartCoroutine(PersistentModel.Instance.Server.GetUserDataAttemptForRequest());

    }

    private void OnGetUserDataAttemptForRequestComplete(bool success, ServerData resultData)
    {
        DebugLog.Trace("LoginScreenOverlay.OnGetUserDataAttemptForRequestComplete()");

        PersistentModel.Instance.Server.OnGetUserDataAttemptForRequestComplete -= OnGetUserDataAttemptForRequestComplete;

        LeanTween.rotateAroundLocal(_spinningTire, Vector3.back, 360f, 1f).setRepeat(-1);
        LeanTween.alpha(_spinningTire, 0, 0.65f).setEase(LeanTweenType.easeOutCubic);
        LeanTween.alpha(_elements["Panel"], 0f, 0.5f)
            .setEase(LeanTweenType.easeOutCubic)
            .setDelay(0.25f)
            .setOnComplete(() => {

                base.OnCloseOverlay();

            });
    }

    public override void Remove()
	{
        RemoveEvents();

        base.Remove();
	}
}
