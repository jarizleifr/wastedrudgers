@echo off
cat generate-data.sh | bash
pushd WasteDrudgers
dotnet run
popd