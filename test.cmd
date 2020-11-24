@echo off
cat generate-data.sh | bash
dotnet test -c Release
