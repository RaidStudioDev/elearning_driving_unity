<?php
include_once 'config.php';
include_once 'class.switchback.php';

/*
	query = "&g="; 	// GameMode / Circuit Name;

	https://CLIENTURL/api/GetRecordTimeByGameMode.php

	* Retrieves record time by game mode
*/

if (isset($_GET['g']))
{
	$sb = new SWITCHBACK($DB_con);

	$gamemode = $_GET['g'];

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
			$returnData["currentCircuitRecordTime"] = $timeList[0]["total_time"];
		}
		else
			$returnData["currentCircuitRecordTime"] = 0;

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
