& "E:\Steam\steamapps\common\SpaceEngineers\Bin64\SpaceEngineers.exe"
Start-Sleep -Seconds 30
Get-childItem -Path "${env:APPDATA}\SpaceEngineers\SpaceEngineers*.log" | ForEach-Object {
    Get-Content -Path $_.FullName -Wait | Where-Object {$_.contains("S9")}
}