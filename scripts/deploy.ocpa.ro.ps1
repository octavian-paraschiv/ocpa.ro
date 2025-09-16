# Exec params:
# Command: powershell.exe
# with arguments: -ExecutionPolicy Bypass -File D:\vhosts\ocpa.ro\Scripts\deploy.ocpa.ro.ps1 > D:\vhosts\ocpa.ro\logs\deploy.log 2>&1

# Define variables
$hostPath = "D:\vhosts\ocpa.ro"
$deployPath = "$hostPath\deploy\app"
$appZipFile = "$deployPath\app.zip"
$apiZipFile = "$deployPath\api.zip"
$websitePath = "$hostPath\httpdocs\app"
$extractPath = "$deployPath\extracted"

# Function to check if the APP zip archive exists
function Check-AppZipExists {
    if (Test-Path $appZipFile) {
        return $true
    } else {
        Write-Output "APP ZIP archive not found."
        exit 1
    }
}

# Function to check if the API zip archive exists
function Check-ApiZipExists {
    if (Test-Path $apiZipFile) {
        return $true
    } else {
        Write-Output "API ZIP archive not found."
        exit 1
    }
}

# Function to extract the APP ZIP archive
function Extract-AppZip {
    try {
        Expand-Archive -Path $appZipFile -DestinationPath $extractPath -Force
    } catch {
        Write-Output "Failed to extract the APP ZIP archive."
        exit 1
    }
}

# Function to extract the API ZIP archive
function Extract-ApiZip {
    try {
        Expand-Archive -Path $apiZipFile -DestinationPath $extractPath -Force
    } catch {
        Write-Output "Failed to extract the API ZIP archive."
        exit 1
    }
}

# Function to copy extracted content to the website folder
function Copy-Content {
    try {
        Copy-Item -Path "$extractPath\*" -Destination $websitePath -Recurse -Force
    } catch {
        Write-Output "Failed to copy content to the website folder."
        exit 1
    }
}

# Function to clean up
function Clean-Up {
    try {
        Remove-Item -Path $apiZipFile -Force
        Remove-Item -Path $appZipFile -Force
        Remove-Item -Path $extractPath -Recurse -Force
    } catch {
        Write-Output "Failed to clean up."
        exit 1
    }
}

# Function to delete the website folder contents
function Clear-WebsiteFolder {
    try {
        # This will cause the web site to stop so no need to do it explicitely
        Remove-Item -Path "$websitePath\web.config" -Force
        # Just wait for a little to be sure it stopped
        Start-Sleep -Seconds 10
        # The proceed with initial cleanup
        Remove-Item -Path "$websitePath\*" -Recurse -Force
    } catch {
        Write-Output "Failed to clear the website folder."
        exit 1
    }
}

# Main script execution
Check-ApiZipExists
Check-AppZipExists
Clear-WebsiteFolder
Extract-ApiZip
Extract-AppZip
Copy-Content
Clean-Up

Write-Output "Deployment completed successfully."