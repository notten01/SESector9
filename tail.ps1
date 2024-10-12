Write-Output "*** LOG TAIL ***"
Get-childItem -Path "${env:APPDATA}\SpaceEngineers\SpaceEngineers*.log" | ForEach-Object {
    Get-Content -Path $_.FullName -Wait | Where-Object {$_.contains("S9")}
}