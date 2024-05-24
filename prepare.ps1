dotnet --list-sdks
winget install Microsoft.DotNet.SDK.8
Remove-Item -Path .jellyfin -Recurse -Force
git clone --branch release-10.9.z https://github.com/jellyfin/jellyfin .jellyfin
cd .jellyfin
dotnet build Jellyfin.Server/Jellyfin.Server.csproj
cd ..
Remove-Item -Path .jellyfin-web -Recurse -Force
git clone --branch release-10.9.z https://github.com/jellyfin/jellyfin-web .jellyfin-web
cd .jellyfin-web
npm install
npm run build:development
cd ..
