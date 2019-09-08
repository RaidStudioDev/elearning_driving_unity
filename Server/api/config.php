<?php
header("Access-Control-Allow-Credentials: true");
header('Access-Control-Allow-Origin: *');
header('Access-Control-Allow-Methods: POST, GET, OPTIONS');
header('Access-Control-Allow-Headers: Accept, X-Access-Token, X-Application-Name, X-Request-Sent-Time');
header("Cache-Control: no-cache, no-store, must-revalidate");

date_default_timezone_set('US/Eastern');

error_reporting(E_ALL);
ini_set('display_errors', '1');

$ROOT_PATH = $_SERVER['DOCUMENT_ROOT'];
$SERVER_NAME = $_SERVER['SERVER_NAME'];

$CIRCUIT_MODES = array('winter', 'passenger', 'trucks');
$MAX_TRACKS = 5;

$logger = (isset($_GET['log']) && $_GET['log'] == 1);
if ($logger) trace("Logger Enabled");

$isDebug = false;

if ($_SERVER['SERVER_NAME'] == "RAIDURL")
{
	$APP_PATH = "/clients/demo/br/sb/api";
	$ROOT_PATH .= $APP_PATH;

	$DB_host = "raidp001";
	$DB_user = "";
	$DB_pass = "";
	$DB_name = "raidp001_sb";
}
else
{
	$APP_PATH = "/Switchback/api";
	$ROOT_PATH .= $APP_PATH;

	$DB_host = "localhost";
	$DB_user = "";
	$DB_pass = "";
	$DB_name = "br_games_staging";
}

include_once $ROOT_PATH . '/class.users.php';

try
{
	$DB_con = new PDO("mysql:host={$DB_host};dbname={$DB_name}",$DB_user,$DB_pass);
	$DB_con->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);

	// Create User Object
	$users = new USERS($DB_con);
}
catch(PDOException $e)
{
	$serverResponse["success"] = false;
	$serverResponse["error"] = $e->getMessage();
	$response["items"] = array($serverResponse);
	echo json_encode($response);
}


// ==============================================================================

// FUNCTIONS
function trace($msg, $beautify = false)
{
	$logger = $GLOBALS["logger"];

	if (!$logger) return;

	if (!$beautify) echo "[SB] " . $msg . "<br>";
	else
	{
		highlight_string( var_export($msg, true) );
	}
}

function onSuccessMessage($msg, $returnData, $beautify = false)
{
	$logger = $GLOBALS["logger"];

	$returnData["success"] = true;
	$returnData["status"] = $msg;
	$serverResponse["data"] = array($returnData);

	if ($beautify) highlight_string( var_export($serverResponse, true));
	else echo json_encode($serverResponse);

	$DB_con = null;
}

function onErrorMessage($msg, $beautify = false)
{
	$logger = $GLOBALS["logger"];

	$response["success"] = false;
	$response["status"] = $msg;
	$serverResponse["data"] = array($response);

	if ($beautify) highlight_string( var_export($serverResponse, true));
	else echo json_encode($serverResponse);

	$DB_con = null;
}


?>
