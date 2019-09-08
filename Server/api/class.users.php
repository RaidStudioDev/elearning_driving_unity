<?php
class USERS
{
	private $db;
	private $tableName;
	
	function __construct($DB_con)
	{
		$this->db = $DB_con;
		$this->tableName = "users";
	}
	
	public function trace($msg)
	{
		echo "Logger: [ " . $msg . "]";
	}
	
	public function register($fullname, $email, $region, $org)
	{
		date_default_timezone_set('US/Eastern');
		
		$created = date("Y-m-d H:i:s");
		
		try
		{
			$stmt = $this->db->prepare("INSERT INTO users(fullname,email,region,org,created) 
							VALUES(:fullname, :mail, :region, :org, :created)");
												  
			$stmt->bindparam(":fullname", $fullname);
			$stmt->bindparam(":mail", $email);
			$stmt->bindparam(":region", $region);		  
			$stmt->bindparam(":org", $org);					  
			$stmt->bindparam(":created", $created);					  
				
			if ($stmt->execute()) 
			{
				// Get the newly created ID
				$response["lastInsertId"] = $this->db->lastInsertId();
			}
			else $response = false;	
			
			return $response;	
		}
		catch(PDOException $e)
		{
			// $response["ERROR"] = $e->getMessage();
			$response = false;
			return $response;
		}				
	}
	
	public function registerWithDate($fullname, $email, $region, $org, $created, $modified)
	{
		try
		{
			$stmt = $this->db->prepare("INSERT INTO users(fullname,email,region,org,modified,created) 
							VALUES(:fullname, :mail, :region, :org, :modified, :created)");
												  
			$stmt->bindparam(":fullname", $fullname);
			$stmt->bindparam(":mail", $email);
			$stmt->bindparam(":region", $region);		  
			$stmt->bindparam(":org", $org);					  
			$stmt->bindparam(":modified", $modified);					  
			$stmt->bindparam(":created", $created);					  
				
			if ($stmt->execute()) 
			{
				// Get the newly created ID
				$response["lastInsertId"] = $this->db->lastInsertId();
			}
			else $response = false;	
			
			return $response;	
		}
		catch(PDOException $e)
		{
			// $response["ERROR"] = $e->getMessage();
			$response = false;
			return $response;
		}				
	}
	
	public function update($uid, $fullname, $email, $region, $org) 
	{
		try
		{
			$stmt = $this->db->prepare("UPDATE users SET 
					users.fullname = :fullname,
					users.email = :email,
					users.region = :region,
					users.org = :org
				WHERE users.id = :uid");
				
			$stmt->bindValue(':fullname', $fullname, PDO::PARAM_STR);
			$stmt->bindValue(':email', $email, PDO::PARAM_STR);
			$stmt->bindValue(':region', $region, PDO::PARAM_STR);
			$stmt->bindValue(':org', $org, PDO::PARAM_STR);
			$stmt->bindValue(':uid', $uid, PDO::PARAM_INT);
			
			if ($stmt->execute()) 
			{
				$response = true;	
			}
			else $response = false;	
			
			return $response;	
		}
		catch(PDOException $e)
		{
			echo "ERROR: " . $e->getMessage();
			return false;
		}	
	}
	
	public function removeUserByID($userID)
	{
		try
		{
			$stmt = $this->db->prepare("DELETE FROM users   
				WHERE id = :userID");
			
			$stmt->bindValue(':userID', $userID, PDO::PARAM_INT);
			
			if ($stmt->execute())
			{
				$response["success"] = true;
			}
			else 
			{
				$response["success"] = false;
				$response["error"] = $stmt->errorInfo();
			}
			
			return $response;
		}
		catch(PDOException $e)
		{
			$response["success"] = false;
			$response["error"] = $e->getMessage();
			return $response;
		}		
	}
	
	public function fetchRandomUser($useremail)
	{
		// SELECT * FROM `table` ORDER BY RAND() LIMIT 0,1;
		try
		{
			$stmt = $this->db->prepare("SELECT *
				FROM users 
				WHERE email != :useremail
				ORDER BY RAND() LIMIT 0,1");
				
			$stmt->bindValue(':useremail', $useremail, PDO::PARAM_STR);
	
			if ($stmt->execute())  
			{
				$userData = $stmt->fetchAll(PDO::FETCH_ASSOC);
				
				if (count($userData) >= 1) return $userData[0];
				else return false;
				
			} else return false;
			
		}
		catch(PDOException $e)
		{
			$response["ERROR"] = $e->getMessage();
			return $response;
		}	
	}
	
	public function fetchAllUsers()
	{
		try
		{
			$stmt = $this->db->prepare("SELECT * FROM users");
				
			if ($stmt->execute()) $response["users"] = $stmt->fetchAll(PDO::FETCH_ASSOC);
			else $response["ERROR"] = $stmt->errorInfo();	
				
			return $response;
		}
		catch(PDOException $e)
		{
			$response["ERROR"] = $e->getMessage();
			return $response;
		}	
	}	
	
	public function fetchAllTriviaPHPUsers()
	{
		try
		{
			$stmt = $this->db->prepare("SELECT * FROM TriviaPHP");
				
			if ($stmt->execute())
			{
				$response["success"] = true;
				$response["users"] = $stmt->fetchAll(PDO::FETCH_ASSOC);
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
	
	public function fetchAllUsernames()
	{
		try
		{
			$stmt = $this->db->prepare("SELECT id, fullname FROM users");
				
			if ($stmt->execute()) $response["users"] = $stmt->fetchAll(PDO::FETCH_ASSOC);
			else $response["ERROR"] = $stmt->errorInfo();	
				
			return $response;
		}
		catch(PDOException $e)
		{
			$response["ERROR"] = $e->getMessage();
			return $response;
		}	
	}	
	
	public function fetchAllUsernamesByOrder($orderBy = "ORDER BY users.fullname ASC")
	{
		try
		{
			$stmt = $this->db->prepare("SELECT id, fullname FROM users " . $orderBy);
				
			if ($stmt->execute()) $response["users"] = $stmt->fetchAll(PDO::FETCH_ASSOC);
			else $response["ERROR"] = $stmt->errorInfo();	
				
			return $response;
		}
		catch(PDOException $e)
		{
			$response["ERROR"] = $e->getMessage();
			return $response;
		}	
	}
	
	public function fetchUserIdByEmail($email)
	{
		try
		{
			$stmt = $this->db->prepare("SELECT id
				FROM users 
				WHERE email = :email");
				
			$stmt->bindValue(':email', $email, PDO::PARAM_STR);
	
			if ($stmt->execute())  
			{
				$userData = $stmt->fetchAll(PDO::FETCH_ASSOC);
				
				if (count($userData) >= 1) return $userData[0]["id"];
				else return false;
				
			} else return false;
			
		}
		catch(PDOException $e)
		{
			$response["ERROR"] = $e->getMessage();
			return $response;
		}	
	}	
	
	public function fetchUserInfoByUserId($userId)
	{
		try
		{
			$stmt = $this->db->prepare("SELECT *
				FROM users 
				WHERE id = :userId");
				
			$stmt->bindValue(':userId', $userId, PDO::PARAM_INT);
	
			if ($stmt->execute())  
			{
				$userData = $stmt->fetchAll(PDO::FETCH_ASSOC);
				
				if (count($userData) >= 1) return $userData[0];
				else return false;
				
			} else return false;
			
		}
		catch(PDOException $e)
		{
			$response["ERROR"] = $e->getMessage();
			return $response;
		}	
	}	
	
	public function fetchUserInfoByEmail($email)
	{
		try
		{
			$stmt = $this->db->prepare("SELECT *
				FROM users 
				WHERE email = :email");
				
			$stmt->bindValue(':email', $email, PDO::PARAM_STR);
	
			if ($stmt->execute())  
			{
				$userData = $stmt->fetchAll(PDO::FETCH_ASSOC);
				
				if (count($userData) >= 1) return $userData[0];
				else return false;
				
			} else return false;
			
		}
		catch(PDOException $e)
		{
			$response["ERROR"] = $e->getMessage();
			return $response;
		}	
	}
	
	public function fetchUserInfoByRegion($region)
	{
		try
		{
			$stmt = $this->db->prepare("SELECT *
				FROM users 
				WHERE region = :region");
				
			$stmt->bindValue(':region', $region, PDO::PARAM_STR);
	
			if ($stmt->execute())  
			{
				$userData = $stmt->fetchAll(PDO::FETCH_ASSOC);
				
				if (count($userData) >= 1) return $userData;
				else return false;
				
			} else return false;
			
		}
		catch(PDOException $e)
		{
			$response["ERROR"] = $e->getMessage();
			return $response;
		}	
	}
	
	public function fetchUsersByDate($month, $year)
	{
		try
		{
			$stmt = $this->db->prepare("SELECT * FROM users WHERE MONTH(created) = :month AND YEAR(created) = :year ORDER BY users.created ASC");
				
			$stmt->bindValue(':month', $month, PDO::PARAM_INT);	
			$stmt->bindValue(':year', $year, PDO::PARAM_INT);	
				
			if ($stmt->execute()) $response["users"] = $stmt->fetchAll(PDO::FETCH_ASSOC);
			else $response["ERROR"] = $stmt->errorInfo();	
				
			return $response;
		}
		catch(PDOException $e)
		{
			$response["ERROR"] = $e->getMessage();
			return $response;
		}	
	}
	
	public function getTheOldestDateRegistered()
	{
		try
		{
			$stmt = $this->db->prepare("SELECT MIN(created) AS lastDate 
			FROM users 
			ORDER BY users.created");
				
			if ($stmt->execute()) 
			{
				$fetch = $stmt->fetchAll(PDO::FETCH_ASSOC);
				if (count($fetch) >= 1) 
				{
					$response = $fetch[0]["lastDate"];
				}
			}
			else $response["ERROR"] = $stmt->errorInfo();	
				
			return $response;
		}
		catch(PDOException $e)
		{
			$response["ERROR"] = $e->getMessage();
			return $response;
		}	
	}
	
	public function fetchUsersByYear($year)
	{
		try
		{
			$stmt = $this->db->prepare("SELECT * FROM users WHERE YEAR(created) = :year ORDER BY users.created ASC");
				
			$stmt->bindValue(':year', $year, PDO::PARAM_INT);	
				
			if ($stmt->execute()) $response["users"] = $stmt->fetchAll(PDO::FETCH_ASSOC);
			else $response["ERROR"] = $stmt->errorInfo();	
				
			return $response;
		}
		catch(PDOException $e)
		{
			$response["ERROR"] = $e->getMessage();
			return $response;
		}	
	}
	
	public function searchUsers($uid, $searchTerm, $excludeUserIds, $skip = 0, $max = 5)
	{
		try
		{
			$searchTerm = "%" . $searchTerm . "%";	
			
			if (!is_null($excludeUserIds))
			{
				$fetchUsers = $this->db->prepare("SELECT 
					users.id,
					users.username, 
					users.useremail
				FROM users 
				WHERE users.username LIKE :searchTerm 
				AND users.id NOT IN (".$excludeUserIds.")  
				ORDER BY users.username ASC LIMIT :skip, :max");
			}
			else
			{
				$fetchUsers = $this->db->prepare("SELECT 
					users.id,
					users.username, 
					users.useremail,
					user_avatar.thumb
				FROM users 
				INNER JOIN user_avatar 
				ON users.id = user_avatar.userid 
				WHERE users.username LIKE :searchTerm 
				ORDER BY users.username ASC LIMIT :skip, :max");
			}
				
			$fetchUsers->bindValue(':searchTerm', $searchTerm, PDO::PARAM_STR);
	
			if(isset($skip)) $fetchUsers->bindValue(':skip', trim($skip), PDO::PARAM_INT);    
			else $fetchUsers->bindValue(':skip', 0, PDO::PARAM_INT);  
	
			$fetchUsers->bindValue(':max', $max, PDO::PARAM_INT);
					
			if ($fetchUsers->execute())
			{
				$response["result"] = true;
				$response["usersFound"] = $fetchUsers->fetchAll(PDO::FETCH_ASSOC);
			}
			else 
			{
				$response["result"] = false;
				$response["ERROR"] = $fetchUsers->errorInfo();
			}
			
			return $response;
		}
		catch(PDOException $e)
		{
			$response["ERROR"] = $e->getMessage();
			return $response;
		}			
		
	}
	
	public function isEmailValid($email)
	{
		return filter_var($email, FILTER_VALIDATE_EMAIL);
	}
}
?>