dotnet build BLang/BLang.csproj &&
dotnet-coverage collect "dotnet BLang/bin/Debug/net10.0/bc.dll test" -f cobertura -o "misc/coverage.cobertura.xml" &&
reportgenerator "-reports:misc/coverage.cobertura.xml" "-targetdir:coveragereport" "-reporttypes:Html" &&
google-chrome coveragereport/index.html
