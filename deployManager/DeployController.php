
<?php

class DeployController {

    public function __construct() {}

    public function getAllDeployables(): void {
        try {
            http_response_code(200);
            header('Content-Type: application/json');
            echo json_encode([
                "status" => "success",
                "data" => $this->_getAllDeployables()
            ]);
        } catch (\Throwable $e) {
            http_response_code(500);
            echo json_encode([
                "status" => "error",
                "message" => "server error occurred"
            ]);
        }
    }

    public function deploy($deployable): void {
        try {
            $deployables = $this->_getAllDeployables();
            $found = in_array($deployable, $deployables);

            if ($found === false) {
                http_response_code(404);
                header('Content-Type: application/json');
                echo json_encode([
                    "status" => "error",
                    "message" => "Deployable '$deployable' not found"
                ]);
                return;
            }

            $deployStatus = $this->_performDeployment($deployable);

            if (strlen($deployStatus) === 0) {
                http_response_code(200);
                header('Content-Type: application/json');
                echo json_encode([
                    "status" => "success",
                    "message" => "Successfully deployed '$deployable'"
                ]);
                return;
            }

            http_response_code(400);
            header('Content-Type: application/json');
            echo json_encode([
                "status" => "error",
                "message" => "Failed to deploy '$deployable', reason: $deployStatus"
            ]);

        } catch (\Throwable $e) {
            http_response_code(500);
            echo json_encode([
                "status" => "error",
                "message" => "server error occurred"
            ]);
        }
    }

    private function _getAllDeployables(): array {
		$subfolders = [];
		$path = '../../deploy';
		foreach (scandir($path) as $item) {
			if ($item === '.' || $item === '..') 
				continue; // skip current and parent
			
			if (is_dir($path . DIRECTORY_SEPARATOR . $item))
				$subfolders[] = $item;
		}
		return $subfolders;
    }

    private function _performDeployment($deployable): string {
        try {
        } catch (Exception $e) {
			return $e;
        }
    }
}

?>
