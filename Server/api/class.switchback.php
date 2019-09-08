<?php

class SWITCHBACK
{
	private $db;

	function __construct($DB_con)
	{
		$this->db = $DB_con;
	}

	public function createSwitchBackUser($userid)
	{
		date_default_timezone_set('US/Eastern');

		$created = date("Y-m-d H:i:s");

		$gamemode = "";			// default
		$track_uid = "";		// default
		$challengeindex = 0;	// default

		try
		{
			$stmt = $this->db->prepare("INSERT INTO switchback(userid, gamemode, challengeindex, created)
							VALUES(:userid, :gamemode, :challengeindex, :created)");

			$stmt->bindparam(":userid", $userid, PDO::PARAM_INT);
			$stmt->bindparam(":gamemode", $gamemode, PDO::PARAM_STR);
			$stmt->bindparam(":challengeindex", $challengeindex, PDO::PARAM_INT);
			$stmt->bindparam(":created", $created, PDO::PARAM_STR);

			if ($stmt->execute())
			{
				$response["gamemode"] =  $gamemode;
				$response["challengeindex"] =  0;
				$response["lastInsertId"] =  $this->db->lastInsertId(); // Get the newly created ID;
				$response["success"] = true;
			}
			else
			{
				$response["success"] = false;
				$response["ERROR"] = $stmt->errorInfo();
			}

			return $response;
		}
		catch(PDOException $e)
		{
			$response["success"] = false;
			$response["ERROR"] = $e->getMessage();
			return $response;
		}
	}

	public function getSwitchBackInfoByEmail($email)
	{
		try
		{
			$stmt = $this->db->prepare("SELECT
					users.id AS userid,
					users.email,
					users.region,
					users.org,
					switchback.gamemode,
					switchback.challengeindex
				FROM switchback
				INNER JOIN users
				ON switchback.userid = users.id
				WHERE users.email = :email");

			$stmt->bindValue(':email', $email, PDO::PARAM_STR);

			if ($stmt->execute())
			{
				$count = $stmt->rowCount();
				// print("$count rows.\n");
				if ($count > 0)
				{
					// $response = $stmt->fetchAll(PDO::FETCH_ASSOC)[0];
					$response["success"] = true;
					$response["data"] = $stmt->fetchAll(PDO::FETCH_ASSOC)[0];
				}
				else
				{
					$response["success"] = false;
					$response["data"] = $stmt->fetchAll(PDO::FETCH_ASSOC)[0];
				}
			}
			else
			{
				$response["success"] = false;
				$response["ERROR"] = $stmt->errorInfo();
			}
			return $response;
		}
		catch(PDOException $e)
		{
			$response["success"] = false;
			$response["ERROR"] = $e->getMessage();
			return $response;
		}
	}

	public function fetchIdByUserId($userid)
	{
		try
		{
			$stmt = $this->db->prepare("SELECT id
				FROM switchback
				WHERE userid = :userid");

			$stmt->bindValue(':userid', $userid, PDO::PARAM_INT);

			if ($stmt->execute())
			{
				$userData = $stmt->fetchAll(PDO::FETCH_ASSOC);
				if (count($userData) >= 1) return $userData[0]["id"];
				else return false;

			} else return false;
		}
		catch(PDOException $e)
		{
			return false;
		}
	}

	public function getTimeByTrackType($userid, $track_type)
	{
		$response["success"] = false;

		try
		{
			$stmt = $this->db->prepare("SELECT *
				FROM switchback_tracks
				WHERE switchback_tracks.userid = :userid AND switchback_tracks.track_type = :track_type");

			$stmt->bindValue(':userid', $userid, PDO::PARAM_INT);
			$stmt->bindValue(':track_type', $track_type, PDO::PARAM_STR);

			if ($stmt->execute())
			{
				$trackData = $stmt->fetchAll(PDO::FETCH_ASSOC);

				if (count($trackData) >= 1)
				{
					$response["success"] = true;
					$response["track_time"] = $trackData[0]["track_time"];
				}
			}

			return $response;
		}
		catch(PDOException $e)
		{
			$response["error"] = $e->getMessage();
			return $response;
		}

	}

	public function getChallengeIndex($userid)
	{
		$response["success"] = false;

		try
		{
			$stmt = $this->db->prepare("SELECT *
				FROM switchback
				WHERE switchback.userid = :userid");

			$stmt->bindValue(':userid', $userid, PDO::PARAM_INT);

			if ($stmt->execute())
			{
				$trackData = $stmt->fetchAll(PDO::FETCH_ASSOC);

				if (count($trackData) >= 1)
				{
					$response["success"] = true;
					$response["gamemode"] = $trackData[0]["gamemode"];
					$response["challengeindex"] = $trackData[0]["challengeindex"];
				}
			}

			return $response;
		}
		catch(PDOException $e)
		{
			$response["status"] = $e->getMessage();
			return $response;
		}
	}

	public function addUserChallengeIndex($userid, $challengeindex)
	{
		date_default_timezone_set('US/Eastern');

		$response["success"] = false;
		$date = date("Y-m-d H:i:s");

		try
		{
			$stmt = $this->db->prepare("INSERT INTO switchback(userid,challengeindex,date)
							VALUES(:userid, :challengeindex, :date)");

			$stmt->bindparam(":userid", $userid, PDO::PARAM_INT);
			$stmt->bindparam(":challengeindex", $challengeindex, PDO::PARAM_INT);
			$stmt->bindparam(":date", $date, PDO::PARAM_STR);

			if ($stmt->execute())
			{
				$response["success"] = true;
			}

			return $response;
		}
		catch(PDOException $e)
		{
			$response["success"] = false;
			$response["status"] = $e->getMessage();
			return $response;
		}
	}

	public function updateUserChallengeIndex($userid, $gamemode, $challengeindex)
	{
		try
		{
			$stmt = $this->db->prepare("UPDATE switchback SET
					switchback.gamemode = :gamemode,
					switchback.challengeindex = :challengeindex
				WHERE switchback.userid = :userid");

			$stmt->bindValue(':gamemode', $gamemode, PDO::PARAM_STR);
			$stmt->bindValue(':challengeindex', $challengeindex, PDO::PARAM_INT);
			$stmt->bindValue(':userid', $userid, PDO::PARAM_INT);

			if ($stmt->execute())
			{
				$response["success"] = true;
			}
			else
			{
				$response["success"] = false;
				$response["ERROR"] = $stmt->errorInfo();
			}

			return $response;
		}
		catch(PDOException $e)
		{
			$response["success"] = false;
			$response["ERROR"] = $e->getMessage();
			return $response;
		}
	}

	public function clearUserProfileData($userid)
	{
		$gamemode = "";
		$challengeindex = 0;

		try
		{
			$stmt = $this->db->prepare("UPDATE switchback SET
					switchback.gamemode = :gamemode,
					switchback.challengeindex = :challengeindex
				WHERE switchback.userid = :userid");

			$stmt->bindValue(':gamemode', $gamemode, PDO::PARAM_STR);
			$stmt->bindValue(':challengeindex', $challengeindex, PDO::PARAM_INT);
			$stmt->bindValue(':userid', $userid, PDO::PARAM_INT);

			if ($stmt->execute())
			{
				$response["success"] = true;
			}
			else
			{
				$response["success"] = false;
				$response["ERROR"] = $stmt->errorInfo();
			}

			return $response;
		}
		catch(PDOException $e)
		{
			$response["success"] = false;
			$response["ERROR"] = $e->getMessage();
			return $response;
		}
	}

	public function checkIfUserIsAdded($userid)
	{
		$response["success"] = false;

		try
		{
			$stmt = $this->db->prepare("SELECT *
				FROM switchback
				WHERE switchback.userid = :userid");

			$stmt->bindValue(':userid', $userid, PDO::PARAM_INT);

			if ($stmt->execute())
			{
				$trackData = $stmt->fetchAll(PDO::FETCH_ASSOC);

				if (count($trackData) >= 1)
				{
					$response["success"] = true;
				}
			}

			return $response;
		}
		catch(PDOException $e)
		{
			$response["success"] = false;
			$response["ERROR"] = $e->getMessage();
			return $response;
		}
	}

	// adds all the tracks in circuit to switchback_tracks
	public function addCompletedTrackSet($userID, $trackData)
	{
		try
		{
			$sqlStmt = "INSERT INTO switchback_tracks(userid,gamemode,track_uid,track_type,track_time,date) VALUES ";
			$querySegments = array();
			$trackLength = count($trackData);
			for($x = 0; $x < $trackLength; $x++)
			{
				$querySegment = "('" . $userID . "', ";
				$querySegment .= "'" . $trackData[$x]["gamemode"] . "', ";
				$querySegment .= "'" . $trackData[$x]["track_uid"] . "', ";
				$querySegment .= "'" . $trackData[$x]["track_type"] . "', ";
				$querySegment .= "'" . $trackData[$x]["track_time"] . "', ";
				$querySegment .= "'" . $trackData[$x]["date"] . "'";
				$querySegment .= ")";

				$querySegments[] = $querySegment;
			}
			$sqlStmt .= implode(',', $querySegments);

			$stmt = $this->db->prepare($sqlStmt);

			if ($stmt->execute())
			{
				$response["success"] = true;
			}
			else
			{
				$response["success"] = false;
				$response["ERROR"] = $stmt->errorInfo();
			}

			return $response;
		}
		catch(PDOException $e)
		{
			$response["success"] = false;
			$response["ERROR"] = $e->getMessage();
			return $response;
		}
	}

	// removes all the completed tracks in circuit by gamemode
	public function removeCompletedTrackSet($userID, $gamemode)
	{
		try
		{
			$stmt = $this->db->prepare("DELETE FROM switchback_tracks
				WHERE userid = :userid AND gamemode = :gamemode");

			$stmt->bindValue(':userid', $userID, PDO::PARAM_INT);
			$stmt->bindValue(':gamemode', $gamemode, PDO::PARAM_STR);

			if ($stmt->execute())
			{
				$response["success"] = true;
			}
			else
			{
				$response["success"] = false;
				$response["ERROR"] = $stmt->errorInfo();
			}

			return $response;
		}
		catch(PDOException $e)
		{
			$response["success"] = false;
			$response["ERROR"] = $e->getMessage();
			return $response;
		}
	}

	// removes all the current tracks in circuit by gamemode
	public function removeCurrentTrackSet($userID, $gamemode)
	{
		try
		{
			$stmt = $this->db->prepare("DELETE FROM switchback_data
				WHERE userid = :userid AND gamemode = :gamemode");

			$stmt->bindValue(':userid', $userID, PDO::PARAM_INT);
			$stmt->bindValue(':gamemode', $gamemode, PDO::PARAM_STR);

			if ($stmt->execute())
			{
				$response["success"] = true;
			}
			else
			{
				$response["success"] = false;
				$response["ERROR"] = $stmt->errorInfo();
			}

			return $response;
		}
		catch(PDOException $e)
		{
			$response["success"] = false;
			$response["ERROR"] = $e->getMessage();
			return $response;
		}
	}

	// removes all the current tracks in circuit by gamemode
	public function removeAllCurrentTrackSets($userID, $sqlStmt)
	{
		try
		{
			$stmt = $this->db->prepare($sqlStmt);

			$stmt->bindValue(':userid', $userID, PDO::PARAM_INT);

			if ($stmt->execute())
			{
				$response["success"] = true;
			}
			else
			{
				$response["success"] = false;
				$response["ERROR"] = $stmt->errorInfo();
			}

			return $response;
		}
		catch(PDOException $e)
		{
			$response["success"] = false;
			$response["ERROR"] = $e->getMessage();
			return $response;
		}
	}

	public function createCurrentTrackRecord($userInfo, $gamemode, $track_uid, $track_type, $track_time)
	{
		date_default_timezone_set('US/Eastern');

		$response["success"] = false;
		$userid = $userInfo["id"];
		$date = date("Y-m-d H:i:s");

		try
		{
			$stmt = $this->db->prepare("INSERT INTO switchback_data(userid,gamemode,track_uid,track_type,track_time,date)
							VALUES(:userid, :gamemode, :track_uid, :track_type, :track_time, :date)");

			$stmt->bindparam(":userid", $userid, PDO::PARAM_INT);
			$stmt->bindparam(":gamemode", $gamemode, PDO::PARAM_STR);
			$stmt->bindparam(":track_uid", $track_uid, PDO::PARAM_STR);
			$stmt->bindparam(":track_type", $track_type, PDO::PARAM_STR);
			$stmt->bindparam(":track_time", $track_time, PDO::PARAM_INT);
			$stmt->bindparam(":date", $date, PDO::PARAM_STR);

			if ($stmt->execute())
			{
				$response["success"] = true;
			}
			else
			{
				$response["success"] = false;
				$response["ERROR"] = $stmt->errorInfo();
			}

			return $response;
		}
		catch(PDOException $e)
		{
			$response["success"] = false;
			$response["ERROR"] = $e->getMessage();
			return $response;
		}
	}

	public function createTrackRecord($userInfo, $gamemode, $track_uid, $track_type, $track_time)
	{
		date_default_timezone_set('US/Eastern');

		$response["success"] = false;
		$userid = $userInfo["id"];
		$date = date("Y-m-d H:i:s");

		try
		{
			$stmt = $this->db->prepare("INSERT INTO switchback_tracks(userid,gamemode,track_uid,track_type,track_time,date)
							VALUES(:userid, :gamemode, :track_uid, :track_type, :track_time, :date)");

			$stmt->bindparam(":userid", $userid, PDO::PARAM_INT);
			$stmt->bindparam(":gamemode", $gamemode, PDO::PARAM_STR);
			$stmt->bindparam(":track_uid", $track_uid, PDO::PARAM_STR);
			$stmt->bindparam(":track_type", $track_type, PDO::PARAM_STR);
			$stmt->bindparam(":track_time", $track_time, PDO::PARAM_INT);
			$stmt->bindparam(":date", $date, PDO::PARAM_STR);

			if ($stmt->execute())
			{
				$response["success"] = true;
			}
			else
			{
				$response["success"] = false;
				$response["ERROR"] = $stmt->errorInfo();
			}

			return $response;
		}
		catch(PDOException $e)
		{
			$response["success"] = false;
			$response["ERROR"] = $e->getMessage();
			return $response;
		}
	}

	public function updateTrackRecord($userInfo, $track_type, $track_time)
	{
		date_default_timezone_set('US/Eastern');

		$response["success"] = false;
		$userid = $userInfo["id"];
		$date = date("Y-m-d H:i:s");

		try
		{
			$stmt = $this->db->prepare("UPDATE switchback_tracks SET
					switchback_tracks.track_time = :track_time
				WHERE switchback_tracks.userid = :userid AND switchback_tracks.track_type = :track_type");

			$stmt->bindValue(':track_time', $track_time, PDO::PARAM_STR);
			$stmt->bindValue(':track_type', $track_type, PDO::PARAM_STR);
			$stmt->bindValue(':userid', $userid, PDO::PARAM_INT);

			if ($stmt->execute())
			{
				$response["success"] = true;
			}

			return $response;
		}
		catch(PDOException $e)
		{
			$response["success"] = false;
			$response["ERROR"] = $e->getMessage();
			return $response;
		}
	}

	public function checkIfTrackTypeExists($userid, $track_type)
	{
		$response["success"] = false;

		try
		{
			$stmt = $this->db->prepare("SELECT *
				FROM switchback_tracks
				WHERE switchback_tracks.userid = :userid AND switchback_tracks.track_type = :track_type");

			$stmt->bindValue(':userid', $userid, PDO::PARAM_INT);
			$stmt->bindValue(':track_type', $track_type, PDO::PARAM_STR);

			if ($stmt->execute())
			{
				$trackData = $stmt->fetchAll(PDO::FETCH_ASSOC);

				if (count($trackData) >= 1)
				{
					$response["success"] = true;
				}
			}

			return $response;
		}
		catch(PDOException $e)
		{
			$response["ERROR"] = $e->getMessage();
			return $response;
		}
	}

	public function getTracksByUserID($userid)
	{
		$response["success"] = false;

		try
		{
			$stmt = $this->db->prepare("SELECT
					switchback_tracks.id,
					switchback_tracks.userid,
					switchback_tracks.gamemode,
					switchback_tracks.track_uid,
					switchback_tracks.track_type,
					switchback_tracks.track_time,
					switchback_tracks.date
				FROM switchback_tracks
				WHERE switchback_tracks.userid = :userid
				ORDER BY switchback_tracks.date ASC");

			$stmt->bindValue(':userid', $userid, PDO::PARAM_INT);

			if ($stmt->execute())
			{
				$trackData = $stmt->fetchAll(PDO::FETCH_ASSOC);

				if (count($trackData) >= 1)
				{
					$response["success"] = true;
					$response["track_data"] = $trackData;
				}
				else
				{
					$response["success"] = false;
					$response["ERROR"] = "User has no track data";
				}
			}

			return $response;
		}
		catch(PDOException $e)
		{
			$response["success"] = false;
			$response["ERROR"] = $e->getMessage();;
			return $response;
		}

	}

	/* 	lets us know where the user is at
		gives us the current played tracks
	*/
	public function getCurrentTracksPlayed($userid, $gamemode)
	{
		$response["success"] = false;

		try
		{
			$stmt = $this->db->prepare("SELECT
					switchback_data.id,
					switchback_data.userid,
					switchback_data.gamemode,
					switchback_data.track_uid,
					switchback_data.track_type,
					switchback_data.track_time,
					switchback_data.date
				FROM switchback_data
				WHERE switchback_data.userid = :userid AND switchback_data.gamemode = :gamemode
				ORDER BY switchback_data.date ASC");

			$stmt->bindValue(':userid', $userid, PDO::PARAM_INT);
			$stmt->bindValue(':gamemode', $gamemode, PDO::PARAM_STR);

			if ($stmt->execute())
			{
				$trackData = $stmt->fetchAll(PDO::FETCH_ASSOC);

				if (count($trackData) >= 1)
				{
					$response["success"] = true;
					$response["track_data"] = $trackData;
				}
				else
				{
					$response["success"] = false;
					$response["ERROR"] = "User has no track data";
				}
			}

			return $response;
		}
		catch(PDOException $e)
		{
			$response["success"] = false;
			$response["ERROR"] = $e->getMessage();;
			return $response;
		}

	}

	public function getTracksCompletedByGameMode($userid, $gamemode)
	{
		$response["success"] = false;

		try
		{
			$stmt = $this->db->prepare("SELECT
					switchback_tracks.id,
					switchback_tracks.userid,
					switchback_tracks.gamemode,
					switchback_tracks.track_uid,
					switchback_tracks.track_type,
					switchback_tracks.track_time,
					switchback_tracks.date
				FROM switchback_tracks
				WHERE switchback_tracks.userid = :userid AND switchback_tracks.gamemode = :gamemode
				ORDER BY switchback_tracks.date ASC");

			$stmt->bindValue(':userid', $userid, PDO::PARAM_INT);
			$stmt->bindValue(':gamemode', $gamemode, PDO::PARAM_STR);

			if ($stmt->execute())
			{
				$trackData = $stmt->fetchAll(PDO::FETCH_ASSOC);

				if (count($trackData) >= 1)
				{
					$response["success"] = true;
					$response["track_data"] = $trackData;
				}
				else
				{
					$response["success"] = false;
					$response["ERROR"] = "User has no track data";
				}
			}

			return $response;
		}
		catch(PDOException $e)
		{
			$response["success"] = false;
			$response["ERROR"] = $e->getMessage();;
			return $response;
		}

	}

	public function getCurrentTracksCompletedByGameMode($userid, $gamemode)
	{
		$response["success"] = false;

		try
		{
			$stmt = $this->db->prepare("SELECT
					switchback_data.id,
					switchback_data.userid,
					switchback_data.gamemode,
					switchback_data.track_uid,
					switchback_data.track_type,
					switchback_data.track_time,
					switchback_data.date
				FROM switchback_data
				WHERE switchback_data.userid = :userid AND switchback_data.gamemode = :gamemode
				ORDER BY switchback_data.date ASC");

			$stmt->bindValue(':userid', $userid, PDO::PARAM_INT);
			$stmt->bindValue(':gamemode', $gamemode, PDO::PARAM_STR);

			if ($stmt->execute())
			{
				$trackData = $stmt->fetchAll(PDO::FETCH_ASSOC);

				if (count($trackData) >= 1)
				{
					$response["success"] = true;
					$response["track_data"] = $trackData;
				}
				else
				{
					$response["success"] = false;
					$response["ERROR"] = "User has no track data";
				}
			}

			return $response;
		}
		catch(PDOException $e)
		{
			$response["success"] = false;
			$response["ERROR"] = $e->getMessage();;
			return $response;
		}

	}

	public function getCurrentTimeSumFromUserTracks($userid, $gamemode)
	{
		$response["success"] = false;

		try
		{
			$stmt = $this->db->prepare("SELECT SUM(track_time) AS track_time_sum
				FROM switchback_data
				WHERE switchback_data.userid = :userid AND switchback_data.gamemode = :gamemode");

			$stmt->bindValue(':userid', $userid, PDO::PARAM_INT);
			$stmt->bindValue(':gamemode', $gamemode, PDO::PARAM_STR);

			if ($stmt->execute())
			{
				$trackData = $stmt->fetch(PDO::FETCH_ASSOC);

				$response["success"] = true;
				$response["track_time_sum"] = $trackData['track_time_sum'];
			}
			else
			{
				$response["success"] = false;
				$response["ERROR"] = "No tracks found";
			}

			return $response;
		}
		catch(PDOException $e)
		{
			$response["success"] = false;
			$response["ERROR"] = $e->getMessage();;
			return $response;
		}
	}

	public function getBestRecordTimeSumMyGameMode($gamemode)
	{
		$response["success"] = false;

		try
		{
			$stmt = $this->db->prepare("SELECT SUM(track_time) AS track_time_sum
				FROM switchback_tracks
				WHERE switchback_tracks.userid = :userid AND switchback_tracks.gamemode = :gamemode");

			$stmt->bindValue(':userid', $userid, PDO::PARAM_INT);
			$stmt->bindValue(':gamemode', $gamemode, PDO::PARAM_STR);

			if ($stmt->execute())
			{
				$trackData = $stmt->fetch(PDO::FETCH_ASSOC);

				$response["success"] = true;
				$response["track_time_sum"] = $trackData['track_time_sum'];
			}
			else
			{
				$response["success"] = false;
				$response["ERROR"] = "No tracks found";
			}

			return $response;
		}
		catch(PDOException $e)
		{
			$response["success"] = false;
			$response["ERROR"] = $e->getMessage();;
			return $response;
		}
	}

	public function getTimeSumFromUserTracks($userid, $gamemode)
	{
		$response["success"] = false;

		try
		{
			$stmt = $this->db->prepare("SELECT SUM(track_time) AS track_time_sum
				FROM switchback_tracks
				WHERE switchback_tracks.userid = :userid AND switchback_tracks.gamemode = :gamemode");

			$stmt->bindValue(':userid', $userid, PDO::PARAM_INT);
			$stmt->bindValue(':gamemode', $gamemode, PDO::PARAM_STR);

			if ($stmt->execute())
			{
				$trackData = $stmt->fetch(PDO::FETCH_ASSOC);

				$response["success"] = true;
				$response["track_time_sum"] = $trackData['track_time_sum'];
			}
			else
			{
				$response["success"] = false;
				$response["ERROR"] = "No tracks found";
			}

			return $response;
		}
		catch(PDOException $e)
		{
			$response["success"] = false;
			$response["ERROR"] = $e->getMessage();;
			return $response;
		}
	}

	public function getUserOverallTime($userid)
	{
		$response["success"] = false;

		// get total track count gameModeCount * maxTracks
		$amountOfTracks = count($GLOBALS["CIRCUIT_MODES"]) * $GLOBALS["MAX_TRACKS"];

		// create query string from game mode list
		$gameModesStmt = implode("','", $GLOBALS["CIRCUIT_MODES"]);

		try
		{
			$stmt = $this->db->prepare("SELECT
				SUM(track_time) AS total_time
				FROM switchback_data
				WHERE switchback_data.userid = :userid AND switchback_data.gamemode IN ('$gameModesStmt')
				HAVING COUNT(*) = $amountOfTracks");

			$stmt->bindValue(':userid', $userid, PDO::PARAM_INT);

			if ($stmt->execute())
			{
				$list = $stmt->fetchAll(PDO::FETCH_ASSOC);

				$response["success"] = true;

				if (count($list) > 0) $response["total_time"] = $list[0]["total_time"];
				else $response["total_time"] = 0;
			}

			return $response;
		}
		catch(PDOException $e)
		{
			$response["success"] = false;
			$response["ERROR"] = $e->getMessage();
			return $response;
		}
	}

	public function getBestRecordTimeByTrackID($trackid)
	{
		$response["success"] = false;

		try
		{
			$stmt = $this->db->prepare("SELECT
					userid,
					users.fullname,
					track_uid,
					track_type,
					gamemode,
					track_time
				FROM switchback_tracks
				INNER JOIN users
				ON switchback_tracks.userid = users.id AND switchback_tracks.track_uid = :trackid
				GROUP BY userid
				ORDER BY track_time ASC LIMIT 10");

			$stmt->bindValue(':trackid', $trackid, PDO::PARAM_STR);

			if ($stmt->execute())
			{
				$trackData = $stmt->fetch(PDO::FETCH_ASSOC);

				if (count($trackData) > 0)
				{
					$response["record_time"] = $trackData['track_time'];
				}
				else
				{
					$response["record_time"] = 0;
				}

				$response["trackData"] = $trackData;
				$response["success"] = true;

			}
			else
			{
				$response["success"] = false;
				$response["ERROR"] = "No tracks found";
			}

			return $response;
		}
		catch(PDOException $e)
		{
			$response["success"] = false;
			$response["ERROR"] = $e->getMessage();;
			return $response;
		}
	}

	// LEADERBOARDS

	/*
		* get the top 10 times
		* only the users that have completed all game modes
	*/
	public function getOverallList()
	{
		$response["success"] = false;

		// get total track count gameModeCount * maxTracks
		$amountOfTracks = count($GLOBALS["CIRCUIT_MODES"]) * $GLOBALS["MAX_TRACKS"];

		// create query string from game mode list
		$gameModesStmt = implode("','", $GLOBALS["CIRCUIT_MODES"]);

		try
		{
			$stmt = $this->db->prepare("SELECT
				userid,
				users.fullname,
				users.email,
				users.region,
				users.org,
				SUM(track_time) AS total_time
				FROM switchback_tracks
				INNER JOIN users
				ON switchback_tracks.userid = users.id AND switchback_tracks.gamemode IN ('$gameModesStmt')
				GROUP BY userid
				HAVING COUNT(*) = $amountOfTracks
				ORDER BY total_time ASC LIMIT 10");

			if ($stmt->execute())
			{
				$list = $stmt->fetchAll(PDO::FETCH_ASSOC);

				$response["success"] = true;
				$response["list"] = $list;
			}

			return $response;
		}
		catch(PDOException $e)
		{
			$response["success"] = false;
			$response["ERROR"] = $e->getMessage();
			return $response;
		}
	}

	public function getOverallListByGameMode($gamemode)
	{
		$response["success"] = false;

		// get total track count gameModeCount * maxTracks
		$amountOfTracks = $GLOBALS["MAX_TRACKS"];

		try
		{
			$stmt = $this->db->prepare("SELECT
				userid,
				users.fullname,
				users.email,
				users.region,
				users.org,
				gamemode,
				SUM(track_time) AS total_time
				FROM switchback_tracks
				INNER JOIN users
				ON switchback_tracks.userid = users.id AND switchback_tracks.gamemode IN ('$gamemode')
				GROUP BY userid
				HAVING COUNT(*) = $amountOfTracks
				ORDER BY total_time ASC LIMIT 10");

			if ($stmt->execute())
			{
				$list = $stmt->fetchAll(PDO::FETCH_ASSOC);

				$response["success"] = true;
				$response["list"] = $list;
			}

			return $response;
		}
		catch(PDOException $e)
		{
			$response["success"] = false;
			$response["ERROR"] = $e->getMessage();
			return $response;
		}
	}

	public function getOverallListByGameModeDesc($gamemode)
	{
		$response["success"] = false;

		// get total track count gameModeCount * maxTracks
		$amountOfTracks = $GLOBALS["MAX_TRACKS"];

		try
		{
			$stmt = $this->db->prepare("SELECT
				userid,
				users.fullname,
				users.email,
				users.region,
				users.org,
				SUM(track_time) AS total_time
				FROM switchback_tracks
				INNER JOIN users
				ON switchback_tracks.userid = users.id AND switchback_tracks.gamemode IN ('$gamemode')
				GROUP BY userid
				HAVING COUNT(*) = $amountOfTracks
				ORDER BY total_time DESC LIMIT 10");

			if ($stmt->execute())
			{
				$list = $stmt->fetchAll(PDO::FETCH_ASSOC);

				$response["success"] = true;
				$response["list"] = $list;
			}

			return $response;
		}
		catch(PDOException $e)
		{
			$response["success"] = false;
			$response["ERROR"] = $e->getMessage();
			return $response;
		}
	}

	public function getRank($email)
	{
		$response["success"] = false;

		// get total track count gameModeCount * maxTracks
		$amountOfTracks = count($GLOBALS["CIRCUIT_MODES"]) * $GLOBALS["MAX_TRACKS"];

		// create query string from game mode list
		$gameModesStmt = implode("','", $GLOBALS["CIRCUIT_MODES"]);

		try
		{
			$stmt = $this->db->prepare("SELECT
				switchback_tracks_outer.*,
				users.fullname,
				users.email
				FROM (
					SELECT switchback_tracks_inner.*, @rank := @rank + 1 rank FROM
					(
						SELECT userid, gamemode, sum(track_time) total_time FROM switchback_tracks
						GROUP BY userid
						HAVING COUNT(*) = $amountOfTracks
					) switchback_tracks_inner, (SELECT @rank := 0) init
					ORDER BY total_time ASC
				) switchback_tracks_outer
				INNER JOIN users
				ON userid = users.id AND gamemode IN ('$gameModesStmt')
				WHERE email = :email
				");

			$stmt->bindValue(':email', $email, PDO::PARAM_STR);

			if ($stmt->execute())
			{
				$data = $stmt->fetchAll(PDO::FETCH_ASSOC);
				$response["success"] = true;
				$response["userdata"] = $data;
			}
			else
			{
				$response["success"] = false;
				$response["ERROR"] = $stmt->getMessage();
			}

			return $response;
		}
		catch(PDOException $e)
		{
			$response["success"] = false;
			$response["ERROR"] = $e->getMessage();
			return $response;
		}
	}


	public function getRankByGameMode($email, $gamemode)
	{
		$response["success"] = false;

		// get total track count gameModeCount * maxTracks
		$amountOfTracks = $GLOBALS["MAX_TRACKS"];

		try
		{
			$stmt = $this->db->prepare("SELECT
				switchback_tracks_outer.*,
				users.fullname,
				users.email
				FROM (
					SELECT switchback_tracks_inner.*, @rank := @rank + 1 rank FROM
					(
						SELECT userid, gamemode, sum(track_time) total_time FROM switchback_tracks
						WHERE gamemode IN ('$gamemode')
						GROUP BY userid
						HAVING COUNT(*) = $amountOfTracks
					) switchback_tracks_inner, (SELECT @rank := 0) init
					ORDER BY total_time ASC
				) switchback_tracks_outer
				INNER JOIN users
				ON userid = users.id
				WHERE email = :email
				");

			$stmt->bindValue(':email', $email, PDO::PARAM_STR);

			if ($stmt->execute())
			{
				$data = $stmt->fetchAll(PDO::FETCH_ASSOC);
				$response["success"] = true;
				$response["userdata"] = $data;
			}
			else
			{
				$response["success"] = false;
				$response["ERROR"] = $stmt->getMessage();
			}

			return $response;
		}
		catch(PDOException $e)
		{
			$response["success"] = false;
			$response["ERROR"] = $e->getMessage();
			return $response;
		}
	}

	public function getAllRanks()
	{
		$response["success"] = false;

		// get total track count gameModeCount * maxTracks
		$amountOfTracks = count($GLOBALS["CIRCUIT_MODES"]) * $GLOBALS["MAX_TRACKS"];

		// create query string from game mode list
		$gameModesStmt = implode("','", $GLOBALS["CIRCUIT_MODES"]);

		try
		{
			$stmt = $this->db->prepare("SELECT
				switchback_tracks_outer.*,
				users.fullname,
				users.email
				FROM (
					SELECT switchback_tracks_inner.*, @rank := @rank + 1 rank FROM
					(
						SELECT userid, gamemode, sum(track_time) total_time FROM switchback_tracks
						WHERE gamemode IN ('$gameModesStmt')
						GROUP BY userid
						HAVING COUNT(*) = $amountOfTracks
					) switchback_tracks_inner, (SELECT @rank := 0) init
					ORDER BY total_time ASC
				) switchback_tracks_outer
				INNER JOIN users
				ON userid = users.id
				");

			if ($stmt->execute())
			{
				$data = $stmt->fetchAll(PDO::FETCH_ASSOC);
				$response["success"] = true;
				$response["userdata"] = $data;
			}
			else
			{
				$response["success"] = false;
				$response["ERROR"] = $stmt->getMessage();
			}

			return $response;
		}
		catch(PDOException $e)
		{
			$response["success"] = false;
			$response["ERROR"] = $e->getMessage();
			return $response;
		}
	}

	public function getAllRanksByGameMode($gamemode)
	{
		$response["success"] = false;

		// get total track count gameModeCount * maxTracks
		$amountOfTracks = $GLOBALS["MAX_TRACKS"];

		try
		{
			$stmt = $this->db->prepare("SELECT
				switchback_tracks_outer.*,
				users.fullname,
				users.email
				FROM (
					SELECT switchback_tracks_inner.*, @rank := @rank + 1 rank FROM
					(
						SELECT userid, gamemode, sum(track_time) total_time FROM switchback_tracks
						WHERE gamemode IN ('$gamemode')
						GROUP BY userid
						HAVING COUNT(*) = $amountOfTracks
					) switchback_tracks_inner, (SELECT @rank := 0) init
					ORDER BY total_time ASC
				) switchback_tracks_outer
				INNER JOIN users
				ON userid = users.id
				");

			if ($stmt->execute())
			{
				$data = $stmt->fetchAll(PDO::FETCH_ASSOC);
				$response["success"] = true;
				$response["userdata"] = $data;
			}
			else
			{
				$response["success"] = false;
				$response["ERROR"] = $stmt->getMessage();
			}

			return $response;
		}
		catch(PDOException $e)
		{
			$response["success"] = false;
			$response["ERROR"] = $e->getMessage();
			return $response;
		}
	}
}
?>
