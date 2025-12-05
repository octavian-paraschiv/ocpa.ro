<?php

require_once __DIR__ . '/DeployController.php';

$requestMethod = $_SERVER['REQUEST_METHOD'];
$deployableId = $_SERVER['QUERY_STRING'];
$deployController = new DeployController();

switch ($requestMethod) {
    case "GET":
        $deployController->getAllDeployables();
        break;

    case "POST":
        $deployController->deploy($deployableId);
        break;

    default:
        sendResponse405();
        break;
}
	
function sendResponse405()
{
    http_response_code(405);
    header('Content-Type: application/json');
    echo json_encode(["message" => "Method Not Allowed"]);
}

?>