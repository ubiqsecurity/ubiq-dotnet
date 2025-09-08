set /p UbiqDotnetMssqlVersion=dotnet-ubiq-mssql version number to publish 

del .\bin\ubiq-mssql.zip
del .\bin\Release\credentials
del .\bin\Release\config.json

dotnet build -c Release

copy credentials-sample .\bin\Release\credentials /Y 
copy config-sample.json .\bin\Release\config.json /Y

"C:\Program Files\7-Zip\7z.exe" a -tzip .\bin\ubiq-mssql.zip .\bin\Release\*

curl --location --header "PRIVATE-TOKEN: glpat-TsUsYFlxD0BuOmuR2Chegm86MQp1OjhpcWwzCw.01.120ry2lbv" --upload-file ./bin/ubiq-mssql.zip https://gitlab.com/api/v4/projects/21301026/packages/generic/Ubiq-Security.MSSQL/%UbiqDotnetMssqlVersion%/ubiq-mssql.zip