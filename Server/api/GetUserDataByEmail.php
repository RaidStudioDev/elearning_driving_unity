<?php
include_once 'config.php';
include_once 'class.switchback.php';

/*
	query = "?f="; 			// Full Name;
	query += "&e="; 		// Email;
	query += "&r="; 		// Region;
	query += "&o="; 		// Organization;

	https://CLIENTURL/api/GetUserDataByEmail.php

	* Retrieves the challenge index by user id.
	* Returns current gamemode & challengeindex
	* Returns trackTimeTotal & isTracksCompleted
*/

trace("max tracks: " . $MAX_TRACKS);

if (isset($_GET['f']) && isset($_GET['e']) && isset($_GET['r']) && isset($_GET['o']) )
{
	$sb = new SWITCHBACK($DB_con);

	$userID = null;
	$switchbackID = null;

	$fullname = $_GET['f'];
	$email = $_GET['e'];
	$region = $_GET['r'];
	$org = $_GET['o'];

	// first check if user exists
	$fetchUserIdByEmail = $users->fetchUserIdByEmail($email);
	if (!$fetchUserIdByEmail)
	{
		trace("*USER DOES NOT EXIST*");
		trace("Creating User");

		// user does not exist, create user
		$createUser = createUser($users, $fullname, $email, $region, $org);
		if ($createUser["success"])
		{
			trace("User Created ID:" . $createUser["userid"]);

			// create switchback profile
			$createSwitchBackUserData = createSwitchBackUserData($sb, $createUser["userid"]);

			if ($createSwitchBackUserData["success"])
			{
				trace("Switchback Profile Created ID:" . $createSwitchBackUserData["switchbackid"]);

				getSwitchbackProfileData($createSwitchBackUserData["switchbackid"]);
			}
			else
			{
				echo json_encode($createSwitchBackUserData);
				exit;
			}
		}
		else
		{
			echo json_encode($fetchUserIdByEmail);
			exit;
		}
	}
	else // user found
	{
		// set user id
		$userID = $fetchUserIdByEmail;
		trace("UserID:" . $userID);
		trace("Email: " . $email);

		// get switchback profile id
		$switchbackID = $sb->fetchIdByUserId($userID);
		trace("SwitchBack Profile ID: " . $switchbackID);
		if ($switchbackID)
		{
			getSwitchbackProfileData($switchbackID);
		}
		else
		{
			trace("*SWITCHBACK USER DOES NOT EXIST*");
			trace("Creating SwitchbackUser");

			// create switchback profile
			$createSwitchBackUserData = createSwitchBackUserData($sb, $userID);

			if ($createSwitchBackUserData["success"])
			{
				trace("Switchback Profile Created ID:" . $createSwitchBackUserData["switchbackid"]);

				getSwitchbackProfileData($createSwitchBackUserData["switchbackid"]);
			}
			else
			{
				echo json_encode($createSwitchBackUserData);
				exit;
			}

		}
	}

}
else
{
	onErrorMessage("Invalid entry.");
}

function getSwitchbackProfileData($switchbackID)
{
	trace("**MAX_TRACKS: " . $GLOBALS["MAX_TRACKS"]);

	$sb = $GLOBALS["sb"];
	$email = $GLOBALS["email"];

	trace("getSwitchbackProfileData(".$switchbackID.")");

	$getSwitchBackInfoByEmail = $sb->getSwitchBackInfoByEmail($email);

	if ($getSwitchBackInfoByEmail["success"])
	{
		// trace(json_encode($getSwitchBackInfoByEmail));
		trace("Current gamemode: " . $getSwitchBackInfoByEmail["data"]["gamemode"]);

		$userid = $getSwitchBackInfoByEmail["data"]["userid"];
		$gamemode = $getSwitchBackInfoByEmail["data"]["gamemode"];

		// current gamemode and index
		$returnData["gamemode"] = $getSwitchBackInfoByEmail["data"]["gamemode"];
		$returnData["challengeIndex"] = $getSwitchBackInfoByEmail["data"]["challengeindex"];


		// check if user completed circuit ( all tracks )
		$getTrackData = $sb->getCurrentTracksCompletedByGameMode($userid, $gamemode);

		$returnData["previouslyCompleted"] = false;
		if ($getTrackData["success"])
		{
			trace("\n");
			trace("*LeaderboardTrackData* total:" . count($getTrackData["track_data"]));
			trace(json_encode($getTrackData));
			trace("*end of LeaderboardTrackData*");

			if (count($getTrackData["track_data"]) == $GLOBALS["MAX_TRACKS"])
			{
				$returnData["previouslyCompleted"] = true;
			}
		}

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

		// get current track/mode
		$getCurrentTrackData = $sb->getCurrentTracksPlayed($userid, $gamemode);

		if ($getCurrentTrackData["success"])
		{
			trace("\n");
			trace("*CurrentTrackData* total:" . count($getCurrentTrackData["track_data"]));
			trace(json_encode($getCurrentTrackData));
			trace("*end of CurrentTrackData*");
			trace("\n");

			$returnData["completion"] = $completion;

			// all current track data
			$returnData["track_data"] = $getCurrentTrackData["track_data"];

			// total num of tracks completed
			$returnData["totalTracksCompleted"] = count($getCurrentTrackData["track_data"]);

			// check for total completed tracks in circuit
			$returnData["isTracksCompleted"] = (count($getCurrentTrackData["track_data"]) == $GLOBALS['MAX_TRACKS']);

			$getUserOverallTime = $sb->getUserOverallTime($userid);
			if ($getUserOverallTime["success"])
			{
				$returnData["currentCircuitTime"] = $sb->getUserOverallTime($userid)["total_time"];
			}
			else $returnData["currentCircuitTime"] = 0;


			// get total time on tracks played
			$userid = $getSwitchBackInfoByEmail["data"]["userid"];
			$gamemode = $getSwitchBackInfoByEmail["data"]["gamemode"];
			$getTimeSumFromUserTracks = $sb->getCurrentTimeSumFromUserTracks($userid, $gamemode);

			if ($getTimeSumFromUserTracks["success"])
			{
				$returnData["trackTimeTotal"] = $getTimeSumFromUserTracks["track_time_sum"];

				onSuccessMessage("Switchback Profile successful", $returnData);
			}
			else
			{
				onErrorMessage($getTimeSumFromUserTracks["ERROR"]);
			}
		}
		else	// user has no track data
		{
			// all current track data
			$returnData["track_data"] = array();

			// total num of tracks completed
			$returnData["totalTracksCompleted"] = 0;

			// check for total completed tracks in circuit
			$returnData["isTracksCompleted"] = false;

			// get total time on tracks played
			$returnData["trackTimeTotal"] = 0;

			onSuccessMessage($getCurrentTrackData["ERROR"], $returnData);

			// onErrorMessage($getCurrentTrackData["ERROR"]);
		}
	}
	else
	{
		onErrorMessage($getSwitchBackInfoByEmail["ERROR"]);
	}
}



// creates user from users table
function createUser($users, $fullname, $email, $region, $org)
{
	// If user does not exist, we will register it to the user table
	$result = $users->register($fullname, $email, $region, $org);

	if ($result) // Registration successful
	{
		$response["userid"] = $result["lastInsertId"];
		$response["success"] = true;
	}
	else
	{
		$response["success"] = false;
		$response["ERROR"] = "User could not be created.";
	}

	return $response;
}

// creates index from userid and challenge index table
function createSwitchBackUserData($sb, $userid)
{
	$result = $sb->createSwitchBackUser($userid);

	if ($result["success"]) // successful
	{
		$response["switchbackid"] = $result["lastInsertId"];
		$response["success"] = true;
	}
	else
	{
		$response["success"] = false;
		$response["DB_ERROR"] = $result["ERROR"];
		$response["ERROR"] = "SwitchBack user could not be created.";
	}

	return $response;
}

?>
