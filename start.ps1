& "E:\Steam\steamapps\common\SpaceEngineers\Bin64\SpaceEngineers.exe"
cls
Write-Output "Waiting for game to start..."
Start-Sleep -Seconds 20
Write-Output "*** LOG TAIL ***"
Get-childItem -Path "${env:APPDATA}\SpaceEngineers\SpaceEngineers*.log" | ForEach-Object {
    Get-Content -Path $_.FullName -Wait | Where-Object {$_.contains("S9")}
}