<?php
include_once 'config.php';
include_once 'class.switchback.php';

/*
	query = "?e="; 			// Email
	query += "&mode_*="; 	// Unlimited # of Modes

	// for example
	query += "&mode_1="; 	// Mode 1 * must start at index = 1
	query += "&mode_2="; 	// Mode 2

	https://CLIENTURL/api/StartNewGame.php

	* Passes in the modes in sequential fashion
	* Removes All Current Tracks from switchback_data
	* Resets SwitchBack User Profile - clears gamemode && challengeIndex

	* NOTE: All Completed Circuit Tracks (switchback_tracks) will still remain for leaderboards
	* If user has a better TotalTime for all tracks in a circuit, we will then replace
*/

if (isset($_GET['e']))
{
	trace("*StartNewGame*");

	$sb = new SWITCHBACK($DB_con);

	$email = $_GET['e'];

	// get user info
	$userInfo = $users->fetchUserInfoByEmail($email);
	if (!$userInfo)
	{
		onErrorMessage("Invalid entry.");
		exit;
	}

	$userID = $userInfo["id"];
	trace("UserID: " .  $userID);


	// search for mode_* properties in query
	trace("Searching for modes");

	$sqlStmt = "DELETE FROM switchback_data WHERE userid = :userid AND gamemode IN ";

	$modesFound = false;
	$lastMode = false;
	$modeIndex = 0;
	$querySegment = "(";

	$MODES["mode_0"] = "passenger";
	$MODES["mode_1"] = "trucks";
	$MODES["mode_2"] = "winter";

	// try to find mode_* properties from query
	// continue looping until we hit an undefined
	while (!$lastMode)
	{
		// check if mode exists
		if ( isset($MODES["mode_" . $modeIndex]) )
		{
			$modesFound = true;
			$mode = $MODES["mode_" . $modeIndex];
			$querySegment .= "'" . $mode . "',";
			trace("$modeIndex) mode: $mode");
			$modeIndex++;
		}
		else // end search
		{
			// remove last comma and add closing parenteses
			$queryEndSegment = substr($querySegment, 0, -1);
			$queryEndSegment .= ")";
			trace("querySegment: " . $queryEndSegment);

			// add to sql statement
			$sqlStmt .= $queryEndSegment;
			trace("sqlStmt: " . $sqlStmt);

			// stop loop
			$lastMode = true;
		}
	}

	// check if we have modes
	if ($modesFound)
	{
		trace("Removing all current tracks");
		// remove all current tracks
		$removeCompletedTrackSet = $sb->removeAllCurrentTrackSets($userID, $sqlStmt);
		if ($removeCompletedTrackSet["success"])
		{
			trace("*Current Tracks removed*");

			// reset profile
			resetSwitchBackProfile($userID);
		}
		else
		{
			onErrorMessage("Error: " . $removeCompletedTrackSet["ERROR"]);
		}
	}
	else // no mode props in query
	{
		onErrorMessage("No modes found.");
	}
}
else // email missing
{
	onErrorMessage("Invalid entry.");
}

// we are restarting the game, clear gamemode and challengeIndex
function resetSwitchBackProfile($userid)
{
	trace("resetSwitchBackProfile.UserID: " . $userid);

	$sb = $GLOBALS["sb"];

	// user added, update
	$clearUserProfileData = $sb->clearUserProfileData($userid);

	if (!$clearUserProfileData["success"])
	{
		onErrorMessage("Could not update profile. Error: " . $clearUserProfileData["ERROR"]);
	}
	else
	{
		onSuccessMessage("Tracks removed and Profile reset", $clearUserProfileData);
	}
}

?>
