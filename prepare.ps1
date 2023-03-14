dotnet --list-sdks
winget install Microsoft.DotNet.SDK.6
git clone --branch release-10.8.z https://github.com/jellyfin/jellyfin .jellyfin
cd .jellyfin
dotnet build Jellyfin.Server/Jellyfin.Server.csproj
cd ..
git clone --branch release-10.8.z https://github.com/jellyfin/jellyfin-web .jellyfin-web
cd .jellyfin-web
npm install
npm run build:development
cd ..
