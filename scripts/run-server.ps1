#!/usr/bin/env pwsh

Start-Process "pwsh" -ArgumentList "-c dotnet run --project ./src/ChatTcp.Server/ChatTcp.Server.csproj"
