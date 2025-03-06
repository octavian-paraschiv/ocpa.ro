# Define variables
$hostPath = "D:\vhosts\ocpa.ro"
$deployPath = "$hostPath\deploy"
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

# Function to stop the website
function Stop-Website {
    try {
        Stop-WebSite -Name "YourWebsiteName"
    } catch {
        Write-Error "Failed to stop the website."
        exit 1
    }
}

# Function to extract the APP ZIP archive
function Extract-AppZip {
    try {
        Expand-Archive -Path $appZipFile -DestinationPath $extractPath -Force
    } catch {
        Write-Error "Failed to extract the APP ZIP archive."
        exit 1
    }
}

# Function to extract the API ZIP archive
function Extract-ApiZip {
    try {
        Expand-Archive -Path $apiZipFile -DestinationPath $extractPath -Force
    } catch {
        Write-Error "Failed to extract the API ZIP archive."
        exit 1
    }
}

# Function to copy extracted content to the website folder
function Copy-Content {
    try {
        Copy-Item -Path "$extractPath\*" -Destination $websitePath -Recurse -Force
    } catch {
        Write-Error "Failed to copy content to the website folder."
        exit 1
    }
}

# Function to start the website
function Start-Website {
    try {
        Start-WebSite -Name "YourWebsiteName"
    } catch {
        Write-Error "Failed to start the website."
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
        Write-Error "Failed to clean up."
        exit 1
    }
}

# Function to delete the website folder contents
function Clear-WebsiteFolder {
    try {
        Remove-Item -Path "$websitePath\*" -Recurse -Force
    } catch {
        Write-Error "Failed to clear the website folder."
        exit 1
    }
}

# Main script execution
Check-ApiZipExists
Check-AppZipExists
Stop-Website
Clear-WebsiteFolder
Extract-ApiZip
Extract-AppZip
Copy-Content
Start-Website
Clean-Up

Write-Output "Deployment completed successfully."