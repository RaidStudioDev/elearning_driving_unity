<?php
error_reporting(E_ALL);
ini_set('display_errors', '1');

include_once 'config.php';
include_once 'class.switchback.php';

/*
	query += "?e=" // Email;
	query += "&time=" // Time;
	query += "&type=" // Track Type;
	query += "&index=" // Challenge Index ;

	https://CLIENTURL/api/UpdateTimeByTrackType.php?e=email@email.com&time=89&type=island&index=2

	* Saves the track time to its specific track type.
	* Silently saves the users progress by challenge index.
*/

$isDebug = (isset($_GET['d'])) ? true : false;


if(isset($_GET['e']) && isset($_GET['mode']) && isset($_GET['uid']) && isset($_GET['time']) && isset($_GET['type']) && isset($_GET['index']))
{
	trace("*UpdateTimeByTrackType*");

	$sb = new SWITCHBACK($DB_con);

	$email = $_GET['e'];
	$gamemode = $_GET['mode'];
	$track_uid = $_GET['uid'];
	$track_type = $_GET['type'];
	$track_time = $_GET['time'];
	$challengeindex = $_GET['index'];

	// get user info
	$userInfo = $users->fetchUserInfoByEmail($email);

	if (!$userInfo)
	{
		onErrorMessage("User not found.");
		exit;
	}

	$userID = $userInfo["id"];

	trace("UserID: " .  $userID);
	trace("GameMode: " .  $gamemode);
	trace("TrackUID: " .  $track_uid);
	trace("ChallengeIndex: " .  $challengeindex);

	// save progress by challenge index
	saveProgressSilent($sb, $userID, $gamemode, $challengeindex);

	// get current tracks
	$getCurrentTrackData = $sb->getCurrentTracksPlayed($userID, $gamemode);
	if ($getCurrentTrackData["success"])
	{
		$totalTracksComplete = count($getCurrentTrackData["track_data"]);
		trace("TotalTracksComplete: " . $totalTracksComplete);
		trace("MAX_TRACKS: " . $MAX_TRACKS);
		if ($totalTracksComplete < $MAX_TRACKS)
		{
			trace("Creating Track Record");
			$addTrack = $sb->createCurrentTrackRecord($userInfo, $gamemode, $track_uid, $track_type, $track_time);
			if ($addTrack["success"])
			{
				onSuccessMessage("Track Record has been added.", $addTrack);
			}
			else
			{
				onErrorMessage("Could not add Track record. Error: " . $addTrack["ERROR"]);
			}
		}
		else
		{
			onErrorMessage("Track index(".$challengeindex."), exceeded the maximum(".$MAX_TRACKS.") number of tracks.  Could not save Track info.");
		}
	}
	else
	{
		onErrorMessage("Could not get Track info.");
	}





	/*

	// check if track type already exists
	$isTrackTypeExist = $sb->checkIfTrackTypeExists($userInfo["id"], $track_type);

	if ($isTrackTypeExist["success"]) // track type has already been added
	{
		// get the previous track time
		$previousTrackTime = $sb->getTimeByTrackType($userInfo["id"], $track_type);

		if ($isDebug)
		{
			echo "previousTrackTime: " . $previousTrackTime["track_time"];
		}

		// compare
		if ($track_time > $previousTrackTime["track_time"])
		{
			// update score
			$updateTrackType = $sb->updateTrackRecord($userInfo, $track_type, $track_time);

			if (!$updateTrackType["success"])
			{
				onErrorMessage("Could not update Track Type record.");
			}
			else onSuccessMessage("Track has been updated.", $previousTrackTime["track_time"], $track_time);
		}
		else
		{
			if ($track_time == $previousTrackTime["track_time"])
			{
				onSuccessMessage("Previous track time is the same. Did not update.", $previousTrackTime["track_time"], $track_time);
			}
			else
			{
				onSuccessMessage("Previous track time is greater. Did not update.", $previousTrackTime["track_time"], $track_time);
			}
		}

	}
	else // track type does not exist, create it
	{
		$addTrackType = $sb->createTrackRecord($userInfo, $track_type, $track_time);

		if (!$addTrackType["success"])
		{
			onErrorMessage("Could not create Track Type record.");
		}
		else onSuccessMessage("Track has been created.");
	}*/
}
else
{
	onErrorMessage("Invalid entry.");
}

function saveProgressSilent($sb, $userid, $gamemode, $challengeindex)
{
	trace("saveProgressSilent.UserID: " . $userid);
	trace("saveProgressSilent.ChallengeIndex: " . $challengeindex);

	// user added, update
	$updateUserChallengeIndex = $sb->updateUserChallengeIndex($userid, $gamemode, $challengeindex);

	if (!$updateUserChallengeIndex["success"])
	{
		onErrorMessage("Could not update challenge index.");
	}
	else
	{
		trace(json_encode($updateUserChallengeIndex));
	}
}

/*function onSuccessMessage($msg, $previousTrackTime = null, $track_time = null)
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
}*/
?>
