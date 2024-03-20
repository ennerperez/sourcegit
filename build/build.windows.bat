
rmdir /S /Q SourceGit
dotnet publish ..\src\Updater\Updater.csproj -c Release -r win-x64 -o SourceGit -p:PublishAot=true -p:PublishTrimmed=true -p:TrimMode=link --self-contained
dotnet publish ..\src\SourceGit\SourceGit.csproj -c Release -r win-x64 -o SourceGit -p:PublishAot=true -p:PublishTrimmed=true -p:TrimMode=link --self-contained
resources\7z.exe a SourceGit.win-x64.zip SourceGit "-xr!en/" "-xr!zh/" "-xr!*.pdb"
rmdir /S /Q SourceGit