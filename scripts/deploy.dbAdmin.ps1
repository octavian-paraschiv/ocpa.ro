# Define the URL of the GIF and the destination file path
$gifUrl = "https://github.com/vrana/adminer/releases/download/v5.3.0/adminer-5.3.0-en.php"
$destinationPath = "D:\vhosts\ocpa.ro\httpdocs\dbAdmin\index.php"

# ensure dest folder exists
$destFolder = Split-Path -Path $destinationPath -Parent

if (-Not (Test-Path -Path $destFolder)) {
    New-Item -ItemType Directory -Path $destFolder
}

# Use Invoke-WebRequest to download the file
Invoke-WebRequest -Uri $gifUrl -OutFile $destinationPath

# Confirm the download
Write-Host "DBAdmin downloaded successfully to $destinationPath"
