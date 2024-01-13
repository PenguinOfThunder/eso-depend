# Publish the command-line program as a self-contained single executable
dotnet publish --self-contained --output publish -c Release -r win-x64 `
    -p:PublishSingleFile=true `
    -p:PublishReadyToRunShowWarnings=true `
    EsoAdv.Cmd
