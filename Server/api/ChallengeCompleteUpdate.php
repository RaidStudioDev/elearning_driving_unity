<?php
error_reporting(E_ALL);
ini_set('display_errors', '1');

include_once 'config.php';
include_once 'class.switchback.php';

/*
	query = "?e="; 			// Email
	query += "&mode="; 		// GameMode
	query += "&uid=";		// Track UID
	query += "&time="; 		// Track Time
	query += "&type="; 		// Track Type
	query += "&index=";		// Challenge Index

	https://CLIENTURL/api/ChallengeCompleteUpdate.php

	* Saves the track time
	* Adds Track Record
	* Saves the users progress
*/

if (isset($_GET['e']) && isset($_GET['mode']) && isset($_GET['uid']) && isset($_GET['time']) && isset($_GET['type']) && isset($_GET['index']))
{
	trace("*ChallengeCompleteUpdate*");

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
		onErrorMessage("Invalid entry.");
		exit;
	}

	$userID = $userInfo["id"];

	trace("UserID: " .  $userID);
	trace("GameMode: " .  $gamemode);
	trace("TrackUID: " .  $track_uid);
	trace("ChallengeIndex: " .  $challengeindex);

	// TESTER
	$completionTest = getAllTracksCompletion($sb, $userID);
	$addTestTrack["completion"] = $completionTest;
	$timesTest = getAllCircuitRecordTimes($sb, $userID);
	$addTestTrack["times"] = $timesTest;
	$addTestTrack["currentCircuitTime"] = getCurrentCircuitTime($sb, $userID, $gamemode);

	// save progress by challenge index
	saveProgressSilent($sb, $userID, $gamemode, $challengeindex);

	// get current tracks
	$getCurrentTrackData = $sb->getCurrentTracksPlayed($userID, $gamemode);
	$hasRemovedPreviousCurrentTracks = false;
	if ($getCurrentTrackData["success"])
	{
		$totalTracksComplete = count($getCurrentTrackData["track_data"]);
		trace("TotalTracksComplete: " . $totalTracksComplete);
		trace("MAX_TRACKS: " . $MAX_TRACKS);

		// here we check our Current Track Data if user has played before
		// If completed tracks is more than 0 and index is at 1
		// this means the user is replaying the circuit
		if ($totalTracksComplete > 0 && $challengeindex == 1)
		{
			// first remove completed tracks
			$removeCurrentTrackSet = $sb->removeCurrentTrackSet($userID, $gamemode);

			$hasRemovedPreviousCurrentTracks = true;
			$totalTracksComplete = 0;

			if (!$removeCurrentTrackSet["success"])
			{
				onErrorMessage("Error: " . $removeCurrentTrackSet["ERROR"]);
				exit;
			}
		}

		if ($totalTracksComplete < $MAX_TRACKS)
		{
			trace("Creating Track Record");
			$addTrack = $sb->createCurrentTrackRecord($userInfo, $gamemode, $track_uid, $track_type, $track_time);
			if ($addTrack["success"])
			{
				$completion = getAllTracksCompletion($sb, $userID);
				$addTrack["completion"] = $completion;

				$times = getAllCircuitRecordTimes($sb, $userID);
				$addTrack["circuitRecordTimes"] = $times;

				$addTrack["currentCircuitTime"] = getCurrentCircuitTime($sb, $userID, $gamemode);

				// get record time by game mode
				$circuitRecordTime = getRecordTimeByGameMode($sb, $userID, $gamemode);
				$addTrack["userid"] = $circuitRecordTime["userid"];
				$addTrack["fullname"] = $circuitRecordTime["fullname"];
				$addTrack["gamemode"] = $circuitRecordTime["gamemode"];
				$addTrack["currentCircuitRecordTime"] = $circuitRecordTime["total_time"];

				onSuccessMessage("Track Record has been added. Removed Tracks: " . $hasRemovedPreviousCurrentTracks, $addTrack);
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
		// onErrorMessage("Could not get Track info.");

		trace("Creating Track Record");
		$addTrack = $sb->createCurrentTrackRecord($userInfo, $gamemode, $track_uid, $track_type, $track_time);
		if ($addTrack["success"])
		{
			$completion = getAllTracksCompletion($sb, $userID);
			$addTrack["completion"] = $completion;

			$times = getAllCircuitRecordTimes($sb, $userID);
			$addTrack["circuitRecordTimes"] = $times;

			$addTrack["currentCircuitTime"] = getCurrentCircuitTime($sb, $userID, $gamemode);

			// get record time by game mode
			$circuitRecordTime = getRecordTimeByGameMode($sb, $userID, $gamemode);
			$addTrack["userid"] = $circuitRecordTime["userid"];
			$addTrack["fullname"] = $circuitRecordTime["fullname"];
			$addTrack["gamemode"] = $circuitRecordTime["gamemode"];
			$addTrack["currentCircuitRecordTime"] = $circuitRecordTime["total_time"];

			onSuccessMessage("Track Record has been added.", $addTrack);
		}
		else
		{
			onErrorMessage("Could not add Track record. Error: " . $addTrack["ERROR"]);
		}
	}
}
else
{
	onErrorMessage("Invalid entry.");
}

function getRecordTimeByGameMode($sb, $userid, $gamemode)
{
	$getOverallListByGameMode = $sb->getOverallListByGameMode($gamemode);

	if ($getOverallListByGameMode["success"])
	{
		$recordTime = 0;
		$timeList = $getOverallListByGameMode["list"];
		$timeLength = count($timeList);

		if ($timeLength >= 1)
		{
			$returnData["userid"] = $timeList[0]["userid"];
			$returnData["fullname"] = $timeList[0]["fullname"];
			$returnData["gamemode"] = $timeList[0]["gamemode"];
			$returnData["total_time"] = $timeList[0]["total_time"];
		}
		else
		{
			$returnData["userid"] = "";
			$returnData["fullname"] = "";
			$returnData["gamemode"] = $gamemode;
			$returnData["total_time"] = 0;
		}

		return $returnData;
	}

	$returnData["currentCircuitRecordTime"] = 0;
	return $returnData;
}


function getAllTracksCompletion($sb, $userid)
{
	// check passenser game mode if completed
	$getPassengerTrackData = $sb->getCurrentTracksCompletedByGameMode($userid, "passenger");
	$isPassengerCompleted = false;
	if ($getPassengerTrackData["success"])
	{
		trace(json_encode($getPassengerTrackData));

		if (count($getPassengerTrackData["track_data"]) == $GLOBALS["MAX_TRACKS"])
		{
			$isPassengerCompleted = true;
		}

	}
	else
	{
		trace("passenger not completed");
	}

	// check truck game mode if completed
	$getTruckTrackData = $sb->getCurrentTracksCompletedByGameMode($userid, "trucks");
	$isTrucksCompleted = false;
	if ($getTruckTrackData["success"])
	{
		trace(json_encode($getTruckTrackData));
		if (count($getTruckTrackData["track_data"]) == $GLOBALS["MAX_TRACKS"])
		{
			$isTrucksCompleted = true;
		}
	}
	else
	{
		trace("trucks not completed");
	}

	// check winter game mode if completed
	$getWinterTrackData = $sb->getCurrentTracksCompletedByGameMode($userid, "winter");
	$isWinterCompleted = false;
	if ($getWinterTrackData["success"])
	{
		trace(json_encode($getWinterTrackData));
		if (count($getWinterTrackData["track_data"]) == $GLOBALS["MAX_TRACKS"])
		{
			$isWinterCompleted = true;
		}
	}
	else
	{
		trace("winter not completed");
	}

	$completion = array("passenger"=>$isPassengerCompleted,"trucks"=>$isTrucksCompleted,"winter"=>$isWinterCompleted);
	trace("completion: " . json_encode($completion));

	return $completion;
}

function getCurrentCircuitTime($sb, $userid, $gamemode)
{
	$getCurrentTimeSumFromUserTracks = $sb->getCurrentTimeSumFromUserTracks($userid, $gamemode);

	trace(json_encode($getCurrentTimeSumFromUserTracks));

	return $getCurrentTimeSumFromUserTracks["track_time_sum"];
}

function getAllCircuitRecordTimes($sb, $userid)
{
	// check passenser best circuit times
	$getPassengerCircuitTime = $sb->getOverallListByGameMode("passenger");

	$isPassengerTime = 0;
	if ($getPassengerCircuitTime["success"])
	{
		trace(json_encode($getPassengerCircuitTime));

		$timeList = $getPassengerCircuitTime["list"];
		$timeLength = count($timeList);

		if ($timeLength >= 1)
			$isPassengerTime = $timeList[0]["total_time"];
		else
			$isPassengerTime = 0;
	}
	else
	{
		trace("passenger not completed");
	}


	// check trucks best circuit times
	$getTrucksCircuitTime = $sb->getOverallListByGameMode("trucks");

	$isTrucksTime = 0;
	if ($getTrucksCircuitTime["success"])
	{
		trace(json_encode($getTrucksCircuitTime));

		$timeList = $getTrucksCircuitTime["list"];
		$timeLength = count($timeList);

		if ($timeLength >= 1)
			$isTrucksTime = $timeList[0]["total_time"];
		else
			$isTrucksTime = 0;
	}
	else
	{
		trace("trucks not completed");
	}


	// check passenger best circuit times
	$getWinterCircuitTime = $sb->getOverallListByGameMode("winter");

	$isWinterTime = 0;
	if ($getWinterCircuitTime["success"])
	{
		trace(json_encode($getWinterCircuitTime));

		$timeList = $getWinterCircuitTime["list"];
		$timeLength = count($timeList);

		if ($timeLength >= 1)
			$isWinterTime = $timeList[0]["total_time"];
		else
			$isWinterTime = 0;
	}
	else
	{
		trace("winter not completed");
	}

	$times = array("passenger"=>$isPassengerTime,"trucks"=>$isTrucksTime,"winter"=>$isWinterTime);
	trace("times: " . json_encode($times));

	return $times;
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
?>
