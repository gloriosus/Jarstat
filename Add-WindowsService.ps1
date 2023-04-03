$FolderPath = "C:\PATH_TO_APP_FOLDER"

New-Service -Name "Jarstat.Web" -BinaryPathName "$($FolderPath)\Jarstat.Web.exe --contentRoot $($FolderPath)" -Description "Веб-приложение ЦХСД" -DisplayName "Jarstat.Web" -StartupType Automatic