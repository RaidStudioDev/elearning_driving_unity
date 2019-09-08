<?php
include_once 'config.php';
include_once 'class.switchback.php';

/*
	query += "?e=" // Email;
	query += "?r=" // Region;

	https://CLIENTURL/api/GetRegionRank.php
*/

if(isset($_GET['e']))
{
	$sb = new SWITCHBACK($DB_con);

	$email = $_GET['e'];

	// get user info
	$userInfo = $users->fetchUserInfoByEmail($email);

	// check if user exists, if not don't continue
	if ($userInfo["id"] == null) onErrorMessage("Invalid entry.");

	// get region
	$region = $userInfo["region"];

	// get org
	$org = $userInfo["org"];

	// get top 10 list
	$loadRank = $sb->loadRank($email);

	// get region ranking
	$loadRegionRank = $sb->loadRegionRank($email, $region);

	// get organization ranking
	$loadOrgRank = $sb->loadOrgRank($email, $org);

	if ($loadRank["success"] && $loadRegionRank["success"] && $loadOrgRank["success"])
	{
		if (count($loadRank["list"]) > 0)
		{
			$response["rank"] = $loadRank["list"][0]["rank"];
			$response["regionRank"] = $loadRegionRank["list"][0]["rank"];
			$response["orgRank"] = $loadOrgRank["list"][0]["rank"];
		}
		else
		{
			$response["success"] = false;
			$response["error"] = "No tracks found";
		}

		$serverResponse["data"] = array($response);

		if ($isDebug) highlight_string( var_export($serverResponse, true));
		else echo json_encode($serverResponse);
	}
	else // track type does not exist, create it
	{
		onErrorMessage($loadRank["status"]);
	}
}
else
{
	onErrorMessage("Invalid entry.");
}

function onSuccessMessage($msg, $response)
{
	$response["success"] = true;
	$response["status"] = $msg;
	$serverResponse["data"] = $response;
	echo json_encode($serverResponse);
}

function onErrorMessage($msg)
{
	$response["success"] = false;
	$response["status"] = $msg;
	$serverResponse["data"] = $response;
	echo json_encode($serverResponse);
	exit;
}
?>
