<?php
include_once 'config.php';
include_once 'class.switchback.php';

/*
	query = "?e="; 			// Email
	query += "&mode="; 		// GameMode

	https://CLIENTURL/api/CircuitCompleteUpdate.php

	* Replaces Completed Tracks with Current Track if TotalTime is better
*/

if (isset($_GET['e']) && isset($_GET['mode']))
{
	trace("*CircuitCompleteUpdate*");

	$sb = new SWITCHBACK($DB_con);

	$email = $_GET['e'];
	$gamemode = $_GET['mode'];

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

	// switchback_data = current tracks being played, not completed yet
	// switchback_tracks = saved tracks completed

	// first check if we have an exisiting record of the challenges
	// in switchback_tracks
	// we check if user completed circuit ( all tracks )
	// if we do find existing tracks, we will compare total time
	// between the switchback_data to the switchback_tracks
	$getTrackData = $sb->getTracksCompletedByGameMode($userID, $gamemode);

	$totalAllCircuitsTime = 0;

	// get user's total time for all circuits from switchback_data
	$getUserOverallTime = $sb->getUserOverallTime($userID);
	if ($getUserOverallTime["success"])
	{
		$totalAllCircuitsTime = $getUserOverallTime["total_time"];
	}


	trace("getTrackData: " . json_encode($getTrackData));

	if ($getTrackData["success"])
	{
		trace("Tracks found in switchback_tracks. Total count: " . count($getTrackData["track_data"]));

		$amountOfTracks = $GLOBALS["MAX_TRACKS"];
		if (count($getTrackData["track_data"]) < $amountOfTracks)
		{
			trace("No Update. User has not completed circuit.");
			onSuccessMessage("User has not completed circuit.", null);
			exit;
		}


		// get current total time for switchback_data
		$getCurrentTotalTime = $sb->getCurrentTimeSumFromUserTracks($userID, $gamemode);
		trace("Current Total Time: " . $getCurrentTotalTime["track_time_sum"]);

		// get completed total time for switchback_tracks
		$getCompletedTotalTime = $sb->getTimeSumFromUserTracks($userID, $gamemode);
		trace("Completed Total Time: " . $getCompletedTotalTime["track_time_sum"]);

		// if we have a better current total time, lets replace
		if ($getCurrentTotalTime["track_time_sum"] <  $getCompletedTotalTime["track_time_sum"])
		{
			trace("Replacing Time from " . $getCompletedTotalTime["track_time_sum"] . " TO " . $getCurrentTotalTime["track_time_sum"]);
			trace("userID: " . $userID . " gamemode: " . $gamemode);

			$removeCompletedTrackSet = null;

			// first remove completed tracks
			$removeCompletedTrackSet = $sb->removeCompletedTrackSet($userID, $gamemode);

			if (!$removeCompletedTrackSet["success"])
			{
				onErrorMessage("Error: " . $removeCompletedTrackSet["ERROR"]);
				exit;
			}

			trace("Completed Tracks Removed: " . json_encode($removeCompletedTrackSet));

			// then add the current tracks
			// get current tracks
			$getCurrentTrackData = $sb->getCurrentTracksPlayed($userID, $gamemode);
			if ($getCurrentTrackData["success"])
			{
				$trackData = $getCurrentTrackData["track_data" 	];
				$trackLength = count($getCurrentTrackData["track_data"]);

				$addCompletedTrackSet = $sb->addCompletedTrackSet($userID, $trackData);

				// add all circuites
				$addCompletedTrackSet["totalAllCircuitsTime"] = $totalAllCircuitsTime;

				if ($addCompletedTrackSet["success"])
				{
					trace("*Tracks added*");
					onSuccessMessage("Tracks added", $addCompletedTrackSet);
				}
				else
				{
					onErrorMessage("Error: " . $addCompletedTrackSet["ERROR"]);
				}
			}
			else
			{
				onErrorMessage("Error: " . $getCurrentTrackData["ERROR"]);
			}
		}
		else
		{
			$addCompletedTrackSet["totalAllCircuitsTime"] = $totalAllCircuitsTime;

			trace("No Update. Current Tracks are not better then the Completed Tracks.");
			onSuccessMessage("No Update. Current Tracks are not better then the Completed Tracks.", $addCompletedTrackSet);
		}
	}
	else
	{
		// no tracks found, so no need to compare times
		// we can copy the track data to switchback tracks

		trace($getTrackData["ERROR"]);

		// get current tracks
		$getCurrentTrackData = $sb->getCurrentTracksPlayed($userID, $gamemode);
		if ($getCurrentTrackData["success"])
		{
			$trackData = $getCurrentTrackData["track_data"];
			$trackLength = count($getCurrentTrackData["track_data"]);

			$addCompletedTrackSet = $sb->addCompletedTrackSet($userID, $trackData);
			if ($addCompletedTrackSet["success"])
			{
				trace("*Tracks added*");

				// add all circuites
				$addCompletedTrackSet["totalAllCircuitsTime"] = $totalAllCircuitsTime;

				onSuccessMessage("Tracks added", $addCompletedTrackSet);
			}
			else
			{
				onErrorMessage("Error: " . $addCompletedTrackSet["ERROR"]);
			}
		}
		else
		{
			onErrorMessage("Error: " . $getCurrentTrackData["ERROR"]);
		}
	}
}
else
{
	onErrorMessage("Invalid entry.");
}


?>
