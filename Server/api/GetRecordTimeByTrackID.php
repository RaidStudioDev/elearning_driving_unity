<?php
include_once 'config.php';
include_once 'class.switchback.php';

/*
	query = "&trackid="; 	// TrackID

	https://CLIENTURL/api/GetRecordTimeByTrackID.php

	* Retrieves record time by track ID
*/

if (isset($_GET['trackid']))
{
	$sb = new SWITCHBACK($DB_con);

	$trackid = $_GET['trackid'];

	$getOverallListByGameMode = $sb->getBestRecordTimeByTrackID($trackid);

	if ($getOverallListByGameMode["success"])
	{
		$recordTime = 0;
		$recordTime = $getOverallListByGameMode["record_time"];

		// $returnData["track_data"] = $getOverallListByGameMode["trackData"];

		$trackData = $getOverallListByGameMode["trackData"];

		$returnData["userid"] = $trackData["userid"];
		$returnData["fullname"] = $trackData["fullname"];
		$returnData["gamemode"] = $trackData["gamemode"];
		$returnData["track_data"] = [];
		array_push($returnData["track_data"], (object)[
        'track_uid' => $trackData["track_uid"],
        'track_type' => $trackData["track_type"],
        'track_time' => $trackData["track_time"]
		]);

		//$returnData["track_data"]["track_uid"] = $trackData["track_uid"];
		//$returnData["track_data"]["track_type"] = $trackData["track_type"];
		//$returnData["track_data"]["track_time"] = $trackData["track_time"];

		$returnData["currentTrackRecordTime"] = $recordTime;

		onSuccessMessage("Switchback Record Time", $returnData);
	}
	else
	{
		onErrorMessage($getOverallListByGameMode["ERROR"]);
	}


}
else
{
	onErrorMessage("Invalid entry.");
}
?>
