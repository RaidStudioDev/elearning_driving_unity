<?php
include_once 'config.php';
include_once 'class.switchback.php';

/*
	query += "?e=" // Email;
	query += "&index=" // Challenge Index ;

	https://CLIENTURL/api/UpdateChallengeIndex.php?e=email@email.com&index=0

	* Saves the track time to its specific track type.
	* Silently saves the users progress by challenge index.
*/

$isDebug = (isset($_GET['d'])) ? true : false;

if(isset($_GET['e']) && isset($_GET['index']))
{
	$sb = new SWITCHBACK($DB_con);

	$email = $_GET['e'];
	$challengeindex = $_GET['index'];

	// get user info
	$userInfo = $users->fetchUserInfoByEmail($email);

	// save progress by challenge index
	$update = updateChallengeIndex($sb, $userInfo["id"], $challengeindex);

	if ($update["success"]) // track type has already been added
	{
		onSuccessMessage("Challenge Index has been updated.");
	}
	else // track type does not exist, create it
	{
		onErrorMessage("An error occurred when updating the Challenge Index.");
	}
}
else
{
	onErrorMessage("Invalid entry.");
}

function updateChallengeIndex($sb, $userid, $challengeindex)
{
	// check if user has been added
	$isUserAdded = $sb->fetchIdByUserId($userid);

	if ($isUserAdded)
	{
		// user added, update
		$updateUserChallengeIndex = $sb->updateUserChallengeIndex($userid, $challengeindex);

		if (!$updateUserChallengeIndex["success"])
		{
			$response["success"] = false;
		}
		else
		{
			$response["success"] = true;
		}

		return $response;
	}
	else
	{
		$response["success"] = false;

		return $response;
	}
}

function onSuccessMessage($msg, $previousTrackTime = null, $track_time = null)
{
	$response["success"] = true;
	$response["status"] = $msg;
	if ($previousTrackTime != null) $response["previousTrackTime"] = $previousTrackTime;
	if ($track_time != null) $response["track_time"] = $track_time;
	//$serverResponse["data"] = $response;
	//echo json_encode($serverResponse);

	$serverResponse["data"] = array($response);
	echo json_encode($serverResponse);

}

function onErrorMessage($msg)
{
	$response["success"] = false;
	$response["status"] = $msg;
	// $serverResponse["data"] = $response;
	// echo json_encode($serverResponse);

	$serverResponse["data"] = array($response);
	echo json_encode($serverResponse);

	exit;
}
?>
