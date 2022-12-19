(robocopy /MIR Data %appdata%/SpaceEngineers/Mods/Sector9/Data) ^& IF %ERRORLEVEL% LSS 8 SET ERRORLEVEL = 0
(robocopy /MIR Models %appdata%/SpaceEngineers/Mods/Sector9/Models) ^& IF %ERRORLEVEL% LSS 8 SET ERRORLEVEL = 0
(robocopy /MIR Textures %appdata%/SpaceEngineers/Mods/Sector9/Textures) ^& IF %ERRORLEVEL% LSS 8 SET ERRORLEVEL = 0