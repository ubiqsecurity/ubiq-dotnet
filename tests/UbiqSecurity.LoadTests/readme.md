dotnet run -- -c c:\Users\Danny\.ubiq\credentials -P dev -i ../UbiqSecurity.Tests/TestData/dev/100/dev-100.json

dotnet run -- -c c:\Users\Danny\.ubiq\credentials -P default -i ../UbiqSecurity.Tests/TestData/prod/100/prod-100.json

dotnet run -c Release -- -c c:\Users\Danny\.ubiq\credentials -P default -i ../UbiqSecurity.Tests/TestData/prod/100/prod-100.json