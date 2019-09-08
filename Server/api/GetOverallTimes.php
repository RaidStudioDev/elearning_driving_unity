<?php
include_once 'config.php';
include_once 'class.switchback.php';

/*
	query = "?e="; 			// Email;

	https://CLIENTURL/api/GetOverallTimes.php

	* Retrieves top 10 list
	* Also includes user is not in the top 10
*/


if(isset($_GET['e']))
{
	$sb = new SWITCHBACK($DB_con);

	$email = $_GET['e'];

	// get user info
	$userInfo = $users->fetchUserInfoByEmail($email);

	// check if user exists, if not don't continue
	if ($userInfo["id"] == null) onErrorMessage("Invalid user.");

	// get top 10 list
	$top10List = $sb->getOverallList();

	if ($top10List["success"])
	{
		$response["leaderboard_data"] = $top10List["list"];

		// we will add the user at the end of the list
		// if time is not in top 10
		$getRank = $sb->getRank($email);

		trace(json_encode($getRank));

		if ($getRank)
		{
			if (count($getRank["userdata"]) == 1 && $getRank["userdata"][0]["rank"] > 10)
			{
				$response["leaderboard_data"][] = $getRank["userdata"][0];
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
