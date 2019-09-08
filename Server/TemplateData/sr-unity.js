(function () 
{
	window.UNITY_CONSTANTS = {
		"UNITY_WEB_NAME":"Bridgestone Switchback",
		"UNITY_WEBGL_LOADER_URL":"Build/UnityLoader.js",
		"UNITY_WEBGL_BUILD_URL":"Build/Build.json",
		"UNITY_WIDTH":960,"UNITY_HEIGHT":600
	}
	
	var unityCanvas = null;
	var hasMetaContainer = false;

	var qId = function (id) 
	{
		return document.getElementById(id);
	}
	
	var q = function (selector)
	{
		return document.querySelectorAll(selector)[0];
	}

	var gameContainer = qId('gameContainer');
	var progressBar = qId('sr-progress-bar');

	if (window.nosimmerload)
	{
		var logoEl = q('.sr-logo');
		logoEl.style.display = 'none';
	}

	function setupUnityLoader() 
	{
		// ignore mobile compatibility check and just run it!
		var originalCompatibilityCheck = UnityLoader.compatibilityCheck;
		UnityLoader.compatibilityCheck = function () 
		{
			var originalIsMobile = UnityLoader.SystemInfo.mobile;
			UnityLoader.SystemInfo.mobile = false; // pretend that we're not mobile for a sec
			originalCompatibilityCheck.apply(this, arguments);
			UnityLoader.SystemInfo.mobile = originalIsMobile; //then put it back
		}

		//turn off unity's annoying alert() dialog.
		UnityLoader.Error.handler = function () { }
	}
	
	function initializeGameInstance() 
	{
		var logoEl = q('.sr-logo');
		var gameInstance = window.gameInstance = UnityLoader.instantiate("gameContainer", window.baseUrl + '' + UNITY_CONSTANTS.UNITY_WEBGL_BUILD_URL, 
		{
			onProgress: function (gameInstance, progress) 
			{
				progressBar.style.width = (progress * 95) + '%';

				if (progress > 0.9999) 
				{
					unityCanvas = unityCanvas || document.getElementsByTagName('canvas')[0];
					setTimeout(function () 
					{ 
						//fake it, microsoft style!
						progressBar.style.width = '100%';

						var loadFrame = qId('sr-load-frame');
						if (loadFrame && loadFrame.parentNode)
						{
							loadFrame.parentNode.removeChild(loadFrame);
						}

						var loadOverlay = qId('sr-loading-overlay')
						if (loadOverlay && loadOverlay.parentNode)
						{
							loadOverlay.parentNode.removeChild(loadOverlay);
						}
				}, 700);
			}

			if (progress > .4)
			{
				var logoUrl = logoEl.getAttribute("data-logo-url");
				if (logoUrl)
				{
					logoEl.removeAttribute('data-logo-url');
					logoEl.setAttribute('src', logoUrl);
					logoEl.style.opacity = 0;

					setTimeout(function()
					{
						logoEl.style.transition = 'all 1s ease-in-out';
						logoEl.style.opacity = 1;
					},500);
				}
			}
		}
		});
	}

	function sendMessageOnGameLoad() 
	{
		//yes... this is bad code :-)
		var interval = setInterval(function () 
		{
			if (window.gameInstance && window.gameInstance.Module && window.gameInstance.Module.calledRun) 
			{
				clearInterval(interval);
				progressBar.style.width = '100%';

				unityCanvas = unityCanvas || document.getElementsByTagName('canvas')[0];
				qId('sr-loading-container').style.opacity = '0';
				qId('sr-loading-container').style.pointerEvents = 'none';
				if (window.showControls) 
				{
					qId('sr-game-controls').style.display = 'block';
				}
				else
				{
					qId('sr-game-controls').style.display = 'none';
				}
			}
		}, 50);
	}

	function setDimensions() 
	{
		var META_CONTAINER_WIDTH = hasMetaContainer ? 260 : 0;
		var UNITY_W = UNITY_CONSTANTS.UNITY_WIDTH;
		var UNITY_H = UNITY_CONSTANTS.UNITY_HEIGHT;
		var winW = window.innerWidth - META_CONTAINER_WIDTH;
		var winH = window.innerHeight;
		var scale = Math.min(winW / UNITY_W, winH / UNITY_H);

		var fitW = Math.round(UNITY_W * scale * 100) / 100;
		var fitH = Math.round(UNITY_H * scale * 100) / 100;

		gameContainer.style.width = fitW + 'px';
		gameContainer.style.height = fitH + 'px';
		if (unityCanvas) 
		{
		  unityCanvas.setAttribute('width', fitW);
		  unityCanvas.setAttribute('height', fitH);
		}
	}

	var isFullScreen = false;
	function requestFullScreen() 
	{
		var element = document.body;
		
		// Supports most browsers and their versions.
		var requestMethod = element.requestFullScreen || element.webkitRequestFullScreen || element.mozRequestFullScreen || element.msRequestFullScreen;
	
		if (requestMethod && !isFullScreen) 
		{ 	
			// Native full screen.
			console.log("FULL SCREEN");
			isFullScreen = true;
			
			requestMethod.call(element);
		}
		else 
		{
			// Supports most browsers and their versions.
		
			var requestCancelMethod = document.exitFullscreen || document.webkitExitFullscreen || document.mozCancelFullScreen || document.msExitFullscreen;
		
			if (requestCancelMethod)
			{
				isFullScreen = false;
				if (document.exitFullscreen) {
					document.exitFullscreen();
				} else if (document.mozCancelFullScreen) {
					document.mozCancelFullScreen();
				} else if (document.webkitExitFullscreen) {
					document.webkitExitFullscreen();
				} else if (document.msExitFullscreen) {
					document.msExitFullscreen();
				}
			}
		}
	}

	var debounceTimeout = null;
	function debouncedSetDimensions() 
	{
		if (debounceTimeout !== null) {
			clearTimeout(debounceTimeout);
		}
		debounceTimeout = setTimeout(setDimensions, 200);
	}

	window.addEventListener('resize', debouncedSetDimensions, false);
	window.addEventListener("message", function (event) 
	{
		if (event.data === 'requestFullscreen') 
		{
			gameInstance.SetFullscreen(1);
		}
	
	}, false);

	function startAudioOnClick()
	{
		window.AudioContext = window.AudioContext || window.webkitAudioContext;
		var bind = Function.bind;
		var unbind = bind.bind(bind);

		
		
		function instantiate(constructor, args) {
			return new (unbind(constructor, null).apply(null, args));
		}

		window.AudioContext = function (AudioContext) 
		{
			return function () 
			{
				var audioContext = instantiate(AudioContext, arguments);
				window.myAudioContext = audioContext;
				console.log('AudioContext has been instantiated!');
				return audioContext;
			}
		}(AudioContext);

		var webAudioEnabled = false;

		function resumeAudio()
		{
			if (!webAudioEnabled && window.myAudioContext)
			{
				console.log('Starting Audio!');
				window.myAudioContext.resume();
				webAudioEnabled = true;
			}
		}

		document.body.addEventListener('click', resumeAudio, true);
		document.addEventListener('keydown', resumeAudio, true);
		document.body.addEventListener('touchstart', resumeAudio, true);
		document.addEventListener('touchstart', resumeAudio, true);
	}

	

	function initialize() 
	{
		qId('sr-loading-container').style.opacity = '1';

		var loadingEl = q('.sr-loading-container-radial');
		var playEl = q('.sr-play-container');
		var loadingContainerEl = q('#sr-loading-container');
		var bodyEl = q('body');
		
		// autoplay is set for any browsers that is not Safari
		if (window.autoplay) 
		{
			loadingEl.style.display = 'block';
			if (!window.userNotLogged) initializeGameInstance();
			else removeProgressBar();
		}
		else 
		{
			// if no user credentials and on a Mac 
			if (window.userNotLogged && window.isMacintosh)
			{
				loadingEl.style.display = 'block';
				removeProgressBar();
			}
			else	
			{
				// If we are here, we are running on a Mac 
				// We need to display an intro splash with a play button
				// The play button will trigger a sound and then load 
				// the Unity game.  
				// We do this to avoid no sound from playing.
				playEl.style.display = 'block';
				bodyEl.style.cursor = 'pointer';
				if (window.imageUrl) 
				{
					loadingContainerEl.style.backgroundImage = "url('" + window.imageUrl + "')";
				}

				function playClickListener() 
				{
					document.getElementById("sound").play();
					
					document.removeEventListener('click', playClickListener);
					playEl.style.display = 'none';
					bodyEl.style.cursor = null;
					loadingContainerEl.style.backgroundImage = null;
					loadingEl.style.display = 'block';
					if (!window.userNotLogged) initializeGameInstance();
					else removeProgressBar();
				}

				document.addEventListener('click', playClickListener);
			}
		}
		
		sendMessageOnGameLoad();

		document.getElementById("fullScreenButton").addEventListener('click', requestFullScreen);
	}
	
	function removeProgressBar()
	{
		var progressBar = q('.cssProgress').style.display = 'none';
	}
	
	if (!window.urlParams.fullname 
	&& !window.urlParams.d)
	{
		window.showControls = false;
		window.userNotLogged = true;
		
		startAudioOnClick();
		setupUnityLoader();
		setDimensions();
		initialize();
		setupShadertoy(qId, '4lXyWN');
	}
	else
	{
		startAudioOnClick();
		setupUnityLoader();
		setDimensions();
		
		initialize();
	}
	
	console.log(new Date());

})();


