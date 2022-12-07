Get-childItem -Path "${env:APPDATA}\SpaceEngineers\SpaceEngineers*.log" | ForEach-Object {
    Get-Content -Path $_.FullName | Where-Object {$_.contains("S9")}
}