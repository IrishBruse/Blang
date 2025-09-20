dotnet build
rm -f ./profile.chromium.json
dotnet trace collect -o profile.json --format Chromium -- BLang/bin/Debug/net8.0/bc $@
rm -f ./profile.json
perfetto profile.chromium.json
