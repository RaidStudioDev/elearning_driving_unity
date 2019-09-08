<?php
include_once 'config.php';
include_once '../../class.switchback.php';

/*
	query += "?e=" // Email;

	https://CLIENTURL/api/GetTop10List.php

	* Retrieves the challenge index by user id.
	* Returns challengeindex
*/

if(isset($_GET['e']))
{
	$sb = new SWITCHBACK($DB_con);

	$email = $_GET['e'];

	// get user info
	$userInfo = $users->fetchUserInfoByEmail($email);

	// get the challenge index previously saved
	$getUserTracks = $sb->getTracksByUserID($userInfo["id"]);

	if ($getUserTracks["success"])
	{
		echo "<div>UserID: " . $userInfo["id"] . "</div>";
		echo "<div>Name: " . strtoupper($userInfo["fullname"]) . "</div>";
		echo "<div>Email: " . strtolower($userInfo["email"]) . "</div>";

		$listLen = count($getUserTracks["track_data"]);
		for($i = 0; $i < $listLen; $i++)
		{
			echo "<p>";
			echo "<div>Challenge id: " . $getUserTracks["track_data"][$i]["id"] . "</div>";
			echo "<div>Challenge track_type: " . $getUserTracks["track_data"][$i]["track_type"] . "</div>";
			echo "<div>Challenge track_time: " . $getUserTracks["track_data"][$i]["track_time"] . "</div>";
			echo "<div>Challenge date: " . $getUserTracks["track_data"][$i]["date"] . "</div>";
			echo "</p>";
		}

		$getTimeSumFromUserTracks = $sb->getTimeSumFromUserTracks($userInfo["id"]);

		echo "Total Time: " . $getTimeSumFromUserTracks["track_time_sum"];

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
}
?>
