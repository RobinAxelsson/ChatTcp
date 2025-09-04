#!/usr/bin/env pwsh

Start-Process "pwsh" -ArgumentList "-c dotnet run --project ./src/ChatTcp.Cli/ChatTcp.Cli.csproj --no-build --no-restore -- 8889"
