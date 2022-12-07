Get-childItem -Path "${env:APPDATA}\SpaceEngineers\SpaceEngineers*.log" | ForEach-Object {
    ii $_.FullName
}