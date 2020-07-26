$SDL2Url = "https://www.libsdl.org/release/SDL2-2.0.12-win32-x64.zip"
$SDL2Output = ".\SDL2-2.0.12-win32-x64.zip"

$SDL2ImageUrl = "https://www.libsdl.org/projects/SDL_image/release/SDL2_image-2.0.5-win32-x64.zip"
$SDL2ImageOutput = ".\SDL2_image-2.0.5-win32-x64.zip"

# Add custom dependencies
Invoke-WebRequest -Uri $SDL2Url -OutFile $SDL2Output
Invoke-WebRequest -Uri $SDL2ImageUrl -OutFile $SDL2ImageOutput
Expand-Archive -LiteralPath ".\SDL2-2.0.12-win32-x64.zip" -DestinationPath ".\SDL2\"
Expand-Archive -LiteralPath ".\SDL2_image-2.0.5-win32-x64.zip" -DestinationPath ".\SDL2_image\"
New-Item -Path ".\wastedrudgers\WasteDrudgers\lib" -Type Directory
Copy-Item ".\SDL2\SDL2.dll" -Destination ".\wastedrudgers\WasteDrudgers\lib"
Copy-Item ".\SDL2_image\SDL2_image.dll" -Destination ".\wastedrudgers\WasteDrudgers\lib"
Copy-Item ".\SDL2_image\libpng16-16.dll" -Destination ".\wastedrudgers\WasteDrudgers\lib"
Copy-Item ".\SDL2_image\zlib1.dll" -Destination ".\wastedrudgers\WasteDrudgers\lib"

# Publish project and create a release archive
dotnet publish .\wastedrudgers\WasteDrudgers\ -c Release
Compress-Archive -Path ".\wastedrudgers\WasteDrudgers\bin\Release\netcoreapp3.1\win-x64\publish\*" -DestinationPath ".\WasteDrudgers.zip"
