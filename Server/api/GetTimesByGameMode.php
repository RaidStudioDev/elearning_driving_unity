<?php
include_once 'config.php';
include_once 'class.switchback.php';

/*
	query = "?e="; 			// Email;
	query = "&gamemode="; 	// GameMode / Circuit Name;

	https://CLIENTURL/api/GetTimesByGameMode.php

	* Retrieves
*/

if (isset($_GET['e']) && isset($_GET['g']))
{
	$sb = new SWITCHBACK($DB_con);

	$email = $_GET['e'];
	$gamemode = $_GET['g'];

	// get user info
	$userInfo = $users->fetchUserInfoByEmail($email);

	// check if user exists, if not don't continue
	if ($userInfo["id"] == null) onErrorMessage("Invalid user.");

	// get top 10 list
	$top10List = $sb->getOverallListByGameMode($gamemode);

	if ($top10List["success"])
	{
		$response["leaderboard_data"] = $top10List["list"];

		// we will add the user at the end of the list
		// if time is not in top 10
		$getRankByGameMode = $sb->getRankByGameMode($email, $gamemode);

		if ($getRankByGameMode)
		{
			if (count($getRankByGameMode["userdata"]) == 1 && $getRankByGameMode["userdata"][0]["rank"] > 10)
			{
				$response["leaderboard_data"][] = $getRankByGameMode["userdata"][0];
			}
		}

		onSuccessMessage("Successfully loaded", $response);
	}
	else // track type does not exist, create it
	{
		onErrorMessage("Could not find index.");
	}
}
else
{
	onErrorMessage("Invalid entry.");
}
?>
