# Define the URL of the GIF and the destination file path
$gifUrl = "https://cataas.com/cat/gif?type=square&fontSize=20&width=400"
$destinationPath = "D:\vhosts\ocpa.ro\httpdocs\content\wiki\daily-cat\daily-cat.gif"

# Use Invoke-WebRequest to download the file
Invoke-WebRequest -Uri $gifUrl -OutFile $destinationPath

# Confirm the download
Write-Host "GIF downloaded successfully to $destinationPath"
