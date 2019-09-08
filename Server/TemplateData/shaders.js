function setupShadertoy(qId, forceID)
{
	var loadFrame = qId('sr-load-frame');

	setTimeout(function()
	{
		if (loadFrame)
		{	
			loadFrame.style.opacity = 1;
		}
	}, 1000);

	var keys = ["ltccRl","ltyGWD","llKcRR","ldSfRG","4tG3zG","4ljBRy","XldSD7","ld3BzM","4lXyWN","4dtXWN","4lyyzh","XssfR2","MlB3WK","Ml2XDt","4tVcRW","4lfSRM","XdcGDB","MdyfWz","4dsGDn","XlsBDf","ltXfzr","ldK3RW","4dlGRn","4dsczr","XljXz3","4tX3DM","lsffWs","Xls3WM","4l3yDB","XtsSzH","Mds3Wr","4sSXDG","Mdf3Dr"];
	
	console.log("forceID: ", forceID);
	if (forceID)
	{
		keys = [forceID];
	}
	
	var randomKey = Math.floor(Math.random() * (keys.length - 1));
	console.log(keys[randomKey]);
	if (loadFrame)
	{
		loadFrame.setAttribute('src', 'https://www.shadertoy.com/embed/' + keys[randomKey] + '?gui=false&paused=false')
	}

	if (!forceID)
	{
		qId('sr-loading-overlay').addEventListener('click', function()
		{
			var randomKey = Math.floor(Math.random() * (keys.length - 1));
			console.log('clicked', keys[randomKey]);
			if (loadFrame)
			{
				loadFrame.setAttribute('src', 'https://www.shadertoy.com/embed/' + keys[randomKey] + '?gui=false&paused=false')
			}
		});
	}
	
	
}
	
	